using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DrawingApp.UI.Pages;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<MainViewModel>();
        DataContext = ViewModel;

        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}