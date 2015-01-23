// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.MaxSum
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class MaxSum : TimeSlicer
  {
    public double Max { get; protected set; }

    public MaxSum(List<InstanceData> instanceList, bool isTimeInvolved)
      : base(instanceList, isTimeInvolved)
    {
    }

    protected override void Initialize()
    {
      this.Max = 0.0;
    }
  }
}
