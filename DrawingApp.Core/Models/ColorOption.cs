namespace DrawingApp.UI.Models;

public class ColorOption
{
    public string Hex { get; set; } = "#FF000000";
    public string? Name { get; set; }

    public override string ToString() => Name ?? Hex;
}
