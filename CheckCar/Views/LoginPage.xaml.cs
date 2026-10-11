using CheckCar.Services;

namespace CheckCar.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _api;
        private readonly SyncService _sync;

        public LoginPage(ApiService api, SyncService sync)
        {
            InitializeComponent();
            _api = api;
            _sync = sync;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Ingresa correo y contraseña.");
                return;
            }

            SetBusy(true);
            try
            {
                var ok = await _api.LoginAsync(email, password);
                if (!ok)
                {
                    ShowError("Credenciales inválidas o correo no confirmado.");
                    return;
                }
                // Primer sync para cachear definiciones y vehículos.
                await _sync.SyncAsync();
                await Shell.Current.GoToAsync("//dashboard");
            }
            catch (Exception ex)
            {
                ShowError($"No se pudo conectar: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ShowError(string msg)
        {
            ErrorLabel.Text = msg;
            ErrorLabel.IsVisible = true;
        }

        private void SetBusy(bool busy)
        {
            Busy.IsVisible = busy;
            Busy.IsRunning = busy;
            LoginButton.IsEnabled = !busy;
        }
    }
}
