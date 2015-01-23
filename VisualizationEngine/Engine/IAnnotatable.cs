// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.IAnnotatable
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  internal interface IAnnotatable
  {
    AnnotationStyle Style { get; set; }

    bool IsAnnotationDirty { get; }

    void SetAnnotationImageSource(IAnnotationImageSource imageSource);

    void DrawAnnotation(Renderer renderer, SceneState state, bool blockUntilComplete);

    void DrawAnnotationHitTest(Renderer renderer, SceneState state, DepthStencilState depthStencil, BlendState blendState, RasterizerState rasterizer);
  }
}
