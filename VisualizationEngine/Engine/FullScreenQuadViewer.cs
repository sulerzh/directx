// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.FullScreenQuadViewer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System.IO;

namespace Microsoft.Data.Visualization.Engine
{
  internal class FullScreenQuadViewer : DisposableResource
  {
    private VertexBuffer screenQuad;
    private TextureSampler sampler;
    private Effect effect;
    private DepthStencilState depthState;
    private BlendState blendState;
    private RasterizerState rasterizerState;

    public FullScreenQuadViewer()
    {
      this.BuildVertices();
      this.InitializeEffect();
      this.InitializeStates();
    }

    private void BuildVertices()
    {
      this.screenQuad = VertexBuffer.Create<Vertex.Position2D>(new Vertex.Position2D[4]
      {
        new Vertex.Position2D()
        {
          X = -1f,
          Y = 1f
        },
        new Vertex.Position2D()
        {
          X = 1f,
          Y = 1f
        },
        new Vertex.Position2D()
        {
          X = -1f,
          Y = -1f
        },
        new Vertex.Position2D()
        {
          X = 1f,
          Y = -1f
        }
      }, false);
    }

    private void InitializeEffect()
    {
      this.sampler = TextureSampler.Create(TextureFilter.Point, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(1f, 1f, 0.0f, 0.0f));
      this.effect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.FullScreenQuad.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.FullScreenQuad.ps"),
        GeometryShaderData = (Stream) null,
        VertexFormat = VertexFormats.Position2D,
        Samplers = new TextureSampler[1]
        {
          this.sampler
        },
        Parameters = (RenderParameters) null
      });
    }

    private void InitializeStates()
    {
      this.rasterizerState = RasterizerState.Create(new RasterizerStateDescription()
      {
        CullMode = CullMode.None,
        ScissorEnable = false
      });
      this.blendState = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = false
      });
      this.depthState = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthEnable = true,
        DepthFunction = ComparisonFunction.Always,
        DepthWriteEnable = true,
        StencilEnable = false
      });
    }

    public void Render(Texture texture, Renderer renderer)
    {
      renderer.SetRasterizerState(this.rasterizerState);
      renderer.SetBlendState(this.blendState);
      renderer.SetDepthStencilState(this.depthState);
      renderer.SetEffect(this.effect);
      renderer.SetTexture(0, texture);
      renderer.SetVertexSource(this.screenQuad);
      renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
      renderer.SetTexture(0, (Texture) null);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      this.rasterizerState.Dispose();
      this.blendState.Dispose();
      this.depthState.Dispose();
      this.screenQuad.Dispose();
      this.sampler.Dispose();
      this.effect.Dispose();
    }
  }
}
