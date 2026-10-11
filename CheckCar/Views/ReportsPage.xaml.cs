using CheckCar.Services;

namespace CheckCar.Views
{
    public partial class ReportsPage : ContentPage
    {
        private readonly LocalDb _db;
        private readonly SyncService _sync;

        public ReportsPage(LocalDb db, SyncService sync)
        {
            InitializeComponent();
            _db = db;
            _sync = sync;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            ReportsList.ItemsSource = await _db.GetReportsAsync();
            var pending = await _sync.PendingCountAsync();
            PendingLabel.Text = pending > 0 ? $"{pending} pendiente(s) de sincronizar" : "Todo sincronizado";
        }

        private async void OnNewExit(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("dynamicform?type=Exit");

        private async void OnNewEntry(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("dynamicform?type=Entry");

        private async void OnSync(object sender, EventArgs e)
        {
            SyncButton.IsEnabled = false;
            try
            {
                var result = await _sync.SyncAsync();
                await DisplayAlert("Sincronización", result.Message ?? "Listo", "OK");
                await RefreshAsync();
            }
            finally
            {
                SyncButton.IsEnabled = true;
            }
        }

        private async void OnRefresh(object sender, EventArgs e)
        {
            await _sync.SyncAsync();
            await RefreshAsync();
            Refresh.IsRefreshing = false;
        }
    }
}
