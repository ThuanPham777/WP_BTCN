using DrawingApp.Core.Enums;
using Microsoft.UI.Xaml;

namespace DrawingApp.UI.Services;

public class ThemeService : IThemeService
{
    public AppTheme CurrentTheme { get; private set; } = AppTheme.System;

    public void ApplyTheme(AppTheme theme)
    {
        CurrentTheme = theme;

        if (Application.Current is not App app) return;
    }
}
