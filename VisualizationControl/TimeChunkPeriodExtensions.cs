namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class TimeChunkPeriodExtensions
  {
    public static string DisplayString(this TimeChunkPeriod timeChunkPeriod)
    {
      switch (timeChunkPeriod)
      {
        case TimeChunkPeriod.None:
          return Resources.TimeChunking_None;
        case TimeChunkPeriod.Day:
          return Resources.TimeChunking_Day;
        case TimeChunkPeriod.Month:
          return Resources.TimeChunking_Month;
        case TimeChunkPeriod.Quarter:
          return Resources.TimeChunking_Quarter;
        case TimeChunkPeriod.Year:
          return Resources.TimeChunking_Year;
        default:
          return ((object) timeChunkPeriod).ToString();
      }
    }
  }
}
