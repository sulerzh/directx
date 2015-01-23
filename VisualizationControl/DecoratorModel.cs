using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class DecoratorModel : CompositePropertyChangeNotificationBase
  {
    private double _Width = 400.0;
    private double _Height = 250.0;
    private double _ActualWidth = 400.0;
    private double _ActualHeight = 250.0;
    private bool _IsVisible = true;
    public const double DefaultHeight = 250.0;
    public const double DefaultWidth = 400.0;
    private double _X;
    private double _Y;
    private double _DistanceToNearestCornerX;
    private double _DistanceToNearestCornerY;
    private int _ZOrder;
    private DecoratorContentBase _Content;
    private DecoratorDock _Dock;

    public string PropertyX
    {
      get
      {
        return "X";
      }
    }

    public double X
    {
      get
      {
        return this._X;
      }
      set
      {
        base.SetProperty<double>(this.PropertyX, ref this._X, value);
      }
    }

    public string PropertyY
    {
      get
      {
        return "Y";
      }
    }

    public double Y
    {
      get
      {
        return this._Y;
      }
      set
      {
        base.SetProperty<double>(this.PropertyY, ref this._Y, value);
      }
    }

    public string PropertyDistanceToNearestCornerX
    {
      get
      {
        return "DistanceToNearestCornerX";
      }
    }

    public double DistanceToNearestCornerX
    {
      get
      {
        return this._DistanceToNearestCornerX;
      }
      set
      {
        base.SetProperty<double>(this.PropertyDistanceToNearestCornerX, ref this._DistanceToNearestCornerX, value);
      }
    }

    public string PropertyDistanceToNearestCornerY
    {
      get
      {
        return "DistanceToNearestCornerY";
      }
    }

    public double DistanceToNearestCornerY
    {
      get
      {
        return this._DistanceToNearestCornerY;
      }
      set
      {
        base.SetProperty<double>(this.PropertyDistanceToNearestCornerY, ref this._DistanceToNearestCornerY, value);
      }
    }

    public string PropertyZOrder
    {
      get
      {
        return "ZOrder";
      }
    }

    public int ZOrder
    {
      get
      {
        return this._ZOrder;
      }
      set
      {
        base.SetProperty<int>(this.PropertyZOrder, ref this._ZOrder, value);
      }
    }

    public string PropertyWidth
    {
      get
      {
        return "Width";
      }
    }

    public double Width
    {
      get
      {
        return this._Width;
      }
      set
      {
        base.SetProperty<double>(this.PropertyWidth, ref this._Width, value);
      }
    }

    public string PropertyHeight
    {
      get
      {
        return "Height";
      }
    }

    public double Height
    {
      get
      {
        return this._Height;
      }
      set
      {
        base.SetProperty<double>(this.PropertyHeight, ref this._Height, value);
      }
    }

    public string PropertyActualWidth
    {
      get
      {
        return "ActualWidth";
      }
    }

    public double ActualWidth
    {
      get
      {
        return this._ActualWidth;
      }
      set
      {
        base.SetProperty<double>(this.PropertyActualWidth, ref this._ActualWidth, value);
      }
    }

    public string PropertyActualHeight
    {
      get
      {
        return "ActualHeight";
      }
    }

    public double ActualHeight
    {
      get
      {
        return this._ActualHeight;
      }
      set
      {
        base.SetProperty<double>(this.PropertyActualHeight, ref this._ActualHeight, value);
      }
    }

    public string PropertyIsVisible
    {
      get
      {
        return "IsVisible";
      }
    }

    public bool IsVisible
    {
      get
      {
        return this._IsVisible;
      }
      set
      {
        base.SetProperty<bool>(this.PropertyIsVisible, ref this._IsVisible, value);
      }
    }

    public bool SetFocusOnLoadView { get; set; }

    public string PropertyContent
    {
      get
      {
        return "Content";
      }
    }

    [XmlElement("Label", typeof (LabelDecoratorModel))]
    [XmlElement("Chart", typeof (ChartDecoratorModel))]
    [XmlElement("Time", typeof (TimeDecoratorModel))]
    [XmlElement("Legend", typeof (LayerLegendDecoratorModel))]
    public DecoratorContentBase Content
    {
      get
      {
        return this._Content;
      }
      set
      {
        base.SetProperty<DecoratorContentBase>(this.PropertyContent, ref this._Content, value);
      }
    }

    public string PropertyDock
    {
      get
      {
        return "Dock";
      }
    }

    public DecoratorDock Dock
    {
      get
      {
        return this._Dock;
      }
      set
      {
        base.SetProperty<DecoratorDock>(this.PropertyDock, ref this._Dock, value);
      }
    }

    public void UpdatePositionFromScreenSize(double screenWidth, double screenHeight)
    {
      switch (this.Dock)
      {
        case DecoratorDock.TopLeft:
          this.X = this.DistanceToNearestCornerX;
          this.Y = this.DistanceToNearestCornerY;
          break;
        case DecoratorDock.TopRight:
          this.X = screenWidth - (this.ActualWidth + this.DistanceToNearestCornerX);
          this.Y = this.DistanceToNearestCornerY;
          break;
        case DecoratorDock.BottomLeft:
          this.X = this.DistanceToNearestCornerX;
          this.Y = screenHeight - (this.ActualHeight + this.DistanceToNearestCornerY);
          break;
        case DecoratorDock.BottomRight:
          this.X = screenWidth - (this.ActualWidth + this.DistanceToNearestCornerX);
          this.Y = screenHeight - (this.ActualHeight + this.DistanceToNearestCornerY);
          break;
      }
    }

    public void UpdateNearestDockOffsetFromScreenSize(double screenWidth, double screenHeight)
    {
      switch (this.Dock)
      {
        case DecoratorDock.TopLeft:
          this.DistanceToNearestCornerX = this.X;
          this.DistanceToNearestCornerY = this.Y;
          break;
        case DecoratorDock.TopRight:
          this.DistanceToNearestCornerX = screenWidth - (this.ActualWidth + this.X);
          this.DistanceToNearestCornerY = this.Y;
          break;
        case DecoratorDock.BottomLeft:
          this.DistanceToNearestCornerX = this.X;
          this.DistanceToNearestCornerY = screenHeight - (this.ActualHeight + this.Y);
          break;
        case DecoratorDock.BottomRight:
          this.DistanceToNearestCornerX = screenWidth - (this.ActualWidth + this.X);
          this.DistanceToNearestCornerY = screenHeight - (this.ActualHeight + this.Y);
          break;
      }
    }

    public void AutoSize()
    {
      this.Width = double.NaN;
      this.Height = double.NaN;
    }

    public void Focus()
    {
      this.SetFocusOnLoadView = true;
    }
  }
}
