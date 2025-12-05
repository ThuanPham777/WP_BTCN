using System.Collections.Generic;

namespace DrawingApp.Core.Models.Geometry;

public sealed class PolygonGeometry
{
    public List<PointDto> Points { get; set; } = new();
}

public sealed class PointDto
{
    public double X { get; set; }
    public double Y { get; set; }
}
