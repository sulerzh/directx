using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public class TimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value.GetType() == typeof(int)))
                return DependencyProperty.UnsetValue;
            else
                return (object)((int)value).ToString("00");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result;
            if (value == null || !(value.GetType() == typeof(string)) || !int.TryParse((string)value, out result))
                return DependencyProperty.UnsetValue;
            else
                return (object)result;
        }
    }
}
