using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class SceneTitleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
        return DependencyProperty.UnsetValue;
      string str1 = (string) values[0];
      string str2 = (string) values[1];
      if (str2 != null)
        return (object) str2;
      return (object) string.Format((IFormatProvider) CultureInfo.CurrentUICulture, Resources.SceneSettings_SceneTitle, new object[1]
      {
        (object) str1
      });
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
