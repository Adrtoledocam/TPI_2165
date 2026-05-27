using TPI_ArcaludoApp.ViewModels;

namespace TPI_ArcaludoApp.Pages;

public partial class WishlistPage : ContentPage
{
    public WishlistPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is WishlistViewModel viewModel)
        {
            await viewModel.LoadWishlistAsync();
        }
    }
}