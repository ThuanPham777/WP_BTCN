using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using DrawingApp.UI.Drawing;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class LineTool : IDrawTool
{
    public ShapeType Type => ShapeType.Line;

    private Point _start;
    private Line? _line;

    public Shape? Preview => _line;

    public void Begin(Point start, StrokeStyle style)
    {
        _start = start;

        _line = new Line
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = start.X,
            Y2 = start.Y,
        };

        ShapeFactory.ApplyStroke(_line, style);
    }

    public void Update(Point current)
    {
        if (_line == null) return;

        _line.X2 = current.X;
        _line.Y2 = current.Y;
    }

    public Shape? End(Point end)
    {
        if (_line == null) return null;

        Update(end);

        var result = _line;
        _line = null;

        return result;
    }
}
