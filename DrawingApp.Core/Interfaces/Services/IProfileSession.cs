using DrawingApp.Core.Entities;
using System;

namespace DrawingApp.Core.Interfaces.Services;

public interface IProfileSession
{
    Profile? Current { get; }
    event Action<Profile?>? ProfileChanged;

    void SetProfile(Profile profile);
    void Clear();
}