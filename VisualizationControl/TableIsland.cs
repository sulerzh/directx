using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TableIsland
  {
    public List<TableMetadata> Tables { get; private set; }

    public TableIsland()
    {
      this.Tables = new List<TableMetadata>();
    }

    public void AddTable(TableMetadata table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      this.Tables.Add(table);
      table.Island = this;
    }

    public void UpdateLookupTables()
    {
      foreach (TableMetadata tableMetadata in this.Tables)
        tableMetadata.UpdateLookupTables();
    }

    public void RankTables()
    {
      foreach (TableMetadata tableMetadata in this.Tables)
        tableMetadata.Rank = tableMetadata.PrimaryKeyTables.Count == 0 ? 0 : -1;
      bool flag;
      do
      {
        flag = false;
        foreach (TableMetadata tableMetadata1 in this.Tables)
        {
          if (tableMetadata1.Rank == -1)
          {
            foreach (TableMetadata tableMetadata2 in tableMetadata1.PrimaryKeyTables)
            {
              if (tableMetadata2.Rank == -1)
              {
                tableMetadata1.Rank = -1;
                break;
              }
              else if (tableMetadata1.Rank <= tableMetadata2.Rank)
                tableMetadata1.Rank = tableMetadata2.Rank + 1;
            }
            flag = flag | tableMetadata1.Rank != -1;
          }
        }
      }
      while (flag);
    }

    public void Sort()
    {
      foreach (TableMetadata tableMetadata in this.Tables)
        tableMetadata.Sort();
      List<TableMetadata> list = new List<TableMetadata>((IEnumerable<TableMetadata>) this.Tables);
      list.Sort((Comparison<TableMetadata>) ((t1, t2) =>
      {
        if (t1 == null || t2 == null)
          throw new InvalidOperationException("Null TableMetadata elements in this TableIsland object");
        else
          return string.Compare(t1.DisplayName, t2.DisplayName, true);
      }));
      this.Tables.Clear();
      list.ForEach((Action<TableMetadata>) (table => this.Tables.Add(table)));
    }

    public TableColumn Find(TableColumn col)
    {
      if (col == null || col.Table == null)
        return (TableColumn) null;
      foreach (TableMetadata tableMetadata in this.Tables)
      {
        TableColumn tableColumn = tableMetadata.Find(col);
        if (tableColumn != null)
          return tableColumn;
      }
      return (TableColumn) null;
    }

    public TableMeasure Find(TableMeasure measure)
    {
      if (measure == null || measure.Table == null)
        return (TableMeasure) null;
      foreach (TableMetadata tableMetadata in this.Tables)
      {
        TableMeasure tableMeasure = tableMetadata.Find(measure);
        if (tableMeasure != null)
          return tableMeasure;
      }
      return (TableMeasure) null;
    }
  }
}
