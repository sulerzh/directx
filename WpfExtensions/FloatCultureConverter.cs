using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class FloatCultureConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double num1 = (double) (value ?? (object) 0.0);
      int num2 = parameter == null ? 2 : int.Parse(parameter.ToString());
      return (object) num1.ToString(Math.Abs(num1) <= 0.01 || Math.Abs(num1) >= 1E+21 ? (string) null : "N" + (object) num2, (IFormatProvider) culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string s = (string) value;
      double result = 0.0;
      if (string.IsNullOrEmpty(s))
        return (object) result;
      if (!double.TryParse(s, NumberStyles.Float, (IFormatProvider) culture, out result))
        return DependencyProperty.UnsetValue;
      else
        return (object) result;
    }
  }
}
