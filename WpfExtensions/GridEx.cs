using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class GridEx
  {
    private static string RowDefinitionAuto = "Auto";
    private static string RowDefinitionStarString = "*";
    public static readonly DependencyProperty RowDefsProperty = DependencyProperty.RegisterAttached("RowDefs", typeof (string), typeof (GridEx), (PropertyMetadata) new UIPropertyMetadata(new PropertyChangedCallback(GridEx.OnRowDefsChanged)));
    public static readonly DependencyProperty ColDefsProperty = DependencyProperty.RegisterAttached("ColDefs", typeof (string), typeof (GridEx), (PropertyMetadata) new UIPropertyMetadata(new PropertyChangedCallback(GridEx.OnColDefsChanged)));

    public static void SetRowDefs(UIElement element, string val)
    {
      element.SetValue(GridEx.RowDefsProperty, (object) val);
    }

    public static string GetRowDefs(UIElement element)
    {
      return (string) element.GetValue(GridEx.RowDefsProperty);
    }

    private static void OnRowDefsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
      Grid grid = dependencyObject as Grid;
      if (grid == null || args.NewValue == args.OldValue)
        return;
      grid.RowDefinitions.Clear();
      foreach (GridLength gridLength in GridEx.ParseGridLengthsFromString((string) args.NewValue))
        grid.RowDefinitions.Add(new RowDefinition()
        {
          Height = gridLength
        });
    }

    public static void SetColDefs(UIElement element, string val)
    {
      element.SetValue(GridEx.ColDefsProperty, (object) val);
    }

    public static string GetColDefs(UIElement element)
    {
      return (string) element.GetValue(GridEx.ColDefsProperty);
    }

    private static void OnColDefsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
      Grid grid = dependencyObject as Grid;
      if (grid == null || args.NewValue == args.OldValue)
        return;
      grid.ColumnDefinitions.Clear();
      foreach (GridLength gridLength in GridEx.ParseGridLengthsFromString((string) args.NewValue))
        grid.ColumnDefinitions.Add(new ColumnDefinition()
        {
          Width = gridLength
        });
    }

    private static IEnumerable<GridLength> ParseGridLengthsFromString(string value)
    {
      List<GridLength> list = new List<GridLength>();
      if (string.IsNullOrEmpty(value))
        return (IEnumerable<GridLength>) list;
      string str = value;
      char[] chArray = new char[1]
      {
        ','
      };
      foreach (string s in str.Split(chArray))
      {
        if (s == GridEx.RowDefinitionAuto)
          list.Add(GridLength.Auto);
        else if (s.Contains(GridEx.RowDefinitionStarString))
        {
          double result = 1.0;
          if (s.Length > 1)
            double.TryParse(s.Substring(0, s.Length - 1), out result);
          list.Add(new GridLength(result, GridUnitType.Star));
        }
        else
        {
          double result = 0.0;
          if (double.TryParse(s, out result))
            list.Add(new GridLength(result));
        }
      }
      return (IEnumerable<GridLength>) list;
    }
  }
}
