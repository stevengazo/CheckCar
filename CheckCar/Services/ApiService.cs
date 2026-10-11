using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CheckCar.Models.Api;
using CheckCar.Models.Local;

namespace CheckCar.Services
{
    /// <summary>
    /// Cliente HTTP de la API CheckCars. Maneja autenticación JWT (SecureStorage)
    /// y las operaciones de sincronización.
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService()
        {
            _http = new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl), Timeout = TimeSpan.FromSeconds(30) };
        }

        public async Task<bool> HasTokenAsync() => !string.IsNullOrEmpty(await SecureStorage.GetAsync("token"));

        private async Task ApplyAuthAsync()
        {
            var token = await SecureStorage.GetAsync("token");
            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>Inicia sesión y guarda token + credenciales (para re-login en sync).</summary>
        public async Task<bool> LoginAsync(string email, string password)
        {
            var resp = await _http.PostAsJsonAsync("/api/Account/Login", new { email, password });
            if (!resp.IsSuccessStatusCode) return false;
            var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            if (string.IsNullOrEmpty(data?.Token)) return false;

            await SecureStorage.SetAsync("token", data.Token);
            await SecureStorage.SetAsync("email", email);
            await SecureStorage.SetAsync("password", password);
            await ApplyAuthAsync();
            return true;
        }

        /// <summary>Reintenta login con credenciales guardadas (sync en background).</summary>
        public async Task<bool> EnsureAuthenticatedAsync()
        {
            if (await HasTokenAsync()) { await ApplyAuthAsync(); return true; }
            var email = await SecureStorage.GetAsync("email");
            var password = await SecureStorage.GetAsync("password");
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return false;
            return await LoginAsync(email, password);
        }

        public void Logout()
        {
            SecureStorage.Remove("token");
            SecureStorage.Remove("email");
            SecureStorage.Remove("password");
        }

        public async Task<FormDefinitionDto?> GetFormDefinitionAsync(string type)
        {
            await ApplyAuthAsync();
            var resp = await _http.GetAsync($"/api/FormDefinitions?type={Uri.EscapeDataString(type)}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<FormDefinitionDto>();
        }

        public async Task<List<CarDto>> GetCarsAsync()
        {
            await ApplyAuthAsync();
            var resp = await _http.GetAsync("/api/Cars");
            if (!resp.IsSuccessStatusCode) return new();
            return await resp.Content.ReadFromJsonAsync<List<CarDto>>() ?? new();
        }

        /// <summary>Sube un reporte dinámico con sus fotos (multipart, archivos nombrados por slotKey).</summary>
        public async Task<bool> PostDynamicReportAsync(LocalReport report, List<LocalPhoto> photos)
        {
            await ApplyAuthAsync();
            using var content = new MultipartFormDataContent
            {
                { new StringContent(report.Type), "type" },
                { new StringContent(report.DataJson, Encoding.UTF8), "data" },
            };
            if (!string.IsNullOrEmpty(report.CarId))
                content.Add(new StringContent(report.CarId), "carId");

            var streams = new List<Stream>();
            foreach (var p in photos)
            {
                if (!File.Exists(p.FilePath)) continue;
                var stream = File.OpenRead(p.FilePath);
                streams.Add(stream);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, p.SlotKey, Path.GetFileName(p.FilePath));
            }

            try
            {
                var resp = await _http.PostAsync("/api/DynamicReports/form", content);
                return resp.IsSuccessStatusCode;
            }
            finally
            {
                foreach (var s in streams) s.Dispose();
            }
        }
    }
}
