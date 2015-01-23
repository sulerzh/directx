namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class TableMemberClassificationUtil
  {
    public static GeoFieldMappingType GeoFieldType(this TableMemberClassification category)
    {
      switch (category)
      {
        case TableMemberClassification.None:
          return GeoFieldMappingType.None;
        case TableMemberClassification.Latitude:
          return GeoFieldMappingType.Latitude;
        case TableMemberClassification.Longitude:
          return GeoFieldMappingType.Longitude;
        case TableMemberClassification.Address:
          return GeoFieldMappingType.Address;
        case TableMemberClassification.Street:
          return GeoFieldMappingType.Street;
        case TableMemberClassification.City:
          return GeoFieldMappingType.City;
        case TableMemberClassification.County:
          return GeoFieldMappingType.County;
        case TableMemberClassification.State:
          return GeoFieldMappingType.State;
        case TableMemberClassification.PostalCode:
          return GeoFieldMappingType.Zip;
        case TableMemberClassification.Country:
          return GeoFieldMappingType.Country;
        case TableMemberClassification.XCoord:
          return GeoFieldMappingType.XCoord;
        case TableMemberClassification.YCoord:
          return GeoFieldMappingType.YCoord;
        default:
          return GeoFieldMappingType.Other;
      }
    }
  }
}
