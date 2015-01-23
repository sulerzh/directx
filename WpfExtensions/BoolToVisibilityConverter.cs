using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  [ValueConversion(typeof (bool), typeof (Visibility))]
  public class BoolToVisibilityConverter : BoolToValueConverter<Visibility>
  {
    public BoolToVisibilityConverter()
    {
      this.TrueValue = Visibility.Visible;
      this.FalseValue = Visibility.Collapsed;
    }
  }
}
