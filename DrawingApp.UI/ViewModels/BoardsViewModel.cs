using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class BoardsViewModel : ObservableObject
{
    private readonly IBoardRepository _repo;

    [ObservableProperty] private List<DrawingBoard> items = new();
    [ObservableProperty] private DrawingBoard? selected;

    public BoardsViewModel(IBoardRepository repo) => _repo = repo;

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
}
