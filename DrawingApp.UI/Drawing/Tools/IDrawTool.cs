using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public interface IDrawTool
{
    ShapeType Type { get; }
    void Begin(Point start, StrokeStyle style);
    void Update(Point current);
    Shape? End(Point end);
    Shape? Preview { get; }
}
