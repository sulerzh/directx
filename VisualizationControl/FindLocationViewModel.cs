using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Threading;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class FindLocationViewModel : ViewModelBase
  {
    private ILatLonProvider latLonProvider;
    private VisualizationEngine engine;
    private CancellationTokenSource cancellationSource;
    private string _StatusMessage;
    private string _Location;

    public ICommand FindCommand { get; private set; }

    public string PropertyStatusMessage
    {
      get
      {
        return "StatusMessage";
      }
    }

    public string StatusMessage
    {
      get
      {
        return this._StatusMessage;
      }
      set
      {
        this.SetProperty<string>(this.PropertyStatusMessage, ref this._StatusMessage, value, false);
      }
    }

    public string PropertyLocation
    {
      get
      {
        return "Location";
      }
    }

    public string Location
    {
      get
      {
        return this._Location;
      }
      set
      {
        this.SetProperty<string>(this.PropertyLocation, ref this._Location, value, false);
      }
    }

    private FindLocationViewModel()
    {
    }

    public FindLocationViewModel(ILatLonProvider latLonProvider, VisualizationEngine engine)
      : this()
    {
      if (latLonProvider == null)
        throw new ArgumentNullException("latLonProvider");
      if (engine == null)
        throw new ArgumentNullException("engine");
      this.latLonProvider = latLonProvider;
      this.engine = engine;
      this.FindCommand = (ICommand) new DelegatedCommand(new Action(this.GoToLocation));
    }

    public void GoToLocation()
    {
      if (string.IsNullOrWhiteSpace(this.Location))
        return;
      CancellationTokenSource cancellationTokenSource1 = new CancellationTokenSource();
      CancellationTokenSource cancellationTokenSource2 = Interlocked.Exchange<CancellationTokenSource>(ref this.cancellationSource, cancellationTokenSource1);
      if (cancellationTokenSource2 != null)
        cancellationTokenSource2.Cancel(false);
      this.latLonProvider.GetLatLonAsync(this.Location, this.cancellationSource.Token, new Action<object, bool, double, double, GeoResolutionBorder, GeoAmbiguity>(this.GoToLocationCallback), (object) new FindLocationViewModel.GoToLocationContext()
      {
        CancellationSource = cancellationTokenSource1
      });
    }

    public void Cancel()
    {
      if (this.cancellationSource == null)
        return;
      this.cancellationSource.Cancel();
    }

    private void GoToLocationCallback(object contextParam, bool succeeded, double lat, double lon, GeoResolutionBorder boundingBox, GeoAmbiguity ambiguity)
    {
      FindLocationViewModel.GoToLocationContext toLocationContext = contextParam as FindLocationViewModel.GoToLocationContext;
      if (toLocationContext == null)
        return;
      Interlocked.CompareExchange<CancellationTokenSource>(ref this.cancellationSource, (CancellationTokenSource) null, toLocationContext.CancellationSource);
      bool cancellationRequested = toLocationContext.CancellationSource.IsCancellationRequested;
      toLocationContext.CancellationSource.Dispose();
      if (cancellationRequested)
        return;
      if (!succeeded || double.IsNaN(boundingBox.South) || (double.IsNaN(boundingBox.North) || double.IsNaN(boundingBox.West)) || double.IsNaN(boundingBox.East))
        this.SetFailureStatus();
      else
        this.engine.MoveCamera(boundingBox.South, boundingBox.North, boundingBox.West, boundingBox.East, CameraMoveStyle.FlyTo);
    }

    internal void ClearStatus()
    {
      this.StatusMessage = string.Empty;
    }

    internal void SetFailureStatus()
    {
      this.StatusMessage = Resources.FindLocationDialog_NoResultsFound;
    }

    private class GoToLocationContext
    {
      public CancellationTokenSource CancellationSource;
    }
  }
}
