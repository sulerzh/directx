using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class GeoField : TableField
  {
    public List<TableColumn> GeoColumns { get; private set; }

    public TableColumn Latitude { get; protected set; }

    public TableColumn Longitude { get; protected set; }

    public TableColumn LatitudeNotUsedForGeocoding { get; protected set; }

    public TableColumn LongitudeNotUsedForGeocoding { get; protected set; }

    public TableColumn XCoordNotUsedForGeocoding { get; protected set; }

    public TableColumn YCoordNotUsedForGeocoding { get; protected set; }

    public TableColumn AddressLineNotUsedForGeocoding { get; protected set; }

    public TableColumn LocalityNotUsedForGeocoding { get; protected set; }

    public TableColumn AdminDistrict2NotUsedForGeocoding { get; protected set; }

    public TableColumn AdminDistrictNotUsedForGeocoding { get; protected set; }

    public TableColumn PostalCodeNotUsedForGeocoding { get; protected set; }

    public TableColumn CountryNotUsedForGeocoding { get; protected set; }

    public TableColumn FullAddressNotUsedForGeocoding { get; protected set; }

    public TableColumn OtherLocationDescriptionNotUsedForGeocoding { get; protected set; }

    public bool HasLatLongOrXY
    {
      get
      {
        if (this.Latitude != null)
          return this.Longitude != null;
        else
          return false;
      }
    }

    public virtual bool IsUsingXY
    {
      get
      {
        return false;
      }
    }

    public GeoField(string name, bool visible = true)
      : base(name, visible)
    {
      this.GeoColumns = new List<TableColumn>();
    }

    internal GeoField()
    {
      this.GeoColumns = new List<TableColumn>();
    }

    public void AddGeoColumn(TableColumn column, bool isLat = false, bool isLon = false)
    {
      if (column == null)
        throw new ArgumentNullException("column");
      this.GeoColumns.Add(column);
      if (isLat)
        this.Latitude = column;
      if (!isLon)
        return;
      this.Longitude = column;
    }

    public virtual bool QuerySubstitutable(GeoField other)
    {
      if (other == null || this.GetType() != other.GetType() || this.GeoColumns.Count != other.GeoColumns.Count)
        return false;
      for (int index = 0; index < this.GeoColumns.Count; ++index)
      {
        if (!this.GeoColumns[index].QuerySubstitutable((TableMember) other.GeoColumns[index]))
          return false;
      }
      return true;
    }

    internal void ModelMetadataChanged(ModelMetadata modelMetadata, List<string> tablesWithUpdatedData, ref bool requery, ref bool queryChanged)
    {
      List<TableColumn> list = new List<TableColumn>();
      TableIsland tableIsland = (TableIsland) null;
      bool flag1 = false;
      bool flag2 = false;
      foreach (TableColumn col in this.GeoColumns)
      {
        TableColumn columnInModelMetadata = modelMetadata.FindVisibleTableColumnInModelMetadata(col);
        if (columnInModelMetadata == null)
        {
          flag2 = true;
          break;
        }
        else
        {
          flag1 = flag1 | tablesWithUpdatedData.Contains(columnInModelMetadata.Table.ModelName);
          if (tableIsland == null)
          {
            tableIsland = columnInModelMetadata.Table.Island;
            list.Add(columnInModelMetadata);
          }
          else if (tableIsland == columnInModelMetadata.Table.Island)
          {
            list.Add(columnInModelMetadata);
          }
          else
          {
            flag2 = true;
            break;
          }
        }
      }
      if (flag2 || tableIsland == null)
        flag2 = true;
      this.GeoColumns = !flag2 ? list : new List<TableColumn>();
      this.ReplaceColumns(modelMetadata);
      if (flag2)
        flag1 = true;
      requery |= flag1;
      queryChanged |= flag2;

    }

    protected virtual void ReplaceColumns(ModelMetadata modelMetadata)
    {
      this.Latitude = modelMetadata.FindVisibleTableColumnInModelMetadata(this.Latitude);
      this.Longitude = modelMetadata.FindVisibleTableColumnInModelMetadata(this.Longitude);
      this.LatitudeNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.LatitudeNotUsedForGeocoding);
      this.LongitudeNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.LongitudeNotUsedForGeocoding);
      this.XCoordNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.XCoordNotUsedForGeocoding);
      this.YCoordNotUsedForGeocoding = modelMetadata.FindVisibleTableColumnInModelMetadata(this.YCoordNotUsedForGeocoding);
      if (this.Latitude != null && !this.GeoColumns.Contains(this.Latitude))
      {
        if (this.IsUsingXY)
          this.YCoordNotUsedForGeocoding = this.Latitude;
        else
          this.LatitudeNotUsedForGeocoding = this.Latitude;
        this.Latitude = (TableColumn) null;
      }
      if (this.Longitude == null || this.GeoColumns.Contains(this.Longitude))
        return;
      if (this.IsUsingXY)
        this.XCoordNotUsedForGeocoding = this.Longitude;
      else
        this.LongitudeNotUsedForGeocoding = this.Longitude;
      this.Longitude = (TableColumn) null;
    }

    protected override void Wrap(TableField.SerializableTableField state)
    {
      GeoField.SerializableGeoField serializableGeoField = state as GeoField.SerializableGeoField;
      serializableGeoField.GeoColumns = Enumerable.ToList<TableColumn.SerializableTableColumn>(Enumerable.Select<TableColumn, TableColumn.SerializableTableColumn>((IEnumerable<TableColumn>) this.GeoColumns, (Func<TableColumn, TableColumn.SerializableTableColumn>) (col => col.Wrap() as TableColumn.SerializableTableColumn)));
      serializableGeoField.LatitudeNotUsedForGeocoding = this.LatitudeNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.LatitudeNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.LongitudeNotUsedForGeocoding = this.LongitudeNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.LongitudeNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.XCoordNotUsedForGeocoding = this.XCoordNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.XCoordNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.YCoordNotUsedForGeocoding = this.YCoordNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.YCoordNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.AddressLineNotUsedForGeocoding = this.AddressLineNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.AddressLineNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.LocalityNotUsedForGeocoding = this.LocalityNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.LocalityNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.AdminDistrict2NotUsedForGeocoding = this.AdminDistrict2NotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.AdminDistrict2NotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.AdminDistrictNotUsedForGeocoding = this.AdminDistrictNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.AdminDistrictNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.PostalCodeNotUsedForGeocoding = this.PostalCodeNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.PostalCodeNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.CountryNotUsedForGeocoding = this.CountryNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.CountryNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.FullAddressNotUsedForGeocoding = this.FullAddressNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.FullAddressNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      serializableGeoField.OtherLocationDescriptionNotUsedForGeocoding = this.OtherLocationDescriptionNotUsedForGeocoding == null ? (TableColumn.SerializableTableColumn) null : this.OtherLocationDescriptionNotUsedForGeocoding.Wrap() as TableColumn.SerializableTableColumn;
      base.Wrap((TableField.SerializableTableField) serializableGeoField);
    }

    internal override void Unwrap(TableField.SerializableTableField state)
    {
      GeoField.SerializableGeoField serializableGeoField = state as GeoField.SerializableGeoField;
      if (serializableGeoField.GeoColumns == null)
        throw new ArgumentException("geo.GeoColumns must not be null");
      base.Unwrap((TableField.SerializableTableField) serializableGeoField);
      this.GeoColumns = Enumerable.ToList<TableColumn>(Enumerable.Select<TableColumn.SerializableTableColumn, TableColumn>((IEnumerable<TableColumn.SerializableTableColumn>) serializableGeoField.GeoColumns, (Func<TableColumn.SerializableTableColumn, TableColumn>) (serCol => serCol.Unwrap() as TableColumn)));
      this.LatitudeNotUsedForGeocoding = serializableGeoField.LatitudeNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.LatitudeNotUsedForGeocoding.Unwrap() as TableColumn;
      this.LongitudeNotUsedForGeocoding = serializableGeoField.LongitudeNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.LongitudeNotUsedForGeocoding.Unwrap() as TableColumn;
      this.XCoordNotUsedForGeocoding = serializableGeoField.XCoordNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.XCoordNotUsedForGeocoding.Unwrap() as TableColumn;
      this.YCoordNotUsedForGeocoding = serializableGeoField.YCoordNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.YCoordNotUsedForGeocoding.Unwrap() as TableColumn;
      this.AddressLineNotUsedForGeocoding = serializableGeoField.AddressLineNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.AddressLineNotUsedForGeocoding.Unwrap() as TableColumn;
      this.LocalityNotUsedForGeocoding = serializableGeoField.LocalityNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.LocalityNotUsedForGeocoding.Unwrap() as TableColumn;
      this.AdminDistrict2NotUsedForGeocoding = serializableGeoField.AdminDistrict2NotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.AdminDistrict2NotUsedForGeocoding.Unwrap() as TableColumn;
      this.AdminDistrictNotUsedForGeocoding = serializableGeoField.AdminDistrictNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.AdminDistrictNotUsedForGeocoding.Unwrap() as TableColumn;
      this.PostalCodeNotUsedForGeocoding = serializableGeoField.PostalCodeNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.PostalCodeNotUsedForGeocoding.Unwrap() as TableColumn;
      this.CountryNotUsedForGeocoding = serializableGeoField.CountryNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.CountryNotUsedForGeocoding.Unwrap() as TableColumn;
      this.FullAddressNotUsedForGeocoding = serializableGeoField.FullAddressNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.FullAddressNotUsedForGeocoding.Unwrap() as TableColumn;
      this.OtherLocationDescriptionNotUsedForGeocoding = serializableGeoField.OtherLocationDescriptionNotUsedForGeocoding == null ? (TableColumn) null : serializableGeoField.OtherLocationDescriptionNotUsedForGeocoding.Unwrap() as TableColumn;
    }

    [Serializable]
    public abstract class SerializableGeoField : TableField.SerializableTableField
    {
      [XmlArrayItem("GeoColumn")]
      [XmlArray("GeoColumns")]
      public List<TableColumn.SerializableTableColumn> GeoColumns;

      [XmlElement("OLat")]
      public TableColumn.SerializableTableColumn LatitudeNotUsedForGeocoding { get; set; }

      [XmlElement("OLon")]
      public TableColumn.SerializableTableColumn LongitudeNotUsedForGeocoding { get; set; }

      [XmlElement("OXCd")]
      public TableColumn.SerializableTableColumn XCoordNotUsedForGeocoding { get; set; }

      [XmlElement("OYCd")]
      public TableColumn.SerializableTableColumn YCoordNotUsedForGeocoding { get; set; }

      public TableColumn.SerializableTableColumn AddressLineNotUsedForGeocoding { get; set; }

      [XmlElement("OLoc")]
      public TableColumn.SerializableTableColumn LocalityNotUsedForGeocoding { get; set; }

      [XmlElement("OADTwo")]
      public TableColumn.SerializableTableColumn AdminDistrict2NotUsedForGeocoding { get; set; }

      [XmlElement("OAD")]
      public TableColumn.SerializableTableColumn AdminDistrictNotUsedForGeocoding { get; set; }

      [XmlElement("OZip")]
      public TableColumn.SerializableTableColumn PostalCodeNotUsedForGeocoding { get; set; }

      [XmlElement("OCountry")]
      public TableColumn.SerializableTableColumn CountryNotUsedForGeocoding { get; set; }

      [XmlElement("OFA")]
      public TableColumn.SerializableTableColumn FullAddressNotUsedForGeocoding { get; set; }

      [XmlElement("OLD")]
      public TableColumn.SerializableTableColumn OtherLocationDescriptionNotUsedForGeocoding { get; set; }
    }
  }
}
