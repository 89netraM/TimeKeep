using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace TimeKeep.App.Converters;

public class ThicknessSelector : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d || targetType != typeof(Thickness) || parameter is not string param)
        {
            return null;
        }

        var thickness = Thickness.Parse(param[2..]);
        return param[0] switch
        {
            'l' => new Thickness(d, thickness.Top, thickness.Right, thickness.Bottom),
            't' => new Thickness(thickness.Left, d, thickness.Right, thickness.Bottom),
            'r' => new Thickness(thickness.Left, thickness.Top, d, thickness.Bottom),
            'b' => new Thickness(thickness.Left, thickness.Top, thickness.Right, d),
            _ => null,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
