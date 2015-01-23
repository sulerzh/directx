using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class SceneTimeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      object obj = value;
      if (!(obj is TimeSpan))
        return (object) string.Empty;
      return (object) string.Format((IFormatProvider) CultureInfo.CurrentUICulture, Resources.SceneSettings_SceneWithTime, new object[1]
      {
        (object) ((TimeSpan) obj).TotalSeconds
      });
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value;
    }
  }
}
