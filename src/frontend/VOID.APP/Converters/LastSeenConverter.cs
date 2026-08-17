using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace VOID.APP.Converters;

public class LastSeenConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime lastSeen)
            return string.Empty;

        var now = DateTime.Now;

        if (lastSeen.Date == now.Date)
        {
            return $"был(а) в {lastSeen:HH:mm}";
        }

        if (lastSeen.Date == now.Date.AddDays(-1))
        {
            return $"был(а) вчера в {lastSeen:HH:mm}";
        }

        return $"был(а) {lastSeen:dd.MM.yyyy} в {lastSeen:HH:mm}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}