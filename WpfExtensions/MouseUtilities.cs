using System;
using System.Windows;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public static class MouseUtilities
  {
    public static bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
    {
      if (Math.Abs(currentPosition.X - initialMousePosition.X) < SystemParameters.MinimumHorizontalDragDistance)
        return Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance;
      else
        return true;
    }
  }
}
