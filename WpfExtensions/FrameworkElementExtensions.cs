using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public static class FrameworkElementExtensions
  {
    public static bool HasVerticalOrientation(this FrameworkElement element)
    {
      bool flag = true;
      if (element != null)
      {
        Panel panel = VisualTreeHelper.GetParent((DependencyObject) element) as Panel;
        StackPanel stackPanel;
        if ((stackPanel = panel as StackPanel) != null)
        {
          flag = stackPanel.Orientation == Orientation.Vertical;
        }
        else
        {
          WrapPanel wrapPanel;
          if ((wrapPanel = panel as WrapPanel) != null)
            flag = wrapPanel.Orientation == Orientation.Vertical;
        }
      }
      return flag;
    }

    public static bool IsInFirstHalf(this FrameworkElement container, Point clickedPoint, bool hasVerticalOrientation)
    {
      if (hasVerticalOrientation)
        return clickedPoint.Y < container.ActualHeight / 2.0;
      else
        return clickedPoint.X < container.ActualWidth / 2.0;
    }

    public static Point GetMouseScreenPosition(this FrameworkElement element)
    {
      return element.PointToScreen(Mouse.GetPosition((IInputElement) element));
    }

    public static T FindDescendant<T>(this Visual element) where T : Visual
    {
      if (element == null)
        return default (T);
      T obj = element as T;
      if ((object) obj != null)
        return obj;
      for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount((DependencyObject) element); ++childIndex)
      {
        T descendant = FrameworkElementExtensions.FindDescendant<T>(VisualTreeHelper.GetChild((DependencyObject) element, childIndex) as Visual);
        if ((object) descendant != null)
          return descendant;
      }
      return default (T);
    }
  }
}
