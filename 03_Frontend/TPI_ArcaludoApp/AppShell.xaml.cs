using TPI_ArcaludoApp.Pages;
using TPI_ArcaludoApp.Services;

namespace TPI_ArcaludoApp
{
    public partial class AppShell : Shell
    {
        private readonly ApiService _apiService = new ApiService();

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
            Routing.RegisterRoute("GameDetailPage", typeof(GameDetailPage));
        }

        protected override async void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            if (args.Previous == null)
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token) && await _apiService.GetProfileAsync(token) != null)
                {
                    await GoToAsync("//Main");
                }
            }
        }
    }
}
