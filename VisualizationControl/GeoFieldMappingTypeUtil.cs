using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class GeoFieldMappingTypeUtil
  {
    public static int[] OrderOfMappings = new int[13]
    {
      0,
      1,
      2,
      3,
      4,
      5,
      6,
      8,
      9,
      7,
      10,
      11,
      12
    };
    private static EnumStringConversionTable displayStringsTable = new EnumStringConversionTable(typeof (GeoFieldMappingType));
    public const int NumGeoFieldMappingTypes = 13;

    public static GeoMappingType ToGeoMappingType(this GeoFieldMappingType mappingType)
    {
      return (GeoMappingType) mappingType;
    }

    public static GeoFieldMappingType FromGeoMappingType(this GeoMappingType geoMappingType)
    {
      return (GeoFieldMappingType) geoMappingType;
    }

    public static int MappingOrder(this GeoFieldMappingType mappingType)
    {
      return GeoFieldMappingTypeUtil.OrderOfMappings[(int) mappingType];
    }

    public static GeoEntityField.GeoEntityLevel GeoEntityLevel(this GeoFieldMappingType mappingType)
    {
      switch (mappingType)
      {
        case GeoFieldMappingType.Street:
          return GeoEntityField.GeoEntityLevel.AddressLine;
        case GeoFieldMappingType.City:
          return GeoEntityField.GeoEntityLevel.Locality;
        case GeoFieldMappingType.County:
          return GeoEntityField.GeoEntityLevel.AdminDistrict2;
        case GeoFieldMappingType.State:
          return GeoEntityField.GeoEntityLevel.AdminDistrict;
        case GeoFieldMappingType.Zip:
          return GeoEntityField.GeoEntityLevel.PostalCode;
        case GeoFieldMappingType.Country:
          return GeoEntityField.GeoEntityLevel.Country;
        default:
          throw new ArgumentException("Cannot map the specified geo field level: " + ((object) mappingType).ToString());
      }
    }

    public static string DisplayString(this GeoFieldMappingType mappingType)
    {
      return GeoFieldMappingTypeUtil.displayStringsTable.GetDisplayString((object) mappingType);
    }

    public static bool SupportsRegions(this GeoFieldMappingType mappingType)
    {
      switch (mappingType)
      {
        case GeoFieldMappingType.County:
        case GeoFieldMappingType.State:
        case GeoFieldMappingType.Zip:
        case GeoFieldMappingType.Country:
          return true;
        default:
          return false;
      }
    }
  }
}
