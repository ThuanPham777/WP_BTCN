using Microsoft.UI.Xaml.Controls;
using System;
namespace DrawingApp.UI.Navigation;

public class NavigationService : INavigationService
{
    public Frame? Frame { get; set; }

    public bool CanGoBack => Frame?.CanGoBack == true;

    public void GoBack()
    {
        if (CanGoBack) Frame!.GoBack();
    }

    public void Navigate(Type pageType)
    {
        Frame?.Navigate(pageType);
    }
    public void Navigate(Type pageType, object? parameter)
    {
        Frame?.Navigate(pageType, parameter);
    }
}
