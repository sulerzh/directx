// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.PolygonalColumnVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class PolygonalColumnVisual : ColumnVisual
  {
    internal PolygonalColumnVisual(int walls, bool isDerived = false)
      : base(walls)
    {
      this.AngleIncrement = 2.0 * Math.PI / (double) walls;
      this.StartAngle = this.AngleIncrement / 2.0 - Math.PI / 2.0;
      if (isDerived)
        return;
      this.CreateMesh();
    }

    protected void SetWallIndices(int i, ref int raw, ref int withAdjacency)
    {
      this.SetTriangleIndexes(this.GetWallVertex(i, this.WallBottom, 0), this.GetWallVertex(i, this.WallBottom, 1), this.GetWallVertex(i, this.WallTop, 1), this.FloorVertex, this.GetWallCenter(i + 1), this.GetWallCenter(i), ref raw, ref withAdjacency);
      this.SetTriangleIndexes(this.GetWallVertex(i, this.WallTop, 1), this.GetWallVertex(i, this.WallTop, 0), this.GetWallVertex(i, this.WallBottom, 0), this.CeilingCenter, this.GetWallCenter(i - 1), this.GetWallCenter(i), ref raw, ref withAdjacency);
    }

    protected virtual Vector2D[] Create2DPositions()
    {
      return InstancedVisual.Create2DRegularPolygon(this.StartAngle, 2 * this.WallCount);
    }

    internal override void CreateInstanceVertices()
    {
      this.MeshVertices = new InstanceVertex[7 * this.WallCount + 2];
      int num1 = 0;
      Vector3D normal = new Vector3D(0.0, 0.0, 1.0);
      Vector2D[] vector2DArray = this.Create2DPositions();
      Vector3D[] vector3DArray1 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray1[index] = InstancedVisual.To3D(vector2DArray[2 * index], 1.0);
      Vector3D[] vector3DArray2 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray2[index] = InstancedVisual.To3D(vector2DArray[2 * index + 1], 0.0);
      Vector3D[] vector3DArray3 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray3[index] = InstancedVisual.To3D(vector2DArray[2 * index], 0.0);
      this.CeilingVertices = num1;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[num1++] = new InstanceVertex(vector3DArray1[index], normal);
      this.WallTop = num1;
      for (int index1 = 0; index1 < this.WallCount; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = num1;
        int num2 = 1;
        int num3 = index2 + num2;
        meshVertices1[index2] = new InstanceVertex(vector3DArray1[index1], vector3DArray2[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = num3;
        int num4 = 1;
        num1 = index3 + num4;
        meshVertices2[index3] = new InstanceVertex(vector3DArray1[this.Mod(index1 + 1)], vector3DArray2[index1]);
      }
      this.WallCenter = num1;
      double num5 = Math.Cos(Math.PI / (double) this.WallCount);
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[num1++] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[2 * index + 1] * num5, 0.5), vector3DArray2[index]);
      this.WallBottom = num1;
      for (int index1 = 0; index1 < this.WallCount; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = num1;
        int num2 = 1;
        int num3 = index2 + num2;
        meshVertices1[index2] = new InstanceVertex(vector3DArray3[index1], vector3DArray2[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = num3;
        int num4 = 1;
        num1 = index3 + num4;
        meshVertices2[index3] = new InstanceVertex(vector3DArray3[this.Mod(index1 + 1)], vector3DArray2[index1]);
      }
      this.FloorVertices = num1;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[num1++] = new InstanceVertex(vector3DArray3[index], -normal);
      Vector3D offset = new Vector3D(0.0, 0.0, 0.0);
      this.FloorVertex = (short) num1;
      InstanceVertex[] meshVertices = this.MeshVertices;
      int index4 = num1;
      int num6 = 1;
      int index5 = index4 + num6;
      meshVertices[index4] = new InstanceVertex(offset, -normal);
      offset.Z = 1.0;
      this.CeilingCenter = (short) index5;
      this.MeshVertices[index5] = new InstanceVertex(offset, normal, 0.0f);
    }

    private void SetCeilingIndexs(ref int currentIndex, ref int currentIndexWithAdjacency)
    {
      this.FirstCeilingIndex = currentIndex;
      for (int i = 0; i < this.WallCount; ++i)
        this.SetTriangleIndexes(this.CeilingCenter, this.GetCeilingVertex(i), this.GetCeilingVertex(i + 1), this.GetCeilingVertex(i - 1), this.GetWallCenter(i), this.GetCeilingVertex(i + 1), ref currentIndex, ref currentIndexWithAdjacency);
      this.CeilingIndexCount = currentIndex - this.FirstCeilingIndex;
    }

    private void SetTriangles3Walls()
    {
      int num1 = 10;
      this.MeshIndices = new short[3 * num1];
      this.MeshIndicesWithAdjacency = new short[6 * num1];
      int num2 = 0;
      int num3 = 0;
      this.SetCeilingIndexs(ref num2, ref num3);
      this.FirstWallIndex = (int) (short) num2;
      for (int i = 0; i < 3; ++i)
        this.SetWallIndices(i, ref num2, ref num3);
      this.WallIndexCount = num2 - this.FirstWallIndex;
      this.SetTriangleIndexes(this.GetFloorVertex(0), this.GetFloorVertex(2), this.GetFloorVertex(1), this.GetWallCenter(2), this.GetWallCenter(1), this.GetWallCenter(0), ref num2, ref num3);
    }

    protected override void SetTriangles()
    {
      if (this.WallCount < 4)
      {
        if (this.WallCount != 3)
          return;
        this.SetTriangles3Walls();
      }
      else
      {
        int num1 = 4 * this.WallCount;
        this.MeshIndices = new short[4 * num1];
        this.MeshIndicesWithAdjacency = new short[6 * num1];
        int num2 = 0;
        int num3 = 0;
        this.SetCeilingIndexs(ref num2, ref num3);
        this.FirstWallIndex = num2;
        this.SetWallIndices(this.WallCount - 2, ref num2, ref num3);
        this.SetWallIndices(this.WallCount - 1, ref num2, ref num3);
        this.SetWallIndices(0, ref num2, ref num3);
        this.SetWallIndices(1, ref num2, ref num3);
        for (int i = 2; i < this.WallCount - 2; ++i)
          this.SetWallIndices(i, ref num2, ref num3);
        this.WallIndexCount = num2 - this.FirstWallIndex;
        this.SetTriangleIndexes(this.GetFloorVertex(2), this.GetFloorVertex(1), this.GetFloorVertex(0), this.GetWallCenter(1), this.GetWallCenter(0), this.FloorVertex, ref num2, ref num3);
        this.SetTriangleIndexes(this.GetFloorVertex(0), this.GetFloorVertex(this.WallCount - 1), this.GetFloorVertex(this.WallCount - 2), this.GetWallCenter(this.WallCount - 1), this.GetWallCenter(this.WallCount - 2), this.FloorVertex, ref num2, ref num3);
        for (int i = 2; i < this.WallCount - 2; ++i)
          this.SetTriangleIndexes(this.GetFloorVertex(0), this.GetFloorVertex(i + 1), this.GetFloorVertex(i), this.FloorVertex, this.GetWallCenter(i), this.FloorVertex, ref num2, ref num3);
      }
    }
  }
}
