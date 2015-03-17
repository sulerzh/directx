using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    internal class InstanceBlock : DisposableResource, IInstancePositionProvider
    {
        private List<int> filterItems = new List<int>();
        private List<Tuple<int, int>> filterSubsets = new List<Tuple<int, int>>();
        private List<int> annotationItems = new List<int>();
        private List<Tuple<int, int>> annotationSubsets = new List<Tuple<int, int>>();
        private List<int> firstPositionInstances = new List<int>();
        private List<Vector3D> cachedPositions = new List<Vector3D>();
        private List<Tuple<float, bool>> timeStamps = new List<Tuple<float, bool>>();
        private float lastVisualTimeScale = -1f;
        public const int BlockSize = 4096;
        private GatherAccumulateProcessor gatherAccProcessor;
        private InstanceProcessingTechnique processingTechnique;
        private VertexBuffer dataBuffer;
        private VertexBuffer timeBuffer;
        private VertexBuffer idBuffer;
        private uint hitTestLayerId;
        private IndexBuffer positiveIndices;
        private IndexBuffer negativeIndices;
        private IndexBuffer positiveUniqueIndices;
        private IndexBuffer negativeUniqueIndices;
        private IndexBuffer zeroUniqueIndices;
        private IndexBuffer nullUniqueIndices;
        private int filterItemsInSubsetsCount;
        private bool filterDirty;
        private int annotationItemsInSubsetsCount;
        private bool annotationDirty;
        private bool planarCoordinates;
        private int dataCount;
        private int geoCount;
        private int positiveCount;
        private int negativeCount;
        private int positiveUniqueCount;
        private int negativeUniqueCount;
        private int maxPositiveVisibleCount;
        private int maxNegativeVisibleCount;
        private Vector3D baseReferencePosition;
        private Vector3D previousDataPosition;
        private int zeroCount;
        private int nullCount;
        private int maxRenderPriority;
        private int maxShift;
        private bool timeEnabled;
        private bool hasMultiInstanceData;
        private bool hasEventData;
        private bool lastShowOnlyMaxValue;

        public ushort MaxClusterOrStackSize
        {
            get
            {
                return (ushort)maxShift;
            }
        }

        public ushort MaxRenderPriority
        {
            get
            {
                return (ushort)maxRenderPriority;
            }
        }

        public bool ShowNegatives { get; set; }

        public double TimeRange { get; private set; }

        public DateTime? MinInstanceTime { get; private set; }

        public unsafe bool PlanarCoordinates
        {
            get
            {
                return planarCoordinates;
            }
            set
            {
                if (value == planarCoordinates)
                    return;
                planarCoordinates = value;
                if (cachedPositions.Count <= 0)
                    return;
                InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)dataBuffer.GetData().ToPointer();
                baseReferencePosition = Coordinates.ComputePosition(cachedPositions[0], planarCoordinates);
                for (int index = 0; index < cachedPositions.Count; ++index)
                {
                    Vector3D position = Coordinates.ComputePosition(cachedPositions[index], planarCoordinates);
                    instanceBlockVertexPtr[index].X = (float)(position.X - baseReferencePosition.X);
                    instanceBlockVertexPtr[index].Y = (float)(position.Y - baseReferencePosition.Y);
                    instanceBlockVertexPtr[index].Z = (float)(position.Z - baseReferencePosition.Z);
                }
                dataBuffer.SetDirty();
            }
        }

        public int Capacity
        {
            get
            {
                if (dataBuffer != null)
                    return dataBuffer.VertexCount;
                return 0;
            }
        }

        public Vector3D BasePosition
        {
            get
            {
                return baseReferencePosition;
            }
        }

        public bool HasNegativeValues
        {
            get
            {
                return negativeUniqueCount > 0;
            }
        }

        public int Count
        {
            get
            {
                return dataCount;
            }
        }

        public bool TimeEnabled
        {
            get
            {
                return timeEnabled;
            }
        }

        public bool HasMultiInstanceData
        {
            get
            {
                return hasMultiInstanceData;
            }
        }

        public bool HasEventData
        {
            get
            {
                return hasEventData;
            }
        }

        public InstanceBlock(GatherAccumulateProcessor gatherAcc, InstanceProcessingTechnique instanceProcessingTechnique, uint layerId)
        {
            gatherAccProcessor = gatherAcc;
            processingTechnique = instanceProcessingTechnique;
            ShowNegatives = true;
            hitTestLayerId = layerId;
        }

        public unsafe InstanceId GetInstanceIdAt(int pos)
        {
            if (pos >= Count)
                return new InstanceId();
            return new InstanceId(((HitTestVertex*)idBuffer.GetData().ToPointer())[pos].HitTestId);
        }

        public unsafe Vector3F GetInstancePositionAt(int pos)
        {
            if (pos >= Count)
                return Vector3F.Empty;
            InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)dataBuffer.GetData().ToPointer();
            return (Vector3F)(new Vector3D(instanceBlockVertexPtr[pos].X, instanceBlockVertexPtr[pos].Y, instanceBlockVertexPtr[pos].Z) + baseReferencePosition);
        }

        public Vector3F GetInstancePositionAtGeoIndex(int pos, InstanceBlockQueryType queryType)
        {
            int dataPosition = GetDataPosition(pos, queryType);
            if (dataPosition < 0)
                return Vector3F.Empty;
            return GetInstancePositionAt(dataPosition);
        }

        public InstanceId GetInstanceIdAtGeoIndex(int pos, InstanceBlockQueryType queryType)
        {
            int dataPosition = GetDataPosition(pos, queryType);
            if (dataPosition < 0)
                return new InstanceId();
            return GetInstanceIdAt(dataPosition);
        }

        private unsafe int GetDataPosition(int pos, InstanceBlockQueryType queryType)
        {
            IndexBuffer indexBuffer = null;
            switch (queryType)
            {
                case InstanceBlockQueryType.PositiveInstances:
                    indexBuffer = positiveUniqueIndices;
                    break;
                case InstanceBlockQueryType.NegativeInstances:
                    indexBuffer = negativeUniqueIndices;
                    break;
                case InstanceBlockQueryType.ZeroInstances:
                    indexBuffer = zeroUniqueIndices;
                    break;
                case InstanceBlockQueryType.NullInstances:
                    indexBuffer = nullUniqueIndices;
                    break;
            }
            if (indexBuffer == null || pos >= indexBuffer.IndexCount)
                return -1;
            return (int)*((uint*)indexBuffer.GetData().ToPointer() + pos);
        }

        private static Vector3D GetPosition(bool flatMap, ref Vector3D globePosition, double longitude)
        {
            globePosition.AssertIsUnitVector();
            if (flatMap)
                return new Vector3D(1.0, Coordinates.MercatorFromSine(globePosition.Y), longitude);
            return globePosition;
        }

        public unsafe void SetData(IEnumerable<InstanceData> instanceData, int instanceDataCount, Dictionary<int, int> colorOverrides, ref Box3D bounds, bool updateBounds, double timeRange, DateTime? minTime, bool useColorAsRenderPriority)
        {
            Clear();
            TimeRange = timeRange;
            MinInstanceTime = minTime;
            previousDataPosition = Vector3D.Empty;
            int num1 = int.MinValue;
            int num2 = int.MinValue;
            foreach (InstanceData instanceData1 in instanceData)
            {
                CreateBuffersIfNeeded(instanceDataCount, instanceData1.Value < 0.0, minTime.HasValue);
                Vector3D position1 = instanceData1.Location.Position;
                if (baseReferencePosition == Vector3D.Empty)
                    baseReferencePosition = GetPosition(planarCoordinates, ref position1, instanceData1.Location.Longitude);
                if (position1 != previousDataPosition || instanceData1.FirstInstance)
                {
                    ++geoCount;
                    firstPositionInstances.Add(dataCount);
                    previousDataPosition = position1;
                    num1 = int.MinValue;
                    num2 = int.MinValue;
                }
                cachedPositions.Add(position1);
                if (updateBounds)
                    bounds.UpdateWith(position1);
                InstanceBlockVertex instanceBlockVertex = new InstanceBlockVertex();
                Vector3D position2 = GetPosition(planarCoordinates, ref position1, instanceData1.Location.Longitude);
                instanceBlockVertex.X = (float)(position2.X - baseReferencePosition.X);
                instanceBlockVertex.Y = (float)(position2.Y - baseReferencePosition.Y);
                instanceBlockVertex.Z = (float)(position2.Z - baseReferencePosition.Z);
                instanceBlockVertex.GeoIndex = (short)(geoCount - 1);
                instanceBlockVertex.Shift = instanceData1.Shift;
                int num3 = 0;
                instanceBlockVertex.ColorIndex = !colorOverrides.TryGetValue(instanceData1.SourceShift, out num3) ? instanceData1.Color : (short)num3;
                instanceBlockVertex.RenderPriority = useColorAsRenderPriority ? instanceData1.Color : instanceData1.SourceShift;
                instanceBlockVertex.Value = Math.Abs(instanceData1.Value);
                InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)dataBuffer.GetData().ToPointer();
                HitTestVertex* hitTestVertexPtr = (HitTestVertex*)idBuffer.GetData().ToPointer();
                instanceBlockVertexPtr[dataCount] = instanceBlockVertex;
                hitTestVertexPtr[dataCount].HitTestId = new InstanceId(hitTestLayerId, instanceData1.Id).Id;
                dataBuffer.SetDirty();
                idBuffer.SetDirty();
                if (float.IsNaN(instanceData1.Value))
                {
                    *(int*)((IntPtr)nullUniqueIndices.GetData().ToPointer() + nullCount++) = dataCount;
                    instanceBlockVertexPtr[dataCount].Value = -2f;
                }
                if (instanceData1.Value == 0.0)
                    *(int*)((IntPtr)zeroUniqueIndices.GetData().ToPointer() + zeroCount++) = dataCount;
                if (minTime.HasValue)
                {
                    InstanceTime* instanceTimePtr = (InstanceTime*)timeBuffer.GetData().ToPointer();
                    timeBuffer.SetDirty();
                    instanceTimePtr[dataCount].StartTime = !instanceData1.StartTime.HasValue ? -1f : (timeRange == 0.0 ? 0.0f : (float)((instanceData1.StartTime.Value - minTime.Value).TotalMilliseconds / timeRange));
                    instanceTimePtr[dataCount].EndTime = !instanceData1.EndTime.HasValue ? -1f : (timeRange == 0.0 ? 0.0f : (float)((instanceData1.EndTime.Value - minTime.Value).TotalMilliseconds / timeRange));
                    if (instanceData1.StartTime.HasValue && instanceData1.EndTime.HasValue && instanceData1.StartTime.Value == instanceData1.EndTime.Value)
                        hasEventData = true;
                }
                else if (timeBuffer != null)
                {
                    timeBuffer.Dispose();
                    timeBuffer = null;
                }
                if (float.IsNaN(instanceData1.Value) || instanceData1.Value >= 0.0)
                {
                    *(int*)((IntPtr)positiveIndices.GetData().ToPointer() + positiveCount++) = dataCount;
                    positiveIndices.SetDirty();
                    if (num1 != instanceData1.Shift)
                    {
                        *(int*)((IntPtr)positiveUniqueIndices.GetData().ToPointer() + positiveUniqueCount++) = dataCount;
                        num1 = instanceData1.Shift;
                        positiveUniqueIndices.SetDirty();
                    }
                }
                else
                {
                    *(int*)((IntPtr)negativeIndices.GetData().ToPointer() + negativeCount++) = dataCount;
                    negativeIndices.SetDirty();
                    if (num2 != instanceData1.Shift)
                    {
                        *(int*)((IntPtr)negativeUniqueIndices.GetData().ToPointer() + negativeUniqueCount++) = dataCount;
                        num2 = instanceData1.Shift;
                        negativeUniqueIndices.SetDirty();
                    }
                }
                ++dataCount;
                maxShift = Math.Max(maxShift, instanceData1.Shift);
                maxRenderPriority = Math.Max(maxRenderPriority, useColorAsRenderPriority ? instanceData1.Color : instanceData1.SourceShift);
            }
            if (zeroCount > 0)
                zeroUniqueIndices.SetDirty();
            if (nullCount > 0)
                nullUniqueIndices.SetDirty();
            if (minTime.HasValue)
                timeEnabled = true;
            if (dataCount == geoCount && !timeEnabled && negativeCount <= 0)
                return;
            hasMultiInstanceData = true;
        }

        private unsafe void ComputeMaxSimultaneousInstances(float scaledFadeTime)
        {
            if (!timeEnabled)
            {
                maxPositiveVisibleCount = positiveUniqueCount;
                maxNegativeVisibleCount = negativeUniqueCount;
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    timeStamps.Clear();
                    int count = i == 0 ? positiveCount : negativeCount;
                    if (count != 0)
                    {
                        InstanceTime* instanceTimeArray = (InstanceTime*)timeBuffer.GetData().ToPointer();
                        IndexBuffer buffer = i == 0 ? positiveIndices : negativeIndices;
                        uint* indexArray = (uint*)buffer.GetData().ToPointer();
                        for (int j = 0; j < count; ++j)
                        {
                            timeStamps.Add(new Tuple<float, bool>(instanceTimeArray[indexArray[j]].StartTime <= 0.0 ? 0.0f : instanceTimeArray[indexArray[j]].StartTime, true));
                            float num2 = instanceTimeArray[indexArray[j]].EndTime;
                            if (num2 >= 0.0)
                                timeStamps.Add(new Tuple<float, bool>(num2 + scaledFadeTime * 2f, false));
                        }
                        timeStamps.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                        int val1 = 0;
                        int val2 = 0;
                        for (int index2 = 0; index2 < timeStamps.Count; ++index2)
                        {
                            val2 += timeStamps[index2].Item2 ? 1 : -1;
                            val1 = Math.Max(val1, val2);
                        }
                        if (i == 0)
                            maxPositiveVisibleCount = Math.Min(val1, positiveUniqueCount);
                        else
                            maxNegativeVisibleCount = Math.Min(val1, negativeUniqueCount);
                    }
                }
            }
        }

        public void Clear()
        {
            dataCount = 0;
            geoCount = 0;
            positiveCount = 0;
            negativeCount = 0;
            positiveUniqueCount = 0;
            negativeUniqueCount = 0;
            maxPositiveVisibleCount = 0;
            maxNegativeVisibleCount = 0;
            zeroCount = 0;
            nullCount = 0;
            maxShift = 0;
            maxRenderPriority = 0;
            timeEnabled = false;
            hasMultiInstanceData = false;
            hasEventData = false;
            baseReferencePosition = Vector3D.Empty;
            previousDataPosition = Vector3D.Empty;
            lastVisualTimeScale = -1f;
            firstPositionInstances.Clear();
            cachedPositions.Clear();
            filterItems.Clear();
            filterSubsets.Clear();
            filterItemsInSubsetsCount = 0;
            filterDirty = false;
            annotationItems.Clear();
            annotationSubsets.Clear();
            annotationItemsInSubsetsCount = 0;
            annotationDirty = false;
        }

        public void AddFilterInstance(int pos)
        {
            filterItems.Add(pos);
            filterDirty = true;
        }

        public void ClearFilterInstances()
        {
            filterItems.Clear();
            filterDirty = true;
        }

        public void AddAnnotationInstance(int pos)
        {
            annotationItems.Add(pos);
            annotationDirty = true;
        }

        public void ClearAnnotationInstances()
        {
            annotationItems.Clear();
            annotationDirty = true;
        }

        public unsafe InstanceId? GetAnnotationInstanceId(int annotationIndex, DateTime? time, SceneState state)
        {
            if (annotationIndex >= annotationItems.Count)
                return new InstanceId?();
            if (Count == 0)
                return new InstanceId?();
            int pos1 = annotationItems[annotationIndex];
            if (!time.HasValue || timeBuffer == null || timeBuffer.VertexCount == 0)
                return GetInstanceIdAt(pos1);
            if (!MinInstanceTime.HasValue)
                return new InstanceId?();
            int num1 = 0;
            int num2 = 0;
            for (int index = 0; index < firstPositionInstances.Count; ++index)
            {
                if (firstPositionInstances[index] > pos1)
                {
                    num2 = firstPositionInstances[index] - num1;
                    break;
                }
                num1 = firstPositionInstances[index];
            }
            if (num2 == 0)
                num2 = dataCount - num1;
            InstanceId? nullable1 = new InstanceId?();
            InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)dataBuffer.GetData().ToPointer();
            InstanceTime* instanceTimePtr = (InstanceTime*)timeBuffer.GetData().ToPointer();
            int num3 = instanceBlockVertexPtr[pos1].Shift;
            for (int pos2 = num1; pos2 < num1 + num2; ++pos2)
            {
                if (instanceBlockVertexPtr[pos2].Shift >= num3)
                {
                    if (instanceBlockVertexPtr[pos2].Shift <= num3)
                    {
                        DateTime? nullable2 = (double)instanceTimePtr[pos2].StartTime == -1.0 ? new DateTime?() : MinInstanceTime.Value.AddMilliseconds(instanceTimePtr[pos2].StartTime * TimeRange);
                        DateTime? nullable3 = (double)instanceTimePtr[pos2].EndTime == -1.0 ? new DateTime?() : MinInstanceTime.Value.AddMilliseconds(instanceTimePtr[pos2].EndTime * TimeRange);
                        if (nullable3.HasValue)
                        {
                            double num4 = 0.25 * state.VisualTimeToRealtimeRatio * 1000.0 * 2.0;
                            nullable3 = nullable3.Value.AddMilliseconds(num4);
                        }
                        int num5;
                        if (nullable2.HasValue)
                        {
                            DateTime? nullable4 = nullable2;
                            DateTime? nullable5 = time;
                            num5 = nullable4.HasValue & nullable5.HasValue ? (nullable4.GetValueOrDefault() <= nullable5.GetValueOrDefault() ? 1 : 0) : 0;
                        }
                        else
                            num5 = 1;
                        bool flag1 = num5 != 0;
                        int num6;
                        if (nullable3.HasValue)
                        {
                            DateTime? nullable4 = nullable3;
                            DateTime? nullable5 = time;
                            num6 = nullable4.HasValue & nullable5.HasValue ? (nullable4.GetValueOrDefault() >= nullable5.GetValueOrDefault() ? 1 : 0) : 0;
                        }
                        else
                            num6 = 1;
                        bool flag2 = num6 != 0;
                        if (flag1 && flag2)
                            nullable1 = GetInstanceIdAt(pos2);
                    }
                    else
                        break;
                }
            }
            return nullable1;
        }

        private static void UpdateSubsets(List<int> items, List<Tuple<int, int>> subsets, ref int itemsInSubsetsCount, ref bool dirty)
        {
            if (!dirty)
                return;
            dirty = false;
            subsets.Clear();
            if (items.Count == 0)
            {
                itemsInSubsetsCount = 0;
            }
            else
            {
                items.Sort();
                int num1 = items[0];
                int num2 = 1;
                for (int index = 1; index < items.Count; ++index)
                {
                    if (items[index] == num1 + num2)
                    {
                        ++num2;
                    }
                    else
                    {
                        subsets.Add(new Tuple<int, int>(num1, num2));
                        num1 = items[index];
                        num2 = 1;
                    }
                }
                subsets.Add(new Tuple<int, int>(num1, num2));
                itemsInSubsetsCount = items.Count;
            }
        }

        private void CreateBuffersIfNeeded(int instanceCount, bool needNegativeIndices, bool needsTimeBuffer)
        {
            int count = Math.Max(instanceCount, BlockSize);
            if (dataBuffer == null)
            {
                dataBuffer = VertexBuffer.Create<InstanceBlockVertex>(null, count, false);
                positiveIndices = IndexBuffer.Create<uint>(null, count, false);
                positiveUniqueIndices = IndexBuffer.Create<uint>(null, count, false);
                zeroUniqueIndices = IndexBuffer.Create<uint>(null, count, false);
                nullUniqueIndices = IndexBuffer.Create<uint>(null, count, false);
            }
            if (needNegativeIndices && negativeIndices == null)
            {
                negativeIndices = IndexBuffer.Create<uint>(null, count, false);
                negativeUniqueIndices = IndexBuffer.Create<uint>(null, count, false);
            }
            if (timeBuffer != null && timeBuffer.VertexCount < count)
            {
                timeBuffer.Dispose();
                timeBuffer = null;
            }
            if (needsTimeBuffer && timeBuffer == null)
                timeBuffer = VertexBuffer.Create<InstanceTime>(null, count, false);
            if (idBuffer != null)
                return;
            idBuffer = VertexBuffer.Create<InstanceColor>(null, count, false);
        }

        private int Update(Renderer renderer, bool ignoreInstanceValues, bool hitTest, bool useGatherAccumulate, bool useConstantMode)
        {
            int num = 0;
            if (useGatherAccumulate)
            {
                GatherAccumulateProcessBlock gatherAccumulateBlock = GetGatherAccumulateBlock();
                num = !hitTest ? gatherAccProcessor.Process(gatherAccumulateBlock, renderer, ignoreInstanceValues, useConstantMode) : gatherAccProcessor.ProcessHitTest(gatherAccumulateBlock, renderer, ignoreInstanceValues, useConstantMode);
            }
            if (!hitTest)
            {
                UpdateSubsets(filterItems, filterSubsets, ref filterItemsInSubsetsCount, ref filterDirty);
                UpdateSubsets(annotationItems, annotationSubsets, ref annotationItemsInSubsetsCount, ref annotationDirty);
            }
            return num;
        }

        public GatherAccumulateProcessBlock GetGatherAccumulateBlock()
        {
            return new GatherAccumulateProcessBlock()
            {
                PositiveIndices = positiveIndices,
                PositiveSubset = new Tuple<uint, uint>(0U, (uint)positiveCount),
                NegativeIndices = negativeIndices,
                NegativeSubset = negativeCount <= 0 || !ShowNegatives ? null : new Tuple<uint, uint>(0U, (uint)negativeCount),
                Instances = dataBuffer,
                InstancesTime = timeBuffer,
                InstancesHitId = idBuffer,
                MaxShift = maxShift,
                Owner = this
            };
        }

        public int QueryInstanceCount(float visualTimeScale, bool showOnlyMaxValue, InstanceBlockQueryType queryType)
        {
            if (this.lastVisualTimeScale != visualTimeScale || this.lastShowOnlyMaxValue != showOnlyMaxValue)
            {
                ComputeMaxSimultaneousInstances(1.0f / visualTimeScale * 0.25f);
                this.lastVisualTimeScale = visualTimeScale;
                this.lastShowOnlyMaxValue = showOnlyMaxValue;
            }
            int val2 = Math.Max(this.filterItems.Count, this.annotationItems.Count);
            int zeroCount = 0;
            switch (queryType)
            {
                case InstanceBlockQueryType.PositiveInstances:
                    zeroCount = showOnlyMaxValue ? this.positiveUniqueCount : this.maxPositiveVisibleCount;
                    break;
                case InstanceBlockQueryType.NegativeInstances:
                    zeroCount = showOnlyMaxValue ? this.negativeUniqueCount : this.maxNegativeVisibleCount;
                    break;
                case InstanceBlockQueryType.ZeroInstances:
                    zeroCount = this.zeroCount;
                    break;
                case InstanceBlockQueryType.NullInstances:
                    zeroCount = this.nullCount;
                    break;
            }
            return Math.Max(zeroCount, val2);
        }

        public int QueryInstances(Renderer renderer, SceneState state, InstanceBlockQueryParameters parameters)
        {
            if (parameters == null)
                return 0;
            switch (parameters.QueryType)
            {
                case InstanceBlockQueryType.PositiveInstances:
                    if (maxPositiveVisibleCount > 0)
                        UpdateInstances(renderer, parameters);
                    switch (parameters.InstanceSource)
                    {
                        case InstanceBlockQueryInstanceSource.Filter:
                            return filterItemsInSubsetsCount;
                        case InstanceBlockQueryInstanceSource.Annotation:
                            return annotationItemsInSubsetsCount;
                        default:
                            if (parameters.ShowOnlyMaxValues)
                                return positiveUniqueCount;
                            return maxPositiveVisibleCount;
                    }
                case InstanceBlockQueryType.NegativeInstances:
                    if (maxNegativeVisibleCount > 0)
                        UpdateInstances(renderer, parameters);
                    switch (parameters.InstanceSource)
                    {
                        case InstanceBlockQueryInstanceSource.Filter:
                            return filterItemsInSubsetsCount;
                        case InstanceBlockQueryInstanceSource.Annotation:
                            return annotationItemsInSubsetsCount;
                        default:
                            if (parameters.ShowOnlyMaxValues)
                                return negativeUniqueCount;
                            return maxNegativeVisibleCount;
                    }
                case InstanceBlockQueryType.ZeroInstances:
                    if (zeroCount > 0)
                    {
                        processingTechnique.Mode = parameters.HitTest ? InstanceProcessingTechniqueMode.ZeroHitTest : InstanceProcessingTechniqueMode.Zero;
                        UpdateZeroNullInstances(renderer, parameters);
                    }
                    switch (parameters.InstanceSource)
                    {
                        case InstanceBlockQueryInstanceSource.Filter:
                            return Math.Min(filterItemsInSubsetsCount, zeroCount);
                        case InstanceBlockQueryInstanceSource.Annotation:
                            return Math.Max(annotationItemsInSubsetsCount, zeroCount);
                        default:
                            return zeroCount;
                    }
                case InstanceBlockQueryType.NullInstances:
                    if (nullCount > 0)
                    {
                        processingTechnique.Mode = parameters.HitTest ? InstanceProcessingTechniqueMode.NullHitTest : InstanceProcessingTechniqueMode.Null;
                        UpdateZeroNullInstances(renderer, parameters);
                    }
                    switch (parameters.InstanceSource)
                    {
                        case InstanceBlockQueryInstanceSource.Filter:
                            return Math.Min(filterItemsInSubsetsCount, nullCount);
                        case InstanceBlockQueryInstanceSource.Annotation:
                            return Math.Max(annotationItemsInSubsetsCount, nullCount);
                        default:
                            return nullCount;
                    }
                default:
                    return 0;
            }
        }

        private void UpdateInstances(Renderer renderer, InstanceBlockQueryParameters parameters)
        {
            bool flag = parameters.QueryType == InstanceBlockQueryType.NegativeInstances;
            if ((flag ? negativeUniqueCount : positiveUniqueCount) == 0 || parameters.InstanceSource == InstanceBlockQueryInstanceSource.Filter && filterItemsInSubsetsCount == 0 || parameters.InstanceSource == InstanceBlockQueryInstanceSource.Annotation && annotationItemsInSubsetsCount == 0)
                return;
            bool useGatherAccumulate = hasMultiInstanceData || parameters.HitTest || parameters.IsPieChart || parameters.UseLogScale;
            int num = Update(renderer, parameters.IgnoreInstanceValues, parameters.HitTest, useGatherAccumulate, parameters.IsClusterChart);
            renderer.RenderLockEnter();
            try
            {
                Renderer renderer1 = renderer;
                VertexBuffer[] vertexBuffers;
                if (!timeEnabled)
                    vertexBuffers = new VertexBuffer[]{dataBuffer};
                else
                    vertexBuffers = new VertexBuffer[]{dataBuffer,timeBuffer};
                renderer1.SetVertexSourceNoLock(vertexBuffers);
                renderer.SetIndexSourceNoLock(flag ? negativeUniqueIndices : positiveUniqueIndices);
                renderer.SetStreamBufferNoLock(parameters.InstanceOutputBuffer);
                renderer.SetVertexTextureNoLock(0, flag ? gatherAccProcessor.GatherNegativeTexture : gatherAccProcessor.GatherPositiveTexture);
                renderer.SetVertexTextureNoLock(1, flag ? gatherAccProcessor.AccumulateNegativeTexture : gatherAccProcessor.AccumulatePositiveTexture);
                renderer.SetVertexTextureNoLock(2, flag ? gatherAccProcessor.AccumulatePositiveTexture : gatherAccProcessor.AccumulateNegativeTexture);
                renderer.SetVertexTextureNoLock(3, flag ? null : gatherAccProcessor.GatherNegativeTexture);
                renderer.SetVertexTextureNoLock(4, gatherAccProcessor.SelectPositiveTexture);
                if (parameters.HitTest)
                {
                    renderer.SetVertexTextureNoLock(5, flag ? gatherAccProcessor.GatherNegativeHitTestTexture : gatherAccProcessor.GatherPositiveHitTestTexture);
                    processingTechnique.Mode = InstanceProcessingTechniqueMode.HitTest;
                }
                else
                    processingTechnique.Mode = parameters.IsPieChart ? InstanceProcessingTechniqueMode.Pie : InstanceProcessingTechniqueMode.Default;
                processingTechnique.MaxShift = maxShift;
                processingTechnique.UseNegatives = ShowNegatives;
                processingTechnique.UseGatherAccumulate = hasMultiInstanceData || parameters.IsPieChart || parameters.UseLogScale;
                processingTechnique.ShiftOffset = num;
                processingTechnique.ValueOffset = parameters.Offset;
                processingTechnique.AnnotationPlacementEnabled = parameters.InstanceSource == InstanceBlockQueryInstanceSource.Annotation;
                processingTechnique.ShowOnlyMaxValueEnabled = parameters.ShowOnlyMaxValues;
                processingTechnique.NegativeValues = flag;
                processingTechnique.UseLogarithmicScale = parameters.UseLogScale;
                renderer.SetEffect(processingTechnique);
                if (parameters.InstanceSource != InstanceBlockQueryInstanceSource.Block)
                {
                    List<Tuple<int, int>> list = parameters.InstanceSource == InstanceBlockQueryInstanceSource.Filter ? filterSubsets : annotationSubsets;
                    for (int i = 0; i < list.Count; ++i)
                        renderer.DrawNoLock(list[i].Item1, list[i].Item2, PrimitiveTopology.PointList);
                }
                else
                    renderer.DrawIndexedNoLock(0, flag ? negativeUniqueCount : positiveUniqueCount, PrimitiveTopology.PointList);
                renderer.SetStreamBufferNoLock(null);
                renderer.SetVertexTextureNoLock(0, new Texture[6]);
            }
            finally
            {
                renderer.RenderLockExit();
            }
        }

        private void UpdateZeroNullInstances(Renderer renderer, InstanceBlockQueryParameters parameters)
        {
            int num = Update(renderer, parameters.IgnoreInstanceValues, parameters.HitTest,
                hasMultiInstanceData || 
                parameters.HitTest || 
                parameters.UseLogScale, 
                parameters.IsClusterChart);
            if (parameters.InstanceSource == InstanceBlockQueryInstanceSource.Filter && filterItemsInSubsetsCount == 0 || parameters.InstanceSource == InstanceBlockQueryInstanceSource.Annotation && annotationItemsInSubsetsCount == 0)
                return;
            renderer.RenderLockEnter();
            try
            {
                Renderer renderer1 = renderer;
                VertexBuffer[] vertexBuffers;
                if (!timeEnabled)
                    vertexBuffers = new VertexBuffer[]{dataBuffer};
                else
                    vertexBuffers = new VertexBuffer[]{dataBuffer,timeBuffer};
                renderer1.SetVertexSourceNoLock(vertexBuffers);
                if (parameters.QueryType == InstanceBlockQueryType.ZeroInstances)
                    renderer.SetIndexSourceNoLock(zeroUniqueIndices);
                else
                    renderer.SetIndexSourceNoLock(nullUniqueIndices);
                renderer.SetStreamBufferNoLock(parameters.InstanceOutputBuffer);
                renderer.SetVertexTextureNoLock(0, gatherAccProcessor.AccumulatePositiveTexture);
                renderer.SetVertexTextureNoLock(1, gatherAccProcessor.AccumulateNegativeTexture ?? gatherAccProcessor.AccumulatePositiveTexture);
                renderer.SetVertexTextureNoLock(2, gatherAccProcessor.GatherPositiveTexture);
                renderer.SetVertexTextureNoLock(3, gatherAccProcessor.SelectPositiveTexture);
                if (parameters.HitTest)
                    renderer.SetVertexTextureNoLock(4, gatherAccProcessor.GatherPositiveHitTestTexture);
                processingTechnique.MaxShift = maxShift;
                processingTechnique.UseNegatives = ShowNegatives && negativeUniqueCount > 0;
                processingTechnique.UseGatherAccumulate = hasMultiInstanceData || parameters.IsPieChart || parameters.UseLogScale;
                processingTechnique.ShiftOffset = num;
                processingTechnique.ValueOffset = parameters.Offset;
                processingTechnique.AnnotationPlacementEnabled = parameters.InstanceSource == InstanceBlockQueryInstanceSource.Annotation;
                processingTechnique.ShowOnlyMaxValueEnabled = parameters.ShowOnlyMaxValues;
                processingTechnique.UseLogarithmicScale = parameters.UseLogScale;
                renderer.SetEffect(processingTechnique);
                if (parameters.InstanceSource != InstanceBlockQueryInstanceSource.Block)
                {
                    List<Tuple<int, int>> list = parameters.InstanceSource == InstanceBlockQueryInstanceSource.Filter ? filterSubsets : annotationSubsets;
                    for (int index = 0; index < list.Count; ++index)
                        renderer.DrawNoLock(list[index].Item1, list[index].Item2, PrimitiveTopology.PointList);
                }
                else
                    renderer.DrawIndexedNoLock(0, parameters.QueryType == InstanceBlockQueryType.ZeroInstances ? zeroCount : nullCount, PrimitiveTopology.PointList);
                renderer.SetStreamBufferNoLock(null);
                renderer.SetVertexTextureNoLock(0, new Texture[5]);
            }
            finally
            {
                renderer.RenderLockExit();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            DisposableResource[] resArray = new DisposableResource[9]
            {
                dataBuffer,
                timeBuffer,
                idBuffer,
                positiveIndices,
                negativeIndices,
                positiveUniqueIndices,
                negativeUniqueIndices,
                zeroUniqueIndices,
                nullUniqueIndices
            };
            foreach (DisposableResource res in resArray)
            {
                if (res != null && !res.Disposed)
                    res.Dispose();
            }
        }
    }
}
