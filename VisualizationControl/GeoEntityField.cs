using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class GeoEntityField : GeoField
  {
    public const GeoEntityField.GeoEntityLevel FirstGeoLevel = GeoEntityField.GeoEntityLevel.AddressLine;
    public const GeoEntityField.GeoEntityLevel LastGeoLevel = GeoEntityField.GeoEntityLevel.Country;
    public const int NumGeoLevels = 6;

    public bool HasOnlyLocality
    {
      get
      {
        if (this.AddressLine == null && this.Locality != null && (this.AdminDistrict2 == null && this.AdminDistrict == null) && this.PostalCode == null)
          return this.Country == null;
        else
          return false;
      }
    }

    public TableColumn AddressLine { get; private set; }

    public TableColumn Locality { get; private set; }

    public TableColumn AdminDistrict2 { get; private set; }

    public TableColumn AdminDistrict { get; private set; }

    public TableColumn PostalCode { get; private set; }

    public TableColumn Country { get; private set; }

    internal GeoEntityField(GeoEntityField.SerializableGeoEntityField state)
    {
      this.Unwrap((TableField.SerializableTableField) state);
    }

    public GeoEntityField(string name)
      : base(name, false)
    {
    }

    public GeoEntityField(string name, GeoEntityField.GeoEntityLevel mapBy, TableColumn addressLine = null, TableColumn locality = null, TableColumn adminDistrict2 = null, TableColumn adminDistrict = null, TableColumn postalCode = null, TableColumn country = null, TableColumn latitude = null, TableColumn longitude = null, TableColumn xCoord = null, TableColumn yCoord = null, TableColumn fullAddress = null, TableColumn otherLocationDescription = null, bool visible = true)
      : base(name, visible)
    {
      if (addressLine != null && mapBy == GeoEntityField.GeoEntityLevel.AddressLine)
      {
        this.AddGeoColumn(addressLine, false, false);
        this.AddressLine = addressLine;
      }
      else
        this.AddressLineNotUsedForGeocoding = addressLine;
      if (locality != null && (mapBy == GeoEntityField.GeoEntityLevel.AddressLine || mapBy == GeoEntityField.GeoEntityLevel.Locality))
      {
        this.AddGeoColumn(locality, false, false);
        this.Locality = locality;
      }
      else
        this.LocalityNotUsedForGeocoding = locality;
      if (adminDistrict2 != null && mapBy == GeoEntityField.GeoEntityLevel.AdminDistrict2)
      {
        this.AddGeoColumn(adminDistrict2, false, false);
        this.AdminDistrict2 = adminDistrict2;
      }
      else
        this.AdminDistrict2NotUsedForGeocoding = adminDistrict2;
      if (adminDistrict != null && (mapBy == GeoEntityField.GeoEntityLevel.AddressLine || mapBy == GeoEntityField.GeoEntityLevel.Locality || (mapBy == GeoEntityField.GeoEntityLevel.AdminDistrict2 || mapBy == GeoEntityField.GeoEntityLevel.AdminDistrict)))
      {
        this.AddGeoColumn(adminDistrict, false, false);
        this.AdminDistrict = adminDistrict;
      }
      else
        this.AdminDistrictNotUsedForGeocoding = adminDistrict;
      if (postalCode != null && (mapBy == GeoEntityField.GeoEntityLevel.AddressLine || mapBy == GeoEntityField.GeoEntityLevel.PostalCode))
      {
        this.AddGeoColumn(postalCode, false, false);
        this.PostalCode = postalCode;
      }
      else
        this.PostalCodeNotUsedForGeocoding = postalCode;
      if (country != null)
      {
        this.AddGeoColumn(country, false, false);
        this.Country = country;
      }
      else
        this.CountryNotUsedForGeocoding = country;
      this.LatitudeNotUsedForGeocoding = latitude;
      this.LongitudeNotUsedForGeocoding = longitude;
      this.XCoordNotUsedForGeocoding = xCoord;
      this.YCoordNotUsedForGeocoding = yCoord;
      this.FullAddressNotUsedForGeocoding = fullAddress;
      this.OtherLocationDescriptionNotUsedForGeocoding = otherLocationDescription;
      if (this.GeoColumns.Count == 0)
        throw new ArgumentException("None of the geoField columns can be used for the mapBy value that has been provided.");
    }

    public GeoEntityField.GeoEntityLevel GeoLevel(TableColumn column)
    {
      if (column == null)
        throw new ArgumentNullException("column");
      if (column.QuerySubstitutable((TableMember) this.AddressLine))
        return GeoEntityField.GeoEntityLevel.AddressLine;
      if (column.QuerySubstitutable((TableMember) this.Locality))
        return GeoEntityField.GeoEntityLevel.Locality;
      if (column.QuerySubstitutable((TableMember) this.AdminDistrict2))
        return GeoEntityField.GeoEntityLevel.AdminDistrict2;
      if (column.QuerySubstitutable((TableMember) this.AdminDistrict))
        return GeoEntityField.GeoEntityLevel.AdminDistrict;
      if (column.QuerySubstitutable((TableMember) this.PostalCode))
        return GeoEntityField.GeoEntityLevel.PostalCode;
      if (column.QuerySubstitutable((TableMember) this.Country))
        return GeoEntityField.GeoEntityLevel.Country;
      else
        throw new ArgumentException("Column '" + column.Name + "' is not used by this GeoEntityField object");
    }

    public override bool QuerySubstitutable(GeoField otherField)
    {
      if (!base.QuerySubstitutable(otherField))
        return false;
      GeoEntityField geoEntityField = (GeoEntityField) otherField;
      return !(this.AddressLine == null ^ geoEntityField.AddressLine == null) && !(this.Locality == null ^ geoEntityField.Locality == null) && (!(this.AdminDistrict2 == null ^ geoEntityField.AdminDistrict2 == null) && !(this.AdminDistrict == null ^ geoEntityField.AdminDistrict == null)) && (!(this.PostalCode == null ^ geoEntityField.PostalCode == null) && !(this.Country == null ^ geoEntityField.Country == null));
    }

    protected override void ReplaceColumns(ModelMetadata modelMetadata)
    {
      base.ReplaceColumns(modelMetadata);
      this.AddressLine = modelMetadata.FindVisibleTableColumnInModelMetadata(this.AddressLine);
      this.Locality = modelMetadata.FindVisibleTableColumnInModelMetadata(this.Locality);
      this.AdminDistrict2 = modelMetadata.FindVisibleTableColumnInModelMetadata(this.AdminDistrict2);
      this.AdminDistrict = modelMetadata.FindVisibleTableColumnInModelMetadata(this.AdminDistrict);
      this.PostalCode = modelMetadata.FindVisibleTableColumnInModelMetadata(this.PostalCode);
      this.Country = modelMetadata.FindVisibleTableColumnInModelMetadata(this.Country);
      this.AddressLineNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.AddressLineNotUsedForGeocoding);
      this.LocalityNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.LocalityNotUsedForGeocoding);
      this.AdminDistrict2NotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.AdminDistrict2NotUsedForGeocoding);
      this.AdminDistrictNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.AdminDistrictNotUsedForGeocoding);
      this.PostalCodeNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.PostalCodeNotUsedForGeocoding);
      this.CountryNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.CountryNotUsedForGeocoding);
      if (this.AddressLine != null && !this.GeoColumns.Contains(this.AddressLine))
      {
        this.AddressLineNotUsedForGeocoding = this.AddressLine;
        this.AddressLine = (TableColumn) null;
      }
      if (this.Locality != null && !this.GeoColumns.Contains(this.Locality))
      {
        this.LocalityNotUsedForGeocoding = this.Locality;
        this.Locality = (TableColumn) null;
      }
      if (this.AdminDistrict2 != null && !this.GeoColumns.Contains(this.AdminDistrict2))
      {
        this.AdminDistrict2NotUsedForGeocoding = this.AdminDistrict2;
        this.AdminDistrict2 = (TableColumn) null;
      }
      if (this.AdminDistrict != null && !this.GeoColumns.Contains(this.AdminDistrict))
      {
        this.AdminDistrictNotUsedForGeocoding = this.AdminDistrict;
        this.AdminDistrict = (TableColumn) null;
      }
      if (this.PostalCode != null && !this.GeoColumns.Contains(this.PostalCode))
      {
        this.PostalCodeNotUsedForGeocoding = this.PostalCode;
        this.PostalCode = (TableColumn) null;
      }
      if (this.Country == null || this.GeoColumns.Contains(this.Country))
        return;
      this.CountryNotUsedForGeocoding = this.Country;
      this.Country = (TableColumn) null;
    }

    internal override TableField.SerializableTableField Wrap()
    {
      GeoEntityField.SerializableGeoEntityField serializableGeoEntityField = new GeoEntityField.SerializableGeoEntityField()
      {
        AddressLine = this.AddressLine == null ? (TableColumn.SerializableTableColumn) null : this.AddressLine.Wrap() as TableColumn.SerializableTableColumn,
        Locality = this.Locality == null ? (TableColumn.SerializableTableColumn) null : this.Locality.Wrap() as TableColumn.SerializableTableColumn,
        AdminDistrict2 = this.AdminDistrict2 == null ? (TableColumn.SerializableTableColumn) null : this.AdminDistrict2.Wrap() as TableColumn.SerializableTableColumn,
        AdminDistrict = this.AdminDistrict == null ? (TableColumn.SerializableTableColumn) null : this.AdminDistrict.Wrap() as TableColumn.SerializableTableColumn,
        PostalCode = this.PostalCode == null ? (TableColumn.SerializableTableColumn) null : this.PostalCode.Wrap() as TableColumn.SerializableTableColumn,
        Country = this.Country == null ? (TableColumn.SerializableTableColumn) null : this.Country.Wrap() as TableColumn.SerializableTableColumn
      };
      base.Wrap((TableField.SerializableTableField) serializableGeoEntityField);
      return (TableField.SerializableTableField) serializableGeoEntityField;
    }

    internal override void Unwrap(TableField.SerializableTableField wrappedState)
    {
      if (wrappedState == null)
        throw new ArgumentNullException("wrappedState");
      GeoEntityField.SerializableGeoEntityField serializableGeoEntityField = wrappedState as GeoEntityField.SerializableGeoEntityField;
      if (serializableGeoEntityField == null)
        throw new ArgumentException("wrappedState must be of type SerializableGeoEntityField");
      base.Unwrap((TableField.SerializableTableField) serializableGeoEntityField);
      this.AddressLine = serializableGeoEntityField.AddressLine == null ? (TableColumn) null : serializableGeoEntityField.AddressLine.Unwrap() as TableColumn;
      this.Locality = serializableGeoEntityField.Locality == null ? (TableColumn) null : serializableGeoEntityField.Locality.Unwrap() as TableColumn;
      this.AdminDistrict2 = serializableGeoEntityField.AdminDistrict2 == null ? (TableColumn) null : serializableGeoEntityField.AdminDistrict2.Unwrap() as TableColumn;
      this.AdminDistrict = serializableGeoEntityField.AdminDistrict == null ? (TableColumn) null : serializableGeoEntityField.AdminDistrict.Unwrap() as TableColumn;
      this.PostalCode = serializableGeoEntityField.PostalCode == null ? (TableColumn) null : serializableGeoEntityField.PostalCode.Unwrap() as TableColumn;
      this.Country = serializableGeoEntityField.Country == null ? (TableColumn) null : serializableGeoEntityField.Country.Unwrap() as TableColumn;
    }

    [Serializable]
    public enum GeoEntityLevel
    {
      AddressLine,
      Locality,
      AdminDistrict2,
      AdminDistrict,
      PostalCode,
      Country,
    }

    [Serializable]
    public class SerializableGeoEntityField : GeoField.SerializableGeoField
    {
      public TableColumn.SerializableTableColumn AddressLine;
      public TableColumn.SerializableTableColumn Locality;
      public TableColumn.SerializableTableColumn AdminDistrict2;
      public TableColumn.SerializableTableColumn AdminDistrict;
      public TableColumn.SerializableTableColumn PostalCode;
      public TableColumn.SerializableTableColumn Country;

      internal override TableField Unwrap()
      {
        return (TableField) new GeoEntityField(this);
      }
    }
  }
}
