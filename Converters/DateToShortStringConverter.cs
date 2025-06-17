using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using System.Globalization;

namespace Sobs.Converters
{
    public class DateToShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dto)
            return dto.ToString("dd/MM/yy");

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && DateTimeOffset.TryParseExact(s, "dd/MM/yy", culture, DateTimeStyles.None, out var dto))
            return dto;

        return null;
    }
    }
}