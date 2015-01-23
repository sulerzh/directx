// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionEventArgs
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  public class RegionEventArgs : EventArgs
  {
    public int TessellatedCount { get; private set; }

    public InstanceId[] FailedRegions { get; private set; }

    public InstanceId[] EmptyRegions { get; private set; }

    public bool Completed { get; private set; }

    internal RegionEventArgs(int successCount, List<InstanceId> failedRegions, List<InstanceId> emptyRegions, bool completed)
    {
      InstanceId[] array1 = new InstanceId[failedRegions.Count];
      InstanceId[] array2 = new InstanceId[emptyRegions.Count];
      failedRegions.CopyTo(0, array1, 0, array1.Length);
      emptyRegions.CopyTo(0, array2, 0, array2.Length);
      this.TessellatedCount = successCount;
      this.FailedRegions = array1;
      this.EmptyRegions = array2;
      this.Completed = completed;
    }
  }
}
