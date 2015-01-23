using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class IntegerEqualityToVisibilityConverter : IValueConverter
  {
    public Visibility VisibilityForMatch { get; set; }

    public Visibility VisibilityForMismatch { get; set; }

    public IntegerEqualityToVisibilityConverter()
    {
      this.VisibilityForMatch = Visibility.Visible;
      this.VisibilityForMismatch = Visibility.Collapsed;
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
        return (object) (Visibility) (result == (int) value ? (int) this.VisibilityForMatch : (int) this.VisibilityForMismatch);
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
