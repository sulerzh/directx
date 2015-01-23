using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Reflection;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
  public abstract class PropertyValidationAttribute : ResourceStringAttribute, IPropertyValidationAttribute, IResourceString, IValidationRule
  {
    public string Condition { get; set; }

    public string ErrorMessage
    {
      get
      {
        return this.ResourceValue;
      }
    }

    private ITypeChecker ValidationType { get; set; }

    public PropertyValidationAttribute()
      : this((ITypeChecker) new TypeChecker<object>())
    {
    }

    protected PropertyValidationAttribute(ITypeChecker validationType)
    {
      if (validationType == null)
        throw new ArgumentNullException("validationType");
      this.ValidationType = validationType;
    }

    public bool IsConditionMet(object instance)
    {
      if (string.IsNullOrEmpty(this.Condition))
        return true;
      PropertyInfo property = instance.GetType().GetProperty(this.Condition, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if ((PropertyInfo) null == property)
        return true;
      object obj = property.GetValue(instance, (object[]) null);
      if (obj is bool)
        return (bool) obj;
      else
        return true;
    }

    public bool IsValid(object toValidate)
    {
      return this.IsValid(toValidate, (object) null);
    }

    public bool IsValid(object toValidate, object context)
    {
      if (context != null && !this.IsConditionMet(context))
        return true;
      if (this.ValidationType.IsOfMatchingType(toValidate))
        return this.ValidateInternal(toValidate, context);
      else
        return false;
    }

    protected abstract bool ValidateInternal(object toValidate, object context);
  }
}
