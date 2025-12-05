using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace DrawingApp.UI.Pages;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<MainViewModel>();
        DataContext = ViewModel;

        Loaded += async (_, __) => await ViewModel.LoadAsync();
    }
}