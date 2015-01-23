using Microsoft.Data.Visualization.VisualizationCommon;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [XmlInclude(typeof (LayerLegendDecoratorModel))]
  [XmlInclude(typeof (ChartDecoratorModel))]
  [XmlInclude(typeof (TimeDecoratorModel))]
  [XmlInclude(typeof (LabelDecoratorModel))]
  public abstract class DecoratorContentBase : CompositePropertyChangeNotificationBase
  {
  }
}
