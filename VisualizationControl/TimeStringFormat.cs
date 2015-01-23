using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TimeStringFormat
  {
    public string Sample { get; set; }

    public string Format { get; set; }

    public TimeStringFormat(DateTime dateTime, string format)
    {
      this.Format = format;
      this.Sample = dateTime.ToString(format);
    }
  }
}
