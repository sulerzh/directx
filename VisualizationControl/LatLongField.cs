using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class LatLongField : GeoField
  {
    public bool IsXYCoords { get; private set; }

    public override bool IsUsingXY
    {
      get
      {
        return this.IsXYCoords;
      }
    }

    public LatLongField(string name, bool isActuallyXY, TableColumn lat, TableColumn lon, TableColumn xCoord, TableColumn yCoord, TableColumn addressLine = null, TableColumn locality = null, TableColumn adminDistrict2 = null, TableColumn adminDistrict = null, TableColumn postalCode = null, TableColumn country = null, TableColumn fullAddress = null, TableColumn otherLocationDescription = null, bool visible = true)
      : base(name, visible)
    {
      this.IsXYCoords = isActuallyXY;
      if (!isActuallyXY)
      {
        if (lat == null)
          throw new ArgumentNullException("lat");
        if (lon == null)
          throw new ArgumentNullException("lon");
        this.AddGeoColumn(lat, true, false);
        this.AddGeoColumn(lon, false, true);
        this.XCoordNotUsedForGeocoding = xCoord;
        this.YCoordNotUsedForGeocoding = yCoord;
      }
      else
      {
        if (xCoord == null)
          throw new ArgumentNullException("xCoord");
        if (yCoord == null)
          throw new ArgumentNullException("yCoord");
        this.AddGeoColumn(yCoord, true, false);
        this.AddGeoColumn(xCoord, false, true);
        this.LatitudeNotUsedForGeocoding = lat;
        this.LongitudeNotUsedForGeocoding = lon;
      }
      this.AddressLineNotUsedForGeocoding = addressLine;
      this.LocalityNotUsedForGeocoding = locality;
      this.AdminDistrict2NotUsedForGeocoding = adminDistrict2;
      this.AdminDistrictNotUsedForGeocoding = adminDistrict;
      this.PostalCodeNotUsedForGeocoding = postalCode;
      this.CountryNotUsedForGeocoding = country;
      this.FullAddressNotUsedForGeocoding = fullAddress;
      this.OtherLocationDescriptionNotUsedForGeocoding = otherLocationDescription;
    }

    internal LatLongField(LatLongField.SerializableLatLongField state)
    {
      this.Unwrap((TableField.SerializableTableField) state);
    }

    public LatLongField(XYField.SerializableXYField state)
    {
      this.Unwrap((TableField.SerializableTableField) state);
    }

    internal override TableField.SerializableTableField Wrap()
    {
      if (this.IsXYCoords)
      {
        XYField.SerializableXYField serializableXyField = new XYField.SerializableXYField();
        base.Wrap((TableField.SerializableTableField) serializableXyField);
        serializableXyField.XCoord = this.Longitude == null ? (TableColumn.SerializableTableColumn) null : this.Longitude.Wrap() as TableColumn.SerializableTableColumn;
        serializableXyField.YCoord = this.Latitude == null ? (TableColumn.SerializableTableColumn) null : this.Latitude.Wrap() as TableColumn.SerializableTableColumn;
        return (TableField.SerializableTableField) serializableXyField;
      }
      else
      {
        LatLongField.SerializableLatLongField serializableLatLongField = new LatLongField.SerializableLatLongField();
        base.Wrap((TableField.SerializableTableField) serializableLatLongField);
        serializableLatLongField.Latitude = this.Latitude == null ? (TableColumn.SerializableTableColumn) null : this.Latitude.Wrap() as TableColumn.SerializableTableColumn;
        serializableLatLongField.Longitude = this.Longitude == null ? (TableColumn.SerializableTableColumn) null : this.Longitude.Wrap() as TableColumn.SerializableTableColumn;
        return (TableField.SerializableTableField) serializableLatLongField;
      }
    }

    internal override void Unwrap(TableField.SerializableTableField wrappedState)
    {
      if (wrappedState == null)
        throw new ArgumentNullException("wrappedState");
      LatLongField.SerializableLatLongField serializableLatLongField = wrappedState as LatLongField.SerializableLatLongField;
      XYField.SerializableXYField serializableXyField = wrappedState as XYField.SerializableXYField;
      if (serializableLatLongField == null && serializableXyField == null)
        throw new ArgumentException("wrappedState must be of type SerializableLatLongField or SerializableXYField");
      base.Unwrap(wrappedState);
      if (serializableXyField != null)
      {
        this.IsXYCoords = true;
        this.Latitude = serializableXyField.YCoord == null ? (TableColumn) null : serializableXyField.YCoord.Unwrap() as TableColumn;
        this.Longitude = serializableXyField.XCoord == null ? (TableColumn) null : serializableXyField.XCoord.Unwrap() as TableColumn;
      }
      else
      {
        this.IsXYCoords = serializableLatLongField.IsXYCoords;
        this.Latitude = serializableLatLongField.Latitude == null ? (TableColumn) null : serializableLatLongField.Latitude.Unwrap() as TableColumn;
        this.Longitude = serializableLatLongField.Longitude == null ? (TableColumn) null : serializableLatLongField.Longitude.Unwrap() as TableColumn;
      }
    }

    [Serializable]
    public class SerializableLatLongField : GeoField.SerializableGeoField
    {
      public TableColumn.SerializableTableColumn Latitude;
      public TableColumn.SerializableTableColumn Longitude;

      [XmlElement("IsXYCoords")]
      public bool IsXYCoords { get; set; }

      internal override TableField Unwrap()
      {
        return (TableField) new LatLongField(this);
      }
    }
  }
}
