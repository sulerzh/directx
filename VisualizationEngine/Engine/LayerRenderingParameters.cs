// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LayerRenderingParameters
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  public class LayerRenderingParameters
  {
    public float HeatMapAlpha { get; set; }

    public float HeatMapCircleOfInfluence { get; set; }

    public HeatMapBlendMode HeatMapBlendMode { get; set; }

    public bool HeatMapVariableCircleOfInfluence { get; set; }

    public float HeatMapMaxValueForAlpha { get; set; }

    public bool HeatMapGaussianBlurEnable { get; set; }

    public float InstanceFixedScaleFactor { get; set; }

    public float InstanceVariableScaleFactor { get; set; }

    public Color4F InnerOutlineColor { get; set; }

    public Color4F OuterOutlineColor { get; set; }

    public float OutlineWidth { get; set; }

    public float OutlineOffset { get; set; }

    public Color4F RegionOutlineColor { get; set; }

    public bool RegionBrightnessEnabled { get; set; }

    public bool Wireframe { get; set; }

    public LayerRenderingParameters()
    {
      this.InstanceFixedScaleFactor = 1f;
      this.InstanceVariableScaleFactor = 1f;
      this.InnerOutlineColor = new Color4F(1f, 1f, 1f, 1f);
      this.OuterOutlineColor = new Color4F(1f, 0.0f, 0.0f, 0.0f);
      this.OutlineWidth = 2f;
      this.OutlineOffset = 0.0f;
      this.RegionOutlineColor = new Color4F(1f, 1f, 1f, 1f);
      this.RegionBrightnessEnabled = false;
      this.HeatMapAlpha = 0.85f;
      this.HeatMapMaxValueForAlpha = 10f;
      this.HeatMapCircleOfInfluence = 3.0f / 500.0f;
      this.HeatMapBlendMode = HeatMapBlendMode.Add;
      this.HeatMapGaussianBlurEnable = false;
      this.HeatMapVariableCircleOfInfluence = false;
    }
  }
}
