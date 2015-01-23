using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class IfThenElseConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if ((bool) values[0])
        return values[1];
      else
        return values[2];
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
