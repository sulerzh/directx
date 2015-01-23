// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.SquareNullMarkerVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal class SquareNullMarkerVisual : NullMarkerVisual
  {
    public SquareNullMarkerVisual()
      : base(6)
    {
      this.outline = (InstancedVisual) new PolygonalColumnVisual(4, false);
      this.CreateMesh();
    }

    internal override void CreateInstanceVertices()
    {
      double num1 = Constants.SqrtOf2 / 2.0;
      double num2 = num1 * (1.0 - NullMarkerVisual.thickness);
      this.MeshVertices = new InstanceVertex[120];
      Vector2D[] vector2DArray = new Vector2D[6]
      {
        new Vector2D(num1, NullMarkerVisual.halfGap),
        new Vector2D(num1, num1),
        new Vector2D(NullMarkerVisual.halfGap, num1),
        new Vector2D(NullMarkerVisual.halfGap, num2),
        new Vector2D(num2, num2),
        new Vector2D(num2, NullMarkerVisual.halfGap)
      };
      Vector3D[] vector3DArray1 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray1[index] = InstancedVisual.To3D(vector2DArray[index], 1.0);
      Vector3D[] vector3DArray2 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray2[index] = InstancedVisual.To3D(vector2DArray[index], 0.0);
      Vector3D[] vector3DArray3 = new Vector3D[6]
      {
        Vector3D.XVector,
        Vector3D.YVector,
        -Vector3D.XVector,
        -Vector3D.YVector,
        -Vector3D.XVector,
        -Vector3D.YVector
      };
      short num3 = (short) 0;
      this.CeilingVertices = (int) num3;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[(int) num3++] = new InstanceVertex(vector3DArray1[index], Vector3D.ZVector);
      this.WallTop = (int) num3;
      for (int index1 = 0; index1 < this.WallCount; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = (int) num3;
        int num4 = 1;
        short num5 = (short) (index2 + num4);
        meshVertices1[index2] = new InstanceVertex(vector3DArray1[index1], vector3DArray3[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = (int) num5;
        int num6 = 1;
        num3 = (short) (index3 + num6);
        meshVertices2[index3] = new InstanceVertex(vector3DArray1[(index1 + 1) % this.WallCount], vector3DArray3[index1]);
      }
      this.WallBottom = (int) num3;
      for (int index1 = 0; index1 < this.WallCount; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = (int) num3;
        int num4 = 1;
        short num5 = (short) (index2 + num4);
        meshVertices1[index2] = new InstanceVertex(vector3DArray2[index1], vector3DArray3[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = (int) num5;
        int num6 = 1;
        num3 = (short) (index3 + num6);
        meshVertices2[index3] = new InstanceVertex(vector3DArray2[(index1 + 1) % this.WallCount], vector3DArray3[index1]);
      }
      this.ReplicateVertices();
    }

    protected override void SetTriangles()
    {
      this.MeshIndices = new short[192];
      this.FirstCeilingIndex = 0;
      int currentIndex = this.SetQuadIndexes(NullMarkerVisual.GetVertex(this.CeilingVertices, 1), NullMarkerVisual.GetVertex(this.CeilingVertices, 2), NullMarkerVisual.GetVertex(this.CeilingVertices, 3), NullMarkerVisual.GetVertex(this.CeilingVertices, 4), this.SetQuadIndexes(NullMarkerVisual.GetVertex(this.CeilingVertices, 0), NullMarkerVisual.GetVertex(this.CeilingVertices, 1), NullMarkerVisual.GetVertex(this.CeilingVertices, 4), NullMarkerVisual.GetVertex(this.CeilingVertices, 5), 0));
      this.ReplicateIndices(currentIndex, ref currentIndex);
      this.CeilingIndexCount = currentIndex - this.FirstCeilingIndex;
      this.FirstWallIndex = currentIndex;
      for (int wall = 0; wall < this.WallCount; ++wall)
        currentIndex = this.SetRawWallIndexes(wall, currentIndex);
      this.ReplicateIndices(currentIndex - this.FirstWallIndex, ref currentIndex);
      this.WallIndexCount = currentIndex - this.FirstWallIndex;
    }
  }
}
