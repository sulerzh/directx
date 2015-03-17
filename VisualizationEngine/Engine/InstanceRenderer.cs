using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    internal class InstanceRenderer : DisposableResource
    {
        private List<InstanceBlock> instanceBlocks = new List<InstanceBlock>();
        private bool updateIndex = true;
        private object instanceBlocksLock = new object();
        private List<int> visibleBlocks = new List<int>();
        private Dictionary<uint, Tuple<uint, uint>> idToBufferPositionMap = new Dictionary<uint, Tuple<uint, uint>>();
        private InstanceStreams instanceStreams = new InstanceStreams();
        private long lastUpdateTick = -1L;
        private long lastGatherAccumulateFrame = -1L;
        private long lastHitTestGatherAccumulateFrame = -1L;
        private RenderParameterMatrix4x4F instanceViewParameter = new RenderParameterMatrix4x4F("InstanceView");
        private RenderParameterMatrix4x4F instanceViewProjParameter = new RenderParameterMatrix4x4F("InstanceViewProj");
        private RenderParameterVector4F instanceBasePositionParameter = new RenderParameterVector4F("InstanceBasePosition");
        private RenderParameterFloat heightOffsetParameter = new RenderParameterFloat("HeightOffset");
        private RenderParameterFloat horizontalSpacingParameter = new RenderParameterFloat("HorizontalSpacing");
        private RenderParameterFloat viewScaleParameter = new RenderParameterFloat("ViewScale");
        private RenderParameterFloat viewScaleUnboundParameter = new RenderParameterFloat("ViewScaleUnbound");
        private RenderParameterVector2F variableScaleParameter = new RenderParameterVector2F("VariableScale");
        private RenderParameterVector2F fixedScaleParameter = new RenderParameterVector2F("FixedScale");
        private RenderParameterFloat fixedDimensionParameter = new RenderParameterFloat("FixedDimension");
        private RenderParameterFloat fadeTimeParameter = new RenderParameterFloat("FadeTime");
        private RenderParameterFloat visualTimeParameter = new RenderParameterFloat("VisualTime");
        private RenderParameterFloat visualTimeScaleParameter = new RenderParameterFloat("VisualTimeScale");
        private RenderParameterFloat visualTimeFreezeParameter = new RenderParameterFloat("VisualTimeFreeze");
        private RenderParameterBool visualTimeFreezeEnableParameter = new RenderParameterBool("VIsualTimeFreezeEnable");
        private RenderParameterBool applyTextureParameter = new RenderParameterBool("ApplyTexture");
        private RenderParameterFloat zValuesSignParameter = new RenderParameterFloat("ZValuesSign");
        private RenderParameterInt alignmentParameter = new RenderParameterInt("Alignment");
        private RenderParameterFloat maxValueParameter = new RenderParameterFloat("MaxValue");
        private RenderParameterFloat annotationAnchorHeightParameter = new RenderParameterFloat("AnnotationAnchorHeight");
        private RenderParameterFloat opacityParameter = new RenderParameterFloat("Opacity");
        private RenderParameterMatrix4x4F[] instanceViewProjDepthOffsetParameter = new RenderParameterMatrix4x4F[5];
        private const float DesaturationTimeDuration = 0.3f;
        private const int MaxVisibleBlocks = 1000;
        private InstanceLayer visualLayer;
        private InstanceId? previousFrameHoveredId;
        private int previousFrameHoveredIdCount;
        private SpatialIndex spatialIndex;
        private bool hasData;
        private GatherAccumulateProcessor gatherAccumulate;
        private bool blockUntilAnnotationTexturesAreAvailable;
        private LayerScaling scaling;
        private LayerTimeScaling timeScaling;
        private DateTime minVisualTime;
        private DateTime maxVisualTime;
        private long desaturationChangedTick;
        private int hitTestLayerId;
        private int frameCount;
        private bool lastUpdateIsDirty;
        private bool showNegatives;
        private bool showZeros;
        private bool showNulls;
        private RenderParameters sharedParameters;
        private InstanceProcessingTechnique processingTechnique;
        private AnnotationTechnique annotationTechnique;
        public DrawBufferDelegate DrawBufferColor;
        public DrawBufferDelegate DrawBufferShadow;
        public DrawBufferDelegate DrawBufferSelection;
        public OverrideDimensionAndScalesDelegate OverrideDimensionAndScales;

        public AnnotationCache AnnotationTextureCache { get; set; }

        public float HeightOffset { get; set; }

        public float ValueOffset { get; set; }

        public float ShadowScale { get; set; }

        public float FixedDimension { get; set; }

        public Vector2F VariableScale { get; set; }

        public Vector2F FixedScale { get; set; }

        public bool ViewScaleEnabled { get; set; }

        public float MaxValue { get; set; }

        public float MaxPositiveValue { get; set; }

        public InstancedVisual.Alignment Alignment { get; set; }

        public float VariableScaleFactor { get; set; }

        public float FixedScaleFactor { get; set; }

        public float DesaturateFactor { get; set; }

        public bool DesaturationEnabled { get; set; }

        public bool UseTextureForNegativeValues { get; set; }

        public bool UseSqrtValue { get; set; }

        public bool ShowOnlyMaxValueEnabled { get; set; }

        public bool UseLogScale { get; set; }

        private float HorizontalSpacing
        {
            get
            {
                return this.visualLayer.HorizontalSpacing;
            }
        }

        public LayerRenderingParameters RenderingOptions { get; set; }

        public LayerColorManager ColorManager { get; set; }

        public float Opacity { get; set; }

        public bool IsOptimized
        {
            get
            {
                return this.spatialIndex.Optimized;
            }
        }

        internal List<InstanceBlock> InstanceBlocks
        {
            get
            {
                return this.instanceBlocks;
            }
        }

        public AnnotationStyle AnnotationStyle
        {
            get
            {
                return this.annotationTechnique.Style;
            }
            set
            {
                this.annotationTechnique.Style = value;
            }
        }

        public RenderParameters SharedRenderParameters
        {
            get
            {
                return this.sharedParameters;
            }
        }

        public InstanceRenderer(InstanceLayer layer, int hitTestId, LayerScaling layerScaling, LayerTimeScaling layerTimeScaling)
        {
            for (int index = 0; index < this.instanceViewProjDepthOffsetParameter.Length; ++index)
                this.instanceViewProjDepthOffsetParameter[index] = new RenderParameterMatrix4x4F("InstanceViewProjDepthOffset");
            this.hitTestLayerId = hitTestId;
            this.visualLayer = layer;
            this.InitializeTechniques();
            this.scaling = layerScaling;
            this.timeScaling = layerTimeScaling;
            this.spatialIndex = new SpatialIndex(layer, this.gatherAccumulate, this.processingTechnique, (uint)this.hitTestLayerId);
        }

        public void SetHitTestId(int id)
        {
            this.hitTestLayerId = id;
        }

        public void SetDesaturationMode(bool enabled)
        {
            if (this.DesaturationEnabled == enabled)
                return;
            this.DesaturationEnabled = enabled;
            this.desaturationChangedTick = this.lastUpdateTick;
        }

        public bool HasNegativeValues()
        {
            if (!this.UseTextureForNegativeValues)
            {
                lock (this.instanceBlocksLock)
                {
                    foreach (InstanceBlock block in this.instanceBlocks)
                    {
                        if (block.HasNegativeValues)
                            return true;
                    }
                }
            }
            return false;
        }

        public void SetInstanceData(bool resetSpatialIndex, List<InstanceData> instances, Clusters dataClusters, Dictionary<int, int> colorOverrides, bool optimize, bool planar, DateTime? minTime, DateTime? maxTime)
        {
            this.minVisualTime = minTime.HasValue ? minTime.Value : DateTime.MaxValue;
            this.maxVisualTime = maxTime.HasValue ? maxTime.Value : DateTime.MinValue;
            double totalMilliseconds = (this.maxVisualTime - this.minVisualTime).TotalMilliseconds;
            List<InstanceBlock> blocks = this.instanceBlocks;
            List<InstanceBlock> list = this.spatialIndex.Finalize(resetSpatialIndex, dataClusters, instances, colorOverrides, blocks, optimize, this.showNegatives, this.ShowOnlyMaxValueEnabled, planar, totalMilliseconds, minTime);
            lock (this.instanceBlocksLock)
                this.instanceBlocks = list;
            for (int index = 0; index < blocks.Count; ++index)
                blocks[index].Dispose();
            blocks.Clear();
            this.hasData = instances.Count > 0;
            this.UpdateIdMap();
        }

        public void SetSelectedItems(IEnumerable<InstanceId> selectedIds)
        {
            lock (this.instanceBlocksLock)
            {
                foreach (InstanceBlock block in this.instanceBlocks)
                    block.ClearFilterInstances();
                foreach (InstanceId id in selectedIds)
                {
                    Tuple<uint, uint> bufferPosition = this.GetInstanceBufferPosition(id);
                    if (bufferPosition != null)
                        this.instanceBlocks[(int)bufferPosition.Item1].AddFilterInstance((int)bufferPosition.Item2);
                }
            }
        }

        public void SetAnnotations(IEnumerable<InstanceId> annotationIds)
        {
            lock (this.instanceBlocksLock)
            {
                foreach (InstanceBlock block in this.instanceBlocks)
                    block.ClearAnnotationInstances();
                foreach (InstanceId annoId in annotationIds)
                {
                    Tuple<uint, uint> bufferPosition = this.GetInstanceBufferPosition(annoId);
                    if (bufferPosition != null)
                        this.instanceBlocks[(int)bufferPosition.Item1].AddAnnotationInstance((int)bufferPosition.Item2);
                }
            }
        }

        public void ClearInstanceData()
        {
            lock (this.instanceBlocksLock)
            {
                foreach (InstanceBlock block in this.instanceBlocks)
                    block.Clear();
            }
            this.hasData = false;
        }

        public Tuple<uint, uint> GetInstanceBufferPosition(InstanceId instance)
        {
            if (this.idToBufferPositionMap.ContainsKey(instance.ElementId))
                return this.idToBufferPositionMap[instance.ElementId];
            return null;
        }

        public Vector3F GetInstancePosition(InstanceId instance)
        {
            Tuple<uint, uint> instanceBufferPosition = this.GetInstanceBufferPosition(instance);
            if (instanceBufferPosition == null)
                return Vector3F.Empty;
            lock (this.instanceBlocksLock)
                return this.instanceBlocks[(int)instanceBufferPosition.Item1].GetInstancePositionAt((int)instanceBufferPosition.Item2);
        }

        private void UpdateIdMap()
        {
            this.idToBufferPositionMap.Clear();
            lock (this.instanceBlocksLock)
            {
                for (int i = 0; i < this.instanceBlocks.Count; ++i)
                {
                    for (int j = 0; j < this.instanceBlocks[i].Count; ++j)
                    {
                        uint elementId = this.instanceBlocks[i].GetInstanceIdAt(j).ElementId;
                        if (!this.idToBufferPositionMap.ContainsKey(elementId))
                            this.idToBufferPositionMap.Add(elementId, new Tuple<uint, uint>((uint)i, (uint)j));
                    }
                }
            }
        }

        private bool Update(SceneState state)
        {
            if (state.ElapsedTicks == this.lastUpdateTick)
                return this.lastUpdateIsDirty;
            this.lastUpdateTick = state.ElapsedTicks;
            ++this.frameCount;
            bool flag = false;
            if (this.scaling.EaseInScale < 1.0)
                flag = true;
            this.scaling.Update(state);
            float viewScale = this.scaling.ViewScale;
            float num = viewScale * this.FixedScaleFactor;
            this.heightOffsetParameter.Value = this.HeightOffset * num;
            this.horizontalSpacingParameter.Value = this.HorizontalSpacing * num;
            this.viewScaleParameter.Value = viewScale;
            this.viewScaleUnboundParameter.Value = this.scaling.ViewScaleUnbound;
            this.fadeTimeParameter.Value = this.ShowOnlyMaxValueEnabled ? 0.0f : 0.25f;
            this.alignmentParameter.Value = (int)this.Alignment;
            this.annotationAnchorHeightParameter.Value = this.visualLayer.AnnotationAnchorHeight;
            this.timeScaling.Update(this.minVisualTime, this.maxVisualTime, state);
            this.visualTimeParameter.Value = this.timeScaling.VisualTime;
            this.visualTimeScaleParameter.Value = this.timeScaling.VisualTimeScale;
            this.visualTimeFreezeEnableParameter.Value = this.timeScaling.VisualTimeFreezeEnabled;
            this.visualTimeFreezeParameter.Value = this.timeScaling.VisualTimeFreeze;
            if (this.updateIndex)
            {
                this.visibleBlocks.Clear();
                this.spatialIndex.Traverse(this.visualLayer.GetSpatialQuery(state, this.visibleBlocks));
            }
            this.lastUpdateIsDirty = flag;
            return flag;
        }

        private void InitializeTechniques()
        {
            this.sharedParameters = RenderParameters.Create(new IRenderParameter[26]
            {
                this.instanceViewParameter,
                this.instanceViewProjParameter,
                this.instanceViewProjDepthOffsetParameter[0],
                this.instanceViewProjDepthOffsetParameter[1],
                this.instanceViewProjDepthOffsetParameter[2],
                this.instanceViewProjDepthOffsetParameter[3],
                this.instanceViewProjDepthOffsetParameter[4],
                this.instanceBasePositionParameter,
                this.variableScaleParameter,
                this.fixedScaleParameter,
                this.fixedDimensionParameter,
                this.heightOffsetParameter,
                this.horizontalSpacingParameter,
                this.viewScaleParameter,
                this.viewScaleUnboundParameter,
                this.fadeTimeParameter,
                this.visualTimeParameter,
                this.visualTimeScaleParameter,
                this.visualTimeFreezeParameter,
                this.visualTimeFreezeEnableParameter,
                this.applyTextureParameter,
                this.zValuesSignParameter,
                this.alignmentParameter,
                this.maxValueParameter,
                this.annotationAnchorHeightParameter,
                this.opacityParameter
            });
            this.processingTechnique = new InstanceProcessingTechnique(this.sharedParameters);
            this.annotationTechnique = new AnnotationTechnique(this.sharedParameters);
            this.gatherAccumulate = new GatherAccumulateProcessor(this.sharedParameters);
        }

        public void UpdateDisplayOptions(bool nulls, bool zeros, bool negatives)
        {
            this.showNegatives = negatives;
            this.showNulls = nulls;
            this.showZeros = zeros;
            lock (this.instanceBlocksLock)
            {
                foreach (InstanceBlock block in this.instanceBlocks)
                    block.ShowNegatives = this.showNegatives;
            }
        }

        private bool Draw(Renderer renderer, SceneState state, DrawMode drawMode, DrawStyle drawStyle, DepthStencilState depthStencil, BlendState blend, RasterizerState rasterizer)
        {
            if (this.instanceBlocks.Count == 0 || !this.hasData)
                return false;
            bool flag1 = this.Update(state);
            this.UpdateGatherAccumulate(renderer, state, drawMode, drawStyle);
            for (int index = 0; index < this.instanceBlocks.Count; ++index)
                this.instanceBlocks[index].PlanarCoordinates = state.FlatteningFactor == 1.0;
            float baseFrameId = this.GetBaseFrameId(renderer);
            int count;
            this.ColorManager.GetColors(out count);
            this.processingTechnique.ColorCount = count;
            this.opacityParameter.Value = this.Opacity;
            bool flag2 = (drawMode & DrawMode.PointMarker) > DrawMode.None;
            this.processingTechnique.IgnoreValues = flag2;
            bool flag3 = (drawMode & DrawMode.PositiveValues) > DrawMode.None;
            bool flag4 = (drawMode & DrawMode.NegativeValues) > DrawMode.None;
            Dictionary<InstanceBlockQueryType, bool> dictionary = new Dictionary<InstanceBlockQueryType, bool>(4);
            dictionary.Add(InstanceBlockQueryType.PositiveInstances, flag3);
            dictionary.Add(InstanceBlockQueryType.ZeroInstances, flag3 && this.showZeros && !flag2);
            dictionary.Add(InstanceBlockQueryType.NullInstances, flag3 && this.showNulls && !flag2);
            dictionary.Add(InstanceBlockQueryType.NegativeInstances, flag4);
            foreach (InstanceBlockQueryType queryType in dictionary.Keys)
            {
                if (dictionary[queryType])
                {
                    this.zValuesSignParameter.Value = queryType != InstanceBlockQueryType.NegativeInstances || this.UseTextureForNegativeValues ? 1f : -1f;
                    this.applyTextureParameter.Value = queryType == InstanceBlockQueryType.NegativeInstances && this.UseTextureForNegativeValues;
                    for (int index = 0; index < this.visibleBlocks.Count; ++index)
                    {
                        InstanceBlock instanceBlock = this.instanceBlocks[this.visibleBlocks[index]];
                        if (queryType != InstanceBlockQueryType.NegativeInstances || instanceBlock.HasNegativeValues)
                        {
                            float valueOffset = queryType == InstanceBlockQueryType.NegativeInstances ? -this.ValueOffset : this.ValueOffset;
                            if (queryType == InstanceBlockQueryType.NullInstances && valueOffset > 0.0)
                                valueOffset = 0.0001f;
                            this.SetScale(queryType);
                            float num1 = index * (1.0f / 1000.0f);
                            float frameId = baseFrameId + num1;
                            StreamBuffer streamBuffer;
                            StreamBuffer idStreamBuffer;
                            int num2 = this.QueryInstances(renderer, state, drawStyle, drawMode, frameId, this.ShowOnlyMaxValueEnabled, this.UseLogScale, valueOffset, queryType, instanceBlock, out streamBuffer, out idStreamBuffer);
                            if (num2 != 0)
                            {
                                if (drawStyle == DrawStyle.Color)
                                {
                                    float num3 = Math.Min(1f, (float)((this.lastUpdateTick - this.desaturationChangedTick) / 60.0 / 0.300000011920929));
                                    this.DesaturateFactor = this.DesaturationEnabled ? num3 : 1f - num3;
                                    flag1 = ((flag1 ? 1 : 0) | ((double)num3 == 0.0 ? 0 : ((double)num3 != 1.0 ? 1 : 0))) != 0;
                                }
                                else
                                    this.DesaturateFactor = 0.0f;
                                if (queryType == InstanceBlockQueryType.NegativeInstances && !this.UseTextureForNegativeValues && drawStyle == DrawStyle.Selection)
                                    drawStyle = DrawStyle.NegativeSelection;
                                DrawParameters parameters = new DrawParameters()
                                {
                                    SourceStreamBuffer = streamBuffer,
                                    SourceIdStreamBuffer = idStreamBuffer,
                                    InstanceCount = num2,
                                    PositionProvider = instanceBlock,
                                    QueryType = queryType,
                                    RenderPriority = instanceBlock.MaxRenderPriority,
                                    DrawStyle = drawStyle,
                                    FrameId = frameId,
                                    DepthStencil = depthStencil,
                                    Blend = blend,
                                    Rasterizer = rasterizer,
                                    RenderingOptions = this.RenderingOptions
                                };
                                switch (drawStyle)
                                {
                                    case DrawStyle.Color:
                                    case DrawStyle.HitTest:
                                        int num4 = this.DrawBufferColor(renderer, state, parameters) ? 1 : 0;
                                        continue;
                                    case DrawStyle.Shadow:
                                        flag1 = flag1 | this.DrawBufferShadow(renderer, state, parameters);
                                        continue;
                                    case DrawStyle.Selection:
                                    case DrawStyle.NegativeSelection:
                                        int num5 = this.DrawBufferSelection(renderer, state, parameters) ? 1 : 0;
                                        continue;
                                    case DrawStyle.Annotation:
                                    case DrawStyle.AnnotationHitTest:
                                        flag1 = flag1 | this.DrawBufferAnnotation(renderer, state, parameters, instanceBlock);
                                        continue;
                                    default:
                                        continue;
                                }
                            }
                        }
                    }
                }
            }
            return flag1;
        }

        private static InstanceBlockQueryInstanceSource GetInstanceSource(DrawStyle drawStyle)
        {
            InstanceBlockQueryInstanceSource queryInstanceSource;
            switch (drawStyle)
            {
                case DrawStyle.Selection:
                case DrawStyle.NegativeSelection:
                    queryInstanceSource = InstanceBlockQueryInstanceSource.Filter;
                    break;
                case DrawStyle.Annotation:
                case DrawStyle.AnnotationHitTest:
                    queryInstanceSource = InstanceBlockQueryInstanceSource.Annotation;
                    break;
                default:
                    queryInstanceSource = InstanceBlockQueryInstanceSource.Block;
                    break;
            }
            return queryInstanceSource;
        }

        private float GetBaseFrameId(Renderer renderer)
        {
            float num = (this.frameCount - 1) % 16000 - 8000;
            if (num == -8000.0)
                this.instanceStreams.ClearStreams(renderer);
            return num;
        }

        private void UpdateGatherAccumulate(Renderer renderer, SceneState state, DrawMode drawMode, DrawStyle drawStyle)
        {
            bool flag1 = (drawMode & DrawMode.PieChart) > DrawMode.None;
            bool ignoreValues = (drawMode & DrawMode.PointMarker) > DrawMode.None;
            bool constantMode = (drawMode & DrawMode.ConstantMode) > DrawMode.None;
            if ((drawStyle == DrawStyle.HitTest || drawStyle == DrawStyle.AnnotationHitTest) && this.lastHitTestGatherAccumulateFrame < state.ElapsedFrames)
            {
                this.lastHitTestGatherAccumulateFrame = state.ElapsedFrames;
                this.UpdateHitTestGatherAccumulate(renderer, drawMode);
            }
            if (this.lastGatherAccumulateFrame >= state.ElapsedFrames)
                return;
            bool flag2 = false;
            for (int index = 0; index < this.visibleBlocks.Count; ++index)
            {
                if (this.instanceBlocks[this.visibleBlocks[index]].HasMultiInstanceData || this.UseLogScale || flag1)
                {
                    flag2 = true;
                    break;
                }
            }
            this.lastGatherAccumulateFrame = state.ElapsedFrames;
            if (!flag2)
                return;
            this.gatherAccumulate.BeginFrame(false, !this.showZeros, !this.showNulls, false, this.ShowOnlyMaxValueEnabled, this.showNegatives, false, this.UseLogScale);
            List<GatherAccumulateProcessBlock> list = new List<GatherAccumulateProcessBlock>();
            for (int index = 0; index < this.visibleBlocks.Count; ++index)
                list.Add(this.instanceBlocks[this.visibleBlocks[index]].GetGatherAccumulateBlock());
            this.gatherAccumulate.Process(list, renderer, ignoreValues, constantMode);
        }

        private int QueryInstances(Renderer renderer, SceneState state, DrawStyle drawStyle, DrawMode drawMode, float frameId, bool showOnlyMaxValues, bool useLogScale, float valueOffset, InstanceBlockQueryType queryType, InstanceBlock instanceBlock, out StreamBuffer streamBuffer, out StreamBuffer idStreamBuffer)
        {
            InstanceBlockQueryInstanceSource instanceSource = InstanceRenderer.GetInstanceSource(drawStyle);
            bool flag1 = (drawMode & DrawMode.PieChart) > DrawMode.None;
            bool flag2 = (drawMode & DrawMode.PointMarker) > DrawMode.None;
            this.processingTechnique.FrameId = frameId;
            this.processingTechnique.UseSqrtValue = queryType != InstanceBlockQueryType.ZeroInstances && this.UseSqrtValue;
            this.processingTechnique.Mode = flag1 ? InstanceProcessingTechniqueMode.Pie : InstanceProcessingTechniqueMode.Default;
            this.instanceStreams.EnsureStreamBuffers(this.timeScaling.VisualTimeScale, instanceBlock, showOnlyMaxValues, drawMode);
            streamBuffer = this.instanceStreams.GetStream(queryType, drawStyle, false);
            this.UpdateTransforms(state, instanceBlock);
            int num = instanceBlock.QueryInstances(renderer, state, new InstanceBlockQueryParameters()
            {
                QueryType = queryType,
                HitTest = false,
                Offset = flag1 ? 0.0f : valueOffset,
                InstanceOutputBuffer = streamBuffer,
                IsPieChart = flag1,
                IsClusterChart = this.Alignment == InstancedVisual.Alignment.Horizontal,
                IgnoreInstanceValues = flag2,
                ShowOnlyMaxValues = showOnlyMaxValues,
                UseLogScale = useLogScale,
                InstanceSource = instanceSource
            });
            idStreamBuffer = null;
            if (num > 0 && (drawStyle == DrawStyle.HitTest || drawStyle == DrawStyle.AnnotationHitTest))
            {
                idStreamBuffer = this.instanceStreams.GetStream(queryType, drawStyle, true);
                instanceBlock.QueryInstances(renderer, state, new InstanceBlockQueryParameters()
                {
                    QueryType = queryType,
                    HitTest = true,
                    Offset = flag1 ? 0.0f : valueOffset,
                    InstanceOutputBuffer = idStreamBuffer,
                    IsPieChart = flag1,
                    IsClusterChart = this.Alignment == InstancedVisual.Alignment.Horizontal,
                    IgnoreInstanceValues = flag2,
                    ShowOnlyMaxValues = showOnlyMaxValues,
                    UseLogScale = useLogScale,
                    InstanceSource = instanceSource
                });
            }
            return num;
        }

        private void UpdateHitTestGatherAccumulate(Renderer renderer, DrawMode drawMode)
        {
            this.gatherAccumulate.BeginFrame(true, !this.showZeros, !this.showNulls, false, this.ShowOnlyMaxValueEnabled, this.showNegatives, false, this.UseLogScale);
            List<GatherAccumulateProcessBlock> list = new List<GatherAccumulateProcessBlock>();
            for (int index = 0; index < this.visibleBlocks.Count; ++index)
                list.Add(this.instanceBlocks[this.visibleBlocks[index]].GetGatherAccumulateBlock());
            this.gatherAccumulate.ProcessHitTest(list, renderer, (drawMode & DrawMode.PointMarker) > DrawMode.None, (drawMode & DrawMode.ConstantMode) > DrawMode.None);
        }

        private void SetScale(InstanceBlockQueryType queryType)
        {
            float fixedDimension = this.FixedDimension;
            Vector2F fixedScale = this.FixedScale;
            Vector2F variableScale = this.VariableScale;
            bool flag = variableScale.X != 0.0;
            this.OverrideDimensionAndScales(queryType, ref fixedDimension, ref fixedScale, ref variableScale);
            fixedScale = new Vector2F(fixedScale.X * (flag ? this.VariableScaleFactor : this.FixedScaleFactor), fixedScale.Y * this.FixedScaleFactor);
            Vector2F vector2F = variableScale * this.VariableScaleFactor;
            if (!this.ShowOnlyMaxValueEnabled)
            {
                float num = this.showNegatives ? 1f : this.MaxValue / this.MaxPositiveValue;
                if (this.UseSqrtValue)
                    num = (float)Math.Sqrt(num);
                vector2F *= num;
            }
            this.fixedDimensionParameter.Value = fixedDimension;
            if (this.ViewScaleEnabled)
            {
                fixedScale *= this.scaling.ViewScale;
                vector2F *= this.scaling.ViewScale;
            }
            this.fixedScaleParameter.Value = fixedScale;
            this.variableScaleParameter.Value = vector2F;
            this.maxValueParameter.Value = 3f + this.MaxValue;
        }

        private bool DrawBufferAnnotation(Renderer renderer, SceneState state, DrawParameters parameters, InstanceBlock block)
        {
            this.annotationTechnique.FrameId = parameters.FrameId;
            this.annotationTechnique.HitTestEnabled = parameters.DrawStyle == DrawStyle.AnnotationHitTest;
            this.annotationTechnique.TextureFiltering = !state.VisualTimeFreeze.HasValue || state.CameraMoved;
            renderer.SetEffect(this.annotationTechnique);
            VertexBuffer vertexBuffer1 = parameters.SourceStreamBuffer.PeekVertexBuffer();
            bool flag = true;
            if (vertexBuffer1 != null)
            {
                VertexBuffer vertexBuffer2 = null;
                if (parameters.DrawStyle == DrawStyle.AnnotationHitTest)
                    vertexBuffer2 = parameters.SourceIdStreamBuffer.PeekVertexBuffer();
                if (parameters.DrawStyle == DrawStyle.Annotation || parameters.DrawStyle == DrawStyle.AnnotationHitTest)
                {
                    renderer.SetVertexSource(new VertexBuffer[2]
                    {
                        vertexBuffer1,
                        vertexBuffer2
                    });
                    for (int index = 0; index < parameters.InstanceCount; ++index)
                    {
                        InstanceId? annotationInstanceId = block.GetAnnotationInstanceId(index, state.VisualTimeFreeze ?? state.VisualTime, state);
                        if (annotationInstanceId.HasValue)
                        {
                            this.annotationTechnique.RenderOnTop = this.visualLayer.HoveredElement.HasValue && ((int)annotationInstanceId.Value.ElementId == (int)this.visualLayer.HoveredElement.Value.ElementId && this.previousFrameHoveredId.HasValue) && (int)annotationInstanceId.Value.ElementId == (int)this.previousFrameHoveredId.Value.ElementId && this.previousFrameHoveredIdCount >= 3;
                            if (this.visualLayer.HoveredElement.HasValue && this.previousFrameHoveredId.HasValue && (int)this.previousFrameHoveredId.Value.ElementId == (int)this.visualLayer.HoveredElement.Value.ElementId)
                                ++this.previousFrameHoveredIdCount;
                            else
                                this.previousFrameHoveredIdCount = 0;
                            this.previousFrameHoveredId = this.visualLayer.HoveredElement;
                            bool hadExactTexture = false;
                            Texture mostRecentTexture = this.AnnotationTextureCache.GetMostRecentTexture(annotationInstanceId.Value, renderer, this.blockUntilAnnotationTexturesAreAvailable, out hadExactTexture);
                            flag = flag & hadExactTexture;
                            renderer.SetTexture(0, mostRecentTexture);
                            this.annotationTechnique.TextureDimensions = mostRecentTexture == null ? Vector2F.Empty : new Vector2F(mostRecentTexture.Width, mostRecentTexture.Height);
                            renderer.Draw(index, 1, PrimitiveTopology.PointList);
                        }
                    }
                }
                if (vertexBuffer2 != null)
                    vertexBuffer2.Dispose();
                renderer.SetVertexSource(new VertexBuffer[4]);
                renderer.SetTexture(0, null);
                vertexBuffer1.Dispose();
            }
            return !flag;
        }

        private void UpdateTransforms(SceneState state, InstanceBlock block)
        {
            Matrix4x4D matrix4x4D1 = Matrix4x4D.Translation(block.BasePosition) * state.View;
            Matrix4x4D matrix4x4D2 = matrix4x4D1 * state.Projection;
            this.instanceBasePositionParameter.Value = (Vector4F)block.BasePosition;
            this.instanceViewParameter.Value = (Matrix4x4F)matrix4x4D1;
            this.instanceViewProjParameter.Value = (Matrix4x4F)matrix4x4D2;
            for (int index = 0; index < 5; ++index)
                this.instanceViewProjDepthOffsetParameter[index].Value = (Matrix4x4F)(matrix4x4D1 * state.DepthOffsetProjection[index]);
        }

        public bool DrawInstances(Renderer renderer, SceneState state, DrawMode drawMode, DrawStyle drawStyle, DepthStencilState depthStencil = null, BlendState blend = null, RasterizerState rasterizer = null)
        {
            bool flag = false;
            lock (this.instanceBlocksLock)
            {
                if (this.DesaturationEnabled && drawStyle == DrawStyle.Color)
                    flag = flag | this.DrawSelection(renderer, state, drawMode);
                flag = flag | this.Draw(renderer, state, drawMode, drawStyle, depthStencil, blend, rasterizer);
                if (!this.DesaturationEnabled)
                {
                    if (drawStyle == DrawStyle.Color)
                        flag = flag | this.DrawSelection(renderer, state, drawMode);
                }
            }
            return flag;
        }

        private bool DrawSelection(Renderer renderer, SceneState state, DrawMode drawMode)
        {
            renderer.Profiler.BeginSection("[Selection]");
            bool flag;
            lock (this.instanceBlocksLock)
                flag = this.Draw(renderer, state, drawMode, DrawStyle.Selection, null, null, null);
            renderer.Profiler.EndSection();
            return flag;
        }

        public bool DrawAnnotation(Renderer renderer, SceneState state, DrawMode drawMode, bool blockUntilComplete)
        {
            renderer.Profiler.BeginSection("[Annotations]");
            this.blockUntilAnnotationTexturesAreAvailable = blockUntilComplete;
            drawMode |= DrawMode.PositiveValues;
            drawMode |= DrawMode.NegativeValues;
            bool flag;
            lock (this.instanceBlocksLock)
                flag = this.Draw(renderer, state, drawMode, DrawStyle.Annotation, null, null, null);
            renderer.Profiler.EndSection();
            return flag;
        }

        public void DrawAnnotationHitTest(Renderer renderer, SceneState state, DrawMode drawMode, DepthStencilState depthStencil, BlendState blendState, RasterizerState rasterizer)
        {
            this.annotationTechnique.SetRenderStates(depthStencil, blendState, rasterizer);
            drawMode |= DrawMode.PositiveValues;
            drawMode |= DrawMode.NegativeValues;
            lock (this.instanceBlocksLock)
                this.Draw(renderer, state, drawMode, DrawStyle.AnnotationHitTest, depthStencil, blendState, rasterizer);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            lock (this.instanceBlocksLock)
            {
                foreach (DisposableResource item_0 in this.instanceBlocks)
                    item_0.Dispose();
                this.instanceBlocks.Clear();
            }
            DisposableResource[] disposableResourceArray = new DisposableResource[5]
            {
                this.processingTechnique,
                this.annotationTechnique,
                this.gatherAccumulate,
                this.sharedParameters,
                this.instanceStreams
            };
            foreach (DisposableResource disposableResource in disposableResourceArray)
            {
                if (disposableResource != null)
                    disposableResource.Dispose();
            }
        }
    }
}
