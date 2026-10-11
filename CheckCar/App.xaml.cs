using CheckCar.Services;

namespace CheckCar
{
    public partial class App : Application
    {
        private readonly LocalDb _db;
        private readonly ApiService _api;

        public App(LocalDb db, ApiService api)
        {
            InitializeComponent();
            _db = db;
            _api = api;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Inicializa la BD local y, si no hay sesión, muestra el login.
            window.Created += async (_, _) =>
            {
                await _db.InitAsync();
                if (!await _api.HasTokenAsync())
                    await Shell.Current.GoToAsync("login");
            };

            return window;
        }
    }
}
