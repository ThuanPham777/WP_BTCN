using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Services;
using System;

namespace DrawingApp.UI.Services;

public class ProfileSession : IProfileSession
{
    public Profile? Current { get; private set; }

    public event Action<Profile?>? ProfileChanged;

    public void SetProfile(Profile profile)
    {
        Current = profile;
        ProfileChanged?.Invoke(Current);
    }

    public void Clear()
    {
        Current = null;
        ProfileChanged?.Invoke(Current);
    }
}
