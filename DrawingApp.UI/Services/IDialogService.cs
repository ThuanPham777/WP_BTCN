using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace DrawingApp.Core.Interfaces.Services;

public interface IDialogService
{
    void SetXamlRoot(XamlRoot xamlRoot);

    Task ShowMessageAsync(string title, string message);
    Task<bool> ShowConfirmAsync(string title, string message, string ok = "OK", string cancel = "Cancel");
}
