// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.GestureEndInputEvent
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public class GestureEndInputEvent : GestureInputEvent
  {
    public GestureEndInputEvent(Vector2D translationDelta, Vector2D scaleDelta, double rotationDelta, bool twoTouchPoints)
      : base(InputEventType.GestureEnd, translationDelta, Vector2D.Empty, scaleDelta, rotationDelta, twoTouchPoints)
    {
    }

    internal override bool ProcessEvent(UIController controller)
    {
      return controller.GestureEnd(this);
    }
  }
}
