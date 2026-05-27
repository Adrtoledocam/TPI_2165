using TPI_ArcaludoApp.ViewModels;

namespace TPI_ArcaludoApp.Pages;

public partial class LibraryPage : ContentPage
{
	public  LibraryPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is LibraryViewModel viewModel)
        {
            await viewModel.LoadGamesAsync();
        }
    }
}