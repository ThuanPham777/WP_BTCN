using DrawingApp.Core.Interfaces.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace DrawingApp.UI.Services;

public class DialogService : IDialogService
{
    private XamlRoot? _xamlRoot;

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        if (_xamlRoot == null)
        {
            System.Diagnostics.Debug.WriteLine("[DialogService] XamlRoot is null. Dialog not shown.");
            return;
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, string ok = "OK", string cancel = "Cancel")
    {
        if (_xamlRoot == null)
        {
            System.Diagnostics.Debug.WriteLine("[DialogService] XamlRoot is null. Confirm not shown.");
            return false;
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = ok,
            CloseButtonText = cancel,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
