using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class DoubleToPercentageConverter : IValueConverter
  {
    public double MinimumValue { get; set; }

    public double MaximumValue { get; set; }

    public bool IgorePercentSign { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string str = string.Format("{0:#0%}", (object) (double) value);
      if (this.IgorePercentSign)
        str = str.Replace(NumberFormatInfo.CurrentInfo.PercentSymbol, "");
      return (object) str;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string s = (string) value;
      if (s.Contains(NumberFormatInfo.CurrentInfo.PercentSymbol))
        s = s.Replace(NumberFormatInfo.CurrentInfo.PercentSymbol, "");
      double result;
      if (!double.TryParse(s, out result))
        return DependencyProperty.UnsetValue;
      double num = result / 100.0;
      if (num > this.MaximumValue)
        return (object) this.MaximumValue;
      if (num < this.MinimumValue)
        return (object) this.MinimumValue;
      else
        return (object) num;
    }
  }
}
