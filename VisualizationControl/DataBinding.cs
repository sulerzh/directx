using System;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class DataBinding
  {
    private object sourceDataVersionLock = new object();

    public object VisualElement { get; protected set; }

    public DataView DataView { get; protected set; }

    public int LastSourceDataVersion { get; private set; }

    public int LastDisplayedSourceDataVersion { get; private set; }

    public bool DisplayNeedsRefresh
    {
      get
      {
        lock (this.SourceDataVersionLock)
          return this.LastSourceDataVersion != 0 && this.LastSourceDataVersion != this.LastDisplayedSourceDataVersion;
      }
    }

    protected object SourceDataVersionLock
    {
      get
      {
        return this.sourceDataVersionLock;
      }
    }

    public DataBinding(object visualElement, DataView dataView)
    {
      this.VisualElement = visualElement;
      this.DataView = dataView;
      this.LastSourceDataVersion = 0;
      this.LastDisplayedSourceDataVersion = 0;
      this.DataView.OnDataChanged += new EventHandler(this.OnDataChanged);
    }

    internal virtual void ClearVisualElement()
    {
      this.ClearCachedData();
      this.ClearDisplay();
      this.VisualElement = (object) null;
    }

    internal virtual void Removed()
    {
      this.ClearVisualElement();
      if (this.DataView != null)
      {
        this.DataView.Removed();
        this.DataView.OnDataChanged -= new EventHandler(this.OnDataChanged);
        this.DataView = (DataView) null;
      }
      this.Shutdown();
    }

    internal virtual void Shutdown()
    {
      this.VisualElement = (object) null;
    }

    private void OnDataChanged(object sender, EventArgs e)
    {
      DataChangedEventArgs dataChangedEventArgs = e as DataChangedEventArgs;
      if (dataChangedEventArgs == null)
        return;
      bool updateDisplay = dataChangedEventArgs.UpdateDisplay;
      int sourceDataVersion = dataChangedEventArgs.SourceDataVersion;
      switch (dataChangedEventArgs.DataUpdateAction)
      {
        case DataChangedEventArgs.DataUpdateType.UpdateData:
          bool flag = false;
          dataChangedEventArgs.CancellationToken.ThrowIfCancellationRequested();
          lock (this.SourceDataVersionLock)
          {
            if (sourceDataVersion < this.LastSourceDataVersion)
              break;
            if (updateDisplay)
            {
              if (this.LastDisplayedSourceDataVersion != 0)
              {
                if (sourceDataVersion != this.LastDisplayedSourceDataVersion)
                  flag = true;
              }
            }
          }
          if (flag)
            this.ClearDisplay();
          this.RefreshData(dataChangedEventArgs.CancellationToken, sourceDataVersion, updateDisplay, dataChangedEventArgs);
          lock (this.SourceDataVersionLock)
          {
            this.LastSourceDataVersion = Math.Max(sourceDataVersion, this.LastSourceDataVersion);
            if (!updateDisplay)
              break;
            this.LastDisplayedSourceDataVersion = Math.Max(sourceDataVersion, this.LastDisplayedSourceDataVersion);
            break;
          }
        case DataChangedEventArgs.DataUpdateType.ClearData:
          if (updateDisplay)
            this.ClearDisplay();
          this.ClearCachedData();
          lock (this.SourceDataVersionLock)
          {
            this.LastSourceDataVersion = 0;
            this.LastDisplayedSourceDataVersion = 0;
            break;
          }
      }
    }

    protected virtual void ClearCachedData()
    {
    }

    protected abstract void RefreshData(CancellationToken cancellationToken, int sourceDataVersion, bool updateDisplay, DataChangedEventArgs dataChangedEventArgs);

    protected abstract void ClearDisplay();
  }
}
