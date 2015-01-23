// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LayerUIController
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.Engine
{
  internal class LayerUIController : UIController
  {
    private double SelectionThreshold = 5.0;
    private LayerStep layerStep;
    private Point mouseDownPosition;

    public static SelectionDrawingMode SelectionDrawingMode { get; private set; }

    public LayerUIController(LayerStep step)
    {
      this.layerStep = step;
    }

    public override bool LeftMouseDown(MouseInputEvent e)
    {
      if (e != null)
        this.mouseDownPosition = e.Position;
      return false;
    }

    public override bool RightMouseDown(MouseInputEvent e)
    {
      if (e != null)
        this.mouseDownPosition = e.Position;
      return false;
    }

    public override bool LeftMouseUp(MouseInputEvent e)
    {
      this.Select(e, false);
      return false;
    }

    public override bool RightMouseUp(MouseInputEvent e)
    {
      this.Select(e, true);
      return false;
    }

    public override bool Tap(TapInputEvent tapIn)
    {
      this.layerStep.SetSelectionMode(tapIn.HasOutstandingTapAndHold ? SelectionMode.Add : SelectionMode.Single);
      return false;
    }

    private void Select(MouseInputEvent e, bool rightClick)
    {
      if ((this.mouseDownPosition - e.Position).Length >= this.SelectionThreshold)
        return;
      this.layerStep.SetSelectionMode((e.ModifierKeys & ModifierKeys.Control) != ModifierKeys.Control ? (rightClick ? SelectionMode.SingleRightClick : SelectionMode.Single) : SelectionMode.Add);
    }

    public override bool KeyUp(KeyInputEvent e)
    {
      if (e == null || e.Key != Key.Escape)
        return false;
      this.layerStep.DeselectAll();
      return true;
    }
  }
}
