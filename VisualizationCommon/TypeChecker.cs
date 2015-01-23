using System;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class TypeChecker<T> : ITypeChecker
  {
    public bool IsOfMatchingType(object obj)
    {
      try
      {
        T obj1 = (T) obj;
        return true;
      }
      catch (InvalidCastException ex)
      {
        return false;
      }
      catch (NullReferenceException ex)
      {
        return false;
      }
    }
  }
}
