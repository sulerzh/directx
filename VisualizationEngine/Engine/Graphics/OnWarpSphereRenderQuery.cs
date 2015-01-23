// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Graphics.OnWarpSphereRenderQuery
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
  internal class OnWarpSphereRenderQuery : RenderQuery
  {
    private double s;
    private double longArcRadius;
    private double longArcLat;
    private double shortArcRadius;
    private double shortArcLat;
    private double LongArcMidX;
    private double shortArcMidX;

    internal OnWarpSphereRenderQuery(double curvature, Matrix4x4D projection, float scale, InstanceLayer layer, List<int> queryResult)
      : base(projection, scale, layer, queryResult)
    {
      this.s = curvature;
      this.sphereRadius = 1.0 / curvature;
    }

    private void ComputeRadiusAndMidX(double latititude, out double radius, out double midX)
    {
      double sin = Math.Tanh(latititude);
      double cos = 1.0 / Math.Cosh(latititude);
      radius = this.sphereRadius * cos;
      double y;
      Coordinates.GetPointOnArc(this.sphereRadius, cos, sin, 1.0, out midX, out y);
    }

    private void GetArcsPoints(double longitude, out double longArcX, out double longArcZ, out double shortArcX, out double shortArcZ)
    {
      double sin = Math.Sin(longitude);
      double cos = Math.Cos(longitude);
      Coordinates.GetPointOnArc(this.longArcRadius, cos, sin, this.LongArcMidX, out longArcX, out longArcZ);
      Coordinates.GetPointOnArc(this.shortArcRadius, cos, sin, this.shortArcMidX, out shortArcX, out shortArcZ);
    }

    protected override Box3D Get3DBounds(SpatialIndex.Node node)
    {
      Box3D box3D = new Box3D();
      box3D.minY = this.sphereRadius * Math.Tanh(this.s * node.LongLatBounds.minY);
      box3D.maxY = this.sphereRadius * Math.Tanh(this.s * node.LongLatBounds.maxY);
      if (Math.Abs(node.LongLatBounds.minY) <= Math.Abs(node.LongLatBounds.maxY))
      {
        this.longArcLat = this.s * node.LongLatBounds.minY;
        this.shortArcLat = this.s * node.LongLatBounds.maxY;
      }
      else
      {
        this.longArcLat = this.s * node.LongLatBounds.maxY;
        this.shortArcLat = this.s * node.LongLatBounds.minY;
      }
      if (box3D.minY * box3D.maxY < 0.0)
      {
        this.longArcLat = 0.0;
        this.longArcRadius = this.sphereRadius;
        this.LongArcMidX = 1.0;
      }
      else
        this.ComputeRadiusAndMidX(this.longArcLat, out this.longArcRadius, out this.LongArcMidX);
      this.ComputeRadiusAndMidX(this.shortArcLat, out this.shortArcRadius, out this.shortArcMidX);
      double longitude1 = this.s * node.LongLatBounds.minX;
      double longArcX1;
      double longArcZ1;
      double shortArcX1;
      double shortArcZ1;
      this.GetArcsPoints(longitude1, out longArcX1, out longArcZ1, out shortArcX1, out shortArcZ1);
      double longitude2 = this.s * node.LongLatBounds.maxX;
      double longArcX2;
      double longArcZ2;
      double shortArcX2;
      double shortArcZ2;
      this.GetArcsPoints(longitude2, out longArcX2, out longArcZ2, out shortArcX2, out shortArcZ2);
      box3D.minX = Math.Min(Math.Min(shortArcX1, longArcX1), Math.Min(shortArcX2, longArcX2));
      box3D.maxX = longitude1 * longitude2 >= 0.0 ? Math.Max(Math.Max(shortArcX1, longArcX1), Math.Max(shortArcX2, longArcX2)) : this.longArcRadius;
      double num = Math.PI / 2.0;
      box3D.maxZ = (longitude1 - num) * (longitude2 - num) >= 0.0 ? Math.Max(Math.Max(shortArcZ1, longArcZ1), Math.Max(shortArcZ2, longArcZ2)) : this.longArcRadius;
      box3D.minZ = (longitude1 + num) * (longitude2 + num) >= 0.0 ? Math.Min(Math.Min(shortArcZ1, longArcZ1), Math.Min(shortArcZ2, longArcZ2)) : -this.longArcRadius;
      return box3D;
    }
  }
}
