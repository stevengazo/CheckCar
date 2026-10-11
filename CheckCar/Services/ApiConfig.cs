namespace CheckCar.Services
{
    /// <summary>
    /// Central configuration for reaching the CheckCars backend.
    /// The API runs in Docker (container port 5050 published on host 8085).
    /// </summary>
    public static class ApiConfig
    {
        /// <summary>
        /// Base URL of the CheckCars API.
        ///
        /// - Physical device on the same Wi-Fi as the dev machine: use the PC LAN IP.
        /// - Android emulator: use http://10.0.2.2:8085 (host loopback alias).
        /// - Windows / local run: http://localhost:8085
        /// </summary>
        public static string BaseUrl { get; set; } = "http://192.168.0.14:8085";
    }
}
