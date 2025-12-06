using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class BoardsViewModel : ObservableObject
{
    private readonly IBoardRepository _repo;
    private readonly INavigationService _nav;

    [ObservableProperty] private List<DrawingBoard> items = new();
    [ObservableProperty] private DrawingBoard? selected;

    public BoardsViewModel(IBoardRepository repo, INavigationService nav)
    {
        _repo = repo;
        _nav = nav;
    }


    [RelayCommand]
    public async Task LoadAsync()
    {
        Items = await _repo.GetAllAsync();
    }

    [RelayCommand]
    public async Task DeleteAsync()
    {
        if (Selected == null) return;
        await _repo.DeleteAsync(Selected.Id);
        await LoadAsync();
    }

    [RelayCommand]
    private void OpenBoard(DrawingBoard? board)
    {
        if (board == null) return;
        _nav.Navigate(typeof(Pages.DrawingPage), board.Id);
    }

}
