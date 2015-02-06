// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionLayer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    public class RegionLayer : InstanceLayer
    {
        private Dictionary<Vector3F, RegionBufferToken> tessellatedRegions =
            new Dictionary<Vector3F, RegionBufferToken>();

        private Dictionary<Vector3F, List<RegionBufferToken>> regionRings =
            new Dictionary<Vector3F, List<RegionBufferToken>>();

        private Dictionary<Vector3F, Task<Tesselator>> pendingRegions = new Dictionary<Vector3F, Task<Tesselator>>();
        private List<InstanceId> emptyRegions = new List<InstanceId>();
        private List<InstanceId> failedRegions = new List<InstanceId>();
        private RegionBuffer regionBuffer = new RegionBuffer();
        private ConcurrentBag<Tesselator> tesselatorPool = new ConcurrentBag<Tesselator>();
        private SemaphoreSlim poolSemaphore = new SemaphoreSlim(24);
        private CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private object regionLock = new object();
        private object rangeCalculatorLock = new object();
        private double[] minValuesPerShift = new double[2048];
        private double[] maxValuesPerShift = new double[2048];
        private double[] scalePerShift = new double[2048];
        private List<RegionBufferToken> tokensToRender = new List<RegionBufferToken>();
        private const int CountryAD1RegionLod = 1;
        private const int DefaultRegionLod = 0;
        private const int RegionPositionDigitsOfPrecision = 5;
        private const int MaxConcurrentTesselationJobs = 24;
        private EntityType entityType;
        private IRegionProvider regionProvider;
        private RegionLayerShadingMode desiredShadingMode;
        private RegionLayerShadingMode? currentShadingMode;
        private bool planarCoordinates;
        private RasterizerState rasterizerState;
        private RasterizerState hitTestRasterizerState;
        private RasterizerState wireframeRasterizerState;
        private DepthStencilState depthStencilState;
        private DepthStencilState depthStencilHitTest;
        private DepthStencilState depthStencilSelection;
        private BlendState blendState;
        private RegionRenderingTechnique technique;
        private OutlineTechnique outlineTechnique;
        private ValueRangeCalculator rangeCalculator;
        private double minGlobalValue;
        private double globalScale;
        private double minLocalValue;
        private double localScale;
        private bool tessellationIsComplete;

        public EntityType RegionEntityType
        {
            get { return this.entityType; }
            set
            {
                if (value == this.entityType)
                    return;
                this.entityType = value;
                this.ClearRegions();
            }
        }

        public RegionLayerShadingMode ShadingMode
        {
            get { return this.desiredShadingMode; }
            set
            {
                this.desiredShadingMode = value;
                this.IsDirty = true;
            }
        }

        public RegionLayerStatistics Stats { get; private set; }

        public override bool DisplayNullValues
        {
            set
            {
                base.DisplayNullValues = value;
                lock (this.rangeCalculatorLock)
                    this.rangeCalculator = null;
                this.instanceCountUsedForScale = 0;
            }
        }

        public override bool DisplayZeroValues
        {
            set
            {
                base.DisplayZeroValues = value;
                lock (this.rangeCalculatorLock)
                    this.rangeCalculator = null;
                this.instanceCountUsedForScale = 0;
            }
        }

        public override bool DisplayNegativeValues
        {
            set
            {
                base.DisplayNegativeValues = value;
                lock (this.rangeCalculatorLock)
                    this.rangeCalculator = null;
                this.instanceCountUsedForScale = 0;
            }
        }

        protected override bool OptimizeSpatialIndex
        {
            get { return this.tessellationIsComplete; }
        }

        internal override int BlockSize
        {
            get { return 4096; }
        }

        internal override bool InflatesBounds
        {
            get { return true; }
        }

        public event EventHandler<RegionEventArgs> OnRegionReady;

        public event EventHandler<RegionEventArgs> OnRegionCompleted;

        public event EventHandler<RegionScaleEventArgs> OnShadingScaleChanged;

        public RegionLayer(IRegionProvider regionProv, IInstanceIdRelationshipProvider instanceIdRelationshipProvider)
            : base(LayerType.RegionChart, instanceIdRelationshipProvider)
        {
            this.regionProvider = regionProv;
            this.entityType = EntityType.CountryRegion;
            this.Stats = new RegionLayerStatistics();
            this.InitializeRenderStates();
            this.RegionEntityType = EntityType.CountryRegion;
            this.ShadingMode = RegionLayerShadingMode.Global;
            this.ClearRegions();
            this.cancellationSource = new CancellationTokenSource();
            this.IsOverlay = true;
            this.UseLogarithmicClampedValue = true;
            this.OffsetNegativeValues = true;
        }

        private void InitializeRenderStates()
        {
            this.rasterizerState = RasterizerState.Create(new RasterizerStateDescription()
            {
                CullMode = CullMode.Front,
                FillMode = FillMode.Solid,
                AntialiasedLineEnable = true,
                MultisampleEnable = true
            });
            this.wireframeRasterizerState = RasterizerState.Create(new RasterizerStateDescription()
            {
                CullMode = CullMode.Front,
                FillMode = FillMode.Wireframe
            });
            this.depthStencilState = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                DepthEnable = false,
                DepthWriteEnable = false,
                StencilEnable = true,
                StencilBackFace = new StencilDescription()
                {
                    PassOperation = StencilOperation.IncrementSaturate,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    Function = ComparisonFunction.Equal
                },
                StencilReferenceValue = 0
            });
            this.depthStencilSelection = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                DepthEnable = false,
                DepthWriteEnable = false,
                StencilEnable = false
            });
            this.blendState = BlendState.Create(new BlendStateDescription()
            {
                BlendEnable = true,
                SourceBlend = BlendFactor.SourceAlpha,
                SourceBlendAlpha = BlendFactor.One,
                DestBlend = BlendFactor.InvSourceAlpha,
                DestBlendAlpha = BlendFactor.One,
                BlendOp = BlendOperation.Add,
                BlendOpAlpha = BlendOperation.Add,
                WriteMask = RenderTargetWriteMask.All
            });
        }

        public override void BeginDataInput(int estimate, bool progressiveDataInput, bool ignoreData,
            CustomSpaceTransform dataCustomSpace, double minInstanceValue, double maxInstanceValue, DateTime? minTime,
            DateTime? maxTime)
        {
            base.BeginDataInput(estimate, progressiveDataInput, ignoreData, dataCustomSpace, minInstanceValue,
                maxInstanceValue, minTime, maxTime);
            this.tessellationIsComplete = false;
        }

        private bool GetTessellation(ref Vector3F position, out RegionBufferToken region)
        {
            position = this.GetCanonicalRegionPosition(position);
            return this.tessellatedRegions.TryGetValue(position, out region);
        }

        private RegionBufferToken GetRegionSubset(Vector3F position, InstanceId id, SceneState state)
        {
            RegionBufferToken region = null;
            List<RegionBufferToken> list = null;
            if (!this.GetTessellation(ref position, out region))
            {
                if (this.pendingRegions.ContainsKey(position))
                {
                    if (this.pendingRegions[position].IsFaulted || this.pendingRegions[position].IsCompleted)
                    {
                        if (this.pendingRegions[position].IsCompleted)
                        {
                            Tesselator result = this.pendingRegions[position].Result;
                            if (result != null)
                            {
                                try
                                {
                                    RegionTriangleList tesselatedRegion = result.TesselatedRegion;
                                    if (tesselatedRegion.GetVertices().Count > 0)
                                    {
                                        region = this.regionBuffer.AddRegion(tesselatedRegion, this.planarCoordinates);
                                        list = this.regionBuffer.AddRegionRings(tesselatedRegion, this.planarCoordinates);
                                    }
                                    else
                                        this.emptyRegions.Add(id);
                                }
                                finally
                                {
                                    this.tesselatorPool.Add(result);
                                }
                            }
                            else
                                this.failedRegions.Add(id);
                        }
                        else
                            this.failedRegions.Add(id);
                        this.tessellatedRegions.Add(position, region);
                        this.regionRings.Add(position, list);
                        this.UpdateUsageStats(region);
                        this.poolSemaphore.Release();
                        this.pendingRegions.Remove(position);
                        if (this.EventDispatcher != null)
                            this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                            {
                                if (this.OnRegionReady == null)
                                    return;
                                this.OnRegionReady(this,
                                    new RegionEventArgs(
                                        this.tessellatedRegions.Count - this.failedRegions.Count -
                                        this.emptyRegions.Count, this.failedRegions, this.emptyRegions,
                                        this.OptimizeSpatialIndex));
                            }));
                    }
                }
                else
                    position = this.RequestRegionData(position, state);
            }
            return region;
        }

        private Vector3F GetCanonicalRegionPosition(Vector3F position)
        {
            if (this.planarCoordinates)
                position = (Vector3F) Coordinates.Flat3DToGeo(position.ToVector3D()).To3D(false);
            position.X = (float) Math.Round(position.X, 5, MidpointRounding.AwayFromZero);
            position.Y = (float) Math.Round(position.Y, 5, MidpointRounding.AwayFromZero);
            position.Z = (float) Math.Round(position.Z, 5, MidpointRounding.AwayFromZero);
            return position;
        }

        private Vector3F RequestRegionData(Vector3F position, SceneState state)
        {
            Coordinates queryCoords = Coordinates.World3DToGeo(position.ToVector3D());
            CancellationToken cancellationToken = this.cancellationSource.Token;
            Task<Tesselator> queryAndTesselatorTask =
                this.regionProvider.GetRegionAsync(
                queryCoords.Latitude * Constants.DegreesPerRadian,
                queryCoords.Longitude * Constants.DegreesPerRadian,
                this.entityType == EntityType.CountryRegion || 
                this.entityType == EntityType.AdminDivision1 ? 1 : 0,
                this.RegionEntityType, cancellationToken, true, false, true)
                    .ContinueWith<Tesselator>((task =>
                    {
                        try
                        {
                            this.poolSemaphore.Wait(cancellationToken);
                        }
                        catch (OperationCanceledException ex)
                        {
                            return (Tesselator) null;
                        }
                        List<RegionData> regionDataList = task.Result;
                        if (regionDataList != null)
                        {
                            Tesselator tesselator = null;
                            try
                            {
                                if (!this.tesselatorPool.TryTake(out tesselator))
                                    tesselator = new Tesselator();
                                tesselator.BeginPolygon();
                                foreach (RegionData regionData in regionDataList)
                                {
                                    Coordinates coordsInPolygon = regionData.Polygon[0];
                                    tesselator.BeginRing(coordsInPolygon.Longitude, coordsInPolygon.Latitude);
                                    for (int i = 1; i < regionData.Polygon.Count; ++i)
                                    {
                                        coordsInPolygon = regionData.Polygon[i];
                                        tesselator.AddVertex(coordsInPolygon.Longitude, coordsInPolygon.Latitude);
                                    }
                                }
                                tesselator.EndPolygon();
                            }
                            catch (Exception ex)
                            {
                                if (tesselator != null)
                                    this.tesselatorPool.Add(tesselator);
                                VisualizationTraceSource.Current.Fail(ex);
                                return (Tesselator) null;
                            }
                            return tesselator;
                        }
                        else
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0,
                                "Error retrieving region polygon.");
                            return (Tesselator) null;
                        }
                    }));
            this.pendingRegions.Add(position, queryAndTesselatorTask);
            if (state.OfflineRender)
                queryAndTesselatorTask.Wait();
            return position;
        }

        private void UpdateUsageStats(RegionBufferToken token)
        {
            if (token != null)
            {
                if (token.VertexCount < this.Stats.MinRegionVertexCount)
                    this.Stats.MinRegionVertexCount = token.VertexCount;
                if (token.VertexCount > this.Stats.MaxRegionVertexCount)
                    this.Stats.MaxRegionVertexCount = token.VertexCount;
            }
            int num1 = 0;
            int num2 = 0;
            foreach (RegionBufferToken regionBufferToken in this.tessellatedRegions.Values)
            {
                if (regionBufferToken != null)
                {
                    num1 += regionBufferToken.VertexCount;
                    ++num2;
                }
            }
            this.Stats.AverageRegionVertexCount = num2 == 0 ? 0 : num1/num2;
            this.Stats.RegionCount = num2;
        }

        internal override void Update(SceneState state)
        {
            base.Update(state);
            if (this.tessellationIsComplete || !this.DataIntakeComplete ||
                (this.InstanceClusterCount == 0 || this.InstanceBlocks == null))
                return;
            foreach (InstanceBlock instanceBlock in this.InstanceBlocks)
            {
                Vector3F vector3F = Vector3F.Empty;
                for (int pos = 0; pos < instanceBlock.Count; ++pos)
                {
                    Vector3F canonicalRegionPosition =
                        this.GetCanonicalRegionPosition(instanceBlock.GetInstancePositionAt(pos));
                    if (!(canonicalRegionPosition == vector3F))
                    {
                        vector3F = canonicalRegionPosition;
                        if (!this.tessellatedRegions.ContainsKey(canonicalRegionPosition))
                            return;
                    }
                }
            }
            this.tessellationIsComplete = true;
            if (this.EventDispatcher == null)
                return;
            Action action = delegate
            {
                if (this.OnRegionCompleted == null)
                    return;
                this.OnRegionCompleted(this,
                    new RegionEventArgs(
                        this.tessellatedRegions.Count - this.failedRegions.Count - this.emptyRegions.Count,
                        this.failedRegions, this.emptyRegions, true));
            };
            this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        protected override bool DrawColor(Renderer renderer, SceneState state, DrawParameters parameters)
        {
            if (renderer == null || state == null || (parameters == null || parameters.InstanceCount == 0))
                return false;
            lock (this.regionLock)
            {
                if (this.technique == null)
                    this.technique = new RegionRenderingTechnique(this.SharedRenderParameters,
                        this.ColorRenderParameters);
                // 设置深度模板点击测试状态
                if (this.depthStencilHitTest == null && parameters.DepthStencil != null)
                {
                    DepthStencilStateDescription paramsDSSD = parameters.DepthStencil.GetStateDescription();
                    DepthStencilStateDescription localDSSD = this.depthStencilState.GetStateDescription();
                    paramsDSSD.StencilEnable = localDSSD.StencilEnable;
                    paramsDSSD.StencilFrontFace = localDSSD.StencilFrontFace;
                    paramsDSSD.StencilBackFace = localDSSD.StencilBackFace;
                    paramsDSSD.StencilReferenceValue = localDSSD.StencilReferenceValue;
                    this.depthStencilHitTest = DepthStencilState.Create(paramsDSSD);
                }
                this.technique.SetRenderStates(
                    parameters.DepthStencil == null ? null : this.depthStencilHitTest,
                    parameters.Blend, parameters.Rasterizer);
                int count;
                this.technique.ColorParameters = this.ColorManager.GetColors(out count);
                this.technique.FrameId = parameters.FrameId;
                this.technique.DesaturateFactor = this.DesaturateFactor;
                if (parameters.DrawStyle == DrawStyle.HitTest)
                {
                    this.technique.ColorShadingEnabled = false;
                    this.technique.LocalShadingEnabled = false;
                }
                else
                    this.UpdateShading();
                if (parameters.QueryType == InstanceBlockQueryType.ZeroInstances ||
                    parameters.QueryType == InstanceBlockQueryType.NullInstances)
                    this.technique.LocalShadingEnabled = false;
                if (state.FlatteningFactor == 1.0 != this.planarCoordinates)
                {
                    this.planarCoordinates = !this.planarCoordinates;
                    this.regionBuffer.ReprojectVertices(this.planarCoordinates);
                }
                using (VertexBuffer buffer = parameters.SourceStreamBuffer.PeekVertexBuffer())
                {
                    VertexBuffer idVertexBuffer = null;
                    if (parameters.DrawStyle == DrawStyle.HitTest)
                        idVertexBuffer = parameters.SourceIdStreamBuffer.PeekVertexBuffer();
                    try
                    {
                        this.technique.Mode = parameters.DrawStyle == DrawStyle.HitTest
                            ? RegionRenderingTechnique.RenderMode.HitTest
                            : RegionRenderingTechnique.RenderMode.Color;
                        renderer.SetRasterizerState(parameters.RenderingOptions.Wireframe
                            ? this.wireframeRasterizerState
                            : this.rasterizerState);
                        renderer.SetBlendState(this.blendState);
                        renderer.SetDepthStencilState(this.depthStencilState);
                        renderer.SetEffect(this.technique);
                        if (parameters.DrawStyle == DrawStyle.HitTest)
                        {
                            if (this.hitTestRasterizerState == null)
                                this.hitTestRasterizerState = RasterizerState.Create(new RasterizerStateDescription()
                                {
                                    CullMode = CullMode.None,
                                    AntialiasedLineEnable = parameters.Rasterizer.GetState().AntialiasedLineEnable,
                                    DepthClipEnable = parameters.Rasterizer.GetState().DepthClipEnable,
                                    FillMode = parameters.Rasterizer.GetState().FillMode,
                                    MultisampleEnable = parameters.Rasterizer.GetState().MultisampleEnable,
                                    ScissorEnable = parameters.Rasterizer.GetState().ScissorEnable
                                });
                            renderer.SetRasterizerState(this.hitTestRasterizerState);
                        }
                        this.tokensToRender.Clear();
                        for (int i = 0; i < parameters.InstanceCount; ++i)
                        {
                            Vector3F insPosition = parameters.PositionProvider.GetInstancePositionAtGeoIndex(i,
                                parameters.QueryType);
                            if (!(insPosition == Vector3F.Empty))
                            {
                                InstanceId insId = parameters.PositionProvider.GetInstanceIdAtGeoIndex(i,
                                    parameters.QueryType);
                                RegionBufferToken region = this.GetRegionSubset(insPosition, insId, state);
                                if (state.OfflineRender && region == null)
                                    region = this.GetRegionSubset(insPosition, insId, state);
                                this.tokensToRender.Add(region);
                            }
                        }
                        RegionBufferToken lastTokenRegion = null;
                        renderer.RenderLockEnter();
                        try
                        {
                            int renderedRegionCount = 0;
                            foreach (RegionBufferToken tokenRegion in this.tokensToRender)
                            {
                                if (tokenRegion != null)
                                {
                                    if (lastTokenRegion == null || lastTokenRegion.Indices != tokenRegion.Indices || tokenRegion.Indices.IsDirty)
                                        renderer.SetIndexSourceNoLock(tokenRegion.Indices);
                                    if (lastTokenRegion == null || lastTokenRegion.Vertices != tokenRegion.Vertices ||
                                        tokenRegion.Vertices.IsDirty)
                                    {
                                        Renderer r = renderer;
                                        VertexBuffer[] vertexBufferArray;
                                        if (parameters.DrawStyle != DrawStyle.HitTest)
                                            vertexBufferArray = new VertexBuffer[3]
                                            {
                                                tokenRegion.Vertices,
                                                buffer,
                                                null
                                            };
                                        else
                                            vertexBufferArray = new VertexBuffer[3]
                                            {
                                                tokenRegion.Vertices,
                                                buffer,
                                                idVertexBuffer
                                            };
                                        r.SetVertexSourceNoLock(vertexBufferArray);
                                    }
                                    renderer.DrawIndexedInstancedNoLock(tokenRegion.StartVertex, tokenRegion.VertexCount, renderedRegionCount,
                                        1, PrimitiveTopology.TriangleList);
                                }
                                lastTokenRegion = tokenRegion;
                                ++renderedRegionCount;
                            }
                        }
                        finally
                        {
                            renderer.RenderLockExit();
                        }
                        renderer.SetTexture(0, state.GlobeDepth);
                        this.DrawOutlines(renderer, parameters, buffer, idVertexBuffer);
                        renderer.SetTexture(0, null);
                    }
                    finally
                    {
                        if (idVertexBuffer != null)
                            idVertexBuffer.Dispose();
                    }
                    renderer.SetVertexSource(new VertexBuffer[3]);
                }
                this.IsDirty = this.pendingRegions.Count > 0;
                return true;
            }
        }

        private void DrawOutlines(Renderer renderer, DrawParameters parameters, VertexBuffer vb, VertexBuffer idBuffer)
        {
            if (parameters.DrawStyle != DrawStyle.Color)
                return;
            this.technique.Mode = RegionRenderingTechnique.RenderMode.Outline;
            Color4F regionOutlineColor = parameters.RenderingOptions.RegionOutlineColor;
            regionOutlineColor.A *= this.Opacity;
            this.technique.OutlineColor = regionOutlineColor;
            this.technique.BrightnessModeEnabled = parameters.RenderingOptions.RegionBrightnessEnabled;
            renderer.SetEffect(this.technique);
            renderer.SetRasterizerState(this.rasterizerState);
            renderer.SetBlendState(this.blendState);
            renderer.SetDepthStencilState(this.depthStencilState);
            RegionBufferToken regionBufferToken = null;
            renderer.RenderLockEnter();
            try
            {
                for (int index1 = 0; index1 < parameters.InstanceCount; ++index1)
                {
                    Vector3F positionAtGeoIndex = parameters.PositionProvider.GetInstancePositionAtGeoIndex(index1,
                        parameters.QueryType);
                    List<RegionBufferToken> list;
                    if (positionAtGeoIndex != Vector3F.Empty &&
                        this.regionRings.TryGetValue(this.GetCanonicalRegionPosition(positionAtGeoIndex), out list) &&
                        list != null)
                    {
                        for (int index2 = 0; index2 < list.Count; ++index2)
                        {
                            if (regionBufferToken == null || regionBufferToken.Vertices != list[index2].Vertices)
                            {
                                Renderer renderer1 = renderer;
                                VertexBuffer[] vertexBuffers;
                                if (parameters.DrawStyle != DrawStyle.HitTest)
                                    vertexBuffers = new VertexBuffer[2]
                                    {
                                        list[index2].Vertices,
                                        vb
                                    };
                                else
                                    vertexBuffers = new VertexBuffer[3]
                                    {
                                        list[index2].Vertices,
                                        vb,
                                        idBuffer
                                    };
                                renderer1.SetVertexSourceNoLock(vertexBuffers);
                            }
                            renderer.DrawInstancedNoLock(list[index2].StartVertex, list[index2].VertexCount, index1, 1,
                                PrimitiveTopology.LineStrip);
                            regionBufferToken = list[index2];
                        }
                    }
                }
            }
            finally
            {
                renderer.RenderLockExit();
            }
        }

        private void UpdateShading()
        {
            this.technique.ShiftGlobalShadingEnabled = false;
            float num = (float) this.GetInstanceValueOffset();
            switch (this.ShadingMode)
            {
                case RegionLayerShadingMode.FullBleed:
                    this.technique.ColorShadingEnabled = false;
                    this.technique.LocalShadingEnabled = false;
                    break;
                case RegionLayerShadingMode.Global:
                    this.technique.ColorShadingEnabled = true;
                    this.technique.LocalShadingEnabled = false;
                    this.technique.MinValue = (float) this.minGlobalValue + num;
                    this.technique.ValueScale = (float) this.globalScale;
                    break;
                case RegionLayerShadingMode.Local:
                    this.technique.ColorShadingEnabled = true;
                    this.technique.LocalShadingEnabled = true;
                    this.technique.MinValue = (float) this.minLocalValue;
                    this.technique.ValueScale = (float) this.localScale;
                    break;
                case RegionLayerShadingMode.ShiftGlobal:
                    this.technique.ColorShadingEnabled = true;
                    this.technique.LocalShadingEnabled = false;
                    this.technique.ShiftGlobalShadingEnabled = true;
                    this.technique.SetMinValueAndScalePerShift(this.minValuesPerShift, this.scalePerShift, (double) num);
                    break;
            }
            if (this.currentShadingMode.HasValue && this.currentShadingMode.Value != this.ShadingMode)
            {
                ValueRangeCalculator calc = null;
                lock (this.rangeCalculatorLock)
                    calc = this.rangeCalculator;
                if (calc != null)
                    this.RaiseShadingScaleChanged(calc);
            }
            this.currentShadingMode = new RegionLayerShadingMode?(this.ShadingMode);
        }

        protected override bool DrawSelection(Renderer renderer, SceneState state, DrawParameters parameters)
        {
            lock (this.regionLock)
            {
                if (this.SelectedItems.Count == 0)
                    return false;
                if (this.DesaturationEnabled)
                {
                    int local_0;
                    this.technique.ColorParameters = this.ColorManager.GetColors(out local_0);
                    this.technique.FrameId = parameters.FrameId;
                    this.technique.DesaturateFactor = 0.0f;
                    this.technique.Mode = RegionRenderingTechnique.RenderMode.Color;
                    renderer.SetEffect((EffectTechnique) this.technique);
                }
                else
                {
                    if (this.outlineTechnique == null)
                    {
                        this.outlineTechnique = new OutlineTechnique(this.SharedRenderParameters);
                        this.outlineTechnique.RegionsMode = true;
                        this.outlineTechnique.DepthEnabled = true;
                    }
                    renderer.SetEffect(this.outlineTechnique);
                    renderer.SetTexture(0, state.GlobeDepth);
                    if (parameters.RenderingOptions != null)
                    {
                        this.outlineTechnique.OutlineWidth = parameters.RenderingOptions.OutlineWidth;
                        this.outlineTechnique.OutlineOffset = parameters.RenderingOptions.OutlineOffset;
                        this.outlineTechnique.OutlineColor = parameters.RenderingOptions.InnerOutlineColor;
                        this.outlineTechnique.OutlineSecondaryColor = parameters.RenderingOptions.OuterOutlineColor;
                    }
                }
                renderer.SetRasterizerState(this.rasterizerState);
                renderer.SetBlendState(this.blendState);
                renderer.SetDepthStencilState(this.depthStencilSelection);
                int local_1 = 0;
                RegionBufferToken local_2 = null;
                VertexBuffer local_3 = null;
                foreach (InstanceId item_0 in this.SelectedItems)
                {
                    Vector3F local_5 = this.GetInstancePosition(item_0);
                    if (this.DesaturationEnabled)
                    {
                        RegionBufferToken local_6 = this.GetRegionSubset(local_5, item_0, state);
                        if (local_6 != null)
                        {
                            if (local_2 == null || local_6.Indices != local_2.Indices || local_6.Indices.IsDirty)
                                renderer.SetIndexSource(local_6.Indices);
                            if (local_2 == null || local_6.Vertices != local_2.Vertices || local_6.Vertices.IsDirty)
                            {
                                local_3 = parameters.SourceStreamBuffer.PeekVertexBuffer();
                                renderer.SetVertexSource(new VertexBuffer[2]
                                {
                                    local_6.Vertices,
                                    local_3
                                });
                            }
                            renderer.DrawIndexedInstanced(local_6.StartVertex, local_6.VertexCount, local_1, 1,
                                PrimitiveTopology.TriangleList);
                        }
                        local_2 = local_6;
                    }
                    else
                    {
                        List<RegionBufferToken> local_7;
                        if (this.regionRings.TryGetValue(this.GetCanonicalRegionPosition(local_5), out local_7) &&
                            local_7 != null)
                        {
                            for (int local_8 = 0; local_8 < local_7.Count; ++local_8)
                            {
                                if (local_2 == null || local_7[local_8].Vertices != local_2.Vertices)
                                    renderer.SetVertexSource(local_7[local_8].Vertices);
                                renderer.Draw(local_7[local_8].StartVertex, local_7[local_8].VertexCount,
                                    PrimitiveTopology.LineStrip);
                                local_2 = local_7[local_8];
                            }
                        }
                    }
                    ++local_1;
                }
                if (local_3 != null)
                    local_3.Dispose();
                renderer.SetVertexSource(new VertexBuffer[4]);
                return true;
            }
        }

        protected override bool DrawShadowVolume(Renderer renderer, SceneState state, DrawParameters parameters)
        {
            return false;
        }

        protected override DrawMode GetDrawMode(bool preRender, bool hitTest)
        {
            DrawMode drawMode = (DrawMode) 3;
            if (this.ShadingMode == RegionLayerShadingMode.Local)
                drawMode |= DrawMode.PieChart;
            if (this.IgnoreData)
                drawMode |= DrawMode.PointMarker;
            return drawMode;
        }

        internal override RenderQuery GetSpatialQuery(SceneState state, List<int> queryResult)
        {
            return RenderQuery.GetQuery(1.0 - state.FlatteningFactor, state.ViewProjection, 1f, (InstanceLayer) this,
                queryResult);
        }

        internal override double GetMaxInstanceExtent(float scale, int maxShift)
        {
            return 0.0;
        }

        protected override void OnUpdate(SceneState state)
        {
            lock (this.regionLock)
            {
                this.UseTextureForNegativeValues = true;
                this.UseSqrtValue = false;
                this.ShowOnlyMaxValueEnabled = true;
                this.VariableScale = new Vector2F(1f, 0.0f);
                this.FixedScale = new Vector2F(0.0f, 0.0f);
                this.ViewScaleEnabled = false;
                if (this.instanceCountUsedForScale >= this.instanceList.Count)
                    return;
                this.UpdateScales();
                this.instanceCountUsedForScale = this.instanceList.Count;
                this.scaleUpdateNeeded = true;
            }
        }

        private void UpdateScales()
        {
            ValueRangeCalculator calc;
            lock (this.rangeCalculatorLock)
            {
                if (this.rangeCalculator == null)
                    this.rangeCalculator = new ValueRangeCalculator(this.instanceList, this.visualTimeRange != 0.0,
                        this.DisplayZeroValues, this.DisplayNegativeValues, this.UseLogarithmicClampedValue);
                this.rangeCalculator.Compute(this.instanceCountUsedForScale);
                double local_1 = this.rangeCalculator.MaxOverallValue - this.rangeCalculator.MinOverallMaxValue;
                this.minGlobalValue = this.rangeCalculator.MinOverallMaxValue;
                this.globalScale = local_1 <= 1E-06 ? 0.0 : 1.0/local_1;
                double local_2 = this.rangeCalculator.MaxLocalMaxValue - this.rangeCalculator.MinLocalMaxValue;
                this.minLocalValue = this.rangeCalculator.MinLocalMaxValue;
                this.localScale = local_2 <= 1E-06 ? 0.0 : 1.0/local_2;
                this.rangeCalculator.GetValuesPerShift(this.minValuesPerShift, this.maxValuesPerShift);
                for (int local_3 = 0; local_3 < this.maxValuesPerShift.Length; ++local_3)
                {
                    double local_4 = this.maxValuesPerShift[local_3] - this.minValuesPerShift[local_3];
                    if (local_4 > 1E-06)
                    {
                        this.scalePerShift[local_3] = 1.0/local_4;
                    }
                    else
                    {
                        this.minValuesPerShift[local_3] = 0.0;
                        this.scalePerShift[local_3] = 0.0;
                    }
                }
                calc = this.rangeCalculator;
            }
            this.RaiseShadingScaleChanged(calc);
        }

        private void RaiseShadingScaleChanged(ValueRangeCalculator calc)
        {
            if (this.EventDispatcher == null || this.OnShadingScaleChanged == null)
                return;
            this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                double[] minValues = new double[2048];
                double[] maxValues = new double[2048];
                calc.GetValuesPerShift(minValues, maxValues);
                if (this.OnShadingScaleChanged == null)
                    return;
                this.OnShadingScaleChanged((object) this,
                    new RegionScaleEventArgs(calc, minValues, maxValues, this.ShadingMode, this.MaxAbsValue));
            }));
        }

        protected override void OverrideDimensionAndScales(InstanceBlockQueryType queryType, ref float dimension,
            ref Vector2F fixedScale, ref Vector2F variableScale)
        {
            base.OverrideDimensionAndScales(queryType, ref dimension, ref fixedScale, ref variableScale);
        }

        protected override bool RenderOnPreDraw()
        {
            return false;
        }

        public override void EraseAllData()
        {
            base.EraseAllData();
            this.tessellationIsComplete = false;
            if (this.EngineDispatcher != null)
                this.EngineDispatcher.RunOnRenderThread((() => this.ClearRegions()));
            else
                this.ClearRegions();
        }

        private void ClearRegions()
        {
            lock (this.regionLock)
            {
                this.tessellatedRegions.Clear();
                this.regionRings.Clear();
                this.failedRegions.Clear();
                this.emptyRegions.Clear();
                this.minGlobalValue = 0.0;
                this.globalScale = 0.0;
                this.minLocalValue = 0.0;
                this.localScale = 0.0;
                this.tessellationIsComplete = false;
                lock (this.rangeCalculatorLock)
                    this.rangeCalculator = (ValueRangeCalculator) null;
                this.instanceCountUsedForScale = 0;
                try
                {
                    this.cancellationSource.Cancel();
                }
                catch (AggregateException exception_0)
                {
                }
                if (this.pendingRegions.Count > 0)
                    this.poolSemaphore.Release(this.pendingRegions.Count);
                this.pendingRegions.Clear();
                this.cancellationSource = new CancellationTokenSource();
                this.regionBuffer.Clear();
                this.Stats = new RegionLayerStatistics();
            }
        }

        internal override unsafe void UpdateBounds(InstanceBlock block, ref Box2D latLongBounds, ref Box3D bounds3D)
        {
            if (this.regionBuffer == null)
                return;
            Vector3F vector3F = Vector3F.Empty;
            for (int pos = 0; pos < block.Count; ++pos)
            {
                Vector3F instancePositionAt = block.GetInstancePositionAt(pos);
                RegionBufferToken region;
                if (this.GetTessellation(ref instancePositionAt, out region) && instancePositionAt != vector3F)
                {
                    vector3F = instancePositionAt;
                    if (region != null)
                    {
                        int* numPtr = (int*) region.Indices.GetData().ToPointer();
                        if ((IntPtr) numPtr != IntPtr.Zero)
                        {
                            int num = region.StartVertex + region.VertexCount - 1;
                            if (num < region.Indices.IndexCount)
                            {
                                for (int index1 = region.StartVertex; index1 <= num; ++index1)
                                {
                                    int index2 = numPtr[index1];
                                    if (index2 < region.Vertices.VertexCount)
                                    {
                                        Vector3D vector3D = this.regionBuffer.GetVertex(region, index2, false);
                                        vector3D.AssertIsUnitVector();
                                        bounds3D.UpdateWith(vector3D);
                                        vector3D = Coordinates.UnitSphereToFlatMap(vector3D);
                                        latLongBounds.UpdateWith(vector3D.Z, vector3D.Y);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal override unsafe void AddInstancePointsToList(InstanceId id, List<Vector3D> locations)
        {
            if (this.regionBuffer == null)
                return;
            Vector3F instancePosition = this.GetInstancePosition(id);
            RegionBufferToken region;
            if (!this.GetTessellation(ref instancePosition, out region) || region == null)
                return;
            int* numPtr = (int*) region.Indices.GetData().ToPointer();
            if ((IntPtr) numPtr == IntPtr.Zero)
                return;
            int num = region.StartVertex + region.VertexCount - 1;
            if (num >= region.Indices.IndexCount)
                return;
            for (int index1 = region.StartVertex; index1 <= num; ++index1)
            {
                int index2 = numPtr[index1];
                if (index2 >= region.Vertices.VertexCount)
                    break;
                Vector3D vertex = this.regionBuffer.GetVertex(region, index2, this.planarCoordinates);
                locations.Add(vertex);
            }
        }

        public override void Dispose()
        {
            lock (this.regionLock)
            {
                base.Dispose();
                this.EngineDispatcher.RunOnRenderThread((() =>
                {
                    this.ClearRegions();
                    DisposableResource[] disposableResourceArray = new DisposableResource[10]
                    {
                        this.regionBuffer,
                        this.rasterizerState,
                        this.wireframeRasterizerState,
                        this.depthStencilState,
                        this.depthStencilSelection,
                        this.depthStencilHitTest,
                        this.blendState,
                        this.technique,
                        this.outlineTechnique,
                        this.hitTestRasterizerState
                    };
                    foreach (DisposableResource disposableResource in disposableResourceArray)
                    {
                        if (disposableResource != null)
                            disposableResource.Dispose();
                    }
                }));
            }
        }
    }
}
