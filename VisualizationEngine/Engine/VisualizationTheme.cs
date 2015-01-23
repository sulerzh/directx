// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.VisualizationTheme
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  [XmlRoot("Theme", IsNullable = false, Namespace = "http://microsoft.data.visualization.engine.Theme/1.0")]
  [Serializable]
  public class VisualizationTheme
  {
    private List<ColorOperation> globeColorOperations = new List<ColorOperation>();
    private LayerRenderingParameters layerParameters = new LayerRenderingParameters();
    private List<Color4F> colorsForVisuals = new List<Color4F>();
    private List<double> colorStepsForVisuals = new List<double>();

    public LightingRig Lighting { get; set; }

    public Color4F BackgroundTopColor { get; set; }

    public Color4F BackgroundBottomColor { get; set; }

    public bool GlobeGlowEnabled { get; set; }

    public Color4F GlobeGlowColor { get; set; }

    public float GlobeGlowReflectanceIndex { get; set; }

    public float GlobeGlowPower { get; set; }

    public float GlobeGlowFactor { get; set; }

    [XmlArrayItem("InvertTransform", typeof (InvertTransform))]
    [XmlArrayItem("MultiplyTransform", typeof (MultiplyTransform))]
    [XmlArray("ColorOperations")]
    [XmlArrayItem("ColorMap", typeof (ColorMap))]
    [XmlArrayItem("ColorReplace", typeof (ColorReplace))]
    [XmlArrayItem("HueTransform", typeof (HueTransform))]
    [XmlArrayItem("ColorTransform", typeof (ColorLayerTransform))]
    [XmlArrayItem("ScreenTransform", typeof (ScreenColorTransform))]
    [XmlArrayItem("SaturationTransform", typeof (SaturationTransform))]
    public List<ColorOperation> GlobeColorOperations
    {
      get
      {
        return this.globeColorOperations;
      }
    }

    public Color4F ShadowColor { get; set; }

    public AnnotationStyle AnnotationStyle { get; set; }

    public LayerRenderingParameters LayerParameters
    {
      get
      {
        return this.layerParameters;
      }
    }

    [XmlArray("ColorsForVisuals")]
    [XmlArrayItem("Color", typeof (Color4F))]
    public List<Color4F> ColorsForVisuals
    {
      get
      {
        return this.colorsForVisuals;
      }
    }

    [XmlArray("ColorStepsForVisuals")]
    [XmlArrayItem("Step", typeof (double))]
    public List<double> ColorStepsForVisuals
    {
      get
      {
        return this.colorStepsForVisuals;
      }
    }

    public double InstanceVisualWidthFactor { get; set; }

    public double InstanceVisualHeightFactor { get; set; }

    public static VisualizationTheme ReadXml(XmlReader reader)
    {
      return (VisualizationTheme) new XmlSerializer(typeof (VisualizationTheme)).Deserialize(reader);
    }

    public void WriteXml(XmlWriter writer)
    {
      new XmlSerializer(typeof (VisualizationTheme)).Serialize(writer, (object) this);
    }
  }
}
