using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class StringNullOrEmptyToValueConverter<T> : IValueConverter
  {
    public T NullValue { get; set; }

    public T NonNullValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (object) (string.IsNullOrEmpty(value as string) ? this.NullValue : this.NonNullValue);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }
}
