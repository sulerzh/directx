namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class InvertBoolConverter : BoolToValueConverter<bool>
  {
    public InvertBoolConverter()
    {
      this.TrueValue = false;
      this.FalseValue = true;
    }
  }
}
