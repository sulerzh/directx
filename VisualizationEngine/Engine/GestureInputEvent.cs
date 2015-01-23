// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.GestureInputEvent
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.Engine
{
  public class GestureInputEvent : InputEvent
  {
    public Vector2D TranslationDelta { get; private set; }

    public double RotationDelta { get; private set; }

    public Vector2D ScaleDelta { get; private set; }

    public bool TwoTouchPoints { get; private set; }

    public Vector2D Center { get; private set; }

    public List<SingleTouchPoint> MultiTouchCurrentPostions { get; set; }

    public GestureInputEvent(Vector2D translationDelta, Vector2D scaleDelta, Vector2D center, double rotationDelta, bool twoTouchPoints)
      : this(InputEventType.GestureDelta, translationDelta, scaleDelta, center, rotationDelta, twoTouchPoints)
    {
    }

    protected GestureInputEvent(InputEventType eventType, Vector2D translationDelta, Vector2D scaleDelta, Vector2D center, double rotationDelta, bool twoTouchPoints)
      : base(eventType, ModifierKeys.None)
    {
      this.TranslationDelta = translationDelta;
      this.RotationDelta = rotationDelta;
      this.ScaleDelta = scaleDelta;
      this.TwoTouchPoints = twoTouchPoints;
      this.Center = center;
    }

    internal override bool ProcessEvent(UIController controller)
    {
      return controller.Gesture(this);
    }
  }
}
