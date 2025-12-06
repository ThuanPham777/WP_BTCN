using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Enums;
using DrawingApp.Core.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IStatisticsService _stats;

    [ObservableProperty]
    private Dictionary<ShapeType, int> usage = new();

    public DashboardViewModel(IStatisticsService stats) => _stats = stats;

    [RelayCommand]
    public async Task LoadAsync()
    {
        Usage = await _stats.GetShapeUsageAsync();
    }
}
