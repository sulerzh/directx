using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TourDeserializationException : Exception
  {
    public TourDeserializationException(string message)
      : base(message)
    {
    }

    public TourDeserializationException(string message, Exception e)
      : base(message, e)
    {
    }
  }
}
