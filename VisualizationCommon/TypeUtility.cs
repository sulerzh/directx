using System;
using System.Reflection;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public static class TypeUtility
  {
    public static bool PropertyImplementsInterface(object instance, string propName, Type interfaceType)
    {
      return TypeUtility.PropertyImplementsInterface(instance.GetType().GetProperty(propName), interfaceType);
    }

    public static bool PropertyImplementsInterface(PropertyInfo propInfo, Type interfaceType)
    {
      if ((PropertyInfo) null == propInfo)
        return false;
      else
        return TypeUtility.ImplementsInterface(propInfo.PropertyType, interfaceType);
    }

    public static bool ImplementsInterface(Type instanceType, Type interfaceType)
    {
      if ((Type) null == instanceType)
        return false;
      if (instanceType.Equals(interfaceType))
        return true;
      foreach (Type type in instanceType.GetInterfaces())
      {
        if (type.Equals(interfaceType))
          return true;
      }
      return false;
    }
  }
}
