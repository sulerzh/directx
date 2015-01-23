using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public class StringEqualityConverter : IValueConverter
    {
        public string ReferenceValue { get; set; }

        public string ConvertBackValueForTrue { get; set; }

        public string ConvertBackValueForFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value as string) == this.ReferenceValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                    return (object)this.ConvertBackValueForTrue;
                else
                    return (object)this.ConvertBackValueForFalse;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
