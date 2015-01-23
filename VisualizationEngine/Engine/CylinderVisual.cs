// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CylinderVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal class CylinderVisual : PolygonalColumnVisual
  {
    public CylinderVisual()
      : base(32, true)
    {
      this.outline = (InstancedVisual) new PolygonalColumnVisual(32, false);
      this.CreateMesh();
    }

    protected override short GetWallVertex(int whichWall, int topOrBottom, int startOrEnd)
    {
      return (short) (topOrBottom + this.Mod(whichWall + startOrEnd));
    }

    protected override Vector2D[] Create2DPositions()
    {
      return InstancedVisual.Create2DRegularPolygon(this.StartAngle, this.WallCount);
    }

    internal override void CreateInstanceVertices()
    {
      this.MeshVertices = new InstanceVertex[4 * this.WallCount + 1];
      int num1 = 0;
      Vector3D vector3D = Vector3D.ZVector;
      Vector2D[] vector2DArray = this.Create2DPositions();
      Vector3D[] vector3DArray = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray[index] = InstancedVisual.To3D(vector2DArray[index], 0.0);
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[num1++] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[index], 1.0), vector3D);
      this.WallTop = num1;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[num1++] = new InstanceVertex(InstancedVisual.To3D(vector2DArray[index], 1.0), vector3DArray[index]);
      this.WallBottom = num1;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[num1++] = new InstanceVertex(vector3DArray[index], vector3DArray[index]);
      this.CeilingCenter = (short) num1;
      InstanceVertex[] meshVertices = this.MeshVertices;
      int index1 = num1;
      int num2 = 1;
      int num3 = index1 + num2;
      meshVertices[index1] = new InstanceVertex(vector3D, vector3D, 0.0f);
      this.FloorVertices = num3;
      for (int index2 = 0; index2 < this.WallCount; ++index2)
        this.MeshVertices[num3++] = new InstanceVertex(vector3DArray[index2], -vector3D);
    }

    protected override void SetTriangles()
    {
      this.MeshIndices = new short[3 * (4 * this.WallCount - 2)];
      int currentIndex = this.SetRawCeilingIndexs(0);
      this.FirstWallIndex = currentIndex;
      int num = this.SetRawWallIndexes(1, this.SetRawWallIndexes(0, this.SetRawWallIndexes(this.WallCount - 1, this.SetRawWallIndexes(this.WallCount - 2, currentIndex))));
      for (int wall = 2; wall < this.WallCount - 2; ++wall)
        num = this.SetRawWallIndexes(wall, num);
      this.WallIndexCount = num - this.FirstWallIndex;
      int current = this.SetRawTriangleIndexes(this.GetFloorVertex(0), this.GetFloorVertex(this.WallCount - 1), this.GetFloorVertex(this.WallCount - 2), this.SetRawTriangleIndexes(this.GetFloorVertex(2), this.GetFloorVertex(1), this.GetFloorVertex(0), num));
      for (int i = 2; i < this.WallCount - 2; ++i)
        current = this.SetRawTriangleIndexes(this.GetFloorVertex(0), this.GetFloorVertex(i + 1), this.GetFloorVertex(i), current);
    }
  }
}
