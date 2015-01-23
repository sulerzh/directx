// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.MaxAbsSumCalculator
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class MaxAbsSumCalculator : MaxSum
  {
    private double sumOfPositives;
    private double sumOfNegatives;

    public MaxAbsSumCalculator(List<InstanceData> instanceList, bool isTimeInvolved)
      : base(instanceList, isTimeInvolved)
    {
    }

    protected override void BeginSlice()
    {
      this.sumOfPositives = this.sumOfNegatives = 0.0;
    }

    protected override void ProcessInstance(InstanceData instance)
    {
      if (double.IsNaN((double) instance.Value))
        return;
      if ((double) instance.Value >= 0.0)
        this.sumOfPositives += (double) instance.Value;
      else
        this.sumOfNegatives += (double) instance.Value;
    }

    protected override void EndSlice()
    {
      this.Max = Math.Max(this.Max, Math.Max(this.sumOfPositives, -this.sumOfNegatives));
    }
  }
}
