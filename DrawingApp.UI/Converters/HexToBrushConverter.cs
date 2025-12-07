using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace DrawingApp.UI.Converters;

public class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));

        return new SolidColorBrush(ParseColor(hex));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();

    public static Windows.UI.Color ParseColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return Windows.UI.Color.FromArgb(255, 0, 0, 0);

        if (hex.StartsWith("#")) hex = hex[1..];
        if (hex.Length == 6) hex = "FF" + hex;

        byte a = System.Convert.ToByte(hex.Substring(0, 2), 16);
        byte r = System.Convert.ToByte(hex.Substring(2, 2), 16);
        byte g = System.Convert.ToByte(hex.Substring(4, 2), 16);
        byte b = System.Convert.ToByte(hex.Substring(6, 2), 16);

        return Windows.UI.Color.FromArgb(a, r, g, b);
    }

    public static string ToHex(Windows.UI.Color c)
        => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
}
