using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Enums;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IBoardRepository _boards;
    private readonly ITemplateRepository _templates;

    [ObservableProperty] private DashboardStats stats = new();

    public ObservableCollection<UsageBarItem> ShapeUsageBars { get; } = new();
    public ObservableCollection<UsageBarItem> TemplateUsageBars { get; } = new();

    public DashboardViewModel(IBoardRepository boards, ITemplateRepository templates)
    {
        _boards = boards;
        _templates = templates;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        // 1) Load raw data
        var boards = await _boards.GetAllWithShapesAsync();
        var templates = await _templates.GetAllAsync();

        // 2) Compute stats
        var totalBoards = boards.Count;
        var totalShapes = boards.Sum(b => b.Shapes?.Count ?? 0);
        var totalTemplates = templates.Count;

        var shapeTypeCounts = boards
            .SelectMany(b => b.Shapes ?? new List<BoardShape>())
            .GroupBy(s => s.ShapeType)
            .ToDictionary(g => g.Key, g => g.Count());

        var templateTypeCounts = templates
            .GroupBy(t => t.ShapeType)
            .ToDictionary(g => g.Key, g => g.Count());

        var topBoards = boards
            .Select(b => new TopBoardItem
            {
                Id = b.Id,
                Name = b.Name,
                ShapeCount = b.Shapes?.Count ?? 0
            })
            .OrderByDescending(x => x.ShapeCount)
            .Take(5)
            .ToList();

        Stats = new DashboardStats
        {
            TotalBoards = totalBoards,
            TotalShapes = totalShapes,
            TotalTemplates = totalTemplates,
            AvgShapesPerBoard = totalBoards == 0 ? 0 : Math.Round((double)totalShapes / totalBoards, 2),
            ShapeTypeCounts = shapeTypeCounts,
            TemplateTypeCounts = templateTypeCounts,
            TopBoards = topBoards
        };

        // 3) Build chart-like bars
        BuildBars(ShapeUsageBars, shapeTypeCounts);
        BuildBars(TemplateUsageBars, templateTypeCounts);
    }

    private static void BuildBars(
        ObservableCollection<UsageBarItem> target,
        Dictionary<ShapeType, int> counts)
    {
        target.Clear();

        // ensure all enum values appear
        var allTypes = Enum.GetValues<ShapeType>().ToList();

        var normalized = allTypes
            .Select(t => new { Type = t, Count = counts.TryGetValue(t, out var c) ? c : 0 })
            .Where(x => x.Count > 0)
            .OrderByDescending(x => x.Count)
            .ToList();

        var total = normalized.Sum(x => x.Count);
        if (total == 0) return;

        foreach (var x in normalized)
        {
            var ratio = (double)x.Count / total;
            target.Add(new UsageBarItem
            {
                Label = x.Type.ToString(),
                Count = x.Count,
                Ratio = ratio,
                PercentText = $"{Math.Round(ratio * 100, 1)}%"
            });
        }
    }
}

public class UsageBarItem
{
    public string Label { get; set; } = "";
    public int Count { get; set; }

    public double Ratio { get; set; }

    public string PercentText { get; set; } = "";
}
