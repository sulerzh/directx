using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
    internal class RoundNullMarkerVisual : NullMarkerVisual
    {
        private int innerPointCount;
        private int outerPointCount;
        private int outerCeiling;
        private int innerCeiling;
        private int horizontalWallTop;
        private int outerWallTop;
        private int verticalWallTop;
        private int innerWallTop;
        private int horizontalWallBottom;
        private int outerWallBottom;
        private int verticalWallBottom;
        private int innerWallBottom;
        private double radius;
        private double innerAngleGap;
        private double innerArcAngle;
        private double outerAngleGap;
        private double outerArcAngle;

        public RoundNullMarkerVisual()
            : base(0)
        {
            outline = new PolygonalColumnVisual(32, false);
            double num = Math.PI / 2.0;
            radius = 1.0 - thickness;
            innerAngleGap = Math.Asin(halfGap / radius);
            innerArcAngle = num - 2.0 * innerAngleGap;
            innerPointCount = (int)(innerArcAngle * 8.0 / num) + 2;
            outerAngleGap = Math.Asin(halfGap);
            outerArcAngle = num - 2.0 * outerAngleGap;
            outerPointCount = (int)(outerArcAngle * 8.0 / num) + 2;
            if ((outerPointCount - innerPointCount) % 2 != 0)
                ++outerPointCount;
            WallCount = innerPointCount + outerPointCount;
            CreateMesh();
        }

        private static Vector2D[] SubdivideArc(double radius, double startAngle, double extent, int pointCount)
        {
            Vector2D[] vector2DArray = new Vector2D[pointCount];
            int num1 = pointCount - 1;
            double num2 = extent / num1;
            double num3 = Math.Cos(num2);
            double num4 = Math.Sin(num2);
            vector2DArray[0].X = radius * Math.Cos(startAngle);
            vector2DArray[0].Y = radius * Math.Sin(startAngle);
            for (int index = 1; index < pointCount; ++index)
            {
                vector2DArray[index].X = vector2DArray[index - 1].X * num3 - vector2DArray[index - 1].Y * num4;
                vector2DArray[index].Y = vector2DArray[index - 1].X * num4 + vector2DArray[index - 1].Y * num3;
            }
            return vector2DArray;
        }

        private static Vector3D[] GetVertexPositions(Vector2D[] points, double z)
        {
            Vector3D[] vector3DArray = new Vector3D[points.Length];
            for (int index = 0; index < points.Length; ++index)
                vector3DArray[index] = To3D(points[index], z);
            return vector3DArray;
        }

        internal override void CreateInstanceVertices()
        {
            Vector2D[] points1 = SubdivideArc(1.0, outerAngleGap, outerArcAngle, outerPointCount);
            Vector2D[] points2 = SubdivideArc(radius, innerAngleGap, innerArcAngle, innerPointCount);
            Vector3D[] vertexPositions1 = GetVertexPositions(points1, 1.0);
            Vector3D[] vertexPositions2 = GetVertexPositions(points2, 1.0);
            Vector3D[] vertexPositions3 = GetVertexPositions(points1, 0.0);
            Vector3D[] vertexPositions4 = GetVertexPositions(points2, 0.0);
            MeshVertices = new InstanceVertex[WallCount * 12 + 32];
            short num1 = 0;
            outerCeiling = num1;
            for (int i = 0; i < outerPointCount; ++i)
            {
                MeshVertices[num1++] = new InstanceVertex(vertexPositions1[i], Vector3D.ZVector);
            }
            innerCeiling = num1;
            for (int i = 0; i < innerPointCount; ++i)
            {
                MeshVertices[num1++] = new InstanceVertex(vertexPositions2[i], Vector3D.ZVector);
            }
            ReplicateVertices();
            horizontalWallTop = num1;
            InstanceVertex[] meshVertices1 = MeshVertices;
            int index1 = num1;
            int num2 = 1;
            short num3 = (short)(index1 + num2);
            meshVertices1[index1] = new InstanceVertex(vertexPositions2[0], -Vector3D.YVector);
            InstanceVertex[] meshVertices2 = MeshVertices;
            int index2 = num3;
            int num4 = 1;
            short num5 = (short)(index2 + num4);
            meshVertices2[index2] = new InstanceVertex(vertexPositions1[0], -Vector3D.YVector);
            outerWallTop = num5;
            for (int i = 0; i < outerPointCount; ++i)
            {
                MeshVertices[num5++] = new InstanceVertex(vertexPositions1[i], vertexPositions3[i]);
            }
            verticalWallTop = num5;
            InstanceVertex[] meshVertices3 = MeshVertices;
            int index4 = num5;
            int num6 = 1;
            short num7 = (short)(index4 + num6);
            meshVertices3[index4] = new InstanceVertex(vertexPositions1[outerPointCount - 1], -Vector3D.XVector);
            InstanceVertex[] meshVertices4 = MeshVertices;
            int index5 = num7;
            int num8 = 1;
            short num9 = (short)(index5 + num8);
            meshVertices4[index5] = new InstanceVertex(vertexPositions2[innerPointCount - 1], -Vector3D.XVector);
            innerWallTop = num9;
            for (int i = innerPointCount - 1; i >= 0; --i)
            {
                MeshVertices[num9++] = new InstanceVertex(vertexPositions2[i], -vertexPositions2[i]);
            }
            horizontalWallBottom = num9;
            InstanceVertex[] meshVertices5 = MeshVertices;
            int index6 = num9;
            int num10 = 1;
            short num11 = (short)(index6 + num10);
            meshVertices5[index6] = new InstanceVertex(vertexPositions4[0], -Vector3D.YVector);
            InstanceVertex[] meshVertices6 = MeshVertices;
            int index7 = num11;
            int num12 = 1;
            short num13 = (short)(index7 + num12);
            meshVertices6[index7] = new InstanceVertex(vertexPositions3[0], -Vector3D.YVector);
            outerWallBottom = num13;
            for (int i = 0; i < outerPointCount; ++i)
            {
                MeshVertices[num13++] = new InstanceVertex(vertexPositions3[i], vertexPositions3[i]);
            }
            verticalWallBottom = num13;
            InstanceVertex[] meshVertices7 = MeshVertices;
            int index8 = num13;
            int num14 = 1;
            short num15 = (short)(index8 + num14);
            meshVertices7[index8] = new InstanceVertex(vertexPositions3[outerPointCount - 1], -Vector3D.XVector);
            InstanceVertex[] meshVertices8 = MeshVertices;
            int index9 = num15;
            int num16 = 1;
            short num17 = (short)(index9 + num16);
            meshVertices8[index9] = new InstanceVertex(vertexPositions4[innerPointCount - 1], -Vector3D.XVector);
            innerWallBottom = num17;
            for (int i = innerPointCount - 1; i >= 0; --i)
            {
                MeshVertices[num17++] = new InstanceVertex(vertexPositions4[i], -vertexPositions4[i]);
            }
            ReplicateVertices();
        }

        protected override void SetTriangles()
        {
            MeshIndices = new short[12 * (3 * WallCount - 2)];
            int currentIndex1 = 0;
            FirstCeilingIndex = currentIndex1;
            int num = (outerPointCount - innerPointCount) / 2;
            int whichVertex1 = innerPointCount - 1;
            for (int whichVertex2 = 0; whichVertex2 < num; ++whichVertex2)
                currentIndex1 = SetRawTriangleIndexes(GetVertex(innerCeiling, 0), GetVertex(outerCeiling, whichVertex2), GetVertex(outerCeiling, whichVertex2 + 1), currentIndex1);
            for (int whichVertex2 = 1; whichVertex2 < innerPointCount; ++whichVertex2)
            {
                int whichVertex3 = whichVertex2 + num;
                currentIndex1 = SetQuadIndexes(GetVertex(innerCeiling, whichVertex2 - 1), GetVertex(outerCeiling, whichVertex3 - 1), GetVertex(outerCeiling, whichVertex3), GetVertex(innerCeiling, whichVertex2), currentIndex1);
            }
            for (int index = 0; index < num; ++index)
            {
                int whichVertex2 = innerPointCount + index;
                currentIndex1 = SetRawTriangleIndexes(GetVertex(innerCeiling, whichVertex1), GetVertex(outerCeiling, whichVertex2), GetVertex(outerCeiling, whichVertex2 + 1), currentIndex1);
            }
            ReplicateIndices(currentIndex1, ref currentIndex1);
            CeilingIndexCount = currentIndex1 - FirstCeilingIndex;
            FirstWallIndex = currentIndex1;
            int current = SetQuadIndexes(GetVertex(horizontalWallBottom, 0), GetVertex(horizontalWallBottom, 1), GetVertex(horizontalWallTop, 1), GetVertex(horizontalWallTop, 0), currentIndex1);
            for (int whichVertex2 = 1; whichVertex2 < outerPointCount; ++whichVertex2)
                current = SetQuadIndexes(GetVertex(outerWallBottom, whichVertex2 - 1), GetVertex(outerWallBottom, whichVertex2), GetVertex(outerWallTop, whichVertex2), GetVertex(outerWallTop, whichVertex2 - 1), current);
            int currentIndex2 = SetQuadIndexes(GetVertex(verticalWallBottom, 0), GetVertex(verticalWallBottom, 1), GetVertex(verticalWallTop, 1), GetVertex(verticalWallTop, 0), current);
            for (int whichVertex2 = innerPointCount - 1; whichVertex2 >= 1; --whichVertex2)
                currentIndex2 = SetQuadIndexes(GetVertex(innerWallBottom, whichVertex2 - 1), GetVertex(innerWallBottom, whichVertex2), GetVertex(innerWallTop, whichVertex2), GetVertex(innerWallTop, whichVertex2 - 1), currentIndex2);
            ReplicateIndices(currentIndex2 - FirstWallIndex, ref currentIndex2);
            WallIndexCount = currentIndex2 - FirstWallIndex;
        }
    }
}
