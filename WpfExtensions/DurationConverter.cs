using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class DurationConverter : IValueConverter
    {
        public TimeSpan Minimum { get; set; }

        public TimeSpan Maximum { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (object)((TimeSpan)value).TotalSeconds.ToString("0.00", (IFormatProvider)CultureInfo.CurrentCulture);
            }
            catch (InvalidCastException ex)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;
            if (s == null)
                return DependencyProperty.UnsetValue;
            double result;
            if (!double.TryParse(s, NumberStyles.Float, (IFormatProvider)CultureInfo.CurrentCulture, out result))
                return DependencyProperty.UnsetValue;
            try
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(result);
                if (timeSpan > this.Maximum)
                    return (object)this.Maximum;
                if (timeSpan < this.Minimum)
                    return (object)this.Minimum;
                else
                    return (object)timeSpan;
            }
            catch (OverflowException ex)
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
