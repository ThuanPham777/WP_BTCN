using DrawingApp.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace DrawingApp.UI.Pages;

public sealed partial class ShellPage : Page
{
    private readonly INavigationService _nav;

    public ShellPage()
    {
        this.InitializeComponent();
        _nav = App.Host.Services.GetRequiredService<INavigationService>();
        _nav.Frame = RootFrame;

        RootFrame.Navigate(typeof(MainPage));
    }

    private void Nav_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender,
        Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
    {
        _nav.GoBack();
    }

    private void Nav_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender,
        Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            switch (item.Tag?.ToString())
            {
                case "main":
                    _nav.Navigate(typeof(MainPage));
                    break;
                case "profiles":
                    _nav.Navigate(typeof(ProfilePage));
                    break;
                case "draw":
                    _nav.Navigate(typeof(DrawingPage));
                    break;
                case "mgmt":
                    _nav.Navigate(typeof(ManagementShellPage));
                    break;
            }
        }
    }
}
