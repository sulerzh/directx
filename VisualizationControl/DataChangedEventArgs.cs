using System;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DataChangedEventArgs : EventArgs
  {
    internal CancellationToken CancellationToken { get; private set; }

    internal int SourceDataVersion { get; private set; }

    internal bool ZoomToData { get; private set; }

    internal bool UpdateDisplay { get; private set; }

    internal DataChangedEventArgs.DataUpdateType DataUpdateAction { get; private set; }

    public DataChangedEventArgs(CancellationToken cancellationToken, int sourceDataVersion, bool zoomToData, DataChangedEventArgs.DataUpdateType dataUpdateAction, bool updateDisplay)
    {
      this.CancellationToken = cancellationToken;
      this.SourceDataVersion = sourceDataVersion;
      this.ZoomToData = zoomToData;
      this.UpdateDisplay = updateDisplay;
      this.DataUpdateAction = dataUpdateAction;
    }

    public enum DataUpdateType
    {
      UpdateData,
      ClearData,
    }
  }
}
