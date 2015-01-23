// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.DefaultCameraUIController
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.Engine
{
  internal class DefaultCameraUIController : UIController
  {
    private double PixelsPerRadianRatio = 200.0 * Math.PI;
    private CameraViewportSnapshot cameraViewportIter = new CameraViewportSnapshot();
    private const double MinTranslationSpeedForAccumulation = 3.0;
    private const double MinAccumulatedTranslation = 30.0;
    private DefaultCameraController cameraController;
    private bool dragging;
    private bool pivoting;
    private bool wasLastGestureScaling;
    private Point lastCursorPosition;
    private Vector2D accumulatedTwoTouchTranslationDelta;
    private Vector3D mouseDownTouchPoint3D;

    public DefaultCameraUIController(DefaultCameraController controller)
    {
      this.cameraController = controller;
    }

    private bool KeyDownAltOn(KeyInputEvent e)
    {
      switch (e.Key)
      {
        case Key.Prior:
        case Key.Subtract:
        case Key.OemMinus:
          this.cameraController.ZoomOut();
          return true;
        case Key.Next:
        case Key.Add:
        case Key.OemPlus:
          this.cameraController.ZoomIn();
          return true;
        case Key.Left:
          this.cameraController.Rotate(CameraRotation.Left);
          return true;
        case Key.Up:
          this.cameraController.Rotate(CameraRotation.Up);
          return true;
        case Key.Right:
          this.cameraController.Rotate(CameraRotation.Right);
          return true;
        case Key.Down:
          this.cameraController.Rotate(CameraRotation.Down);
          return true;
        default:
          return false;
      }
    }

    private bool KeyDownAltOff(KeyInputEvent e)
    {
      switch (e.Key)
      {
        case Key.Prior:
        case Key.Subtract:
        case Key.OemMinus:
          this.cameraController.ZoomOut();
          return true;
        case Key.Next:
        case Key.Add:
        case Key.OemPlus:
          this.cameraController.ZoomIn();
          return true;
        case Key.Home:
          this.cameraController.ResetView();
          return true;
        case Key.Left:
          this.cameraController.Move(1.0, 0.0);
          return true;
        case Key.Up:
          this.cameraController.Move(0.0, 1.0);
          return true;
        case Key.Right:
          this.cameraController.Move(-1.0, 0.0);
          return true;
        case Key.Down:
          this.cameraController.Move(0.0, -1.0);
          return true;
        default:
          return false;
      }
    }

    public override bool KeyDown(KeyInputEvent e)
    {
      if (e == null)
        return false;
      if ((e.ModifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
        return this.KeyDownAltOn(e);
      else
        return this.KeyDownAltOff(e);
    }

    public override bool LeftMouseDown(MouseInputEvent e)
    {
      if (e == null)
        return false;
      this.mouseDownTouchPoint3D = this.cameraController.UpdateViewportSnapshot(this.cameraViewportIter).GetWorldPoint3D(e.Position.X, e.Position.Y);
      if ((e.ModifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
        this.pivoting = true;
      else
        this.dragging = true;
      this.lastCursorPosition = e.Position;
      return true;
    }

    public override bool RightMouseDown(MouseInputEvent e)
    {
      if (e == null)
        return false;
      this.lastCursorPosition = e.Position;
      return true;
    }

    public override bool LeftMouseUp(MouseInputEvent e)
    {
      this.dragging = false;
      this.pivoting = false;
      return true;
    }

    public override bool DoubleClick(MouseInputEvent e)
    {
      if (e == null)
        return false;
      this.cameraController.MoveToHoveredObject(e.Position.X, e.Position.Y);
      this.cameraController.ZoomIn(2.0);
      return true;
    }

    public override bool MouseMove(MouseInputEvent e)
    {
      if (e == null)
        return false;
      Point position = e.Position;
      if (e.LeftButtonPressed)
      {
        if (this.dragging)
          this.cameraController.MovePointToMatchScreenLocation(this.cameraViewportIter, this.mouseDownTouchPoint3D, position.X, position.Y, this.lastCursorPosition.X, this.lastCursorPosition.Y, (CameraSnapshot) null, (SingleTouchPoint) null);
        else if (this.pivoting)
        {
          this.cameraController.RotationAngleTarget = this.cameraController.RotationAngleTarget + (position.X - this.lastCursorPosition.X) / this.PixelsPerRadianRatio;
          this.cameraController.PivotAngleTarget = this.cameraController.PivotAngleTarget + (position.Y - this.lastCursorPosition.Y) / this.PixelsPerRadianRatio;
        }
      }
      else
      {
        this.dragging = false;
        this.pivoting = false;
      }
      this.lastCursorPosition = position;
      return true;
    }

    public override bool MouseWheel(MouseWheelInputEvent e)
    {
      if (e == null)
        return false;
      if (e.WheelDelta != 0)
      {
        Point point = this.lastCursorPosition;
        Vector3D worldPos = this.cameraController.BeginKeepCameraTargetted(this.cameraViewportIter, point.X, point.Y);
        if (e.WheelDelta < 0)
          this.cameraController.ZoomOut();
        else
          this.cameraController.ZoomIn();
        if (this.cameraController.AutoPivotEnabled)
          this.cameraController.PivotAngleTarget = DefaultCameraController.GetAutoPivotAngle(this.cameraController.DistanceToTarget);
        this.cameraController.EndKeepCameraTargetted(this.cameraViewportIter, worldPos, point.X, point.Y);
      }
      return true;
    }

    public override bool Tap(TapInputEvent tapIn)
    {
      if (tapIn == null || !(tapIn.TapCount == 2 & !tapIn.HasOutstandingTapAndHold))
        return false;
      this.cameraController.MoveToHoveredObject(tapIn.Position.X, tapIn.Position.Y);
      this.cameraController.ZoomIn(2.0);
      return true;
    }

    private void UpdateGestureState(GestureInputEvent e)
    {
      foreach (SingleTouchPoint singleTouchPoint in e.MultiTouchCurrentPostions)
      {
        if (!singleTouchPoint.HasFirstWorldSet)
        {
          Vector3D worldPoint3D = this.cameraController.UpdateViewportSnapshot(this.cameraViewportIter).GetWorldPoint3D(singleTouchPoint.PosCurrent.X, singleTouchPoint.PosCurrent.Y);
          singleTouchPoint.WorldPosCur = worldPoint3D;
          singleTouchPoint.WorldPosStart = worldPoint3D;
          singleTouchPoint.HasFirstWorldSet = true;
        }
      }
    }

    public override bool Gesture(GestureInputEvent e)
    {
      if (e == null)
        return false;
      this.UpdateGestureState(e);
      bool flag1 = !this.cameraController.AutoPivotEnabled;
      if (this.cameraController.AutoPivotEnabled && e.TwoTouchPoints && Math.Abs(e.TranslationDelta.Y) > 3.0)
      {
        this.accumulatedTwoTouchTranslationDelta += e.TranslationDelta;
        if (Math.Abs(this.accumulatedTwoTouchTranslationDelta.Y) > 30.0)
          flag1 = true;
      }
      bool flag2 = e.TwoTouchPoints && e.ScaleDelta.X > 0.0;
      SingleTouchPoint singleTouchPoint = (SingleTouchPoint) null;
      SingleTouchPoint scndTouchPoint = (SingleTouchPoint) null;
      if (e.MultiTouchCurrentPostions.Count >= 1)
        singleTouchPoint = e.MultiTouchCurrentPostions[0];
      if (e.MultiTouchCurrentPostions.Count >= 2)
      {
        scndTouchPoint = e.MultiTouchCurrentPostions[1];
        if (!CameraViewportSnapshot.WorldPoint3DIsOnSurface(scndTouchPoint.WorldPosStart))
          scndTouchPoint = (SingleTouchPoint) null;
        if (singleTouchPoint != null && (!CameraViewportSnapshot.WorldPoint3DIsOnSurface(singleTouchPoint.WorldPosStart) || !CameraViewportSnapshot.WorldPoint3DIsOnSurface(singleTouchPoint.WorldPosCur) || singleTouchPoint.UsedForGesture))
          scndTouchPoint = (SingleTouchPoint) null;
        if (scndTouchPoint != null && scndTouchPoint.UsedForGesture)
          scndTouchPoint = (SingleTouchPoint) null;
      }
      Vector3D worldPos = singleTouchPoint != null ? singleTouchPoint.WorldPosStart : Vector3D.Empty;
      if (e.TwoTouchPoints && Math.Abs(e.TranslationDelta.Y) > Math.Abs(e.TranslationDelta.X) && flag1)
      {
        this.cameraController.PivotAngleTarget = this.cameraController.PivotAngleTarget - e.TranslationDelta.Y * 3.0 / this.PixelsPerRadianRatio;
        this.cameraController.AutoPivotEnabled = false;
        if (singleTouchPoint != null)
          singleTouchPoint.UsedForGesture = true;
        if (scndTouchPoint != null)
        {
          scndTouchPoint.UsedForGesture = true;
          scndTouchPoint = (SingleTouchPoint) null;
        }
      }
      if (scndTouchPoint == null)
      {
        if (flag2)
          this.cameraController.DistanceToTarget *= 2.0 - Math.Max(0.5, Math.Min(1.5, Math.Abs(e.ScaleDelta.X - 1.0) > Math.Abs(e.ScaleDelta.Y - 1.0) ? e.ScaleDelta.X : e.ScaleDelta.Y));
        if (e.RotationDelta != 0.0)
          this.cameraController.RotationAngleTarget = this.cameraController.RotationAngleTarget - e.RotationDelta;
      }
      if (singleTouchPoint != null)
        this.cameraController.MovePointToMatchScreenLocation(this.cameraViewportIter, worldPos, singleTouchPoint.PosCurrent.X, singleTouchPoint.PosCurrent.Y, singleTouchPoint.PosCurrent.X - e.TranslationDelta.X, singleTouchPoint.PosCurrent.Y - e.TranslationDelta.Y, this.cameraController.GetTargetCameraSnapshot(), scndTouchPoint);
      this.wasLastGestureScaling = flag2;
      return true;
    }

    public override bool GestureEnd(GestureEndInputEvent gestureIn)
    {
      this.wasLastGestureScaling = false;
      this.accumulatedTwoTouchTranslationDelta = Vector2D.Empty;
      return true;
    }
  }
}
