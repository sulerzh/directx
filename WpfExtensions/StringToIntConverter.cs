using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class StringToIntConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (object) int.Parse((string) parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (object) ((int) parameter).ToString();
    }
  }
}
