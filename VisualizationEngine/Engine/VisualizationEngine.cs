using Microsoft.Data.Visualization.DirectX;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using System.Xml;

namespace Microsoft.Data.Visualization.Engine
{
    public class VisualizationEngine : IDisposable, IVisualizationEngineDispatcher
    {
        public BuiltinTheme DefaultTheme = BuiltinTheme.BingRoad;
        private Color4F clearColor = new Color4F(1f, 0.95f, 0.95f, 0.95f);
        private List<RenderThreadMethod> renderThreadDispatcherCalls = new List<RenderThreadMethod>();
        private object renderThreadDispatcherLock = new object();
        private List<Exception> renderExceptions = new List<Exception>();
        private KeyValuePair<Guid, CustomMap> mCachedCustomMap = new KeyValuePair<Guid, CustomMap>(CustomMap.InvalidMapId, (CustomMap)null);
        public const string BingKey = "AutmxuJvVVVQyluwfF-Le9A6WQ_ypucXcJbzx5Rwf5u8on47kJRDu19BzV4kZlq9";
        private const int FixedFrameWaitTime = 50;
        private const int MaxRenderThreadShutdownTime = 4000;
        private const int MaxExceptionFrames = 30;
        private const int MaxPendingFrameRequests = 6;
        private const int resizeTimeout = 250;
        private const int minimumFrameSize = 8;
        private Renderer renderer;
        private Thread renderThread;
        private Dispatcher dispatcher;
        private ICustomMapProvider customMapsProvider;
        private SemaphoreSlim requestRenderSemaphore;
        private EventWaitHandle shutdownEvent;
        private Timer resizeTimer;
        private volatile bool running;
        private bool disposed;
        private bool frameSizeUpdated;
        private int forceRenderingFrameCount;
        private bool themeUpdated;
        private GraphicsLevel graphicsLevel;
        private readonly int fixedFramerate;
        private FixedTimeProvider fixedTimeProvider;
        private bool useSwapChain;
        private FullScreenQuadViewer fullScreenViewer;
        private IntPtr windowHandle;
        private InputHandler inputHandler;
        private BuiltinThemes themes;
        private EngineStep[] steps;
        private int[] stepIds;
        private TimeStep timeStep;
        private HitTestStep hitTestStep;
        private ShadowStep shadowStep;
        private LightingStep lightingStep;
        private GlobeStep globeStep;
        private LayerStep layerStep;
        private BackgroundStep backgroundStep;
        private InputStep inputStep;
        private CameraStep cameraStep;
        private TourStep tourStep;
        private AnnotationStep annotationStep;
        private AntialiasingStep antialiasingStep;
        private SceneState sceneState;
        private int screenWidth;
        private int screenHeight;
        private long lastTimestamp;
        private int requestedFrameCount;
        private int renderedFrameCount;
        private ReadableBitmap targetBitmap;

        private EventWaitHandle InitializedEvent { get; set; }

        public BuiltinTheme CurrentTheme { get; private set; }

        public bool CurrentThemeWithLabels { get; private set; }

        public Guid CurrentCustomMapId { get; private set; }

        public CustomMap CurrentCustomMap
        {
            get
            {
                if (this.mCachedCustomMap.Key == this.CurrentCustomMapId)
                    return this.mCachedCustomMap.Value;
                Guid currentCustomMapId = this.CurrentCustomMapId;
                if (currentCustomMapId == CustomMap.InvalidMapId)
                {
                    this.mCachedCustomMap = new KeyValuePair<Guid, CustomMap>(currentCustomMapId, (CustomMap)null);
                }
                else
                {
                    CustomMap orCreateMapFromId = this.customMapsProvider.MapCollection.FindOrCreateMapFromId(currentCustomMapId);
                    this.mCachedCustomMap = new KeyValuePair<Guid, CustomMap>(currentCustomMapId, orCreateMapFromId);
                }
                return this.mCachedCustomMap.Value;
            }
        }

        public ImagerySet? CurrentImageSet
        {
            get
            {
                ImageSet imageSet = this.themes.GetImageSet(this.CurrentTheme, this.CurrentThemeWithLabels);
                if (imageSet == null)
                    return new ImagerySet?();
                else
                    return new ImagerySet?(imageSet.ImagerySet);
            }
        }

        public VisualizationTheme CurrentVisualizationTheme { get; private set; }

        public GraphicsLevel GraphicsLevel
        {
            get
            {
                return this.graphicsLevel;
            }
            set
            {
                if (this.dispatcher != null)
                    this.dispatcher.BeginInvoke((Action)(() =>
                  {
                      if (value == this.graphicsLevel)
                          return;
                      this.graphicsLevel = value;
                      this.renderer.Msaa = this.graphicsLevel == GraphicsLevel.Quality;
                  }));
                else
                    this.graphicsLevel = value;
            }
        }

        public int RequestedFramesPerSecond { get; private set; }

        public int RenderedFramesPerSecond { get; private set; }

        public int ElapsedFrames
        {
            get
            {
                return this.renderer.FrameCount;
            }
        }

        public bool FrameProfileEnabled
        {
            get
            {
                this.DisposedCheck();
                return this.renderer.Profiler.Enabled;
            }
            set
            {
                this.DisposedCheck();
                this.renderer.Profiler.Enabled = value;
            }
        }

        public ProfileResult[] FrameProfile
        {
            get
            {
                this.DisposedCheck();
                return this.renderer.Profiler.FrameProfile;
            }
        }

        public ProfileResult[] AverageFrameProfile
        {
            get
            {
                this.DisposedCheck();
                return this.renderer.Profiler.AverageFrameProfile;
            }
        }

        public int ResourceCount
        {
            get
            {
                this.DisposedCheck();
                return this.renderer.Resources.Count;
            }
        }

        public long EstimatedSystemMemoryUsage
        {
            get
            {
                this.DisposedCheck();
                return this.renderer.Resources.EstimatedSystemMemoryUsage;
            }
        }

        public long EstimatedVideoMemoryUsage
        {
            get
            {
                this.DisposedCheck();
                return this.renderer.Resources.EstimatedVideoMemoryUsage;
            }
        }

        public TileCacheStatistics TileStatistics
        {
            get
            {
                this.DisposedCheck();
                return this.globeStep.TileStatistics;
            }
        }

        public RegionLayerStatistics RegionStatistics
        {
            get
            {
                this.DisposedCheck();
                foreach (Layer layer in (IEnumerable<Layer>)this.layerStep.Layers)
                {
                    RegionLayer regionLayer = layer as RegionLayer;
                    if (regionLayer != null)
                        return regionLayer.Stats;
                }
                return (RegionLayerStatistics)null;
            }
        }

        public IList<Layer> Layers
        {
            get
            {
                this.DisposedCheck();
                return this.layerStep.Layers;
            }
        }

        public bool FlatMode
        {
            get
            {
                return this.cameraStep.FlatMode;
            }
            set
            {
                this.cameraStep.FlatMode = value;
            }
        }

        public double FlatModeTransitionTime
        {
            set
            {
                this.cameraStep.FlatModeTransitionTime = value;
            }
        }

        public ITimeController TimeControl
        {
            get
            {
                this.DisposedCheck();
                return (ITimeController)this.timeStep;
            }
        }

        public ITourPlayer TourPlayer
        {
            get
            {
                this.DisposedCheck();
                return (ITourPlayer)this.tourStep;
            }
        }

        public bool IsOfflineModeEnabled
        {
            get
            {
                return this.fixedFramerate > 0;
            }
        }

        public int ScreenWidth
        {
            get
            {
                return this.screenWidth;
            }
        }

        public int ScreenHeight
        {
            get
            {
                return this.screenHeight;
            }
        }

        public event InternalErrorEventHandler OnInternalError;

        public event Action<BuiltinTheme, VisualizationTheme, bool> ThemeChanged;

        public event Action<CameraSnapshot> CameraMoveStarted;

        public event Action<CameraSnapshot> CameraMoveEnded;

        public event CameraIdleEventHandler CameraIdle;

        public event TourSceneStateChangeHandler TourSceneStateChanged;

        public event Action<CameraSnapshot> CameraTargetChanged
        {
            add
            {
                this.cameraStep.DefaultController.CameraTargetChanged += value;
            }
            remove
            {
                this.cameraStep.DefaultController.CameraTargetChanged -= value;
            }
        }

        public event Action<bool> FlatModeChanged
        {
            add
            {
                this.cameraStep.FlatModeChanged += value;
            }
            remove
            {
                this.cameraStep.FlatModeChanged -= value;
            }
        }

        public VisualizationEngine(int renderTargetWidth, int renderTargetHeight, Dispatcher eventDispatcher, ICustomMapProvider mapsList, BingMapResourceUri bingMapResourceUri, int framerate)
            : this(renderTargetWidth, renderTargetHeight, IntPtr.Zero, (InputHandler)null, eventDispatcher, mapsList, GraphicsLevel.Quality, false, bingMapResourceUri, framerate)
        {
        }

        public VisualizationEngine(int renderTargetWidth, int renderTargetHeight, IntPtr handle, InputHandler input, Dispatcher eventDispatcher, ICustomMapProvider engCustomMapsProvider, GraphicsLevel graphicsQualityLevel, bool debugScreen = false, BingMapResourceUri bingMapResourceUri = null, int framerate = 0)
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Initializing visualization engine. Width: {0}, Height: {1}", (object)renderTargetWidth, (object)renderTargetHeight);
            this.themes = new BuiltinThemes(bingMapResourceUri);
            if (renderTargetWidth <= 0)
                renderTargetWidth = 8;
            if (renderTargetHeight <= 0)
                renderTargetHeight = 8;
            this.graphicsLevel = graphicsQualityLevel;
            this.fixedFramerate = framerate;
            this.screenWidth = renderTargetWidth;
            this.screenHeight = renderTargetHeight;
            this.windowHandle = handle;
            this.useSwapChain = debugScreen;
            this.inputHandler = input;
            this.dispatcher = eventDispatcher;
            this.customMapsProvider = engCustomMapsProvider;
            this.requestRenderSemaphore = new SemaphoreSlim(1, 6);
            this.InitializedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.shutdownEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.renderThread = new Thread(new ThreadStart(this.RenderLoop));
            this.renderThread.Name = "RenderThread";
            this.running = true;
            this.renderThread.Start();
            this.InitializedEvent.WaitOne();
            if (!this.running)
                throw new VisualizationEngineException();
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Visualization engine successfully initialized.");
        }

        public void Reset()
        {
            this.cameraStep.Reset();
        }

        public bool IsUsingCustomMap_CheckWithoutUpdate()
        {
            return this.mCachedCustomMap.Value != null;
        }

        private bool InitializeEngine()
        {
            this.renderer = Renderer.Create();
            this.renderer.OnInformation += new RendererInfoEventHander(this.renderer_OnInformation);
            this.renderer.OnInternalError += new RendererErrorEventHandler(this.renderer_OnInternalError);
            if (!(!(this.windowHandle == IntPtr.Zero) ? this.renderer.Initialize(this.windowHandle, this.screenWidth, this.screenHeight, this.GraphicsLevel == GraphicsLevel.Quality, this.useSwapChain) : this.renderer.Initialize(this.screenWidth, this.screenHeight, this.GraphicsLevel == GraphicsLevel.Quality, this.IsOfflineModeEnabled)))
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Critical, 0, "Renderer initialization failed. Render target width: {0} height: {1}. A driver update may be required.", (object)this.screenWidth, (object)this.screenHeight);
                return false;
            }
            else
            {
                if (this.useSwapChain)
                {
                    if (this.fullScreenViewer == null)
                        this.fullScreenViewer = new FullScreenQuadViewer();
                    this.renderer.OnPresent += new RendererPresentEventHandler(this.renderer_OnPresent);
                }
                this.forceRenderingFrameCount = this.renderer.FrameLatency;
                this.sceneState = new SceneState();
                this.InitializeSteps();
                this.SetTheme(this.DefaultTheme, false);
                return true;
            }
        }

        private void renderer_OnPresent(Renderer sender, RendererPresentEventArgs args)
        {
            if (!this.running || args.Backbuffer == null)
                return;
            this.fullScreenViewer.Render(args.Backbuffer.RenderTargetTexture, this.renderer);
        }

        private void renderer_OnInternalError(Renderer sender, RendererErrorEventArgs args)
        {
            VisualizationTraceSource.Current.Fail(args.Message, args.Exception);
        }

        private void renderer_OnInformation(Renderer sender, RendererInfoEventArgs args)
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, args.Message);
        }

        private void InitializeSteps()
        {
            ITimeProvider timeProvider;
            if (this.fixedFramerate > 0)
            {
                timeProvider = (ITimeProvider)new FixedTimeProvider(this.fixedFramerate);
                this.fixedTimeProvider = (FixedTimeProvider)timeProvider;
            }
            else
                timeProvider = (ITimeProvider)new RealtimeTimeProvider();
            this.timeStep = new TimeStep((IVisualizationEngineDispatcher)this, timeProvider, this.dispatcher);
            this.inputStep = new InputStep((IVisualizationEngineDispatcher)this, this.dispatcher, this.inputHandler);
            this.tourStep = new TourStep(this, this.dispatcher);
            this.cameraStep = new CameraStep((IVisualizationEngineDispatcher)this, this.dispatcher);
            this.hitTestStep = new HitTestStep((IVisualizationEngineDispatcher)this, this.dispatcher);
            this.shadowStep = new ShadowStep((IVisualizationEngineDispatcher)this, this.dispatcher);
            this.lightingStep = new LightingStep((IVisualizationEngineDispatcher)this, this.dispatcher);
            this.globeStep = new GlobeStep((IVisualizationEngineDispatcher)this, this.dispatcher, this.themes.GetImageSet(BuiltinTheme.BingRoad, false), new Action<Exception>(this.InternalErrorHandler));
            this.backgroundStep = new BackgroundStep((IVisualizationEngineDispatcher)this, this.dispatcher);
            this.antialiasingStep = new AntialiasingStep((IVisualizationEngineDispatcher)this, this.dispatcher);
            this.annotationStep = new AnnotationStep((IVisualizationEngineDispatcher)this, this.dispatcher);
            this.layerStep = new LayerStep((IVisualizationEngineDispatcher)this, this.dispatcher, this.annotationStep);
            this.steps = new EngineStep[15]
      {
        (EngineStep) this.timeStep,
        (EngineStep) this.inputStep,
        (EngineStep) this.tourStep,
        (EngineStep) this.cameraStep,
        (EngineStep) this.lightingStep,
        (EngineStep) this.backgroundStep,
        (EngineStep) this.globeStep,
        (EngineStep) this.hitTestStep,
        (EngineStep) this.layerStep,
        (EngineStep) this.globeStep,
        (EngineStep) this.layerStep,
        (EngineStep) this.shadowStep,
        (EngineStep) this.backgroundStep,
        (EngineStep) this.antialiasingStep,
        (EngineStep) this.annotationStep
      };
            this.stepIds = new int[this.steps.Length];
            HashSet<EngineStep> hashSet = new HashSet<EngineStep>();
            for (int index = 0; index < this.steps.Length; ++index)
            {
                if (!hashSet.Contains(this.steps[index]))
                {
                    this.steps[index].Initialize((IUIControllerManager)this.inputStep, (ICameraControllerManager)this.cameraStep, (IHitTestManager)this.hitTestStep, (IShadowManager)this.shadowStep);
                    this.stepIds[index] = 0;
                    hashSet.Add(this.steps[index]);
                }
                else
                    ++this.stepIds[index];
            }
            foreach (EngineStep engineStep in hashSet)
                engineStep.OnInitialized();
            this.cameraStep.CameraIdle += new EventHandler(this.cameraStep_CameraIdle);
            this.tourStep.TourSceneStateChanged += new TourSceneStateChangeHandler(this.TourStepOnTourSceneStateChanged);
        }

        private void TourStepOnTourSceneStateChanged(object sender, TourSceneStateChangedEventArgs eventArgs)
        {
            if (this.dispatcher == null)
                return;
            eventArgs.TileExtents = this.globeStep.GetTileExtents();
            this.dispatcher.BeginInvoke((Action)(() =>
            {
                TourSceneStateChangeHandler stateChangeHandler = this.TourSceneStateChanged;
                if (stateChangeHandler == null)
                    return;
                stateChangeHandler(sender, eventArgs);
            }));
        }

        private void cameraStep_CameraIdle(object sender, EventArgs e)
        {
            if (this.CameraIdle == null || this.dispatcher == null)
                return;
            Dictionary<int, TileExtent> extents = this.globeStep.GetTileExtents();
            this.dispatcher.BeginInvoke((Action)(() =>
            {
                if (this.CameraIdle == null)
                    return;
                this.CameraIdle(sender, new CameraIdleEventArgs(extents));
            }));
        }

        private void RenderLoop()
        {
            try
            {
                this.running = this.InitializeEngine();
                if (!this.running)
                    VisualizationTraceSource.Current.Fail("The engine could not be initialized, likely because the hardware/driver is not compatible with the application. Try updating the video driver.");
                this.InitializedEvent.Set();
                while (this.running)
                {
                    this.requestRenderSemaphore.Wait();
                    try
                    {
                        RenderThreadMethod[] renderThreadMethodArray;
                        lock (this.renderThreadDispatcherLock)
                        {
                            renderThreadMethodArray = this.renderThreadDispatcherCalls.ToArray();
                            this.renderThreadDispatcherCalls.Clear();
                        }
                        foreach (RenderThreadMethod renderThreadMethod in renderThreadMethodArray)
                            renderThreadMethod();
                        if (this.running)
                        {
                            bool flag = this.RunFrame(this.frameSizeUpdated || this.fixedFramerate > 0 || this.useSwapChain);
                            int num = 0;
                            while (this.fixedFramerate > 0 && !flag)
                            {
                                Thread.Sleep(50);
                                flag = this.RunFrame(true);
                                ++num;
                                if (num % 50 == 0)
                                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Still waiting for an offline frame to complete... count=" + num.ToString());
                            }
                            this.frameSizeUpdated = false;
                        }
                    }
                    catch (OutOfMemoryException ex)
                    {
                        GC.Collect();
                        VisualizationTraceSource.Current.Fail("Render thread raised an out of memory exception. Aborting execution.");
                        this.InternalErrorHandler((Exception)ex);
                    }
                    catch (ThreadAbortException ex)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Render thread aborted, thread exiting.");
                        this.running = false;
                    }
                    catch (COMException ex)
                    {
                        VisualizationTraceSource.Current.Fail(string.Format("Fatal error while rendering a frame. Possible cause: {0}; DirectX error: {1}", (object)this.renderer.CorruptionErrorMessage, (object)((object)(ErrorCode)ex.ErrorCode).ToString()), (Exception)ex);
                        this.InternalErrorHandler((Exception)ex);
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while rendering a frame.", ex);
                        if (ex.GetType() == typeof(OutOfMemoryException))
                        {
                            VisualizationTraceSource.Current.Fail("Render thread raised an out of memory exception. Aborting execution.");
                            this.InternalErrorHandler(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail("The engine failed to be initialized.", ex);
            }
            finally
            {
                try
                {
                    if (this.steps != null)
                    {
                        foreach (EngineStep engineStep in this.steps)
                            engineStep.Dispose();
                    }
                    if (this.themes != null)
                        this.themes.Dispose();
                    if (this.targetBitmap != null)
                    {
                        this.targetBitmap.Dispose();
                        this.targetBitmap = (ReadableBitmap)null;
                    }
                    if (this.renderer != null)
                    {
                        this.renderer.OnInformation -= new RendererInfoEventHander(this.renderer_OnInformation);
                        this.renderer.OnInternalError -= new RendererErrorEventHandler(this.renderer_OnInternalError);
                        this.renderer.Dispose();
                    }
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Visualization engine shutting down.");
                    if (this.shutdownEvent != null)
                        this.shutdownEvent.Set();
                    if (this.InitializedEvent != null)
                        this.InitializedEvent.Set();
                }
                catch
                {
                }
                this.running = false;
            }
        }

        private void InternalErrorHandler(Exception e)
        {
            if (this.OnInternalError != null && this.dispatcher != null)
                this.dispatcher.BeginInvoke((Action)(() => this.OnInternalError((object)this, new InternalErrorEventArgs(e))));
            this.running = false;
        }

        private void DisposedCheck()
        {
            if (this.disposed)
                throw new ObjectDisposedException(this.ToString());
        }

        public void RunOnRenderThread(RenderThreadMethod methodOnRenderThread)
        {
            this.DisposedCheck();
            lock (this.renderThreadDispatcherLock)
                this.renderThreadDispatcherCalls.Add(methodOnRenderThread);
        }

        public void SetInputHandler(InputHandler handler)
        {
            this.DisposedCheck();
            this.inputStep.InputHandler = handler;
        }

        public void RequestFrame()
        {
            this.DisposedCheck();
            if (this.fixedTimeProvider != null)
                this.fixedTimeProvider.IncrementFrame();
            if (this.requestRenderSemaphore.CurrentCount >= 6)
                return;
            this.requestRenderSemaphore.Release();
        }

        public void ForceUpdate()
        {
            this.DisposedCheck();
            this.RunOnRenderThread((RenderThreadMethod)(() => this.frameSizeUpdated = true));
        }

        public void SetTheme(BuiltinTheme theme, bool isWithLabels)
        {
            this.DisposedCheck();
            if (this.CurrentThemeWithLabels == isWithLabels && this.CurrentTheme == theme)
                return;
            this.globeStep.CurrentImageSet = this.themes.GetImageSet(theme, isWithLabels);
            if (this.CurrentThemeWithLabels != this.themes.IsWithLabels(theme, isWithLabels))
            {
                this.CurrentThemeWithLabels = !this.CurrentThemeWithLabels;
                this.themeUpdated = true;
            }
            VisualizationTheme theme1 = (VisualizationTheme)null;
            if (this.CurrentTheme == theme)
            {
                if (!this.themeUpdated)
                    return;
            }
            try
            {
                using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.themes.GetResourcePath(theme)))
                {
                    using (XmlReader reader = XmlReader.Create(manifestResourceStream))
                    {
                        theme1 = VisualizationTheme.ReadXml(reader);
                        this.CurrentTheme = theme;
                        this.SetTheme(theme1);
                    }
                }
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(string.Format("Loading theme {0} failed with exception", (object)theme1), ex);
            }
        }

        private void SetTheme(VisualizationTheme theme)
        {
            this.RunOnRenderThread((RenderThreadMethod)(() =>
            {
                this.lightingStep.Rig = theme.Lighting;
                this.backgroundStep.TopColor = theme.BackgroundTopColor;
                this.backgroundStep.BottomColor = theme.BackgroundBottomColor;
                this.globeStep.Renderer.GlowEnabled = theme.GlobeGlowEnabled;
                this.globeStep.Renderer.GlowColor = theme.GlobeGlowColor;
                this.globeStep.Renderer.GlowFactor = theme.GlobeGlowFactor;
                this.globeStep.Renderer.GlowPower = theme.GlobeGlowPower;
                this.globeStep.Renderer.GlowReflectanceIndex = theme.GlobeGlowReflectanceIndex;
                this.globeStep.Renderer.PostProcessingOperations.Clear();
                foreach (ColorOperation operation in theme.GlobeColorOperations)
                    this.globeStep.Renderer.PostProcessingOperations.AddOperation(operation);
                this.shadowStep.Color = theme.ShadowColor;
                this.layerStep.RenderingParameters = theme.LayerParameters;
                this.layerStep.ColorManager.SetVisualColors((IList<Color4F>)theme.ColorsForVisuals, (IList<double>)theme.ColorStepsForVisuals);
                this.annotationStep.SetStyle(theme.AnnotationStyle);
                this.themeUpdated = true;
                if (this.ThemeChanged == null || this.dispatcher == null)
                    return;
                this.dispatcher.BeginInvoke((Action)(() => this.ThemeChanged(this.CurrentTheme, theme, this.CurrentThemeWithLabels)));
            }));
            this.CurrentVisualizationTheme = theme;
        }

        public void SetCustomMap(Guid customMap)
        {
            if (!(this.CurrentCustomMapId != customMap))
                return;
            this.CurrentCustomMapId = customMap;
        }

        private bool RunFrame(bool forceUpdate)
        {
            bool flag1 = false;
            ++this.requestedFrameCount;
            SceneState state = new SceneState();
            state.ScreenWidth = (double)this.screenWidth;
            state.ScreenHeight = (double)this.screenHeight;
            state.ElapsedFrames = (long)this.renderer.FrameCount;
            state.GraphicsLevel = this.GraphicsLevel;
            state.SceneCustomMap = this.CurrentCustomMap;
            state.OfflineRender = this.fixedFramerate > 0;
            bool flag2 = false;
            for (int index = 0; index < this.steps.Length; ++index)
                flag2 = flag2 | this.steps[index].PreExecute(state, this.stepIds[index]);
            bool flag3 = flag2 | this.themeUpdated;
            this.themeUpdated = false;
            if (flag3 || forceUpdate)
                this.forceRenderingFrameCount = this.renderer.FrameLatency * 2;
            if (this.forceRenderingFrameCount > 0)
            {
                try
                {
                    if (this.renderer.BeginFrame(new Color4F?(this.clearColor)))
                    {
                        for (int index = 0; index < this.steps.Length; ++index)
                            this.steps[index].Execute(this.renderer, state, this.stepIds[index]);
                        this.renderer.EndFrame();
                        --this.forceRenderingFrameCount;
                        ++this.renderedFrameCount;
                        this.renderExceptions.Clear();
                        flag1 = true;
                    }
                }
                catch (Exception ex)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Exception while rendering a frame: " + (object)ex);
                    if (ex.GetType() == typeof(OutOfMemoryException))
                    {
                        if (this.renderExceptions.FindIndex((Predicate<Exception>)(re => re.GetType() == typeof(OutOfMemoryException))) < 0)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        else
                            throw;
                    }
                    this.renderExceptions.Add(ex);
                    if (this.renderExceptions.Count > 30)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "The maximum number of exception frames has been reached. Aborting...");
                        throw;
                    }
                }
            }
            long timestamp = Stopwatch.GetTimestamp();
            if ((double)(timestamp - this.lastTimestamp) / (double)Stopwatch.Frequency > 1.0)
            {
                this.RequestedFramesPerSecond = this.requestedFrameCount;
                this.RenderedFramesPerSecond = this.renderedFrameCount;
                this.lastTimestamp = timestamp;
                this.requestedFrameCount = 0;
                this.renderedFrameCount = 0;
            }
            if (state.ElapsedTicks % 600L == 0L)
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Rendering stats: {0} requested frames/s, {1} rendered frames/s.", (object)this.RequestedFramesPerSecond, (object)this.RenderedFramesPerSecond);
            if (this.sceneState != null)
            {
                if (!this.sceneState.CameraMoved && state.CameraMoved)
                {
                    if (this.CameraMoveStarted != null && this.dispatcher != null)
                        this.dispatcher.BeginInvoke((Action)(() => this.CameraMoveStarted(state.CameraSnapshot.Clone())));
                }
                else if (this.sceneState.CameraMoved && !state.CameraMoved && (this.CameraMoveEnded != null && this.dispatcher != null))
                    this.dispatcher.BeginInvoke((Action)(() => this.CameraMoveEnded(state.CameraSnapshot.Clone())));
            }
            this.sceneState = state;
            return flag1;
        }

        public bool TryGetFrame(out RenderTarget target)
        {
            this.DisposedCheck();
            return this.GetRenderTargetFrame(out target);
        }

        public bool TryGetFrame(out ReadableBitmap target)
        {
            this.DisposedCheck();
            if (!this.running)
            {
                target = (ReadableBitmap)null;
                return false;
            }
            else
            {
                RenderTarget target1 = (RenderTarget)null;
                bool renderTargetFrame = this.GetRenderTargetFrame(out target1);
                if (this.targetBitmap == null && renderTargetFrame)
                    this.targetBitmap = ReadableBitmap.Create(target1.RenderTargetTexture.Width, target1.RenderTargetTexture.Height, target1.RenderTargetTexture.Format);
                if (renderTargetFrame)
                    this.renderer.CopyRenderTargetData(target1, new Rect(0, 0, target1.RenderTargetTexture.Width, target1.RenderTargetTexture.Height), this.targetBitmap);
                target = this.targetBitmap;
                return renderTargetFrame;
            }
        }

        private bool GetRenderTargetFrame(out RenderTarget target)
        {
            bool renderTargetFrame = this.renderer.TryGetRenderTargetFrame(out target);
            if (this.fixedFramerate > 0)
            {
                for (; !renderTargetFrame && this.running; renderTargetFrame = this.renderer.TryGetRenderTargetFrame(out target))
                    Thread.Sleep(50);
            }
            return renderTargetFrame;
        }

        public void ReleaseFrame()
        {
            this.DisposedCheck();
            this.renderer.ReleaseRenderTargetFrame();
        }

        public void UpdateFrameSize(int width, int height)
        {
            this.DisposedCheck();
            if (this.fixedFramerate > 0)
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Request frame resize; width: {0}, height: {1}.", (object)width, (object)height);
            if (this.resizeTimer != null)
                this.resizeTimer.Dispose();
            TimerCallback callback = (TimerCallback)(o => this.RunOnRenderThread((RenderThreadMethod)(() =>
            {
                this.renderer.UpdateRenderTargetSize(width, height);
                this.screenWidth = width;
                this.screenHeight = height;
                this.frameSizeUpdated = true;
            })));
            if (this.renderer.FrameWidth == 8)
            {
                int frameHeight = this.renderer.FrameHeight;
            }
            this.resizeTimer = new Timer(callback, (object)null, 250, -1);
        }

        public SceneState GetCurrentSceneState()
        {
            this.DisposedCheck();
            if (this.sceneState != null)
                return this.sceneState.Clone();
            else
                return (SceneState)null;
        }

        public void AddLayer(Layer layer)
        {
            this.DisposedCheck();
            if (layer == null || !this.layerStep.AddLayer(layer))
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Added layer to the visualization engine. Layer type: {0}.", (object)layer.GetType().ToString());
        }

        public bool RemoveLayer(Layer layer)
        {
            this.DisposedCheck();
            if (layer == null)
                return false;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Removing layer from the visualization engine. Layer type: {0}.", (object)layer.GetType().ToString());
            return this.layerStep.RemoveLayer(layer);
        }

        public bool MoveCamera(CameraSnapshot newCameraSnapshot, CameraMoveStyle moveStyle, bool retainPivotAngle = false)
        {
            this.DisposedCheck();
            if (this.cameraStep.Controller != this.cameraStep.DefaultController || newCameraSnapshot == null)
                return false;
            this.cameraStep.DefaultController.MoveTo(newCameraSnapshot, moveStyle, retainPivotAngle);
            return true;
        }

        public bool MoveCamera(double south, double north, double west, double east, CameraMoveStyle style)
        {
            this.DisposedCheck();
            if (this.cameraStep.Controller != this.cameraStep.DefaultController)
                return false;
            this.cameraStep.DefaultController.MoveTo(south * (Math.PI / 180.0), north * (Math.PI / 180.0), west * (Math.PI / 180.0), east * (Math.PI / 180.0), style);
            return true;
        }

        public bool Rotate(CameraRotation rotation)
        {
            this.DisposedCheck();
            if (this.cameraStep.Controller != this.cameraStep.DefaultController)
                return false;
            this.cameraStep.DefaultController.Rotate(rotation);
            return true;
        }

        public bool ZoomIn()
        {
            this.DisposedCheck();
            if (this.cameraStep.Controller != this.cameraStep.DefaultController)
                return false;
            this.cameraStep.DefaultController.ZoomIn();
            return true;
        }

        public bool ZoomOut()
        {
            this.DisposedCheck();
            if (this.cameraStep.Controller != this.cameraStep.DefaultController)
                return false;
            this.cameraStep.DefaultController.ZoomOut();
            return true;
        }

        public void ZoomToSelection()
        {
            this.DisposedCheck();
            this.FocusOnSelection(true);
        }

        public void FocusOnSelection(bool zoomInAndLookNorth)
        {
            this.DisposedCheck();
            List<Vector3D> locations = new List<Vector3D>();
            for (int index = 0; index < this.Layers.Count; ++index)
            {
                HitTestableLayer hitTestableLayer = this.Layers[index] as HitTestableLayer;
                if (hitTestableLayer != null)
                {
                    foreach (InstanceId id in hitTestableLayer.GetSelected())
                        hitTestableLayer.AddInstancePointsToList(id, locations);
                }
            }
            if (locations.Count <= 0)
                return;
            this.cameraStep.DefaultController.MoveTo(locations, zoomInAndLookNorth, CameraMoveStyle.FlyTo);
        }

        public void MoveTo(Cap cap)
        {
            this.cameraStep.DefaultController.MoveTo(cap, CameraMoveStyle.FlyTo);
        }

        public bool IsAnythingSelected()
        {
            this.DisposedCheck();
            for (int index = 0; index < this.Layers.Count; ++index)
            {
                HitTestableLayer hitTestableLayer = this.Layers[index] as HitTestableLayer;
                if (hitTestableLayer != null && hitTestableLayer.GetSelected().Length > 0)
                    return true;
            }
            return false;
        }

        public bool ResetView()
        {
            this.DisposedCheck();
            if (this.cameraStep.Controller != this.cameraStep.DefaultController)
                return false;
            this.cameraStep.DefaultController.ResetView();
            return true;
        }

        public Color4F GetVisualColor(int index)
        {
            this.DisposedCheck();
            if (this.layerStep.ColorManager != null)
                return this.layerStep.ColorManager.GetColor(index);
            else
                return new Color4F();
        }

        public void Dispose()
        {
            if (this.fullScreenViewer != null)
                this.fullScreenViewer.Dispose();
            this.running = false;
            if (this.requestRenderSemaphore != null && this.requestRenderSemaphore.CurrentCount == 0)
                this.requestRenderSemaphore.Release();
            if (this.shutdownEvent != null && !this.shutdownEvent.WaitOne(4000))
            {
                this.renderThread.Abort();
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "The render thread exceeded the maximum allowed ({0} ms) time before it was forced to shut down.", (object)4000);
            }
            if (this.requestRenderSemaphore != null)
            {
                this.requestRenderSemaphore.Dispose();
                this.requestRenderSemaphore = (SemaphoreSlim)null;
            }
            if (this.InitializedEvent != null)
            {
                this.InitializedEvent.Dispose();
                this.InitializedEvent = (EventWaitHandle)null;
            }
            if (this.shutdownEvent != null)
            {
                this.shutdownEvent.Dispose();
                this.shutdownEvent = (EventWaitHandle)null;
            }
            if (this.resizeTimer != null)
            {
                this.resizeTimer.Dispose();
                this.resizeTimer = (Timer)null;
            }
            this.disposed = true;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Visualization Engine shutdown complete.");
        }
    }
}
