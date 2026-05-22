using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace TPI_ArcaludoApp;

public class CustomShellBottomNavHandler : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        => new CustomBottomNavTracker();
}

public class CustomBottomNavTracker : IShellBottomNavViewAppearanceTracker
{
    public void SetAppearance(Google.Android.Material.BottomNavigation.BottomNavigationView bottomView, IShellAppearanceElement appearance)
        => ApplyStyle(bottomView);

    public void ResetAppearance(Google.Android.Material.BottomNavigation.BottomNavigationView bottomView)
        => ApplyStyle(bottomView);

    private static void ApplyStyle(Google.Android.Material.BottomNavigation.BottomNavigationView bottomView)
    {
        bottomView.SetBackgroundResource(Resource.Drawable.tab_bar_background);

        var states = new int[][]
        {
            new[] { Android.Resource.Attribute.StateChecked },
            new int[] { }
        };
        var colors = new int[]
        {
            Android.Graphics.Color.White,
            Android.Graphics.Color.ParseColor("#888A9E")
        };
        var colorList = new Android.Content.Res.ColorStateList(states, colors);
        bottomView.ItemIconTintList = colorList;
        bottomView.ItemTextColor    = colorList;

        // Fuerza el fondo plano sin padding extra
        bottomView.Elevation = 0;
    }

    public void Dispose() { }
}
