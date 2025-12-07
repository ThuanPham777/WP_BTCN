using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using DrawingApp.UI.Drawing;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class PolygonTool : IDrawTool
{
    public ShapeType Type => ShapeType.Polygon;

    private Polygon? _poly;

    public Shape? Preview => _poly;

    public void Begin(Point start, StrokeStyle style)
    {
        _poly = new Polygon
        {
            Points = new Microsoft.UI.Xaml.Media.PointCollection()
        };

        ShapeFactory.ApplyStroke(_poly, style);

        // không tự add start
        // user sẽ tap để add
    }

    public void Update(Point current)
    {
        if (_poly == null) return;
        if (_poly.Points.Count == 0) return;

        // last is preview
        var lastIndex = _poly.Points.Count - 1;
        _poly.Points[lastIndex] = current;
    }

    public void AddPoint(Point p)
    {
        if (_poly == null) return;

        // first point
        if (_poly.Points.Count == 0)
        {
            _poly.Points.Add(p); // actual
            _poly.Points.Add(p); // preview
            return;
        }

        // convert preview -> actual
        var lastIndex = _poly.Points.Count - 1;
        _poly.Points[lastIndex] = p;

        // add new preview
        _poly.Points.Add(p);
    }

    public Shape? End(Point end)
    {
        if (_poly == null) return null;

        if (_poly.Points.Count == 0)
        {
            _poly = null;
            return null;
        }

        // ensure preview follows end
        Update(end);

        // remove preview point
        if (_poly.Points.Count >= 2)
        {
            _poly.Points.RemoveAt(_poly.Points.Count - 1);
        }

        // if end is meaningfully different from last actual -> add it
        if (_poly.Points.Count > 0)
        {
            var last = _poly.Points[_poly.Points.Count - 1];
            if (Distance(last, end) > 0.5)
                _poly.Points.Add(end);
        }

        var result = _poly.Points.Count >= 3 ? _poly : null;

        _poly = null;

        return result;
    }

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
