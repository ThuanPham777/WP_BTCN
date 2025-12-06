using CommunityToolkit.Mvvm.ComponentModel;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Services;

namespace DrawingApp.UI.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IProfileSession _session;

    [ObservableProperty] private Profile? currentProfile;
    [ObservableProperty] private bool canDraw;

    public ShellViewModel(IProfileSession session)
    {
        _session = session;

        CurrentProfile = _session.CurrentProfile;
        CanDraw = CurrentProfile != null;

        _session.ProfileChanged += OnProfileChanged;
    }

    private void OnProfileChanged(Profile? profile)
    {
        CurrentProfile = profile;
        CanDraw = profile != null;
    }
}
