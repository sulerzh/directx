using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class VisualizationModel : IDisposable
  {
    private bool disposed;
    private ModelMetadata modelMetadata;
    private readonly GeospatialDataProviders _geospatialDataProviders;
    private readonly WeakEventListener<VisualizationModel, object, InternalErrorEventArgs> engineOnInternalError;
    private readonly WeakEventListener<VisualizationModel, Exception> latlonOnInternalError;
    private WeakEventListener<VisualizationModel, object, PropertyChangedEventArgs> settingsPropertyChanged;
    private IDataSourceFactory dataSourceFactory;
    private BingMapResourceUri bingMapResources;
    private ICustomMapProvider customMapProvider;

    public Thread TwoDRenderThread { get; private set; }

    public bool ConnectionsDisabled
    {
      get
      {
        return this.ModelWrapper.ConnectionsDisabled;
      }
    }

    internal ILatLonProvider LatLonProvider
    {
      get
      {
        return this._geospatialDataProviders.LatLonProvider;
      }
    }

    internal IRegionProvider RegionProvider
    {
      get
      {
        return this._geospatialDataProviders.RegionProvider;
      }
    }

    internal ITourPersist TourPersist { get; private set; }

    internal VisualizationEngine Engine { get; private set; }

    internal ITimeController TimeController
    {
      get
      {
        if (this.Engine != null)
          return this.Engine.TimeControl;
        else
          return (ITimeController) null;
      }
    }

    internal LayerManager LayerManager { get; private set; }

    internal Tour CurrentTour { get; private set; }

    public Dispatcher UIDispatcher { get; private set; }

    internal Thread QueryEngineThread { get; private set; }

    internal Dispatcher QueryEngineDispatcher
    {
      get
      {
        if (this.QueryEngineThread == null)
          return (Dispatcher) null;
        else
          return Dispatcher.FromThread(this.QueryEngineThread);
      }
    }

    private IModelWrapper ModelWrapper { get; set; }

    internal ColorSelector ColorSelector { get; private set; }

    internal BingMapResourceUri BingMapResources
    {
      get
      {
        return this.bingMapResources;
      }
    }

    internal ICustomMapProvider CustomMapProvider
    {
      get
      {
        return this.customMapProvider;
      }
    }

    public event InternalErrorEventHandler OnInternalError;

    public VisualizationModel(IDataSourceFactory dataSourceFactory, ILatLonProvider latLonProvider, ITourPersist tourPersist, IModelWrapper modelWrapper)
      : this(dataSourceFactory, new GeospatialDataProviders()
      {
        LatLonProvider = latLonProvider,
        RegionProvider = (IRegionProvider) new BingRegionProvider(2)
      }, tourPersist, modelWrapper)
    {
    }

    public VisualizationModel(IDataSourceFactory dataSourceFactory, GeospatialDataProviders _geospatialDataProviders, ITourPersist tourPersist, IModelWrapper modelWrapper)
    {
      if (_geospatialDataProviders == null)
        throw new ArgumentNullException("_geospatialDataProviders");
      if (_geospatialDataProviders.LatLonProvider == null)
        throw new ArgumentException("LatLonProvider can not be null.", "_geospatialDataProviders");
      if (_geospatialDataProviders.RegionProvider == null)
        throw new ArgumentException("RegionProvider can not be null.", "_geospatialDataProviders");
      if (tourPersist == null)
        throw new ArgumentNullException("tourPersist");
      if (modelWrapper == null)
        throw new ArgumentNullException("modelWrapper");
      if (dataSourceFactory == null)
        throw new ArgumentNullException("dataSourceFactory");
      this._geospatialDataProviders = _geospatialDataProviders;
      this.TourPersist = tourPersist;
      this.ModelWrapper = modelWrapper;
      this.modelMetadata = (ModelMetadata) null;
      this.dataSourceFactory = dataSourceFactory;
      this.UIDispatcher = Dispatcher.CurrentDispatcher;
      this.engineOnInternalError = new WeakEventListener<VisualizationModel, object, InternalErrorEventArgs>(this)
      {
        OnEventAction = new Action<VisualizationModel, object, InternalErrorEventArgs>(VisualizationModel.Engine_OnInternalError)
      };
      this.latlonOnInternalError = new WeakEventListener<VisualizationModel, Exception>(this)
      {
        OnEventAction = new Action<VisualizationModel, Exception>(VisualizationModel.LatLon_OnInternalError)
      };
      this.LatLonProvider.OnInternalError = new Action<Exception>(this.latlonOnInternalError.OnEvent);
    }

    public void Initialize(ICustomMapProvider customMapsProv = null, BingMapResourceUri bingMapResourceUri = null, int width = 0, int height = 0, int fps = 0)
    {
      this.bingMapResources = bingMapResourceUri;
      this.customMapProvider = customMapsProv;
      if (fps > 0)
      {
        this.Engine = new VisualizationEngine(width, height, Dispatcher.CurrentDispatcher, customMapsProv, this.bingMapResources, fps);
      }
      else
      {
        this.Engine = new VisualizationEngine(width, height, IntPtr.Zero, (InputHandler) null, Dispatcher.CurrentDispatcher, customMapsProv, Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.GraphicsLevel, false, bingMapResourceUri, 0);
        this.settingsPropertyChanged = new WeakEventListener<VisualizationModel, object, PropertyChangedEventArgs>(this)
        {
          OnEventAction = new Action<VisualizationModel, object, PropertyChangedEventArgs>(VisualizationModel.Settings_PropertyChanged)
        };
        Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(this.settingsPropertyChanged.OnEvent);
      }
      this.Engine.OnInternalError += new InternalErrorEventHandler(this.engineOnInternalError.OnEvent);
      this.ColorSelector = new ColorSelector(this.Engine);
      this.LayerManager = new LayerManager(this, this.dataSourceFactory);
      this.TwoDRenderThread = new Thread(new ParameterizedThreadStart(this.TwoDRenderLoopStart));
      this.TwoDRenderThread.Name = "2D Render Thread";
      this.TwoDRenderThread.SetApartmentState(ApartmentState.STA);
      this.TwoDRenderThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
      this.TwoDRenderThread.CurrentCulture = (CultureInfo) Thread.CurrentThread.CurrentCulture.Clone();
      this.TwoDRenderThread.CurrentCulture.DateTimeFormat.Calendar = Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar;
      this.TwoDRenderThread.Start();
      this.QueryEngineThread = new Thread(new ParameterizedThreadStart(this.QueryEngineLoopStart));
      this.QueryEngineThread.Name = "Query Engine Thread";
      this.QueryEngineThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
      this.QueryEngineThread.CurrentCulture = (CultureInfo) Thread.CurrentThread.CurrentCulture.Clone();
      this.QueryEngineThread.CurrentCulture.DateTimeFormat.Calendar = Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar;
      this.QueryEngineThread.Start();
    }

    private static void Settings_PropertyChanged(VisualizationModel model, object sender, PropertyChangedEventArgs e)
    {
      if (model.Engine == null || !(e.PropertyName == "GraphicsLevel"))
        return;
      model.Engine.GraphicsLevel = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.GraphicsLevel;
    }

    public void Reset()
    {
      if (this.LayerManager != null)
        this.LayerManager.Reset();
      if (this.ColorSelector == null)
        return;
      this.ColorSelector.Reset();
    }

    public void RefreshAll()
    {
      this.ModelWrapper.RefreshAll();
    }

    internal void RaiseInternalError(InternalErrorEventArgs args)
    {
      try
      {
        if (this.OnInternalError == null)
          return;
        this.OnInternalError((object) this, args);
      }
      catch (Exception ex)
      {
      }
    }

    internal void RaiseInternalError(Exception ex)
    {
      if (this.OnInternalError == null)
        return;
      this.OnInternalError((object) this, new InternalErrorEventArgs(ex));
    }

    private static void LatLon_OnInternalError(VisualizationModel model, Exception ex)
    {
      model.RaiseInternalError(ex);
    }

    private static void Engine_OnInternalError(VisualizationModel model, object sender, InternalErrorEventArgs args)
    {
      VisualizationTraceSource.Current.Fail(args.InternalErrorException);
      model.RaiseInternalError(args);
    }

    internal void ModelChanged(ModelMetadata modelMetadata, List<string> tablesWithUpdatedData)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationModel.ModelChanged(): starting");
      this.SetModelMetadata(modelMetadata);
      this.LayerManager.ModelChanged(tablesWithUpdatedData);
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationModel.ModelChanged(): done");
    }

    internal ModelMetadata GetModelMetadata()
    {
      if (this.modelMetadata == null)
        this.SetModelMetadata(this.ModelWrapper.GetTableMetadata());
      return this.modelMetadata;
    }

    internal void SetModelMetadata(ModelMetadata modelMetadata)
    {
      this.modelMetadata = modelMetadata;
      if (this.LatLonProvider == null)
        return;
      this.LatLonProvider.ModelCulture = modelMetadata == null ? (CultureInfo) null : modelMetadata.Culture;
    }

    private void TwoDRenderLoopStart(object context)
    {
      try
      {
        Dispatcher.CurrentDispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(this.DispatcherUnhandledException);
        Dispatcher.Run();
      }
      finally
      {
        this.TwoDRenderThread = (Thread) null;
      }
    }

    private void QueryEngineLoopStart(object context)
    {
      try
      {
        Dispatcher.CurrentDispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(this.DispatcherUnhandledException);
        Dispatcher.Run();
      }
      finally
      {
        this.QueryEngineThread = (Thread) null;
      }
    }

    public void SetCurrentTour(Tour tour)
    {
      this.CurrentTour = tour;
    }

    public VisualizationModel Clone()
    {
      return new VisualizationModel(this.dataSourceFactory, this._geospatialDataProviders, this.TourPersist, this.ModelWrapper);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing)
      {
        if (this.LatLonProvider != null)
          this.LatLonProvider.OnInternalError = (Action<Exception>) null;
        if (this.Engine != null)
        {
          this.Engine.OnInternalError -= new InternalErrorEventHandler(this.engineOnInternalError.OnEvent);
          this.Engine.Dispose();
          this.Engine = (VisualizationEngine) null;
        }
        if (this.TwoDRenderThread != null)
        {
          Dispatcher dispatcher = Dispatcher.FromThread(this.TwoDRenderThread);
          if (dispatcher != null)
            dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
          this.TwoDRenderThread = (Thread) null;
        }
        if (this.QueryEngineThread != null)
        {
          Dispatcher dispatcher = Dispatcher.FromThread(this.QueryEngineThread);
          if (dispatcher != null)
            dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
          this.QueryEngineThread = (Thread) null;
        }
      }
      this.disposed = true;
    }

    private void DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
    {
      try
      {
        args.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
        args.Handled = true;
        if (!(args.Exception.GetType() != typeof (ThreadAbortException)))
          return;
        VisualizationTraceSource.Current.Fail(args.Exception);
        this.RaiseInternalError(args.Exception);
      }
      catch (Exception ex)
      {
      }
    }
  }
}
