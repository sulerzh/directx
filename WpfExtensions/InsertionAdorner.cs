using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class InsertionAdorner : Adorner
  {
    private static Pen pen = new Pen()
    {
      Brush = (Brush) Brushes.Gray,
      Thickness = 2.0
    };
    private bool isSeparatorHorizontal;
    private AdornerLayer adornerLayer;
    private static PathGeometry triangle;

    public bool IsInFirstHalf { get; set; }

    static InsertionAdorner()
    {
      InsertionAdorner.pen.Freeze();
      LineSegment lineSegment1 = new LineSegment(new Point(0.0, -5.0), false);
      lineSegment1.Freeze();
      LineSegment lineSegment2 = new LineSegment(new Point(0.0, 5.0), false);
      lineSegment2.Freeze();
      PathFigure pathFigure = new PathFigure()
      {
        StartPoint = new Point(5.0, 0.0)
      };
      pathFigure.Segments.Add((PathSegment) lineSegment1);
      pathFigure.Segments.Add((PathSegment) lineSegment2);
      pathFigure.Freeze();
      InsertionAdorner.triangle = new PathGeometry();
      InsertionAdorner.triangle.Figures.Add(pathFigure);
      InsertionAdorner.triangle.Freeze();
    }

    public InsertionAdorner(bool isSeparatorHorizontal, bool isInFirstHalf, UIElement adornedElement, AdornerLayer adornerLayer)
      : base(adornedElement)
    {
      this.isSeparatorHorizontal = isSeparatorHorizontal;
      this.IsInFirstHalf = isInFirstHalf;
      this.adornerLayer = adornerLayer;
      this.IsHitTestVisible = false;
      this.adornerLayer.Add((Adorner) this);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      Point startPoint;
      Point endPoint;
      this.CalculateStartAndEndPoint(out startPoint, out endPoint);
      drawingContext.DrawLine(InsertionAdorner.pen, startPoint, endPoint);
      if (this.isSeparatorHorizontal)
      {
        this.DrawTriangle(drawingContext, startPoint, 0.0);
        this.DrawTriangle(drawingContext, endPoint, 180.0);
      }
      else
      {
        this.DrawTriangle(drawingContext, startPoint, 90.0);
        this.DrawTriangle(drawingContext, endPoint, -90.0);
      }
    }

    private void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
    {
      drawingContext.PushTransform((Transform) new TranslateTransform(origin.X, origin.Y));
      drawingContext.PushTransform((Transform) new RotateTransform(angle));
      drawingContext.DrawGeometry(InsertionAdorner.pen.Brush, (Pen) null, (Geometry) InsertionAdorner.triangle);
      drawingContext.Pop();
      drawingContext.Pop();
    }

    private void CalculateStartAndEndPoint(out Point startPoint, out Point endPoint)
    {
      startPoint = new Point();
      endPoint = new Point();
      double width = this.AdornedElement.RenderSize.Width;
      double height = this.AdornedElement.RenderSize.Height;
      if (this.isSeparatorHorizontal)
      {
        endPoint.X = width;
        if (this.IsInFirstHalf)
          return;
        startPoint.Y = height;
        endPoint.Y = height;
      }
      else
      {
        endPoint.Y = height;
        if (this.IsInFirstHalf)
          return;
        startPoint.X = width;
        endPoint.X = width;
      }
    }

    public void Detach()
    {
      this.adornerLayer.Remove((Adorner) this);
    }
  }
}
