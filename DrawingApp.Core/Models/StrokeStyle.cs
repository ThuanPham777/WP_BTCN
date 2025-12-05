using DrawingApp.Core.Enums;

namespace DrawingApp.Core.Models;

public sealed class StrokeStyle
{
    public string StrokeColor { get; set; } = "#FF000000";
    public string? FillColor { get; set; }
    public double Thickness { get; set; } = 2;
    public StrokeDash Dash { get; set; } = StrokeDash.Solid;
}
