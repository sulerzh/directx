using System.Collections.Generic;
using System.Text;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class GeoAmbiguity
  {
    public string AmbiguityKey { get; private set; }

    public GeoField GeoField { get; private set; }

    public string AddressLine { get; private set; }

    public string Locality { get; private set; }

    public string AdminDistrict2 { get; private set; }

    public string AdminDistrict { get; private set; }

    public string PostalCode { get; private set; }

    public string Country { get; private set; }

    public string AddressOrOther { get; set; }

    public GeoAmbiguity.Resolution ResolutionType { get; internal set; }

    public int ResolutionIndex { get; internal set; }

    public List<GeoResolution> GeoResolutions { get; private set; }

    public GeoResolution SelectedGeoResolution
    {
      get
      {
        if (this.ResolutionIndex < 0 || this.ResolutionIndex >= this.GeoResolutions.Count)
          return (GeoResolution) null;
        else
          return this.GeoResolutions[this.ResolutionIndex];
      }
    }

    public string SmallestGeoValue { get; private set; }

    public GeoEntityField.GeoEntityLevel SmallestGeoValueIndex { get; private set; }

    public GeoAmbiguity(string ambiguityKey, GeoField geoField, string addressLine, string locality, string adminDistrict2, string adminDistrict, string postalCode, string country, string addressOrOtherValue, GeoAmbiguity.Resolution resolutionType, int smartPickIndex, List<GeoResolution> ambiguities)
    {
      this.AmbiguityKey = ambiguityKey;
      this.GeoField = geoField;
      this.AddressLine = addressLine;
      if (this.SmallestGeoValue == null)
      {
        this.SmallestGeoValue = this.AddressLine;
        this.SmallestGeoValueIndex = GeoEntityField.GeoEntityLevel.AddressLine;
      }
      this.Locality = locality;
      if (this.SmallestGeoValue == null)
      {
        this.SmallestGeoValue = this.Locality;
        this.SmallestGeoValueIndex = GeoEntityField.GeoEntityLevel.Locality;
      }
      this.AdminDistrict2 = adminDistrict2;
      if (this.SmallestGeoValue == null)
      {
        this.SmallestGeoValue = this.AdminDistrict2;
        this.SmallestGeoValueIndex = GeoEntityField.GeoEntityLevel.AdminDistrict2;
      }
      this.AdminDistrict = adminDistrict;
      if (this.SmallestGeoValue == null)
      {
        this.SmallestGeoValue = this.AdminDistrict;
        this.SmallestGeoValueIndex = GeoEntityField.GeoEntityLevel.AdminDistrict;
      }
      this.PostalCode = postalCode;
      if (this.SmallestGeoValue == null)
      {
        this.SmallestGeoValue = this.PostalCode;
        this.SmallestGeoValueIndex = GeoEntityField.GeoEntityLevel.PostalCode;
      }
      this.Country = country;
      if (this.SmallestGeoValue == null)
      {
        this.SmallestGeoValue = this.Country;
        this.SmallestGeoValueIndex = GeoEntityField.GeoEntityLevel.Country;
      }
      this.AddressOrOther = addressOrOtherValue;
      this.ResolutionType = resolutionType;
      this.ResolutionIndex = smartPickIndex;
      this.GeoResolutions = ambiguities;
    }

    internal void LogString(StringBuilder sb, string separator)
    {
      if (sb == null)
        return;
      if (this.AddressLine != null)
        sb.AppendFormat("AddressLine:{0}{1}{0}", (object) separator, (object) this.AddressLine);
      if (this.Locality != null)
        sb.AppendFormat("Locality:{0}{1}{0}", (object) separator, (object) this.Locality);
      if (this.AdminDistrict2 != null)
        sb.AppendFormat("AdminDistrict2:{0}{1}{0}", (object) separator, (object) this.AdminDistrict2);
      if (this.AdminDistrict != null)
        sb.AppendFormat("AdminDistrict:{0}{1}{0}", (object) separator, (object) this.AdminDistrict);
      if (this.PostalCode != null)
        sb.AppendFormat("PostalCode:{0}{1}{0}", (object) separator, (object) this.PostalCode);
      if (this.Country != null)
        sb.AppendFormat("Country:{0}{1}{0}", (object) separator, (object) this.Country);
      if (this.AddressOrOther != null)
        sb.AppendFormat("AddressOrOther:{0}{1}{0}", (object) separator, (object) this.AddressOrOther);
      sb.AppendFormat("ResolutionType:{0}{1}{0}", (object) separator, (object) ((object) this.ResolutionType).ToString());
      sb.AppendFormat("ResolutionIndex:{0}{1}{0}", (object) separator, (object) this.ResolutionIndex);
      GeoResolution selectedGeoResolution = this.SelectedGeoResolution;
      if (selectedGeoResolution == null)
        return;
      selectedGeoResolution.LogString(sb, separator);
    }

    public enum Resolution
    {
      NoMatch = 0,
      NoRegionPolygon = 10,
      Deferred = 20,
      TopCountCountryFirstValue = 30,
      FirstListed = 31,
      SingleMatchMediumConf = 40,
      EntityTypeMatchMediumConf = 45,
      EntityTypeAndValueMatchMediumConf = 48,
      TopCountCountryClosestFirstMatch = 50,
      TopCountCountryClosestMatch = 60,
      EntityTypeMatch = 61,
      TopCountCountrySingleMatch = 70,
      EntityTypeAndValueMatch = 71,
      SingleMatchHighConf = 80,
    }
  }
}
