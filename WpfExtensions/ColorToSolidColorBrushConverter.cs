using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class ColorToSolidColorBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return DependencyProperty.UnsetValue;
      try
      {
        return (object) new SolidColorBrush((Color) value);
      }
      catch
      {
        return DependencyProperty.UnsetValue;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      SolidColorBrush solidColorBrush = value as SolidColorBrush;
      if (solidColorBrush == null)
        return DependencyProperty.UnsetValue;
      else
        return (object) solidColorBrush.Color;
    }
  }
}
