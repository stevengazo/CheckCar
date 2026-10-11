using CheckCar.Models.Local;
using SQLite;

namespace CheckCar.Services
{
    /// <summary>
    /// Acceso a la base de datos local SQLite (offline-first).
    /// </summary>
    public class LocalDb
    {
        private SQLiteAsyncConnection? _db;

        private async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_db is not null) return _db;

            var path = Path.Combine(FileSystem.AppDataDirectory, "checkcar.db3");
            _db = new SQLiteAsyncConnection(path,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await _db.CreateTableAsync<LocalFormDefinition>();
            await _db.CreateTableAsync<LocalCar>();
            await _db.CreateTableAsync<LocalReport>();
            await _db.CreateTableAsync<LocalPhoto>();
            return _db;
        }

        public Task InitAsync() => GetConnectionAsync();

        // ---- Form definitions ----
        public async Task UpsertFormDefinitionAsync(LocalFormDefinition def)
            => await (await GetConnectionAsync()).InsertOrReplaceAsync(def);

        public async Task<LocalFormDefinition?> GetFormDefinitionAsync(string type)
            => await (await GetConnectionAsync()).Table<LocalFormDefinition>().Where(d => d.Type == type).FirstOrDefaultAsync();

        // ---- Cars ----
        public async Task UpsertCarsAsync(IEnumerable<LocalCar> cars)
        {
            var db = await GetConnectionAsync();
            await db.RunInTransactionAsync(tran =>
            {
                foreach (var c in cars) tran.InsertOrReplace(c);
            });
        }

        public async Task<List<LocalCar>> GetCarsAsync()
            => await (await GetConnectionAsync()).Table<LocalCar>().OrderBy(c => c.Brand).ToListAsync();

        // ---- Reports ----
        public async Task<int> InsertReportAsync(LocalReport report)
        {
            var db = await GetConnectionAsync();
            await db.InsertAsync(report);
            return report.LocalId;
        }

        public async Task InsertPhotoAsync(LocalPhoto photo)
            => await (await GetConnectionAsync()).InsertAsync(photo);

        public async Task UpdateReportAsync(LocalReport report)
            => await (await GetConnectionAsync()).UpdateAsync(report);

        public async Task<List<LocalReport>> GetReportsAsync()
            => await (await GetConnectionAsync()).Table<LocalReport>().OrderByDescending(r => r.CreatedUtc).ToListAsync();

        public async Task<List<LocalReport>> GetPendingReportsAsync()
            => await (await GetConnectionAsync()).Table<LocalReport>().Where(r => r.Status != "Synced").ToListAsync();

        public async Task<List<LocalPhoto>> GetPhotosForReportAsync(int localReportId)
            => await (await GetConnectionAsync()).Table<LocalPhoto>().Where(p => p.LocalReportId == localReportId).ToListAsync();

        public async Task<int> PendingCountAsync()
            => await (await GetConnectionAsync()).Table<LocalReport>().Where(r => r.Status != "Synced").CountAsync();
    }
}
