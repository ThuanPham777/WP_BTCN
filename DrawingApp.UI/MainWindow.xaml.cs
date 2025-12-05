using Microsoft.UI.Xaml;

namespace DrawingApp.UI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        ExtendsContentIntoTitleBar = true;
    }

}
