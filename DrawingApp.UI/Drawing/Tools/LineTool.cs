using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class LineTool : IDrawTool
{
    public ShapeType Type => ShapeType.Line;
    public Shape? Preview { get; private set; }

    private Point _start;
    private StrokeStyle _style = new();

    public void Begin(Point start, StrokeStyle style)
    {
        _start = start;
        _style = style;

        var line = new Line
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = start.X,
            Y2 = start.Y
        };

        ShapeFactory.ApplyStroke(line, style);
        Preview = line;
    }

    public void Update(Point current)
    {
        if (Preview is Line line)
        {
            line.X2 = current.X;
            line.Y2 = current.Y;
        }
    }

    public Shape? End(Point end)
    {
        Update(end);
        var result = Preview;
        Preview = null;
        return result;
    }
}
