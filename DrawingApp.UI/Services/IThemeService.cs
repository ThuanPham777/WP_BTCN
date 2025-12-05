using DrawingApp.Core.Enums;

namespace DrawingApp.UI.Services;

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    void ApplyTheme(AppTheme theme);
}
