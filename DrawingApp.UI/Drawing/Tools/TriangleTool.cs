using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class TriangleTool : IDrawTool
{
    public ShapeType Type => ShapeType.Triangle;
    public Shape? Preview { get; private set; }

    private Point _start;
    private StrokeStyle _style = new();

    public void Begin(Point start, StrokeStyle style)
    {
        _start = start;
        _style = style;

        var poly = new Polygon();
        ShapeFactory.ApplyStroke(poly, style);
        Preview = poly;
    }

    public void Update(Point current)
    {
        if (Preview is not Polygon poly) return;

        // Triangle is derived from bounding box
        var x1 = _start.X;
        var y1 = _start.Y;
        var x2 = current.X;
        var y2 = current.Y;

        var left = Math.Min(x1, x2);
        var right = Math.Max(x1, x2);
        var top = Math.Min(y1, y2);
        var bottom = Math.Max(y1, y2);

        var midX = (left + right) / 2;

        poly.Points.Clear();
        poly.Points.Add(new Windows.Foundation.Point(midX, top));
        poly.Points.Add(new Windows.Foundation.Point(right, bottom));
        poly.Points.Add(new Windows.Foundation.Point(left, bottom));
    }

    public Shape? End(Point end)
    {
        Update(end);
        var result = Preview;
        Preview = null;
        return result;
    }
}
