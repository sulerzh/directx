using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.WpfExtensions;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ColorPickerButtonImageConverter : SimpleOneWayConverter<Color4F, ImageSource>
  {
    protected override bool TryConvert(Color4F source, out ImageSource result)
    {
      result = (ImageSource) new DrawingImage()
      {
        Drawing = (Drawing) new GeometryDrawing()
        {
          Brush = (Brush) new SolidColorBrush(source.ToWindowsColor()),
          Geometry = (Geometry) new RectangleGeometry()
          {
            Rect = new System.Windows.Rect(0.0, 0.0, 16.0, 16.0)
          }
        }
      };
      return true;
    }
  }
}
