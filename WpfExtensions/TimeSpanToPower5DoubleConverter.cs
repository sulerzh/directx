using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class TimeSpanToPower5DoubleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      TimeSpan? nullable = (TimeSpan?) value;
      if (nullable.HasValue)
        return (object) new double?(Math.Pow(nullable.Value.TotalSeconds, 0.2) * -1.0);
      else
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double? nullable = (double?) value;
      if (nullable.HasValue)
        return (object) new TimeSpan?(TimeSpan.FromSeconds(Math.Pow(nullable.Value * -1.0, 5.0)));
      else
        return DependencyProperty.UnsetValue;
    }
  }
}
