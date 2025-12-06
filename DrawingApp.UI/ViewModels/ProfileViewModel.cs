using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Enums;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Interfaces.Services;
using DrawingApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileRepository _repo;
    private readonly IDialogService _dialog;

    public IReadOnlyList<AppTheme> ThemeOptions { get; }
        = new[] { AppTheme.System, AppTheme.Light, AppTheme.Dark };

    [ObservableProperty] private ObservableCollection<Profile> items = new();
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

    partial void OnSelectedChanged(Profile? value)
    {
        if (value == null)
        {
            ResetForm();
            return;
        }

        Name = value.Name;
        Theme = value.Theme;
        DefaultBoardWidth = value.DefaultBoardWidth;
        DefaultBoardHeight = value.DefaultBoardHeight;
        DefaultStrokeColor = value.DefaultStrokeColor;
        DefaultStrokeThickness = value.DefaultStrokeThickness;
    }

    private void ResetForm()
    {
        Name = "";
        Theme = AppTheme.System;
        DefaultBoardWidth = AppDefaults.DefaultBoardWidth;
        DefaultBoardHeight = AppDefaults.DefaultBoardHeight;
        DefaultStrokeColor = "#FF0F172A";
        DefaultStrokeThickness = 2;
    }

    [RelayCommand]
    public void NewProfile()
    {
        Selected = null;
        ResetForm();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            Items.Clear();
            var list = await _repo.GetAllAsync();

            foreach (var p in list)
                Items.Add(p);

            Debug.WriteLine($"[ProfileVM] Loaded {Items.Count} profiles");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await _dialog.ShowMessageAsync("Load failed", ex.Message);
        }
    }

    [RelayCommand]
    public async Task CreateAsync()
    {
        Debug.WriteLine("[ProfileVM] CreateAsync clicked");

        try
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
                DefaultStrokeThickness = DefaultStrokeThickness,
                DefaultStrokeDash = StrokeDash.Solid
            };

            await _repo.AddAsync(p);
            await LoadAsync();

            Selected = Items.FirstOrDefault(x => x.Id == p.Id)
                       ?? Items.FirstOrDefault(x => x.Name == p.Name);

            await _dialog.ShowMessageAsync("Create success", $"Profile '{p.Name}' created.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await _dialog.ShowMessageAsync("Create failed", ex.Message);
        }
    }

    [RelayCommand]
    public async Task UpdateAsync()
    {
        Debug.WriteLine("[ProfileVM] UpdateAsync clicked");

        try
        {
            if (Selected == null)
            {
                await _dialog.ShowMessageAsync("Update", "Please select a profile first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                await _dialog.ShowMessageAsync("Validation", "Profile name is required.");
                return;
            }

            var id = Selected.Id;

            Selected.Name = Name.Trim();
            Selected.Theme = Theme;
            Selected.DefaultBoardWidth = DefaultBoardWidth;
            Selected.DefaultBoardHeight = DefaultBoardHeight;
            Selected.DefaultStrokeColor = DefaultStrokeColor;
            Selected.DefaultStrokeThickness = DefaultStrokeThickness;

            await _repo.UpdateAsync(Selected);
            await LoadAsync();

            Selected = Items.FirstOrDefault(x => x.Id == id);

            await _dialog.ShowMessageAsync("Update success", "Profile updated.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await _dialog.ShowMessageAsync("Update failed", ex.Message);
        }
    }

    [RelayCommand]
    public async Task DeleteAsync()
    {
        Debug.WriteLine("[ProfileVM] DeleteAsync clicked");

        try
        {
            if (Selected == null)
            {
                await _dialog.ShowMessageAsync("Delete", "Please select a profile first.");
                return;
            }

            var ok = await _dialog.ShowConfirmAsync(
                "Confirm delete",
                $"Delete profile '{Selected.Name}'?");

            if (!ok) return;

            var id = Selected.Id;
            var name = Selected.Name;

            await _repo.DeleteAsync(id);

            Selected = null;
            await LoadAsync();

            await _dialog.ShowMessageAsync("Delete success", $"Profile '{name}' deleted.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await _dialog.ShowMessageAsync("Delete failed", ex.Message);
        }
    }
}
