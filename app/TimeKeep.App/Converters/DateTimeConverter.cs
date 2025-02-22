using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TimeKeep.App.Converters;

public class DateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dateTime || targetType != typeof(string))
        {
            return value;
        }

        return dateTime.Date != DateTime.Today
            ? $"{dateTime:yyyy-MM-dd HH:mm}"
            : $"{dateTime:HH:mm}";
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotImplementedException();
}
