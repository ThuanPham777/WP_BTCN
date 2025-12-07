using DrawingApp.Core.Enums;
using System;
using System.Collections.Generic;

namespace DrawingApp.Core.Models;

public class DashboardStats
{
    public int TotalBoards { get; set; }
    public int TotalShapes { get; set; }
    public int TotalTemplates { get; set; }

    public double AvgShapesPerBoard { get; set; }

    public Dictionary<ShapeType, int> ShapeTypeCounts { get; set; } = new();
    public Dictionary<ShapeType, int> TemplateTypeCounts { get; set; } = new();

    public List<TopBoardItem> TopBoards { get; set; } = new();
}

public class TopBoardItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int ShapeCount { get; set; }
}
