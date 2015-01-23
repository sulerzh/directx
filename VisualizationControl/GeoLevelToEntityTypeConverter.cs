using Microsoft.Data.Visualization.Engine;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class GeoLevelToEntityTypeConverter
  {
    public static EntityType? GetEntityType(this GeoEntityField.GeoEntityLevel geoLevel)
    {
      switch (geoLevel)
      {
        case GeoEntityField.GeoEntityLevel.Locality:
          return new EntityType?(EntityType.PopulatedPlace);
        case GeoEntityField.GeoEntityLevel.AdminDistrict2:
          return new EntityType?(EntityType.AdminDivision2);
        case GeoEntityField.GeoEntityLevel.AdminDistrict:
          return new EntityType?(EntityType.AdminDivision1);
        case GeoEntityField.GeoEntityLevel.PostalCode:
          return new EntityType?(EntityType.Postcode1);
        case GeoEntityField.GeoEntityLevel.Country:
          return new EntityType?(EntityType.CountryRegion);
        default:
          return new EntityType?();
      }
    }
  }
}
