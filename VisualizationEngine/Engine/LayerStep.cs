// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LayerStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    internal class LayerStep : EngineStep
    {
        private DarkGlowRenderer glowRenderer = new DarkGlowRenderer();
        private object layerLock = new object();
        private SelectionStyle currentSelectionRenderingStyle = SelectionStyle.Outline;
        private LayerColorManager colorManager = new LayerColorManager();
        private const int BlockOnDataInputWaitTime = 1000;
        private const int BlockOnDataInputMaxAttemptCount = 20;
        private AnnotationStep annotationStep;
        private bool layersChanged;
        private LayerUIController uiController;

        private List<Layer> layers { get; set; }

        public LayerColorManager ColorManager
        {
            get
            {
                return this.colorManager;
            }
        }

        public LayerRenderingParameters RenderingParameters { get; set; }

        public IList<Layer> Layers
        {
            get
            {
                return (IList<Layer>)this.layers.AsReadOnly();
            }
        }

        public LayerStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher, AnnotationStep annStep)
            : base(dispatcher, eventDispatcher)
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerStep constructor");
            this.annotationStep = annStep;
            this.layers = new List<Layer>();
            this.RenderingParameters = new LayerRenderingParameters();
        }

        public override void OnInitialized()
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerStep.OnInitialized");
            base.OnInitialized();
            this.uiController = new LayerUIController(this);
            this.AddUIController((UIController)this.uiController);
        }

        public bool AddLayer(Layer layer)
        {
            lock (this.layerLock)
            {
                if (this.layers.Contains(layer))
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerStep.AddLayer, layer already in Engine, not added again, layer count={0}", (object)this.layers.Count);
                    return false;
                }
                else
                {
                    this.layers.Add(layer);
                    this.layersChanged = true;
                    layer.EngineDispatcher = this.EngineDispatcher;
                    layer.EventDispatcher = this.EventDispatcher;
                    HitTestableLayer local_0 = layer as HitTestableLayer;
                    if (local_0 != null)
                    {
                        int local_1 = local_0.Id;
                        local_0.Id = this.HitTestManager.ReserveHitTestableId(local_1);
                        if (local_0.Id != local_1)
                            local_0.ResetGraphicsData();
                        this.HitTestManager.AddHitTestable((IHitTestable)local_0);
                        local_0.OnSelectionStyleChanged += new EventHandler<SelectionStyleChangedEventArgs>(this.OnSelectionStyleChanged);
                    }
                    IAnnotatable local_2 = layer as IAnnotatable;
                    if (local_2 != null)
                        this.annotationStep.AddAnnotatable(local_2);
                    IShadowCaster local_3 = layer as IShadowCaster;
                    if (local_3 != null)
                        this.ShadowManager.AddShadowCaster(local_3);
                    layer.ColorManager = this.colorManager;
                    layer.OnAddLayer();
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerStep.AddLayer, added layer id={0}, layer count={1}", local_0 == null ? (object)"(not hitTestableLayer)" : (object)local_0.Id.ToString(), (object)this.layers.Count);
                    return true;
                }
            }
        }

        public bool RemoveLayer(Layer layer)
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerStep.RemoveLayer called, current count={0}", (object)this.layers.Count);
            lock (this.layerLock)
            {
                if (layer != null)
                    layer.EventDispatcher = (Dispatcher)null;
                IShadowCaster local_0 = layer as IShadowCaster;
                if (local_0 != null)
                    this.ShadowManager.RemoveShadowCaster(local_0);
                HitTestableLayer local_1 = layer as HitTestableLayer;
                if (local_1 != null)
                {
                    this.HitTestManager.RemoveHitTestable(local_1.Id);
                    local_1.SetSelected((IList<InstanceId>)null, true);
                    local_1.OnSelectionStyleChanged -= new EventHandler<SelectionStyleChangedEventArgs>(this.OnSelectionStyleChanged);
                }
                IAnnotatable local_2 = layer as IAnnotatable;
                if (local_2 != null)
                    this.annotationStep.RemoveAnnotatable(local_2);
                bool local_3 = this.layers.Remove(layer);
                this.layersChanged = local_3;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerStep.RemoveLayer returning, layer count={0}, removed={1}, layer id={2}", (object)this.layers.Count, (object)(bool)(local_3 ? true : false), local_1 == null ? (object)"(not hitTestableLayer)" : (object)local_1.Id.ToString());
                return local_3;
            }
        }

        private void OnSelectionStyleChanged(object sender, SelectionStyleChangedEventArgs e)
        {
            this.currentSelectionRenderingStyle = e.Style;
        }

        public void SetSelectionMode(SelectionMode mode)
        {
            lock (this.layerLock)
            {
                foreach (Layer item_0 in this.layers)
                {
                    HitTestableLayer local_1 = item_0 as HitTestableLayer;
                    if (local_1 != null)
                        local_1.SetSelectionMode(mode);
                }
            }
        }

        public void DeselectAll()
        {
            lock (this.layerLock)
            {
                foreach (Layer item_0 in this.layers)
                {
                    HitTestableLayer local_1 = item_0 as HitTestableLayer;
                    if (local_1 != null)
                        local_1.SetSelected((IList<InstanceId>)null, true);
                }
            }
        }

        internal override bool PreExecute(SceneState state, int phase)
        {
            if (phase != 0)
                return false;
            if (state.OfflineRender)
                this.BlockOnPendingDataInput();
            lock (this.layerLock)
            {
                bool local_0 = false;
                bool local_1 = false;
                foreach (Layer item_0 in this.layers)
                {
                    if (item_0.IsDirty)
                    {
                        local_0 = true;
                        item_0.IsDirty = false;
                    }
                    if (item_0.DisplayNegativeValues)
                    {
                        ChartLayer local_3 = item_0 as ChartLayer;
                        if (local_3 != null && local_3.HasNegativeValues())
                            local_1 = true;
                    }
                }
                state.GlowEnabled = this.glowRenderer.IsEnabled(state);
                state.TranslucentGlobe = local_1;
                return local_0 || this.layersChanged;
            }
        }

        private void BlockOnPendingDataInput()
        {
            bool flag = true;
            int num1 = 0;
            int num2 = 0;
            while (flag && num2 < 20)
            {
                flag = false;
                int num3 = 0;
                foreach (Layer layer in this.layers)
                {
                    num3 += layer.DataCount;
                    if (layer.DataInputInProgress)
                        flag = true;
                }
                if (num1 == num3)
                {
                    ++num2;
                }
                else
                {
                    num2 = 0;
                    num1 = num3;
                }
                if (flag)
                    Thread.Sleep(1000);
            }
        }

        internal override void Execute(Renderer renderer, SceneState state, int phase)
        {
            lock (this.layerLock)
            {
                this.layersChanged = false;
                bool local_0 = false;
                foreach (Layer item_0 in this.layers)
                {
                    if (item_0.IsOverlay)
                    {
                        local_0 = true;
                        break;
                    }
                }
                if (local_0)
                    renderer.ClearCurrentStencilTarget(0);
                foreach (Layer item_1 in this.layers)
                {
                    item_1.Update(state);
                    HitTestableLayer local_3 = item_1 as HitTestableLayer;
                    if (local_3 != null)
                        local_3.SelectionStyle = this.currentSelectionRenderingStyle;
                }
                bool local_4 = state.VisualTimeFreeze.HasValue && !state.CameraMoved;
                if (phase == 0)
                {
                    renderer.Profiler.BeginSection("[Layers Pre-Draw]");
                    foreach (Layer item_2 in this.layers)
                    {
                        if ((double)item_2.Opacity > 0.0 && !state.IsSkipLayerRelatedSteps)
                            item_2.PreDraw(renderer, state, this.RenderingParameters);
                    }
                    renderer.Profiler.EndSection();
                }
                else
                {
                    if (phase != 1)
                        return;
                    renderer.Profiler.BeginSection("[Layers]");
                    renderer.Profiler.BeginSection("[Layers] Rendering pass");
                    renderer.Profiler.BeginSection("[Layers] Overlays");
                    foreach (Layer item_3 in this.layers)
                    {
                        if (item_3.IsOverlay && (double)item_3.Opacity > 0.0 && !state.IsSkipLayerRelatedSteps)
                            item_3.Draw(renderer, state, this.RenderingParameters);
                    }
                    if (local_0)
                        renderer.ClearCurrentStencilTarget(0);
                    renderer.Profiler.EndSection();
                    if (state.GlowEnabled)
                        renderer.BeginRenderTargetFrame(this.glowRenderer.GetRenderTarget(renderer, state), new Color4F?(new Color4F(0.0f, 1f, 0.0f, 0.0f)), true);
                    bool local_7 = false;
                    try
                    {
                        if (!state.IsSkipLayerRelatedSteps)
                        {
                            foreach (Layer item_4 in this.layers)
                            {
                                HitTestableLayer local_9 = item_4 as HitTestableLayer;
                                if (local_9 != null && local_4)
                                    local_9.HitTestRequested = true;
                                if (!item_4.IsOverlay && (double)item_4.Opacity > 0.0)
                                {
                                    local_7 = true;
                                    item_4.Draw(renderer, state, this.RenderingParameters);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (state.GlowEnabled)
                            renderer.EndRenderTargetFrame();
                    }
                    renderer.Profiler.EndSection();
                    if (local_7)
                        this.glowRenderer.Render(renderer, state);
                    renderer.Profiler.EndSection();
                }
            }
        }

        public override void Dispose()
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerStep.Dispose");
            foreach (Layer layer in this.layers)
                layer.Dispose();
            if (this.glowRenderer != null)
                this.glowRenderer.Dispose();
            if (this.colorManager == null)
                return;
            this.colorManager.Dispose();
        }
    }
}
