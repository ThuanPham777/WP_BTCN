using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawingApp.Core.Entities;
using DrawingApp.Core.Enums;
using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Interfaces.Services;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

    public IReadOnlyList<ShapeType> ToolOptions { get; }
        = Enum.GetValues<ShapeType>();

    [ObservableProperty] private Guid? currentBoardId;
    [ObservableProperty] private string boardName = "Untitled Board";

    [ObservableProperty] private double boardWidth;
    [ObservableProperty] private double boardHeight;
    [ObservableProperty] private string boardBackground = "#FFFFFFFF";

    [ObservableProperty] private ShapeType currentTool = ShapeType.Line;

    [ObservableProperty] private string strokeColor = "#FF0F172A";
    [ObservableProperty] private string? fillColor;
    [ObservableProperty] private double thickness = 2;

    [ObservableProperty] private bool isFillMode;
    [ObservableProperty] private bool isSelectionMode;

    public ObservableCollection<Shape> RuntimeShapes { get; } = new();

    public ObservableCollection<Shape> SelectedShapes { get; } = new();

    public bool HasSelection => SelectedShapes.Count > 0;

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

        SelectedShapes.CollectionChanged += SelectedShapes_CollectionChanged;
    }

    private void SelectedShapes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(HasSelection));

    public void SetSelectedShapes(IEnumerable<Shape> shapes)
    {
        SelectedShapes.Clear();
        foreach (var s in shapes)
            SelectedShapes.Add(s);

        OnPropertyChanged(nameof(HasSelection));
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

    // ==========================
    // Save Board (create/update)
    // ==========================
    [RelayCommand]
    public async Task SaveBoardAsync()
    {
        var profile = _session.CurrentProfile;
        if (profile == null)
        {
            await _dialog.ShowMessageAsync("Profile required", "Please select a profile first.");
            return;
        }

        var shapes = RuntimeShapes.Select(MapRuntimeShape).ToList();

        if (CurrentBoardId.HasValue)
        {
            var id = CurrentBoardId.Value;
            try
            {
                await _boards.UpdateContentAsync(
                    id,
                    BoardName,
                    BoardWidth,
                    BoardHeight,
                    BoardBackground,
                    shapes);

                await _dialog.ShowMessageAsync("Saved", "Board updated successfully.");
                return;
            }
            catch (Exception ex)
            {
                await _dialog.ShowMessageAsync("Save failed", ex.Message);
                return;
            }
        }

        var newBoard = new DrawingBoard
        {
            Name = BoardName,
            Width = BoardWidth,
            Height = BoardHeight,
            BackgroundColor = BoardBackground,
            ProfileId = profile.Id,
            Shapes = shapes
        };

        await _boards.AddAsync(newBoard);
        CurrentBoardId = newBoard.Id;

        await _dialog.ShowMessageAsync("Saved", "Board created successfully.");
    }

    // ==========================
    // Save Selection -> Templates
    // ==========================
    [RelayCommand]
    public async Task SaveSelectionAsync()
    {
        if (!HasSelection) return;

        try
        {
            int i = 1;
            foreach (var s in SelectedShapes)
            {
                var template = MapRuntimeShapeToTemplate(s, i++);
                await _templates.AddAsync(template);
            }

            await _dialog.ShowMessageAsync("Templates",
                $"Saved {SelectedShapes.Count} template(s).");
        }
        catch (Exception ex)
        {
            await _dialog.ShowMessageAsync("Save selection failed", ex.Message);
        }
    }

    private ShapeTemplate MapRuntimeShapeToTemplate(Shape s, int index)
        => new()
        {
            Name = $"{GuessType(s)} Template {DateTime.Now:HHmmss}-{index}",
            ShapeType = GuessType(s),
            StrokeColor = ExtractBrushHex(s.Stroke) ?? "#FF000000",
            FillColor = ExtractBrushHex(s.Fill),
            Thickness = s.StrokeThickness,
            GeometryJson = SerializeShapeGeometry(s),
        };

    // ==========================
    // Mapping shape -> BoardShape
    // IMPORTANT:
    // Lấy style từ chính shape để tránh bug:
    // "đổi stroke mới làm shape cũ đổi theo"
    // ==========================
    private BoardShape MapRuntimeShape(Shape s)
    {
        var strokeHex = ExtractBrushHex(s.Stroke) ?? "#FF000000";
        var fillHex = ExtractBrushHex(s.Fill);

        return new BoardShape
        {
            ShapeType = GuessType(s),
            StrokeColor = strokeHex,
            FillColor = fillHex,
            Thickness = s.StrokeThickness,
            DashStyle = StrokeDash.Solid,
            GeometryJson = SerializeShapeGeometry(s)
        };
    }

    private static string? ExtractBrushHex(Brush? brush)
    {
        if (brush is SolidColorBrush scb)
        {
            var c = scb.Color;
            return $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
        }
        return null;
    }

    private static ShapeType GuessType(Shape s)
        => s switch
        {
            Line => ShapeType.Line,
            Rectangle => ShapeType.Rectangle,
            Ellipse => ShapeType.Oval,
            Polygon => ShapeType.Polygon,
            _ => ShapeType.Line
        };

    private static string SerializeShapeGeometry(Shape s)
    {
        if (s is Line l)
            return JsonSerializer.Serialize(new { x1 = l.X1, y1 = l.Y1, x2 = l.X2, y2 = l.Y2 });

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

    // ==========================
    // Load Board
    // ==========================
    public async Task LoadBoardAsync(Guid boardId)
    {
        CurrentBoardId = boardId;

        var board = await _boards.GetByIdAsync(CurrentBoardId);
        if (board == null)
        {
            await _dialog.ShowMessageAsync("Board", "Board not found.");
            return;
        }

        BoardName = board.Name;
        BoardWidth = board.Width;
        BoardHeight = board.Height;
        BoardBackground = board.BackgroundColor ?? "#FFFFFFFF";

        RuntimeShapes.Clear();

        foreach (var bs in board.Shapes)
        {
            var shape = BuildShapeFromBoardShape(bs);
            if (shape != null)
                RuntimeShapes.Add(shape);
        }

        // Clear selection khi load board khác
        SelectedShapes.Clear();
        OnPropertyChanged(nameof(HasSelection));

        await _dialog.ShowMessageAsync("Board loaded", $"Loaded '{board.Name}'.");
    }

    public async Task<Shape?> LoadTemplateAsShapeAsync(Guid templateId)
    {
        var t = await _templates.GetByIdAsync(templateId);
        if (t == null)
        {
            await _dialog.ShowMessageAsync("Template", "Template not found.");
            return null;
        }

        // build shape theo type + geometry
        Shape? shape = t.ShapeType switch
        {
            Core.Enums.ShapeType.Line => BuildLine(t.GeometryJson),
            Core.Enums.ShapeType.Rectangle => BuildRectangle(t.GeometryJson),
            Core.Enums.ShapeType.Oval or Core.Enums.ShapeType.Circle => BuildEllipse(t.GeometryJson),
            Core.Enums.ShapeType.Polygon => BuildPolygon(t.GeometryJson),
            Core.Enums.ShapeType.Triangle => BuildPolygon(t.GeometryJson),
            _ => null
        };

        if (shape == null) return null;

        // apply style từ template
        var style = new StrokeStyle
        {
            StrokeColor = t.StrokeColor,
            FillColor = t.FillColor,
            Thickness = t.Thickness,
            Dash = t.DashStyle
        };

        DrawingApp.UI.Drawing.ShapeFactory.ApplyStroke(shape, style);

        return shape;
    }

    private Shape? BuildShapeFromBoardShape(BoardShape bs)
    {
        var style = new StrokeStyle
        {
            StrokeColor = bs.StrokeColor,
            FillColor = bs.FillColor,
            Thickness = bs.Thickness,
            Dash = bs.DashStyle
        };

        Shape? shape = bs.ShapeType switch
        {
            ShapeType.Line => BuildLine(bs.GeometryJson),
            ShapeType.Rectangle => BuildRectangle(bs.GeometryJson),
            ShapeType.Oval or ShapeType.Circle => BuildEllipse(bs.GeometryJson),
            ShapeType.Polygon => BuildPolygon(bs.GeometryJson),
            ShapeType.Triangle => BuildPolygon(bs.GeometryJson),
            _ => null
        };

        if (shape != null)
            DrawingApp.UI.Drawing.ShapeFactory.ApplyStroke(shape, style);

        return shape;
    }

    private static Line? BuildLine(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new Line
            {
                X1 = root.GetProperty("x1").GetDouble(),
                Y1 = root.GetProperty("y1").GetDouble(),
                X2 = root.GetProperty("x2").GetDouble(),
                Y2 = root.GetProperty("y2").GetDouble()
            };
        }
        catch { return null; }
    }

    private static Rectangle? BuildRectangle(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var r = new Rectangle
            {
                Width = root.GetProperty("w").GetDouble(),
                Height = root.GetProperty("h").GetDouble()
            };

            Microsoft.UI.Xaml.Controls.Canvas.SetLeft(r, root.GetProperty("x").GetDouble());
            Microsoft.UI.Xaml.Controls.Canvas.SetTop(r, root.GetProperty("y").GetDouble());
            return r;
        }
        catch { return null; }
    }

    private static Ellipse? BuildEllipse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var e = new Ellipse
            {
                Width = root.GetProperty("w").GetDouble(),
                Height = root.GetProperty("h").GetDouble()
            };

            Microsoft.UI.Xaml.Controls.Canvas.SetLeft(e, root.GetProperty("x").GetDouble());
            Microsoft.UI.Xaml.Controls.Canvas.SetTop(e, root.GetProperty("y").GetDouble());
            return e;
        }
        catch { return null; }
    }

    private static Polygon? BuildPolygon(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var poly = new Polygon
            {
                Points = new Microsoft.UI.Xaml.Media.PointCollection()
            };

            foreach (var p in root.GetProperty("points").EnumerateArray())
            {
                poly.Points.Add(new Windows.Foundation.Point(
                    p.GetProperty("x").GetDouble(),
                    p.GetProperty("y").GetDouble()));
            }

            return poly;
        }
        catch { return null; }
    }

    // ==========================
    // Fill helper
    // ==========================
    public void ApplyFillTo(Shape shape)
    {
        var hex = string.IsNullOrWhiteSpace(FillColor) ? StrokeColor : FillColor!;
        shape.Fill = new SolidColorBrush(ParseColor(hex));
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return Windows.UI.Color.FromArgb(255, 0, 0, 0);

        if (hex.StartsWith("#")) hex = hex[1..];
        if (hex.Length == 6) hex = "FF" + hex;

        byte a = Convert.ToByte(hex.Substring(0, 2), 16);
        byte r = Convert.ToByte(hex.Substring(2, 2), 16);
        byte g = Convert.ToByte(hex.Substring(4, 2), 16);
        byte b = Convert.ToByte(hex.Substring(6, 2), 16);
        return Windows.UI.Color.FromArgb(a, r, g, b);
    }
}
