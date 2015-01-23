// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RealtimeTimeProvider
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class RealtimeTimeProvider : ITimeProvider
  {
    private Stopwatch timer = new Stopwatch();

    public RealtimeTimeProvider()
    {
      this.timer.Start();
    }

    public long GetElapsedMilliseconds()
    {
      return this.timer.ElapsedMilliseconds;
    }

    public long GetElapsedTicks()
    {
      return this.timer.ElapsedTicks * 60L / Stopwatch.Frequency;
    }
  }
}
