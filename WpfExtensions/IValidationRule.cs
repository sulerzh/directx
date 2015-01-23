namespace Microsoft.Data.Visualization.WpfExtensions
{
  public interface IValidationRule
  {
    bool IsValid(object param, object context);
  }
}
