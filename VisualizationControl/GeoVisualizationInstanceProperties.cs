using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class GeoVisualizationInstanceProperties
  {
    [XmlAttribute("InstanceId")]
    public string ModelId { get; set; }

    [XmlElement("Annotation")]
    public AnnotationTemplateModel Annotation { get; set; }

    [XmlElement("ColorSet")]
    public bool ColorSet { get; set; }

    [XmlElement("Color")]
    public Color4F Color { get; set; }

    public GeoVisualizationInstanceProperties()
    {
    }

    public GeoVisualizationInstanceProperties(string modelId)
    {
      this.ModelId = modelId;
    }

    public bool IsEmpty()
    {
      if (this.Annotation == null)
        return !this.ColorSet;
      else
        return false;
    }

    public void Repair()
    {
      if (this.Annotation == null)
        return;
      this.Annotation.Repair();
    }

    internal void Shutdown()
    {
      if (this.Annotation == null)
        return;
      this.Annotation.Shutdown();
    }
  }
}
