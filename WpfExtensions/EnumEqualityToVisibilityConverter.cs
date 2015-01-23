using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class EnumEqualityToVisibilityConverter : IValueConverter
  {
    public Visibility VisibilityForMatch { get; set; }

    public Visibility VisibilityForMismatch { get; set; }

    public EnumEqualityToVisibilityConverter()
    {
      this.VisibilityForMatch = Visibility.Visible;
      this.VisibilityForMismatch = Visibility.Collapsed;
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
        return (object) (Visibility) (Enum.Parse(value.GetType(), str).Equals(value) ? (int) this.VisibilityForMatch : (int) this.VisibilityForMismatch);
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
