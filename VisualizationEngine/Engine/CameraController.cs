// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CameraController
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class CameraController
  {
    public abstract bool Completed { get; }

    public virtual bool Warping
    {
      set
      {
      }
    }

    protected static Ray3D ScreenToWorldRay(int screenX, int screenY, SceneState state)
    {
      Vector3D vector3D;
      vector3D.X = (2.0 * (double) screenX / state.ScreenWidth - 1.0) / state.Projection.M11;
      vector3D.Y = -(2.0 * (double) screenY / state.ScreenHeight - 1.0) / state.Projection.M22;
      vector3D.Z = 1.0;
      Vector3D direction = new Vector3D();
      Matrix4x4D inverseView = state.InverseView;
      direction.X = vector3D.X * inverseView.M11 + vector3D.Y * inverseView.M21 + vector3D.Z * inverseView.M31;
      direction.Y = vector3D.X * inverseView.M12 + vector3D.Y * inverseView.M22 + vector3D.Z * inverseView.M32;
      direction.Z = vector3D.X * inverseView.M13 + vector3D.Y * inverseView.M23 + vector3D.Z * inverseView.M33;
      return new Ray3D(new Vector3D()
      {
        X = inverseView.M41,
        Y = inverseView.M42,
        Z = inverseView.M43
      }, direction);
    }

    internal static Vector3D UnitSphereIntersectRay(Ray3D worldRay)
    {
      return worldRay.GetSphereIntersection(1.0);
    }

    public static Vector3D PlaneIntersectRay(Ray3D worldRay)
    {
      double num1 = 1.0 - worldRay.Origin.X;
      if (num1 * worldRay.Direction.X < 0.0 || Math.Abs(worldRay.Direction.X) <= Math.Abs(num1) * 1E-06)
        return Vector3D.Empty;
      double num2 = num1 / worldRay.Direction.X;
      return new Vector3D(1.0, worldRay.Origin.Y + num2 * worldRay.Direction.Y, worldRay.Origin.Z + num2 * worldRay.Direction.Z);
    }

    public abstract CameraSnapshot Update(SceneState state);

    public virtual CameraSnapshot UpdateWithCurrentState(CameraSnapshot cs, SceneState st)
    {
      return cs;
    }

    public virtual void OnFocus(CameraSnapshot previousSnapshot)
    {
    }
  }
}
