using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public enum TableMemberClassification
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
    PostalCode,
    Country,
    XCoord,
    YCoord,
  }
}
