// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HollowColumnVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class HollowColumnVisual : ColumnVisual
  {
    protected static double innerRadius = 0.6;
    protected int outerCeiling;
    protected int innerCeiling;
    protected int outerTopWall;
    protected int outerBottomWall;
    protected int innerTopWall;
    protected int innerBottomWall;

    internal override bool IsZeroMarker
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

    internal override float HorizontalSpacing
    {
      get
      {
        return this.outline.HorizontalSpacing;
      }
    }

    public HollowColumnVisual(int walls)
      : base(walls)
    {
      if (this.outline != null && this.outline != this)
        this.outline.Dispose();
      this.outline = (InstancedVisual) new PolygonalColumnVisual(walls, false);
    }

    protected short GetVertex(int whichComponent, int whichVertex)
    {
      return (short) (whichComponent + whichVertex % this.WallCount);
    }

    protected override void SetTriangles()
    {
      this.MeshIndices = new short[72 * this.WallCount];
      int current = 0;
      this.FirstCeilingIndex = current;
      for (int whichVertex = 0; whichVertex < this.WallCount; ++whichVertex)
        current = this.SetQuadIndexes(this.GetVertex(this.innerCeiling, whichVertex + 1), this.GetVertex(this.innerCeiling, whichVertex), this.GetVertex(this.outerCeiling, whichVertex), this.GetVertex(this.outerCeiling, whichVertex + 1), current);
      this.CeilingIndexCount = current - this.FirstCeilingIndex;
      this.FirstWallIndex = current;
      for (int whichWall = 0; whichWall < this.WallCount; ++whichWall)
        current = this.SetQuadIndexes(this.GetWallVertex(whichWall, this.outerBottomWall, 0), this.GetWallVertex(whichWall, this.outerBottomWall, 1), this.GetWallVertex(whichWall, this.outerTopWall, 1), this.GetWallVertex(whichWall, this.outerTopWall, 0), current);
      for (int whichWall = 0; whichWall < this.WallCount; ++whichWall)
        current = this.SetQuadIndexes(this.GetWallVertex(whichWall, this.innerBottomWall, 1), this.GetWallVertex(whichWall, this.innerBottomWall, 0), this.GetWallVertex(whichWall, this.innerTopWall, 0), this.GetWallVertex(whichWall, this.innerTopWall, 1), current);
      this.WallIndexCount = current - this.FirstWallIndex;
    }

    internal override void OverrideDimensionAndScales(ref float fixedDimension, ref Vector2F fixedScale, ref Vector2F variableScale)
    {
      fixedDimension = 1f;
      fixedScale.X = FlatMarker.Width;
      fixedScale.Y = FlatMarker.Height;
      variableScale.X = variableScale.Y = 0.0f;
    }
  }
}
