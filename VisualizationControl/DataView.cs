using System;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class DataView
  {
    public int DisplayedDataVersion { get; private set; }

    public bool Cleared { get; private set; }

    public DataSource Source { get; private set; }

    public event EventHandler OnDataChanged;

    protected DataView(DataSource source)
    {
      this.Source = source;
      this.DisplayedDataVersion = 0;
      this.Cleared = true;
    }

    public void Clear(int sourceDataVersion, bool onlyIfStale = true)
    {
      if (this.Cleared || onlyIfStale && this.DisplayedDataVersion == sourceDataVersion)
        return;
      if (this.OnDataChanged != null)
        this.OnDataChanged((object) this, (EventArgs) new DataChangedEventArgs(CancellationToken.None, sourceDataVersion, false, DataChangedEventArgs.DataUpdateType.ClearData, true));
      this.DisplayedDataVersion = 0;
      this.Cleared = true;
    }

    public int GetRowCount(int sourceDataVersion)
    {
      try
      {
        DataSource source = this.Source;
        if (source == null)
          return -1;
        else
          return source.GetQueryResultsItemCount(sourceDataVersion);
      }
      catch (DataSource.InvalidQueryResultsException ex)
      {
        return -1;
      }
    }

    internal void Update(CancellationToken cancellationToken, int sourceDataVersion, bool zoomToData, bool updateDisplay = true)
    {
      if (!updateDisplay && this.DisplayedDataVersion == sourceDataVersion)
        return;
      if (this.OnDataChanged != null)
        this.OnDataChanged((object) this, (EventArgs) new DataChangedEventArgs(cancellationToken, sourceDataVersion, zoomToData, DataChangedEventArgs.DataUpdateType.UpdateData, updateDisplay));
      this.DisplayedDataVersion = sourceDataVersion;
      this.Cleared = false;
    }

    internal void Removed()
    {
      DataSource source = this.Source;
      if (source != null)
        source.RemoveDataView(this);
      this.Shutdown();
    }

    internal virtual void Shutdown()
    {
      this.Source = (DataSource) null;
    }

    public enum DataViewType
    {
      Excel,
      Chart,
    }
  }
}
