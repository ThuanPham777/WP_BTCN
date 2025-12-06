using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class EllipseTool : IDrawTool
{
    public ShapeType Type { get; }

    private Point _start;
    private Ellipse? _ellipse;

    public Shape? Preview => _ellipse;

    public EllipseTool(ShapeType type)
    {
        Type = type; // Oval hoặc Circle
    }

    public void Begin(Point start, StrokeStyle style)
    {
        _start = start;

        _ellipse = new Ellipse
        {
            Width = 0,
            Height = 0
        };

        ShapeFactory.ApplyStroke(_ellipse, style);
        Canvas.SetLeft(_ellipse, start.X);
        Canvas.SetTop(_ellipse, start.Y);
    }

    public void Update(Point current)
    {
        if (_ellipse == null) return;

        var left = Math.Min(_start.X, current.X);
        var top = Math.Min(_start.Y, current.Y);
        var right = Math.Max(_start.X, current.X);
        var bottom = Math.Max(_start.Y, current.Y);

        var w = right - left;
        var h = bottom - top;

        if (Type == ShapeType.Circle)
        {
            // ép thành hình tròn
            var size = Math.Min(w, h);
            w = size;
            h = size;

            // giữ circle theo hướng kéo
            if (current.X < _start.X) left = _start.X - size;
            if (current.Y < _start.Y) top = _start.Y - size;
        }

        _ellipse.Width = w;
        _ellipse.Height = h;

        Canvas.SetLeft(_ellipse, left);
        Canvas.SetTop(_ellipse, top);
    }

    public Shape? End(Point end)
    {
        Update(end);
        return _ellipse;
    }
}
