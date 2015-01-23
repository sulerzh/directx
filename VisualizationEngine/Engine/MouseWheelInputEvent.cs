// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.MouseWheelInputEvent
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.Engine
{
  public class MouseWheelInputEvent : MouseInputEvent
  {
    public int WheelDelta { get; private set; }

    public MouseWheelInputEvent(int wheelDelta, Point cursorPosition, ModifierKeys modifierKeys, bool leftPressed, bool rightPressed)
      : base(MouseEventType.Wheel, cursorPosition, new Vector(0.0, 0.0), modifierKeys, leftPressed, rightPressed)
    {
      this.WheelDelta = wheelDelta;
    }

    internal override bool ProcessEvent(UIController controller)
    {
      return controller.MouseWheel(this);
    }
  }
}
