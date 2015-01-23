// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.MouseInputEvent
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.Engine
{
  public class MouseInputEvent : InputEvent
  {
    public Point Position { get; private set; }

    public Vector PositionDelta { get; private set; }

    public bool LeftButtonPressed { get; private set; }

    public bool RightButtonPressed { get; private set; }

    public MouseInputEvent(MouseEventType eventType, Point cursorPosition, Vector positionDelta, ModifierKeys modifierKeys, bool leftPressed, bool rightPressed)
      : base((InputEventType) eventType, modifierKeys)
    {
      this.Position = cursorPosition;
      this.PositionDelta = positionDelta;
      this.LeftButtonPressed = leftPressed;
      this.RightButtonPressed = rightPressed;
    }

    internal override bool ProcessEvent(UIController controller)
    {
      switch (this.Type)
      {
        case InputEventType.LeftMouseDown:
          return controller.LeftMouseDown(this);
        case InputEventType.LeftMouseUp:
          return controller.LeftMouseUp(this);
        case InputEventType.RightMouseDown:
          return controller.RightMouseDown(this);
        case InputEventType.RightMouseUp:
          return controller.RightMouseUp(this);
        case InputEventType.MouseMove:
          return controller.MouseMove(this);
        case InputEventType.MouseLeave:
          return controller.MouseLeave(this);
        case InputEventType.DoubleClick:
          return controller.DoubleClick(this);
        default:
          return false;
      }
    }
  }
}
