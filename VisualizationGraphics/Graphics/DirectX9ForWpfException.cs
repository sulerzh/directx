using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class DirectX9ForWpfException : Exception
    {
        public string DX9ExceptionDetails;

        public DirectX9ForWpfException(string msg)
        {
            this.DX9ExceptionDetails = msg;
        }
    }
}
