// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HollowCylinderVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal class HollowCylinderVisual : HollowColumnVisual
  {
    public HollowCylinderVisual()
      : base(32)
    {
      if (this.outline != null && this.outline != this)
        this.outline.Dispose();
      this.outline = (InstancedVisual) new PolygonalColumnVisual(32, false);
      this.CreateMesh();
    }

    protected override short GetWallVertex(int whichWall, int topOrBottom, int startOrEnd)
    {
      return (short) (topOrBottom + this.Mod(whichWall + startOrEnd));
    }

    internal override void CreateInstanceVertices()
    {
      this.MeshVertices = new InstanceVertex[6 * this.WallCount];
      Vector2D[] vector2DArray1 = InstancedVisual.Create2DRegularPolygon(this.StartAngle, this.WallCount);
      Vector2D[] vector2DArray2 = new Vector2D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector2DArray2[index] = vector2DArray1[index] * HollowColumnVisual.innerRadius;
      Vector3D[] vector3DArray1 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray1[index] = InstancedVisual.To3D(vector2DArray1[index], 1.0);
      Vector3D[] vector3DArray2 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray2[index] = InstancedVisual.To3D(vector2DArray2[index], 1.0);
      Vector3D[] vector3DArray3 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray3[index] = InstancedVisual.To3D(vector2DArray1[index], 0.0);
      Vector3D[] vector3DArray4 = new Vector3D[this.WallCount];
      for (int index = 0; index < this.WallCount; ++index)
        vector3DArray4[index] = InstancedVisual.To3D(vector2DArray2[index], 0.0);
      short num = (short) 0;
      this.outerCeiling = (int) num;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[(int) num++] = new InstanceVertex(vector3DArray1[index], Vector3D.ZVector);
      this.innerCeiling = (int) num;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[(int) num++] = new InstanceVertex(vector3DArray2[index], Vector3D.ZVector);
      this.outerTopWall = (int) num;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[(int) num++] = new InstanceVertex(vector3DArray1[index], vector3DArray3[index]);
      this.outerBottomWall = (int) num;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[(int) num++] = new InstanceVertex(vector3DArray3[index], vector3DArray3[index]);
      this.innerTopWall = (int) num;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[(int) num++] = new InstanceVertex(vector3DArray2[index], -vector3DArray3[index]);
      this.innerBottomWall = (int) num;
      for (int index = 0; index < this.WallCount; ++index)
        this.MeshVertices[(int) num++] = new InstanceVertex(vector3DArray4[index], -vector3DArray3[index]);
    }
  }
}
