using Microsoft.UI.Xaml.Controls;
using System;

namespace DrawingApp.UI.Pages;

public sealed partial class ManagementShellPage : Page
{
    public ManagementShellPage()
    {
        this.InitializeComponent();
        MgmtFrame.Navigate(typeof(BoardsPage));
    }

    private void MgmtNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            switch (item.Tag?.ToString())
            {
                case "boards":
                    MgmtFrame.Navigate(typeof(BoardsPage));
                    break;
                case "templates":
                    MgmtFrame.Navigate(typeof(TemplatesPage));
                    break;
                case "dashboard":
                    MgmtFrame.Navigate(typeof(DashboardPage));
                    break;
            }
        }
    }
}
