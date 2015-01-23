using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }

        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            else
                return (object)((bool)value ? this.TrueValue : this.FalseValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            else
                return (((bool)value) ? this.TrueValue : this.FalseValue);

        }
    }
}
