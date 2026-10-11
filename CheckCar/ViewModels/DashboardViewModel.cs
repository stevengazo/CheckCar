using System.Collections.ObjectModel;
using CheckCar.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CheckCar.ViewModels
{
    /// <summary>
    /// View model for the home dashboard. For now it is seeded with sample
    /// data; the collections are ready to be fed from the CheckCars API.
    /// </summary>
    public partial class DashboardViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _greeting = "¡Hola!";

        [ObservableProperty]
        private string _userName = "Conductor";

        public ObservableCollection<StatCard> Stats { get; } = new();
        public ObservableCollection<QuickAction> QuickActions { get; } = new();
        public ObservableCollection<ActivityItem> RecentActivity { get; } = new();

        public DashboardViewModel()
        {
            Title = "Inicio";
            Greeting = BuildGreeting();
            SeedSampleData();
        }

        [RelayCommand]
        private async Task GoToAsync(string? route)
        {
            if (string.IsNullOrWhiteSpace(route))
                return;

            await Shell.Current.GoToAsync($"//{route}");
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                // Placeholder for a future API call. Simulates a short load.
                await Task.Delay(600);
                Greeting = BuildGreeting();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string BuildGreeting()
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                < 12 => "Buenos días",
                < 19 => "Buenas tardes",
                _ => "Buenas noches"
            };
        }

        private void SeedSampleData()
        {
            Stats.Clear();
            Stats.Add(new StatCard { Title = "Vehículos", Value = "12", Caption = "en flota", Glyph = "🚗", Accent = Color.FromArgb("#2563EB") });
            Stats.Add(new StatCard { Title = "Disponibles", Value = "8", Caption = "listos hoy", Glyph = "✅", Accent = Color.FromArgb("#16A34A") });
            Stats.Add(new StatCard { Title = "Reportes", Value = "34", Caption = "este mes", Glyph = "📋", Accent = Color.FromArgb("#0EA5E9") });
            Stats.Add(new StatCard { Title = "Pendientes", Value = "3", Caption = "por revisar", Glyph = "⚠️", Accent = Color.FromArgb("#F59E0B") });

            QuickActions.Clear();
            QuickActions.Add(new QuickAction { Title = "Entrada / Salida", Glyph = "🔑", Accent = Color.FromArgb("#2563EB"), Route = "reports" });
            QuickActions.Add(new QuickAction { Title = "Reportar choque", Glyph = "💥", Accent = Color.FromArgb("#DC2626"), Route = "reports" });
            QuickActions.Add(new QuickAction { Title = "Reportar avería", Glyph = "🛠️", Accent = Color.FromArgb("#F59E0B"), Route = "reports" });
            QuickActions.Add(new QuickAction { Title = "Ver vehículos", Glyph = "🚙", Accent = Color.FromArgb("#0EA5E9"), Route = "vehicles" });

            RecentActivity.Clear();
            RecentActivity.Add(new ActivityItem { Title = "Salida registrada", Subtitle = "Toyota Hilux · ABC-123", When = "Hace 2 h", Glyph = "🔑", Accent = Color.FromArgb("#2563EB") });
            RecentActivity.Add(new ActivityItem { Title = "Avería reportada", Subtitle = "Nissan Frontier · XYZ-789", When = "Ayer", Glyph = "🛠️", Accent = Color.FromArgb("#F59E0B") });
            RecentActivity.Add(new ActivityItem { Title = "Mantenimiento completado", Subtitle = "Kia Rio · JKL-456", When = "Hace 3 días", Glyph = "✅", Accent = Color.FromArgb("#16A34A") });
        }
    }
}
