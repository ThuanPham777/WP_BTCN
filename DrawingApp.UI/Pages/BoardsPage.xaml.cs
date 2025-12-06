using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DrawingApp.UI.Pages;

public sealed partial class BoardsPage : Page
{
    public BoardsViewModel ViewModel { get; }

    public BoardsPage()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<BoardsViewModel>();
        DataContext = ViewModel;

        Loaded += BoardsPage_Loaded;
    }
    private async void BoardsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}
