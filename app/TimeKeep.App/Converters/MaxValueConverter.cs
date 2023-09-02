using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TimeKeep.App.Converters;

public class MaxValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double v && parameter is string parameterString && double.TryParse(parameterString, out var max) && targetType == typeof(double))
        {
            return Math.Min(v, max);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
