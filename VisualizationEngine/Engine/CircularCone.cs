// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CircularCone
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal class CircularCone : PolygonalCone
  {
    public CircularCone()
      : base(32, true)
    {
      this.outline = (InstancedVisual) new PolygonalCone(32, false);
      this.CreateMesh();
    }

    internal override void CreateInstanceVertices()
    {
      Vector2D[] vector2DArray = InstancedVisual.Create2DRegularPolygon(this.StartAngle, this.WallCount);
      Vector3D[] vector3DArray1 = new Vector3D[this.WallCount];
      Vector3D[] vector3DArray2 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
      {
        vector3DArray1[index] = InstancedVisual.To3D(vector2DArray[index], 1.0);
        vector3DArray2[index] = InstancedVisual.To3D(vector2DArray[index], -1.0) * Constants.SqrtOf2 / 2.0;
        vector3DArray2[index].AssertIsUnitVector();
      }
      this.MeshVertices = new InstanceVertex[3 * this.WallCount];
      short num1 = (short) 0;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[(int) num1++] = new InstanceVertex(vector3DArray1[index], Vector3D.ZVector);
      this.sideVertices = num1;
      for (int index1 = 0; index1 < this.WallCount; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = (int) num1;
        int num2 = 1;
        short num3 = (short) (index2 + num2);
        meshVertices1[index2] = new InstanceVertex(vector3DArray1[index1], vector3DArray2[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = (int) num3;
        int num4 = 1;
        num1 = (short) (index3 + num4);
        meshVertices2[index3] = new InstanceVertex(Vector3D.Empty, vector3DArray2[index1]);
      }
    }

    private static short BaseVertex(int i)
    {
      return (short) i;
    }

    private short SideEdgeVertex(int edge, int vertex)
    {
      return (short) ((int) this.sideVertices + 2 * this.Mod(edge) + vertex);
    }

    protected override void SetTriangles()
    {
      this.MeshIndices = new short[3 * (2 * this.WallCount - 2)];
      int current = 0;
      this.FirstCeilingIndex = current;
      for (int i = 1; i < this.WallCount - 1; ++i)
        current = this.SetRawTriangleIndexes(CircularCone.BaseVertex(0), CircularCone.BaseVertex(i), CircularCone.BaseVertex(i + 1), current);
      this.CeilingIndexCount = current - this.FirstCeilingIndex;
      this.FirstWallIndex = current;
      for (int edge = 0; edge < this.WallCount; ++edge)
        current = this.SetRawTriangleIndexes(this.SideEdgeVertex(edge, 0), this.SideEdgeVertex(edge, 1), this.SideEdgeVertex(edge + 1, 0), current);
      this.WallIndexCount = current - this.FirstWallIndex;
    }
  }
}
