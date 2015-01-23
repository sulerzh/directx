using System;
using System.Reflection;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  [AttributeUsage(AttributeTargets.Field)]
  public class DisplayStringAttribute : Attribute
  {
    public string Value { get; private set; }

    public DisplayStringAttribute(Type resourceManagerType, string resourceKey)
    {
      PropertyInfo property = resourceManagerType.GetProperty(resourceKey);
      if (property == (PropertyInfo) null)
        property = resourceManagerType.GetProperty(resourceKey, BindingFlags.Static | BindingFlags.NonPublic);
      this.Value = property.GetValue((object) null, (object[]) null) as string;
    }
  }
}
