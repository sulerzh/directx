namespace Microsoft.Data.Visualization.WpfExtensions
{
  public interface IPropertyValidationAttribute : IResourceString, IValidationRule
  {
    string Condition { get; set; }

    string ErrorMessage { get; }

    bool IsConditionMet(object instance);
  }
}
