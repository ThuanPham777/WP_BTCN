using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace DrawingApp.UI.Services;

public class DialogService : IDialogService
{
    private readonly MainWindow _window;

    public DialogService(MainWindow window)
    {
        _window = window;
    }

    public async Task ShowMessageAsync(string title, string content)
    {
        var root = (_window.Content as FrameworkElement)?.XamlRoot;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = root
        };

        await dialog.ShowAsync();
    }
}
