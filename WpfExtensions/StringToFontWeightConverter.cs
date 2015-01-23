using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class StringToFontWeightConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string str = value as string;
      if (string.IsNullOrEmpty(str))
        return (object) FontWeights.Normal;
      if (str == FontWeights.Black.ToString())
        return (object) FontWeights.Black;
      if (str == FontWeights.Bold.ToString())
        return (object) FontWeights.Bold;
      if (str == FontWeights.DemiBold.ToString())
        return (object) FontWeights.DemiBold;
      if (str == FontWeights.ExtraBlack.ToString())
        return (object) FontWeights.ExtraBlack;
      if (str == FontWeights.ExtraBold.ToString())
        return (object) FontWeights.ExtraBold;
      if (str == FontWeights.ExtraLight.ToString())
        return (object) FontWeights.ExtraLight;
      if (str == FontWeights.Heavy.ToString())
        return (object) FontWeights.Heavy;
      if (str == FontWeights.Light.ToString())
        return (object) FontWeights.Light;
      if (str == FontWeights.Medium.ToString())
        return (object) FontWeights.Medium;
      if (str == FontWeights.Normal.ToString())
        return (object) FontWeights.Normal;
      if (str == FontWeights.Regular.ToString())
        return (object) FontWeights.Regular;
      if (str == FontWeights.SemiBold.ToString())
        return (object) FontWeights.SemiBold;
      if (str == FontWeights.Thin.ToString())
        return (object) FontWeights.Thin;
      if (str == FontWeights.UltraBlack.ToString())
        return (object) FontWeights.UltraBlack;
      if (str == FontWeights.UltraBold.ToString())
        return (object) FontWeights.UltraBold;
      if (str == FontWeights.UltraLight.ToString())
        return (object) FontWeights.UltraLight;
      else
        return (object) FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        return (object) ((FontWeight) value).ToString();
      }
      catch
      {
        return DependencyProperty.UnsetValue;
      }
    }
  }
}
