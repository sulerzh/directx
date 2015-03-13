using Microsoft.Data.Visualization.Utilities;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Data.Visualization.DataProvider
{
    public class RangeModelData : IModelData
    {
        private const int s_MaxColumnCount = 6;
        private ExcelProxy<Microsoft.Office.Interop.Excel.Range> m_Range;

        public string[] ColumnIndicesToAddToCanvas
        {
            get
            {
                List<string> list = new List<string>();
                int num = 0;
                foreach (ExcelProxy<Microsoft.Office.Interop.Excel.Range> excelProxy in this.m_Range.Enumerate<Microsoft.Office.Interop.Excel.Range>(range => (IEnumerable)range.Columns))
                {
                    if (excelProxy.Invoke<bool>(columnArg =>
                    {
                        return !columnArg.EntireColumn.Hidden;
                    }))
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Column with index {0} is not hidden, adding to canvas.", (object)num);
                        list.Add(num.ToString(CultureInfo.InvariantCulture));
                    }
                    if (list.Count != s_MaxColumnCount)
                        ++num;
                    else
                        break;
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Found {0} columns, done looking.", (object)list.Count);
                return list.ToArray();
            }
        }

        public ExcelProxy<WorkbookConnection> DataWorkbookConnection { get; set; }

        public RangeModelData(ExcelProxy<Microsoft.Office.Interop.Excel.Range> selection)
        {
            this.m_Range = selection;
        }

        public string GetCommandText()
        {
            string str = this.m_Range.InvokeAndProxy<Microsoft.Office.Interop.Excel.Range>(range => range.Areas.get_Item(range.Areas.Count)).Invoke<string>(range => range.get_Address(true, true, XlReferenceStyle.xlA1, false, Type.Missing));
            return string.Format(CultureInfo.InvariantCulture, "{0}!{1}", new object[2]
            {
                this.m_Range.Invoke<string>(range => range.Worksheet.Name),
                str
            });
        }

        public object GetAddToModelObject()
        {
            return this.m_Range.Invoke<Microsoft.Office.Interop.Excel.Range>(range => range);
        }
    }
}
