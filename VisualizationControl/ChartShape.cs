using Microsoft.Data.Visualization.Engine;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public sealed class ChartShape
  {
    public static ChartShape Triangle { get; private set; }

    public static ChartShape Square { get; private set; }

    public static ChartShape Circle { get; private set; }

    public static ChartShape Pentagon { get; private set; }

    public static ChartShape TwelvePointStar { get; private set; }

    public string IconPath { get; private set; }

    public InstancedShape Shape { get; private set; }

    public string Name { get; private set; }

    public static List<ChartShape> ShapesList { get; private set; }

    static ChartShape()
    {
      ChartShape.Triangle = new ChartShape(InstancedShape.Triangle, "pack://application:,,,/VisualizationControl;component/Images/triangle.PNG", Resources.ShapesGallery_Triangle);
      ChartShape.Square = new ChartShape(InstancedShape.Square, "pack://application:,,,/VisualizationControl;component/Images/square.PNG", Resources.ShapesGallery_Square);
      ChartShape.Circle = new ChartShape(InstancedShape.Circle, "pack://application:,,,/VisualizationControl;component/Images/circle.PNG", Resources.ShapesGallery_Circle);
      ChartShape.Pentagon = new ChartShape(InstancedShape.Pentagon, "pack://application:,,,/VisualizationControl;component/Images/pentagon.PNG", Resources.ShapesGallery_Pentagon);
      ChartShape.TwelvePointStar = new ChartShape(InstancedShape.Star12, "pack://application:,,,/VisualizationControl;component/Images/12pt_star.PNG", Resources.ShapesGallery_Star);
      ChartShape.ShapesList = new List<ChartShape>();
      ChartShape.ShapesList.Add(ChartShape.Triangle);
      ChartShape.ShapesList.Add(ChartShape.Square);
      ChartShape.ShapesList.Add(ChartShape.Circle);
      ChartShape.ShapesList.Add(ChartShape.Pentagon);
      ChartShape.ShapesList.Add(ChartShape.TwelvePointStar);
    }

    private ChartShape(InstancedShape shape, string iconResourcePath, string name)
    {
      this.Shape = shape;
      this.IconPath = iconResourcePath;
      this.Name = name;
    }
  }
}
