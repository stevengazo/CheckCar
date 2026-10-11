using System.Text.Json;
using CheckCar.Models.Api;
using CheckCar.Models.Local;

namespace CheckCar.Services
{
    /// <summary>
    /// Motor de sincronización offline-first: baja definiciones/vehículos y
    /// sube los reportes pendientes cuando hay conexión.
    /// </summary>
    public class SyncService
    {
        private readonly ApiService _api;
        private readonly LocalDb _db;

        public SyncService(ApiService api, LocalDb db)
        {
            _api = api;
            _db = db;
        }

        public bool IsOnline => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        public Task<int> PendingCountAsync() => _db.PendingCountAsync();

        public record SyncResult(bool Online, int Pushed, int Failed, bool Pulled, string? Message);

        public async Task<SyncResult> SyncAsync()
        {
            if (!IsOnline)
                return new SyncResult(false, 0, 0, false, "Sin conexión. Los reportes quedan pendientes.");

            if (!await _api.EnsureAuthenticatedAsync())
                return new SyncResult(true, 0, 0, false, "No autenticado. Inicia sesión.");

            bool pulled = await PullAsync();
            var (pushed, failed) = await PushPendingAsync();

            var msg = $"{pushed} subido(s)" + (failed > 0 ? $", {failed} con error" : "") + (pulled ? ", catálogos actualizados" : "");
            return new SyncResult(true, pushed, failed, pulled, msg);
        }

        private async Task<bool> PullAsync()
        {
            try
            {
                foreach (var type in new[] { "Exit", "Entry" })
                {
                    var def = await _api.GetFormDefinitionAsync(type);
                    if (def != null)
                        await _db.UpsertFormDefinitionAsync(new LocalFormDefinition
                        {
                            Type = type,
                            Json = JsonSerializer.Serialize(def),
                            UpdatedAt = DateTime.UtcNow
                        });
                }

                var cars = await _api.GetCarsAsync();
                if (cars.Count > 0)
                    await _db.UpsertCarsAsync(cars.Select(c => new LocalCar
                    {
                        CarId = c.CarId, Brand = c.Brand, Model = c.Model, Plate = c.Plate
                    }));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<(int pushed, int failed)> PushPendingAsync()
        {
            int pushed = 0, failed = 0;
            foreach (var report in await _db.GetPendingReportsAsync())
            {
                var photos = await _db.GetPhotosForReportAsync(report.LocalId);
                try
                {
                    var ok = await _api.PostDynamicReportAsync(report, photos);
                    if (ok)
                    {
                        report.Status = "Synced";
                        report.Error = null;
                        pushed++;
                    }
                    else
                    {
                        report.Status = "Error";
                        report.Error = "La API rechazó el reporte.";
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    report.Status = "Error";
                    report.Error = ex.Message;
                    failed++;
                }
                await _db.UpdateReportAsync(report);
            }
            return (pushed, failed);
        }
    }
}
