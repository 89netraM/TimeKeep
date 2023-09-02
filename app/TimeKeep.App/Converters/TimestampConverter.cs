using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace TimeKeep.App.Converters;

public class TimestampConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Timestamp timestamp || targetType != typeof(string))
        {
            return value;
        }

        var dateTime = timestamp.ToDateTimeOffset().ToLocalTime();
        
        return dateTime.Date != DateTime.Today ?
            $"{dateTime:yyyy-MM-dd HH:mm}" :
            $"{dateTime:HH:mm}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}