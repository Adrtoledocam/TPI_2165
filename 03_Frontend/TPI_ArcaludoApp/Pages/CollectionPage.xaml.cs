using TPI_ArcaludoApp.ViewModels;

namespace TPI_ArcaludoApp.Pages;

public partial class CollectionPage : ContentPage
{
	public CollectionPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CollectionViewModel viewModel)
        {
            await viewModel.LoadCollectionAsync();
        }
    }
}