using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class DraggedAdorner : Adorner
  {
    private ContentPresenter contentPresenter;
    private double left;
    private double top;
    private AdornerLayer adornerLayer;

    protected override int VisualChildrenCount
    {
      get
      {
        return 1;
      }
    }

    public DraggedAdorner(object dragDropData, DataTemplate dragDropTemplate, UIElement adornedElement, AdornerLayer adornerLayer)
      : base(adornedElement)
    {
      this.adornerLayer = adornerLayer;
      this.contentPresenter = new ContentPresenter();
      this.contentPresenter.Content = dragDropData;
      this.contentPresenter.ContentTemplate = dragDropTemplate;
      this.contentPresenter.Opacity = 0.7;
      this.adornerLayer.Add((Adorner) this);
    }

    public void SetPosition(double left, double top)
    {
      this.left = left - 1.0;
      this.top = top + 13.0;
      if (this.adornerLayer == null)
        return;
      this.adornerLayer.Update(this.AdornedElement);
    }

    protected override Size MeasureOverride(Size constraint)
    {
      this.contentPresenter.Measure(constraint);
      return this.contentPresenter.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      this.contentPresenter.Arrange(new Rect(finalSize));
      return finalSize;
    }

    protected override Visual GetVisualChild(int index)
    {
      return (Visual) this.contentPresenter;
    }

    public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
    {
      return (GeneralTransform) new GeneralTransformGroup()
      {
        Children = {
          base.GetDesiredTransform(transform),
          (GeneralTransform) new TranslateTransform(this.FlowDirection == FlowDirection.RightToLeft ? -this.left : this.left, this.top)
        }
      };
    }

    public void Detach()
    {
      this.adornerLayer.Remove((Adorner) this);
    }
  }
}
