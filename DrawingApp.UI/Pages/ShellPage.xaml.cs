using DrawingApp.Core.Interfaces.Services;
using DrawingApp.UI.Navigation;
using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DrawingApp.UI.Pages;

public sealed partial class ShellPage : Page
{
    private readonly INavigationService _nav;
    private readonly IDialogService _dialog;

    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<ShellViewModel>();
        DataContext = ViewModel;

        _nav = App.Host.Services.GetRequiredService<INavigationService>();
        _nav.Frame = RootFrame;

        RootFrame.Navigated += RootFrame_Navigated;

        _dialog = App.Host.Services.GetRequiredService<IDialogService>();

        Loaded += ShellPage_Loaded;

        // default route
        RootFrame.Navigate(typeof(MainPage));
        Nav.SelectedItem = Nav.MenuItems[0];
    }

    private void RootFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        var pageType = e.SourcePageType;

        if (pageType == typeof(MainPage))
            SetSelectedByTag("main");
        else if (pageType == typeof(ProfilePage))
            SetSelectedByTag("profiles");
        else if (pageType == typeof(DrawingPage))
            SetSelectedByTag("draw");
        else if (pageType == typeof(ManagementShellPage)
              || pageType == typeof(BoardsPage)
              || pageType == typeof(TemplatesPage)
              || pageType == typeof(DashboardPage))
            SetSelectedByTag("mgmt");
    }

    private void SetSelectedByTag(string tag)
    {
        foreach (var obj in Nav.MenuItems)
        {
            if (obj is NavigationViewItem item && item.Tag?.ToString() == tag)
            {
                Nav.SelectedItem = item;
                return;
            }
        }
    }


    private void Nav_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        _nav.GoBack();
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Set XamlRoot sau khi UI đã có cây visual
        _dialog.SetXamlRoot(this.XamlRoot);
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is not NavigationViewItem item) return;

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
