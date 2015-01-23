// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.IHitTestable
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public interface IHitTestable
  {
    int Id { get; }

    bool HitTestRequested { get; set; }

    bool RenderInFront { get; }

    object DrawHitTest(Renderer renderer, SceneState state, DepthStencilState depthStencil, BlendState blend, RasterizerState rasterizer, IHitTestManager hitTestManager);

    bool OnHitTestPass(InstanceId hitId, SceneState state, object context, out Vector3F objectPos);

    void OnHitTestFail(SceneState state, object context, bool anyHitTestReturnedTrue);
  }
}
