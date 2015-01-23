using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class SpatialIndex
    {
        private List<Node> nodes = new List<Node>();
        private InstanceLayer instanceLayer;
        private InstanceBlockData instanceBlockData;
        private int firstDataClusterToAdd;
        private int unmodifiedCachedBlockCount;
        private bool readyForTraversal;
        private bool computeBoundsAtConstruction;
        private IQuery query;

        internal bool Optimized { get; private set; }

        internal SpatialIndex(
            InstanceLayer layer,
            GatherAccumulateProcessor gatherAccumulateProcessor,
            InstanceProcessingTechnique processingTechnique,
            uint ID)
        {
            this.instanceLayer = layer;
            this.instanceBlockData = new InstanceBlockData(processingTechnique, gatherAccumulateProcessor, ID);
            this.Optimized = false;
            this.computeBoundsAtConstruction = false;
        }

        private void Reset()
        {
            this.firstDataClusterToAdd = 0;
            this.unmodifiedCachedBlockCount = 0;
            this.nodes.Clear();
            this.readyForTraversal = false;
        }

        internal List<InstanceBlock> Finalize(
            bool resetSpatialIndex, 
            Clusters dataClusters, 
            List<InstanceData> instances, 
            Dictionary<int, int> colorIndexOverrides, 
            List<InstanceBlock> blocks, 
            bool optimize,
            bool renderNegatives, 
            bool renderOnlyMaxValues, 
            bool planar, 
            double visualTimeRange, 
            DateTime? timeMin)
        {
            if (resetSpatialIndex || optimize)
                this.Reset();
            BuildInputs inputs = new BuildInputs()
            {
                InstanceList = instances,
                DataClusters = dataClusters,
                CachedBlocks = blocks,
                InstanceBlocks = new List<InstanceBlock>()
            };
            this.instanceBlockData.ColorOverrides = colorIndexOverrides;
            this.instanceBlockData.ShowNegatives = renderNegatives;
            this.instanceBlockData.ShowOnlyMaxValues = renderOnlyMaxValues;
            this.instanceBlockData.PlanarCoordinates = planar;
            this.instanceBlockData.TimeRange = visualTimeRange;
            this.instanceBlockData.MinTime = timeMin;
            this.Optimized = optimize;
            this.computeBoundsAtConstruction = this.Optimized && !this.instanceLayer.InflatesBounds;
            if (optimize)
            {
                this.nodes.Add(new Node());
                this.ConstructBranch(inputs, 0, 0, inputs.DataClusters.Count);
                this.UpdateBounds(inputs, 0);
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Index tree construction complete");
            }
            else
            {
                int unmodifiedCacheBlockCount = this.unmodifiedCachedBlockCount;
                for (int i = 0; i < unmodifiedCacheBlockCount; ++i)
                {
                    inputs.InstanceBlocks.Add(inputs.CachedBlocks[0]);
                    inputs.CachedBlocks.RemoveAt(0);
                }
                if (inputs.CachedBlocks.Count > 0 && !resetSpatialIndex)
                    this.nodes.RemoveAt(this.nodes.Count - 1);
                int firstClusterDataCount = this.firstDataClusterToAdd;
                int count = inputs.DataClusters.InstanceCount(0, firstClusterDataCount);
                int clusterDataCount = this.firstDataClusterToAdd;
                while (clusterDataCount < inputs.DataClusters.Count)
                {
                    int num4 = Math.Min(this.instanceLayer.BlockSize, inputs.DataClusters.Count - clusterDataCount);
                    int num5 = inputs.DataClusters.InstanceCount(firstClusterDataCount, firstClusterDataCount + num4);
                    this.AddALeaf(inputs, 
                        inputs.InstanceList.Skip<InstanceData>(count).Take<InstanceData>(num5),
                        num5);
                    clusterDataCount += num4;
                    count += num5;
                    if (num4 == this.instanceLayer.BlockSize)
                    {
                        firstClusterDataCount += this.instanceLayer.BlockSize;
                        ++unmodifiedCacheBlockCount;
                    }
                }
                this.firstDataClusterToAdd = firstClusterDataCount;
                this.unmodifiedCachedBlockCount = unmodifiedCacheBlockCount;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Trivial index construction complete");
            }
            this.instanceBlockData.Reset();
            this.readyForTraversal = true;
            return inputs.InstanceBlocks;
        }

        private void AddALeaf(
            BuildInputs inputs, 
            IEnumerable<InstanceData> instances, 
            int instanceCount)
        {
            Node node = new Node();
            this.AddBlock(inputs, instances, instanceCount, node);
            this.nodes.Add(node);
        }

        private void PopulateAndAddBlock(BuildInputs inputs, int firstCluster, int clusterCount, Node node)
        {
            BlockScaffold scaffold = this.instanceBlockData.Scaffold;
            scaffold.Populate(inputs, firstCluster, clusterCount);
            this.AddBlock(inputs, scaffold.InstanceBuffer, scaffold.InstanceCount, node);
            scaffold.Clear();
        }

        private void AddBlock(BuildInputs inputs, IEnumerable<InstanceData> instances, int instanceCount, Node node)
        {
            InstanceBlock instanceBlock = (InstanceBlock)null;
            for (int index = inputs.CachedBlocks.Count - 1; index >= 0; --index)
            {
                if (inputs.CachedBlocks[index].Capacity >= instanceCount)
                {
                    instanceBlock = inputs.CachedBlocks[index];
                    inputs.CachedBlocks.RemoveAt(index);
                    break;
                }
            }
            if (instanceBlock == null)
                instanceBlock = new InstanceBlock(this.instanceBlockData.GatherAccumulateProcessor, this.instanceBlockData.ProcessingTechnique, this.instanceBlockData.HitTestLayerId);
            instanceBlock.SetData(instances, instanceCount, this.instanceBlockData.ColorOverrides, ref node.Bounds3D, this.computeBoundsAtConstruction, this.instanceBlockData.TimeRange, this.instanceBlockData.MinTime, this.instanceBlockData.ShowOnlyMaxValues);
            instanceBlock.ShowNegatives = this.instanceBlockData.ShowNegatives;
            instanceBlock.PlanarCoordinates = this.instanceBlockData.PlanarCoordinates;
            inputs.InstanceBlocks.Add(instanceBlock);
            node.MaxClusterOrStackSize = instanceBlock.MaxClusterOrStackSize;
            node.BlockId = inputs.InstanceBlocks.Count - 1;
        }

        private void ConstructBranch(BuildInputs inputs, int nodeId, int firstCluster, int clusterCount)
        {
            int direction = 0;
            Node node = this.nodes[nodeId];
            if (this.computeBoundsAtConstruction)
            {
                node.LongLatBounds = inputs.DataClusters.GetBounds(firstCluster, firstCluster + clusterCount - 1);
                double[] numArray = new double[2]
                {
                    Math.Abs(node.LongLatBounds.maxX - node.LongLatBounds.minX),
                    Math.Abs(node.LongLatBounds.maxY - node.LongLatBounds.minY)
                };
                direction = numArray[1] > numArray[0] ? 1 : 0;
                node.LongLatBounds.minY = Coordinates.Mercator(node.LongLatBounds.minY);
                node.LongLatBounds.maxY = Coordinates.Mercator(node.LongLatBounds.maxY);
            }
            int num1 = clusterCount / this.instanceLayer.BlockSize;
            if (num1 * this.instanceLayer.BlockSize < clusterCount)
                ++num1;
            if (num1 < 2)
            {
                this.PopulateAndAddBlock(inputs, firstCluster, clusterCount, node);
            }
            else
            {
                int clusterCount1 = num1 / 2 * this.instanceLayer.BlockSize;
                int num2 = firstCluster + clusterCount1;
                inputs.DataClusters.PartitionByMedian(firstCluster, clusterCount, num2, direction);
                node.Left = this.AddNewNode();
                this.ConstructBranch(inputs, node.Left, firstCluster, clusterCount1);
                node.Right = this.AddNewNode();
                this.ConstructBranch(inputs, node.Right, num2, clusterCount - clusterCount1);
                node.MaxClusterOrStackSize = Math.Max(this.nodes[node.Left].MaxClusterOrStackSize, this.nodes[node.Right].MaxClusterOrStackSize);
            }
        }

        private int AddNewNode()
        {
            this.nodes.Add(new Node());
            return this.nodes.Count - 1;
        }

        private void UpdateBounds(BuildInputs inputs, ref Node node, int i)
        {
            this.UpdateBounds(inputs, i);
            node.UpdateBounds(this.nodes[i]);
        }

        private void UpdateBounds(BuildInputs inputs, int i)
        {
            Node node = this.nodes[i];
            if (node.IsALeaf)
            {
                this.instanceLayer.UpdateBounds(inputs.InstanceBlocks[node.BlockId], ref node.LongLatBounds, ref node.Bounds3D);
            }
            else
            {
                this.UpdateBounds(inputs, ref node, node.Left);
                this.UpdateBounds(inputs, ref node, node.Right);
            }
        }

        internal void Traverse(IQuery task)
        {
            if (!this.readyForTraversal || this.nodes.Count < 1)
                return;
            this.query = task;
            if (this.Optimized)
            {
                this.TraverseBranch(this.nodes[0]);
            }
            else
            {
                for (int index = 0; index < this.nodes.Count; ++index)
                    this.query.ProcessLeaf(this.nodes[index]);
            }
        }

        private void TraverseBranch(Node node)
        {
            if (!this.query.IsRelevant(node))
                return;
            if (node.IsALeaf)
            {
                this.query.ProcessLeaf(node);
            }
            else
            {
                this.TraverseBranch(this.nodes[node.Left]);
                this.TraverseBranch(this.nodes[node.Right]);
            }
        }

        private struct BuildInputs
        {
            internal Clusters DataClusters { get; set; }

            internal List<InstanceBlock> CachedBlocks { get; set; }

            internal List<InstanceBlock> InstanceBlocks { get; set; }

            internal List<InstanceData> InstanceList { get; set; }
        }

        private class InstanceBlockData
        {
            internal InstanceProcessingTechnique ProcessingTechnique { get; private set; }

            internal GatherAccumulateProcessor GatherAccumulateProcessor { get; private set; }

            internal uint HitTestLayerId { get; private set; }

            internal BlockScaffold Scaffold { get; private set; }

            internal bool ShowNegatives { get; set; }

            internal bool ShowOnlyMaxValues { get; set; }

            internal double TimeRange { get; set; }

            internal DateTime? MinTime { get; set; }

            internal Dictionary<int, int> ColorOverrides { get; set; }

            internal bool PlanarCoordinates { get; set; }

            internal InstanceBlockData(InstanceProcessingTechnique technique, GatherAccumulateProcessor processor, uint ID)
            {
                this.ProcessingTechnique = technique;
                this.GatherAccumulateProcessor = processor;
                this.HitTestLayerId = ID;
                this.Scaffold = new BlockScaffold();
            }

            internal void Reset()
            {
                this.ShowNegatives = false;
                this.ShowOnlyMaxValues = false;
                this.TimeRange = double.NaN;
                this.MinTime = new DateTime?();
                this.ColorOverrides = null;
                this.PlanarCoordinates = false;
                this.Scaffold.Clear();
            }
        }

        private struct ClusterReference
        {
            public int Index;
            public double Value;

            internal ClusterReference(int index, double value)
            {
                this.Index = index;
                this.Value = value;
            }
        }

        private class BlockScaffold
        {
            private List<ClusterReference> order;
            private List<InstanceData> instanceBuffer;

            internal List<InstanceData> InstanceBuffer
            {
                get
                {
                    return this.instanceBuffer;
                }
            }

            internal int InstanceCount
            {
                get
                {
                    return this.instanceBuffer.Count;
                }
            }

            internal BlockScaffold()
            {
                this.order = new List<ClusterReference>();
                this.instanceBuffer = new List<InstanceData>();
            }

            private static int Comparison(ClusterReference left, ClusterReference right)
            {
                if (double.IsNaN(left.Value))
                {
                    return double.IsNaN(right.Value) ? 0 : 1;
                }
                if (double.IsNaN(right.Value) || left.Value < right.Value)
                    return -1;
                return left.Value > right.Value ? 1 : 0;
            }

            /// <summary>
            /// 对inputs的InstanceList按照簇进排序，并将排序结果存储在instanceBuffer中
            /// </summary>
            /// <param name="inputs"></param>
            /// <param name="firstCluster"></param>
            /// <param name="count"></param>
            internal void Populate(BuildInputs inputs, int firstCluster, int count)
            {
                this.instanceBuffer.Clear();
                List<InstanceData> instanceList = inputs.InstanceList;
                Clusters dataClusters = inputs.DataClusters;
                int clusterNum = firstCluster + count - 1;
                for (int i = firstCluster; i <= clusterNum; ++i)
                {
                    double sum = 0.0;
                    int dataNum = dataClusters[i].First + dataClusters[i].Count - 1;
                    for (int j = dataClusters[i].First; j <= dataNum; ++j)
                        sum += Math.Abs(instanceList[j].Value);
                    this.order.Add(new ClusterReference(i, sum));
                }
                this.order.Sort(BlockScaffold.Comparison);
                for (int i = 0; i < this.order.Count; ++i)
                {
                    int index = this.order[i].Index;
                    int first = dataClusters[index].First;
                    int num = first + dataClusters[index].Count - 1;
                    for (int j = first; j <= num; ++j)
                        this.instanceBuffer.Add(instanceList[j]);
                }
                this.order.Clear();
            }

            internal void Clear()
            {
                this.instanceBuffer.Clear();
                this.order.Clear();
            }
        }

        internal class Node
        {
            internal Box2D LongLatBounds;
            internal Box3D Bounds3D;

            internal int Left { get; set; }

            internal int Right { get; set; }

            internal int BlockId
            {
                get
                {
                    return this.Left;
                }
                set
                {
                    this.Left = value;
                    this.Right = -1;
                }
            }

            internal bool IsALeaf
            {
                get
                {
                    return this.Right < 0;
                }
            }

            internal ushort MaxClusterOrStackSize { get; set; }

            internal Node()
            {
                this.LongLatBounds.Initialize();
                this.Bounds3D.Initialize();
            }

            internal void UpdateBounds(Node other)
            {
                this.LongLatBounds.UpdateWith(other.LongLatBounds);
                this.Bounds3D.UpdateWith(other.Bounds3D);
            }
        }
    }
}
