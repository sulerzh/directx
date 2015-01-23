using System.Windows.Data;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  [ValueConversion(typeof (object), typeof (int))]
  public class ReferenceNullToIntegerConverter : ReferenceNullToValueConverter<int>
  {
  }
}
