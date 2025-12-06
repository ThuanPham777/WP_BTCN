using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Enums;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Interfaces.Services;
using DrawingApp.Core.Models;
using DrawingApp.UI.Services;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DrawingApp.UI.ViewModels;

public partial class DrawingViewModel : ObservableObject
{
    private readonly IProfileSession _session;
    private readonly IBoardRepository _boards;
    private readonly ITemplateRepository _templates;
    private readonly IDialogService _dialog;

    [ObservableProperty] private double boardWidth;
    [ObservableProperty] private double boardHeight;
    [ObservableProperty] private string boardBackground = "#FFFFFFFF";

    [ObservableProperty] private ShapeType currentTool = ShapeType.Line;

    [ObservableProperty] private string strokeColor = "#FF0F172A";
    [ObservableProperty] private string? fillColor;
    [ObservableProperty] private double thickness = 2;

    public List<Shape> RuntimeShapes { get; } = new();

    public DrawingViewModel(
        IProfileSession session,
        IBoardRepository boards,
        ITemplateRepository templates,
        IDialogService dialog)
    {
        _session = session;
        _boards = boards;
        _templates = templates;
        _dialog = dialog;

        ApplyProfileDefaults();
        _session.ProfileChanged += _ => ApplyProfileDefaults();
    }

    private void ApplyProfileDefaults()
    {
        var p = _session.CurrentProfile;
        BoardWidth = p?.DefaultBoardWidth ?? 900;
        BoardHeight = p?.DefaultBoardHeight ?? 600;

        StrokeColor = p?.DefaultStrokeColor ?? "#FF0F172A";
        Thickness = p?.DefaultStrokeThickness ?? 2;
    }

    public StrokeStyle BuildStyle()
        => new()
        {
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            Thickness = Thickness
        };

    [RelayCommand]
    public async Task SaveBoardAsync()
    {
        var profile = _session.CurrentProfile;
        if (profile == null)
        {
            await _dialog.ShowMessageAsync("Profile required", "Please select a profile first.");
            return;
        }

        var board = new DrawingBoard
        {
            Name = $"Board {DateTime.Now:HHmmss}",
            Width = BoardWidth,
            Height = BoardHeight,
            BackgroundColor = BoardBackground,
            ProfileId = profile.Id,
            Shapes = RuntimeShapes.Select(MapRuntimeShape).ToList()
        };

        await _boards.AddAsync(board);
        await _dialog.ShowMessageAsync("Saved", "Board saved successfully.");
    }

    [RelayCommand]
    public async Task SaveLastShapeAsTemplateAsync()
    {
        if (RuntimeShapes.Count == 0) return;

        var last = RuntimeShapes.Last();
        var template = new ShapeTemplate
        {
            Name = $"Template {DateTime.Now:HHmmss}",
            ShapeType = GuessType(last),
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            Thickness = Thickness,
            GeometryJson = SerializeShapeGeometry(last)
        };

        await _templates.AddAsync(template);
        await _dialog.ShowMessageAsync("Template", "Saved last shape as template.");
    }

    private BoardShape MapRuntimeShape(Shape s)
    {
        return new BoardShape
        {
            ShapeType = GuessType(s),
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            Thickness = Thickness,
            GeometryJson = SerializeShapeGeometry(s)
        };
    }

    private static ShapeType GuessType(Shape s)
    {
        return s switch
        {
            Line => ShapeType.Line,
            Rectangle => ShapeType.Rectangle,
            Ellipse => ShapeType.Oval, // Circle phân biệt bằng geometry ở UI/tool
            Polygon => ShapeType.Polygon,
            _ => ShapeType.Line
        };
    }

    private static string SerializeShapeGeometry(Shape s)
    {
        if (s is Line l)
        {
            return JsonSerializer.Serialize(new
            {
                x1 = l.X1,
                y1 = l.Y1,
                x2 = l.X2,
                y2 = l.Y2
            });
        }

        if (s is Rectangle r)
        {
            var x = Microsoft.UI.Xaml.Controls.Canvas.GetLeft(r);
            var y = Microsoft.UI.Xaml.Controls.Canvas.GetTop(r);
            return JsonSerializer.Serialize(new { x, y, w = r.Width, h = r.Height });
        }

        if (s is Ellipse e)
        {
            var x = Microsoft.UI.Xaml.Controls.Canvas.GetLeft(e);
            var y = Microsoft.UI.Xaml.Controls.Canvas.GetTop(e);
            return JsonSerializer.Serialize(new { x, y, w = e.Width, h = e.Height });
        }

        if (s is Polygon p)
        {
            var pts = p.Points.Select(pt => new { x = pt.X, y = pt.Y }).ToList();
            return JsonSerializer.Serialize(new { points = pts });
        }

        return "{}";
    }
}
