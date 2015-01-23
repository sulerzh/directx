using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public static class SpacingHelper
  {
    public static readonly DependencyProperty ChildSpacingProperty = DependencyProperty.RegisterAttached("ChildSpacing", typeof (Thickness), typeof (SpacingHelper), (PropertyMetadata) new UIPropertyMetadata(new PropertyChangedCallback(SpacingHelper.ChildSpacingPropertyChanged)));

    public static Thickness GetChildSpacing(DependencyObject obj)
    {
      return (Thickness) obj.GetValue(SpacingHelper.ChildSpacingProperty);
    }

    public static void SetChildSpacing(DependencyObject obj, bool value)
    {
      obj.SetValue(SpacingHelper.ChildSpacingProperty, value);
    }

    private static void ChildSpacingPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      Panel panel = obj as Panel;
      if (panel == null)
        return;
      panel.Loaded += new RoutedEventHandler(SpacingHelper.Panel_Loaded);
    }

    private static void Panel_Loaded(object sender, RoutedEventArgs e)
    {
      Panel panel = sender as Panel;
      foreach (FrameworkElement frameworkElement in Enumerable.OfType<FrameworkElement>((IEnumerable) panel.Children))
      {
        Thickness margin = frameworkElement.Margin;
        Thickness childSpacing = SpacingHelper.GetChildSpacing((DependencyObject) panel);
        frameworkElement.Margin = new Thickness(margin.Left + childSpacing.Left, margin.Top + childSpacing.Top, margin.Right + childSpacing.Right, margin.Bottom + childSpacing.Bottom);
      }
    }
  }
}
