// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.DrawParameters
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  public class DrawParameters
  {
    public StreamBuffer SourceStreamBuffer { get; internal set; }

    public StreamBuffer SourceIdStreamBuffer { get; internal set; }

    public int InstanceCount { get; internal set; }

    public IInstancePositionProvider PositionProvider { get; internal set; }

    public InstanceBlockQueryType QueryType { get; internal set; }

    public ushort RenderPriority { get; internal set; }

    public DrawStyle DrawStyle { get; internal set; }

    public float FrameId { get; internal set; }

    public DepthStencilState DepthStencil { get; internal set; }

    public BlendState Blend { get; internal set; }

    public RasterizerState Rasterizer { get; internal set; }

    public LayerRenderingParameters RenderingOptions { get; internal set; }
  }
}
