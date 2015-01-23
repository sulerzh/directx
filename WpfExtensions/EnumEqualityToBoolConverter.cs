using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public class EnumEqualityToBoolConverter : IValueConverter
    {
        public bool ValueForMatch { get; set; }

        public bool ValueForMismatch { get; set; }

        public EnumEqualityToBoolConverter()
        {
            this.ValueForMatch = true;
            this.ValueForMismatch = false;
        }

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
                return (Enum.Parse(value.GetType(), str).Equals(value) ? this.ValueForMatch : this.ValueForMismatch);
            }
            catch (ArgumentException ex)
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
