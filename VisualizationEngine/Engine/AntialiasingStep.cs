// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.AntialiasingStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal class AntialiasingStep : EngineStep
  {
    private AntialiasingRenderer antialiasRenderer;

    public AntialiasingStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher)
      : base(dispatcher, eventDispatcher)
    {
    }

    internal override bool PreExecute(SceneState state, int phase)
    {
      return false;
    }

    internal override void Execute(Renderer renderer, SceneState state, int phase)
    {
      if (state.GraphicsLevel == GraphicsLevel.Speed || renderer.Msaa)
        return;
      if (this.antialiasRenderer == null)
        this.antialiasRenderer = new AntialiasingRenderer();
      switch (state.GraphicsLevel)
      {
        case GraphicsLevel.Balanced:
          this.antialiasRenderer.Quality = AntialiasingQuality.Medium1;
          break;
        case GraphicsLevel.Balanced2:
          this.antialiasRenderer.Quality = AntialiasingQuality.Medium2;
          break;
        case GraphicsLevel.Quality:
          this.antialiasRenderer.Quality = AntialiasingQuality.Quality;
          break;
      }
      this.antialiasRenderer.Render(renderer, state);
    }

    public override void Dispose()
    {
      if (this.antialiasRenderer == null)
        return;
      this.antialiasRenderer.Dispose();
      this.antialiasRenderer = (AntialiasingRenderer) null;
    }
  }
}
