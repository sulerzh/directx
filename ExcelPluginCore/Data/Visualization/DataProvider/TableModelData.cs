using Microsoft.Data.Visualization.Utilities;
using Microsoft.Office.Interop.Excel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Data.Visualization.DataProvider
{
    public class TableModelData : IModelData
    {
        private const int s_MaxColumnCount = 6;
        private ExcelProxy<ListObject> m_Table;

        public string[] ColumnIndicesToAddToCanvas
        {
            get
            {
                List<string> list = new List<string>();
                foreach (ExcelProxy<ListColumn> excelProxy in this.m_Table.Enumerate<ListColumn>(table => (IEnumerable)table.ListColumns))
                {
                    if (excelProxy.Invoke<bool>(columnArg =>
                    {
                        return !columnArg.Range.EntireColumn.Hidden;
                    }))
                    {
                        int num = excelProxy.Invoke<int>(columnArg => columnArg.Index - 1);
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Column with index {0} is not hidden, adding to canvas.", (object)num);
                        list.Add(num.ToString(CultureInfo.InvariantCulture));
                    }
                    if (list.Count == 6)
                        break;
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Found {0} columns, done looking.", (object)list.Count);
                return list.ToArray();
            }
        }

        public ExcelProxy<WorkbookConnection> DataWorkbookConnection { get; set; }

        public TableModelData(ExcelProxy<ListObject> table)
        {
            this.m_Table = table;
        }

        public string GetCommandText()
        {
            return this.m_Table.Invoke<string>(table => table.DisplayName);
        }

        public object GetAddToModelObject()
        {
            return this.GetCommandText();
        }
    }
}
