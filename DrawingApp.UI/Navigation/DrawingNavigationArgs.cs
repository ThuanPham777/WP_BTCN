using System;

namespace DrawingApp.UI.Navigation;

public sealed class DrawingNavigationArgs
{
    public Guid? BoardId { get; set; }
    public Guid? TemplateId { get; set; }
}
