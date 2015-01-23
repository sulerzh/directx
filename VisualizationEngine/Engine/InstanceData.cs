// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceData
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class InstanceData
  {
    public Coordinates Location;
    public float Value;
    public short Color;
    public short Shift;
    public short SourceShift;
    public DateTime? StartTime;
    public DateTime? EndTime;
    public InstanceId Id;
    public bool FirstInstance;
  }
}
