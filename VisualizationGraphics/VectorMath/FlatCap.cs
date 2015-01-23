using Microsoft.Data.Visualization.Engine.MathExtensions;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
  public class FlatCap : Cap
  {
    public static FlatCap Empty = new FlatCap(Vector3D.XVector, -1.0);

    public override double AngularExtent
    {
      get
      {
        return this.MaxDistanceFromCenter;
      }
    }

    public override double ProjectionRadius
    {
      get
      {
        return this.MaxDistanceFromCenter;
      }
    }

    public override bool IsWholeWorld
    {
      get
      {
        return this.extent == double.MaxValue;
      }
      set
      {
        this.extent = double.MaxValue;
      }
    }

    public FlatCap()
    {
    }

    public FlatCap(Vector3D C, double e)
      : base(C, e)
    {
    }

    public FlatCap(List<Vector3D> locations)
      : base(Vector3D.XVector, -1.0)
    {
      if (!this.SetCenterImplementation(locations))
      {
        this.center = Vector3D.XVector;
        this.extent = -1.0;
      }
      else
        this.SetExtent(locations);
    }

    public override bool SetCenter(List<Vector3D> locations)
    {
      return this.SetCenterImplementation(locations);
    }

    private bool SetCenterImplementation(List<Vector3D> locations)
    {
      if (locations.Count < 1)
        return false;
      this.AssertPointValidity(locations[0]);
      double maxY;
      double minY = maxY = locations[0].Y;
      double maxZ;
      double minZ = maxZ = locations[0].Z;
      for (int i = 1; i < locations.Count; ++i)
      {
        this.AssertPointValidity(locations[i]);
        if (locations[i].Y < minY)
          minY = locations[i].Y;
        else if (locations[i].Y > maxY)
          maxY = locations[i].Y;
        if (locations[i].Z < minZ)
          minZ = locations[i].Z;
        else if (locations[i].Z > maxZ)
          maxZ = locations[i].Z;
      }
      this.center = new Vector3D(1.0, (minY + maxY) / 2.0, (minZ + maxZ) / 2.0);
      return true;
    }

    public override double SquaredDistanceFromCenter(Vector3D point)
    {
      this.AssertPointValidity(point);
      return MathEx.Square(point.Y - this.center.Y) + MathEx.Square(point.Z - this.center.Z);
    }

    protected override void AssertPointValidity(Vector3D point)
    {
    }
  }
}
