// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CameraViewportSnapshot
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public class CameraViewportSnapshot
  {
    public double FlatteningFactor { get; internal set; }

    public double ScreenWidth { get; internal set; }

    public double ScreenHeight { get; internal set; }

    public CameraSnapshot CameraSnapshot { get; internal set; }

    public Matrix4x4D Projection { get; internal set; }

    public Matrix4x4D World { get; internal set; }

    public Matrix4x4D View { get; internal set; }

    public Matrix4x4D InverseWorld { get; internal set; }

    public Matrix4x4D InverseView { get; internal set; }

    public Matrix4x4D ViewProjection { get; private set; }

    public bool flatEnabled
    {
      get
      {
        return this.FlatteningFactor > 0.5;
      }
    }

    public void CopyFromSceneState(SceneState ss)
    {
      this.ScreenWidth = ss.ScreenWidth;
      this.ScreenHeight = ss.ScreenHeight;
      this.CameraSnapshot = ss.CameraSnapshot;
      this.Projection = ss.Projection;
      this.World = ss.World;
      this.InverseWorld = ss.InverseWorld;
      this.InverseView = ss.InverseView;
      this.ViewProjection = ss.ViewProjection;
      this.FlatteningFactor = ss.FlatteningFactor;
      this.View = ss.View;
    }

    public void SetCameraSnapshotAndRecalulate(CameraSnapshot cs)
    {
      CameraStep.UpdateCameraViewportSnapshot(this, cs);
    }

    public Ray3D ScreenToWorldRay(int screenX, int screenY)
    {
      CameraViewportSnapshot viewportSnapshot = this;
      Vector3D vector3D;
      vector3D.X = (2.0 * (double) screenX / viewportSnapshot.ScreenWidth - 1.0) / viewportSnapshot.Projection.M11;
      vector3D.Y = -(2.0 * (double) screenY / viewportSnapshot.ScreenHeight - 1.0) / viewportSnapshot.Projection.M22;
      vector3D.Z = 1.0;
      Vector3D direction = new Vector3D();
      Matrix4x4D inverseView = viewportSnapshot.InverseView;
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

    public Vector2D WorldToScreenPoint(Vector3D worldPos)
    {
      CameraViewportSnapshot viewportSnapshot = this;
      Vector4D p = new Vector4D(viewportSnapshot.View.Transform(worldPos), 1.0);
      Vector4D vector4D1 = viewportSnapshot.Projection.Transform(p);
      Vector4D vector4D2 = vector4D1 * (1.0 / vector4D1.W);
      return new Vector2D((vector4D2.X + 1.0) / 2.0 * viewportSnapshot.ScreenWidth, (-vector4D2.Y + 1.0) / 2.0 * viewportSnapshot.ScreenHeight);
    }

    public Vector3D GetSpherePoint(double x, double y, double radius, out double distance)
    {
      Ray3D ray3D = this.ScreenToWorldRay((int) x, (int) y);
      Vector3D sphereIntersection = ray3D.GetSphereIntersection(radius);
      distance = !(sphereIntersection == Vector3D.Empty) ? 0.0 : ray3D.GetDistanceFromOrigin();
      return sphereIntersection;
    }

    public Vector3D GetPlanePoint(double x, double y)
    {
      return CameraController.PlaneIntersectRay(this.ScreenToWorldRay((int) x, (int) y));
    }

    public static bool WorldPoint3DIsOnSurface(Vector3D worldPos)
    {
      return worldPos.Length() < 1.000001;
    }

    public Vector3D ScreenToWorldPoint(double screenx, double screeny)
    {
      return this.GetWorldPoint3D(screenx, screeny);
    }

    public Vector3D GetWorldPoint3D(double screenx, double screeny)
    {
      Vector3D vector3D;
      if (this.flatEnabled)
      {
        vector3D = this.GetPlanePoint(screenx, screeny);
      }
      else
      {
        double radius1 = 1.0;
        double distance1;
        vector3D = this.GetSpherePoint(screenx, screeny, radius1, out distance1);
        if (vector3D == Vector3D.Empty)
        {
          double radius2 = (distance1 - 1.0) * 2.0 + 1.0;
          double distance2;
          vector3D = this.GetSpherePoint(screenx, screeny, radius2, out distance2);
        }
      }
      return vector3D;
    }

    public Coordinates GetWorldPoint(double screenx, double screeny)
    {
      return Coordinates.World3DToGeo(this.GetWorldPoint3D(screenx, screeny), this.flatEnabled);
    }
  }
}
