using Microsoft.Data.Visualization.Engine;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class GeospatialDataProviders
  {
    public IRegionProvider RegionProvider { get; set; }

    public ILatLonProvider LatLonProvider { get; set; }
  }
}
