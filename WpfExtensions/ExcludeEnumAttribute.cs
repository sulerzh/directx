using Microsoft.Data.Visualization.VisualizationCommon;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public sealed class ExcludeEnumAttribute : PropertyValidationAttribute
  {
    public int Exclude { get; set; }

    public ExcludeEnumAttribute()
      : base((ITypeChecker) new TypeChecker<int>())
    {
    }

    protected override bool ValidateInternal(object toValidate, object context)
    {
      return this.Exclude != (int) toValidate;
    }
  }
}
