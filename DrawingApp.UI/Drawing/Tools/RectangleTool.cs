using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using DrawingApp.UI.Drawing;
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

        _rect = new Rectangle
        {
            Width = 0,
            Height = 0
        };

        ShapeFactory.ApplyStroke(_rect, style);
        Canvas.SetLeft(_rect, start.X);
        Canvas.SetTop(_rect, start.Y);
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
        if (_rect == null) return null;

        Update(end);

        var result = _rect;
        _rect = null;

        return result;
    }
}
