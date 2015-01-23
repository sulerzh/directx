using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public class IntegerEqualityTestConverter : IValueConverter
    {
        public bool ValueIfEqual { get; set; }

        public IntegerEqualityTestConverter()
        {
            this.ValueIfEqual = true;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = parameter as string;
            int result;
            if (s == null || !int.TryParse(s, out result))
                return DependencyProperty.UnsetValue;
            if (value == null)
                throw new ArgumentNullException("value");
            try
            {
                return !((result == ((int)value)) ^ this.ValueIfEqual);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
