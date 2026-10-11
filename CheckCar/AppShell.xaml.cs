using CheckCar.Views;

namespace CheckCar
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Rutas navegables (no en el TabBar).
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("dynamicform", typeof(DynamicFormPage));
        }
    }
}
