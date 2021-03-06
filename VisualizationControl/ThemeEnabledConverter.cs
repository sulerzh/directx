﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class ThemeEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //bool isAerial = (bool)values[0];
            //bool canUseRoad = (bool)values[1];
            //bool canUseAerial = (bool)values[1];

            //return isAerial || !canUseRoad ?
            //    (!isAerial ? 0 : (canUseAerial ? 1 : 0)) :
            //    1;
            return ((bool)values[0] || !(bool)values[1] ? (!(bool)values[0] ? 0 : ((bool)values[2] ? 1 : 0)) : 1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
