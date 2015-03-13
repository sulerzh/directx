using Microsoft.Office.Interop.Excel;

namespace Microsoft.Data.Visualization.DataProvider
{
    public interface IModelData
    {
        string[] ColumnIndicesToAddToCanvas { get; }

        ExcelProxy<WorkbookConnection> DataWorkbookConnection { get; set; }

        string GetCommandText();

        object GetAddToModelObject();
    }
}
