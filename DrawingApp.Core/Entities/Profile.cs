using DrawingApp.Core.Enums;
using System;
using System.Collections.Generic;

namespace DrawingApp.Core.Entities;

public class Profile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Default";

    public AppTheme Theme { get; set; } = AppTheme.System;

    public double DefaultBoardWidth { get; set; }
    public double DefaultBoardHeight { get; set; }

    public string DefaultStrokeColor { get; set; } = "#FF000000";
    public double DefaultStrokeThickness { get; set; } = 2;
    public StrokeDash DefaultStrokeDash { get; set; } = StrokeDash.Solid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<DrawingBoard> Boards { get; set; } = new();
}
