using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using DrawingApp.UI.Drawing;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class TriangleTool : IDrawTool
{
    public ShapeType Type => ShapeType.Triangle;

    private Point _start;
    private Polygon? _poly;

    public Shape? Preview => _poly;

    public void Begin(Point start, StrokeStyle style)
    {
        _start = start;

        _poly = new Polygon
        {
            Points = new Microsoft.UI.Xaml.Media.PointCollection()
        };

        ShapeFactory.ApplyStroke(_poly, style);

        _poly.Points.Add(start);
        _poly.Points.Add(start);
        _poly.Points.Add(start);
    }

    public void Update(Point current)
    {
        if (_poly == null) return;

        var left = Math.Min(_start.X, current.X);
        var top = Math.Min(_start.Y, current.Y);
        var right = Math.Max(_start.X, current.X);
        var bottom = Math.Max(_start.Y, current.Y);

        var midX = (left + right) / 2;

        var p1 = new Point(midX, top);
        var p2 = new Point(right, bottom);
        var p3 = new Point(left, bottom);

        _poly.Points[0] = p1;
        _poly.Points[1] = p2;
        _poly.Points[2] = p3;
    }

    public Shape? End(Point end)
    {
        if (_poly == null) return null;

        Update(end);

        var result = _poly;
        _poly = null;

        return result;
    }
}
