using Microsoft.UI.Xaml.Controls;
using System;

namespace DrawingApp.UI.Navigation;

public interface INavigationService
{
    Frame? Frame { get; set; }
    bool CanGoBack { get; }
    void GoBack();
    void Navigate(Type pageType);
    void Navigate(Type pageType, object? parameter = null);
}