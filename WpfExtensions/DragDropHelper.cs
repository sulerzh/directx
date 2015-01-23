using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class DragDropHelper
  {
    private static string DataFormatString = "VisualizationClientDragDropFormat";
    private static readonly WeakReference<DragDropHelper> _instance = new WeakReference<DragDropHelper>((DragDropHelper) null);
    public static readonly DependencyProperty DragDropTemplateProperty = DependencyProperty.RegisterAttached("DragDropTemplate", typeof (DataTemplate), typeof (DragDropHelper), (PropertyMetadata) new UIPropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty DropErrorTemplateProperty = DependencyProperty.RegisterAttached("DropErrorTemplate", typeof (DataTemplate), typeof (DragDropHelper), (PropertyMetadata) new UIPropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty DefaultDragDropEffectProperty = DependencyProperty.RegisterAttached("DefaultDragDropEffect", typeof (DragDropEffects), typeof (DragDropHelper), (PropertyMetadata) new UIPropertyMetadata((object) DragDropEffects.Copy));
    public static readonly DependencyProperty DropHandlerProperty = DependencyProperty.RegisterAttached("DropHandler", typeof (IDropHandler), typeof (DragDropHelper), (PropertyMetadata) new UIPropertyMetadata((object) null, new PropertyChangedCallback(DragDropHelper.DropHandlerChanged)));
    public static readonly DependencyProperty DragHandlerProperty = DependencyProperty.RegisterAttached("DragHandler", typeof (IDragHandler), typeof (DragDropHelper), (PropertyMetadata) new UIPropertyMetadata((object) null, new PropertyChangedCallback(DragDropHelper.DragHandlerChanged)));
    private Point initialMouseDragStartPosition;
    private Vector initialMouseOffset;
    private object draggedData;
    private DraggedAdorner draggedAdorner;
    private DropErrorAdorner dropErrorAdorner;
    private InsertionAdorner insertionAdorner;
    private Window topWindow;
    private Control dragSourceControl;
    private FrameworkElement sourceItemContainer;
    private FrameworkElement targetItemContainer;
    private bool hasVerticalOrientation;
    private int insertionIndex;
    private bool isInFirstHalf;

    private static DragDropHelper Instance
    {
      get
      {
        DragDropHelper target;
        if (!DragDropHelper._instance.TryGetTarget(out target))
        {
          lock (DragDropHelper._instance)
          {
            if (!DragDropHelper._instance.TryGetTarget(out target))
            {
              target = new DragDropHelper();
              DragDropHelper._instance.SetTarget(target);
            }
          }
        }
        return target;
      }
    }

    public static DataTemplate GetDragDropTemplate(DependencyObject obj)
    {
      return (DataTemplate) obj.GetValue(DragDropHelper.DragDropTemplateProperty);
    }

    public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
    {
      obj.SetValue(DragDropHelper.DragDropTemplateProperty, (object) value);
    }

    public static DataTemplate GetDropErrorTemplate(DependencyObject obj)
    {
      return (DataTemplate) obj.GetValue(DragDropHelper.DropErrorTemplateProperty);
    }

    public static void SetDropErrorTemplate(DependencyObject obj, DataTemplate value)
    {
      obj.SetValue(DragDropHelper.DropErrorTemplateProperty, (object) value);
    }

    public static DragDropEffects GetDefaultDragDropEffect(DependencyObject obj)
    {
      return (DragDropEffects) obj.GetValue(DragDropHelper.DefaultDragDropEffectProperty);
    }

    public static void SetDefaultDragDropEffect(DependencyObject obj, DragDropEffects value)
    {
      obj.SetValue(DragDropHelper.DefaultDragDropEffectProperty, (object) value);
    }

    public static IDropHandler GetDropHandler(DependencyObject obj)
    {
      return (IDropHandler) obj.GetValue(DragDropHelper.DropHandlerProperty);
    }

    public static void SetDropHandler(DependencyObject obj, IDropHandler value)
    {
      obj.SetValue(DragDropHelper.DropHandlerProperty, (object) value);
    }

    private static void DropHandlerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      Control control = obj as Control;
      if (control == null)
        return;
      if (e.NewValue != null && e.NewValue is IDropHandler)
      {
        control.AllowDrop = true;
        control.PreviewDrop += new DragEventHandler(DragDropHelper.Instance.DropTarget_PreviewDrop);
        control.PreviewDragEnter += new DragEventHandler(DragDropHelper.Instance.DropTarget_PreviewDragEnter);
        control.PreviewDragOver += new DragEventHandler(DragDropHelper.Instance.DropTarget_PreviewDragOver);
        control.PreviewDragLeave += new DragEventHandler(DragDropHelper.Instance.DropTarget_PreviewDragLeave);
      }
      else
      {
        control.AllowDrop = false;
        control.PreviewDrop -= new DragEventHandler(DragDropHelper.Instance.DropTarget_PreviewDrop);
        control.PreviewDragEnter -= new DragEventHandler(DragDropHelper.Instance.DropTarget_PreviewDragEnter);
        control.PreviewDragOver -= new DragEventHandler(DragDropHelper.Instance.DropTarget_PreviewDragOver);
        control.PreviewDragLeave -= new DragEventHandler(DragDropHelper.Instance.DropTarget_PreviewDragLeave);
      }
    }

    public static IDragHandler GetDragHandler(DependencyObject obj)
    {
      return (IDragHandler) obj.GetValue(DragDropHelper.DragHandlerProperty);
    }

    public static void SetDragHandler(DependencyObject obj, IDragHandler value)
    {
      obj.SetValue(DragDropHelper.DragHandlerProperty, (object) value);
    }

    private static void DragHandlerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      Control control = obj as Control;
      if (control == null)
        return;
      if (e.NewValue != null && e.NewValue is IDragHandler)
      {
        control.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DragDropHelper.Instance.DragSource_PreviewMouseLeftButtonDown);
        control.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragDropHelper.Instance.DragSource_PreviewMouseLeftButtonUp);
        control.PreviewMouseMove += new MouseEventHandler(DragDropHelper.Instance.DragSource_PreviewMouseMove);
      }
      else
      {
        control.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(DragDropHelper.Instance.DragSource_PreviewMouseLeftButtonDown);
        control.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(DragDropHelper.Instance.DragSource_PreviewMouseLeftButtonUp);
        control.PreviewMouseMove -= new MouseEventHandler(DragDropHelper.Instance.DragSource_PreviewMouseMove);
      }
    }

    private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      this.dragSourceControl = (Control) sender;
      this.topWindow = Window.GetWindow((DependencyObject) this.dragSourceControl);
      if (!this.CheckTopWindowAccess((RoutedEventArgs) e))
        return;
      this.initialMouseDragStartPosition = e.GetPosition((IInputElement) this.topWindow);
      ItemsControl itemsControl = this.dragSourceControl as ItemsControl;
      if (itemsControl != null)
      {
        this.sourceItemContainer = itemsControl.ContainerFromElement((DependencyObject) (e.OriginalSource as Visual)) as FrameworkElement;
        if (this.sourceItemContainer == null)
          return;
        this.draggedData = this.sourceItemContainer.DataContext;
      }
      else
      {
        DragDropContentControl dropContentControl = this.dragSourceControl as DragDropContentControl;
        if (dropContentControl == null)
          return;
        this.sourceItemContainer = (FrameworkElement) dropContentControl;
        this.draggedData = dropContentControl.Content;
      }
    }

    private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (this.draggedData == null || !this.CheckTopWindowAccess((RoutedEventArgs) e) || !MouseUtilities.IsMovementBigEnough(this.initialMouseDragStartPosition, e.GetPosition((IInputElement) this.topWindow)))
        return;
      this.initialMouseOffset = this.initialMouseDragStartPosition - this.sourceItemContainer.TranslatePoint(new Point(0.0, 0.0), (UIElement) this.topWindow);
      DataObject dataObject = new DataObject(DragDropHelper.DataFormatString, this.draggedData);
      bool allowDrop = this.topWindow.AllowDrop;
      this.topWindow.AllowDrop = true;
      this.topWindow.DragEnter += new DragEventHandler(this.TopWindow_DragEnter);
      this.topWindow.DragOver += new DragEventHandler(this.TopWindow_DragOver);
      this.topWindow.DragLeave += new DragEventHandler(this.TopWindow_DragLeave);
      int num = (int) DragDrop.DoDragDrop((DependencyObject) sender, (object) dataObject, DragDropHelper.GetDefaultDragDropEffect((DependencyObject) sender));
      this.RemoveDraggedAdorner();
      this.RemoveErrorAdorner();
      this.topWindow.AllowDrop = allowDrop;
      this.topWindow.DragEnter -= new DragEventHandler(this.TopWindow_DragEnter);
      this.topWindow.DragOver -= new DragEventHandler(this.TopWindow_DragOver);
      this.topWindow.DragLeave -= new DragEventHandler(this.TopWindow_DragLeave);
      this.draggedData = (object) null;
    }

    private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.draggedData = (object) null;
    }

    private void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
    {
      object data = e.Data.GetData(DragDropHelper.DataFormatString);
      Control control = (Control) sender;
      this.DecideDropTarget(control, e);
      if (data != null)
      {
        if (!this.CheckTopWindowAccess((RoutedEventArgs) e))
          return;
        this.ShowDraggedAdorner(e.GetPosition((IInputElement) this.topWindow));
        IDropHandler dropHandler = DragDropHelper.GetDropHandler((DependencyObject) control);
        if (dropHandler != null)
        {
          string error = dropHandler.ValidateDropItem(data);
          if (error != null)
            this.CreateErrorAdorner(error, control);
        }
        this.CreateInsertionAdorner();
      }
      e.Handled = true;
    }

    private void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
    {
      object data = e.Data.GetData(DragDropHelper.DataFormatString);
      this.DecideDropTarget((Control) sender, e);
      if (data != null)
      {
        if (!this.CheckTopWindowAccess((RoutedEventArgs) e))
          return;
        this.ShowDraggedAdorner(e.GetPosition((IInputElement) this.topWindow));
        this.UpdateErrorAdorner(e.GetPosition((IInputElement) this.topWindow));
        this.UpdateInsertionAdornerPosition();
      }
      e.Handled = true;
    }

    private void DropTarget_PreviewDrop(object sender, DragEventArgs e)
    {
      object data = e.Data.GetData(DragDropHelper.DataFormatString);
      int num = -1;
      if (data != null)
      {
        Control control = (Control) sender;
        IDropItemsHandler dropItemsHandler = DragDropHelper.GetDropHandler((DependencyObject) control) as IDropItemsHandler;
        IDropItemHandler dropItemHandler = DragDropHelper.GetDropHandler((DependencyObject) control) as IDropItemHandler;
        if (dropItemsHandler != null && !dropItemsHandler.CanDropItem(data) || dropItemHandler != null && !dropItemHandler.CanDropItem(data))
        {
          e.Handled = true;
          return;
        }
        else
        {
          if (e.Effects != DragDropEffects.None)
          {
            IDragItemsHandler dragItemsHandler = DragDropHelper.GetDragHandler((DependencyObject) this.dragSourceControl) as IDragItemsHandler;
            if (dragItemsHandler != null && dragItemsHandler.CanDragItem(data))
              num = dragItemsHandler.ProcessDraggedItem(data, e.Effects);
            IDragItemHandler dragItemHandler = DragDropHelper.GetDragHandler((DependencyObject) this.dragSourceControl) as IDragItemHandler;
            if (dragItemHandler != null && dragItemHandler.CanDragItem(data))
              dragItemHandler.ProcessDraggedItem(data, e.Effects);
          }
          if (num != -1 && this.dragSourceControl == sender && num < this.insertionIndex)
            --this.insertionIndex;
          if (dropItemsHandler != null)
          {
            dropItemsHandler.DropItem(data, this.insertionIndex);
            this.RemoveDraggedAdorner();
            this.RemoveErrorAdorner();
            this.RemoveInsertionAdorner();
          }
          if (dropItemHandler != null)
          {
            dropItemHandler.DropItem(data);
            this.RemoveDraggedAdorner();
            this.RemoveErrorAdorner();
          }
        }
      }
      e.Handled = true;
    }

    private void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
    {
      if (e.Data.GetData(DragDropHelper.DataFormatString) != null)
      {
        this.RemoveErrorAdorner();
        this.RemoveInsertionAdorner();
      }
      e.Handled = true;
    }

    private bool CheckTopWindowAccess(RoutedEventArgs e)
    {
      if (this.topWindow == null || this.topWindow.Dispatcher.CheckAccess())
        return true;
      e.Handled = true;
      return false;
    }

    private void DecideDropTarget(Control targetDropControl, DragEventArgs e)
    {
      if (targetDropControl == null)
        return;
      this.targetItemContainer = (FrameworkElement) null;
      ItemsControl itemsControl = targetDropControl as ItemsControl;
      if (itemsControl != null)
      {
        int count = itemsControl.Items.Count;
        object data = e.Data.GetData(DragDropHelper.DataFormatString);
        IDropItemsHandler dropItemsHandler = DragDropHelper.GetDropHandler((DependencyObject) itemsControl) as IDropItemsHandler;
        if (dropItemsHandler != null && dropItemsHandler.CanDropItem(data))
        {
          if (count > 0)
          {
            this.hasVerticalOrientation = FrameworkElementExtensions.HasVerticalOrientation(itemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement);
            this.targetItemContainer = itemsControl.ContainerFromElement((DependencyObject) e.OriginalSource) as FrameworkElement;
            if (this.targetItemContainer != null)
            {
              this.isInFirstHalf = FrameworkElementExtensions.IsInFirstHalf(this.targetItemContainer, e.GetPosition((IInputElement) this.targetItemContainer), this.hasVerticalOrientation);
              this.insertionIndex = itemsControl.ItemContainerGenerator.IndexFromContainer((DependencyObject) this.targetItemContainer);
              if (this.isInFirstHalf)
                return;
              ++this.insertionIndex;
            }
            else
            {
              this.targetItemContainer = itemsControl.ItemContainerGenerator.ContainerFromIndex(count - 1) as FrameworkElement;
              this.isInFirstHalf = false;
              this.insertionIndex = count;
            }
          }
          else
            this.insertionIndex = 0;
        }
        else
          e.Effects = DragDropEffects.None;
      }
      else
      {
        DragDropContentControl dropContentControl = targetDropControl as DragDropContentControl;
        if (dropContentControl == null)
          return;
        object data = e.Data.GetData(DragDropHelper.DataFormatString);
        IDropItemHandler dropItemHandler = DragDropHelper.GetDropHandler((DependencyObject) dropContentControl) as IDropItemHandler;
        if (dropItemHandler != null && dropItemHandler.CanDropItem(data))
          return;
        e.Effects = DragDropEffects.None;
      }
    }

    private void TopWindow_DragEnter(object sender, DragEventArgs e)
    {
      if (!this.CheckTopWindowAccess((RoutedEventArgs) e))
        return;
      this.ShowDraggedAdorner(e.GetPosition((IInputElement) this.topWindow));
      e.Effects = DragDropEffects.Scroll;
      e.Handled = true;
    }

    private void TopWindow_DragOver(object sender, DragEventArgs e)
    {
      if (!this.CheckTopWindowAccess((RoutedEventArgs) e))
        return;
      this.ShowDraggedAdorner(e.GetPosition((IInputElement) this.topWindow));
      e.Effects = DragDropEffects.Scroll;
      e.Handled = true;
    }

    private void TopWindow_DragLeave(object sender, DragEventArgs e)
    {
      if (!this.CheckTopWindowAccess((RoutedEventArgs) e))
        return;
      this.RemoveDraggedAdorner();
      this.RemoveErrorAdorner();
      e.Handled = true;
    }

    private void ShowDraggedAdorner(Point currentPosition)
    {
      if (this.draggedAdorner == null)
        this.draggedAdorner = new DraggedAdorner(this.draggedData, DragDropHelper.GetDragDropTemplate((DependencyObject) this.dragSourceControl), (UIElement) this.sourceItemContainer, AdornerLayer.GetAdornerLayer((Visual) this.dragSourceControl));
      this.draggedAdorner.SetPosition(currentPosition.X - this.initialMouseDragStartPosition.X + this.initialMouseOffset.X, currentPosition.Y - this.initialMouseDragStartPosition.Y + this.initialMouseOffset.Y);
    }

    private void RemoveDraggedAdorner()
    {
      if (this.draggedAdorner == null)
        return;
      this.draggedAdorner.Detach();
      this.draggedAdorner = (DraggedAdorner) null;
    }

    private void CreateErrorAdorner(string error, Control dropTarget)
    {
      if (this.dropErrorAdorner != null)
        return;
      AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer((Visual) this.dragSourceControl);
      this.dropErrorAdorner = new DropErrorAdorner(error, DragDropHelper.GetDropErrorTemplate((DependencyObject) dropTarget), (UIElement) this.sourceItemContainer, adornerLayer);
    }

    private void UpdateErrorAdorner(Point currentPosition)
    {
      if (this.dropErrorAdorner == null)
        return;
      this.dropErrorAdorner.SetPosition(currentPosition.X - this.initialMouseDragStartPosition.X + this.initialMouseOffset.X, currentPosition.Y - this.initialMouseDragStartPosition.Y + this.initialMouseOffset.Y);
    }

    private void RemoveErrorAdorner()
    {
      if (this.dropErrorAdorner == null)
        return;
      this.dropErrorAdorner.Detach();
      this.dropErrorAdorner = (DropErrorAdorner) null;
    }

    private void CreateInsertionAdorner()
    {
      if (this.targetItemContainer == null)
        return;
      this.insertionAdorner = new InsertionAdorner(this.hasVerticalOrientation, this.isInFirstHalf, (UIElement) this.targetItemContainer, AdornerLayer.GetAdornerLayer((Visual) this.targetItemContainer));
    }

    private void UpdateInsertionAdornerPosition()
    {
      if (this.insertionAdorner == null)
        return;
      this.insertionAdorner.IsInFirstHalf = this.isInFirstHalf;
      this.insertionAdorner.InvalidateVisual();
    }

    private void RemoveInsertionAdorner()
    {
      if (this.insertionAdorner == null)
        return;
      this.insertionAdorner.Detach();
      this.insertionAdorner = (InsertionAdorner) null;
    }
  }
}
