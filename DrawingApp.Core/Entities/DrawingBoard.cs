using System;
using System.Collections.Generic;

namespace DrawingApp.Core.Entities;

public class DrawingBoard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Untitled Board";

    public double Width { get; set; }
    public double Height { get; set; }
    public string BackgroundColor { get; set; } = "#FFFFFFFF";

    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<BoardShape> Shapes { get; set; } = new();
}
