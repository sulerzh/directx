// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.DarkGlowRenderer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.IO;

namespace Microsoft.Data.Visualization.Engine
{
  internal class DarkGlowRenderer : DisposableResource
  {
    private Texture infoRenderTargetTextureSource;
    private RenderTarget infoRenderTargetSource;
    private Texture depthRenderTargetTextureResolved;
    private Texture depthRenderTargetTextureSource;
    private RenderTarget depthRenderTargetResolved;
    private RenderTarget depthRenderTargetSource;
    private RenderParameterFloat occlusionDepthFactor;
    private VertexBuffer screenQuad;
    private Effect effectMultisample;
    private Effect effect;
    private Effect depthResolveEffect;
    private DepthStencilState depthState;
    private BlendState blendState;
    private BlendState resolveBlendState;
    private RasterizerState rasterizerState;

    public DarkGlowRenderer()
    {
      this.occlusionDepthFactor = new RenderParameterFloat("OcclusionDepthFactor");
      this.BuildVertices();
      this.InitializeEffect();
      this.InitializeStates();
    }

    private void BuildVertices()
    {
      this.screenQuad = VertexBuffer.Create<Vertex.Position2D>(new Vertex.Position2D[4]
      {
        new Vertex.Position2D(-1f, 1f),
        new Vertex.Position2D(1f, 1f),
        new Vertex.Position2D(-1f, -1f),
        new Vertex.Position2D(1f, -1f)
      }, false);
    }

    private void InitializeEffect()
    {
      TextureSampler textureSampler1 = TextureSampler.Create(TextureFilter.Point, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(1f, 1f, 0.0f, 0.0f));
      TextureSampler textureSampler2 = TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(0.0f, 0.0f, 0.0f, 0.0f), ComparisonFunction.Greater);
      this.effectMultisample = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.FullScreenQuad.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.DarkGlowMultisample.ps"),
        GeometryShaderData = (Stream) null,
        VertexFormat = VertexFormats.Position2D,
        Samplers = new TextureSampler[2]
        {
          textureSampler1,
          textureSampler2
        },
        Parameters = RenderParameters.Create(new IRenderParameter[1]
        {
          (IRenderParameter) this.occlusionDepthFactor
        })
      });
      this.effect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.FullScreenQuad.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.DarkGlow.ps"),
        GeometryShaderData = (Stream) null,
        VertexFormat = VertexFormats.Position2D,
        Samplers = new TextureSampler[2]
        {
          textureSampler1,
          textureSampler2
        },
        Parameters = RenderParameters.Create(new IRenderParameter[1]
        {
          (IRenderParameter) this.occlusionDepthFactor
        })
      });
      this.depthResolveEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.FullScreenQuad.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.SimpleResolve.ps"),
        GeometryShaderData = (Stream) null,
        VertexFormat = VertexFormats.Position2D,
        Samplers = new TextureSampler[1]
        {
          textureSampler1
        },
        Parameters = (RenderParameters) null
      });
    }

    private void InitializeStates()
    {
      this.rasterizerState = RasterizerState.Create(new RasterizerStateDescription()
      {
        CullMode = CullMode.None,
        ScissorEnable = false,
        MultisampleEnable = true
      });
      this.blendState = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = true,
        SourceBlend = BlendFactor.SourceAlpha,
        SourceBlendAlpha = BlendFactor.One,
        DestBlend = BlendFactor.InvSourceAlpha,
        DestBlendAlpha = BlendFactor.One,
        BlendOp = BlendOperation.Add,
        BlendOpAlpha = BlendOperation.Add,
        WriteMask = RenderTargetWriteMask.All
      });
      this.resolveBlendState = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = false
      });
      this.depthState = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthEnable = false,
        StencilEnable = false
      });
    }

    public RenderTarget[] GetRenderTarget(Renderer renderer, SceneState state)
    {
      this.EnsureRenderTarget(renderer, state);
      return new RenderTarget[2]
      {
        this.infoRenderTargetSource,
        this.depthRenderTargetSource
      };
    }

    public bool IsEnabled(SceneState state)
    {
      if (state.GraphicsLevel == GraphicsLevel.Speed)
        return false;
      this.occlusionDepthFactor.Value = Math.Max(0.0f, Math.Min(1f, (float) Math.Cos(7.0 * Math.PI / 12.0 - Math.Abs(state.CameraSnapshot.PivotAngle)) * 2f));
      return (double) this.occlusionDepthFactor.Value != 0.0;
    }

    public void Render(Renderer renderer, SceneState state)
    {
      if (!this.IsEnabled(state))
        return;
      renderer.Profiler.BeginSection("[Layers] Dark Glow");
      renderer.SetRasterizerState(this.rasterizerState);
      renderer.SetDepthStencilState(this.depthState);
      renderer.SetVertexSource(this.screenQuad);
      if (renderer.Msaa)
        this.DepthTextureResolve(renderer);
      renderer.SetBlendState(this.blendState);
      renderer.SetEffect(renderer.Msaa ? this.effectMultisample : this.effect);
      renderer.SetTexture(0, this.infoRenderTargetTextureSource);
      renderer.SetTexture(1, renderer.Msaa ? this.depthRenderTargetTextureResolved : this.depthRenderTargetTextureSource);
      renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
      renderer.SetTexture(0, (Texture) null);
      renderer.Profiler.EndSection();
    }

    private void DepthTextureResolve(Renderer renderer)
    {
      renderer.Profiler.BeginSection("[Layers] Texture resolve");
      renderer.SetBlendState(this.resolveBlendState);
      renderer.SetEffect(this.depthResolveEffect);
      renderer.SetTexture(0, this.depthRenderTargetTextureSource);
      renderer.BeginRenderTargetFrame(this.depthRenderTargetResolved, new Color4F?());
      renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
      renderer.EndRenderTargetFrame();
      renderer.Profiler.EndSection();
    }

    private void EnsureRenderTarget(Renderer renderer, SceneState state)
    {
      if (this.infoRenderTargetTextureSource != null && (this.infoRenderTargetSource.RenderTargetTexture.Width != (int) state.ScreenWidth || this.infoRenderTargetSource.RenderTargetTexture.Height != (int) state.ScreenHeight || this.infoRenderTargetSource.RenderTargetTexture.Usage == TextureUsage.MultiSampledRenderTarget != renderer.Msaa))
      {
        this.infoRenderTargetSource.Dispose();
        this.infoRenderTargetSource = (RenderTarget) null;
        this.infoRenderTargetTextureSource.Dispose();
        this.infoRenderTargetTextureSource = (Texture) null;
        if (this.depthRenderTargetResolved != null)
        {
          this.depthRenderTargetResolved.Dispose();
          this.depthRenderTargetResolved = (RenderTarget) null;
        }
        this.depthRenderTargetSource.Dispose();
        this.depthRenderTargetSource = (RenderTarget) null;
        if (this.depthRenderTargetTextureResolved != null)
        {
          this.depthRenderTargetTextureResolved.Dispose();
          this.depthRenderTargetTextureResolved = (Texture) null;
        }
        this.depthRenderTargetTextureSource.Dispose();
        this.depthRenderTargetTextureSource = (Texture) null;
      }
      if (this.infoRenderTargetTextureSource != null)
        return;
      int width = (int) state.ScreenWidth;
      int height = (int) state.ScreenHeight;
      using (Image textureData = new Image(IntPtr.Zero, width, height, PixelFormat.R16Unorm16bpp))
        this.infoRenderTargetTextureSource = renderer.CreateTexture(textureData, false, false, renderer.Msaa ? TextureUsage.MultiSampledRenderTarget : TextureUsage.RenderTarget);
      if (this.infoRenderTargetTextureSource != null)
        this.infoRenderTargetSource = RenderTarget.Create(this.infoRenderTargetTextureSource, RenderTargetDepthStencilMode.None);
      using (Image textureData = new Image(IntPtr.Zero, width, height, PixelFormat.Float32Bpp))
        this.depthRenderTargetTextureSource = renderer.CreateTexture(textureData, false, false, renderer.Msaa ? TextureUsage.MultiSampledRenderTarget : TextureUsage.RenderTarget);
      if (this.depthRenderTargetTextureSource != null)
        this.depthRenderTargetSource = RenderTarget.Create(this.depthRenderTargetTextureSource, RenderTargetDepthStencilMode.None);
      if (!renderer.Msaa)
        return;
      using (Image textureData = new Image(IntPtr.Zero, width, height, PixelFormat.Float32Bpp))
        this.depthRenderTargetTextureResolved = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
      if (this.depthRenderTargetTextureResolved == null)
        return;
      this.depthRenderTargetResolved = RenderTarget.Create(this.depthRenderTargetTextureResolved, RenderTargetDepthStencilMode.None);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      DisposableResource[] disposableResourceArray = new DisposableResource[14]
      {
        (DisposableResource) this.infoRenderTargetSource,
        (DisposableResource) this.infoRenderTargetTextureSource,
        (DisposableResource) this.depthRenderTargetResolved,
        (DisposableResource) this.depthRenderTargetSource,
        (DisposableResource) this.depthRenderTargetTextureResolved,
        (DisposableResource) this.depthRenderTargetTextureSource,
        (DisposableResource) this.rasterizerState,
        (DisposableResource) this.blendState,
        (DisposableResource) this.depthState,
        (DisposableResource) this.resolveBlendState,
        (DisposableResource) this.screenQuad,
        (DisposableResource) this.effect,
        (DisposableResource) this.effectMultisample,
        (DisposableResource) this.depthResolveEffect
      };
      foreach (DisposableResource disposableResource in disposableResourceArray)
      {
        if (disposableResource != null)
          disposableResource.Dispose();
      }
    }
  }
}
