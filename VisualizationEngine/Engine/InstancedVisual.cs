using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
    internal abstract class InstancedVisual : DisposableResource
    {
        protected InstancedVisual outline;
        protected InstancedVisual shadowVolume;
        protected short[] MeshIndicesWithAdjacency;

        protected InstanceVertex[] MeshVertices { get; set; }

        protected short[] MeshIndices { get; set; }

        public VertexBuffer MeshVertexBuffer { get; private set; }

        public VertexBuffer OutlineMeshVertexBuffer
        {
            get
            {
                return this.outline.MeshVertexBuffer;
            }
        }

        public VertexBuffer ShadowMeshVertexBuffer
        {
            get
            {
                return this.shadowVolume.MeshVertexBuffer;
            }
        }

        public IndexBuffer MeshIndexBuffer { get; private set; }

        public IndexBuffer OutlineMeshIndexBuffer
        {
            get
            {
                return this.outline.MeshIndexBufferWithAdjacency;
            }
        }

        public IndexBuffer ShadowMeshIndexBuffer
        {
            get
            {
                return this.shadowVolume.MeshIndexBuffer;
            }
        }

        private IndexBuffer MeshIndexBufferWithAdjacency { get; set; }

        protected double StartAngle { get; set; }

        protected double AngleIncrement { get; set; }

        protected int WallCount { get; set; }

        internal int FirstWallIndex { get; set; }

        internal int WallIndexCount { get; set; }

        internal int FirstCeilingIndex { get; set; }

        internal int CeilingIndexCount { get; set; }

        internal virtual bool IsPieSegment
        {
            get
            {
                return false;
            }
        }

        internal virtual bool IsNullMarker
        {
            get
            {
                return false;
            }
        }

        internal virtual bool IsZeroMarker
        {
            get
            {
                return false;
            }
        }

        internal virtual bool MayHaveNegativeInstances
        {
            get
            {
                return true;
            }
        }

        internal virtual float HorizontalSpacing
        {
            get
            {
                return (float)(2.0 * Math.Cos(this.StartAngle + Math.Round(-this.StartAngle / this.AngleIncrement) * this.AngleIncrement));
            }
        }

        protected InstancedVisual(int walls)
        {
            this.WallCount = walls;
            this.outline = this;
            this.shadowVolume = this;
        }

        protected void CreateMesh()
        {
            this.CreateInstanceVertices();
            this.SetTriangles();
            this.CreateBuffers();
        }

        private void CreateBuffers()
        {
            OffsetVertex[] data = new OffsetVertex[this.MeshVertices.Length];
            for (int index = 0; index < this.MeshVertices.Length; ++index)
            {
                data[index].X = (short)(this.MeshVertices[index].Position.X * short.MaxValue);
                data[index].Y = (short)(this.MeshVertices[index].Position.Y * short.MaxValue);
                data[index].Z = (short)(this.MeshVertices[index].Position.Z * short.MaxValue);
                data[index].R = (short)(this.MeshVertices[index].Texture * (double)short.MaxValue);
                data[index].NX = (short)(this.MeshVertices[index].Normal.X * short.MaxValue);
                data[index].NY = (short)(this.MeshVertices[index].Normal.Y * short.MaxValue);
                data[index].NZ = (short)(this.MeshVertices[index].Normal.Z * short.MaxValue);
                data[index].Unused = 0;
            }
            this.MeshVertexBuffer = VertexBuffer.Create(data, false);
            this.MeshIndexBuffer = IndexBuffer.Create(this.MeshIndices, false);
            if (this.MeshIndicesWithAdjacency == null)
                return;
            this.MeshIndexBufferWithAdjacency = IndexBuffer.Create(this.MeshIndicesWithAdjacency, true);
        }

        internal static InstancedVisual Create(InstancedShape shape)
        {
            switch (shape)
            {
                case InstancedShape.InvertedPyramid:
                    return new PolygonalCone(4, false);
                case InstancedShape.CircularCone:
                    return new CircularCone();
                case InstancedShape.Triangle:
                    return new PolygonalColumnVisual(3, false);
                case InstancedShape.Square:
                    return new PolygonalColumnVisual(4, false);
                case InstancedShape.Pentagon:
                    return new PolygonalColumnVisual(5, false);
                case InstancedShape.Hexagon:
                    return new PolygonalColumnVisual(6, false);
                case InstancedShape.Heptagon:
                    return new PolygonalColumnVisual(7, false);
                case InstancedShape.Octagon:
                    return new PolygonalColumnVisual(8, false);
                case InstancedShape.Circle:
                    return new CylinderVisual();
                case InstancedShape.Star4:
                    return new StarColumnVisual(4);
                case InstancedShape.Star5:
                    return new StarColumnVisual(5);
                case InstancedShape.Star8:
                    return new StarColumnVisual(8);
                case InstancedShape.Star12:
                    return new StarColumnVisual(12);
                case InstancedShape.Star16:
                    return new StarColumnVisual(16);
                case InstancedShape.Star24:
                    return new StarColumnVisual(24);
                case InstancedShape.Cross:
                    return new CrossColumnVisual();
                case InstancedShape.SquareNullMarker:
                    return new SquareNullMarkerVisual();
                case InstancedShape.RoundNullMarker:
                    return new RoundNullMarkerVisual();
                default:
                    return null;
            }
        }

        protected static Vector2D[] Create2DRegularPolygon(double startAngle, int count)
        {
            Vector2D[] vector2DArray = new Vector2D[count];
            double num1 = 2.0 * Math.PI / count;
            double num2 = Math.Cos(num1);
            double num3 = Math.Sin(num1);
            vector2DArray[0].X = Math.Cos(startAngle);
            vector2DArray[0].Y = Math.Sin(startAngle);
            for (int index = 1; index < count; ++index)
            {
                vector2DArray[index].X = vector2DArray[index - 1].X * num2 - vector2DArray[index - 1].Y * num3;
                vector2DArray[index].Y = vector2DArray[index - 1].X * num3 + vector2DArray[index - 1].Y * num2;
            }
            return vector2DArray;
        }

        protected static Vector3D To3D(Vector2D vector, double thirdCoordinate)
        {
            return new Vector3D(vector.X, vector.Y, thirdCoordinate);
        }

        protected int Mod(int index)
        {
            return (index + this.WallCount) % this.WallCount;
        }

        protected virtual void SetTriangleIndexes(short vertex1, short vertex2, short vertex3, short adjacent1, short adjacent2, short adjacent3, ref int raw, ref int withAdjacency)
        {
            this.MeshIndices[raw++] = this.MeshIndicesWithAdjacency[withAdjacency++] = vertex1;
            this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent1;
            this.MeshIndices[raw++] = this.MeshIndicesWithAdjacency[withAdjacency++] = vertex2;
            this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent2;
            this.MeshIndices[raw++] = this.MeshIndicesWithAdjacency[withAdjacency++] = vertex3;
            this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent3;
        }

        protected int SetRawTriangleIndexes(short v1, short v2, short v3, int current)
        {
            int index = current;
            this.MeshIndices[index++] = v1;
            this.MeshIndices[index++] = v2;
            this.MeshIndices[index++] = v3;
            return index;
        }

        protected int SetQuadIndexes(short v1, short v2, short v3, short v4, int current)
        {
            current = this.SetRawTriangleIndexes(v1, v2, v3, current);
            return this.SetRawTriangleIndexes(v1, v3, v4, current);
        }

        internal virtual void OverrideDimensionAndScales(ref float fixedDimension, ref Vector2F fixedScale, ref Vector2F variableScale)
        {
        }

        internal abstract void CreateInstanceVertices();

        protected abstract void SetTriangles();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            if (this.MeshVertexBuffer != null)
                this.MeshVertexBuffer.Dispose();
            if (this.MeshIndexBuffer != null)
                this.MeshIndexBuffer.Dispose();
            if (this.MeshIndexBufferWithAdjacency != null)
                this.MeshIndexBufferWithAdjacency.Dispose();
            if (this.outline != null && this.outline != this && !this.outline.Disposed)
                this.outline.Dispose();
            if (this.shadowVolume == null || this.shadowVolume == this || this.shadowVolume.Disposed)
                return;
            this.shadowVolume.Dispose();
        }

        internal enum Alignment
        {
            None,
            Horizontal,
            Vertical,
            Angular,
        }
    }
}
