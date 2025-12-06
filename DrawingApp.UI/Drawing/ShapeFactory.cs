using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace DrawingApp.UI.Drawing;

public static class ShapeFactory
{
    public static void ApplyStroke(Shape shape, StrokeStyle style)
    {
        shape.Stroke = new SolidColorBrush(ParseColor(style.StrokeColor));
        shape.StrokeThickness = style.Thickness;

        if (!string.IsNullOrWhiteSpace(style.FillColor))
            shape.Fill = new SolidColorBrush(ParseColor(style.FillColor!));
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        // Expect #AARRGGBB
        if (hex.StartsWith("#")) hex = hex[1..];
        byte a = Convert.ToByte(hex.Substring(0, 2), 16);
        byte r = Convert.ToByte(hex.Substring(2, 2), 16);
        byte g = Convert.ToByte(hex.Substring(4, 2), 16);
        byte b = Convert.ToByte(hex.Substring(6, 2), 16);
        return Windows.UI.Color.FromArgb(a, r, g, b);
    }
}
