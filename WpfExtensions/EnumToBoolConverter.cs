using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    [ValueConversion(typeof(bool), typeof(Enum))]
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = parameter as string;
            if (str == null)
                return DependencyProperty.UnsetValue;
            if (value == null)
                throw new ArgumentNullException("value");
            if (!value.GetType().IsEnum)
                return DependencyProperty.UnsetValue;
            try
            {
                return Enum.Parse(value.GetType(), str).Equals(value);
            }
            catch (ArgumentException ex)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = parameter as string;
            if ((string.IsNullOrEmpty(str) || value == null) && !(bool)value)
            {
                return DependencyProperty.UnsetValue;
            }

            try
            {
                return Enum.Parse(targetType, str);
            }
            catch (ArgumentException ex)
            {
                return DependencyProperty.UnsetValue;
            }

        }
    }
}
