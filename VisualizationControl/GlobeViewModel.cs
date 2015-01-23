using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class GlobeViewModel : ViewModelBase, IDisposable
  {
    private bool firstFrame = true;
    private readonly object syncLock = new object();
    private Brush copyrightTextColor = (Brush) Brushes.Black;
    private bool isLogoVisible = true;
    private bool isContentSourceAvailable = true;
    private bool isContentLogoVisible = true;
    private DateTime mPreviousFrameUpdateEnd = DateTime.UtcNow;
    private TimeSpan mPreviousFrameUpdateLength = new TimeSpan(0, 0, 0, 0, 2);
    private DateTime mPreviousRenderUpdate = DateTime.UtcNow;
    private TimeSpan mPreviousUpdateLength = new TimeSpan(0, 0, 0, 0, 2);
    private DateTime mLastProfileUpdateTime = DateTime.UtcNow;
    private const double dpi = 96.0;
    private readonly VisualizationModel visualizationModel;
    private readonly VisualizationEngine visualizationEngine;
    private EventHandler renderHandler;
    private volatile bool disposed;
    private string engineProfile;
    private bool engineProfileVisible;
    private bool useAerial;
    private string bingLogoUrl;
    private string combinedLogoUrl;
    private string bingLogoAerialUrl;
    private string combinedLogoAerialUrl;
    private int token;
    private readonly WeakEventListener<GlobeViewModel, object, EventArgs> inputHandlerOnTapAndHoldLeave;
    private readonly WeakEventListener<GlobeViewModel, object, EventArgs> inputHandlerOnTouchDown;
    private readonly WeakEventListener<GlobeViewModel, object, DependencyPropertyChangedEventArgs> frontBufferChanged;
    private readonly WeakEventListener<GlobeViewModel, object, CameraIdleEventArgs> onCameraIdle;
    private readonly WeakEventListener<GlobeViewModel, object, TourSceneStateChangedEventArgs> onSceneStateChanged;
    private readonly WeakEventListener<GlobeViewModel, BuiltinTheme, VisualizationTheme, bool> onThemeChanged;
    private string logoUrl;
    private string copyright;
    private bool _showContextMenu;
    private Dictionary<int, TileExtent> currentTileExtents;
    private int _copyrightRequestFailedCount;
    private RenderTarget mRecentRenderTarget;

    public string PropertyLogoUrl
    {
      get
      {
        return "LogoUrl";
      }
    }

    public string LogoUrl
    {
      get
      {
        return this.logoUrl;
      }
      set
      {
        this.SetProperty<string>(this.PropertyLogoUrl, ref this.logoUrl, value, false);
      }
    }

    public string PropertyCopyrightText
    {
      get
      {
        return "CopyrightText";
      }
    }

    public string CopyrightText
    {
      get
      {
        return this.copyright;
      }
      set
      {
        this.SetProperty<string>(this.PropertyCopyrightText, ref this.copyright, value, false);
      }
    }

    public string PropertyCopyrightTextColor
    {
      get
      {
        return "CopyrightTextColor";
      }
    }

    public Brush CopyrightTextColor
    {
      get
      {
        return this.copyrightTextColor;
      }
      set
      {
        this.SetProperty<Brush>(this.PropertyCopyrightTextColor, ref this.copyrightTextColor, value, false);
      }
    }

    public string PropertyEngineProfile
    {
      get
      {
        return "EngineProfile";
      }
    }

    public string EngineProfile
    {
      get
      {
        return this.engineProfile;
      }
      set
      {
        this.SetProperty<string>(this.PropertyEngineProfile, ref this.engineProfile, value, false);
      }
    }

    public string PropertyEngineProfileVisible
    {
      get
      {
        return "EngineProfileVisible";
      }
    }

    public bool EngineProfileVisible
    {
      get
      {
        return this.engineProfileVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyEngineProfileVisible, ref this.engineProfileVisible, value, false);
      }
    }

    public string PropertyIsLogoVisible
    {
      get
      {
        return "IsLogoVisible";
      }
    }

    public bool IsLogoVisible
    {
      get
      {
        return this.isLogoVisible;
      }
      set
      {
        if (!this.SetProperty<bool>(this.PropertyIsLogoVisible, ref this.isLogoVisible, value, false))
          return;
        this.IsContentLogoVisible = this.IsLogoVisible && this.IsContentSourceAvailable;
      }
    }

    public string PropertyIsContentSourceAvailable
    {
      get
      {
        return "IsContentSourceAvailable";
      }
    }

    public bool IsContentSourceAvailable
    {
      get
      {
        return this.isContentSourceAvailable;
      }
      set
      {
        if (!this.SetProperty<bool>(this.PropertyIsContentSourceAvailable, ref this.isContentSourceAvailable, value, false))
          return;
        this.IsContentLogoVisible = this.IsLogoVisible && this.IsContentSourceAvailable;
      }
    }

    public string PropertyIsContentLogoVisible
    {
      get
      {
        return "IsContentLogoVisible";
      }
    }

    public bool IsContentLogoVisible
    {
      get
      {
        return this.isContentLogoVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyIsContentLogoVisible, ref this.isContentLogoVisible, value, false);
      }
    }

    public static string PropertyShowContextMenu
    {
      get
      {
        return "ShowContextMenu";
      }
    }

    public bool ShowContextMenu
    {
      get
      {
        return this._showContextMenu;
      }
      set
      {
        this.SetProperty<bool>(GlobeViewModel.PropertyShowContextMenu, ref this._showContextMenu, value, false);
      }
    }

    public string PropertyCopyrightRequestFailedCount
    {
      get
      {
        return "CopyrightRequestFailedCount";
      }
    }

    public int CopyrightRequestFailedCount
    {
      get
      {
        return this._copyrightRequestFailedCount;
      }
      set
      {
        this.SetProperty<int>(this.PropertyCopyrightRequestFailedCount, ref this._copyrightRequestFailedCount, value, false);
      }
    }

    public BitmapSource RecordingImage { get; private set; }

    private D3DImage11 D3dImageBack { get; set; }

    public event GlobeViewModel.D3DImageUpdatedEvent OnD3DImageUpdated;

    public GlobeViewModel(VisualizationModel visualizationModel, BingMapResourceUri mapResourceUri)
    {
      if (visualizationModel == null)
        throw new ArgumentNullException("visualizationModel");
      if (visualizationModel.Engine == null)
        throw new ArgumentException("A valid Visualization Engine must be set on the Visualization Model");
      this.visualizationModel = visualizationModel;
      this.visualizationEngine = visualizationModel.Engine;
      if (this.visualizationEngine.IsOfflineModeEnabled)
      {
        this.RecordingImage = (BitmapSource) new WriteableBitmap(this.visualizationEngine.ScreenWidth, this.visualizationEngine.ScreenHeight, 96.0, 96.0, PixelFormats.Pbgra32, (BitmapPalette) null);
      }
      else
      {
        this.mRecentRenderTarget = (RenderTarget) null;
        this.D3dImageBack = new D3DImage11();
        this.renderHandler = new EventHandler(this.CompositionTarget_Rendering);
        this.frontBufferChanged = new WeakEventListener<GlobeViewModel, object, DependencyPropertyChangedEventArgs>(this)
        {
          OnEventAction = new Action<GlobeViewModel, object, DependencyPropertyChangedEventArgs>(GlobeViewModel.d3dImage_IsFrontBufferAvailableChanged)
        };
        this.D3dImageBack.IsFrontBufferAvailableChanged += new DependencyPropertyChangedEventHandler(this.frontBufferChanged.OnEvent);
      }
      this.visualizationEngine.MoveCamera(this.InitialCameraPosition(), CameraMoveStyle.JumpTo, false);
      this.bingLogoUrl = mapResourceUri.BingLogoUrl;
      this.combinedLogoUrl = mapResourceUri.CombinedLogoUrl;
      this.bingLogoAerialUrl = mapResourceUri.BingLogoAerialUrl;
      this.combinedLogoAerialUrl = mapResourceUri.CombinedLogoAerialUrl;
      this.logoUrl = this.bingLogoUrl;
      this.inputHandlerOnTapAndHoldLeave = new WeakEventListener<GlobeViewModel, object, EventArgs>(this)
      {
        OnEventAction = new Action<GlobeViewModel, object, EventArgs>(GlobeViewModel.inputHandler_OnTapAndHoldLeave)
      };
      this.inputHandlerOnTouchDown = new WeakEventListener<GlobeViewModel, object, EventArgs>(this)
      {
        OnEventAction = new Action<GlobeViewModel, object, EventArgs>(GlobeViewModel.inputHandler_OnTouchDown)
      };
      this.onCameraIdle = new WeakEventListener<GlobeViewModel, object, CameraIdleEventArgs>(this)
      {
        OnEventAction = new Action<GlobeViewModel, object, CameraIdleEventArgs>(GlobeViewModel.VisualizationEngineOnCameraIdle)
      };
      this.onSceneStateChanged = new WeakEventListener<GlobeViewModel, object, TourSceneStateChangedEventArgs>(this)
      {
        OnEventAction = new Action<GlobeViewModel, object, TourSceneStateChangedEventArgs>(GlobeViewModel.VisualizationEngineOnTourSceneStateChanged)
      };
      this.onThemeChanged = new WeakEventListener<GlobeViewModel, BuiltinTheme, VisualizationTheme, bool>(this)
      {
        OnEventAction = new Action<GlobeViewModel, BuiltinTheme, VisualizationTheme, bool>(GlobeViewModel.VisualizationEngineOnThemeChanged)
      };
      this.visualizationEngine.CameraIdle += new CameraIdleEventHandler(this.onCameraIdle.OnEvent);
      this.visualizationEngine.ThemeChanged += new Action<BuiltinTheme, VisualizationTheme, bool>(this.onThemeChanged.OnEvent);
      this.visualizationEngine.TourSceneStateChanged += new TourSceneStateChangeHandler(this.onSceneStateChanged.OnEvent);
      this.EngineProfileVisible = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.ShowProfilerInfo;
      this.visualizationEngine.FrameProfileEnabled = this.EngineProfileVisible;
      if (this.visualizationEngine.IsOfflineModeEnabled)
        return;
      this.visualizationEngine.RequestFrame();
      this.visualizationEngine.RequestFrame();
      this.visualizationEngine.RequestFrame();
    }

    public bool AdvanceRecordingFrame()
    {
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      ReadableBitmap target = (ReadableBitmap) null;
      WriteableBitmap writeableBitmap = (WriteableBitmap) this.RecordingImage;
      try
      {
        this.visualizationEngine.RequestFrame();
        if (this.visualizationEngine.TryGetFrame(out target))
        {
          int pitch;
          IntPtr buffer = target.LockDataImmediate(out pitch);
          flag3 = true;
          writeableBitmap.Lock();
          flag2 = true;
          Int32Rect int32Rect = new Int32Rect(0, 0, target.Width, target.Height);
          writeableBitmap.WritePixels(int32Rect, buffer, target.Height * pitch, pitch);
          writeableBitmap.AddDirtyRect(int32Rect);
          this.UpdateGlobeOverlays();
          if (this.EngineProfileVisible)
            this.UpdateEngineProfile();
          flag1 = true;
        }
      }
      finally
      {
        if (flag2)
          writeableBitmap.Unlock();
        if (flag3)
          target.Unlock();
        this.visualizationEngine.ReleaseFrame();
      }
      return flag1;
    }

    public void SetGlobeInputElement(UIElement inputElement)
    {
      if (inputElement == null)
      {
        this.visualizationEngine.SetInputHandler((InputHandler) null);
      }
      else
      {
        WPFInputHandler wpfInputHandler = new WPFInputHandler(inputElement);
        wpfInputHandler.OnTapAndHoldLeave += new EventHandler(this.inputHandlerOnTapAndHoldLeave.OnEvent);
        wpfInputHandler.OnTouchDown += new EventHandler(this.inputHandlerOnTouchDown.OnEvent);
        this.visualizationEngine.SetInputHandler((InputHandler) wpfInputHandler);
      }
    }

    private static void inputHandler_OnTouchDown(GlobeViewModel viewModel, object sender, EventArgs e)
    {
      viewModel.ShowContextMenu = false;
    }

    private static void inputHandler_OnTapAndHoldLeave(GlobeViewModel viewModel, object sender, EventArgs e)
    {
      viewModel.ShowContextMenu = true;
    }

    public void UpdateFrameSize(int width, int height)
    {
      if (this.RecordingImage != null)
        return;
      this.visualizationEngine.UpdateFrameSize(width, height);
    }

    private void SetAndUpdateBackBuffer(D3DImage11 buffer, RenderTarget rt)
    {
      lock (this.syncLock)
      {
        try
        {
          buffer.Lock();
          if (!buffer.SetBackBuffer(rt))
            return;
          try
          {
            if (rt == null || rt.RenderTargetTexture == null)
              return;
            buffer.AddDirtyRect(new Int32Rect()
            {
              X = 0,
              Y = 0,
              Width = rt.RenderTargetTexture.Width,
              Height = rt.RenderTargetTexture.Height
            });
          }
          catch (ArgumentOutOfRangeException exception_0)
          {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Error while setting the D3DImage dirty rect: " + ((object) exception_0).ToString());
          }
        }
        finally
        {
          buffer.Unlock();
        }
      }
    }

    private void CompositionTarget_Rendering(object sender, EventArgs e)
    {
      this.UpdateImageBuffer();
    }

    private void UpdateImageBuffer()
    {
      if (this.disposed)
        return;
      DateTime utcNow = DateTime.UtcNow;
      double totalMilliseconds = utcNow.Subtract(this.mPreviousRenderUpdate).TotalMilliseconds;
      if ((totalMilliseconds < 5.0 || totalMilliseconds < Math.Min(this.mPreviousUpdateLength.TotalMilliseconds, 100.0)) && !this.visualizationEngine.IsOfflineModeEnabled)
        return;
      this.mPreviousRenderUpdate = utcNow;
      lock (this.syncLock)
      {
        if (this.disposed)
          return;
        if (!this.firstFrame)
          this.visualizationEngine.ReleaseFrame();
        RenderTarget local_3;
        bool local_2_1 = this.visualizationEngine.TryGetFrame(out local_3);
        this.firstFrame = false;
        bool local_4 = local_3 != this.mRecentRenderTarget;
        if (local_4)
        {
          this.mRecentRenderTarget = local_3;
          if (this.mRecentRenderTarget != null)
            this.mRecentRenderTarget.CustomRenderTargetDispose = (RenderTarget.CustomRenderTargetDisposeCallback) ((oldRt, finishDispose) => DispatcherExtensions.CheckedInvoke(this.visualizationModel.UIDispatcher, (Action) (() =>
            {
              this.D3dImageBack.EnsureNotUsing(oldRt);
              this.visualizationEngine.RunOnRenderThread((RenderThreadMethod) (() => finishDispose()));
            }), true));
        }
        if (local_2_1 || local_4)
          this.SetAndUpdateBackBuffer(this.D3dImageBack, local_3);
        if (local_2_1 && this.OnD3DImageUpdated != null)
          this.OnD3DImageUpdated(this.D3dImageBack);
        this.UpdateGlobeOverlays();
        if (this.EngineProfileVisible)
          this.UpdateEngineProfile();
        this.visualizationEngine.RequestFrame();
        if (!local_2_1)
          return;
        DateTime local_5 = DateTime.UtcNow;
        this.mPreviousUpdateLength = local_5.Subtract(utcNow);
        this.mPreviousRenderUpdate = local_5;
        this.mPreviousFrameUpdateLength = local_5.Subtract(this.mPreviousFrameUpdateEnd);
        this.mPreviousFrameUpdateEnd = local_5;
      }
    }

    private void UpdateEngineProfile()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(string.Format("Request FPS: {0} Rendered FPS: {1} Current Frame: {2} Graphics Level: {3}\r\n\r\n", (object) this.visualizationEngine.RequestedFramesPerSecond, (object) this.visualizationEngine.RenderedFramesPerSecond, (object) this.visualizationEngine.ElapsedFrames, (object) ((object) this.visualizationEngine.GraphicsLevel).ToString()));
      DateTime now = DateTime.Now;
      double totalMilliseconds1 = now.Subtract(this.mLastProfileUpdateTime).TotalMilliseconds;
      this.mLastProfileUpdateTime = now;
      stringBuilder.Append(string.Format("WPF Update FPS/MS: {0} at {1}ms\r\n", (object) (1000.0 / totalMilliseconds1), (object) totalMilliseconds1));
      double totalMilliseconds2 = this.mPreviousFrameUpdateLength.TotalMilliseconds;
      stringBuilder.Append(string.Format("Frame Update Speed: {0} at {1}ms\r\n", (object) (1000.0 / totalMilliseconds2), (object) totalMilliseconds2));
      double totalMilliseconds3 = this.mPreviousUpdateLength.TotalMilliseconds;
      stringBuilder.Append(string.Format("Frame Cost Time: {0} at {1}ms\r\n", (object) (1000.0 / totalMilliseconds3), (object) totalMilliseconds3));
      stringBuilder.Append(string.Format("Width/Height in Pixels {0}*{1} = {2}\r\n", (object) this.visualizationEngine.ScreenWidth, (object) this.visualizationEngine.ScreenHeight, (object) (this.visualizationEngine.ScreenWidth * this.visualizationEngine.ScreenHeight)));
      stringBuilder.Append(string.Format("Resource count: {0}\r\n", (object) this.visualizationEngine.ResourceCount));
      stringBuilder.Append(string.Format("Resource GPU bytes: {0}\r\n", (object) this.visualizationEngine.EstimatedVideoMemoryUsage));
      stringBuilder.Append("\r\n");
      ProfileResult[] averageFrameProfile = this.visualizationEngine.AverageFrameProfile;
      if (averageFrameProfile != null)
      {
        foreach (ProfileResult profileResult in averageFrameProfile)
        {
          for (int index = 0; index < profileResult.Level; ++index)
            stringBuilder.Append("   ");
          stringBuilder.Append(string.Format("{0}: {1} ms\r\n", (object) profileResult.Name, !profileResult.Duration.HasValue ? (object) "n/a" : (object) profileResult.Duration.Value.ToString("0.##")));
        }
      }
      stringBuilder.Append("\r\n");
      if (this.visualizationEngine.RegionStatistics != null)
      {
        stringBuilder.Append(this.visualizationEngine.RegionStatistics.ToString());
        stringBuilder.Append("\r\n");
      }
      stringBuilder.Append(this.visualizationEngine.TileStatistics.ToString());
      this.EngineProfile = ((object) stringBuilder).ToString();
    }

    private static void d3dImage_IsFrontBufferAvailableChanged(GlobeViewModel viewModel, object sender, DependencyPropertyChangedEventArgs e)
    {
      if ((bool) e.NewValue)
        viewModel.ResumeRendering();
      else
        viewModel.StopRendering();
    }

    public void ResumeRendering()
    {
      if (this.disposed)
        return;
      this.visualizationEngine.ForceUpdate();
      CompositionTarget.Rendering += this.renderHandler;
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Resume rendering -- a new D3DImage's frontbuffer is available.");
    }

    public void StopRendering()
    {
      if (this.disposed)
        return;
      if (Process.GetCurrentProcess().SessionId != 0)
      {
        this.D3dImageBack.Invalidate();
        this.visualizationEngine.ForceUpdate();
        this.CompositionTarget_Rendering((object) this, new EventArgs());
      }
      else
      {
        CompositionTarget.Rendering -= this.renderHandler;
        this.D3dImageBack.Invalidate();
      }
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Stop rendering while the D3DImage's front buffer is not available.");
    }

    private void UpdateGlobeOverlays()
    {
      if (this.visualizationEngine == null)
        return;
      this.IsContentSourceAvailable = !this.visualizationEngine.IsUsingCustomMap_CheckWithoutUpdate();
    }

    public CameraSnapshot InitialCameraPosition()
    {
      TimeZoneInfo local = TimeZoneInfo.Local;
      switch (local.Id)
      {
        case "Alaskan Standard Time":
        case "Atlantic Standard Time":
        case "Central Standard Time":
        case "Canada Central Standard Time":
        case "Eastern Standard Time":
        case "US Eastern Standard Time":
        case "Hawaiian Standard Time":
        case "Mountain Standard Time":
        case "Newfoundland Standard Time":
        case "US Mountain Standard Time":
        case "Pacific Standard Time":
          return CameraSnapshot.FromDegreesLatLong(44.0, -93.0);
        default:
          return CameraSnapshot.FromUtcOffset(local.BaseUtcOffset);
      }
    }

    public void Reset()
    {
      this.visualizationEngine.SetCustomMap(CustomMap.InvalidMapId);
      this.visualizationEngine.SetTheme(this.visualizationEngine.DefaultTheme, false);
      this.visualizationEngine.Reset();
      this.visualizationEngine.MoveCamera(this.InitialCameraPosition(), CameraMoveStyle.JumpTo, false);
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
      lock (this.syncLock)
      {
        if (disposing)
        {
          if (this.OnD3DImageUpdated != null)
            this.OnD3DImageUpdated((D3DImage11) null);
          if (this.D3dImageBack != null || this.mRecentRenderTarget != null)
          {
            this.StopRendering();
            if (this.D3dImageBack != null)
            {
              this.D3dImageBack.IsFrontBufferAvailableChanged -= new DependencyPropertyChangedEventHandler(this.frontBufferChanged.OnEvent);
              this.D3dImageBack.Dispose();
            }
          }
          if (this.mRecentRenderTarget != null)
          {
            this.mRecentRenderTarget.CustomRenderTargetDispose = (RenderTarget.CustomRenderTargetDisposeCallback) null;
            this.mRecentRenderTarget = (RenderTarget) null;
          }
          if (this.visualizationEngine != null)
          {
            this.visualizationEngine.ThemeChanged -= new Action<BuiltinTheme, VisualizationTheme, bool>(this.onThemeChanged.OnEvent);
            this.visualizationEngine.CameraIdle -= new CameraIdleEventHandler(this.onCameraIdle.OnEvent);
            this.visualizationEngine.TourSceneStateChanged -= new TourSceneStateChangeHandler(this.onSceneStateChanged.OnEvent);
          }
        }
        this.D3dImageBack = (D3DImage11) null;
        this.mRecentRenderTarget = (RenderTarget) null;
        this.disposed = true;
      }
    }

    private static void VisualizationEngineOnTourSceneStateChanged(GlobeViewModel viewModel, object sender, TourSceneStateChangedEventArgs eventArgs)
    {
      if (eventArgs.TourSceneState != TourSceneState.InFrame)
        return;
      viewModel.FetchCopyright(eventArgs.TileExtents);
    }

    private static void VisualizationEngineOnCameraIdle(GlobeViewModel viewModel, object sender, CameraIdleEventArgs cameraIdleEventArgs)
    {
      viewModel.FetchCopyright(cameraIdleEventArgs.TileExtents);
    }

    private void AddCopyright(int currentToken, bool displayCombo, List<string> copyrights, int failureCount)
    {
      if (failureCount > 0)
        this.CopyrightRequestFailedCount += failureCount;
      else
        this.CopyrightRequestFailedCount = failureCount;
      if (currentToken != this.token)
        return;
      this.LogoUrl = displayCombo ? (!this.useAerial || string.IsNullOrEmpty(this.combinedLogoAerialUrl) ? this.combinedLogoUrl : this.combinedLogoAerialUrl) : (!this.useAerial || string.IsNullOrEmpty(this.bingLogoAerialUrl) ? this.bingLogoUrl : this.bingLogoAerialUrl);
      if (this.visualizationModel.LayerManager != null && this.visualizationModel.LayerManager.LayerDefinitions != null && Enumerable.Any<LayerDefinition>((IEnumerable<LayerDefinition>) this.visualizationModel.LayerManager.LayerDefinitions, (Func<LayerDefinition, bool>) (ld =>
      {
        if (ld.GeoVisualization != null && ld.GeoVisualization.VisualType == LayerType.RegionChart)
          return ld.GeoVisualization.Visible;
        else
          return false;
      })))
      {
        foreach (string str in this.visualizationModel.RegionProvider.Sources)
        {
          if (!copyrights.Contains(str))
            copyrights.Add(str);
        }
      }
      StringBuilder stringBuilder = new StringBuilder();
      copyrights.ForEach((Action<string>) (str => stringBuilder.AppendFormat("{0}  ", (object) str.Trim())));
      this.CopyrightText = ((object) stringBuilder).ToString();
    }

    private void FetchCopyright(Dictionary<int, TileExtent> tileExtents)
    {
      try
      {
        if (tileExtents == null || tileExtents.Count == 0)
          return;
        int token = Interlocked.Increment(ref this.token);
        ImagerySet? nullable1 = this.visualizationEngine.CurrentImageSet;
        if (!nullable1.HasValue)
          return;
        ImagerySet? nullable2 = nullable1;
        if ((nullable2.GetValueOrDefault() != ImagerySet.RoadWithoutLabels ? 0 : (nullable2.HasValue ? 1 : 0)) != 0)
          nullable1 = new ImagerySet?(ImagerySet.Road);
        if (token != this.token)
          return;
        this.currentTileExtents = tileExtents;
        CopyrightService.FetchCopyrightAsync(token, nullable1.ToString(), tileExtents, new Action<int, bool, List<string>, int>(this.AddCopyright));
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.Fail("Failed to fetch copyright notice.", ex);
      }
    }

    private static void VisualizationEngineOnThemeChanged(GlobeViewModel viewModel, BuiltinTheme builtinTheme, VisualizationTheme visualizationTheme, bool arg3)
    {
      viewModel.FetchCopyright(viewModel.currentTileExtents);
      switch (builtinTheme)
      {
        case BuiltinTheme.None:
        case BuiltinTheme.BingRoad:
        case BuiltinTheme.Light:
        case BuiltinTheme.Mono:
        case BuiltinTheme.White:
        case BuiltinTheme.Earthy:
        case BuiltinTheme.Modern:
        case BuiltinTheme.Organic:
          viewModel.useAerial = false;
          viewModel.CopyrightTextColor = (Brush) Brushes.Black;
          break;
        case BuiltinTheme.BingRoadHighContrast:
        case BuiltinTheme.Aerial:
        case BuiltinTheme.Grey:
        case BuiltinTheme.Dark:
        case BuiltinTheme.Radiate:
          viewModel.useAerial = true;
          viewModel.CopyrightTextColor = (Brush) Brushes.White;
          break;
      }
    }

    public delegate void D3DImageUpdatedEvent(D3DImage11 latestFront);
  }
}
