// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RoundNullMarkerVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

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
      this.outline = (InstancedVisual) new PolygonalColumnVisual(32, false);
      double num = Math.PI / 2.0;
      this.radius = 1.0 - NullMarkerVisual.thickness;
      this.innerAngleGap = Math.Asin(NullMarkerVisual.halfGap / this.radius);
      this.innerArcAngle = num - 2.0 * this.innerAngleGap;
      this.innerPointCount = (int) (this.innerArcAngle * 8.0 / num) + 2;
      this.outerAngleGap = Math.Asin(NullMarkerVisual.halfGap);
      this.outerArcAngle = num - 2.0 * this.outerAngleGap;
      this.outerPointCount = (int) (this.outerArcAngle * 8.0 / num) + 2;
      if ((this.outerPointCount - this.innerPointCount) % 2 != 0)
        ++this.outerPointCount;
      this.WallCount = this.innerPointCount + this.outerPointCount;
      this.CreateMesh();
    }

    private static Vector2D[] SubdivideArc(double radius, double startAngle, double extent, int pointCount)
    {
      Vector2D[] vector2DArray = new Vector2D[pointCount];
      int num1 = pointCount - 1;
      double num2 = extent / (double) num1;
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
        vector3DArray[index] = InstancedVisual.To3D(points[index], z);
      return vector3DArray;
    }

    internal override void CreateInstanceVertices()
    {
      Vector2D[] points1 = RoundNullMarkerVisual.SubdivideArc(1.0, this.outerAngleGap, this.outerArcAngle, this.outerPointCount);
      Vector2D[] points2 = RoundNullMarkerVisual.SubdivideArc(this.radius, this.innerAngleGap, this.innerArcAngle, this.innerPointCount);
      Vector3D[] vertexPositions1 = RoundNullMarkerVisual.GetVertexPositions(points1, 1.0);
      Vector3D[] vertexPositions2 = RoundNullMarkerVisual.GetVertexPositions(points2, 1.0);
      Vector3D[] vertexPositions3 = RoundNullMarkerVisual.GetVertexPositions(points1, 0.0);
      Vector3D[] vertexPositions4 = RoundNullMarkerVisual.GetVertexPositions(points2, 0.0);
      this.MeshVertices = new InstanceVertex[this.WallCount * 12 + 32];
      short num1 = (short) 0;
      this.outerCeiling = (int) num1;
      for (int index = 0; index < this.outerPointCount; ++index)
        this.MeshVertices[(int) num1++] = new InstanceVertex(vertexPositions1[index], Vector3D.ZVector);
      this.innerCeiling = (int) num1;
      for (int index = 0; index < this.innerPointCount; ++index)
        this.MeshVertices[(int) num1++] = new InstanceVertex(vertexPositions2[index], Vector3D.ZVector);
      this.ReplicateVertices();
      this.horizontalWallTop = (int) num1;
      InstanceVertex[] meshVertices1 = this.MeshVertices;
      int index1 = (int) num1;
      int num2 = 1;
      short num3 = (short) (index1 + num2);
      meshVertices1[index1] = new InstanceVertex(vertexPositions2[0], -Vector3D.YVector);
      InstanceVertex[] meshVertices2 = this.MeshVertices;
      int index2 = (int) num3;
      int num4 = 1;
      short num5 = (short) (index2 + num4);
      meshVertices2[index2] = new InstanceVertex(vertexPositions1[0], -Vector3D.YVector);
      this.outerWallTop = (int) num5;
      for (int index3 = 0; index3 < this.outerPointCount; ++index3)
        this.MeshVertices[(int) num5++] = new InstanceVertex(vertexPositions1[index3], vertexPositions3[index3]);
      this.verticalWallTop = (int) num5;
      InstanceVertex[] meshVertices3 = this.MeshVertices;
      int index4 = (int) num5;
      int num6 = 1;
      short num7 = (short) (index4 + num6);
      meshVertices3[index4] = new InstanceVertex(vertexPositions1[this.outerPointCount - 1], -Vector3D.XVector);
      InstanceVertex[] meshVertices4 = this.MeshVertices;
      int index5 = (int) num7;
      int num8 = 1;
      short num9 = (short) (index5 + num8);
      meshVertices4[index5] = new InstanceVertex(vertexPositions2[this.innerPointCount - 1], -Vector3D.XVector);
      this.innerWallTop = (int) num9;
      for (int index3 = this.innerPointCount - 1; index3 >= 0; --index3)
        this.MeshVertices[(int) num9++] = new InstanceVertex(vertexPositions2[index3], -vertexPositions2[index3]);
      this.horizontalWallBottom = (int) num9;
      InstanceVertex[] meshVertices5 = this.MeshVertices;
      int index6 = (int) num9;
      int num10 = 1;
      short num11 = (short) (index6 + num10);
      meshVertices5[index6] = new InstanceVertex(vertexPositions4[0], -Vector3D.YVector);
      InstanceVertex[] meshVertices6 = this.MeshVertices;
      int index7 = (int) num11;
      int num12 = 1;
      short num13 = (short) (index7 + num12);
      meshVertices6[index7] = new InstanceVertex(vertexPositions3[0], -Vector3D.YVector);
      this.outerWallBottom = (int) num13;
      for (int index3 = 0; index3 < this.outerPointCount; ++index3)
        this.MeshVertices[(int) num13++] = new InstanceVertex(vertexPositions3[index3], vertexPositions3[index3]);
      this.verticalWallBottom = (int) num13;
      InstanceVertex[] meshVertices7 = this.MeshVertices;
      int index8 = (int) num13;
      int num14 = 1;
      short num15 = (short) (index8 + num14);
      meshVertices7[index8] = new InstanceVertex(vertexPositions3[this.outerPointCount - 1], -Vector3D.XVector);
      InstanceVertex[] meshVertices8 = this.MeshVertices;
      int index9 = (int) num15;
      int num16 = 1;
      short num17 = (short) (index9 + num16);
      meshVertices8[index9] = new InstanceVertex(vertexPositions4[this.innerPointCount - 1], -Vector3D.XVector);
      this.innerWallBottom = (int) num17;
      for (int index3 = this.innerPointCount - 1; index3 >= 0; --index3)
        this.MeshVertices[(int) num17++] = new InstanceVertex(vertexPositions4[index3], -vertexPositions4[index3]);
      this.ReplicateVertices();
    }

    protected override void SetTriangles()
    {
      this.MeshIndices = new short[12 * (3 * this.WallCount - 2)];
      int currentIndex1 = 0;
      this.FirstCeilingIndex = currentIndex1;
      int num = (this.outerPointCount - this.innerPointCount) / 2;
      int whichVertex1 = this.innerPointCount - 1;
      for (int whichVertex2 = 0; whichVertex2 < num; ++whichVertex2)
        currentIndex1 = this.SetRawTriangleIndexes(NullMarkerVisual.GetVertex(this.innerCeiling, 0), NullMarkerVisual.GetVertex(this.outerCeiling, whichVertex2), NullMarkerVisual.GetVertex(this.outerCeiling, whichVertex2 + 1), currentIndex1);
      for (int whichVertex2 = 1; whichVertex2 < this.innerPointCount; ++whichVertex2)
      {
        int whichVertex3 = whichVertex2 + num;
        currentIndex1 = this.SetQuadIndexes(NullMarkerVisual.GetVertex(this.innerCeiling, whichVertex2 - 1), NullMarkerVisual.GetVertex(this.outerCeiling, whichVertex3 - 1), NullMarkerVisual.GetVertex(this.outerCeiling, whichVertex3), NullMarkerVisual.GetVertex(this.innerCeiling, whichVertex2), currentIndex1);
      }
      for (int index = 0; index < num; ++index)
      {
        int whichVertex2 = this.innerPointCount + index;
        currentIndex1 = this.SetRawTriangleIndexes(NullMarkerVisual.GetVertex(this.innerCeiling, whichVertex1), NullMarkerVisual.GetVertex(this.outerCeiling, whichVertex2), NullMarkerVisual.GetVertex(this.outerCeiling, whichVertex2 + 1), currentIndex1);
      }
      this.ReplicateIndices(currentIndex1, ref currentIndex1);
      this.CeilingIndexCount = currentIndex1 - this.FirstCeilingIndex;
      this.FirstWallIndex = currentIndex1;
      int current = this.SetQuadIndexes(NullMarkerVisual.GetVertex(this.horizontalWallBottom, 0), NullMarkerVisual.GetVertex(this.horizontalWallBottom, 1), NullMarkerVisual.GetVertex(this.horizontalWallTop, 1), NullMarkerVisual.GetVertex(this.horizontalWallTop, 0), currentIndex1);
      for (int whichVertex2 = 1; whichVertex2 < this.outerPointCount; ++whichVertex2)
        current = this.SetQuadIndexes(NullMarkerVisual.GetVertex(this.outerWallBottom, whichVertex2 - 1), NullMarkerVisual.GetVertex(this.outerWallBottom, whichVertex2), NullMarkerVisual.GetVertex(this.outerWallTop, whichVertex2), NullMarkerVisual.GetVertex(this.outerWallTop, whichVertex2 - 1), current);
      int currentIndex2 = this.SetQuadIndexes(NullMarkerVisual.GetVertex(this.verticalWallBottom, 0), NullMarkerVisual.GetVertex(this.verticalWallBottom, 1), NullMarkerVisual.GetVertex(this.verticalWallTop, 1), NullMarkerVisual.GetVertex(this.verticalWallTop, 0), current);
      for (int whichVertex2 = this.innerPointCount - 1; whichVertex2 >= 1; --whichVertex2)
        currentIndex2 = this.SetQuadIndexes(NullMarkerVisual.GetVertex(this.innerWallBottom, whichVertex2 - 1), NullMarkerVisual.GetVertex(this.innerWallBottom, whichVertex2), NullMarkerVisual.GetVertex(this.innerWallTop, whichVertex2), NullMarkerVisual.GetVertex(this.innerWallTop, whichVertex2 - 1), currentIndex2);
      this.ReplicateIndices(currentIndex2 - this.FirstWallIndex, ref currentIndex2);
      this.WallIndexCount = currentIndex2 - this.FirstWallIndex;
    }
  }
}
