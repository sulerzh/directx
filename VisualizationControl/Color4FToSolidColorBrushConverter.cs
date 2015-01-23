using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.WpfExtensions;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class Color4FToSolidColorBrushConverter : SimpleOneWayConverter<Color4F, SolidColorBrush>
  {
    protected override bool TryConvert(Color4F source, out SolidColorBrush result)
    {
      result = new SolidColorBrush(source.ToWindowsColor());
      return true;
    }
  }
}
