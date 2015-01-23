using System;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class GeoResolution
  {
    [XmlAttribute("lt")]
    public double Lat;
    [XmlAttribute("ln")]
    public double Lon;

    public string FormattedAddress { get; set; }

    public string[] AddressFields { get; set; }

    [XmlAttribute("et")]
    public string EntityType { get; set; }

    internal void LogString(StringBuilder sb, string separator)
    {
      sb.AppendFormat("Provider EntityType:{0}{1}{0}", (object) separator, (object) this.EntityType);
      sb.AppendFormat("Provider FormattedAddress:{0}{1}{0}", (object) separator, (object) this.FormattedAddress);
      for (int index = 0; index < 6; ++index)
      {
        if (this.AddressFields[index] != null)
          sb.AppendFormat("Provider {1}:{0}{2}{0}", (object) separator, (object) ((object) (GeoEntityField.GeoEntityLevel) index).ToString(), (object) this.AddressFields[index]);
      }
    }
  }
}
