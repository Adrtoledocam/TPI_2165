using TPI_ArcaludoApp.Pages;

namespace TPI_ArcaludoApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
            Routing.RegisterRoute("GameDetailPage", typeof(GameDetailPage));
        }
        protected override async void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            // Se ejecuta solo la primera vez que el Shell navega
            if (args.Previous == null && Preferences.Get("user_id", 0) > 0)
            {
                await GoToAsync("//Main");
            }
        }
    }
}
