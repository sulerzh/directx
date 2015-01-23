// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.MercatorLine
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class MercatorLine
  {
    private const double MaxLatitude = 3.1415929004566;

    internal Vector2D Start { get; set; }

    internal Vector2D End { get; set; }

    internal double Length
    {
      get
      {
        return Vector2D.Distance(this.Start, this.End);
      }
    }

    internal MercatorLine(Vector2D start, Vector2D end)
    {
      this.Start = new Vector2D(start.X, Coordinates.Mercator(start.Y));
      this.End = new Vector2D(end.X, Coordinates.Mercator(end.Y));
    }

    internal MercatorLine(double midLong, double midLat, double angle, double length)
    {
      Vector2D point = new Vector2D(midLong, Coordinates.Mercator(midLat));
      Vector2D direction = new Vector2D(length * Math.Sin(angle), length * Math.Cos(angle));
      this.Start = MercatorLine.ClipToDomain(point, -direction);
      this.End = MercatorLine.ClipToDomain(point, direction);
    }

    private static void Clip(double coordinate, double increment, double bound, ref double t)
    {
      if (coordinate + t * increment <= bound)
        return;
      t = Math.Max((bound - coordinate) / increment - 1E-06, 0.0);
    }

    private static Vector2D ClipToDomain(Vector2D point, Vector2D direction)
    {
      double t = 1.0;
      MercatorLine.Clip(-point.X, -direction.X, Math.PI, ref t);
      MercatorLine.Clip(point.X, direction.X, Math.PI, ref t);
      MercatorLine.Clip(-point.Y, -direction.Y, 3.1415929004566, ref t);
      MercatorLine.Clip(point.Y, direction.Y, 3.1415929004566, ref t);
      return point + direction * t;
    }

    internal void Reverse()
    {
      Vector2D start = this.Start;
      this.Start = this.End;
      this.End = start;
    }

    internal Vector2D GetPoint(double t)
    {
      double num = 1.0 - t;
      return new Vector2D(num * this.Start.X + t * this.End.X, Coordinates.InverseMercator(num * this.Start.Y + t * this.End.Y));
    }

    internal void GetPoint(DifferentiableScalar t, out DifferentiableScalar longitude, out DifferentiableScalar y)
    {
      DifferentiableScalar differentiableScalar = new DifferentiableScalar(1.0, 0.0) - t;
      longitude = differentiableScalar * this.Start.X + t * this.End.X;
      y = Coordinates.InverseMercator(differentiableScalar * this.Start.Y + t * this.End.Y);
    }
  }
}
