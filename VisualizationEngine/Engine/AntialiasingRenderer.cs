// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.AntialiasingRenderer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class AntialiasingRenderer : DisposableResource
  {
    private Effect[] fxaaEffect = new Effect[4];
    private RenderParameterFloat subpixelRemovalParameter = new RenderParameterFloat("FxaaQualitySubpix");
    private RenderParameterFloat edgeThresholdParameter = new RenderParameterFloat("FxaaQualityEdgeThreshold");
    private RenderParameterFloat minEdgeThresholdParameter = new RenderParameterFloat("FxaaQualityEdgeThresholdMin");
    private Texture lumaTexture;
    private RenderTarget lumaTarget;
    private Effect lumaEffect;
    private RasterizerState rasterizerState;
    private BlendState blendState;
    private DepthStencilState depthStencilState;

    public AntialiasingQuality Quality { get; set; }

    public AntialiasingRenderer()
    {
      this.subpixelRemovalParameter.Value = 0.75f;
      this.edgeThresholdParameter.Value = 0.166f;
      this.minEdgeThresholdParameter.Value = 0.0833f;
    }

    public void Render(Renderer renderer, SceneState state)
    {
      this.EnsureResources(renderer, state);
      renderer.Profiler.BeginSection("[Antialiasing]");
      renderer.SetRasterizerState(this.rasterizerState);
      renderer.SetBlendState(this.blendState);
      renderer.SetDepthStencilState(this.depthStencilState);
      renderer.SetVertexSource((VertexBuffer) null);
      renderer.Profiler.BeginSection("[Antialiasing] Compute Luma");
      renderer.BeginRenderTargetFrame(this.lumaTarget, new Color4F?());
      try
      {
        renderer.SetTextureFromBackBuffer(0);
        renderer.SetEffect(this.lumaEffect);
        renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
        renderer.SetTexture(0, (Texture) null);
      }
      finally
      {
        renderer.EndRenderTargetFrame();
      }
      renderer.Profiler.EndSection();
      renderer.Profiler.BeginSection("[Antialiasing] FXAA");
      renderer.SetTexture(0, this.lumaTexture);
      renderer.SetEffect(this.fxaaEffect[(int) this.Quality]);
      renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
      renderer.Profiler.EndSection();
      renderer.Profiler.EndSection();
    }

    private void EnsureResources(Renderer renderer, SceneState state)
    {
      if (this.lumaTexture != null && ((double) this.lumaTexture.Width != state.ScreenWidth || (double) this.lumaTexture.Height != state.ScreenHeight))
      {
        this.lumaTexture.Dispose();
        this.lumaTexture = (Texture) null;
        this.lumaTarget.Dispose();
        this.lumaTarget = (RenderTarget) null;
      }
      if (this.lumaTexture == null)
      {
        using (Image textureData = new Image(IntPtr.Zero, (int) state.ScreenWidth, (int) state.ScreenHeight, PixelFormat.Rgba32Bpp))
          this.lumaTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
        this.lumaTarget = RenderTarget.Create(this.lumaTexture, RenderTargetDepthStencilMode.None);
      }
      if (this.lumaEffect == null)
        this.lumaEffect = Effect.Create(new EffectDefinition()
        {
          VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.Luma.vs"),
          PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.Luma.ps"),
          VertexFormat = (VertexFormat) null,
          Samplers = (TextureSampler[]) null,
          Parameters = (RenderParameters) null
        });
      for (int index = 0; index < this.fxaaEffect.Length; ++index)
      {
        if (this.fxaaEffect[index] == null)
          this.fxaaEffect[index] = Effect.Create(new EffectDefinition()
          {
            VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.Fxaa.vs"),
            PixelShaderData = this.GetType().Assembly.GetManifestResourceStream(string.Format("Microsoft.Data.Visualization.Engine.Shaders.Compiled.FxaaQ{0}.ps", (object) index)),
            VertexFormat = (VertexFormat) null,
            Samplers = new TextureSampler[1]
            {
              TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(0.0f, 0.0f, 0.0f, 0.0f))
            },
            Parameters = RenderParameters.Create(new IRenderParameter[3]
            {
              (IRenderParameter) this.subpixelRemovalParameter,
              (IRenderParameter) this.edgeThresholdParameter,
              (IRenderParameter) this.minEdgeThresholdParameter
            })
          });
      }
      if (this.rasterizerState != null)
        return;
      this.rasterizerState = RasterizerState.Create(new RasterizerStateDescription()
      {
        CullMode = CullMode.None,
        DepthClipEnable = false,
        FillMode = FillMode.Solid,
        MultisampleEnable = false
      });
      this.blendState = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = false
      });
      this.depthStencilState = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthEnable = false,
        DepthWriteEnable = false,
        StencilEnable = false
      });
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      DisposableResource[] disposableResourceArray = new DisposableResource[6]
      {
        (DisposableResource) this.lumaTexture,
        (DisposableResource) this.lumaTarget,
        (DisposableResource) this.lumaEffect,
        (DisposableResource) this.rasterizerState,
        (DisposableResource) this.blendState,
        (DisposableResource) this.depthStencilState
      };
      foreach (DisposableResource disposableResource in disposableResourceArray)
      {
        if (disposableResource != null)
          disposableResource.Dispose();
      }
      foreach (Effect effect in this.fxaaEffect)
      {
        if (effect != null)
          effect.Dispose();
      }
    }
  }
}
