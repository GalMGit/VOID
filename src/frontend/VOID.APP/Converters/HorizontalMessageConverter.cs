using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace VOID.APP.Converters;

public class HorizontalMessageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}