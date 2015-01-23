// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CameraStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal class CameraStep : EngineStep, ICameraControllerManager
  {
    private double flatModeTransitionTime = 2.7;
    private DefaultCameraController defaultCameraController = new DefaultCameraController();
    public const int DepthOffsetMatrixCount = 5;
    public const double CameraIdleDuration = 0.5;
    private const double DepthOffsetFactor = 1.8E-07;
    private const double NearPlaneFactor = 0.00185;
    private const double FlatMapFarPlaneFactor = 7.5;
    private const double MaxWarpingFactor = 0.99;
    private const double MaxFlatModeTransitionTime = 2.7;
    public const double MinFlatModeTransitionTime = 0.01;
    private const double MinAltitudeForWarp = 0.025;
    private CameraController currentCameraController;
    private CameraSnapshot previousCameraSnapshot;
    private SceneState previousSceneState;
    private bool previousFlatMode;
    private bool flatMode;
    private double flatModeStart;
    private double previousFlatFactor;
    private double lastCameraMoveTime;
    private bool cameraIdleRaised;

    public double FlatModeTransitionTime
    {
      set
      {
        this.flatModeTransitionTime = Math.Min(value, 2.7);
      }
    }

    public CameraController Controller
    {
      get
      {
        return this.currentCameraController;
      }
      set
      {
        CameraController cameraController = this.currentCameraController;
        this.currentCameraController = value != null ? value : (CameraController) this.defaultCameraController;
        if (this.currentCameraController == cameraController)
          return;
        this.currentCameraController.OnFocus(this.previousCameraSnapshot);
      }
    }

    public DefaultCameraController DefaultController
    {
      get
      {
        return this.defaultCameraController;
      }
    }

    public bool FlatMode
    {
      get
      {
        return this.flatMode;
      }
      set
      {
        if (value == this.flatMode)
          return;
        this.flatMode = value;
        if (this.FlatModeChanged == null)
          return;
        this.FlatModeChanged(value);
      }
    }

    public event EventHandler CameraIdle;

    public event Action<bool> FlatModeChanged;

    public CameraStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher)
      : base(dispatcher, eventDispatcher)
    {
      this.currentCameraController = (CameraController) this.defaultCameraController;
    }

    public void Reset()
    {
      this.flatModeTransitionTime = 2.7;
      this.flatMode = this.previousFlatMode = false;
      this.previousFlatFactor = 0.0;
    }

    internal override bool PreExecute(SceneState state, int phase)
    {
      bool flag1 = false;
      bool flag2 = false;
      if (this.flatModeTransitionTime > 0.01 && this.previousCameraSnapshot != null && (this.previousCameraSnapshot.Altitude > 0.025 && this.previousSceneState.SceneCustomMap == null) && state.SceneCustomMap == null)
      {
        if (this.FlatMode != this.previousFlatMode)
        {
          this.previousFlatFactor = 1.0 - Math.Min(1.0, (state.ElapsedSeconds - this.flatModeStart) / this.flatModeTransitionTime + this.previousFlatFactor);
          this.flatModeStart = state.ElapsedSeconds;
          this.previousFlatMode = this.FlatMode;
        }
        if (this.previousSceneState != null)
        {
          if (this.FlatMode && this.previousSceneState.FlatteningFactor != 1.0 || !this.FlatMode && this.previousSceneState.FlatteningFactor != 0.0)
          {
            double num = MathEx.Square(Math.Sin(Math.Min(1.0, (state.ElapsedSeconds - this.flatModeStart) / this.flatModeTransitionTime + this.previousFlatFactor) * (Math.PI / 2.0)));
            MathEx.Clamp(ref num, 0.0, 1.0);
            flag2 = num != 1.0;
            if (!this.FlatMode)
              num = 1.0 - num;
            if (num > 0.99)
              num = 1.0;
            state.FlatteningFactor = num;
            flag1 = true;
          }
          else
            state.FlatteningFactor = this.previousSceneState.FlatteningFactor;
        }
      }
      else
      {
        if (this.FlatMode != this.previousFlatMode)
        {
          flag1 = true;
          this.previousFlatMode = this.FlatMode;
        }
        state.FlatteningFactor = this.FlatMode ? 1.0 : 0.0;
      }
      if (!flag2)
        this.flatModeTransitionTime = 2.7;
      CameraSnapshot cameraSnapshot = this.currentCameraController.UpdateWithCurrentState(this.currentCameraController.Update(this.previousSceneState), state);
      state.CameraSnapshot = cameraSnapshot;
      bool flag3 = flag1 || !cameraSnapshot.Equals(this.previousCameraSnapshot);
      state.CameraMoved = flag3;
      this.previousCameraSnapshot = cameraSnapshot;
      if (this.currentCameraController.Completed)
        this.Controller = (CameraController) null;
      if (flag3)
      {
        this.lastCameraMoveTime = state.ElapsedSeconds;
        this.cameraIdleRaised = false;
      }
      else if (!this.cameraIdleRaised && this.lastCameraMoveTime < state.ElapsedSeconds - 0.5)
      {
        this.cameraIdleRaised = true;
        if (this.CameraIdle != null)
          this.CameraIdle((object) this, new EventArgs());
      }
      return flag3;
    }

    internal override void Execute(Renderer renderer, SceneState state, int phase)
    {
      try
      {
        CameraStep.SetupMatrices(state);
        CameraStep.MakeFrustum(state);
        renderer.FrameParameters.CameraPos.Value = (Vector4F) state.CameraPosition;
        renderer.FrameParameters.ViewProj.Value = (Matrix4x4F) state.ViewProjection;
        renderer.FrameParameters.View.Value = (Matrix4x4F) state.View;
        renderer.FrameParameters.Projection.Value = (Matrix4x4F) state.Projection;
        renderer.FrameParameters.FlatteningFactor.Value = (float) state.FlatteningFactor;
        for (int index = 0; index < 5; ++index)
          renderer.FrameParameters.DepthOffsetViewProj[index].Value = (Matrix4x4F) state.DepthOffsetViewProjection[index];
        this.previousSceneState = state.Clone();
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.Fail("Exception while executing the camera step.", ex);
        throw;
      }
    }

    public static void UpdateCameraViewportSnapshot(CameraViewportSnapshot state, CameraSnapshot cs)
    {
      state.CameraSnapshot = cs;
      Matrix4x4D world = CameraStep.ComputeWorld(cs, state.FlatteningFactor);
      Vector3D cameraPosition;
      state.View = world * CameraStep.ComputeView(cs, out cameraPosition);
      Matrix4x4D matrix4x4D = Matrix4x4D.Invert(state.View);
      state.InverseView = matrix4x4D;
    }

    private static Matrix4x4D ComputeWorld(CameraSnapshot camera, double flattening)
    {
      Vector3D position;
      Vector3D normal;
      Coordinates.ComputeWarp(camera.Latitude, camera.Longitude, flattening, out position, out normal);
      Vector3D north;
      Vector3D east;
      Coordinates.GetLocalFrame(normal, out north, out east);
      Matrix4x4D matrix4x4D = new Matrix4x4D(east.X, north.X, -normal.X, 0.0, east.Y, north.Y, -normal.Y, 0.0, east.Z, north.Z, -normal.Z, 0.0, 0.0, 0.0, 0.0, 1.0);
      if (flattening > 0.0)
      {
        Vector3D vector3D = matrix4x4D.Transform(position);
        matrix4x4D.Multiply(Matrix4x4D.Translation(-vector3D.X, -vector3D.Y, -vector3D.Z - 1.0));
      }
      return matrix4x4D;
    }

    private static double GetAdjustedDistance(double distance)
    {
      double num = 7.83927971443699E-06;
      return distance + num;
    }

    private static Matrix4x4D ComputeView(CameraSnapshot camera, out Vector3D cameraPosition)
    {
      double adjustedDistance = CameraStep.GetAdjustedDistance(camera.Distance);
      double rotation = camera.Rotation;
      double num1 = Math.Sin(camera.Rotation);
      double num2 = Math.Cos(camera.Rotation);
      double z = Math.Sin(camera.PivotAngle);
      double num3 = Math.Cos(camera.PivotAngle);
      cameraPosition = new Vector3D(num1 * z * adjustedDistance, num2 * z * adjustedDistance, -(1.0 + num3 * adjustedDistance));
      Vector3D cameraTarget = new Vector3D(0.0, 0.0, -1.0);
      Vector3D cameraUpVector = new Vector3D(num1 * num3, num2 * num3, z);
      return Matrix4x4D.LookAtLH(cameraPosition, cameraTarget, cameraUpVector);
    }

    internal static void SetupMatrices(SceneState state)
    {
      CameraSnapshot cameraSnapshot = state.CameraSnapshot;
      Matrix4x4D world = CameraStep.ComputeWorld(cameraSnapshot, state.FlatteningFactor);
      Vector3D cameraPosition;
      state.View = world * CameraStep.ComputeView(cameraSnapshot, out cameraPosition);
      Matrix4x4D matrix4x4D1 = Matrix4x4D.Invert(state.View);
      state.InverseView = matrix4x4D1;
      double adjustedDistance = CameraStep.GetAdjustedDistance(cameraSnapshot.Distance);
      double num1 = Math.Sqrt((adjustedDistance + 1.0) * (adjustedDistance + 1.0) - 1.0);
      double num2 = num1 * 0.00185;
      state.FarPlaneDistance = num1;
      state.NearPlaneDistance = num2;
      if (state.FlatteningFactor > 0.0)
        state.FarPlaneDistance *= 7.5;
      double aspectRatio = state.ScreenWidth / state.ScreenHeight;
      state.FieldOfViewY = Math.PI / 4.0;
      state.FieldOfViewX = Math.Asin(aspectRatio * Math.Sin(state.FieldOfViewY / 2.0)) * 2.0;
      Matrix4x4D matrix4x4D2 = Matrix4x4D.PerspectiveFovLH(state.FieldOfViewY, aspectRatio, state.NearPlaneDistance, state.FarPlaneDistance, 0.0);
      state.Projection = matrix4x4D2;
      Matrix4x4D[] matrix4x4DArray = new Matrix4x4D[5];
      for (int index = 0; index < 5; ++index)
      {
        double offset = 1.8E-07 * (double) (index - 2);
        matrix4x4DArray[index] = Matrix4x4D.PerspectiveFovLH(state.FieldOfViewY, aspectRatio, state.NearPlaneDistance, state.FarPlaneDistance, offset);
        state.DepthOffsetProjection = matrix4x4DArray;
      }
      Matrix4x4D matrix4x4D3 = Matrix4x4D.Invert(world);
      Vector3D vector3D = cameraPosition * matrix4x4D3;
      state.World = world;
      state.InverseWorld = matrix4x4D3;
      state.CameraPosition = vector3D;
    }

    internal static Matrix4x4D GetHypotheticalCameraParameters(SceneState state, CameraSnapshot camera, out Vector3D cameraPosition)
    {
      return CameraStep.ComputeWorld(camera, state.FlatteningFactor) * CameraStep.ComputeView(camera, out cameraPosition) * state.Projection;
    }

    private static void MakeFrustum(SceneState state)
    {
      Matrix4x4D viewProjection = state.ViewProjection;
      PlaneD[] frustum = new PlaneD[6];
      frustum[0].A = viewProjection.M14 + viewProjection.M11;
      frustum[0].B = viewProjection.M24 + viewProjection.M21;
      frustum[0].C = viewProjection.M34 + viewProjection.M31;
      frustum[0].D = viewProjection.M44 + viewProjection.M41;
      frustum[1].A = viewProjection.M14 - viewProjection.M11;
      frustum[1].B = viewProjection.M24 - viewProjection.M21;
      frustum[1].C = viewProjection.M34 - viewProjection.M31;
      frustum[1].D = viewProjection.M44 - viewProjection.M41;
      frustum[2].A = viewProjection.M14 + viewProjection.M12;
      frustum[2].B = viewProjection.M24 + viewProjection.M22;
      frustum[2].C = viewProjection.M34 + viewProjection.M32;
      frustum[2].D = viewProjection.M44 + viewProjection.M42;
      frustum[3].A = viewProjection.M14 - viewProjection.M12;
      frustum[3].B = viewProjection.M24 - viewProjection.M22;
      frustum[3].C = viewProjection.M34 - viewProjection.M32;
      frustum[3].D = viewProjection.M44 - viewProjection.M42;
      frustum[4].A = viewProjection.M13;
      frustum[4].B = viewProjection.M23;
      frustum[4].C = viewProjection.M33;
      frustum[4].D = viewProjection.M43;
      frustum[5].A = viewProjection.M14 - viewProjection.M13;
      frustum[5].B = viewProjection.M24 - viewProjection.M23;
      frustum[5].C = viewProjection.M34 - viewProjection.M33;
      frustum[5].D = viewProjection.M44 - viewProjection.M43;
      for (int index = 0; index < 6; ++index)
        frustum[index].Normalize();
      state.SetViewFrustum(frustum);
    }

    public override void Dispose()
    {
    }
  }
}
