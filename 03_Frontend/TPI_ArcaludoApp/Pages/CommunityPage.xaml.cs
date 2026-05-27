using TPI_ArcaludoApp.ViewModels;

namespace TPI_ArcaludoApp.Pages;

public partial class CommunityPage : ContentPage
{
	public CommunityPage()
	{
		InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CommunityViewModel viewModel)
        {
            await viewModel.LoadCommunityAsync();
        }
    }
}