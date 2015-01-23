// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ColumnVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class ColumnVisual : InstancedVisual
  {
    protected int WallBottom { get; set; }

    protected int WallTop { get; set; }

    protected int FloorVertices { get; set; }

    protected int CeilingVertices { get; set; }

    protected int WallCenter { get; set; }

    protected short FloorVertex { get; set; }

    protected short CeilingCenter { get; set; }

    internal ColumnVisual(int walls)
      : base(walls)
    {
      this.WallCount = walls;
      this.StartAngle = this.AngleIncrement = 0.0;
    }

    protected short GetCeilingVertex(int i)
    {
      return (short) (this.CeilingVertices + this.Mod(i));
    }

    protected short GetFloorVertex(int i)
    {
      return (short) (this.FloorVertices + this.Mod(i));
    }

    protected virtual short GetWallVertex(int whichWall, int whichBatch, int startOrEnd)
    {
      return (short) (whichBatch + 2 * this.Mod(whichWall) + startOrEnd);
    }

    protected short GetWallCenter(int i)
    {
      return (short) (this.WallCenter + this.Mod(i));
    }

    protected int SetRawCeilingIndexs(int currentIndex)
    {
      this.FirstCeilingIndex = currentIndex;
      for (int i = 0; i < this.WallCount; ++i)
        currentIndex = this.SetRawTriangleIndexes(this.CeilingCenter, this.GetCeilingVertex(i), this.GetCeilingVertex(i + 1), currentIndex);
      this.CeilingIndexCount = currentIndex - this.FirstCeilingIndex;
      return currentIndex;
    }

    protected int SetRawWallIndexes(int wall, int currentIndex)
    {
      int current = this.SetRawTriangleIndexes(this.GetWallVertex(wall, this.WallBottom, 0), this.GetWallVertex(wall, this.WallBottom, 1), this.GetWallVertex(wall, this.WallTop, 1), currentIndex);
      return this.SetRawTriangleIndexes(this.GetWallVertex(wall, this.WallTop, 1), this.GetWallVertex(wall, this.WallTop, 0), this.GetWallVertex(wall, this.WallBottom, 0), current);
    }
  }
}
