// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RangeExtensions
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Visualization.Engine
{
  public static class RangeExtensions
  {
    public static RangeOf<double> RangeOfNaN
    {
      get
      {
        return new RangeOf<double>(double.NaN, double.NaN);
      }
    }

    public static RangeOf<double> RangeExcludingNaNs(this IEnumerable<double> ar)
    {
      if (ar == null)
        return RangeExtensions.RangeOfNaN;
      IEnumerable<double> source = Enumerable.Where<double>(ar, (Func<double, bool>) (k => !double.IsNaN(k)));
      if (!Enumerable.Any<double>(source))
        return RangeExtensions.RangeOfNaN;
      else
        return new RangeOf<double>(Enumerable.Min(source), Enumerable.Max(source));
    }
  }
}
