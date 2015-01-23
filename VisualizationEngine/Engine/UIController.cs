// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.UIController
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

namespace Microsoft.Data.Visualization.Engine
{
  public abstract class UIController
  {
    public virtual bool KeyDown(KeyInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool KeyUp(KeyInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool LeftMouseDown(MouseInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool LeftMouseUp(MouseInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool RightMouseDown(MouseInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool RightMouseUp(MouseInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool DoubleClick(MouseInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool MouseWheel(MouseWheelInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool MouseMove(MouseInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool MouseLeave(MouseInputEvent mouseIn)
    {
      return false;
    }

    public virtual bool Gesture(GestureInputEvent gestureIn)
    {
      return false;
    }

    public virtual bool GestureEnd(GestureEndInputEvent gestureIn)
    {
      return false;
    }

    public virtual bool Tap(TapInputEvent tapIn)
    {
      return false;
    }

    public virtual bool TapAndHoldEnter(TapInputEvent tapIn)
    {
      return false;
    }

    public virtual bool TapAndHoldLeave(TapInputEvent tapIn)
    {
      return false;
    }
  }
}
