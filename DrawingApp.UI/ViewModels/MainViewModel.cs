using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Interfaces.Services;
using DrawingApp.UI.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProfileRepository _profiles;
    private readonly IProfileSession _session;
    private readonly INavigationService _nav;

    [ObservableProperty] private List<Profile> items = new();
    [ObservableProperty] private Profile? selected;

    public MainViewModel(
        IProfileRepository profiles,
        IProfileSession session,
        INavigationService nav)
    {
        _profiles = profiles;
        _session = session;
        _nav = nav;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Items = await _profiles.GetAllAsync();
    }

    [RelayCommand]
    public void UseSelected()
    {
        if (Selected == null) return;

        _session.SetProfile(Selected);
        _nav.Navigate(typeof(Pages.DrawingPage));
    }

    [RelayCommand]
    public void GoProfiles()
    {
        _nav.Navigate(typeof(Pages.ProfilePage));
    }
}
