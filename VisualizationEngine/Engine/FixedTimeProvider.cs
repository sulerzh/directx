// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.FixedTimeProvider
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

namespace Microsoft.Data.Visualization.Engine
{
  internal class FixedTimeProvider : ITimeProvider
  {
    private readonly double frametime;
    private long elapsedFrames;

    public FixedTimeProvider(int framerate)
    {
      this.frametime = 1.0 / (double) framerate;
    }

    public void IncrementFrame()
    {
      ++this.elapsedFrames;
    }

    public long GetElapsedMilliseconds()
    {
      return (long) ((double) this.elapsedFrames * this.frametime * 1000.0);
    }

    public long GetElapsedTicks()
    {
      return (long) ((double) (this.elapsedFrames * 60L) * this.frametime);
    }
  }
}
