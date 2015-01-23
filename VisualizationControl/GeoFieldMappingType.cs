using Microsoft.Data.Visualization.WpfExtensions;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public enum GeoFieldMappingType
  {
    [DisplayString(typeof (Resources), "GeoFieldMappingType_None")] None,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_Latitude")] Latitude,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_Longitude")] Longitude,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_Address")] Address,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_Other")] Other,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_Street")] Street,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_City")] City,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_County")] County,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_State")] State,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_Zip")] Zip,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_Country")] Country,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_XCoord")] XCoord,
    [DisplayString(typeof (Resources), "GeoFieldMappingType_YCoord")] YCoord,
  }
}
