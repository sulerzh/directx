using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ClientProxy : IDisposable
  {
    private VisualizationModel visualizationModel;
    private HostControlViewModel hostControlViewModel;
    private ChromelessWindowBase hostWindow;
    private Thread visualizationThread;
    private bool disposed;
    private bool initializeFailed;
    private readonly IDataSourceFactory dataSourceFactory;
    private readonly ITourPersist tourPersist;
    private readonly IModelWrapper modelWrapper;
    private readonly IHelpViewer helpViewer;
    private readonly ICustomMapProvider customMapsProvider;
    private readonly GeospatialDataProviders _geospatialDataProviders;
    private readonly WeakEventListener<ClientProxy, object, InternalErrorEventArgs> modelOnInternalError;
    private readonly WeakEventListener<ClientProxy, object, DispatcherUnhandledExceptionEventArgs> dispatcherOnUnhandledException;
    private BingMapResourceUri mapResourceUris;

    public int CurrentTourHandle
    {
      get
      {
        if (this.hostWindow != null && this.hostControlViewModel != null && this.hostControlViewModel.Model.CurrentTour != null)
          return this.hostControlViewModel.Model.CurrentTour.Handle;
        else
          return 0;
      }
    }

    public event InternalErrorEventHandler OnInternalError;

    public event EventHandler OnHostWindowOpened;

    public event EventHandler OnHostWindowClosed;

    public ClientProxy(IDataSourceFactory dataSourceFactory, ITourPersist tourPersist, IModelWrapper modelWrapper, IHelpViewer helpViewer, ICustomMapProvider customMapsProvider, GeospatialDataProviders geospatialDataProviders, BingMapResourceUri mapResourceUris)
    {
      this.dataSourceFactory = dataSourceFactory;
      this._geospatialDataProviders = geospatialDataProviders;
      this.tourPersist = tourPersist;
      this.modelWrapper = modelWrapper;
      this.helpViewer = helpViewer;
      this.customMapsProvider = customMapsProvider;
      this.mapResourceUris = mapResourceUris;
      this.modelOnInternalError = new WeakEventListener<ClientProxy, object, InternalErrorEventArgs>(this)
      {
        OnEventAction = new Action<ClientProxy, object, InternalErrorEventArgs>(ClientProxy.visualizationModel_OnInternalError)
      };
      this.dispatcherOnUnhandledException = new WeakEventListener<ClientProxy, object, DispatcherUnhandledExceptionEventArgs>(this)
      {
        OnEventAction = new Action<ClientProxy, object, DispatcherUnhandledExceptionEventArgs>(ClientProxy.DispatcherUnhandledException)
      };
    }

    public bool Initialize(CultureInfo UICultureInfo, List<Color4F> customColors = null, Calendar gregorianCalendar = null)
    {
      this.initializeFailed = false;
      Resources.Culture = UICultureInfo;
      EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
      this.visualizationThread = new Thread(new ParameterizedThreadStart(this.VisualizationStart));
      this.visualizationThread.Name = "UI Thread";
      this.visualizationThread.SetApartmentState(ApartmentState.STA);
      this.visualizationThread.CurrentUICulture = UICultureInfo;
      if (gregorianCalendar != null)
      {
        this.visualizationThread.CurrentCulture = (CultureInfo) Thread.CurrentThread.CurrentCulture.Clone();
        this.visualizationThread.CurrentCulture.DateTimeFormat.Calendar = gregorianCalendar;
      }
      this.visualizationThread.Start((object) new ClientProxy.VisualizationThreadStartContext()
      {
        VisualizationThreadRunning = eventWaitHandle,
        CustomColors = customColors
      });
      try
      {
        eventWaitHandle.WaitOne();
        eventWaitHandle.Dispose();
      }
      catch (SEHException ex1)
      {
        try
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "ClientProxy.Initialize(): Ignoring SEHException {0}", (object) ex1);
          eventWaitHandle.Dispose();
        }
        catch (Exception ex2)
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "ClientProxy.Initialize(): Ignoring exception when disposing thread ex={0}", (object) ex2);
        }
        this.initializeFailed = true;
      }
      return !this.initializeFailed;
    }

    public void ModelChanged(List<string> tablesWithUpdatedData)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "ClientProxy.ModelChanged(): starting");
      ModelMetadata modelMetadata = this.modelWrapper.GetTableMetadata();
      Dispatcher dispatcher = Dispatcher.FromThread(this.visualizationThread);
      if (dispatcher != null && this.visualizationModel != null)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "ClientProxy.ModelChanged(): Notifying VisualizationModel of ModelChanged event asynchronously");
          Action action = delegate
          {
              this.visualizationModel.ModelChanged(modelMetadata, tablesWithUpdatedData);
          };
        dispatcher.BeginInvoke(action);
      }
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "ClientProxy.ModelChanged(): done");
    }

    private void VisualizationStart(object contextParam)
    {
      ClientProxy.VisualizationThreadStartContext threadStartContext = (ClientProxy.VisualizationThreadStartContext) contextParam;
      EventWaitHandle visualizationThreadRunning = threadStartContext.VisualizationThreadRunning;
      try
      {
        this.visualizationModel = new VisualizationModel(this.dataSourceFactory, this._geospatialDataProviders, this.tourPersist, this.modelWrapper);
        this.visualizationModel.OnInternalError += new InternalErrorEventHandler(this.modelOnInternalError.OnEvent);
        this.visualizationModel.Initialize(this.customMapsProvider, this.mapResourceUris, 0, 0, 0);
        this.hostControlViewModel = new HostControlViewModel(this.visualizationModel, this.helpViewer, this.mapResourceUris, threadStartContext.CustomColors);
        visualizationThreadRunning.Set();
      }
      catch (Exception ex1)
      {
        VisualizationTraceSource.Current.Fail(ex1);
        if (this.OnInternalError != null)
        {
          try
          {
            this.OnInternalError((object) this, new InternalErrorEventArgs(ex1));
          }
          catch (Exception ex2)
          {
          }
        }
        if (visualizationThreadRunning != null)
        {
          try
          {
            visualizationThreadRunning.Set();
          }
          catch (Exception ex2)
          {
          }
        }
        this.initializeFailed = true;
        return;
      }
      Dispatcher.CurrentDispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(this.dispatcherOnUnhandledException.OnEvent);
      Dispatcher.Run();
    }

    private static void visualizationModel_OnInternalError(ClientProxy proxy, object sender, InternalErrorEventArgs e)
    {
      if (proxy.OnInternalError == null)
        return;
      proxy.OnInternalError((object) proxy, new InternalErrorEventArgs(e.InternalErrorException));
    }

    public void ShowHostWindow(string windowTitle)
    {
      if (this.hostWindow == null || this.hostControlViewModel == null || this.visualizationModel == null)
        return;
      Dispatcher dispatcher = Dispatcher.FromThread(this.visualizationThread);
      this.hostControlViewModel.Title = windowTitle;
      if (dispatcher == null)
        return;
        Action action = delegate
        {
            if (this.hostWindow.WindowState == WindowState.Minimized)
                this.hostWindow.WindowState = WindowState.Normal;
            this.hostWindow.Show();
            this.hostWindow.Activate();
        };
      dispatcher.Invoke(action, new object[0]);
    }

    public void ShowHostWindow(Tour tour, string windowTitle, bool playTour = false, bool currentTour = false, string defaultTourName = "")
    {
      Dispatcher dispatcher = Dispatcher.FromThread(this.visualizationThread);
      if (dispatcher == null)
        return;
        Action action = delegate
        {
            if (this.hostWindow != null)
            {
                if (this.hostControlViewModel.Dialog != null)
                {
                    if (this.hostWindow.WindowState == WindowState.Minimized)
                        this.hostWindow.WindowState = WindowState.Normal;
                    this.hostWindow.Show();
                    this.hostWindow.Activate();
                    return;
                }
                else
                    this.hostControlViewModel.CloseWindow(true);
            }
            this.hostControlViewModel.Title = windowTitle;
            if (currentTour)
                tour = this.hostControlViewModel.Model.CurrentTour;
            if (tour != null)
                this.hostControlViewModel.SetTour(tour);
            else
                this.hostControlViewModel.CreateNewTour(defaultTourName);
            if (this.hostWindow == null)
            {
                this.hostWindow = new ChromelessWindowBase();
                this.hostWindow.FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
                this.hostWindow.Closing +=
                    (CancelEventHandler) ((s, e) => this.hostWindow = (ChromelessWindowBase) null);
                this.hostWindow.Closed += (EventHandler) ((s, e) =>
                {
                    if (this.OnHostWindowClosed == null)
                        return;
                    this.OnHostWindowClosed((object) this, new EventArgs());
                });
                this.hostWindow.DataContext = (object) this.hostControlViewModel;
                this.hostWindow.WindowState = WindowState.Maximized;
            }
            else if (this.hostWindow.WindowState == WindowState.Minimized)
                this.hostWindow.WindowState = WindowState.Normal;
            this.hostControlViewModel.BeforeShow();
            this.hostWindow.Show();
            this.hostWindow.Activate();
            if (this.OnHostWindowOpened == null)
                return;
            this.OnHostWindowOpened((object) this, new EventArgs());
        };
      dispatcher.Invoke(action, new object[0]);
      if (this.hostWindow == null || tour == null || !playTour)
        return;
        Action action1 = delegate
        {
            this.hostControlViewModel.OnExecutePlayTour();
        };
      dispatcher.Invoke(action1, new object[0]);
    }

    public void SetHostWindowTitle(string windowTitle)
    {
      if (this.hostWindow == null)
        return;
      Dispatcher dispatcher = Dispatcher.FromThread(this.visualizationThread);
      if (dispatcher == null)
        return;
        Action action = delegate
        {
            this.hostControlViewModel.Title = windowTitle;
        };
        dispatcher.Invoke(action, new object[0]);
    }

    public void CloseHostWindow(bool saveContent)
    {
      if (this.hostWindow == null)
        return;
      Dispatcher dispatcher = Dispatcher.FromThread(this.visualizationThread);
      if (dispatcher == null)
        return;
        Action action = delegate
        {
            if (this.hostControlViewModel == null)
                return;
            this.hostControlViewModel.CloseWindow(saveContent);
        };
        dispatcher.Invoke(action, new object[0]);
    }

    public void SaveHostWindow()
    {
      if (this.hostWindow == null)
        return;
      Dispatcher dispatcher = Dispatcher.FromThread(this.visualizationThread);
      if (dispatcher == null)
        return;
        Action action = delegate
        {
            if (this.hostControlViewModel == null || this.hostControlViewModel.TourEditor == null)
                return;
            this.hostControlViewModel.TourEditor.UpdateSelectedScene();
        };
      dispatcher.Invoke(action, new object[0]);
    }

    public void FetchModelMetadata()
    {
      if (this.visualizationModel == null)
        return;
      this.visualizationModel.GetModelMetadata();
    }

    public LayerDefinition AddLayerDefinition(bool addedToModel, string modelTableName, string layerName = null)
    {
      if (this.visualizationModel != null)
      {
        Dispatcher dispatcher = Dispatcher.FromThread(this.visualizationThread);
        bool noLayers = this.visualizationModel.LayerManager.LayerDefinitions.Count == 0;
        if (addedToModel || noLayers)
        {
          ModelMetadata modelMetadata = this.visualizationModel.GetModelMetadata();
          if (dispatcher != null)
          {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "ClientProxy.AddLayerDefinition(): dispatching AddLayerDef");
              AddLayerDefinitionDelegate method = delegate
              {
                  VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0,
                      "ClientProxy.AddLayerDefinition(): in dispatcher");
                  return this.visualizationModel.LayerManager.AddLayerDefinition(
                      layerName,
                      modelTableName,
                      true,
                      !addedToModel && noLayers &&
                      !Enumerable.Any<TableIsland>(
                          (IEnumerable<TableIsland>) modelMetadata.TableIslands,
                          (Func<TableIsland, bool>) (
                              island => Enumerable.Any<TableMetadata>((IEnumerable<TableMetadata>) island.Tables,
                                  (Func<TableMetadata, bool>) (table => table.Visible)))));
              };
              return (LayerDefinition) dispatcher.Invoke(method, new object[0]);
          }
        }
      }
      return (LayerDefinition) null;
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
      if (disposing && this.visualizationThread != null)
      {
        Dispatcher dispatcher = Dispatcher.FromThread(this.visualizationThread);
        if (dispatcher != null)
        {
            Action action = delegate
            {
                if (this.hostWindow != null)
                {
                    if (this.hostControlViewModel != null)
                        this.hostControlViewModel.CloseWindow(true);
                    this.hostWindow.DataContext = (object) null;
                }
                if (this.hostControlViewModel != null)
                    this.hostControlViewModel.Dispose();
                if (this.visualizationModel == null)
                    return;
                this.visualizationModel.OnInternalError -=
                    new InternalErrorEventHandler(this.modelOnInternalError.OnEvent);
                this.visualizationModel.Reset();
                this.visualizationModel.Dispose();
            };
          dispatcher.Invoke(action, new object[0]);
          dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }
      }
      this.hostWindow = (ChromelessWindowBase) null;
      this.visualizationModel = (VisualizationModel) null;
      this.visualizationThread = (Thread) null;
      this.disposed = true;
    }

    private static void DispatcherUnhandledException(ClientProxy proxy, object sender, DispatcherUnhandledExceptionEventArgs args)
    {
      try
      {
        args.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
        args.Handled = true;
        Exception exception = args.Exception;
        COMException comException = exception as COMException;
        if (comException != null && comException.ErrorCode == -2003303418)
          exception = (Exception) new OutOfMemoryException("Error rendering 3D image on map", exception);
        VisualizationTraceSource.Current.Fail(exception);
        if (proxy.OnInternalError == null)
          return;
        proxy.OnInternalError((object) proxy, new InternalErrorEventArgs(exception));
      }
      catch (Exception ex)
      {
      }
    }

    private delegate LayerDefinition AddLayerDefinitionDelegate();

    private class VisualizationThreadStartContext
    {
      public EventWaitHandle VisualizationThreadRunning { get; set; }

      public List<Color4F> CustomColors { get; set; }
    }
  }
}
