using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class TemplatesViewModel : ObservableObject
{
    private readonly ITemplateRepository _repo;

    [ObservableProperty] private List<ShapeTemplate> items = new();
    [ObservableProperty] private ShapeTemplate? selected;

    public TemplatesViewModel(ITemplateRepository repo) => _repo = repo;

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
