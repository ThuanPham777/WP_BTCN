using System.Threading.Tasks;

namespace DrawingApp.UI.Services;

public interface IDialogService
{
    Task ShowMessageAsync(string title, string content);
}