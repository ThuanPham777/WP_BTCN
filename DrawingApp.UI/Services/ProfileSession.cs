using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Services;
using System;

namespace DrawingApp.UI.Services;

public class ProfileSession : IProfileSession
{
    public Profile? CurrentProfile { get; private set; }

    public event Action<Profile?>? ProfileChanged;

    public void SetProfile(Profile profile)
    {
        CurrentProfile = profile;
        ProfileChanged?.Invoke(CurrentProfile);
    }

    public void Clear()
    {
        CurrentProfile = null;
        ProfileChanged?.Invoke(CurrentProfile);
    }
}
