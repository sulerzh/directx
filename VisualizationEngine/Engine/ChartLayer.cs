using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    public sealed class ChartLayer : InstanceLayer
    {
        private Dictionary<InstanceBlockQueryType, InstancedVisual> visualType = new Dictionary<InstanceBlockQueryType, InstancedVisual>(4);
        private InstancedVisual[] visuals;
        private InstancedVisual instanceVisual;
        private InstancedVisual nullVisual;
        private InstancedVisual zeroVisual;
        private RasterizerState backCullRasterizerState;
        private RasterizerState frontCullRasterizerState;
        private RasterizerState backCullRasterizerStateWireframe;
        private RasterizerState frontCullRasterizerStateWireframe;
        private DepthStencilState depthStencilState;
        private DepthStencilState ceilingDepthStencilState;
        private DepthStencilState wallDepthStencilState;
        private BlendState blendState;
        private BlendState transparencyBlendState;
        private InstanceRenderingTechnique renderingTechnique;
        private ShadowVolumeTechnique shadowTechnique;
        private OutlineTechnique outlineTechnique;
        private Chart chart;
        private InstancedShape[] shapes;
        private bool visualsChanged;

        internal short NullMarkerId
        {
            get
            {
                return (short)(this.visuals.Length - 2);
            }
        }

        internal short ZeroMarkerId
        {
            get
            {
                return (short)(this.visuals.Length - 1);
            }
        }

        protected override bool OptimizeSpatialIndex
        {
            get
            {
                return !this.DataInputInProgress;
            }
        }

        public override float AnnotationAnchorHeight
        {
            get
            {
                return this.chart.AnnotationAnchorHeight;
            }
        }

        public override LayerType LayerType
        {
            protected set
            {
                if (this.shapes == null)
                    return;
                LayerType layerType = this.LayerType;
                base.LayerType = value;
                if (layerType == value)
                    return;
                if (this.EngineDispatcher != null)
                    this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod)(() => this.UpdateChartType()));
                else
                    this.UpdateChartType();
            }
        }

        internal override float HorizontalSpacing
        {
            get
            {
                return (float)this.chart.HorizontalSpacing * this.instanceVisual.HorizontalSpacing;
            }
        }

        public ChartLayer(InstancedShape[] visualShapes, LayerType layerType, IInstanceIdRelationshipProvider idProvider)
            : base(layerType, idProvider)
        {
            if (visualShapes == null || visualShapes.Length < 1)
                throw new ArgumentNullException("visualShapes");
            this.shapes = visualShapes;
            this.visualsChanged = true;
            this.LayerType = layerType;
            this.InitializeRenderStates();
        }

        protected override bool RenderOnPreDraw()
        {
            return !this.chart.UsesAbsDimension;
        }

        protected override DrawMode GetDrawMode(bool preRender, bool hitTest)
        {
            DrawMode drawMode = DrawMode.None;
            if (!preRender)
                drawMode |= DrawMode.PositiveValues;
            if (preRender || this.chart.UsesAbsDimension || hitTest)
                drawMode |= DrawMode.NegativeValues;
            switch (this.chart.ChartType)
            {
                case LayerType.PointMarkerChart:
                    drawMode |= DrawMode.PointMarker;
                    break;
                case LayerType.ClusteredColumnChart:
                    drawMode |= DrawMode.ConstantMode;
                    break;
                case LayerType.PieChart:
                    drawMode |= DrawMode.PieChart;
                    break;
            }
            return drawMode;
        }

        private void InitializeTechniques()
        {
            if (this.renderingTechnique != null)
                return;
            this.renderingTechnique = new InstanceRenderingTechnique(this.SharedRenderParameters, (RenderParameters)null);
            this.shadowTechnique = new ShadowVolumeTechnique(this.SharedRenderParameters);
            this.outlineTechnique = new OutlineTechnique(this.SharedRenderParameters);
        }

        private void InitializeRenderStates()
        {
            this.depthStencilState = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                DepthEnable = true,
                DepthFunction = ComparisonFunction.Less,
                DepthWriteEnable = true
            });
            this.ceilingDepthStencilState = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                DepthEnable = true,
                DepthFunction = ComparisonFunction.Less,
                DepthWriteEnable = true,
                StencilEnable = true,
                StencilFrontFace = new StencilDescription()
                {
                    Function = ComparisonFunction.Equal,
                    PassOperation = StencilOperation.IncrementSaturate,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep
                },
                StencilReferenceValue = 0
            });
            this.wallDepthStencilState = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                DepthEnable = true,
                DepthFunction = ComparisonFunction.Less,
                DepthWriteEnable = true,
                StencilEnable = true,
                StencilFrontFace = new StencilDescription()
                {
                    Function = ComparisonFunction.Equal,
                    PassOperation = StencilOperation.Keep,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep
                },
                StencilReferenceValue = 0
            });
            this.blendState = BlendState.Create(new BlendStateDescription()
            {
                BlendEnable = false
            });
            this.transparencyBlendState = BlendState.Create(new BlendStateDescription()
            {
                BlendEnable = true,
                BlendOp = BlendOperation.Add,
                SourceBlend = BlendFactor.SourceAlpha,
                DestBlend = BlendFactor.InvSourceAlpha,
                SourceBlendAlpha = BlendFactor.SourceAlpha,
                DestBlendAlpha = BlendFactor.One
            });
            this.backCullRasterizerState = RasterizerState.Create(new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                MultisampleEnable = true
            });
            this.frontCullRasterizerState = RasterizerState.Create(new RasterizerStateDescription()
            {
                CullMode = CullMode.Front,
                MultisampleEnable = true
            });
            this.backCullRasterizerStateWireframe = RasterizerState.Create(new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Wireframe
            });
            this.frontCullRasterizerStateWireframe = RasterizerState.Create(new RasterizerStateDescription()
            {
                CullMode = CullMode.Front,
                FillMode = FillMode.Wireframe
            });
        }

        private void UpdateChartType()
        {
            switch (this.LayerType)
            {
                case LayerType.PointMarkerChart:
                    this.Alignment = InstancedVisual.Alignment.None;
                    this.chart = (Chart)new PointMarkerChart();
                    break;
                case LayerType.BubbleChart:
                    this.Alignment = InstancedVisual.Alignment.None;
                    this.chart = (Chart)new BubbleChart();
                    break;
                case LayerType.ColumnChart:
                    this.Alignment = InstancedVisual.Alignment.None;
                    this.chart = (Chart)new ColumnChart();
                    break;
                case LayerType.ClusteredColumnChart:
                    this.Alignment = InstancedVisual.Alignment.Horizontal;
                    this.chart = (Chart)new ClusteredColumnChart();
                    break;
                case LayerType.StackedColumnChart:
                    this.Alignment = InstancedVisual.Alignment.Vertical;
                    this.chart = (Chart)new StackedColumnsChart();
                    break;
                case LayerType.PieChart:
                    this.Alignment = InstancedVisual.Alignment.Angular;
                    this.chart = (Chart)new PieChart();
                    break;
                default:
                    throw new NotSupportedException("Layer Type");
            }
            if (this.visuals != null)
            {
                for (int index = 0; index < this.visuals.Length; ++index)
                {
                    if (this.visuals[index] != null)
                        this.visuals[index].Dispose();
                }
            }
            int num = this.shapes == null || this.LayerType == LayerType.PieChart ? 0 : this.shapes.Length;
            this.visuals = new InstancedVisual[num + this.chart.PrivateVisualsCount];
            for (short i = (short) 0; (int) i < num; ++i)
            {
                this.visuals[(int) i] = InstancedVisual.Create(this.shapes[(int) i]);
            }
            this.chart.AddPrivateVisuals(this);
            this.visualsChanged = true;
            this.Scale = 1f;
            this.instanceCountUsedForScale = 0;
        }

        private void SetVisualMeshes()
        {
            if (this.instanceVisual != null && !this.instanceVisual.Disposed)
            {
                this.instanceVisual.Dispose();
                this.instanceVisual = (InstancedVisual)null;
            }
            if (this.nullVisual != null && !this.nullVisual.Disposed)
            {
                this.nullVisual.Dispose();
                this.nullVisual = (InstancedVisual)null;
            }
            if (this.zeroVisual != null && !this.zeroVisual.Disposed)
            {
                this.zeroVisual.Dispose();
                this.zeroVisual = (InstancedVisual)null;
            }
            this.instanceVisual = this.visuals[0];
            if (this.visuals.Length > 1)
            {
                this.nullVisual = this.visuals[1];
                this.zeroVisual = this.visuals[2];
            }
            this.visualType.Clear();
            this.visualType.Add(InstanceBlockQueryType.PositiveInstances, this.instanceVisual);
            this.visualType.Add(InstanceBlockQueryType.ZeroInstances, this.zeroVisual);
            this.visualType.Add(InstanceBlockQueryType.NullInstances, this.nullVisual);
            this.visualType.Add(InstanceBlockQueryType.NegativeInstances, this.instanceVisual);
        }

        internal void AddVisual(short where, InstancedVisual visual)
        {
            if (this.visuals[(int)where] != null && !this.visuals[(int)where].Disposed)
                this.visuals[(int)where].Dispose();
            this.visuals[(int)where] = visual;
        }

        public InstancedShape[] GetShapes()
        {
            return this.shapes;
        }

        public void SetShapes(InstancedShape[] newShapes)
        {
            if (newShapes == null || newShapes.Length < 1)
                throw new ArgumentNullException("newShapes");
            if (this.EngineDispatcher != null)
            {
                this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod)(() =>
                {
                    this.shapes = newShapes;
                    this.UpdateChartType();
                    this.IsDirty = true;
                }));
            }
            else
            {
                this.shapes = newShapes;
                this.UpdateChartType();
                this.IsDirty = true;
            }
        }

        protected override void OnUpdate(SceneState state)
        {
            if (this.visualsChanged)
            {
                this.FixedDimension = this.chart.FixedDimension;
                this.ShadowScale = this.chart.ShadowScale;
                this.HeightOffset = this.chart.Altitude;
                this.UseTextureForNegativeValues = this.chart.UsesAbsDimension;
                this.UseSqrtValue = this.chart.IsBubble;
                this.SetVisualMeshes();
                this.visualsChanged = false;
            }
            if (this.instanceCountUsedForScale < this.instanceList.Count)
            {
                this.Scale = (float)Math.Min((double)this.Scale, this.chart.ComputeScale(this.instanceList, this.visualTimeRange != 0.0, this.instanceCountUsedForScale));
                this.instanceCountUsedForScale = this.instanceList.Count;
                this.scaleUpdateNeeded = true;
            }
            this.VariableScale = this.chart.GetVariableScale((double)this.Scale);
            this.FixedScale = this.chart.FixedScale;
            this.ViewScaleEnabled = true;
        }

        protected override void OverrideDimensionAndScales(InstanceBlockQueryType queryType, ref float dimension, ref Vector2F fixedScale, ref Vector2F variableScale)
        {
            switch (queryType)
            {
                case InstanceBlockQueryType.PositiveInstances:
                case InstanceBlockQueryType.NegativeInstances:
                    this.instanceVisual.OverrideDimensionAndScales(ref dimension, ref fixedScale, ref variableScale);
                    break;
                case InstanceBlockQueryType.ZeroInstances:
                    this.zeroVisual.OverrideDimensionAndScales(ref dimension, ref fixedScale, ref variableScale);
                    break;
                case InstanceBlockQueryType.NullInstances:
                    this.nullVisual.OverrideDimensionAndScales(ref dimension, ref fixedScale, ref variableScale);
                    break;
            }
        }

        internal override RenderQuery GetSpatialQuery(SceneState state, List<int> queryResult)
        {
            float scale = (float)this.ViewScale * this.FixedDimensionScale;
            return RenderQuery.GetQuery(1.0 - state.FlatteningFactor, state.ViewProjection, scale, (InstanceLayer)this, queryResult);
        }

        internal override double GetMaxInstanceExtent(float scale, int maxCount)
        {
            return (double)this.chart.GetMaxExtent(scale, maxCount);
        }

        protected override bool DrawColor(Renderer renderer, SceneState state, DrawParameters parameters)
        {
            if (renderer == null || state == null || parameters == null)
                return false;
            this.InitializeTechniques();
            this.renderingTechnique.SetRenderStates(parameters.DepthStencil, parameters.Blend, parameters.Rasterizer);
            bool flag1 = parameters.QueryType == InstanceBlockQueryType.NegativeInstances && !this.chart.UsesAbsDimension;
            InstancedVisual instancedVisual = this.visualType[parameters.QueryType];
            this.renderingTechnique.DesaturateFactor = this.DesaturateFactor;
            int count;
            this.renderingTechnique.ColorParameters = this.ColorManager.GetColors(out count);
            this.renderingTechnique.RenderDepthWidthInfo = state.GlowEnabled && (double)this.Opacity >= 1.0 && (parameters.QueryType != InstanceBlockQueryType.NegativeInstances || this.chart.UsesAbsDimension);
            bool flag2 = this.chart.ChartType == LayerType.PieChart;
            this.renderingTechnique.FrameId = parameters.FrameId;
            this.renderingTechnique.PieTechnique = parameters.QueryType != InstanceBlockQueryType.NullInstances && parameters.QueryType != InstanceBlockQueryType.ZeroInstances && flag2;
            renderer.RenderLockEnter();
            try
            {
                if (parameters.RenderingOptions != null && parameters.RenderingOptions.Wireframe)
                    renderer.SetRasterizerStateNoLock(flag1 ? this.frontCullRasterizerStateWireframe : this.backCullRasterizerStateWireframe);
                else
                    renderer.SetRasterizerStateNoLock(flag1 ? this.frontCullRasterizerState : this.backCullRasterizerState);
                DepthStencilState state1 = this.chart.IsBubble ? this.ceilingDepthStencilState : this.depthStencilState;
                renderer.SetDepthStencilStateNoLock(state1);
                renderer.SetBlendStateNoLock((double)this.Opacity < 1.0 ? this.transparencyBlendState : this.blendState);
                this.renderingTechnique.UseRenderPriority = true;
                this.renderingTechnique.Mode = parameters.DrawStyle == DrawStyle.Color ? InstanceRenderingTechnique.RenderMode.Color : InstanceRenderingTechnique.RenderMode.HitTest;
                renderer.SetEffect((EffectTechnique)this.renderingTechnique);
                VertexBuffer vertexBuffer1 = parameters.SourceStreamBuffer.PeekVertexBuffer();
                if (vertexBuffer1 != null)
                {
                    VertexBuffer meshVertexBuffer = instancedVisual.MeshVertexBuffer;
                    IndexBuffer meshIndexBuffer = instancedVisual.MeshIndexBuffer;
                    PrimitiveTopology topology = PrimitiveTopology.TriangleList;
                    VertexBuffer vertexBuffer2 = (VertexBuffer)null;
                    if (parameters.DrawStyle == DrawStyle.HitTest)
                    {
                        vertexBuffer2 = parameters.SourceIdStreamBuffer.PeekVertexBuffer();
                        renderer.SetVertexSourceNoLock(new VertexBuffer[3]
            {
              meshVertexBuffer,
              vertexBuffer1,
              vertexBuffer2
            });
                    }
                    else
                        renderer.SetVertexSourceNoLock(new VertexBuffer[2]
            {
              meshVertexBuffer,
              vertexBuffer1
            });
                    renderer.SetIndexSourceNoLock(meshIndexBuffer);
                    if (this.chart.IsBubble)
                    {
                        this.renderingTechnique.UseRenderPriority = false;
                        renderer.DrawIndexedInstancedNoLock(instancedVisual.FirstCeilingIndex, instancedVisual.CeilingIndexCount, 0, parameters.InstanceCount, topology);
                        renderer.SetDepthStencilStateNoLock(this.wallDepthStencilState);
                        renderer.DrawIndexedInstancedNoLock(instancedVisual.FirstWallIndex, instancedVisual.WallIndexCount, 0, parameters.InstanceCount, topology);
                    }
                    else if (!flag1 && (int)parameters.RenderPriority > 1)
                    {
                        this.renderingTechnique.UseRenderPriority = true;
                        renderer.DrawIndexedInstancedNoLock(instancedVisual.FirstWallIndex, instancedVisual.WallIndexCount, 0, parameters.InstanceCount, topology);
                        this.renderingTechnique.UseRenderPriority = false;
                        renderer.DrawIndexedInstancedNoLock(instancedVisual.FirstCeilingIndex, instancedVisual.CeilingIndexCount, 0, parameters.InstanceCount, topology);
                        if (state.GraphicsLevel == GraphicsLevel.Quality)
                        {
                            this.renderingTechnique.UseRenderPriority = true;
                            this.renderingTechnique.OverrideRenderPriority = 0;
                            renderer.SetDepthStencilStateNoLock(this.ceilingDepthStencilState);
                            renderer.DrawIndexedInstancedNoLock(instancedVisual.FirstCeilingIndex, instancedVisual.CeilingIndexCount, 0, parameters.InstanceCount, topology);
                        }
                        this.renderingTechnique.OverrideRenderPriority = -1;
                    }
                    else
                        renderer.DrawIndexedInstancedNoLock(0, meshIndexBuffer.IndexCount, 0, parameters.InstanceCount, topology);
                    if (vertexBuffer2 != null)
                        vertexBuffer2.Dispose();
                    renderer.SetVertexSourceNoLock(new VertexBuffer[4]);
                    renderer.SetTextureNoLock(0, (Texture)null);
                    vertexBuffer1.Dispose();
                }
            }
            finally
            {
                renderer.RenderLockExit();
            }
            return true;
        }

        protected override bool DrawSelection(Renderer renderer, SceneState state, DrawParameters parameters)
        {
            if (renderer == null || state == null || parameters == null)
                return false;
            this.renderingTechnique.SetRenderStates(parameters.DepthStencil, parameters.Blend, parameters.Rasterizer);
            bool flag1 = parameters.QueryType != InstanceBlockQueryType.ZeroInstances && this.chart.ChartType == LayerType.PieChart;
            bool flag2 = parameters.QueryType == InstanceBlockQueryType.NegativeInstances && !this.chart.UsesAbsDimension;
            renderer.SetRasterizerState(flag2 ? this.frontCullRasterizerState : this.backCullRasterizerState);
            DepthStencilState state1 = this.chart.IsBubble ? this.ceilingDepthStencilState : this.depthStencilState;
            renderer.SetDepthStencilState(state1);
            renderer.SetBlendState(this.blendState);
            this.renderingTechnique.UseRenderPriority = true;
            bool flag3 = false;
            if (this.DesaturationEnabled)
            {
                this.renderingTechnique.PieTechnique = flag1;
                this.renderingTechnique.FrameId = parameters.FrameId;
                this.renderingTechnique.Mode = InstanceRenderingTechnique.RenderMode.Color;
                this.renderingTechnique.DesaturateFactor = 0.0f;
                renderer.SetEffect((EffectTechnique)this.renderingTechnique);
            }
            else
            {
                flag3 = true;
                this.outlineTechnique.PieTechnique = flag1;
                this.outlineTechnique.FrameId = parameters.FrameId;
                renderer.SetEffect((EffectTechnique)this.outlineTechnique);
                renderer.SetTexture(0, state.GlobeDepth);
                if (parameters.RenderingOptions != null)
                {
                    this.outlineTechnique.OutlineWidth = parameters.RenderingOptions.OutlineWidth;
                    this.outlineTechnique.OutlineOffset = parameters.RenderingOptions.OutlineOffset;
                    this.outlineTechnique.OutlineColor = parameters.RenderingOptions.InnerOutlineColor;
                    this.outlineTechnique.OutlineSecondaryColor = parameters.RenderingOptions.OuterOutlineColor;
                }
                this.outlineTechnique.DepthEnabled = parameters.DrawStyle != DrawStyle.NegativeSelection;
            }
            VertexBuffer vertexBuffer1 = parameters.SourceStreamBuffer.PeekVertexBuffer();
            if (vertexBuffer1 != null)
            {
                InstancedVisual instancedVisual = this.visualType[parameters.QueryType];
                VertexBuffer vertexBuffer2 = flag3 ? instancedVisual.OutlineMeshVertexBuffer : instancedVisual.MeshVertexBuffer;
                IndexBuffer indexBuffer = flag3 ? instancedVisual.OutlineMeshIndexBuffer : instancedVisual.MeshIndexBuffer;
                PrimitiveTopology topology = flag3 ? PrimitiveTopology.TriangleListWithAdjacency : PrimitiveTopology.TriangleList;
                renderer.SetVertexSource(new VertexBuffer[2]
        {
          vertexBuffer2,
          vertexBuffer1
        });
                renderer.SetIndexSource(indexBuffer);
                renderer.DrawIndexedInstanced(0, indexBuffer.IndexCount, 0, parameters.InstanceCount, topology);
                renderer.SetVertexSource(new VertexBuffer[4]);
                renderer.SetTexture(0, (Texture)null);
                vertexBuffer1.Dispose();
            }
            return true;
        }

        protected override bool DrawShadowVolume(Renderer renderer, SceneState state, DrawParameters parameters)
        {
            if (renderer == null || state == null || (parameters == null || this.chart.ChartType != LayerType.PointMarkerChart))
                return false;
            this.shadowTechnique.SetRenderStates(parameters.DepthStencil, parameters.Blend, parameters.Rasterizer);
            this.shadowTechnique.FrameId = parameters.FrameId;
            this.shadowTechnique.ShadowScale = this.ShadowScale;
            bool flag = this.chart.ChartType == LayerType.PieChart;
            this.shadowTechnique.PieTechnique = parameters.QueryType != InstanceBlockQueryType.ZeroInstances && flag;
            renderer.SetEffect((EffectTechnique)this.shadowTechnique);
            VertexBuffer vertexBuffer = parameters.SourceStreamBuffer.PeekVertexBuffer();
            if (vertexBuffer == null)
                return false;
            InstancedVisual instancedVisual = this.visualType[parameters.QueryType];
            VertexBuffer meshVertexBuffer = instancedVisual.ShadowMeshVertexBuffer;
            IndexBuffer shadowMeshIndexBuffer = instancedVisual.ShadowMeshIndexBuffer;
            PrimitiveTopology topology = PrimitiveTopology.TriangleList;
            renderer.SetVertexSource(new VertexBuffer[2]
      {
        meshVertexBuffer,
        vertexBuffer
      });
            renderer.SetIndexSource(shadowMeshIndexBuffer);
            renderer.DrawIndexedInstanced(0, shadowMeshIndexBuffer.IndexCount, 0, parameters.InstanceCount, topology);
            renderer.SetVertexSource(new VertexBuffer[4]);
            renderer.SetTexture(0, (Texture)null);
            vertexBuffer.Dispose();
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod)(() =>
            {
                for (int index = 0; index < this.visuals.Length; ++index)
                {
                    if (this.visuals[index] != null && !this.visuals[index].Disposed)
                        this.visuals[index].Dispose();
                }
                DisposableResource[] disposableResourceArray = new DisposableResource[15]
        {
          (DisposableResource) this.renderingTechnique,
          (DisposableResource) this.shadowTechnique,
          (DisposableResource) this.outlineTechnique,
          (DisposableResource) this.instanceVisual,
          (DisposableResource) this.zeroVisual,
          (DisposableResource) this.nullVisual,
          (DisposableResource) this.backCullRasterizerState,
          (DisposableResource) this.frontCullRasterizerState,
          (DisposableResource) this.depthStencilState,
          (DisposableResource) this.ceilingDepthStencilState,
          (DisposableResource) this.frontCullRasterizerStateWireframe,
          (DisposableResource) this.backCullRasterizerStateWireframe,
          (DisposableResource) this.wallDepthStencilState,
          (DisposableResource) this.blendState,
          (DisposableResource) this.transparencyBlendState
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
