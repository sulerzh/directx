using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DecoratorCollectionViewModel : ViewModelBase
  {
    public const double ModelMarginThickness = 8.0;
    public const double MinimumModelX = -8.0;
    public const double MinimumModelY = 0.0;
    public const double MinimumModelHeight = 60.0;
    public const double MinimumModelWidth = 60.0;
    public const double MaximimModelWidth = 50000.0;
    private const double DecoratorCanvasLowerRightCornerPadding = 50.0;
    private readonly WeakEventListener<DecoratorCollectionViewModel, DecoratorViewModel> addItemListener;
    private readonly WeakEventListener<DecoratorCollectionViewModel, DecoratorViewModel> removeItemListener;

    public double Width { get; set; }

    public double Height { get; set; }

    public ObservableCollectionEx<DecoratorViewModel> Decorators { get; private set; }

    private ICommand CloseCommand { get; set; }

    private ICommand IncrementZOrderCommand { get; set; }

    private ICommand DecrementZOrderCommand { get; set; }

    private ICommand TopZOrderCommand { get; set; }

    private ICommand BottomZOrderCommand { get; set; }

    public event Action<DecoratorViewModel> DecoratorClosed;

    public event Action<DecoratorViewModel> DecoratorEditted;

    public DecoratorCollectionViewModel()
    {
      this.Decorators = new ObservableCollectionEx<DecoratorViewModel>();
      this.addItemListener = new WeakEventListener<DecoratorCollectionViewModel, DecoratorViewModel>(this)
      {
        OnEventAction = new Action<DecoratorCollectionViewModel, DecoratorViewModel>(DecoratorCollectionViewModel.OnAddItem)
      };
      this.Decorators.ItemAdded += new ObservableCollectionExChangedHandler<DecoratorViewModel>(this.addItemListener.OnEvent);
      this.removeItemListener = new WeakEventListener<DecoratorCollectionViewModel, DecoratorViewModel>(this)
      {
        OnEventAction = new Action<DecoratorCollectionViewModel, DecoratorViewModel>(DecoratorCollectionViewModel.OnRemoveItem)
      };
      this.Decorators.ItemRemoved += new ObservableCollectionExChangedHandler<DecoratorViewModel>(this.removeItemListener.OnEvent);
    }

    public void UpdateSize(double previousWidth, double previousHeight, double newWidth, double newHeight)
    {
      double deltaWidth = newWidth - this.Width;
      double deltaHeight = newHeight - this.Height;
      this.Width = newWidth;
      this.Height = newHeight;
      this.UpdateDeltaSize(deltaWidth, deltaHeight);
    }

    private void UpdateDeltaSize(double deltaWidth, double deltaHeight)
    {
      foreach (DecoratorViewModel decoratorViewModel in (Collection<DecoratorViewModel>) this.Decorators)
      {
        switch (decoratorViewModel.Model.Dock)
        {
          case DecoratorDock.TopRight:
            decoratorViewModel.Model.X += deltaWidth;
            continue;
          case DecoratorDock.BottomLeft:
            decoratorViewModel.Model.Y += deltaHeight;
            continue;
          case DecoratorDock.BottomRight:
            decoratorViewModel.Model.X += deltaWidth;
            decoratorViewModel.Model.Y += deltaHeight;
            continue;
          default:
            continue;
        }
      }
    }

    private void ValidateNewSize(ref double x, ref double y, ref double height, ref double width, ResizeSource resizeSource = ResizeSource.None)
    {
      if (resizeSource == ResizeSource.BottomRight)
      {
        double num1 = Math.Min(this.Width - x + 8.0, 50000.0);
        double num2 = this.Height - y + 8.0;
        if (width > num1)
          width = num1;
        else if (width < 60.0)
          width = 60.0;
        if (height > num2)
        {
          height = num2;
        }
        else
        {
          if (height >= 60.0)
            return;
          height = 60.0;
        }
      }
      else
      {
        if (resizeSource != ResizeSource.TopLeft)
          return;
        if (width > 50000.0)
        {
          double num = width - 50000.0;
          width -= num;
          x += num;
        }
        if (x < -8.0)
        {
          width -= -8.0 - x;
          x = -8.0;
        }
        if (y < 0.0)
        {
          height -= 0.0 - y;
          y = 0.0;
        }
        double num1;
        if (width < 60.0)
        {
          double num2 = 60.0 - width;
          num1 = x - num2;
          width = 60.0;
        }
        else
          num1 = x + width - 60.0;
        double num3;
        if (height < 60.0)
        {
          double num2 = 60.0 - height;
          num3 = y - num2;
          height = 60.0;
        }
        else
          num3 = y + height - 60.0;
        if (x > num1)
          x = num1;
        if (y <= num3)
          return;
        y = num3;
      }
    }

    private void OnDecoratorDragged(DecoratorViewModel sender, double xDelta, double yDelta)
    {
      DecoratorModel model = sender.Model;
      if (model == null)
        return;
      if (model.ActualHeight + 8.0 < this.Height && model.ActualWidth + 8.0 < this.Width)
      {
        double num1 = this.Width - model.ActualWidth + 8.0;
        double num2 = this.Height - model.ActualHeight;
        if (model.X > num1)
          model.X = num1;
        else if (model.X < -8.0)
          model.X = -8.0;
        if (model.Y > num2)
          model.Y = num2;
        else if (model.Y < 0.0)
          model.Y = 0.0;
        model.Dock = this.GetQuadrantFromClosestCorner(sender);
        model.UpdateNearestDockOffsetFromScreenSize(this.Width, this.Height);
      }
      else
      {
        double num1 = this.Width - 50.0;
        double num2 = this.Height - 50.0;
        if (model.X > num1)
          model.X = num1;
        if (model.X < -8.0)
          model.X = -8.0;
        if (model.Y > num2)
          model.Y = num2;
        if (model.Y < 0.0)
          model.Y = 0.0;
        model.Dock = DecoratorDock.TopLeft;
        model.UpdateNearestDockOffsetFromScreenSize(this.Width, this.Height);
      }
    }

    private DecoratorDock GetQuadrantFromClosestCorner(DecoratorViewModel sender)
    {
      double distanceBetween1 = this.GetDistanceBetween(sender.Model.X, sender.Model.Y, 0.0, 0.0);
      double distanceBetween2 = this.GetDistanceBetween(sender.Model.X + sender.Model.ActualWidth, sender.Model.Y, this.Width, 0.0);
      double distanceBetween3 = this.GetDistanceBetween(sender.Model.X, sender.Model.Y + sender.Model.ActualHeight, 0.0, this.Height);
      double distanceBetween4 = this.GetDistanceBetween(sender.Model.X + sender.Model.ActualWidth, sender.Model.Y + sender.Model.ActualHeight, this.Width, this.Height);
      double num = distanceBetween1;
      DecoratorDock decoratorDock = DecoratorDock.TopLeft;
      if (distanceBetween2 < num)
      {
        num = distanceBetween2;
        decoratorDock = DecoratorDock.TopRight;
      }
      if (distanceBetween3 < num)
      {
        num = distanceBetween3;
        decoratorDock = DecoratorDock.BottomLeft;
      }
      if (distanceBetween4 < num)
        decoratorDock = DecoratorDock.BottomRight;
      return decoratorDock;
    }

    private double GetDistanceBetween(double ax, double ay, double bx, double by)
    {
      return Math.Sqrt(Math.Pow(ax - bx, 2.0) + Math.Pow(ay - by, 2.0));
    }

    private DecoratorDock GetQuadrantFromPoint(double x, double y)
    {
      if (x <= this.Width / 2.0 && y <= this.Height / 2.0)
        return DecoratorDock.TopLeft;
      if (x <= this.Width / 2.0 && y > this.Height / 2.0)
        return DecoratorDock.BottomLeft;
      return x > this.Width / 2.0 && y <= this.Height / 2.0 ? DecoratorDock.TopRight : DecoratorDock.BottomRight;
    }

    private void OnEditDecorator(DecoratorViewModel decorator)
    {
      if (this.DecoratorEditted == null)
        return;
      this.DecoratorEditted(decorator);
    }

    private bool OnCanEditDecorator(DecoratorViewModel decorator)
    {
      if (!(decorator.Model.Content is LabelDecoratorModel))
        return decorator.Model.Content is TimeDecoratorModel;
      else
        return true;
    }

    private void OnCloseDecorator(DecoratorViewModel decorator)
    {
      if (this.DecoratorClosed == null)
        return;
      this.DecoratorClosed(decorator);
    }

    private void OnDecoratorIncrementZOrder(DecoratorViewModel decorator)
    {
      int index = this.Decorators.IndexOf(decorator);
      if (index == -1 || index == this.Decorators.Count - 1)
        return;
      DecoratorViewModel decoratorViewModel = this.Decorators[index + 1];
      this.Decorators.Remove(decorator);
      this.Decorators.Remove(decoratorViewModel);
      this.Decorators.Insert(index, decoratorViewModel);
      this.Decorators.Insert(index + 1, decorator);
    }

    private void OnDecoratorDecrementZOrder(DecoratorViewModel decorator)
    {
      int num = this.Decorators.IndexOf(decorator);
      switch (num)
      {
        case -1:
          break;
        case 0:
          break;
        default:
          DecoratorViewModel decoratorViewModel = this.Decorators[num - 1];
          this.Decorators.Remove(decorator);
          this.Decorators.Remove(decoratorViewModel);
          this.Decorators.Insert(num - 1, decoratorViewModel);
          this.Decorators.Insert(num - 1, decorator);
          break;
      }
    }

    private void RefreshZOrders()
    {
      for (int index = 0; index < this.Decorators.Count; ++index)
        this.Decorators[index].Model.ZOrder = index;
    }

    private void OnDecoratorBringToFront(DecoratorViewModel decorator)
    {
      this.Decorators.Remove(decorator);
      this.Decorators.Add(decorator);
    }

    private void OnDecoratorSendToBottom(DecoratorViewModel decorator)
    {
      this.ReOrderDecorator(decorator, 0);
    }

    private void ReOrderDecorator(DecoratorViewModel decorator, int position)
    {
      if (decorator == null || position < 0)
        return;
      this.Decorators.Remove(decorator);
      this.Decorators.Insert(position, decorator);
    }

    private static void OnAddItem(DecoratorCollectionViewModel model, DecoratorViewModel item)
    {
      item.EditCommandCallback = new Action<DecoratorViewModel>(model.OnEditDecorator);
      item.CanEditCommandCallback = new Predicate<DecoratorViewModel>(model.OnCanEditDecorator);
      item.CloseCommandCallback = new Action<DecoratorViewModel>(model.OnCloseDecorator);
      item.IncrementZOrderCommandCallback = new Action<DecoratorViewModel>(model.OnDecoratorIncrementZOrder);
      item.DecrementZOrderCommandCallback = new Action<DecoratorViewModel>(model.OnDecoratorDecrementZOrder);
      item.TopZOrderCommandCallback = new Action<DecoratorViewModel>(model.OnDecoratorBringToFront);
      item.BottomZOrderCommandCallback = new Action<DecoratorViewModel>(model.OnDecoratorSendToBottom);
      item.ValidateNewSizeCallback = new ValidateNewSizeHandler(model.ValidateNewSize);
      item.OnDragged += new DragDeltaEventHandler(model.OnDecoratorDragged);
      model.RefreshZOrders();
    }

    private static void OnRemoveItem(DecoratorCollectionViewModel model, DecoratorViewModel item)
    {
      item.CloseCommandCallback = (Action<DecoratorViewModel>) null;
      item.IncrementZOrderCommandCallback = (Action<DecoratorViewModel>) null;
      item.DecrementZOrderCommandCallback = (Action<DecoratorViewModel>) null;
      item.TopZOrderCommandCallback = (Action<DecoratorViewModel>) null;
      item.BottomZOrderCommandCallback = (Action<DecoratorViewModel>) null;
      item.ValidateNewSizeCallback = (ValidateNewSizeHandler) null;
      item.OnDragged -= new DragDeltaEventHandler(model.OnDecoratorDragged);
      model.RefreshZOrders();
    }
  }
}
