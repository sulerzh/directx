using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class DateTimeToDoubleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      DateTime? nullable = (DateTime?) value;
      if (nullable.HasValue)
        return (object) new double?((double) nullable.Value.Ticks);
      else
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double? nullable = (double?) value;
      if (nullable.HasValue)
        return (object) new DateTime?(new DateTime((long) nullable.Value));
      else
        return DependencyProperty.UnsetValue;
    }
  }
}
