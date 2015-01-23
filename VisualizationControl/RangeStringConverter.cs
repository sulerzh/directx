using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class RangeStringConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (Enumerable.Any<object>((IEnumerable<object>) values, (Func<object, bool>) (val => val == DependencyProperty.UnsetValue)) || Enumerable.Any<object>(Enumerable.Skip<object>((IEnumerable<object>) values, 1), (Func<object, bool>) (val => val == null)))
        return values[0];
      string str = (string) values[0];
      double num1 = (double) values[1];
      double num2 = (double) values[2];
      double num3 = (double) values[3];
      double num4 = (double) values[4];
      bool flag1 = (bool) values[5];
      bool flag2 = (bool) values[6];
      if (num1 == num3 && num2 == num4 && flag2)
        return (object) str;
      double? val1 = RangeFilterViewModel.RoundRangeValue(new double?(num3), new double?(num1), new double?(num2), flag1);
      double? val2 = RangeFilterViewModel.RoundRangeValue(new double?(num4), new double?(num1), new double?(num2), flag1);
      if (num3 == num4)
        return (object) string.Format((IFormatProvider) CultureInfo.CurrentUICulture, Resources.FiltersTab_RangeFilter_ExactString, new object[1]
        {
          (object) RangeStringConverter.GetValue(val1, flag1)
        });
      else
        return (object) string.Format((IFormatProvider) CultureInfo.CurrentUICulture, Resources.FiltersTab_RangeFilter_RangeString, new object[2]
        {
          (object) RangeStringConverter.GetValue(val1, flag1),
          (object) RangeStringConverter.GetValue(val2, flag1)
        });
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    private static string GetValue(double? val, bool isInteger)
    {
      if (!val.HasValue)
        return (string) null;
      string format = Math.Abs(val.Value) <= 0.01 || Math.Abs(val.Value) >= 1E+21 ? (string) null : (isInteger ? "N0" : "N");
      return val.Value.ToString(format, (IFormatProvider) CultureInfo.CurrentUICulture);
    }
  }
}
