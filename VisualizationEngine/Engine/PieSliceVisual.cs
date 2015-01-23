// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.PieSliceVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal class PieSliceVisual : ColumnVisual
  {
    private int triangleCount;
    private int radialWallTop;
    private int radialWallBottom;
    private int arcWallTop;
    private int arcWallBottom;
    private int outlineWallTop;
    private int outlineWallBottom;

    internal override bool IsPieSegment
    {
      get
      {
        return true;
      }
    }

    internal override bool MayHaveNegativeInstances
    {
      get
      {
        return false;
      }
    }

    private int ArcWallCount
    {
      get
      {
        return this.WallCount - 2;
      }
    }

    private int ArcPointCount
    {
      get
      {
        return this.WallCount - 1;
      }
    }

    public PieSliceVisual(int arcWallCount)
      : base(arcWallCount + 2)
    {
      this.triangleCount = arcWallCount + 2 * this.WallCount;
      this.WallTop = 0;
      this.WallBottom = 1;
      this.CreateMesh();
    }

    internal override void CreateInstanceVertices()
    {
      this.MeshVertices = new InstanceVertex[5 * this.WallCount + 2 * this.ArcPointCount + 1];
      int num1 = 0;
      int num2 = 2 * this.ArcPointCount - 2;
      Vector2D[] vector2DArray = new Vector2D[num2 + 1];
      double num3 = 1.0 / (double) (2 * this.ArcWallCount);
      vector2DArray[0].X = vector2DArray[0].Y = 1.0;
      for (int index = 1; index <= num2; ++index)
      {
        vector2DArray[index].X = (double) (num2 - index) * num3;
        vector2DArray[index].Y = 1.0;
      }
      InstanceVertex[] meshVertices1 = this.MeshVertices;
      int index1 = num1;
      int num4 = 1;
      int num5 = index1 + num4;
      meshVertices1[index1] = new InstanceVertex(Vector3D.ZVector, Vector3D.ZVector);
      for (int index2 = 0; index2 < this.ArcPointCount; ++index2)
        this.MeshVertices[num5++] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[2 * index2], 1.0), Vector3D.ZVector);
      this.radialWallTop = num5;
      InstanceVertex[] meshVertices2 = this.MeshVertices;
      int index3 = num5;
      int num6 = 1;
      int num7 = index3 + num6;
      meshVertices2[index3] = new InstanceVertex(Vector3D.ZVector, Vector3D.XVector);
      InstanceVertex[] meshVertices3 = this.MeshVertices;
      int index4 = num7;
      int num8 = 1;
      int num9 = index4 + num8;
      meshVertices3[index4] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[0], 1.0), Vector3D.XVector);
      InstanceVertex[] meshVertices4 = this.MeshVertices;
      int index5 = num9;
      int num10 = 1;
      int num11 = index5 + num10;
      meshVertices4[index5] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[2 * (this.ArcPointCount - 1)], 1.0), Vector3D.Empty);
      InstanceVertex[] meshVertices5 = this.MeshVertices;
      int index6 = num11;
      int num12 = 1;
      int num13 = index6 + num12;
      meshVertices5[index6] = new InstanceVertex(Vector3D.ZVector, Vector3D.Empty);
      this.arcWallTop = num13;
      for (int index2 = 0; index2 < this.ArcPointCount; ++index2)
        this.MeshVertices[num13++] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[2 * index2], 1.0), InstancedVisual.To3D(vector2DArray[2 * index2], 0.0));
      this.radialWallBottom = num13;
      InstanceVertex[] meshVertices6 = this.MeshVertices;
      int index7 = num13;
      int num14 = 1;
      int num15 = index7 + num14;
      meshVertices6[index7] = new InstanceVertex(Vector3D.Empty, Vector3D.XVector);
      InstanceVertex[] meshVertices7 = this.MeshVertices;
      int index8 = num15;
      int num16 = 1;
      int num17 = index8 + num16;
      meshVertices7[index8] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[0], 0.0), Vector3D.XVector);
      InstanceVertex[] meshVertices8 = this.MeshVertices;
      int index9 = num17;
      int num18 = 1;
      int num19 = index9 + num18;
      meshVertices8[index9] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[2 * (this.ArcPointCount - 1)], 0.0), Vector3D.Empty);
      InstanceVertex[] meshVertices9 = this.MeshVertices;
      int index10 = num19;
      int num20 = 1;
      int num21 = index10 + num20;
      meshVertices9[index10] = new InstanceVertex(Vector3D.Empty, Vector3D.Empty);
      this.arcWallBottom = num21;
      for (int index2 = 0; index2 < this.ArcPointCount; ++index2)
      {
        Vector3D vector3D = InstancedVisual.To3D(vector2DArray[2 * index2], 0.0);
        this.MeshVertices[num21++] = new InstanceVertex(vector3D, vector3D);
      }
      this.FloorVertex = (short) num21;
      InstanceVertex[] meshVertices10 = this.MeshVertices;
      int index11 = num21;
      int num22 = 1;
      int num23 = index11 + num22;
      meshVertices10[index11] = new InstanceVertex(Vector3D.Empty, -Vector3D.ZVector);
      this.outlineWallTop = num23;
      for (int index2 = 1; index2 < this.ArcPointCount; ++index2)
      {
        int index12 = 2 * index2;
        Vector3D normal = InstancedVisual.To3D(vector2DArray[index12 - 1], 0.0);
        InstanceVertex[] meshVertices11 = this.MeshVertices;
        int index13 = num23;
        int num24 = 1;
        int num25 = index13 + num24;
        meshVertices11[index13] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[index12 - 2], 1.0), normal);
        InstanceVertex[] meshVertices12 = this.MeshVertices;
        int index14 = num25;
        int num26 = 1;
        num23 = index14 + num26;
        meshVertices12[index14] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[index12], 1.0), normal);
      }
      this.outlineWallBottom = num23;
      for (int index2 = 1; index2 < this.ArcPointCount; ++index2)
      {
        int index12 = 2 * index2;
        Vector3D normal = InstancedVisual.To3D(vector2DArray[index12 - 1], 0.0);
        InstanceVertex[] meshVertices11 = this.MeshVertices;
        int index13 = num23;
        int num24 = 1;
        int num25 = index13 + num24;
        meshVertices11[index13] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[index12 - 2], 0.0), normal);
        InstanceVertex[] meshVertices12 = this.MeshVertices;
        int index14 = num25;
        int num26 = 1;
        num23 = index14 + num26;
        meshVertices12[index14] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[index12], 0.0), normal);
      }
    }

    private int GetRadialWall(int whichWall, int TopOrBottom, int startOrEnd)
    {
      if (TopOrBottom == this.WallTop)
        return this.radialWallTop + 2 * whichWall + startOrEnd;
      else
        return this.radialWallBottom + 2 * whichWall + startOrEnd;
    }

    protected int GetArcWallVertex(int whichWall, int topOrBottom, int startOrEnd)
    {
      if (topOrBottom == this.WallTop)
        return this.arcWallTop + whichWall + startOrEnd;
      else
        return this.arcWallBottom + whichWall + startOrEnd;
    }

    protected override short GetWallVertex(int whichWall, int topOrBottom, int startOrEnd)
    {
      int num = this.Mod(whichWall);
      if (num == 0)
        return (short) this.GetRadialWall(0, topOrBottom, startOrEnd);
      if (num == this.WallCount - 1)
        return (short) this.GetRadialWall(1, topOrBottom, startOrEnd);
      else
        return (short) this.GetArcWallVertex(num - 1, topOrBottom, startOrEnd);
    }

    private short GetOutlineVertex(int whichWall, int topOrBottom, int startOrEnd)
    {
      int num = this.Mod(whichWall);
      return num <= 0 ? (short) this.GetRadialWall(0, topOrBottom, startOrEnd) : (num >= this.WallCount - 1 ? (short) this.GetRadialWall(1, topOrBottom, startOrEnd) : (topOrBottom != this.WallTop ? (short) (this.outlineWallBottom + 2 * (num - 1) + startOrEnd) : (short) (this.outlineWallTop + 2 * (num - 1) + startOrEnd)));
    }

    protected override void SetTriangleIndexes(short vertex1, short vertex2, short vertex3, short adjacent1, short adjacent2, short adjacent3, ref int raw, ref int withAdjacency)
    {
      this.MeshIndicesWithAdjacency[withAdjacency++] = vertex1;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent1;
      this.MeshIndicesWithAdjacency[withAdjacency++] = vertex2;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent2;
      this.MeshIndicesWithAdjacency[withAdjacency++] = vertex3;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent3;
    }

    private void SetOutlineWallIndices(int i, ref int raw, ref int withAdjacency)
    {
      this.SetTriangleIndexes(this.GetOutlineVertex(i, this.WallBottom, 0), this.GetOutlineVertex(i, this.WallBottom, 1), this.GetOutlineVertex(i, this.WallTop, 1), this.FloorVertex, this.GetOutlineVertex(i + 1, this.WallTop, 1), this.GetOutlineVertex(i, this.WallTop, 0), ref raw, ref withAdjacency);
      this.SetTriangleIndexes(this.GetOutlineVertex(i, this.WallTop, 1), this.GetOutlineVertex(i, this.WallTop, 0), this.GetOutlineVertex(i, this.WallBottom, 0), this.GetCeilingVertex(0), this.GetOutlineVertex(i - 1, this.WallBottom, 0), this.GetOutlineVertex(i, this.WallBottom, 1), ref raw, ref withAdjacency);
    }

    private void SetRawTriangles()
    {
      this.MeshIndices = new short[3 * this.triangleCount];
      int num = 0;
      this.FirstCeilingIndex = num;
      for (int i = 1; i < this.WallCount - 1; ++i)
        num = this.SetRawTriangleIndexes(this.GetCeilingVertex(0), this.GetCeilingVertex(i), this.GetCeilingVertex(i + 1), num);
      this.CeilingIndexCount = num - this.FirstCeilingIndex;
      this.FirstWallIndex = num;
      int currentIndex = this.SetRawWallIndexes(this.WallCount - 1, this.SetRawWallIndexes(0, num));
      for (int wall = 1; wall < this.WallCount - 1; ++wall)
        currentIndex = this.SetRawWallIndexes(wall, currentIndex);
      this.WallIndexCount = currentIndex - this.FirstWallIndex;
    }

    private void SetTrianglesWithAdjacency()
    {
      this.MeshIndicesWithAdjacency = new short[6 * this.triangleCount];
      int raw = 0;
      int withAdjacency = 0;
      for (int index = 1; index < this.WallCount - 1; ++index)
      {
        short adjacent1 = index > 1 ? this.GetCeilingVertex(index - 1) : this.GetOutlineVertex(0, this.WallBottom, 0);
        short adjacent3 = index < this.WallCount - 2 ? this.GetCeilingVertex(index + 2) : this.GetOutlineVertex(this.WallCount - 1, this.WallBottom, 0);
        this.SetTriangleIndexes(this.GetCeilingVertex(0), this.GetCeilingVertex(index), this.GetCeilingVertex(index + 1), adjacent1, this.GetOutlineVertex(index, this.WallBottom, 0), adjacent3, ref raw, ref withAdjacency);
      }
      for (int i = 0; i < this.WallCount; ++i)
        this.SetOutlineWallIndices(i, ref raw, ref withAdjacency);
    }

    protected override void SetTriangles()
    {
      this.SetRawTriangles();
      this.SetTrianglesWithAdjacency();
    }
  }
}
