using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class EllipseTool : IDrawTool
{
    private readonly ShapeType _type;
    public ShapeType Type => _type;

    public Shape? Preview { get; private set; }
    private Point _start;
    private StrokeStyle _style = new();

    public EllipseTool(ShapeType type)
    {
        _type = type; // Oval or Circle
    }

    public void Begin(Point start, StrokeStyle style)
    {
        _start = start;
        _style = style;

        var el = new Ellipse();
        ShapeFactory.ApplyStroke(el, style);
        Preview = el;
    }

    public void Update(Point current)
    {
        if (Preview is not Ellipse el) return;

        var x = Math.Min(_start.X, current.X);
        var y = Math.Min(_start.Y, current.Y);
        var w = Math.Abs(_start.X - current.X);
        var h = Math.Abs(_start.Y - current.Y);

        if (_type == ShapeType.Circle)
        {
            var s = Math.Min(w, h);
            w = h = s;
        }

        Canvas.SetLeft(el, x);
        Canvas.SetTop(el, y);
        el.Width = w;
        el.Height = h;
    }

    public Shape? End(Point end)
    {
        Update(end);
        var result = Preview;
        Preview = null;
        return result;
    }
}
