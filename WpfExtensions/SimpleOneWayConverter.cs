using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public abstract class SimpleOneWayConverter<TSource, TResult> : IValueConverter
  {
    protected abstract bool TryConvert(TSource source, out TResult result);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      TResult result;
      if (value == null || !(value is TSource) || !this.TryConvert((TSource) value, out result))
        return DependencyProperty.UnsetValue;
      else
        return (object) result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
