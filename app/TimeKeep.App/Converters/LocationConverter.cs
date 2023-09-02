using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TimeKeep.RPC.Locations;

namespace TimeKeep.App.Converters;

public class LocationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Location location || targetType != typeof(string))
        {
            return value;
        }

        return location.HasName ?
            $"{location.Name} ({location.Address})" :
            location.Address;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}