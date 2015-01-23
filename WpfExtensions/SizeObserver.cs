using System.Windows;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class SizeObserver
  {
    public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached("Observe", typeof (bool), typeof (SizeObserver), (PropertyMetadata) new UIPropertyMetadata((object) false, new PropertyChangedCallback(SizeObserver.OnObserveChanged)));
    public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached("ObservedWidth", typeof (double), typeof (SizeObserver), (PropertyMetadata) new UIPropertyMetadata((object) 0.0));
    public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached("ObservedHeight", typeof (double), typeof (SizeObserver), (PropertyMetadata) new UIPropertyMetadata((object) 0.0));

    public static bool GetObserve(FrameworkElement element)
    {
      return (bool) element.GetValue(SizeObserver.ObserveProperty);
    }

    public static void SetObserve(FrameworkElement element, bool value)
    {
      element.SetValue(SizeObserver.ObserveProperty, value);
    }

    private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FrameworkElement frameworkElement = d as FrameworkElement;
      if (frameworkElement == null || !(e.NewValue is bool))
        return;
      if ((bool) e.NewValue)
        frameworkElement.SizeChanged += new SizeChangedEventHandler(SizeObserver.OnSizeChanged);
      else
        frameworkElement.SizeChanged -= new SizeChangedEventHandler(SizeObserver.OnSizeChanged);
    }

    private static void OnSizeChanged(object sender, RoutedEventArgs e)
    {
      if (!object.ReferenceEquals(sender, e.OriginalSource))
        return;
      FrameworkElement frameworkElement = e.OriginalSource as FrameworkElement;
      if (frameworkElement == null)
        return;
      SizeObserver.SetObservedWidth((DependencyObject) frameworkElement, frameworkElement.ActualWidth);
      SizeObserver.SetObservedHeight((DependencyObject) frameworkElement, frameworkElement.ActualHeight);
    }

    public static double GetObservedWidth(DependencyObject obj)
    {
      return (double) obj.GetValue(SizeObserver.ObservedWidthProperty);
    }

    public static void SetObservedWidth(DependencyObject obj, double value)
    {
      obj.SetValue(SizeObserver.ObservedWidthProperty, (object) value);
    }

    public static double GetObservedHeight(DependencyObject obj)
    {
      return (double) obj.GetValue(SizeObserver.ObservedHeightProperty);
    }

    public static void SetObservedHeight(DependencyObject obj, double value)
    {
      obj.SetValue(SizeObserver.ObservedHeightProperty, (object) value);
    }
  }
}
