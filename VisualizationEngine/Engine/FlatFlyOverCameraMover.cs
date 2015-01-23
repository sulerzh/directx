// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.FlatFlyOverCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class FlatFlyOverCameraMover : FlatLineCameraMover
  {
    internal FlatFlyOverCameraMover(CameraSnapshot midSnapshot, double extentFactor, TimeSpan moveDuration)
      : base(midSnapshot, moveDuration)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "FlatFlyOverCameraMover constructor.");
      this.path = new MercatorLine(midSnapshot.Longitude, midSnapshot.Latitude, midSnapshot.Rotation, this.GetLength(extentFactor));
    }
  }
}
