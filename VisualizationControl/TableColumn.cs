using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TableColumn : TableMember
  {
    public TableColumn(TableMetadata table, string name, string modelQueryName, TableMemberDataType dataType, bool visible = true)
      : base(table, name, modelQueryName, dataType, visible)
    {
    }

    internal TableColumn(TableColumn.SerializableTableColumn state)
    {
      this.Unwrap((TableField.SerializableTableField) state);
    }

    internal override TableField.SerializableTableField Wrap()
    {
      TableColumn.SerializableTableColumn serializableTableColumn = new TableColumn.SerializableTableColumn();
      base.Wrap((TableField.SerializableTableField) serializableTableColumn);
      return (TableField.SerializableTableField) serializableTableColumn;
    }

    [Serializable]
    public class SerializableTableColumn : TableMember.SerializableTableMember
    {
      internal override TableField Unwrap()
      {
        return (TableField) new TableColumn(this);
      }
    }
  }
}
