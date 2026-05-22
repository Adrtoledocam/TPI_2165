using TPI_ArcaludoApp.ViewModels;

namespace TPI_ArcaludoApp.Pages;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ProfileViewModel viewModel)
        {
            await viewModel.LoadProfileAsync();
        }
    }
}