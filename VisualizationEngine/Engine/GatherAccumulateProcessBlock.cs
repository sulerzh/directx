// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.GatherAccumulateProcessBlock
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class GatherAccumulateProcessBlock
  {
    public IndexBuffer PositiveIndices { get; set; }

    public IndexBuffer NegativeIndices { get; set; }

    public VertexBuffer Instances { get; set; }

    public VertexBuffer InstancesTime { get; set; }

    public VertexBuffer InstancesHitId { get; set; }

    public int MaxShift { get; set; }

    public Tuple<uint, uint> PositiveSubset { get; set; }

    public Tuple<uint, uint> NegativeSubset { get; set; }

    public InstanceBlock Owner { get; set; }
  }
}
