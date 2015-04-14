using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
    internal class PolygonalCone : InstancedVisual
    {
        protected short sideVertices;
        private short baseCenterVertex;

        internal override bool MayHaveNegativeInstances
        {
            get
            {
                return false;
            }
        }

        public PolygonalCone(int walls, bool isDerived = false)
            : base(walls)
        {
            this.AngleIncrement = 2.0 * Math.PI / (double)walls;
            this.StartAngle = this.AngleIncrement / 2.0 - Math.PI / 2.0;
            this.shadowVolume = (InstancedVisual)new PolygonalColumnVisual(walls, false);
            if (isDerived)
                return;
            this.CreateMesh();
        }

        internal override void CreateInstanceVertices()
        {
            Vector2D[] vector2DArray = InstancedVisual.Create2DRegularPolygon(this.StartAngle, 2 * this.WallCount);
            Vector3D[] vector3DArray1 = new Vector3D[this.WallCount];
            Vector3D[] vector3DArray2 = new Vector3D[this.WallCount];
            for (int index1 = 0; index1 < this.WallCount; ++index1)
            {
                int index2 = 2 * index1;
                vector3DArray1[index1] = InstancedVisual.To3D(vector2DArray[index2], 1.0);
                double d = Math.PI / (double)this.WallCount;
                double num1 = Math.Cos(d);
                double num2 = 1.0 / Math.Sqrt(1.0 + num1 * num1);
                vector3DArray2[index1] = InstancedVisual.To3D(vector2DArray[index2 + 1], -Math.Cos(d));
                vector3DArray2[index1].Normalize();
            }
            this.MeshVertices = new InstanceVertex[4 * this.WallCount + 1];
            short num3;
            for (num3 = (short)0; (int)num3 < this.WallCount; ++num3)
                this.MeshVertices[(int)num3] = new InstanceVertex(vector3DArray1[(int)num3], Vector3D.ZVector);
            this.sideVertices = num3;
            for (int index1 = 0; index1 < this.WallCount; ++index1)
            {
                InstanceVertex[] meshVertices1 = this.MeshVertices;
                int index2 = (int)num3;
                int num1 = 1;
                short num2 = (short)(index2 + num1);
                meshVertices1[index2] = new InstanceVertex(Vector3D.Empty, vector3DArray2[index1]);
                InstanceVertex[] meshVertices2 = this.MeshVertices;
                int index3 = (int)num2;
                int num4 = 1;
                short num5 = (short)(index3 + num4);
                meshVertices2[index3] = new InstanceVertex(vector3DArray1[this.Mod(index1 + 1)], vector3DArray2[index1]);
                InstanceVertex[] meshVertices3 = this.MeshVertices;
                int index4 = (int)num5;
                int num6 = 1;
                num3 = (short)(index4 + num6);
                meshVertices3[index4] = new InstanceVertex(vector3DArray1[index1], vector3DArray2[index1]);
            }
            this.baseCenterVertex = num3;
            this.MeshVertices[(int)num3] = new InstanceVertex(Vector3D.ZVector, Vector3D.ZVector);
        }

        private short BaseVertex(int i)
        {
            return (short)this.Mod(i);
        }

        private short SideFacetVertex(int facet, int vertex)
        {
            return (short)((int)this.sideVertices + 3 * this.Mod(facet) + vertex);
        }

        protected override void SetTriangles()
        {
            if (this.WallCount <= 3)
                return;
            int num = 2 * this.WallCount - 2;
            this.MeshIndices = new short[3 * num];
            this.MeshIndicesWithAdjacency = new short[6 * num];
            int raw = 0;
            int withAdjacency = 0;
            this.FirstCeilingIndex = raw;
            this.SetTriangleIndexes(this.BaseVertex(0), this.BaseVertex(1), this.BaseVertex(2), this.SideFacetVertex(0, 0), this.SideFacetVertex(1, 0), this.baseCenterVertex, ref raw, ref withAdjacency);
            this.SetTriangleIndexes(this.BaseVertex(0), this.BaseVertex(this.WallCount - 2), this.BaseVertex(this.WallCount - 1), this.baseCenterVertex, this.SideFacetVertex(this.WallCount - 2, 0), this.SideFacetVertex(this.WallCount - 1, 0), ref raw, ref withAdjacency);
            this.CeilingIndexCount = raw - this.FirstCeilingIndex;
            this.FirstWallIndex = raw;
            for (int index = 2; index < this.WallCount - 2; ++index)
                this.SetTriangleIndexes(this.BaseVertex(0), this.BaseVertex(index), this.BaseVertex(index + 1), this.baseCenterVertex, this.SideFacetVertex(index, 0), this.baseCenterVertex, ref raw, ref withAdjacency);
            for (int facet = 0; facet < this.WallCount; ++facet)
                this.SetTriangleIndexes(this.SideFacetVertex(facet, 0), this.SideFacetVertex(facet, 1), this.SideFacetVertex(facet, 2), this.SideFacetVertex(facet + 1, 1), this.baseCenterVertex, this.SideFacetVertex(facet - 1, 2), ref raw, ref withAdjacency);
            this.WallIndexCount = raw - this.FirstWallIndex;
        }
    }
}
