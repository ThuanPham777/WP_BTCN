using DrawingApp.Core.Enums;
using System;

namespace DrawingApp.Core.Entities;

public class ShapeTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "Template";
    public ShapeType ShapeType { get; set; }

    public string StrokeColor { get; set; } = "#FF000000";
    public string? FillColor { get; set; }
    public double Thickness { get; set; } = 2;
    public StrokeDash DashStyle { get; set; } = StrokeDash.Solid;

    public string GeometryJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
