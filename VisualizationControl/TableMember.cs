// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.VisualizationControls.TableMember
// Assembly: VisualizationControl, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: A037A4F1-A72D-4DB4-A14A-579F6D8CFD96
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\visualizationcontrol.dll

using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class TableMember : TableField
  {
    public TableMemberDataType DataType { get; private set; }

    public TableMemberClassification Classification { get; set; }

    public TableMetadata Table { get; private set; }

    public string ModelQueryName { get; private set; }

    public TableMember(TableMetadata table, string name, string modelQueryName, TableMemberDataType dataType, bool visible = true)
      : base(name, visible)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (modelQueryName == null)
        throw new ArgumentNullException("modelQueryName");
      this.DataType = dataType;
      this.Table = table;
      this.ModelQueryName = modelQueryName;
    }

    internal TableMember()
    {
    }

    public bool RefersToTheSameMemberAs(TableMember tableMember)
    {
      if (tableMember == null || this.GetType() != tableMember.GetType() || string.Compare(this.Name, tableMember.Name, StringComparison.OrdinalIgnoreCase) != 0)
        return false;
      else
        return this.Table.RefersToTheSameTableAs(tableMember.Table);
    }

    public bool QuerySubstitutable(TableMember other)
    {
      if (object.ReferenceEquals((object) this, (object) other))
        return true;
      else
        return this.RefersToTheSameMemberAs(other);
    }

    public static bool QuerySubstitutable(object first, object second)
    {
      if (!(first is TableMember))
        return second == null;
      else
        return (first as TableMember).QuerySubstitutable(second as TableMember);
    }

    protected override void Wrap(TableField.SerializableTableField field)
    {
      TableMember.SerializableTableMember serializableTableMember = (TableMember.SerializableTableMember) field;
      serializableTableMember.Table = this.Table.Wrap();
      serializableTableMember.DataType = this.DataType;
      serializableTableMember.ModelQueryName = this.ModelQueryName;
      base.Wrap((TableField.SerializableTableField) serializableTableMember);
    }

    internal override void Unwrap(TableField.SerializableTableField wrappedState)
    {
      if (wrappedState == null)
        throw new ArgumentNullException("wrappedState");
      TableMember.SerializableTableMember serializableTableMember = wrappedState as TableMember.SerializableTableMember;
      if (serializableTableMember == null)
        throw new ArgumentException("wrappedState must be of type SerializableTableMember");
      if (serializableTableMember.Table == null)
        throw new ArgumentException("state.Table must not be null");
      base.Unwrap((TableField.SerializableTableField) serializableTableMember);
      this.Table = serializableTableMember.Table.Unwrap();
      this.DataType = serializableTableMember.DataType;
      this.ModelQueryName = serializableTableMember.ModelQueryName;
    }

    [Serializable]
    public abstract class SerializableTableMember : TableField.SerializableTableField
    {
      public TableMetadata.SerializableTableMetadata Table;
      [XmlAttribute]
      public TableMemberDataType DataType;
      [XmlAttribute]
      public string ModelQueryName;
    }
  }
}
