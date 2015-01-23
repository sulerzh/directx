// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TileExtent
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  public struct TileExtent
  {
    public Coordinates TopLeft { get; private set; }

    public Coordinates BottomRight { get; private set; }

    public static TileExtent Empty
    {
      get
      {
        return new TileExtent();
      }
    }

    public TileExtent(Coordinates topLeft, Coordinates bottomRight)
    {
      this = new TileExtent();
      this.TopLeft = topLeft;
      this.BottomRight = bottomRight;
    }

    public static bool operator ==(TileExtent left, TileExtent right)
    {
      if (left.TopLeft == right.TopLeft)
        return left.BottomRight == right.BottomRight;
      else
        return false;
    }

    public static bool operator !=(TileExtent left, TileExtent right)
    {
      if (!(left.TopLeft != right.TopLeft))
        return left.BottomRight != right.BottomRight;
      else
        return true;
    }

    public TileExtent Union(TileExtent extent)
    {
      return new TileExtent(new Coordinates(Math.Min(this.TopLeft.Longitude, extent.TopLeft.Longitude), Math.Max(this.TopLeft.Latitude, extent.TopLeft.Latitude)), new Coordinates(Math.Max(this.BottomRight.Longitude, extent.BottomRight.Longitude), Math.Min(this.BottomRight.Latitude, extent.BottomRight.Latitude)));
    }
  }
}
