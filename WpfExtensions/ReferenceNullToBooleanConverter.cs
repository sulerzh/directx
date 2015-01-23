using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  [ValueConversion(typeof (object), typeof (bool))]
  public class ReferenceNullToBooleanConverter : ReferenceNullToValueConverter<bool>
  {
  }
}
