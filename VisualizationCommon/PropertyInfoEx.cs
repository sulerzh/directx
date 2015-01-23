using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public static class PropertyInfoEx
  {
    private static readonly ConcurrentDictionary<Tuple<Type, string>, PropertyInfo> Infos = new ConcurrentDictionary<Tuple<Type, string>, PropertyInfo>();

    internal static PropertyInfo GetCachedPropertyInfo(this Type type, string propertyName)
    {
      return PropertyInfoEx.Infos.GetOrAdd(Tuple.Create<Type, string>(type, propertyName), (Func<Tuple<Type, string>, PropertyInfo>) (t => t.Item1.GetProperty(t.Item2)));
    }

    public static bool TryGetCachedPropertyInfo(this Type type, string propertyName, out PropertyInfo info)
    {
      return PropertyInfoEx.Infos.TryGetValue(Tuple.Create<Type, string>(type, propertyName), out info);
    }
  }
}
