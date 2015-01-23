using System.Windows;
using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  [ValueConversion(typeof (object), typeof (Visibility))]
  public class ReferenceNullToVisibilityConverter : ReferenceNullToValueConverter<Visibility>
  {
  }
}
