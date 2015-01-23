using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class TableField
  {
    public string Name { get; private set; }

    public bool Visible { get; private set; }

    public List<TableField> PrimaryKeyFields { get; private set; }

    public List<TableField> ForeignKeyFields { get; private set; }

    public TableField(string name, bool visible)
      : this()
    {
      if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("name is null or whitespace");
      this.Name = name;
      this.Visible = visible;
    }

    internal TableField()
    {
      this.PrimaryKeyFields = new List<TableField>();
      this.ForeignKeyFields = new List<TableField>();
    }

    public void AddPrimaryKeyField(TableField field)
    {
      if (field == null)
        throw new ArgumentNullException("field");
      this.PrimaryKeyFields.Add(field);
    }

    public void AddForeignKeyField(TableField field)
    {
      if (field == null)
        throw new ArgumentNullException("field");
      this.ForeignKeyFields.Add(field);
    }

    protected virtual void Wrap(TableField.SerializableTableField field)
    {
      field.Name = this.Name;
      field.Visible = this.Visible;
    }

    internal virtual void Unwrap(TableField.SerializableTableField field)
    {
      if (string.IsNullOrWhiteSpace(field.Name))
        throw new ArgumentException("field.Name is null or whitespace");
      this.Name = field.Name;
      this.Visible = field.Visible;
    }

    protected virtual void NotifyPropertyChanged(string property)
    {
    }

    internal abstract TableField.SerializableTableField Wrap();

    [Serializable]
    public abstract class SerializableTableField
    {
      [XmlAttribute]
      public string Name;
      [XmlAttribute]
      public bool Visible;

      internal abstract TableField Unwrap();
    }
  }
}
