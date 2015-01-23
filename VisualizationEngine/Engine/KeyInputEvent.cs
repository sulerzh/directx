// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.KeyInputEvent
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Windows.Input;

namespace Microsoft.Data.Visualization.Engine
{
  public class KeyInputEvent : InputEvent
  {
    public Key Key { get; private set; }

    public KeyInputEvent(KeyEventType eventType, Key key, ModifierKeys modifierKeys)
      : base((InputEventType) eventType, modifierKeys)
    {
      this.Key = key;
    }

    internal override bool ProcessEvent(UIController controller)
    {
      switch (this.Type)
      {
        case InputEventType.KeyDown:
          return controller.KeyDown(this);
        case InputEventType.KeyUp:
          return controller.KeyUp(this);
        default:
          return false;
      }
    }
  }
}
