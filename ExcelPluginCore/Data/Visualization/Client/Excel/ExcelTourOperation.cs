using System;

namespace Microsoft.Data.Visualization.Client.Excel
{
    public enum ExcelTourOperation
    {
        Open,
        Play,
        Delete,
        Duplicate,
    }

    public class ExcelTourOperationEventArgs : EventArgs
    {
        public ExcelTourOperation Operation { get; set; }

        public ExcelTourOperationEventArgs(ExcelTourOperation operation)
        {
            this.Operation = operation;
        }
    }

    public delegate void ExcelTourOperationEventHandler(object sender, ExcelTourOperationEventArgs e);

}
