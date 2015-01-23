using Microsoft.Data.Visualization.VisualizationCommon;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public sealed class ExcludeChildValidationElementAttribute : PropertyValidationAttribute
  {
    public ExcludeChildValidationElementAttribute()
      : base((ITypeChecker) new TypeChecker<IValidationElement>())
    {
    }

    protected override bool ValidateInternal(object toValidate, object context)
    {
      return true;
    }
  }
}
