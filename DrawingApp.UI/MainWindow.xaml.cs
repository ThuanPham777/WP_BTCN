using Microsoft.UI.Xaml;
using DrawingApp.UI.Pages;
namespace DrawingApp.UI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        Content = new ShellPage();
    }
}
