// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TourStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    internal class TourStep : EngineStep, ITourPlayer, INotifyPropertyChanged
    {
        private TimeSpan animationTimeBuffer = new TimeSpan(0, 0, 1);
        private const double minEffectSpeed = 0.05;
        private Tour tour;
        private bool isInvalidTour;
        private TourStep.TourState _tourState;
        private bool _IsPlaying;
        private int _currentSceneIndex;
        private TourSceneState sceneState;
        private MoverCameraController tourCameraController;
        private ITourLayerManager layerManager;
        private ITimeController timeController;
        private DispatcherOperation dispatcherOperation;
        private bool skipTransition;
        private bool layerslessTransition;
        private VisualizationEngine engine;
        private TourStep.QueryCompletedContext queryCompletedContext;
        private bool visualTimeEnabledAtPause;
        private List<CameraMover> transitions;
        private List<CameraMover> effects;
        private object setLayerContext;
        private TourSceneState previousSceneState;
        private int loopCount;

        private TourStep.TourState tourState
        {
            get
            {
                return this._tourState;
            }
            set
            {
                this._tourState = value;
                this.IsPlaying = value == TourStep.TourState.Playing;
            }
        }

        public string PropertyIsPlaying
        {
            get
            {
                return "IsPlaying";
            }
        }

        public bool IsPlaying
        {
            get
            {
                return this._IsPlaying;
            }
            set
            {
                this.SetProperty<bool>(this.PropertyIsPlaying, ref this._IsPlaying, value);
            }
        }

        public string PropertyCurrentSceneIndex
        {
            get
            {
                return "CurrentSceneIndex";
            }
        }

        public int CurrentSceneIndex
        {
            get
            {
                return this._currentSceneIndex;
            }
            set
            {
                this.SetProperty<int>(this.PropertyCurrentSceneIndex, ref this._currentSceneIndex, value);
            }
        }

        public int SceneCount
        {
            get
            {
                if (this.tour != null)
                    return this.tour.Scenes.Count;
                else
                    return 0;
            }
        }

        public bool Loop { get; set; }

        public static double DefaultSpeedFactor
        {
            get
            {
                return 0.525;
            }
        }

        public static double DefaultSceneDuration
        {
            get
            {
                return 6.0;
            }
        }

        public event TourSceneStateChangeHandler TourSceneStateChanged;

        public TourStep(VisualizationEngine engine, Dispatcher dispatcher)
            : base((IVisualizationEngineDispatcher)engine, dispatcher)
        {
            this.engine = engine;
            this.isInvalidTour = true;
            this.CurrentSceneIndex = 0;
        }

        private void SetAnimation()
        {
            if (!this.layerManager.PlayToTime.HasValue)
                return;
            TimeSpan duration = this.tour.Scenes[this.CurrentSceneIndex].Duration;
            if (duration > this.animationTimeBuffer)
                duration -= this.animationTimeBuffer;
            VisualizationTraceSource.Current.TraceInformation("TourStep.SetAnimation: Setting visual time range on time controller - From: {0}, To: {1}", (object)this.layerManager.PlayFromTime.Value, (object)this.layerManager.PlayToTime.Value);
            this.timeController.SetVisualTimeRange(this.layerManager.PlayFromTime.Value, this.layerManager.PlayToTime.Value, false);
            this.timeController.Duration = duration;
            this.timeController.CurrentVisualTime = this.layerManager.PlayFromTime.Value;
            this.timeController.VisualTimeEnabled = true;
        }

        internal override bool PreExecute(SceneState state, int phase)
        {
            if (this.isInvalidTour)
                return false;
            if (this.tourState == TourStep.TourState.Playing)
            {
                bool flag = true;
                while (flag)
                {
                    flag = false;
                    TourSceneStateChangeHandler stateChangeHandler = this.TourSceneStateChanged;
                    if (stateChangeHandler != null && this.previousSceneState != this.sceneState)
                        stateChangeHandler((object)this, new TourSceneStateChangedEventArgs()
                        {
                            SceneIndex = this.CurrentSceneIndex,
                            TourSceneState = this.sceneState,
                            LoopCount = this.loopCount
                        });
                    this.previousSceneState = this.sceneState;
                    switch (this.sceneState)
                    {
                        case TourSceneState.NotStarted:
                            this.layerslessTransition = false;
                            VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: NotStarted scene # {0}.", (object)this.CurrentSceneIndex);
                            if (this.engine.CurrentCustomMapId != this.tour.Scenes[this.CurrentSceneIndex].CustomMapId)
                            {
                                this.engine.SetCustomMap(this.tour.Scenes[this.CurrentSceneIndex].CustomMapId);
                                state.SceneCustomMap = this.engine.CurrentCustomMap;
                                this.layerslessTransition = true;
                            }
                            if (!this.skipTransition && this.CurrentSceneIndex > 0)
                            {
                                this.tourCameraController.SetMover(this.transitions[this.CurrentSceneIndex - 1], false);
                                this.engine.FlatModeTransitionTime = this.tour.Scenes[this.CurrentSceneIndex].TransitionDuration.TotalSeconds;
                            }
                            else
                            {
                                this.tourCameraController.SetMover((CameraMover)new StillCameraMover(this.effects[this.CurrentSceneIndex].Start), false);
                                this.engine.FlatModeTransitionTime = 0.0;
                                this.skipTransition = false;
                            }
                            this.queryCompletedContext = new TourStep.QueryCompletedContext();
                            if (!string.IsNullOrEmpty(this.tour.Scenes[this.CurrentSceneIndex].LayersContent))
                            {
                                Action action = delegate
                                {
                                    this.setLayerContext = this.layerManager.PrepareSceneLayers(this.tour.Scenes[this.CurrentSceneIndex].LayersContent, this.tour.Scenes[this.CurrentSceneIndex].CustomMapId, (Action<object, Exception>)((context, exception) => ((TourStep.QueryCompletedContext)context).PrepareSceneLayersCompleted = true), (object)this.queryCompletedContext);
                                    VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: PrepareSceneLayers has been invoked for Scene # {0}", (object)this.CurrentSceneIndex);
                                };
                                this.dispatcherOperation = this.EventDispatcher.BeginInvoke(action);
                            }
                            else
                            {
                                this.queryCompletedContext.PrepareSceneLayersCompleted = true;
                                this.queryCompletedContext.SetSceneLayersStarted = true;
                                this.queryCompletedContext.SetSceneLayersCompleted = true;
                            }
                            if (this.engine.FlatMode != this.tour.Scenes[this.CurrentSceneIndex].FlatModeEnabled)
                                this.engine.FlatMode = this.tour.Scenes[this.CurrentSceneIndex].FlatModeEnabled;
                            this.sceneState = TourSceneState.InTransition;
                            state.IsSkipLayerRelatedSteps = this.layerslessTransition;
                            flag = true;
                            continue;
                        case TourSceneState.InTransition:
                            VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: InTransition scene # {0}.", (object)this.CurrentSceneIndex);
                            state.IsSkipLayerRelatedSteps = this.layerslessTransition;
                            if (this.tourCameraController.Mover.MoveCompleted && this.dispatcherOperation != null && this.dispatcherOperation.Status == DispatcherOperationStatus.Completed)
                            {
                                if (this.engine.CurrentTheme != this.tour.Scenes[this.CurrentSceneIndex].ThemeId || this.engine.CurrentThemeWithLabels != this.tour.Scenes[this.CurrentSceneIndex].ThemeWithLabel)
                                {
                                    VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: Setting theme on scene # {0}.", (object)this.CurrentSceneIndex);
                                    this.engine.SetTheme(this.tour.Scenes[this.CurrentSceneIndex].ThemeId, this.tour.Scenes[this.CurrentSceneIndex].ThemeWithLabel);
                                }
                                if (this.queryCompletedContext.PrepareSceneLayersCompleted && !this.queryCompletedContext.SetSceneLayersStarted && !this.queryCompletedContext.SetSceneLayersCompleted)
                                {
                                    this.queryCompletedContext.SetSceneLayersStarted = true;
                                    this.timeController.SetVisualTimeRange(DateTime.MinValue, DateTime.MinValue, false);
                                    this.timeController.CurrentVisualTime = DateTime.MinValue;
                                    Action action = delegate
                                    {
                                        try
                                        {
                                            this.layerManager.SetSceneLayers(this.setLayerContext, (Action<object, Exception>)((context, exception) => ((TourStep.QueryCompletedContext)context).SetSceneLayersCompleted = true), (object)this.queryCompletedContext);
                                            VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: SetSceneLayers has been invoked for Scene # {0}", (object)this.CurrentSceneIndex);
                                        }
                                        catch (Exception ex)
                                        {
                                            VisualizationTraceSource.Current.Fail(string.Format("TourStep.PreExecute: SetSceneLayers has thrown an exception for Scene # {0}", (object)this.CurrentSceneIndex), ex);
                                            this.sceneState = TourSceneState.Finished;
                                        }
                                    };
                                    this.dispatcherOperation = this.EventDispatcher.BeginInvoke(action);
                                    if (this.sceneState == TourSceneState.Finished)
                                    {
                                        flag = true;
                                        continue;
                                    }
                                    else if (state.OfflineRender)
                                    {
                                        VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: Waiting on SetSceneLayers for scene # {0}.", (object)this.CurrentSceneIndex);
                                        int num = (int)this.dispatcherOperation.Wait();
                                    }
                                }
                                if (this.queryCompletedContext.PrepareSceneLayersCompleted && this.queryCompletedContext.SetSceneLayersStarted && this.queryCompletedContext.SetSceneLayersCompleted)
                                {
                                    this.tourCameraController.SetMover(this.effects[this.CurrentSceneIndex], false);
                                    this.SetAnimation();
                                    this.queryCompletedContext = (TourStep.QueryCompletedContext)null;
                                    this.sceneState = TourSceneState.InFrame;
                                    flag = true;
                                    continue;
                                }
                                else
                                    continue;
                            }
                            else
                                continue;
                        case TourSceneState.InFrame:
                            VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: InFrame scene # {0}.", (object)this.CurrentSceneIndex);
                            if (this.tourCameraController.Mover.MoveCompleted)
                            {
                                if (this.layerManager.PlayToTime.HasValue)
                                {
                                    DateTime currentVisualTime = this.timeController.CurrentVisualTime;
                                    DateTime? playToTime = this.layerManager.PlayToTime;
                                    if ((playToTime.HasValue ? (currentVisualTime >= playToTime.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                                        continue;
                                }
                                this.sceneState = TourSceneState.Finished;
                                flag = true;
                                continue;
                            }
                            else
                                continue;
                        case TourSceneState.Finished:
                            VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: Finished scene # {0}.", (object)this.CurrentSceneIndex);
                            if (this.Loop)
                            {
                                this.CurrentSceneIndex = ++this.CurrentSceneIndex > this.SceneCount - 1 ? 0 : this.CurrentSceneIndex;
                                this.sceneState = TourSceneState.NotStarted;
                                ++this.loopCount;
                                flag = true;
                                VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: Starting loop # {0}.", (object)this.loopCount);
                                continue;
                            }
                            else if (this.CurrentSceneIndex == this.tour.Scenes.Count - 1)
                            {
                                this.tourState = TourStep.TourState.Finished;
                                this.CameraControllerManager.Controller = (CameraController)null;
                                VisualizationTraceSource.Current.TraceInformation("TourStep.PreExecute: Finished tour at loop # {0}.", (object)this.loopCount);
                                continue;
                            }
                            else
                            {
                                ++this.CurrentSceneIndex;
                                this.sceneState = TourSceneState.NotStarted;
                                flag = true;
                                continue;
                            }
                        default:
                            continue;
                    }
                }
                return true;
            }
            else
            {
                if (this.tourState != TourStep.TourState.Paused || this.CameraControllerManager.Controller != this.tourCameraController)
                    return false;
                switch (this.sceneState)
                {
                    case TourSceneState.NotStarted:
                        this.layerslessTransition = false;
                        if (this.engine.CurrentCustomMapId != this.tour.Scenes[this.CurrentSceneIndex].CustomMapId)
                        {
                            this.engine.SetCustomMap(this.tour.Scenes[this.CurrentSceneIndex].CustomMapId);
                            state.SceneCustomMap = this.engine.CurrentCustomMap;
                            this.layerslessTransition = true;
                        }
                        if (!this.skipTransition && this.CurrentSceneIndex > 0)
                        {
                            this.tourCameraController.SetMover(this.transitions[this.CurrentSceneIndex - 1], false);
                            this.engine.FlatModeTransitionTime = this.tour.Scenes[this.CurrentSceneIndex].TransitionDuration.TotalSeconds;
                        }
                        else
                        {
                            this.tourCameraController.SetMover((CameraMover)new StillCameraMover(this.tour.Scenes[this.CurrentSceneIndex].Frame.Camera), false);
                            this.engine.FlatModeTransitionTime = 0.0;
                            this.skipTransition = false;
                        }
                        this.queryCompletedContext = new TourStep.QueryCompletedContext();
                        if (!string.IsNullOrEmpty(this.tour.Scenes[this.CurrentSceneIndex].LayersContent))
                        {
                            this.timeController.SetVisualTimeRange(DateTime.MinValue, DateTime.MinValue, false);
                            this.timeController.CurrentVisualTime = DateTime.MinValue;
                            this.dispatcherOperation = this.EventDispatcher.BeginInvoke((Action)(() => this.setLayerContext = this.layerManager.PrepareSceneLayers(this.tour.Scenes[this.CurrentSceneIndex].LayersContent, this.tour.Scenes[this.CurrentSceneIndex].CustomMapId, (Action<object, Exception>)((context, exception) => ((TourStep.QueryCompletedContext)context).PrepareSceneLayersCompleted = true), (object)this.queryCompletedContext)));
                        }
                        else
                        {
                            this.queryCompletedContext.PrepareSceneLayersCompleted = true;
                            this.queryCompletedContext.SetSceneLayersStarted = true;
                            this.queryCompletedContext.SetSceneLayersCompleted = true;
                        }
                        if (this.engine.FlatMode != this.tour.Scenes[this.CurrentSceneIndex].FlatModeEnabled)
                            this.engine.FlatMode = this.tour.Scenes[this.CurrentSceneIndex].FlatModeEnabled;
                        this.sceneState = TourSceneState.InTransition;
                        state.IsSkipLayerRelatedSteps = this.layerslessTransition;
                        break;
                    case TourSceneState.InTransition:
                        state.IsSkipLayerRelatedSteps = this.layerslessTransition;
                        if (this.tourCameraController.Mover.MoveCompleted)
                        {
                            if (this.engine.CurrentTheme != this.tour.Scenes[this.CurrentSceneIndex].ThemeId || this.engine.CurrentThemeWithLabels != this.tour.Scenes[this.CurrentSceneIndex].ThemeWithLabel)
                                this.engine.SetTheme(this.tour.Scenes[this.CurrentSceneIndex].ThemeId, this.tour.Scenes[this.CurrentSceneIndex].ThemeWithLabel);
                            if (this.dispatcherOperation != null && this.dispatcherOperation.Status == DispatcherOperationStatus.Completed)
                            {
                                if (this.queryCompletedContext.PrepareSceneLayersCompleted && !this.queryCompletedContext.SetSceneLayersStarted && !this.queryCompletedContext.SetSceneLayersCompleted)
                                {
                                    this.queryCompletedContext.SetSceneLayersStarted = true;
                                    Action action = delegate
                                    {
                                        this.layerManager.SetSceneLayers(this.setLayerContext,
                                            (Action<object, Exception>)
                                                ((context, exception) =>
                                                    ((TourStep.QueryCompletedContext) context).SetSceneLayersCompleted =
                                                        true), (object) this.queryCompletedContext);
                                    };
                                    this.dispatcherOperation = this.EventDispatcher.BeginInvoke(action);
                                }
                                if (this.queryCompletedContext.PrepareSceneLayersCompleted && this.queryCompletedContext.SetSceneLayersStarted && this.queryCompletedContext.SetSceneLayersCompleted)
                                {
                                    this.tourCameraController.SetMover(this.effects[this.CurrentSceneIndex], false);
                                    this.SetAnimation();
                                    this.queryCompletedContext = (TourStep.QueryCompletedContext)null;
                                    this.sceneState = TourSceneState.InFrame;
                                    this.CameraControllerManager.Controller = (CameraController)null;
                                    break;
                                }
                                else
                                    break;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    default:
                        return false;
                }
                return true;
            }
        }

        internal override void Execute(Renderer renderer, SceneState state, int phase)
        {
        }

        private double GetEffectSpeedFactor(int i)
        {
            double effectSpeed = this.tour.Scenes[i].EffectSpeed;
            return 0.05 * (1.0 - effectSpeed) + effectSpeed;
        }

        public void SetTour(Tour setTo, ITourLayerManager manager, ITimeController controller)
        {
            this.tour = setTo;
            this.layerManager = manager;
            this.timeController = controller;
            this.CurrentSceneIndex = 0;
            this.loopCount = 0;
            this.tourState = TourStep.TourState.NotStarted;
            this.sceneState = TourSceneState.NotStarted;
            this.timeController.VisualTimeEnabled = false;
            this.timeController.Looping = false;
            if (this.tour == null || this.tour.Scenes.Count == 0)
            {
                this.isInvalidTour = true;
                this.CameraControllerManager.Controller = (CameraController)null;
            }
            else
            {
                this.isInvalidTour = false;
                this.skipTransition = false;
                this.tourCameraController = new MoverCameraController();
                this.tourCameraController.SetMover((CameraMover)new StillCameraMover(this.tour.Scenes[0].Frame.Camera), false);
                this.CameraControllerManager.Controller = (CameraController)this.tourCameraController;
                if (this.effects == null)
                    this.effects = new List<CameraMover>();
                else
                    this.effects.Clear();
                for (int i = 0; i < this.tour.Scenes.Count; ++i)
                {
                    switch (this.tour.Scenes[i].EffectType)
                    {
                        case SceneEffect.Station:
                            this.effects.Add((CameraMover)new StillEffectCameraMover(this.tour.Scenes[i].Frame.Camera, this.tour.Scenes[i].Duration));
                            break;
                        case SceneEffect.Circle:
                            this.effects.Add((CameraMover)new CircleCameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), this.tour.Scenes[i].Duration));
                            break;
                        case SceneEffect.Dolly:
                            CameraSnapshot camera1 = this.tour.Scenes[Math.Max(i - 1, 0)].Frame.Camera;
                            CameraSnapshot camera2 = this.tour.Scenes[Math.Min(i + 1, this.tour.Scenes.Count - 1)].Frame.Camera;
                            CameraMover cameraMover;
                            if (this.tour.Scenes[i].FlatModeEnabled)
                            {
                                Vector2D startAttractor = new Vector2D(camera1.Longitude, camera1.Latitude);
                                Vector2D endAttractor = new Vector2D(camera2.Longitude, camera2.Latitude);
                                cameraMover = (CameraMover)new FlatDollyCameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), this.tour.Scenes[i].Duration, startAttractor, endAttractor);
                            }
                            else
                            {
                                Vector3D startAttractor = Coordinates.GeoTo3D(camera1.Longitude, camera1.Latitude);
                                Vector3D endAttractor = Coordinates.GeoTo3D(camera2.Longitude, camera2.Latitude);
                                cameraMover = (CameraMover)new DollyCameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), this.tour.Scenes[i].Duration, startAttractor, endAttractor);
                            }
                            this.effects.Add(cameraMover);
                            break;
                        case SceneEffect.FlyOver:
                            if (this.tour.Scenes[i].FlatModeEnabled)
                            {
                                this.effects.Add((CameraMover)new FlatFlyOverCameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), this.tour.Scenes[i].Duration));
                                break;
                            }
                            else
                            {
                                this.effects.Add((CameraMover)new FlyOverCameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), this.tour.Scenes[i].Duration));
                                break;
                            }
                        case SceneEffect.PushIn:
                            this.effects.Add((CameraMover)new PushInCameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), this.tour.Scenes[i].Duration));
                            break;
                        case SceneEffect.Figure8:
                            CameraSnapshot camera3 = this.tour.Scenes[Math.Max(i - 1, 0)].Frame.Camera;
                            if (this.tour.Scenes[i].FlatModeEnabled)
                            {
                                this.effects.Add((CameraMover)new FlatFigure8CameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), this.tour.Scenes[i].Duration, new Vector2D(camera3.Longitude, camera3.Latitude)));
                                break;
                            }
                            else
                            {
                                this.effects.Add((CameraMover)new Figure8CameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), this.tour.Scenes[i].Duration, Coordinates.GeoTo3D(camera3.Longitude, camera3.Latitude)));
                                break;
                            }
                        case SceneEffect.RotateGlobe:
                            CameraSnapshot nextScene = i + 1 < this.tour.Scenes.Count ? this.tour.Scenes[i + 1].Frame.Camera : (CameraSnapshot)null;
                            if (this.tour.Scenes[i].FlatModeEnabled)
                            {
                                double maxRadians = (this.tour.Scenes[i].HasCustomMap ? CustomSpaceTransform.WorldScaleInDegrees : 180.0) * (Math.PI / 180.0);
                                this.effects.Add((CameraMover)new FlatMapEastWestCameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), nextScene, this.tour.Scenes[i].Duration, maxRadians));
                                break;
                            }
                            else
                            {
                                this.effects.Add((CameraMover)new GlobeEastWestCameraMover(this.tour.Scenes[i].Frame.Camera, this.GetEffectSpeedFactor(i), nextScene, this.tour.Scenes[i].Duration));
                                break;
                            }
                    }
                }
                if (this.transitions == null)
                    this.transitions = new List<CameraMover>();
                else
                    this.transitions.Clear();
                for (int index = 1; index < this.tour.Scenes.Count; ++index)
                {
                    CameraMover other;
                    if (this.tour.Scenes[index].TransitionDuration.TotalSeconds > 0.01 && this.tour.Scenes[index].CustomMapId == this.tour.Scenes[index - 1].CustomMapId)
                    {
                        other = this.tour.Scenes[index - 1].FlatModeEnabled || this.tour.Scenes[index].FlatModeEnabled ? (CameraMover)new FlatTransitionCameraMover(this.effects[index - 1].End, this.effects[index].Start, this.tour.Scenes[index].TransitionDuration) : (CameraMover)new TransitionCameraMover(this.effects[index - 1].End, this.effects[index].Start, this.tour.Scenes[index].TransitionDuration);
                        this.effects[index - 1].SetEndBlender(other);
                        other.SetEndBlender(this.effects[index]);
                    }
                    else
                    {
                        other = (CameraMover)new StillCameraMover(this.effects[index].Start);
                        this.effects[index - 1].SetEndBlender((CameraMover)null);
                        other.SetEndBlender((CameraMover)null);
                    }
                    this.transitions.Add(other);
                }
                this.effects[this.effects.Count - 1].SetEndBlender((CameraMover)null);
            }
        }

        public void Play()
        {
            if (this.isInvalidTour)
                return;
            switch (this.tourState)
            {
                case TourStep.TourState.NotStarted:
                    this.CurrentSceneIndex = 0;
                    this.sceneState = TourSceneState.NotStarted;
                    this.timeController.VisualTimeEnabled = false;
                    this.timeController.Looping = false;
                    this.CameraControllerManager.Controller = (CameraController)this.tourCameraController;
                    this.engine.FlatModeTransitionTime = 0.0;
                    this.tourState = TourStep.TourState.Playing;
                    break;
                case TourStep.TourState.Paused:
                    this.CameraControllerManager.Controller = (CameraController)this.tourCameraController;
                    this.tourState = TourStep.TourState.Playing;
                    if (this.layerManager.PlayToTime.HasValue)
                        this.timeController.VisualTimeEnabled = this.visualTimeEnabledAtPause;
                    this.engine.FlatModeTransitionTime = 0.0;
                    this.tourCameraController.Mover.Resume();
                    break;
                case TourStep.TourState.Finished:
                    this.CurrentSceneIndex = 0;
                    this.tourCameraController = new MoverCameraController();
                    this.tourCameraController.SetMover((CameraMover)new StillCameraMover(this.tour.Scenes[this.CurrentSceneIndex].Frame.Camera), false);
                    this.CameraControllerManager.Controller = (CameraController)this.tourCameraController;
                    this.sceneState = TourSceneState.NotStarted;
                    this.timeController.VisualTimeEnabled = false;
                    this.timeController.Looping = false;
                    this.engine.FlatModeTransitionTime = double.MaxValue;
                    this.tourState = TourStep.TourState.Playing;
                    break;
            }
        }

        public void Pause()
        {
            if (this.isInvalidTour)
                return;
            switch (this.tourState)
            {
                case TourStep.TourState.Playing:
                    this.tourState = TourStep.TourState.Paused;
                    ITimeController timeController = this.timeController;
                    if (timeController != null)
                    {
                        this.visualTimeEnabledAtPause = timeController.VisualTimeEnabled;
                        timeController.VisualTimeEnabled = false;
                        timeController.Looping = false;
                    }
                    ICameraControllerManager controllerManager = this.CameraControllerManager;
                    if (controllerManager != null)
                        controllerManager.Controller = (CameraController)null;
                    if (this.tourCameraController == null || this.tourCameraController.Mover == null)
                        break;
                    this.tourCameraController.Mover.Pause();
                    break;
            }
        }

        public void Stop()
        {
            if (this.isInvalidTour)
                return;
            this.tourState = TourStep.TourState.Finished;
            this.timeController.VisualTimeEnabled = false;
            this.timeController.Looping = false;
            this.CameraControllerManager.Controller = (CameraController)null;
        }

        private void StartNewScene()
        {
            this.timeController.VisualTimeEnabled = false;
            this.timeController.Looping = false;
            this.skipTransition = true;
            this.tourCameraController.SetMover((CameraMover)null, false);
            this.CameraControllerManager.Controller = (CameraController)this.tourCameraController;
            this.sceneState = TourSceneState.NotStarted;
        }

        public void NextScene()
        {
            if (this.isInvalidTour)
                return;
            switch (this.tourState)
            {
                case TourStep.TourState.Playing:
                case TourStep.TourState.Paused:
                    if (this.CurrentSceneIndex == this.tour.Scenes.Count - 1)
                        break;
                    ++this.CurrentSceneIndex;
                    this.StartNewScene();
                    break;
            }
        }

        public void PreviousScene()
        {
            if (this.isInvalidTour)
                return;
            switch (this.tourState)
            {
                case TourStep.TourState.Playing:
                case TourStep.TourState.Paused:
                    if (this.CurrentSceneIndex == 0)
                        break;
                    --this.CurrentSceneIndex;
                    this.StartNewScene();
                    break;
                case TourStep.TourState.Finished:
                    if (this.CurrentSceneIndex == 0)
                        break;
                    this.StartNewScene();
                    this.tourState = TourStep.TourState.Paused;
                    break;
            }
        }

        public override void Dispose()
        {
        }

        private enum TourState
        {
            NotStarted,
            Playing,
            Paused,
            Finished,
        }

        internal class QueryCompletedContext
        {
            public volatile bool PrepareSceneLayersCompleted;
            public volatile bool SetSceneLayersStarted;
            public volatile bool SetSceneLayersCompleted;
        }
    }
}
