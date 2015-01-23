// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ShadowStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal class ShadowStep : EngineStep, IShadowManager
  {
    private List<IShadowCaster> shadowCasters = new List<IShadowCaster>();
    private RenderParameterColor4F shadowColor = new RenderParameterColor4F("PixelColor");
    private const float MinCameraDistanceEnable = 1.6f;
    private const float MinCameraDistanceFullOpacity = 1.3f;
    private RasterizerState rasterizer;
    private DepthStencilState pass1DepthStencil;
    private BlendState pass1Blend;
    private DepthStencilState pass2DepthStencil;
    private BlendState pass2Blend;
    private Effect effect;
    private VertexBuffer screenQuad;
    private Color4F color;

    public Color4F Color
    {
      set
      {
        this.shadowColor.Value = value;
      }
    }

    public ShadowStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher)
      : base(dispatcher, eventDispatcher)
    {
      this.InitializeRenderStates();
      this.InitializeEffect();
      this.BuildVertices();
      this.color = new Color4F(0.2f, 0.0f, 0.0f, 0.0f);
    }

    public void AddShadowCaster(IShadowCaster shadowCaster)
    {
      this.shadowCasters.Add(shadowCaster);
    }

    public void RemoveShadowCaster(IShadowCaster shadowCaster)
    {
      this.shadowCasters.Remove(shadowCaster);
    }

    private void InitializeRenderStates()
    {
      this.rasterizer = RasterizerState.Create(new RasterizerStateDescription()
      {
        AntialiasedLineEnable = false,
        CullMode = CullMode.None,
        FillMode = FillMode.Solid,
        MultisampleEnable = false,
        ScissorEnable = false
      });
      this.pass1DepthStencil = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthEnable = true,
        DepthFunction = ComparisonFunction.Less,
        DepthWriteEnable = false,
        StencilEnable = true,
        StencilFrontFace = new StencilDescription()
        {
          PassOperation = StencilOperation.Increment,
          FailOperation = StencilOperation.Keep,
          DepthFailOperation = StencilOperation.Keep,
          Function = ComparisonFunction.Always
        },
        StencilBackFace = new StencilDescription()
        {
          PassOperation = StencilOperation.Decrement,
          FailOperation = StencilOperation.Keep,
          DepthFailOperation = StencilOperation.Keep,
          Function = ComparisonFunction.Always
        }
      });
      this.pass1Blend = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = false,
        WriteMask = RenderTargetWriteMask.None
      });
      this.pass2DepthStencil = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthEnable = false,
        StencilEnable = true,
        StencilFrontFace = new StencilDescription()
        {
          Function = ComparisonFunction.NotEqual
        },
        StencilBackFace = new StencilDescription()
        {
          Function = ComparisonFunction.NotEqual
        },
        StencilReferenceValue = 0
      });
      this.pass2Blend = BlendState.Create(new BlendStateDescription()
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
    }

    private void InitializeEffect()
    {
      this.effect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.SingleColor.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.SingleColor.ps"),
        GeometryShaderData = (Stream) null,
        VertexFormat = VertexFormats.Position2D,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[1]
        {
          (IRenderParameter) this.shadowColor
        })
      });
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

    internal override bool PreExecute(SceneState state, int phase)
    {
      return false;
    }

    internal override void Execute(Renderer renderer, SceneState state, int phase)
    {
      if (state.IsSkipLayerRelatedSteps)
        return;
      try
      {
        float num = (float) (state.CameraSnapshot.Distance + 1.0);
        if ((double) num > 1.60000002384186 || this.shadowCasters.Count == 0)
          return;
        renderer.Profiler.BeginSection("[Shadows]");
        renderer.Profiler.BeginSection("[Shadows] Shadow volume pass");
        renderer.ClearCurrentStencilTarget(0);
        this.shadowColor.Value = new Color4F((1f - Math.Max(0.0f, (float) (((double) num - 1.29999995231628) / 0.300000071525574))) * this.color.A, this.color.R, this.color.G, this.color.B);
        bool flag = false;
        for (int index = 0; index < this.shadowCasters.Count; ++index)
          flag = flag | this.shadowCasters[index].DrawShadowVolume(renderer, state, this.pass1DepthStencil, this.pass1Blend, this.rasterizer);
        renderer.Profiler.EndSection();
        if (flag)
        {
          renderer.Profiler.BeginSection("[Shadows] Full-screen quad pass");
          renderer.SetDepthStencilState(this.pass2DepthStencil);
          renderer.SetBlendState(this.pass2Blend);
          renderer.SetRasterizerState(this.rasterizer);
          renderer.SetEffect(this.effect);
          renderer.SetVertexSource(this.screenQuad);
          renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
          renderer.Profiler.EndSection();
        }
        renderer.Profiler.EndSection();
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.Fail("Exception while executing Shadow Step.", ex);
      }
    }

    public override void Dispose()
    {
      DisposableResource[] disposableResourceArray = new DisposableResource[7]
      {
        (DisposableResource) this.rasterizer,
        (DisposableResource) this.pass1DepthStencil,
        (DisposableResource) this.pass1Blend,
        (DisposableResource) this.pass2DepthStencil,
        (DisposableResource) this.pass2Blend,
        (DisposableResource) this.effect,
        (DisposableResource) this.screenQuad
      };
      foreach (DisposableResource disposableResource in disposableResourceArray)
      {
        if (disposableResource != null)
          disposableResource.Dispose();
      }
    }
  }
}
