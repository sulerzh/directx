using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public enum TimeChunkPeriod
  {
    [DisplayString(typeof (Resources), "TimeChunking_None")] None,
    [DisplayString(typeof (Resources), "TimeChunking_Day")] Day,
    [DisplayString(typeof (Resources), "TimeChunking_Month")] Month,
    [DisplayString(typeof (Resources), "TimeChunking_Quarter")] Quarter,
    [DisplayString(typeof (Resources), "TimeChunking_Year")] Year,
  }
}
