// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.GreatCircleArc
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class GreatCircleArc
  {
    private DifferentiableScalar half = new DifferentiableScalar(0.5, 0.0);
    private Vector3D midPoint;
    private Vector3D midTangent;

    internal double ArcAngle { get; private set; }

    internal Vector3D StartPoint
    {
      get
      {
        return this.GetPosition(0.0);
      }
    }

    internal Vector3D EndPoint
    {
      get
      {
        return this.GetPosition(1.0);
      }
    }

    internal Vector3D StartTangent
    {
      get
      {
        return this.GetTangent(0.0);
      }
    }

    internal Vector3D EndTangent
    {
      get
      {
        return this.GetTangent(1.0);
      }
    }

    internal GreatCircleArc(Vector3D start, Vector3D end)
    {
      start.AssertIsUnitVector();
      end.AssertIsUnitVector();
      this.midPoint = start + end;
      double num = start * end;
      if (!this.midPoint.Normalize())
      {
        this.midPoint = start.GetOrthoNormal();
        this.midTangent = end;
        this.ArcAngle = Math.PI;
      }
      else
      {
        this.midTangent = end - start;
        if (!this.midTangent.Normalize())
        {
          this.midPoint = start;
          this.midTangent = this.midPoint.GetOrthoNormal();
          this.ArcAngle = 0.0;
        }
        else
        {
          double d = start * this.midPoint;
          this.ArcAngle = 2.0 * d <= Constants.SqrtOf2 ? 2.0 * Math.Acos(d) : 4.0 * Math.Asin(Vector3D.Distance(start, this.midPoint) / 2.0);
        }
      }
      this.midTangent.AssertIsOrthogonalTo(this.midPoint);
    }

    internal GreatCircleArc(Vector3D point, Vector3D tangent, double angle)
    {
      this.midPoint = point;
      this.midTangent = tangent;
      this.ArcAngle = angle;
    }

    internal void Reverse()
    {
      this.midTangent = -this.midTangent;
    }

    internal Vector3D GetPosition(double at)
    {
      double num = (at - 0.5) * this.ArcAngle;
      return this.midPoint * Math.Cos(num) + this.midTangent * Math.Sin(num);
    }

    internal Vector3D GetTangent(double at)
    {
      double num = (at - 0.5) * this.ArcAngle;
      return this.midTangent * Math.Cos(num) - this.midPoint * Math.Sin(num);
    }

    internal void GetPositionAndTangent(double at, out Vector3D position, out Vector3D tangent)
    {
      double num1 = (at - 0.5) * this.ArcAngle;
      double num2 = Math.Cos(num1);
      double num3 = Math.Sin(num1);
      position = this.midPoint * num2 + this.midTangent * num3;
      tangent = this.midTangent * num2 - this.midPoint * num3;
    }

    internal void GetPositionAndTangent(DifferentiableScalar at, out DifferentiableVector position, out DifferentiableVector tangent)
    {
      DifferentiableScalar differentiableScalar = (at - this.half) * this.ArcAngle;
      DifferentiableScalar cos = differentiableScalar.Cos;
      DifferentiableScalar sin = differentiableScalar.Sin;
      position = cos * this.midPoint + sin * this.midTangent;
      tangent = cos * this.midTangent - sin * this.midPoint;
    }

    internal void GetGeoCoordinates(double at, out Coordinates coordinates, out double azimuth)
    {
      Vector3D position;
      Vector3D tangent;
      this.GetPositionAndTangent(at, out position, out tangent);
      coordinates = Coordinates.World3DToGeo(position);
      azimuth = Coordinates.GetAzimuth(position, tangent);
    }

    internal void GetGeoCoordinates(DifferentiableScalar at, out DifferentiableScalar latitude, out DifferentiableScalar longitude, out DifferentiableScalar azimuth)
    {
      DifferentiableVector position;
      DifferentiableVector tangent;
      this.GetPositionAndTangent(at, out position, out tangent);
      DifferentiableScalar hypot;
      if (!DifferentiableScalar.Hypot(position.X, position.Z, out hypot))
      {
        longitude = new DifferentiableScalar(0.0, 0.0);
        latitude = new DifferentiableScalar(position.Y.Value > 0.0 ? Math.PI / 2.0 : -1.0 * Math.PI / 2.0, 0.0);
        azimuth = new DifferentiableScalar(0.0, 0.0);
      }
      else
      {
        longitude = DifferentiableScalar.Atan2(position.Z, position.X);
        latitude = position.Y.Asin;
        position.Value.AssertIsUnitVector();
        tangent.Value.AssertIsUnitVector();
        DifferentiableVector differentiableVector1 = new DifferentiableVector(Vector3D.YVector) - position * position.Y;
        differentiableVector1.Normalize();
        DifferentiableVector differentiableVector2 = position ^ differentiableVector1;
        azimuth = DifferentiableScalar.Atan2(tangent * differentiableVector2, tangent * differentiableVector1);
      }
    }
  }
}
