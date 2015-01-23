// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceBlock
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

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
                return (ushort)this.maxShift;
            }
        }

        public ushort MaxRenderPriority
        {
            get
            {
                return (ushort)this.maxRenderPriority;
            }
        }

        public bool ShowNegatives { get; set; }

        public double TimeRange { get; private set; }

        public DateTime? MinInstanceTime { get; private set; }

        public unsafe bool PlanarCoordinates
        {
            get
            {
                return this.planarCoordinates;
            }
            set
            {
                if (value == this.planarCoordinates)
                    return;
                this.planarCoordinates = value;
                if (this.cachedPositions.Count <= 0)
                    return;
                InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)this.dataBuffer.GetData().ToPointer();
                this.baseReferencePosition = Coordinates.ComputePosition(this.cachedPositions[0], this.planarCoordinates);
                for (int index = 0; index < this.cachedPositions.Count; ++index)
                {
                    Vector3D position = Coordinates.ComputePosition(this.cachedPositions[index], this.planarCoordinates);
                    instanceBlockVertexPtr[index].X = (float)(position.X - this.baseReferencePosition.X);
                    instanceBlockVertexPtr[index].Y = (float)(position.Y - this.baseReferencePosition.Y);
                    instanceBlockVertexPtr[index].Z = (float)(position.Z - this.baseReferencePosition.Z);
                }
                this.dataBuffer.SetDirty();
            }
        }

        public int Capacity
        {
            get
            {
                if (this.dataBuffer != null)
                    return this.dataBuffer.VertexCount;
                else
                    return 0;
            }
        }

        public Vector3D BasePosition
        {
            get
            {
                return this.baseReferencePosition;
            }
        }

        public bool HasNegativeValues
        {
            get
            {
                return this.negativeUniqueCount > 0;
            }
        }

        public int Count
        {
            get
            {
                return this.dataCount;
            }
        }

        public bool TimeEnabled
        {
            get
            {
                return this.timeEnabled;
            }
        }

        public bool HasMultiInstanceData
        {
            get
            {
                return this.hasMultiInstanceData;
            }
        }

        public bool HasEventData
        {
            get
            {
                return this.hasEventData;
            }
        }

        public InstanceBlock(GatherAccumulateProcessor gatherAcc, InstanceProcessingTechnique instanceProcessingTechnique, uint layerId)
        {
            this.gatherAccProcessor = gatherAcc;
            this.processingTechnique = instanceProcessingTechnique;
            this.ShowNegatives = true;
            this.hitTestLayerId = layerId;
        }

        public unsafe InstanceId GetInstanceIdAt(int pos)
        {
            if (pos >= this.Count)
                return new InstanceId();
            else
                return new InstanceId(((HitTestVertex*)this.idBuffer.GetData().ToPointer())[pos].HitTestId);
        }

        public unsafe Vector3F GetInstancePositionAt(int pos)
        {
            if (pos >= this.Count)
                return Vector3F.Empty;
            InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)this.dataBuffer.GetData().ToPointer();
            return (Vector3F)(new Vector3D((double)instanceBlockVertexPtr[pos].X, (double)instanceBlockVertexPtr[pos].Y, (double)instanceBlockVertexPtr[pos].Z) + this.baseReferencePosition);
        }

        public Vector3F GetInstancePositionAtGeoIndex(int pos, InstanceBlockQueryType queryType)
        {
            int dataPosition = this.GetDataPosition(pos, queryType);
            if (dataPosition < 0)
                return Vector3F.Empty;
            else
                return this.GetInstancePositionAt(dataPosition);
        }

        public InstanceId GetInstanceIdAtGeoIndex(int pos, InstanceBlockQueryType queryType)
        {
            int dataPosition = this.GetDataPosition(pos, queryType);
            if (dataPosition < 0)
                return new InstanceId();
            else
                return this.GetInstanceIdAt(dataPosition);
        }

        private unsafe int GetDataPosition(int pos, InstanceBlockQueryType queryType)
        {
            IndexBuffer indexBuffer = (IndexBuffer)null;
            switch (queryType)
            {
                case InstanceBlockQueryType.PositiveInstances:
                    indexBuffer = this.positiveUniqueIndices;
                    break;
                case InstanceBlockQueryType.NegativeInstances:
                    indexBuffer = this.negativeUniqueIndices;
                    break;
                case InstanceBlockQueryType.ZeroInstances:
                    indexBuffer = this.zeroUniqueIndices;
                    break;
                case InstanceBlockQueryType.NullInstances:
                    indexBuffer = this.nullUniqueIndices;
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
            else
                return globePosition;
        }

        public unsafe void SetData(IEnumerable<InstanceData> instanceData, int instanceDataCount, Dictionary<int, int> colorOverrides, ref Box3D bounds, bool updateBounds, double timeRange, DateTime? minTime, bool useColorAsRenderPriority)
        {
            this.Clear();
            this.TimeRange = timeRange;
            this.MinInstanceTime = minTime;
            this.previousDataPosition = Vector3D.Empty;
            int num1 = int.MinValue;
            int num2 = int.MinValue;
            foreach (InstanceData instanceData1 in instanceData)
            {
                this.CreateBuffersIfNeeded(instanceDataCount, (double)instanceData1.Value < 0.0, minTime.HasValue);
                Vector3D position1 = instanceData1.Location.Position;
                if (this.baseReferencePosition == Vector3D.Empty)
                    this.baseReferencePosition = InstanceBlock.GetPosition(this.planarCoordinates, ref position1, instanceData1.Location.Longitude);
                if (position1 != this.previousDataPosition || instanceData1.FirstInstance)
                {
                    ++this.geoCount;
                    this.firstPositionInstances.Add(this.dataCount);
                    this.previousDataPosition = position1;
                    num1 = int.MinValue;
                    num2 = int.MinValue;
                }
                this.cachedPositions.Add(position1);
                if (updateBounds)
                    bounds.UpdateWith(position1);
                InstanceBlockVertex instanceBlockVertex = new InstanceBlockVertex();
                Vector3D position2 = InstanceBlock.GetPosition(this.planarCoordinates, ref position1, instanceData1.Location.Longitude);
                instanceBlockVertex.X = (float)(position2.X - this.baseReferencePosition.X);
                instanceBlockVertex.Y = (float)(position2.Y - this.baseReferencePosition.Y);
                instanceBlockVertex.Z = (float)(position2.Z - this.baseReferencePosition.Z);
                instanceBlockVertex.GeoIndex = (short)(this.geoCount - 1);
                instanceBlockVertex.Shift = instanceData1.Shift;
                int num3 = 0;
                instanceBlockVertex.ColorIndex = !colorOverrides.TryGetValue((int)instanceData1.SourceShift, out num3) ? instanceData1.Color : (short)num3;
                instanceBlockVertex.RenderPriority = useColorAsRenderPriority ? instanceData1.Color : instanceData1.SourceShift;
                instanceBlockVertex.Value = Math.Abs(instanceData1.Value);
                InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)this.dataBuffer.GetData().ToPointer();
                HitTestVertex* hitTestVertexPtr = (HitTestVertex*)this.idBuffer.GetData().ToPointer();
                instanceBlockVertexPtr[this.dataCount] = instanceBlockVertex;
                hitTestVertexPtr[this.dataCount].HitTestId = new InstanceId(this.hitTestLayerId, instanceData1.Id).Id;
                this.dataBuffer.SetDirty();
                this.idBuffer.SetDirty();
                if (float.IsNaN(instanceData1.Value))
                {
                    *(int*)((IntPtr)this.nullUniqueIndices.GetData().ToPointer() + this.nullCount++) = this.dataCount;
                    instanceBlockVertexPtr[this.dataCount].Value = -2f;
                }
                if ((double)instanceData1.Value == 0.0)
                    *(int*)((IntPtr)this.zeroUniqueIndices.GetData().ToPointer() + this.zeroCount++) = this.dataCount;
                if (minTime.HasValue)
                {
                    InstanceTime* instanceTimePtr = (InstanceTime*)this.timeBuffer.GetData().ToPointer();
                    this.timeBuffer.SetDirty();
                    instanceTimePtr[this.dataCount].StartTime = !instanceData1.StartTime.HasValue ? -1f : (timeRange == 0.0 ? 0.0f : (float)((instanceData1.StartTime.Value - minTime.Value).TotalMilliseconds / timeRange));
                    instanceTimePtr[this.dataCount].EndTime = !instanceData1.EndTime.HasValue ? -1f : (timeRange == 0.0 ? 0.0f : (float)((instanceData1.EndTime.Value - minTime.Value).TotalMilliseconds / timeRange));
                    if (instanceData1.StartTime.HasValue && instanceData1.EndTime.HasValue && instanceData1.StartTime.Value == instanceData1.EndTime.Value)
                        this.hasEventData = true;
                }
                else if (this.timeBuffer != null)
                {
                    this.timeBuffer.Dispose();
                    this.timeBuffer = (VertexBuffer)null;
                }
                if (float.IsNaN(instanceData1.Value) || (double)instanceData1.Value >= 0.0)
                {
                    *(int*)((IntPtr)this.positiveIndices.GetData().ToPointer() + this.positiveCount++) = this.dataCount;
                    this.positiveIndices.SetDirty();
                    if (num1 != (int)instanceData1.Shift)
                    {
                        *(int*)((IntPtr)this.positiveUniqueIndices.GetData().ToPointer() + this.positiveUniqueCount++) = this.dataCount;
                        num1 = (int)instanceData1.Shift;
                        this.positiveUniqueIndices.SetDirty();
                    }
                }
                else
                {
                    *(int*)((IntPtr)this.negativeIndices.GetData().ToPointer() + this.negativeCount++) = this.dataCount;
                    this.negativeIndices.SetDirty();
                    if (num2 != (int)instanceData1.Shift)
                    {
                        *(int*)((IntPtr)this.negativeUniqueIndices.GetData().ToPointer() + this.negativeUniqueCount++) = this.dataCount;
                        num2 = (int)instanceData1.Shift;
                        this.negativeUniqueIndices.SetDirty();
                    }
                }
                ++this.dataCount;
                this.maxShift = Math.Max(this.maxShift, (int)instanceData1.Shift);
                this.maxRenderPriority = Math.Max(this.maxRenderPriority, useColorAsRenderPriority ? (int)instanceData1.Color : (int)instanceData1.SourceShift);
            }
            if (this.zeroCount > 0)
                this.zeroUniqueIndices.SetDirty();
            if (this.nullCount > 0)
                this.nullUniqueIndices.SetDirty();
            if (minTime.HasValue)
                this.timeEnabled = true;
            if (this.dataCount == this.geoCount && !this.timeEnabled && this.negativeCount <= 0)
                return;
            this.hasMultiInstanceData = true;
        }

        private unsafe void ComputeMaxSimultaneousInstances(float scaledFadeTime)
        {
            if (!this.timeEnabled)
            {
                this.maxPositiveVisibleCount = this.positiveUniqueCount;
                this.maxNegativeVisibleCount = this.negativeUniqueCount;
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    this.timeStamps.Clear();
                    int num1 = index1 == 0 ? this.positiveCount : this.negativeCount;
                    if (num1 != 0)
                    {
                        InstanceTime* instanceTimePtr = (InstanceTime*)this.timeBuffer.GetData().ToPointer();
                        uint* numPtr = (uint*)(index1 == 0 ? this.positiveIndices : this.negativeIndices).GetData().ToPointer();
                        for (int index2 = 0; index2 < num1; ++index2)
                        {
                            this.timeStamps.Add(new Tuple<float, bool>((double)((InstanceTime*)instanceTimePtr + numPtr[index2])->StartTime <= 0.0 ? 0.0f : ((InstanceTime*)instanceTimePtr + numPtr[index2])->StartTime, true));
                            float num2 = ((InstanceTime*)instanceTimePtr + numPtr[index2])->EndTime;
                            if ((double)num2 >= 0.0)
                                this.timeStamps.Add(new Tuple<float, bool>(num2 + scaledFadeTime * 2f, false));
                        }
                        this.timeStamps.Sort((Comparison<Tuple<float, bool>>)((a, b) => a.Item1.CompareTo(b.Item1)));
                        int val1 = 0;
                        int val2 = 0;
                        for (int index2 = 0; index2 < this.timeStamps.Count; ++index2)
                        {
                            val2 += this.timeStamps[index2].Item2 ? 1 : -1;
                            val1 = Math.Max(val1, val2);
                        }
                        if (index1 == 0)
                            this.maxPositiveVisibleCount = Math.Min(val1, this.positiveUniqueCount);
                        else
                            this.maxNegativeVisibleCount = Math.Min(val1, this.negativeUniqueCount);
                    }
                }
            }
        }

        public void Clear()
        {
            this.dataCount = 0;
            this.geoCount = 0;
            this.positiveCount = 0;
            this.negativeCount = 0;
            this.positiveUniqueCount = 0;
            this.negativeUniqueCount = 0;
            this.maxPositiveVisibleCount = 0;
            this.maxNegativeVisibleCount = 0;
            this.zeroCount = 0;
            this.nullCount = 0;
            this.maxShift = 0;
            this.maxRenderPriority = 0;
            this.timeEnabled = false;
            this.hasMultiInstanceData = false;
            this.hasEventData = false;
            this.baseReferencePosition = Vector3D.Empty;
            this.previousDataPosition = Vector3D.Empty;
            this.lastVisualTimeScale = -1f;
            this.firstPositionInstances.Clear();
            this.cachedPositions.Clear();
            this.filterItems.Clear();
            this.filterSubsets.Clear();
            this.filterItemsInSubsetsCount = 0;
            this.filterDirty = false;
            this.annotationItems.Clear();
            this.annotationSubsets.Clear();
            this.annotationItemsInSubsetsCount = 0;
            this.annotationDirty = false;
        }

        public void AddFilterInstance(int pos)
        {
            this.filterItems.Add(pos);
            this.filterDirty = true;
        }

        public void ClearFilterInstances()
        {
            this.filterItems.Clear();
            this.filterDirty = true;
        }

        public void AddAnnotationInstance(int pos)
        {
            this.annotationItems.Add(pos);
            this.annotationDirty = true;
        }

        public void ClearAnnotationInstances()
        {
            this.annotationItems.Clear();
            this.annotationDirty = true;
        }

        public unsafe InstanceId? GetAnnotationInstanceId(int annotationIndex, DateTime? time, SceneState state)
        {
            if (annotationIndex >= this.annotationItems.Count)
                return new InstanceId?();
            if (this.Count == 0)
                return new InstanceId?();
            int pos1 = this.annotationItems[annotationIndex];
            if (!time.HasValue || this.timeBuffer == null || this.timeBuffer.VertexCount == 0)
                return new InstanceId?(this.GetInstanceIdAt(pos1));
            if (!this.MinInstanceTime.HasValue)
                return new InstanceId?();
            int num1 = 0;
            int num2 = 0;
            for (int index = 0; index < this.firstPositionInstances.Count; ++index)
            {
                if (this.firstPositionInstances[index] > pos1)
                {
                    num2 = this.firstPositionInstances[index] - num1;
                    break;
                }
                else
                    num1 = this.firstPositionInstances[index];
            }
            if (num2 == 0)
                num2 = this.dataCount - num1;
            InstanceId? nullable1 = new InstanceId?();
            InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)this.dataBuffer.GetData().ToPointer();
            InstanceTime* instanceTimePtr = (InstanceTime*)this.timeBuffer.GetData().ToPointer();
            int num3 = (int)instanceBlockVertexPtr[pos1].Shift;
            for (int pos2 = num1; pos2 < num1 + num2; ++pos2)
            {
                if ((int)instanceBlockVertexPtr[pos2].Shift >= num3)
                {
                    if ((int)instanceBlockVertexPtr[pos2].Shift <= num3)
                    {
                        DateTime? nullable2 = (double)instanceTimePtr[pos2].StartTime == -1.0 ? new DateTime?() : new DateTime?(this.MinInstanceTime.Value.AddMilliseconds((double)instanceTimePtr[pos2].StartTime * this.TimeRange));
                        DateTime? nullable3 = (double)instanceTimePtr[pos2].EndTime == -1.0 ? new DateTime?() : new DateTime?(this.MinInstanceTime.Value.AddMilliseconds((double)instanceTimePtr[pos2].EndTime * this.TimeRange));
                        if (nullable3.HasValue)
                        {
                            double num4 = 0.25 * state.VisualTimeToRealtimeRatio * 1000.0 * 2.0;
                            nullable3 = new DateTime?(nullable3.Value.AddMilliseconds(num4));
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
                            nullable1 = new InstanceId?(this.GetInstanceIdAt(pos2));
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
            int num = Math.Max(instanceCount, 4096);
            if (this.dataBuffer == null)
            {
                this.dataBuffer = VertexBuffer.Create<InstanceBlockVertex>((InstanceBlockVertex[])null, num, false);
                this.positiveIndices = IndexBuffer.Create<uint>((uint[])null, num, false);
                this.positiveUniqueIndices = IndexBuffer.Create<uint>((uint[])null, num, false);
                this.zeroUniqueIndices = IndexBuffer.Create<uint>((uint[])null, num, false);
                this.nullUniqueIndices = IndexBuffer.Create<uint>((uint[])null, num, false);
            }
            if (needNegativeIndices && this.negativeIndices == null)
            {
                this.negativeIndices = IndexBuffer.Create<uint>((uint[])null, num, false);
                this.negativeUniqueIndices = IndexBuffer.Create<uint>((uint[])null, num, false);
            }
            if (this.timeBuffer != null && this.timeBuffer.VertexCount < num)
            {
                this.timeBuffer.Dispose();
                this.timeBuffer = (VertexBuffer)null;
            }
            if (needsTimeBuffer && this.timeBuffer == null)
                this.timeBuffer = VertexBuffer.Create<InstanceTime>((InstanceTime[])null, num, false);
            if (this.idBuffer != null)
                return;
            this.idBuffer = VertexBuffer.Create<InstanceColor>((InstanceColor[])null, num, false);
        }

        private int Update(Renderer renderer, bool ignoreInstanceValues, bool hitTest, bool useGatherAccumulate, bool useConstantMode)
        {
            int num = 0;
            if (useGatherAccumulate)
            {
                GatherAccumulateProcessBlock gatherAccumulateBlock = this.GetGatherAccumulateBlock();
                num = !hitTest ? this.gatherAccProcessor.Process(gatherAccumulateBlock, renderer, ignoreInstanceValues, useConstantMode) : this.gatherAccProcessor.ProcessHitTest(gatherAccumulateBlock, renderer, ignoreInstanceValues, useConstantMode);
            }
            if (!hitTest)
            {
                InstanceBlock.UpdateSubsets(this.filterItems, this.filterSubsets, ref this.filterItemsInSubsetsCount, ref this.filterDirty);
                InstanceBlock.UpdateSubsets(this.annotationItems, this.annotationSubsets, ref this.annotationItemsInSubsetsCount, ref this.annotationDirty);
            }
            return num;
        }

        public GatherAccumulateProcessBlock GetGatherAccumulateBlock()
        {
            return new GatherAccumulateProcessBlock()
            {
                PositiveIndices = this.positiveIndices,
                PositiveSubset = new Tuple<uint, uint>(0U, (uint)this.positiveCount),
                NegativeIndices = this.negativeIndices,
                NegativeSubset = this.negativeCount <= 0 || !this.ShowNegatives ? (Tuple<uint, uint>)null : new Tuple<uint, uint>(0U, (uint)this.negativeCount),
                Instances = this.dataBuffer,
                InstancesTime = this.timeBuffer,
                InstancesHitId = this.idBuffer,
                MaxShift = this.maxShift,
                Owner = this
            };
        }

        public int QueryInstanceCount(float visualTimeScale, bool showOnlyMaxValue, InstanceBlockQueryType queryType)
        {
            if ((double)this.lastVisualTimeScale != (double)visualTimeScale || this.lastShowOnlyMaxValue != showOnlyMaxValue)
            {
                this.ComputeMaxSimultaneousInstances((float)(1.0 / (double)visualTimeScale * 0.25));
                this.lastVisualTimeScale = visualTimeScale;
                this.lastShowOnlyMaxValue = showOnlyMaxValue;
            }
            int val2 = Math.Max(this.filterItems.Count, this.annotationItems.Count);
            int val1 = 0;
            switch (queryType)
            {
                case InstanceBlockQueryType.PositiveInstances:
                    val1 = showOnlyMaxValue ? this.positiveUniqueCount : this.maxPositiveVisibleCount;
                    break;
                case InstanceBlockQueryType.NegativeInstances:
                    val1 = showOnlyMaxValue ? this.negativeUniqueCount : this.maxNegativeVisibleCount;
                    break;
                case InstanceBlockQueryType.ZeroInstances:
                    val1 = this.zeroCount;
                    break;
                case InstanceBlockQueryType.NullInstances:
                    val1 = this.nullCount;
                    break;
            }
            return Math.Max(val1, val2);
        }

        public int QueryInstances(Renderer renderer, SceneState state, InstanceBlockQueryParameters parameters)
        {
            if (parameters == null)
                return 0;
            switch (parameters.QueryType)
            {
                case InstanceBlockQueryType.PositiveInstances:
                    if (this.maxPositiveVisibleCount > 0)
                        this.UpdateInstances(renderer, parameters);
                    switch (parameters.InstanceSource)
                    {
                        case InstanceBlockQueryInstanceSource.Filter:
                            return this.filterItemsInSubsetsCount;
                        case InstanceBlockQueryInstanceSource.Annotation:
                            return this.annotationItemsInSubsetsCount;
                        default:
                            if (parameters.ShowOnlyMaxValues)
                                return this.positiveUniqueCount;
                            else
                                return this.maxPositiveVisibleCount;
                    }
                case InstanceBlockQueryType.NegativeInstances:
                    if (this.maxNegativeVisibleCount > 0)
                        this.UpdateInstances(renderer, parameters);
                    switch (parameters.InstanceSource)
                    {
                        case InstanceBlockQueryInstanceSource.Filter:
                            return this.filterItemsInSubsetsCount;
                        case InstanceBlockQueryInstanceSource.Annotation:
                            return this.annotationItemsInSubsetsCount;
                        default:
                            if (parameters.ShowOnlyMaxValues)
                                return this.negativeUniqueCount;
                            else
                                return this.maxNegativeVisibleCount;
                    }
                case InstanceBlockQueryType.ZeroInstances:
                    if (this.zeroCount > 0)
                    {
                        this.processingTechnique.Mode = parameters.HitTest ? InstanceProcessingTechniqueMode.ZeroHitTest : InstanceProcessingTechniqueMode.Zero;
                        this.UpdateZeroNullInstances(renderer, parameters);
                    }
                    switch (parameters.InstanceSource)
                    {
                        case InstanceBlockQueryInstanceSource.Filter:
                            return Math.Min(this.filterItemsInSubsetsCount, this.zeroCount);
                        case InstanceBlockQueryInstanceSource.Annotation:
                            return Math.Max(this.annotationItemsInSubsetsCount, this.zeroCount);
                        default:
                            return this.zeroCount;
                    }
                case InstanceBlockQueryType.NullInstances:
                    if (this.nullCount > 0)
                    {
                        this.processingTechnique.Mode = parameters.HitTest ? InstanceProcessingTechniqueMode.NullHitTest : InstanceProcessingTechniqueMode.Null;
                        this.UpdateZeroNullInstances(renderer, parameters);
                    }
                    switch (parameters.InstanceSource)
                    {
                        case InstanceBlockQueryInstanceSource.Filter:
                            return Math.Min(this.filterItemsInSubsetsCount, this.nullCount);
                        case InstanceBlockQueryInstanceSource.Annotation:
                            return Math.Max(this.annotationItemsInSubsetsCount, this.nullCount);
                        default:
                            return this.nullCount;
                    }
                default:
                    return 0;
            }
        }

        private void UpdateInstances(Renderer renderer, InstanceBlockQueryParameters parameters)
        {
            bool flag = parameters.QueryType == InstanceBlockQueryType.NegativeInstances;
            if ((flag ? this.negativeUniqueCount : this.positiveUniqueCount) == 0 || parameters.InstanceSource == InstanceBlockQueryInstanceSource.Filter && this.filterItemsInSubsetsCount == 0 || parameters.InstanceSource == InstanceBlockQueryInstanceSource.Annotation && this.annotationItemsInSubsetsCount == 0)
                return;
            bool useGatherAccumulate = this.hasMultiInstanceData || parameters.HitTest || parameters.IsPieChart || parameters.UseLogScale;
            int num = this.Update(renderer, parameters.IgnoreInstanceValues, parameters.HitTest, useGatherAccumulate, parameters.IsClusterChart);
            renderer.RenderLockEnter();
            try
            {
                Renderer renderer1 = renderer;
                VertexBuffer[] vertexBuffers;
                if (!this.timeEnabled)
                    vertexBuffers = new VertexBuffer[1]
          {
            this.dataBuffer
          };
                else
                    vertexBuffers = new VertexBuffer[2]
          {
            this.dataBuffer,
            this.timeBuffer
          };
                renderer1.SetVertexSourceNoLock(vertexBuffers);
                renderer.SetIndexSourceNoLock(flag ? this.negativeUniqueIndices : this.positiveUniqueIndices);
                renderer.SetStreamBufferNoLock(parameters.InstanceOutputBuffer);
                renderer.SetVertexTextureNoLock(0, flag ? this.gatherAccProcessor.GatherNegativeTexture : this.gatherAccProcessor.GatherPositiveTexture);
                renderer.SetVertexTextureNoLock(1, flag ? this.gatherAccProcessor.AccumulateNegativeTexture : this.gatherAccProcessor.AccumulatePositiveTexture);
                renderer.SetVertexTextureNoLock(2, flag ? this.gatherAccProcessor.AccumulatePositiveTexture : this.gatherAccProcessor.AccumulateNegativeTexture);
                renderer.SetVertexTextureNoLock(3, flag ? (Texture)null : this.gatherAccProcessor.GatherNegativeTexture);
                renderer.SetVertexTextureNoLock(4, this.gatherAccProcessor.SelectPositiveTexture);
                if (parameters.HitTest)
                {
                    renderer.SetVertexTextureNoLock(5, flag ? this.gatherAccProcessor.GatherNegativeHitTestTexture : this.gatherAccProcessor.GatherPositiveHitTestTexture);
                    this.processingTechnique.Mode = InstanceProcessingTechniqueMode.HitTest;
                }
                else
                    this.processingTechnique.Mode = parameters.IsPieChart ? InstanceProcessingTechniqueMode.Pie : InstanceProcessingTechniqueMode.Default;
                this.processingTechnique.MaxShift = this.maxShift;
                this.processingTechnique.UseNegatives = this.ShowNegatives;
                this.processingTechnique.UseGatherAccumulate = this.hasMultiInstanceData || parameters.IsPieChart || parameters.UseLogScale;
                this.processingTechnique.ShiftOffset = num;
                this.processingTechnique.ValueOffset = parameters.Offset;
                this.processingTechnique.AnnotationPlacementEnabled = parameters.InstanceSource == InstanceBlockQueryInstanceSource.Annotation;
                this.processingTechnique.ShowOnlyMaxValueEnabled = parameters.ShowOnlyMaxValues;
                this.processingTechnique.NegativeValues = flag;
                this.processingTechnique.UseLogarithmicScale = parameters.UseLogScale;
                renderer.SetEffect((EffectTechnique)this.processingTechnique);
                if (parameters.InstanceSource != InstanceBlockQueryInstanceSource.Block)
                {
                    List<Tuple<int, int>> list = parameters.InstanceSource == InstanceBlockQueryInstanceSource.Filter ? this.filterSubsets : this.annotationSubsets;
                    for (int index = 0; index < list.Count; ++index)
                        renderer.DrawNoLock(list[index].Item1, list[index].Item2, PrimitiveTopology.PointList);
                }
                else
                    renderer.DrawIndexedNoLock(0, flag ? this.negativeUniqueCount : this.positiveUniqueCount, PrimitiveTopology.PointList);
                renderer.SetStreamBufferNoLock((StreamBuffer)null);
                renderer.SetVertexTextureNoLock(0, new Texture[6]);
            }
            finally
            {
                renderer.RenderLockExit();
            }
        }

        private void UpdateZeroNullInstances(Renderer renderer, InstanceBlockQueryParameters parameters)
        {
            int num = this.Update(renderer, parameters.IgnoreInstanceValues, parameters.HitTest, this.hasMultiInstanceData || parameters.HitTest || parameters.UseLogScale, parameters.IsClusterChart);
            if (parameters.InstanceSource == InstanceBlockQueryInstanceSource.Filter && this.filterItemsInSubsetsCount == 0 || parameters.InstanceSource == InstanceBlockQueryInstanceSource.Annotation && this.annotationItemsInSubsetsCount == 0)
                return;
            renderer.RenderLockEnter();
            try
            {
                Renderer renderer1 = renderer;
                VertexBuffer[] vertexBuffers;
                if (!this.timeEnabled)
                    vertexBuffers = new VertexBuffer[1]
          {
            this.dataBuffer
          };
                else
                    vertexBuffers = new VertexBuffer[2]
          {
            this.dataBuffer,
            this.timeBuffer
          };
                renderer1.SetVertexSourceNoLock(vertexBuffers);
                if (parameters.QueryType == InstanceBlockQueryType.ZeroInstances)
                    renderer.SetIndexSourceNoLock(this.zeroUniqueIndices);
                else
                    renderer.SetIndexSourceNoLock(this.nullUniqueIndices);
                renderer.SetStreamBufferNoLock(parameters.InstanceOutputBuffer);
                renderer.SetVertexTextureNoLock(0, this.gatherAccProcessor.AccumulatePositiveTexture);
                renderer.SetVertexTextureNoLock(1, this.gatherAccProcessor.AccumulateNegativeTexture ?? this.gatherAccProcessor.AccumulatePositiveTexture);
                renderer.SetVertexTextureNoLock(2, this.gatherAccProcessor.GatherPositiveTexture);
                renderer.SetVertexTextureNoLock(3, this.gatherAccProcessor.SelectPositiveTexture);
                if (parameters.HitTest)
                    renderer.SetVertexTextureNoLock(4, this.gatherAccProcessor.GatherPositiveHitTestTexture);
                this.processingTechnique.MaxShift = this.maxShift;
                this.processingTechnique.UseNegatives = this.ShowNegatives && this.negativeUniqueCount > 0;
                this.processingTechnique.UseGatherAccumulate = this.hasMultiInstanceData || parameters.IsPieChart || parameters.UseLogScale;
                this.processingTechnique.ShiftOffset = num;
                this.processingTechnique.ValueOffset = parameters.Offset;
                this.processingTechnique.AnnotationPlacementEnabled = parameters.InstanceSource == InstanceBlockQueryInstanceSource.Annotation;
                this.processingTechnique.ShowOnlyMaxValueEnabled = parameters.ShowOnlyMaxValues;
                this.processingTechnique.UseLogarithmicScale = parameters.UseLogScale;
                renderer.SetEffect((EffectTechnique)this.processingTechnique);
                if (parameters.InstanceSource != InstanceBlockQueryInstanceSource.Block)
                {
                    List<Tuple<int, int>> list = parameters.InstanceSource == InstanceBlockQueryInstanceSource.Filter ? this.filterSubsets : this.annotationSubsets;
                    for (int index = 0; index < list.Count; ++index)
                        renderer.DrawNoLock(list[index].Item1, list[index].Item2, PrimitiveTopology.PointList);
                }
                else
                    renderer.DrawIndexedNoLock(0, parameters.QueryType == InstanceBlockQueryType.ZeroInstances ? this.zeroCount : this.nullCount, PrimitiveTopology.PointList);
                renderer.SetStreamBufferNoLock((StreamBuffer)null);
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
            DisposableResource[] disposableResourceArray = new DisposableResource[9]
      {
        (DisposableResource) this.dataBuffer,
        (DisposableResource) this.timeBuffer,
        (DisposableResource) this.idBuffer,
        (DisposableResource) this.positiveIndices,
        (DisposableResource) this.negativeIndices,
        (DisposableResource) this.positiveUniqueIndices,
        (DisposableResource) this.negativeUniqueIndices,
        (DisposableResource) this.zeroUniqueIndices,
        (DisposableResource) this.nullUniqueIndices
      };
            foreach (DisposableResource disposableResource in disposableResourceArray)
            {
                if (disposableResource != null && !disposableResource.Disposed)
                    disposableResource.Dispose();
            }
        }
    }
}
