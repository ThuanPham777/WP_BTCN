using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DrawingApp.UI.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<DashboardViewModel>();
        DataContext = ViewModel;

        Loaded += DashboardPage_Loaded;
    }

    private async void DashboardPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}
