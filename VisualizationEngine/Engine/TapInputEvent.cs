// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TapInputEvent
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.Engine
{
  public class TapInputEvent : InputEvent
  {
    public bool HasOutstandingTapAndHold { get; private set; }

    public int TapCount { get; private set; }

    public Point Position { get; private set; }

    public TapInputEvent(TouchEventType eventType, Point position, bool hasOutstandingTapAndHold, int tapCount)
      : base((InputEventType) eventType, ModifierKeys.None)
    {
      this.HasOutstandingTapAndHold = hasOutstandingTapAndHold;
      this.TapCount = tapCount;
      this.Position = position;
    }

    internal override bool ProcessEvent(UIController controller)
    {
      switch (this.Type)
      {
        case InputEventType.Tap:
          return controller.Tap(this);
        case InputEventType.TapAndHoldEnter:
          return controller.TapAndHoldEnter(this);
        case InputEventType.TapAndHoldLeave:
          return controller.TapAndHoldLeave(this);
        default:
          return false;
      }
    }
  }
}
