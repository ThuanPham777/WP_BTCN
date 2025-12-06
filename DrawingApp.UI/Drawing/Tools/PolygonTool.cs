using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class PolygonTool : IDrawTool
{
    public ShapeType Type => ShapeType.Polygon;
    public Shape? Preview { get; private set; }

    private readonly List<Point> _points = new();
    private StrokeStyle _style = new();

    public void Begin(Point start, StrokeStyle style)
    {
        _points.Clear();
        _points.Add(start);
        _style = style;

        var poly = new Polygon();
        ShapeFactory.ApplyStroke(poly, style);
        Preview = poly;
        Sync();
    }

    public void Update(Point current)
    {
        if (_points.Count == 0) return;

        // Update last "ghost" point
        if (_points.Count == 1)
        {
            _points.Add(current);
        }
        else
        {
            _points[^1] = current;
        }
        Sync();
    }

    public void AddPoint(Point p)
    {
        if (_points.Count == 0) return;
        _points.Insert(_points.Count - 1, p);
        Sync();
    }

    public Shape? End(Point end)
    {
        // finalize by removing ghost if too close
        if (_points.Count >= 3)
        {
            _points[^1] = end;
        }

        Sync();
        var result = Preview;
        Preview = null;
        return result;
    }

    private void Sync()
    {
        if (Preview is not Polygon poly) return;
        poly.Points.Clear();
        foreach (var p in _points)
            poly.Points.Add(p);
    }
}
