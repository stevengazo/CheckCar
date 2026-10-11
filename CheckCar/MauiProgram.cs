using CheckCar.Services;
using CheckCar.ViewModels;
using CheckCar.Views;
using Microsoft.Extensions.Logging;

namespace CheckCar
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services (offline-first)
            builder.Services.AddSingleton<LocalDb>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<SyncService>();

            // View models
            builder.Services.AddTransient<DashboardViewModel>();

            // Pages
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<VehiclesPage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<MorePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DynamicFormPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
