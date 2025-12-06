using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Interfaces.Services;
using DrawingApp.UI.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProfileRepository _profiles;
    private readonly IProfileSession _session;
    private readonly INavigationService _nav;

    [ObservableProperty]
    private ObservableCollection<Profile> items = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoDrawingCommand))]
    private Profile? selected;

    public MainViewModel(
        IProfileRepository profiles,
        IProfileSession session,
        INavigationService nav)
    {
        _profiles = profiles;
        _session = session;
        _nav = nav;
    }
    public bool CanGoDrawing => Selected != null;

    partial void OnSelectedChanged(Profile? value)
    {
        OnPropertyChanged(nameof(CanGoDrawing));
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var profiles = await _profiles.GetAllAsync();
        Items = new ObservableCollection<Profile>(profiles);
    }

    [RelayCommand(CanExecute = nameof(CanGoDrawing))]
    public void GoDrawing()
    {
        if (Selected == null) return;

        _session.SetProfile(Selected);

        // Navigate sang DrawingPage
        _nav.Navigate(typeof(Pages.DrawingPage));
    }

    [RelayCommand]
    public void GoProfiles()
    {
        _nav.Navigate(typeof(Pages.ProfilePage));
    }
}
