// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.DefaultCameraController
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class DefaultCameraController : CameraController
  {
    private bool autoPivotEnabled = true;
    private double currentDistanceFromTarget = 2.0;
    private double targetDistanceFromTarget = 1.0;
    private CameraSnapshot previousCameraTarget = new CameraSnapshot();
    private const double MaxLatitude = 1.46607657167524;
    private const double PivotToScreenRatio = 0.6;
    private const double MouseSpeedMultiplier = 1.0;
    private const double KeyboardSpeedRatio = 0.1;
    private const double MaxPivotAngle = 1.39626340159546;
    internal const double MaxDistanceFromTarget = 8.0;
    public const double MinDistanceFromTarget = 7.83927971443699E-05;
    private const double MinDistanceFromGround = 7.83927971443699E-05;
    private const double MinAngleThreshold = 0.00174532925199433;
    private const double MinFrameDuration = 16.0;
    private const double MaxFrameDuration = 90.0;
    private const double ZoomFactor = 1.25;
    private const double InertiaFactor = 60.05;
    private const double DefaultDistanceFromTargetBegin = 2.0;
    public const double DefaultDistanceFromTargetEnd = 1.0;
    private const double PivotFactor = 5.2;
    private const double PivotFactorOffset = 0.2;
    private SceneState previousSceneState;
    private bool flatEnabled;
    private double currentLatitude;
    private double targetLatitude;
    private double currentLongitude;
    private double targetLongitude;
    private double currentRotation;
    private double targetRotation;
    private double currentPivotAngle;
    private double targetPivotAngle;
    private bool moveToHoveredObject;
    public static bool SkipCameraSmoothing;

    public double PivotAngle
    {
      get
      {
        return this.currentPivotAngle;
      }
      set
      {
        this.currentPivotAngle = value;
        MathEx.Clamp(ref this.currentPivotAngle, -4.0 * Math.PI / 9.0, 0.0);
        this.targetPivotAngle = this.currentPivotAngle;
        this.autoPivotEnabled = false;
      }
    }

    public double PivotAngleTarget
    {
      get
      {
        if (this.autoPivotEnabled)
          return this.AutoPivotAngle;
        else
          return this.targetPivotAngle;
      }
      set
      {
        this.targetPivotAngle = value;
        MathEx.Clamp(ref this.targetPivotAngle, -4.0 * Math.PI / 9.0, 0.0);
        this.autoPivotEnabled = false;
      }
    }

    private double AutoPivotAngle
    {
      get
      {
        return DefaultCameraController.GetAutoPivotAngle(this.targetDistanceFromTarget);
      }
    }

    public double RotationAngleTarget
    {
      get
      {
        return this.targetRotation;
      }
      set
      {
        this.targetRotation = value;
      }
    }

    public bool AutoPivotEnabled
    {
      get
      {
        return this.autoPivotEnabled;
      }
      set
      {
        this.autoPivotEnabled = value;
      }
    }

    public double DistanceToTarget
    {
      get
      {
        return this.targetDistanceFromTarget;
      }
      set
      {
        this.targetDistanceFromTarget = value;
        MathEx.Clamp(ref this.targetDistanceFromTarget, 7.83927971443699E-05, 8.0);
      }
    }

    public override bool Completed
    {
      get
      {
        return false;
      }
    }

    public event Action<CameraSnapshot> CameraTargetChanged;

    public static double GetAutoPivotAngle(double distance)
    {
      return Math.Max(Math.Atan(Math.Min(2.0, distance) * 5.2 + 0.2) - Math.Atan(10.4), -4.0 * Math.PI / 9.0);
    }

    public static DifferentiableScalar GetAutoPivotAngle(DifferentiableScalar distance)
    {
      return DifferentiableScalar.Max((DifferentiableScalar.Min(distance, 2.0) * 5.2 + 0.2).Atan - Math.Atan(10.4), -4.0 * Math.PI / 9.0);
    }

    public void ZoomIn()
    {
      this.ZoomIn(1.0);
    }

    public void ZoomIn(double multiplier)
    {
      if (this.targetDistanceFromTarget > this.currentDistanceFromTarget)
      {
        this.targetDistanceFromTarget = this.currentDistanceFromTarget;
      }
      else
      {
        this.targetDistanceFromTarget /= 1.25 * multiplier;
        MathEx.Clamp(ref this.targetDistanceFromTarget, 7.83927971443699E-05, 8.0);
      }
    }

    public void ZoomOut()
    {
      this.ZoomOut(1.0);
    }

    public void ZoomOut(double multiplier)
    {
      if (this.targetDistanceFromTarget < this.currentDistanceFromTarget)
      {
        this.targetDistanceFromTarget = this.currentDistanceFromTarget;
      }
      else
      {
        this.targetDistanceFromTarget *= 1.25 * multiplier;
        MathEx.Clamp(ref this.targetDistanceFromTarget, 7.83927971443699E-05, 8.0);
        if (this.targetDistanceFromTarget != 8.0)
          return;
        this.RotationAngleTarget = 0.0;
        this.autoPivotEnabled = true;
      }
    }

    public void RotatePivotView(double pivot, double rotation)
    {
      this.targetRotation = this.currentRotation + 0.1 * rotation;
      if (pivot == 0.0)
        return;
      this.autoPivotEnabled = false;
      this.targetPivotAngle = this.currentPivotAngle + 0.1 * pivot;
      MathEx.Clamp(ref this.targetPivotAngle, -4.0 * Math.PI / 9.0, 0.0);
    }

    public void Rotate(CameraRotation rotation)
    {
      switch (rotation)
      {
        case CameraRotation.Up:
          this.RotatePivotView(1.0, 0.0);
          break;
        case CameraRotation.Down:
          this.RotatePivotView(-1.0, 0.0);
          break;
        case CameraRotation.Left:
          this.RotatePivotView(0.0, 1.0);
          break;
        case CameraRotation.Right:
          this.RotatePivotView(0.0, -1.0);
          break;
      }
    }

    public void ResetView()
    {
      this.AutoPivotEnabled = true;
      this.RotationAngleTarget = 0.0;
      this.targetDistanceFromTarget = this.currentDistanceFromTarget = 1.0;
    }

    public void Move(double moveX, double moveY)
    {
      if (this.previousSceneState == null)
        return;
      double startX = this.previousSceneState.ScreenWidth / 2.0;
      double startY = this.previousSceneState.ScreenHeight / 2.0;
      this.MoveView(startX, startY, startX + 0.1 * this.previousSceneState.ScreenHeight * moveX, startY + 0.1 * this.previousSceneState.ScreenHeight * moveY);
    }

    public void MoveToHoveredObject(double moveToXIfNoneHovered, double moveToYIfNoneHovered)
    {
      this.MoveTo(moveToXIfNoneHovered, moveToYIfNoneHovered);
      this.moveToHoveredObject = true;
    }

    public void MoveTo(double moveToX, double moveToY)
    {
      this.moveToHoveredObject = false;
      Ray3D worldRay = CameraController.ScreenToWorldRay((int) moveToX, (int) moveToY, this.previousSceneState);
      Vector3D worldPos = this.flatEnabled ? CameraController.PlaneIntersectRay(worldRay) : CameraController.UnitSphereIntersectRay(worldRay);
      if (!(worldPos != Vector3D.Empty))
        return;
      Coordinates coordinates = Coordinates.World3DToGeo(worldPos, this.flatEnabled);
      if (coordinates.IsOutOfTheWorld(this.flatEnabled))
        return;
      this.targetLatitude = coordinates.Latitude;
      this.targetLongitude = coordinates.Longitude;
    }

    private static Vector3D Clip(Vector3D vector, double value, double bound)
    {
      if (Math.Abs(value) < bound - 1E-06)
        return vector;
      Vector3D vector3D = vector * (bound / value - 1E-06);
      vector3D.X = 1.0;
      return vector3D;
    }

    public void MoveTowards(double moveToX, double moveToY)
    {
      Ray3D worldRay1 = CameraController.ScreenToWorldRay((int) moveToX, (int) moveToY, this.previousSceneState);
      Vector3D point = this.flatEnabled ? CameraController.PlaneIntersectRay(worldRay1) : CameraController.UnitSphereIntersectRay(worldRay1);
      if (point == Vector3D.Empty)
        return;
      SceneState state = this.previousSceneState.Clone();
      state.CameraSnapshot.Distance = this.targetDistanceFromTarget;
      state.CameraSnapshot.Rotation = this.targetRotation;
      state.CameraSnapshot.PivotAngle = this.PivotAngleTarget;
      Coordinates coordinates;
      if (this.flatEnabled)
      {
        coordinates = Coordinates.Flat3DToGeo(point);
        if (coordinates.IsOutOfTheWorld(this.flatEnabled))
          return;
        state.CameraSnapshot.Latitude = coordinates.Latitude;
        state.CameraSnapshot.Longitude = coordinates.Longitude;
        CameraStep.SetupMatrices(state);
        Vector3D vector3D = CameraController.PlaneIntersectRay(CameraController.ScreenToWorldRay((int) moveToX, (int) moveToY, state));
        if (vector3D != Vector3D.Empty)
          point += point - vector3D;
        DefaultCameraController.ClipToMap(ref point);
        coordinates = Coordinates.Flat3DToGeo(point);
      }
      else
      {
        Vector3D vector3D1 = Coordinates.GeoTo3D(this.targetLongitude, this.targetLatitude);
        state.CameraSnapshot.Latitude = this.targetLatitude;
        state.CameraSnapshot.Longitude = this.targetLongitude;
        CameraStep.SetupMatrices(state);
        Ray3D worldRay2 = CameraController.ScreenToWorldRay((int) moveToX, (int) moveToY, state);
        Vector3D left1 = CameraController.UnitSphereIntersectRay(worldRay2);
        if (left1 == Vector3D.Empty)
        {
          left1 = worldRay2.GetSmallestPoint();
          if (!left1.Normalize())
            return;
        }
        left1.AssertIsUnitVector();
        double num1 = vector3D1 * left1;
        double a1 = left1 * Coordinates.GetNorthVector(vector3D1);
        double a2 = Vector3D.Cross(left1, vector3D1).Y;
        Vector3D vector3D2 = point;
        double num2 = a1 * a1;
        double[] numArray1 = MathEx.SolveQuadratic(num1 * num1 + num2, -vector3D2.Y * num1, vector3D2.Y * vector3D2.Y - num2);
        Vector3D left2 = vector3D2 * num1;
        double expected = (left1 - vector3D1).Length() * Math.Sqrt((1.0 + num1) / 2.0);
        Vector3D north;
        Vector3D east;
        Coordinates.GetLocalFrame(point, out north, out east);
        Vector3D vector3D3 = north * expected;
        Vector3D vector3D4 = east * expected;
        Vector3D vector3D5 = Vector3D.XVector;
        bool flag = false;
        for (int index1 = 0; !flag && index1 < numArray1.Length; ++index1)
        {
          double[] numArray2 = MathEx.SolveTrigEquation(vector3D3.Y, vector3D4.Y, numArray1[index1] - left2.Y);
          for (int index2 = 0; !flag && index2 < numArray2.Length; ++index2)
          {
            vector3D5 = left2 + vector3D3 * Math.Cos(numArray2[index2]) + vector3D4 * Math.Sin(numArray2[index2]);
            vector3D5.AssertIsUnitVector();
            MathEx.AssertEqual((vector3D5 - left2).Length(), expected, 1E-06);
            MathEx.AssertEqual(left2.Y + vector3D3.Y * Math.Cos(numArray2[index2]) + vector3D4.Y * Math.Sin(numArray2[index2]), numArray1[index1], 0.0001);
            flag = MathEx.AreOfEqualSigns(a2, Vector3D.Cross(left2, vector3D5).Y, 1E-06) && MathEx.AreOfEqualSigns(a1, left2 * Coordinates.GetNorthVector(vector3D5), 1E-06);
          }
        }
        if (!flag)
          return;
        coordinates = Coordinates.World3DToGeo(vector3D5);
      }
      this.targetLatitude = coordinates.Latitude;
      this.targetLongitude = coordinates.Longitude;
    }

    private static void ClipToMap(ref Vector3D point)
    {
      point = DefaultCameraController.Clip(point, point.Z, Math.PI);
      point = DefaultCameraController.Clip(point, point.Y, 3.14159290045661);
      point.X = 1.0;
    }

    private static Vector3D GetSpherePoint(double x, double y, double radius, SceneState state, out double distance)
    {
      Ray3D ray3D = CameraController.ScreenToWorldRay((int) x, (int) y, state);
      Vector3D sphereIntersection = ray3D.GetSphereIntersection(radius);
      distance = !(sphereIntersection == Vector3D.Empty) ? 0.0 : ray3D.GetDistanceFromOrigin();
      return sphereIntersection;
    }

    private Vector3D GetPlanePoint(double x, double y, SceneState state)
    {
      return CameraController.PlaneIntersectRay(CameraController.ScreenToWorldRay((int) x, (int) y, state));
    }

    public CameraViewportSnapshot UpdateViewportSnapshot(CameraViewportSnapshot cvs)
    {
      cvs.CopyFromSceneState(this.previousSceneState);
      cvs.SetCameraSnapshotAndRecalulate(this.GetTargetCameraSnapshot());
      return cvs;
    }

    public Vector3D BeginKeepCameraTargetted(CameraViewportSnapshot cvs, double mouseX, double mouseY)
    {
      this.UpdateViewportSnapshot(cvs);
      cvs.SetCameraSnapshotAndRecalulate(this.GetTargetCameraSnapshot());
      return cvs.GetWorldPoint3D(mouseX, mouseY);
    }

    public void EndKeepCameraTargetted(CameraViewportSnapshot cvs, Vector3D worldPos, double mouseX, double mouseY)
    {
      this.MovePointToMatchScreenLocation(cvs, worldPos, mouseX, mouseY, mouseX, mouseY, this.GetTargetCameraSnapshot(), (SingleTouchPoint) null);
    }

    public void MovePointToMatchScreenLocation(CameraViewportSnapshot cvs, Vector3D worldPos, double endX, double endY, double prevEndX, double prevEndY, CameraSnapshot replaceCamera = null, SingleTouchPoint scndTouchPoint = null)
    {
      this.UpdateViewportSnapshot(cvs);
      if (replaceCamera != null)
        cvs.SetCameraSnapshotAndRecalulate(replaceCamera);
      cvs.CameraSnapshot = cvs.CameraSnapshot.Clone();
      if (!CameraViewportSnapshot.WorldPoint3DIsOnSurface(worldPos))
      {
        cvs.SetCameraSnapshotAndRecalulate(cvs.CameraSnapshot);
        Vector3D worldPoint3D = cvs.GetWorldPoint3D(prevEndX, prevEndY);
        if (CameraViewportSnapshot.WorldPoint3DIsOnSurface(worldPoint3D))
          worldPos = worldPoint3D;
      }
      int num = 6;
      for (int index = 0; index < num; ++index)
      {
        cvs.SetCameraSnapshotAndRecalulate(cvs.CameraSnapshot);
        DefaultCameraController.MovePointToMatchScreenLocation_step(cvs, worldPos, endX, endY, prevEndX, prevEndY, scndTouchPoint);
        if (scndTouchPoint != null && this.autoPivotEnabled)
          cvs.CameraSnapshot.PivotAngle = DefaultCameraController.GetAutoPivotAngle(cvs.CameraSnapshot.Distance);
      }
      this.targetLatitude = cvs.CameraSnapshot.Latitude;
      this.targetLongitude = cvs.CameraSnapshot.Longitude;
      if (scndTouchPoint == null)
        return;
      this.RotationAngleTarget = cvs.CameraSnapshot.Rotation;
      this.DistanceToTarget = cvs.CameraSnapshot.Distance;
      if (!this.autoPivotEnabled)
        return;
      this.targetPivotAngle = cvs.CameraSnapshot.PivotAngle;
    }

    private static void MovePointToMatchScreenLocation_step(CameraViewportSnapshot cvs, Vector3D fromGeo3d, double endX, double endY, double prevEndX, double prevEndY, SingleTouchPoint scndTouchPoint)
    {
      Vector3D vector3D = new Vector3D(fromGeo3d);
      double latitude = cvs.CameraSnapshot.Latitude;
      double longitude = cvs.CameraSnapshot.Longitude;
      double num1;
      if (cvs.flatEnabled)
      {
        Vector3D planePoint = cvs.GetPlanePoint(endX, endY);
        if (vector3D == Vector3D.Empty || planePoint == Vector3D.Empty)
          return;
        double y = Coordinates.Mercator(latitude) - (planePoint.Y - vector3D.Y);
        MathEx.Clamp(ref y, -3.14159290045661, 3.14159290045661);
        num1 = Coordinates.InverseMercator(y);
        longitude -= planePoint.Z - vector3D.Z;
        MathEx.Clamp(ref longitude, -1.0 * Math.PI, Math.PI);
      }
      else
      {
        double distance1;
        Vector3D spherePoint = cvs.GetSpherePoint(endX, endY, 1.0, out distance1);
        if (!CameraViewportSnapshot.WorldPoint3DIsOnSurface(vector3D) || spherePoint == Vector3D.Empty)
        {
          double distance2;
          cvs.GetSpherePoint(prevEndX, prevEndY, 1.0, out distance2);
          double radius = Math.Max(1.0, Math.Max(distance2, distance1)) + 1E-06;
          vector3D = cvs.GetSpherePoint(prevEndX, prevEndY, radius, out distance2);
          spherePoint = cvs.GetSpherePoint(endX, endY, radius, out distance1);
          if (radius > 1.0)
            vector3D = vector3D * 0.1 + spherePoint * 0.9;
          vector3D.Normalize();
          spherePoint.Normalize();
        }
        if (Math.Abs(vector3D.Y) < 0.9999 && Math.Abs(spherePoint.Y) < 0.9999)
        {
          double target = Math.Atan2(vector3D.Z, vector3D.X);
          double num2 = Math.Atan2(spherePoint.Z, spherePoint.X);
          longitude -= MathEx.GetClosestRepresentation(num2, target, Math.PI) - target;
        }
        Vector3D axisRotation = new Vector3D(-vector3D.Z, 0.0, vector3D.X);
        if (!axisRotation.Normalize())
          return;
        double sin = Vector3D.Cross(spherePoint, vector3D) * axisRotation;
        double cos = Math.Sqrt(Math.Min(1.0 - sin * sin, 1.0));
        Matrix4x4D matrix4x4D = Matrix4x4D.RotationAxis(axisRotation, sin, cos);
        num1 = Coordinates.World3DToGeo(Coordinates.GeoTo3D(longitude, latitude) * matrix4x4D).Latitude;
      }
      MathEx.Clamp(ref num1, -7.0 * Math.PI / 15.0, 7.0 * Math.PI / 15.0);
      cvs.CameraSnapshot.Latitude = num1;
      cvs.CameraSnapshot.Longitude = longitude;
      if (scndTouchPoint == null)
        return;
      cvs.SetCameraSnapshotAndRecalulate(cvs.CameraSnapshot);
      Vector2D vector2D1 = cvs.WorldToScreenPoint(fromGeo3d);
      Vector2D vector2D2 = cvs.WorldToScreenPoint(scndTouchPoint.WorldPosStart);
      Vector2D a = vector2D1 - vector2D2;
      if (!CameraViewportSnapshot.WorldPoint3DIsOnSurface(cvs.ScreenToWorldPoint(vector2D1.X, vector2D1.Y)))
      {
        scndTouchPoint.UsedForGesture = true;
      }
      else
      {
        Vector2D vector2D3 = new Vector2D(scndTouchPoint.PosCurrent.X, scndTouchPoint.PosCurrent.Y);
        Vector2D b = vector2D1 - vector2D3;
        double num2 = 1.0 / Math.Cos(cvs.CameraSnapshot.PivotAngle);
        a.Y *= num2;
        b.Y *= num2;
        double num3 = 1.0 / (b.Length() / a.Length());
        double angle = Vector2D.GetAngle(a, b);
        double num4 = 0.5;
        double num5 = cvs.CameraSnapshot.Distance * ((num3 - 1.0) * num4 + 1.0);
        MathEx.Clamp(ref num5, 7.83927971443699E-05, 8.0);
        cvs.CameraSnapshot.Distance = num5;
        double num6 = DefaultCameraController.WrapRadiansToZeroPlusMinusPi(cvs.CameraSnapshot.Rotation - DefaultCameraController.WrapRadiansToZeroPlusMinusPi(angle) * num4);
        cvs.CameraSnapshot.Rotation = num6;
      }
    }

    public static double WrapRadiansToZeroPlusMinusPi(double rad)
    {
      double num1 = Math.PI;
      double num2 = num1 * 2.0;
      while (rad > num2)
        rad -= num2;
      while (rad < -num2)
        rad += num2;
      if (rad > num1)
        rad = -(num2 - rad);
      if (rad < -num1)
        rad += num2;
      return rad;
    }

    public void MoveView(double startX, double startY, double endX, double endY)
    {
      if (this.flatEnabled)
      {
        Vector3D planePoint1 = this.GetPlanePoint(startX, startY, this.previousSceneState);
        Vector3D planePoint2 = this.GetPlanePoint(endX, endY, this.previousSceneState);
        if (planePoint1 == Vector3D.Empty || planePoint2 == Vector3D.Empty)
          return;
        double y = Coordinates.Mercator(this.targetLatitude) - (planePoint2.Y - planePoint1.Y);
        MathEx.Clamp(ref y, -3.14159290045661, 3.14159290045661);
        this.targetLatitude = Coordinates.InverseMercator(y);
        this.targetLongitude -= planePoint2.Z - planePoint1.Z;
        MathEx.Clamp(ref this.targetLongitude, -1.0 * Math.PI, Math.PI);
      }
      else
      {
        double distance1;
        Vector3D spherePoint1 = DefaultCameraController.GetSpherePoint(startX, startY, 1.0, this.previousSceneState, out distance1);
        double distance2;
        Vector3D left = DefaultCameraController.GetSpherePoint(endX, endY, 1.0, this.previousSceneState, out distance2);
        double num = Math.Max(distance1, distance2);
        if (num != 0.0)
        {
          double radius = num + 1E-06;
          spherePoint1 = DefaultCameraController.GetSpherePoint(startX, startY, radius, this.previousSceneState, out distance1);
          Vector3D spherePoint2 = DefaultCameraController.GetSpherePoint(endX, endY, radius, this.previousSceneState, out distance2);
          left = spherePoint1 * 0.9 + spherePoint2 * 0.1;
          spherePoint1.Normalize();
          left.Normalize();
        }
        if (Math.Abs(spherePoint1.Y) < 0.9999 && Math.Abs(left.Y) < 0.9999)
        {
          double target = Math.Atan2(spherePoint1.Z, spherePoint1.X);
          this.targetLongitude -= MathEx.GetClosestRepresentation(Math.Atan2(left.Z, left.X), target, Math.PI) - target;
        }
        Vector3D axisRotation = new Vector3D(-spherePoint1.Z, 0.0, spherePoint1.X);
        if (!axisRotation.Normalize())
          return;
        double sin = Vector3D.Cross(left, spherePoint1) * axisRotation;
        double cos = Math.Sqrt(Math.Min(1.0 - sin * sin, 1.0));
        this.targetLatitude = Coordinates.World3DToGeo(Coordinates.GeoTo3D(this.targetLongitude, this.targetLatitude) * Matrix4x4D.RotationAxis(axisRotation, sin, cos)).Latitude;
      }
      MathEx.Clamp(ref this.targetLatitude, -7.0 * Math.PI / 15.0, 7.0 * Math.PI / 15.0);
    }

    private void EnsureCameraConstraints()
    {
      MathEx.Clamp(ref this.targetPivotAngle, -4.0 * Math.PI / 9.0, 0.0);
      MathEx.Clamp(ref this.targetDistanceFromTarget, 7.83927971443699E-05, 8.0);
    }

    public override CameraSnapshot Update(SceneState state)
    {
      double num1 = 0.0;
      if (this.previousSceneState != null)
        num1 = (double) (state.ElapsedMilliseconds - this.previousSceneState.ElapsedMilliseconds);
      if (state != null)
        this.flatEnabled = state.FlatteningFactor == 1.0;
      MathEx.Clamp(ref num1, 16.0, 90.0);
      this.previousSceneState = state;
      double num2 = num1 / 60.05;
      MathEx.Clamp(ref num2, 0.001, 1.0);
      double updateStepSize = 1.0 / num2;
      this.UpdateZoom(updateStepSize);
      this.UpdateRotation(updateStepSize);
      this.UpdatePivot(updateStepSize);
      if (this.moveToHoveredObject)
      {
        this.moveToHoveredObject = false;
        if (state.HoveredObjectPosition != Vector3F.Empty)
        {
          Coordinates coordinates = Coordinates.World3DToGeo(state.HoveredObjectPosition.ToVector3D(), this.flatEnabled);
          this.targetLatitude = coordinates.Latitude;
          this.targetLongitude = coordinates.Longitude;
        }
      }
      this.UpdatePosition(updateStepSize);
      this.EnsureCameraConstraints();
      if (this.CameraTargetChanged != null)
      {
        CameraSnapshot targetCameraSnapshot = this.GetTargetCameraSnapshot();
        if (!targetCameraSnapshot.Equals(this.previousCameraTarget))
        {
          this.previousCameraTarget = targetCameraSnapshot;
          this.CameraTargetChanged(targetCameraSnapshot.Clone());
        }
      }
      return new CameraSnapshot(this.currentLatitude, this.currentLongitude, this.currentRotation, this.currentPivotAngle, this.currentDistanceFromTarget);
    }

    public override CameraSnapshot UpdateWithCurrentState(CameraSnapshot cs, SceneState st)
    {
      cs = base.UpdateWithCurrentState(cs, st);
      if (st != null && st.SceneCustomMap != null)
      {
        cs.Distance = DefaultCameraController.Clamped(cs.Distance, 0.05, 1.5);
        MathEx.Clamp(ref this.currentDistanceFromTarget, 0.05, 1.5);
        MathEx.Clamp(ref this.targetDistanceFromTarget, 0.05, 1.5);
        double num1 = CustomSpaceTransform.WorldScaleInDegrees * (Math.PI / 180.0);
        double num2 = CustomSpaceTransform.WorldScaleInDegrees * (Math.PI / 180.0);
        cs.Latitude = DefaultCameraController.Clamped(cs.Latitude, -num1, num1);
        MathEx.Clamp(ref this.targetLatitude, -num1, num1);
        MathEx.Clamp(ref this.currentLatitude, -num1, num1);
        cs.Longitude = DefaultCameraController.Clamped(cs.Longitude, -num2, num2);
        MathEx.Clamp(ref this.targetLongitude, -num2, num2);
        MathEx.Clamp(ref this.currentLongitude, -num2, num2);
      }
      return cs;
    }

    private static double Clamped(double val, double min, double max)
    {
      if (val < min)
        return min;
      if (val <= max)
        return val;
      else
        return max;
    }

    public override void OnFocus(CameraSnapshot previousSnapshot)
    {
      base.OnFocus(previousSnapshot);
      this.currentLatitude = this.targetLatitude = previousSnapshot.Latitude;
      this.currentLongitude = this.targetLongitude = previousSnapshot.Longitude;
      this.currentRotation = this.targetRotation = previousSnapshot.Rotation;
      this.currentDistanceFromTarget = this.targetDistanceFromTarget = previousSnapshot.Distance;
      this.currentPivotAngle = this.targetPivotAngle = previousSnapshot.PivotAngle;
    }

    private void UpdatePosition(double updateStepSize)
    {
      if (DefaultCameraController.SkipCameraSmoothing)
      {
        this.currentLatitude = this.targetLatitude;
        this.currentLongitude = this.targetLongitude;
      }
      else
      {
        double num1 = this.currentDistanceFromTarget / 3400.0;
        double num2 = this.targetLatitude - this.currentLatitude;
        double num3 = (this.flatEnabled ? this.targetLongitude : MathEx.GetClosestRepresentation(this.targetLongitude, this.currentLongitude, Math.PI)) - this.currentLongitude;
        if (Math.Abs(num2) > num1 || Math.Abs(num3) > num1)
        {
          this.currentLatitude += num2 / updateStepSize;
          this.currentLongitude = MathEx.GetNormalized(this.currentLongitude + num3 / updateStepSize, Math.PI);
        }
        else
        {
          if (this.currentLatitude == this.targetLatitude && this.currentLongitude == this.targetLongitude)
            return;
          this.currentLatitude = this.targetLatitude;
          this.currentLongitude = this.targetLongitude;
        }
      }
    }

    private void UpdatePivot(double updateStepSize)
    {
      double num;
      if (this.autoPivotEnabled)
        num = this.AutoPivotAngle;
      else if (this.currentDistanceFromTarget >= 8.0)
      {
        this.autoPivotEnabled = true;
        num = this.AutoPivotAngle;
      }
      else
        num = this.targetPivotAngle;
      if (Math.Abs(num - this.currentPivotAngle) > 0.00174532925199433)
      {
        if (DefaultCameraController.SkipCameraSmoothing)
          this.currentPivotAngle = num;
        else
          this.currentPivotAngle += (num - this.currentPivotAngle) / updateStepSize;
      }
      else
        this.currentPivotAngle = num;
    }

    private void UpdateRotation(double updateStepSize)
    {
      if (DefaultCameraController.SkipCameraSmoothing)
        this.currentRotation = this.targetRotation;
      else if (Math.Abs(this.targetRotation - this.currentRotation) > 0.00174532925199433)
        this.currentRotation += (this.targetRotation - this.currentRotation) / updateStepSize;
      else
        this.currentRotation = this.targetRotation;
    }

    private void UpdateZoom(double updateStepSize)
    {
      if (DefaultCameraController.SkipCameraSmoothing)
      {
        this.currentDistanceFromTarget = this.targetDistanceFromTarget;
      }
      else
      {
        if (Math.Abs(this.currentDistanceFromTarget - this.targetDistanceFromTarget) > this.currentDistanceFromTarget / 256.0)
          this.currentDistanceFromTarget += (this.targetDistanceFromTarget - this.currentDistanceFromTarget) / updateStepSize;
        else
          this.currentDistanceFromTarget = this.targetDistanceFromTarget;
        MathEx.Clamp(ref this.currentDistanceFromTarget, 7.83927971443699E-05, 8.0);
      }
    }

    private void ApplyMoveStyle(CameraMoveStyle style)
    {
      if (style != CameraMoveStyle.JumpTo)
        return;
      this.currentLatitude = this.targetLatitude;
      this.currentLongitude = this.targetLongitude;
      this.currentRotation = this.targetRotation;
      this.currentDistanceFromTarget = this.targetDistanceFromTarget;
      this.currentPivotAngle = this.targetPivotAngle;
    }

    public void MoveTo(CameraSnapshot targetCameraSnapShot, CameraMoveStyle style, bool retainPivotAngle = false)
    {
      this.targetLatitude = targetCameraSnapShot.Latitude;
      this.targetLongitude = targetCameraSnapShot.Longitude;
      this.targetRotation = targetCameraSnapShot.Rotation;
      this.targetDistanceFromTarget = targetCameraSnapShot.Distance;
      this.targetPivotAngle = targetCameraSnapShot.PivotAngle;
      if (retainPivotAngle)
        this.autoPivotEnabled = Math.Abs(this.targetPivotAngle - DefaultCameraController.GetAutoPivotAngle(this.targetDistanceFromTarget)) < 1E-06;
      this.ApplyMoveStyle(style);
    }

    public void MoveTo(double south, double north, double west, double east, CameraMoveStyle style)
    {
      if (south > north)
        return;
      south = Math.Max(south, -1.0 * Math.PI / 2.0);
      north = Math.Min(north, Math.PI / 2.0);
      east = MathEx.GetNormalized(east, Math.PI);
      west = MathEx.GetNormalized(west, Math.PI);
      if (east < west)
        east += 2.0 * Math.PI;
      double num1 = Math.Min(east - west, Math.PI / 2.0) / 2.0;
      double val1;
      if (this.flatEnabled)
      {
        north = Coordinates.Mercator(north);
        south = Coordinates.Mercator(south);
        val1 = north - south;
      }
      else
      {
        val1 = Math.Min(north - south, Math.PI / 2.0);
        double d = 0.0;
        if (south > 0.0)
          d = south;
        else if (north < 0.0)
          d = north;
        num1 = Math.Sin(num1) * Math.Cos(d);
      }
      double num2 = Math.Max(val1, num1);
      double num3 = Math.Exp(-num2) / 8.0;
      this.targetLatitude = ((1.0 + num3) * south + (1.0 - num3) * north) / 2.0;
      if (this.flatEnabled)
        this.targetLatitude = Coordinates.InverseMercator(this.targetLatitude);
      this.targetLongitude = (west + east) / 2.0;
      this.targetRotation = 0.0;
      this.targetDistanceFromTarget = 2.5 * num2;
      this.targetPivotAngle = 0.0;
      this.ApplyMoveStyle(style);
    }

    public void MoveTo(List<Vector3D> locations, bool zoomInAndLookNorth, CameraMoveStyle style)
    {
      if (locations.Count < 1 || !zoomInAndLookNorth && this.ScreenContains(this.previousSceneState.ViewProjection, this.previousSceneState.CameraPosition, locations))
        return;
      if (zoomInAndLookNorth)
        this.targetRotation = 0.0;
      Cap cap = Cap.Construct(this.flatEnabled);
      if (!cap.SetCenter(locations))
      {
        if (this.targetDistanceFromTarget >= 1.0)
          return;
        this.targetDistanceFromTarget = 1.0;
      }
      else
      {
        Vector3D center = cap.Center;
        if (zoomInAndLookNorth || !this.ScreenWillContain(center, locations))
        {
          cap.SetExtent(locations);
          if (cap.IsEmpty)
            return;
          double distance = this.ComputeDistance(cap.ProjectionRadius, 0.025);
          if (zoomInAndLookNorth || distance > this.targetDistanceFromTarget)
            this.targetDistanceFromTarget = distance;
        }
        this.SetTarget(ref center);
        this.ApplyMoveStyle(style);
      }
    }

    public void MoveTo(Cap cap, CameraMoveStyle style)
    {
      if (cap.IsEmpty)
        return;
      Vector3D center = cap.Center;
      double num;
      if (cap.IsWholeWorld)
      {
        num = 1.0;
        this.targetRotation = 0.0;
      }
      else
        num = this.ComputeDistance(cap.ProjectionRadius, 0.4);
      this.SetTarget(ref center);
      this.targetDistanceFromTarget = num;
      this.ApplyMoveStyle(style);
    }

    private double ComputeDistance(double extent, double lowerBound)
    {
      return Math.Min(Math.Max(lowerBound, 4.0 * extent), this.flatEnabled ? 6.0 : 1.5);
    }

    private void SetTarget(ref Vector3D target)
    {
      Coordinates coordinates = Coordinates.World3DToGeo(target, this.flatEnabled);
      this.targetLatitude = coordinates.Latitude;
      this.targetLongitude = coordinates.Longitude;
    }

    private bool IsVisible(ref Matrix4x4D projection, DefaultCameraController.HorizonProximityTester horizonTester, Vector3D point)
    {
      Vector4D vector4D = projection.Transform((Vector4D) point);
      if (vector4D.W <= 0.0 || Math.Abs(vector4D.X) >= vector4D.W || (Math.Abs(vector4D.Y) >= vector4D.W || vector4D.Z <= 0.0) || vector4D.Z >= vector4D.W)
        return false;
      if (!this.flatEnabled)
        return !horizonTester.IsNearTheHorizon(ref point);
      else
        return true;
    }

    private bool ScreenContains(Matrix4x4D projection, Vector3D cameraPosition, List<Vector3D> locations)
    {
      DefaultCameraController.HorizonProximityTester horizonTester = new DefaultCameraController.HorizonProximityTester(ref cameraPosition);
      for (int index = 0; index < locations.Count; ++index)
      {
        if (!this.IsVisible(ref projection, horizonTester, locations[index]))
          return false;
      }
      return true;
    }

    private bool ScreenWillContain(Vector3D target, List<Vector3D> locations)
    {
      CameraSnapshot camera = this.previousSceneState.CameraSnapshot.Clone();
      Coordinates coordinates = Coordinates.World3DToGeo(target, this.flatEnabled);
      camera.Latitude = coordinates.Latitude;
      camera.Longitude = coordinates.Longitude;
      Vector3D cameraPosition;
      return this.ScreenContains(CameraStep.GetHypotheticalCameraParameters(this.previousSceneState, camera, out cameraPosition), cameraPosition, locations);
    }

    public CameraSnapshot GetTargetCameraSnapshot()
    {
      return new CameraSnapshot(this.targetLatitude, this.targetLongitude, this.targetRotation, this.PivotAngleTarget, this.targetDistanceFromTarget);
    }

    private class HorizonProximityTester
    {
      private const double nearlyZero = 0.0001;
      private Vector3D cameraPosition;
      private double oc2;
      private bool degenerate;
      private Vector3D M;

      public HorizonProximityTester(ref Vector3D cameraPos)
      {
        this.cameraPosition = cameraPos;
        this.oc2 = this.cameraPosition.LengthSq();
        this.degenerate = this.oc2 < 1.0;
        if (this.oc2 < 1.0)
          return;
        this.M = this.cameraPosition / this.oc2;
      }

      public bool IsNearTheHorizon(ref Vector3D point)
      {
        if (this.degenerate)
          return true;
        Vector3D vector3D = point;
        if (!vector3D.Orthonormalize(this.cameraPosition / Math.Sqrt(this.oc2)))
          return true;
        double num = this.M * this.M;
        if (num >= 1.0)
          return true;
        Vector3D left = vector3D * Math.Sqrt(1.0 - num) + this.M - this.cameraPosition;
        Vector3D right = point - this.cameraPosition;
        return Vector3D.Cross(ref left, ref right).LengthSq() < left.LengthSq() * right.LengthSq() * 0.0001;
      }
    }
  }
}
