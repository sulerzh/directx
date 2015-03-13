using Microsoft.Data.Visualization.VisualizationControls;
using System;

namespace Microsoft.Data.Visualization.Client.Excel
{
  internal static class TableColumnExtensions
  {
    public static string DAXQueryName(this TableMetadata self)
    {
        if (self == null)
        throw new ArgumentNullException("self");
        return string.Format("'{0}'", self.ModelName);
    }

      public static string DAXQueryName(this TableMember self)
    {
      if (self == null)
        throw new ArgumentNullException("self");
      if (self is TableColumn)
        return TableColumnExtensions.TableColumnDAXQueryName(self.Table, self.Name);
          return TableColumnExtensions.TableMeasureDAXQueryName(self.Table, self.Name);
    }

    public static string TableDAXQueryName(this TableMember self)
    {
        if (self == null)
        throw new ArgumentNullException("self");
        return TableColumnExtensions.DAXQueryName(self.Table);
    }

      public static string TableColumnDAXQueryName(TableMetadata table, string columnName)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (columnName == null)
        throw new ArgumentNullException("columnName");
      if (columnName.IndexOf(']') == -1)
        return string.Format("{0}[{1}]", TableColumnExtensions.DAXQueryName(table), columnName);
          return string.Format("{0}[{1}]", TableColumnExtensions.DAXQueryName(table), columnName.Replace("]", "]]"));
    }

    public static string TableMeasureDAXQueryName(TableMetadata table, string measureName)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (measureName == null)
        throw new ArgumentNullException("measureName");
      if (measureName.IndexOf(']') == -1)
        return string.Format("{0}[{1}]", TableColumnExtensions.DAXQueryName(table), measureName);
        return string.Format("{0}[{1}]", TableColumnExtensions.DAXQueryName(table), measureName.Replace("]", "]]"));
    }
  }
}
