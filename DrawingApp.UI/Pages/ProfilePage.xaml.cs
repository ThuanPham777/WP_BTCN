using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace DrawingApp.UI.Pages;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; }

    public ProfilePage()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<ProfileViewModel>();
        DataContext = ViewModel;

        Loaded += ProfilePage_Loaded;
    }

    private async void ProfilePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}
