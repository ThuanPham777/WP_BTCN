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
    public Shape? Preview { get; private set; }

    private Point _start;
    private StrokeStyle _style = new();

    public void Begin(Point start, StrokeStyle style)
    {
        _start = start;
        _style = style;

        var rect = new Rectangle();
        ShapeFactory.ApplyStroke(rect, style);
        Preview = rect;
    }

    public void Update(Point current)
    {
        if (Preview is not Rectangle rect) return;

        var x = Math.Min(_start.X, current.X);
        var y = Math.Min(_start.Y, current.Y);
        var w = Math.Abs(_start.X - current.X);
        var h = Math.Abs(_start.Y - current.Y);

        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        rect.Width = w;
        rect.Height = h;
    }

    public Shape? End(Point end)
    {
        Update(end);
        var result = Preview;
        Preview = null;
        return result;
    }
}
