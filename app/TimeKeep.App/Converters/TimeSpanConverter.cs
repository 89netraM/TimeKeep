using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TimeKeep.App.Converters;

public class TimeSpanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan timeSpan || targetType != typeof(string))
        {
            return value;
        }

        return $"{double.Floor(timeSpan.TotalHours)}h {timeSpan.Minutes}m";
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotImplementedException();
}
