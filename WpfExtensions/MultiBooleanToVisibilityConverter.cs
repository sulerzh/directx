using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class MultiBooleanToVisibilityConverter : IMultiValueConverter
  {
    public Visibility IfFalse { get; set; }

    public Visibility IfTrue { get; set; }

    public MultiBooleanToVisibilityConverter()
    {
      this.IfFalse = Visibility.Collapsed;
      this.IfTrue = Visibility.Visible;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return (object) (Visibility) (Enumerable.All<bool>(Enumerable.OfType<bool>((IEnumerable) values), (Func<bool, bool>) (x => x)) ? (int) this.IfTrue : (int) this.IfFalse);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
