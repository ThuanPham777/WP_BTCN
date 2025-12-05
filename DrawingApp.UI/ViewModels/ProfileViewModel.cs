using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Enums;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Models;
using DrawingApp.UI.Services;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DrawingApp.UI.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileRepository _repo;
    private readonly IDialogService _dialog;

    [ObservableProperty] private List<Profile> items = new();
    [ObservableProperty] private Profile? selected;

    [ObservableProperty] private string name = "";
    [ObservableProperty] private AppTheme theme = AppTheme.System;
    [ObservableProperty] private double defaultBoardWidth = AppDefaults.DefaultBoardWidth;
    [ObservableProperty] private double defaultBoardHeight = AppDefaults.DefaultBoardHeight;
    [ObservableProperty] private string defaultStrokeColor = "#FF0F172A";
    [ObservableProperty] private double defaultStrokeThickness = 2;

    public ProfileViewModel(IProfileRepository repo, IDialogService dialog)
    {
        _repo = repo;
        _dialog = dialog;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Items = await _repo.GetAllAsync();
    }

    [RelayCommand]
    public void FillFromSelected()
    {
        if (Selected == null) return;

        Name = Selected.Name;
        Theme = Selected.Theme;
        DefaultBoardWidth = Selected.DefaultBoardWidth;
        DefaultBoardHeight = Selected.DefaultBoardHeight;
        DefaultStrokeColor = Selected.DefaultStrokeColor;
        DefaultStrokeThickness = Selected.DefaultStrokeThickness;
    }

    [RelayCommand]
    public async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await _dialog.ShowMessageAsync("Validation", "Profile name is required.");
            return;
        }

        var p = new Profile
        {
            Name = Name.Trim(),
            Theme = Theme,
            DefaultBoardWidth = DefaultBoardWidth,
            DefaultBoardHeight = DefaultBoardHeight,
            DefaultStrokeColor = DefaultStrokeColor,
            DefaultStrokeThickness = DefaultStrokeThickness
        };

        await _repo.AddAsync(p);
        await LoadAsync();
    }

    [RelayCommand]
    public async Task UpdateAsync()
    {
        if (Selected == null) return;

        Selected.Name = Name.Trim();
        Selected.Theme = Theme;
        Selected.DefaultBoardWidth = DefaultBoardWidth;
        Selected.DefaultBoardHeight = DefaultBoardHeight;
        Selected.DefaultStrokeColor = DefaultStrokeColor;
        Selected.DefaultStrokeThickness = DefaultStrokeThickness;

        await _repo.UpdateAsync(Selected);
        await LoadAsync();
    }

    [RelayCommand]
    public async Task DeleteAsync()
    {
        if (Selected == null) return;
        await _repo.DeleteAsync(Selected.Id);
        Selected = null;
        await LoadAsync();
    }
}
