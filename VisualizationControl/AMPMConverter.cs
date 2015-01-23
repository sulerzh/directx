using Microsoft.Data.Visualization.WpfExtensions;
using System.Globalization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class AMPMConverter : SimpleOneWayConverter<AMPM, string>
  {
    protected override bool TryConvert(AMPM source, out string result)
    {
      switch (source)
      {
        case AMPM.AM:
          result = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
          return true;
        case AMPM.PM:
          result = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;
          return true;
        default:
          result = (string) null;
          return false;
      }
    }
  }
}
