using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TableMetadata
  {
    public string ModelName { get; private set; }

    public string NameInSource { get; private set; }

    public List<TableMetadata> LookupTables { get; private set; }

    public string DisplayName
    {
      get
      {
        return this.ModelName;
      }
    }

    public DateTime LastRefresh { get; private set; }

    public bool Visible { get; private set; }

    public int Rank { get; internal set; }

    public List<TableField> Fields { get; private set; }

    public List<TableMeasure> Measures { get; private set; }

    public List<TableMetadata> PrimaryKeyTables { get; private set; }

    public List<TableMetadata> ForeignKeyTables { get; private set; }

    public TableIsland Island { get; internal set; }

    public TableMetadata(string modelName, string nameInSource, DateTime lastRefresh = new DateTime( ), bool visible = true)
      : this()
    {
      if (string.IsNullOrWhiteSpace(modelName))
        throw new ArgumentException("modelName is null or whitespace");
      this.ModelName = modelName;
      this.NameInSource = string.IsNullOrWhiteSpace(nameInSource) ? (string) null : nameInSource;
      this.LastRefresh = lastRefresh;
      this.LookupTables = new List<TableMetadata>();
      this.LookupTables.Add(this);
      this.Visible = visible;
    }

    internal TableMetadata(TableMetadata.SerializableTableMetadata state)
      : this()
    {
      this.Unwrap(state);
    }

    private TableMetadata()
    {
      this.Fields = new List<TableField>();
      this.Measures = new List<TableMeasure>();
      this.PrimaryKeyTables = new List<TableMetadata>();
      this.ForeignKeyTables = new List<TableMetadata>();
    }

    internal TableMetadata.SerializableTableMetadata Wrap()
    {
      return new TableMetadata.SerializableTableMetadata()
      {
        ModelName = this.ModelName,
        NameInSource = this.NameInSource,
        Visible = this.Visible,
        LastRefresh = this.LastRefresh
      };
    }

    internal void Unwrap(TableMetadata.SerializableTableMetadata state)
    {
      if (state == null)
        throw new ArgumentNullException("state");
      if (string.IsNullOrWhiteSpace(state.ModelName))
        throw new ArgumentException("state.ModelName is null or whitespace");
      this.ModelName = state.ModelName;
      this.NameInSource = state.NameInSource;
      this.Visible = state.Visible;
      this.LastRefresh = state.LastRefresh;
    }

    public void AddField(TableField field)
    {
      if (field == null)
        throw new ArgumentNullException("field");
      if (field is TableMeasure)
      {
        this.AddMeasure((TableMeasure) field);
      }
      else
      {
        if (this.GetField(field.Name) != null)
          throw new ArgumentException("Another field in the table has the same name as the one being added.");
        this.Fields.Add(field);
      }
    }

    public void AddMeasure(TableMeasure measure)
    {
      if (measure == null)
        throw new ArgumentNullException("measure");
      if (this.GetMeasure(measure.Name) != null)
        throw new ArgumentException("Another measure in the table has the same name as the one being added.");
      this.Measures.Add(measure);
    }

    public void AddPrimaryKeyTable(TableMetadata table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      this.PrimaryKeyTables.Add(table);
    }

    public void AddForeignKeyTable(TableMetadata table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      this.ForeignKeyTables.Add(table);
    }

    public TableField GetField(string fieldName)
    {
      return Enumerable.SingleOrDefault<TableField>((IEnumerable<TableField>) this.Fields, (Func<TableField, bool>) (fld => fld.Name == fieldName));
    }

    public TableMeasure GetMeasure(string measureName)
    {
      return Enumerable.SingleOrDefault<TableMeasure>((IEnumerable<TableMeasure>) this.Measures, (Func<TableMeasure, bool>) (measure => measure.Name == measureName));
    }

    public bool RefersToTheSameTableAs(TableMetadata table)
    {
      if (table == null)
        return false;
      else
        return string.Compare(this.ModelName, table.ModelName, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public void Sort()
    {
      List<TableField> list1 = new List<TableField>((IEnumerable<TableField>) this.Fields);
      list1.Sort((Comparison<TableField>) ((c1, c2) =>
      {
        if (c1 == null || c2 == null)
          throw new InvalidOperationException("Null TableField elements in this TableMetadata object");
        else
          return string.Compare(c1.Name, c2.Name, true);
      }));
      this.Fields.Clear();
      list1.ForEach((Action<TableField>) (col => this.Fields.Add(col)));
      List<TableMeasure> list2 = new List<TableMeasure>((IEnumerable<TableMeasure>) this.Measures);
      list2.Sort((Comparison<TableMeasure>) ((m1, m2) =>
      {
        if (m1 == null || m2 == null)
          throw new InvalidOperationException("Null TableMeasure elements in this TableMetadata object");
        else
          return string.Compare(m1.Name, m2.Name, true);
      }));
      this.Measures.Clear();
      list2.ForEach((Action<TableMeasure>) (measure => this.Measures.Add(measure)));
    }

    public TableColumn Find(TableColumn col)
    {
      if (col == null || col.Table == null)
        return (TableColumn) null;
      foreach (TableField tableField in this.Fields)
      {
        TableColumn tableColumn = tableField as TableColumn;
        if (tableColumn != null && tableColumn.RefersToTheSameMemberAs((TableMember) col))
          return tableColumn;
      }
      return (TableColumn) null;
    }

    public TableMeasure Find(TableMeasure measure)
    {
      if (measure == null || measure.Table == null)
        return (TableMeasure) null;
      foreach (TableMeasure tableMeasure in this.Measures)
      {
        if (tableMeasure != null && tableMeasure.RefersToTheSameMemberAs((TableMember) measure))
          return tableMeasure;
      }
      return (TableMeasure) null;
    }

    public bool ContainsLookupTable(TableMetadata table)
    {
      if (table == null)
        return false;
      else
        return this.LookupTables.Contains(table);
    }

    public void UpdateLookupTables()
    {
      List<TableMetadata> list = new List<TableMetadata>();
      Queue<TableMetadata> queue = new Queue<TableMetadata>();
      queue.Enqueue(this);
      while (queue.Count > 0)
      {
        TableMetadata tableMetadata1 = queue.Dequeue();
        list.Add(tableMetadata1);
        foreach (TableMetadata tableMetadata2 in tableMetadata1.PrimaryKeyTables)
          queue.Enqueue(tableMetadata2);
      }
      this.LookupTables.Clear();
      list.ForEach((Action<TableMetadata>) (table => this.LookupTables.Add(table)));
    }

    [Serializable]
    public class SerializableTableMetadata
    {
      [XmlAttribute]
      public string ModelName;
      [XmlAttribute]
      public string NameInSource;
      [XmlAttribute]
      public bool Visible;
      [XmlAttribute]
      public DateTime LastRefresh;

      internal TableMetadata Unwrap()
      {
        return new TableMetadata(this);
      }
    }
  }
}
