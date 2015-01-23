using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class ColorExtensions
  {
    public static Color4F ToColor4F(this System.Drawing.Color c)
    {
      return new Color4F((float) c.A / (float) byte.MaxValue, (float) c.R / (float) byte.MaxValue, (float) c.G / (float) byte.MaxValue, (float) c.B / (float) byte.MaxValue);
    }

    public static Color4F ToColor4F(this System.Windows.Media.Color c)
    {
      return new Color4F((float) c.A / (float) byte.MaxValue, (float) c.R / (float) byte.MaxValue, (float) c.G / (float) byte.MaxValue, (float) c.B / (float) byte.MaxValue);
    }
  }
}
