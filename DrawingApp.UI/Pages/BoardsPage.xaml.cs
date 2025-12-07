using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

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
}
