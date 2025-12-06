using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DrawingApp.UI.Pages;

public sealed partial class TemplatesPage : Page
{
    public TemplatesViewModel ViewModel { get; }

    public TemplatesPage()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<TemplatesViewModel>();
        DataContext = ViewModel;

        Loaded += TemplatesPage_Loaded;
    }

    private async void TemplatesPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}
