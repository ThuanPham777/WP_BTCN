using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.UI.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class TemplatesViewModel : ObservableObject
{
    private readonly ITemplateRepository _repo;
    private readonly INavigationService _nav;

    [ObservableProperty] private List<ShapeTemplate> items = new();
    [ObservableProperty] private ShapeTemplate? selected;

    public TemplatesViewModel(ITemplateRepository repo, INavigationService nav)
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
    public async Task DeleteItemAsync(ShapeTemplate? template)
    {
        if (template == null) return;
        await _repo.DeleteAsync(template.Id);
        await LoadAsync();
    }

    // dùng template -> sang DrawingPage
    [RelayCommand]
    public void OpenTemplate(ShapeTemplate? template)
    {
        if (template == null) return;

        _nav.Navigate(
            typeof(Pages.DrawingPage),
            new DrawingNavigationArgs { TemplateId = template.Id }
        );
    }
}
