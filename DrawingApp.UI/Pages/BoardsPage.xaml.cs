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
        InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<BoardsViewModel>();
        DataContext = ViewModel;

        Loaded += async (_, __) => await ViewModel.LoadAsync();
    }

    private void BoardsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not DrawingApp.Core.Entities.DrawingBoard board) return;

        var nav = App.Host.Services.GetRequiredService<DrawingApp.UI.Navigation.INavigationService>();
        nav.Navigate(typeof(DrawingPage), board.Id);
    }
}
