using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TableMeasure : TableMember
  {
    public TableMeasure(TableMetadata table, string name, string modelQueryName, TableMemberDataType dataType, bool visible = true)
      : base(table, name, modelQueryName, dataType, visible)
    {
    }

    internal TableMeasure(TableMeasure.SerializableTableMeasure state)
    {
      this.Unwrap((TableField.SerializableTableField) state);
    }

    internal override TableField.SerializableTableField Wrap()
    {
      TableMeasure.SerializableTableMeasure serializableTableMeasure = new TableMeasure.SerializableTableMeasure();
      base.Wrap((TableField.SerializableTableField) serializableTableMeasure);
      return (TableField.SerializableTableField) serializableTableMeasure;
    }

    [Serializable]
    public class SerializableTableMeasure : TableMember.SerializableTableMember
    {
      internal override TableField Unwrap()
      {
        return (TableField) new TableMeasure(this);
      }
    }
  }
}
