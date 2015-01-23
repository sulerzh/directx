using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class MultiBooleanAnyConverter : IMultiValueConverter
  {
    public Visibility NotVisible { get; set; }

    public MultiBooleanAnyConverter()
    {
      this.NotVisible = Visibility.Collapsed;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return (object) (Visibility) (Enumerable.Any<bool>(Enumerable.OfType<bool>((IEnumerable) values), (Func<bool, bool>) (x => x)) ? 0 : (int) this.NotVisible);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
