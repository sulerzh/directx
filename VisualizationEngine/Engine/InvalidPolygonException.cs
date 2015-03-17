using System;

namespace Microsoft.Data.Visualization.Engine
{
    public class InvalidPolygonException : Exception
    {
        public InvalidPolygonException(string message)
            : base(message)
        {
        }
    }
}
