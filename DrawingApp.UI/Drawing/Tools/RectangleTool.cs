using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class RectangleTool : IDrawTool
{
    public ShapeType Type => ShapeType.Rectangle;

    private Point _start;
    private Rectangle? _rect;

    public Shape? Preview => _rect;

    public void Begin(Point start, StrokeStyle style)
    {
        _start = start;

        _rect = new Rectangle();
        ShapeFactory.ApplyStroke(_rect, style);

        Canvas.SetLeft(_rect, start.X);
        Canvas.SetTop(_rect, start.Y);

        _rect.Width = 0;
        _rect.Height = 0;
    }

    public void Update(Point current)
    {
        if (_rect == null) return;

        var x = Math.Min(_start.X, current.X);
        var y = Math.Min(_start.Y, current.Y);
        var w = Math.Abs(current.X - _start.X);
        var h = Math.Abs(current.Y - _start.Y);

        Canvas.SetLeft(_rect, x);
        Canvas.SetTop(_rect, y);
        _rect.Width = w;
        _rect.Height = h;
    }

    public Shape? End(Point end)
    {
        Update(end);
        return _rect;
    }
}
