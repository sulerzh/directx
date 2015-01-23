using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public enum GeoMappingType
  {
    None,
    Latitude,
    Longitude,
    Address,
    Other,
    Street,
    City,
    County,
    State,
    Zip,
    Country,
    XCoord,
    YCoord,
  }
}
