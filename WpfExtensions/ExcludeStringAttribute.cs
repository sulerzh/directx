using Microsoft.Data.Visualization.VisualizationCommon;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public sealed class ExcludeStringAttribute : PropertyValidationAttribute
  {
    public string Exclude { get; set; }

    public ExcludeStringAttribute()
      : base((ITypeChecker) new TypeChecker<string>())
    {
    }

    protected override bool ValidateInternal(object toValidate, object context)
    {
      return this.Exclude != (string) toValidate;
    }
  }
}
