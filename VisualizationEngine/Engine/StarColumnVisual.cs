using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
    internal class StarColumnVisual : ColumnVisual
    {
        protected static double[] innerRadius = new double[5]
    {
      0.35,
      0.38197,
      0.57735,
      1.0,
      0.65
    };
        protected int tipCount;

        protected double InnerRadius
        {
            get
            {
                if (this.tipCount > 8)
                    return 0.85;
                else
                    return StarColumnVisual.innerRadius[this.tipCount - 4];
            }
        }

        public StarColumnVisual(int tips)
            : base(tips * 2)
        {
            this.tipCount = tips;
            this.WallCount = 2 * tips;
            this.AngleIncrement = 2.0 * Math.PI / (double)tips;
            this.StartAngle = this.AngleIncrement / 2.0 - Math.PI / 2.0;
            this.outline = (InstancedVisual)new PolygonalOutlineColumnVisual(this.tipCount);
            this.CreateMesh();
        }

        protected short GetFloorTip(int i)
        {
            return (short)(this.FloorVertices + this.Mod(2 * i));
        }

        protected short GetFloorNotch(int i)
        {
            return (short)(this.FloorVertices + this.Mod(2 * i - 1));
        }

        internal override void CreateInstanceVertices()
        {
            this.MeshVertices = new InstanceVertex[6 * this.WallCount + 1];
            int index1 = 0;
            Vector3D vector3D = new Vector3D(0.0, 0.0, 1.0);
            Vector2D[] vector2DArray = InstancedVisual.Create2DRegularPolygon(this.StartAngle, this.WallCount);
            double innerRadius = this.InnerRadius;
            for (int index2 = 0; index2 < this.tipCount; ++index2)
                vector2DArray[2 * index2 + 1].MultiplyBy(innerRadius);
            Vector3D[] vector3DArray1 = new Vector3D[this.WallCount];
            Vector3D[] vector3DArray2 = new Vector3D[this.WallCount];
            for (int index2 = 0; index2 < this.WallCount; ++index2)
            {
                vector3DArray2[index2] = InstancedVisual.To3D(vector2DArray[index2], 0.0);
                vector3DArray1[index2] = InstancedVisual.To3D(vector2DArray[index2], 1.0);
            }
            for (int index2 = 0; index2 < this.WallCount; ++index2)
                this.MeshVertices[index1++] = new InstanceVertex(vector3DArray1[index2], vector3D);
            this.FloorVertices = index1;
            for (int index2 = 0; index2 < this.WallCount; ++index2)
                this.MeshVertices[index1++] = new InstanceVertex(vector3DArray2[index2], -vector3D);
            Vector3D[] vector3DArray3 = new Vector3D[this.WallCount];
            for (int index2 = 0; index2 < this.WallCount; ++index2)
            {
                Vector2D vector = vector2DArray[this.Mod(index2 + 1)];
                InstancedVisual.To3D((vector2DArray[index2] + vector) / 2.0, 0.5);
                vector -= vector2DArray[index2];
                vector.TurnRight();
                vector.Normalize();
                vector3DArray3[index2] = InstancedVisual.To3D(vector, 0.0);
            }
            this.WallTop = index1;
            for (int index2 = 0; index2 < this.WallCount; ++index2)
            {
                InstanceVertex[] meshVertices1 = this.MeshVertices;
                int index3 = index1;
                int num1 = 1;
                int num2 = index3 + num1;
                meshVertices1[index3] = new InstanceVertex(vector3DArray1[index2], vector3DArray3[index2]);
                InstanceVertex[] meshVertices2 = this.MeshVertices;
                int index4 = num2;
                int num3 = 1;
                index1 = index4 + num3;
                meshVertices2[index4] = new InstanceVertex(vector3DArray1[this.Mod(index2 + 1)], vector3DArray3[index2]);
            }
            this.WallBottom = index1;
            for (int index2 = 0; index2 < this.WallCount; ++index2)
            {
                InstanceVertex[] meshVertices1 = this.MeshVertices;
                int index3 = index1;
                int num1 = 1;
                int num2 = index3 + num1;
                meshVertices1[index3] = new InstanceVertex(vector3DArray2[index2], vector3DArray3[index2]);
                InstanceVertex[] meshVertices2 = this.MeshVertices;
                int index4 = num2;
                int num3 = 1;
                index1 = index4 + num3;
                meshVertices2[index4] = new InstanceVertex(vector3DArray2[this.Mod(index2 + 1)], vector3DArray3[index2]);
            }
            this.CeilingCenter = (short)index1;
            this.MeshVertices[index1] = new InstanceVertex(vector3D, vector3D, 0.0f);
        }

        protected override void SetTriangles()
        {
            this.MeshIndices = new short[3 * (2 * this.tipCount - 2 + 3 * this.WallCount)];
            int num = this.SetRawCeilingIndexs(0);
            this.FirstWallIndex = num;
            for (int index = 0; index < this.tipCount; ++index)
            {
                int currentIndex = this.SetRawWallIndexes(2 * index, num);
                num = this.SetRawWallIndexes(2 * index + 1, currentIndex);
            }
            this.WallIndexCount = num - this.FirstWallIndex;
            int current = this.SetRawTriangleIndexes(this.GetFloorNotch(2), this.GetFloorNotch(1), this.GetFloorNotch(0), this.SetRawTriangleIndexes(this.GetFloorNotch(0), this.GetFloorNotch(this.tipCount - 1), this.GetFloorNotch(this.tipCount - 2), num));
            for (int i = 2; i < this.tipCount - 2; ++i)
                current = this.SetRawTriangleIndexes(this.GetFloorNotch(0), this.GetFloorNotch(i + 1), this.GetFloorNotch(i), current);
            for (int i = 0; i < this.tipCount; ++i)
                current = this.SetRawTriangleIndexes(this.GetFloorNotch(i + 1), this.GetFloorTip(i), this.GetFloorNotch(i), current);
        }
    }
}
