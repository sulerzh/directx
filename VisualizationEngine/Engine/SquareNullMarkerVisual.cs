using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
    internal class SquareNullMarkerVisual : NullMarkerVisual
    {
        public SquareNullMarkerVisual()
            : base(6)
        {
            outline = new PolygonalColumnVisual(4, false);
            CreateMesh();
        }

        internal override void CreateInstanceVertices()
        {
            double num1 = Constants.SqrtOf2 / 2.0;
            double num2 = num1 * (1.0 - thickness);
            MeshVertices = new InstanceVertex[120];
            Vector2D[] vector2DArray = new Vector2D[6]
            {
                new Vector2D(num1, halfGap),
                new Vector2D(num1, num1),
                new Vector2D(halfGap, num1),
                new Vector2D(halfGap, num2),
                new Vector2D(num2, num2),
                new Vector2D(num2, halfGap)
            };
            Vector3D[] vector3DArray1 = new Vector3D[WallCount];
            for (int i = 0; i < WallCount; ++i)
            {
                vector3DArray1[i] = To3D(vector2DArray[i], 1.0);
            }
            Vector3D[] vector3DArray2 = new Vector3D[WallCount];
            for (int i = 0; i < WallCount; ++i)
            {
                vector3DArray2[i] = To3D(vector2DArray[i], 0.0);
            }
            Vector3D[] vector3DArray3 = new Vector3D[6]
            {
                Vector3D.XVector,
                Vector3D.YVector,
                -Vector3D.XVector,
                -Vector3D.YVector,
                -Vector3D.XVector,
                -Vector3D.YVector
            };
            short num3 = 0;
            CeilingVertices = num3;
            for (int i = 0; i < WallCount; ++i)
            {
                MeshVertices[num3++] = new InstanceVertex(vector3DArray1[i], Vector3D.ZVector);
            }
            WallTop = num3;
            for (int i = 0; i < WallCount; ++i)
            {
                InstanceVertex[] meshVertices1 = MeshVertices;
                int index2 = num3;
                int num4 = 1;
                short num5 = (short)(index2 + num4);
                meshVertices1[index2] = new InstanceVertex(vector3DArray1[i], vector3DArray3[i]);
                InstanceVertex[] meshVertices2 = MeshVertices;
                int index3 = num5;
                int num6 = 1;
                num3 = (short)(index3 + num6);
                meshVertices2[index3] = new InstanceVertex(vector3DArray1[(i + 1) % WallCount], vector3DArray3[i]);
            }
            WallBottom = num3;
            for (int i = 0; i < WallCount; ++i)
            {
                InstanceVertex[] meshVertices1 = MeshVertices;
                int index2 = num3;
                int num4 = 1;
                short num5 = (short)(index2 + num4);
                meshVertices1[index2] = new InstanceVertex(vector3DArray2[i], vector3DArray3[i]);
                InstanceVertex[] meshVertices2 = MeshVertices;
                int index3 = num5;
                int num6 = 1;
                num3 = (short)(index3 + num6);
                meshVertices2[index3] = new InstanceVertex(vector3DArray2[(i + 1) % WallCount], vector3DArray3[i]);
            }
            ReplicateVertices();
        }

        protected override void SetTriangles()
        {
            MeshIndices = new short[192];
            FirstCeilingIndex = 0;
            int currentIndex = SetQuadIndexes(GetVertex(CeilingVertices, 1), GetVertex(CeilingVertices, 2), GetVertex(CeilingVertices, 3), GetVertex(CeilingVertices, 4), SetQuadIndexes(GetVertex(CeilingVertices, 0), GetVertex(CeilingVertices, 1), GetVertex(CeilingVertices, 4), GetVertex(CeilingVertices, 5), 0));
            ReplicateIndices(currentIndex, ref currentIndex);
            CeilingIndexCount = currentIndex - FirstCeilingIndex;
            FirstWallIndex = currentIndex;
            for (int wall = 0; wall < WallCount; ++wall)
            {
                currentIndex = SetRawWallIndexes(wall, currentIndex);
            }
            ReplicateIndices(currentIndex - FirstWallIndex, ref currentIndex);
            WallIndexCount = currentIndex - FirstWallIndex;
        }
    }
}
