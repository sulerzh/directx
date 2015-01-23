using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class EnumToStringConverter : IValueConverter
  {
    private static Dictionary<Type, EnumStringConversionTable> _conversionTables = new Dictionary<Type, EnumStringConversionTable>();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      Type type = value.GetType();
      if (!EnumToStringConverter._conversionTables.ContainsKey(type))
        EnumToStringConverter._conversionTables.Add(type, new EnumStringConversionTable(type));
      return (object) EnumToStringConverter._conversionTables[type].GetDisplayString(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }
}
