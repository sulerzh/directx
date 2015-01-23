using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class GeoFullAddressField : GeoField
  {
    public TableColumn FullAddress { get; private set; }

    public TableColumn OtherLocationDescription { get; private set; }

    internal GeoFullAddressField(GeoFullAddressField.SerializableGeoFullAddressField state)
    {
      this.Unwrap((TableField.SerializableTableField) state);
    }

    public GeoFullAddressField(string name, TableColumn fullAddress = null, TableColumn otherLocationDescription = null, TableColumn addressLine = null, TableColumn locality = null, TableColumn adminDistrict2 = null, TableColumn adminDistrict = null, TableColumn postalCode = null, TableColumn country = null, TableColumn latitude = null, TableColumn longitude = null, TableColumn xCoord = null, TableColumn yCoord = null, TableColumn fullAddressNotUsedForGeocoding = null, TableColumn otherLocationDescriptionNotUsedForGeocoding = null, bool visible = true)
      : base(name, visible)
    {
      if (fullAddress == null && otherLocationDescription == null)
        throw new ArgumentException("fullAddress and otherLocationDescription are both null");
      if (fullAddress != null && otherLocationDescription != null)
        throw new ArgumentException("fullAddress and otherLocationDescription are both not null");
      if (fullAddress != null && fullAddressNotUsedForGeocoding != null)
        throw new ArgumentException("fullAddress and fullAddressNotUsedForGeocoding are both not null");
      if (otherLocationDescription != null && otherLocationDescriptionNotUsedForGeocoding != null)
        throw new ArgumentException("otherLocationDescription and otherLocationDescriptionNotUsedForGeocoding are both not null");
      if (fullAddress != null)
      {
        this.AddGeoColumn(fullAddress, false, false);
        this.FullAddress = fullAddress;
      }
      if (otherLocationDescription != null)
      {
        this.AddGeoColumn(otherLocationDescription, false, false);
        this.OtherLocationDescription = otherLocationDescription;
      }
      this.LatitudeNotUsedForGeocoding = latitude;
      this.LongitudeNotUsedForGeocoding = longitude;
      this.XCoordNotUsedForGeocoding = xCoord;
      this.YCoordNotUsedForGeocoding = yCoord;
      this.AddressLineNotUsedForGeocoding = addressLine;
      this.LocalityNotUsedForGeocoding = locality;
      this.AdminDistrict2NotUsedForGeocoding = adminDistrict2;
      this.AdminDistrictNotUsedForGeocoding = adminDistrict;
      this.PostalCodeNotUsedForGeocoding = postalCode;
      this.CountryNotUsedForGeocoding = country;
      this.FullAddressNotUsedForGeocoding = fullAddressNotUsedForGeocoding;
      this.OtherLocationDescriptionNotUsedForGeocoding = otherLocationDescriptionNotUsedForGeocoding;
    }

    public override bool QuerySubstitutable(GeoField otherField)
    {
      if (!base.QuerySubstitutable(otherField))
        return false;
      GeoFullAddressField fullAddressField = (GeoFullAddressField) otherField;
      return !(this.FullAddress == null ^ fullAddressField.FullAddress == null) && !(this.OtherLocationDescription == null ^ fullAddressField.OtherLocationDescription == null);
    }

    protected override void ReplaceColumns(ModelMetadata modelMetadata)
    {
      base.ReplaceColumns(modelMetadata);
      this.FullAddress = modelMetadata.FindVisibleTableColumnInModelMetadata(this.FullAddress);
      this.FullAddressNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.FullAddressNotUsedForGeocoding);
      this.OtherLocationDescription = modelMetadata.FindVisibleTableColumnInModelMetadata(this.OtherLocationDescription);
      this.OtherLocationDescriptionNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.OtherLocationDescriptionNotUsedForGeocoding);
      if (this.FullAddress != null && !this.GeoColumns.Contains(this.FullAddress))
      {
        this.FullAddressNotUsedForGeocoding = this.FullAddress;
        this.FullAddress = (TableColumn) null;
      }
      if (this.OtherLocationDescription == null || this.GeoColumns.Contains(this.OtherLocationDescription))
        return;
      this.OtherLocationDescriptionNotUsedForGeocoding = this.OtherLocationDescription;
      this.OtherLocationDescription = (TableColumn) null;
    }

    internal override TableField.SerializableTableField Wrap()
    {
      GeoFullAddressField.SerializableGeoFullAddressField fullAddressField = new GeoFullAddressField.SerializableGeoFullAddressField()
      {
        FullAddress = this.FullAddress == null ? (TableColumn.SerializableTableColumn) null : this.FullAddress.Wrap() as TableColumn.SerializableTableColumn,
        OtherLocationDescription = this.OtherLocationDescription == null ? (TableColumn.SerializableTableColumn) null : this.OtherLocationDescription.Wrap() as TableColumn.SerializableTableColumn
      };
      base.Wrap((TableField.SerializableTableField) fullAddressField);
      return (TableField.SerializableTableField) fullAddressField;
    }

    internal override void Unwrap(TableField.SerializableTableField wrappedState)
    {
      if (wrappedState == null)
        throw new ArgumentNullException("wrappedState");
      GeoFullAddressField.SerializableGeoFullAddressField fullAddressField = wrappedState as GeoFullAddressField.SerializableGeoFullAddressField;
      if (fullAddressField == null)
        throw new ArgumentException("wrappedState must be of type SerializableGeoEntityField");
      base.Unwrap((TableField.SerializableTableField) fullAddressField);
      this.FullAddress = fullAddressField.FullAddress == null ? (TableColumn) null : fullAddressField.FullAddress.Unwrap() as TableColumn;
      this.OtherLocationDescription = fullAddressField.OtherLocationDescription == null ? (TableColumn) null : fullAddressField.OtherLocationDescription.Unwrap() as TableColumn;
    }

    [Serializable]
    public class SerializableGeoFullAddressField : GeoField.SerializableGeoField
    {
      public TableColumn.SerializableTableColumn FullAddress;
      public TableColumn.SerializableTableColumn OtherLocationDescription;

      internal override TableField Unwrap()
      {
        return (TableField) new GeoFullAddressField(this);
      }
    }
  }
}
