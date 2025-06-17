using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Controls;
using Avalonia;

namespace Sobs.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        // Returns true if value is null → placeholder should be visible
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}