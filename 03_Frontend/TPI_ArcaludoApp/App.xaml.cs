using TPI_ArcaludoApp.Pages;
namespace TPI_ArcaludoApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //MainPage = new LoginPage();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}