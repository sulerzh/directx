﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceBlockQueryParameters
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class InstanceBlockQueryParameters
  {
    public InstanceBlockQueryType QueryType { get; set; }

    public StreamBuffer InstanceOutputBuffer { get; set; }

    public bool IsPieChart { get; set; }

    public bool IsClusterChart { get; set; }

    public float Offset { get; set; }

    public bool IgnoreInstanceValues { get; set; }

    public bool ShowOnlyMaxValues { get; set; }

    public bool UseLogScale { get; set; }

    public InstanceBlockQueryInstanceSource InstanceSource { get; set; }

    public bool HitTest { get; set; }
  }
}
