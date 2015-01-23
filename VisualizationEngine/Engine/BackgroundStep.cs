// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.BackgroundStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System.IO;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal class BackgroundStep : EngineStep
  {
    private RenderParameterColor4F topColorParameter = new RenderParameterColor4F("TopColor");
    private RenderParameterColor4F bottomColorParameter = new RenderParameterColor4F("BottomColor");
    private VertexBuffer screenQuad;
    private Effect effect;
    private DepthStencilState depthState;
    private BlendState blendState;
    private RasterizerState rasterizerState;

    public Color4F TopColor { get; set; }

    public Color4F BottomColor { get; set; }

    public BackgroundStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher)
      : base(dispatcher, eventDispatcher)
    {
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
        DepthFunction = ComparisonFunction.Equal,
        DepthWriteEnable = false,
        StencilEnable = false
      });
    }

    public override void OnInitialized()
    {
      base.OnInitialized();
      this.effect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.BackgroundLinear.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.BackgroundLinear.ps"),
        GeometryShaderData = (Stream) null,
        VertexFormat = VertexFormats.Position2D,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[2]
        {
          (IRenderParameter) this.topColorParameter,
          (IRenderParameter) this.bottomColorParameter
        })
      });
      this.BuildVertices();
      this.InitializeStates();
      this.BottomColor = new Color4F(1f, 0.714f, 0.761f, 0.804f);
      this.TopColor = new Color4F(1f, 0.608f, 0.647f, 0.682f);
    }

    internal override bool PreExecute(SceneState state, int phase)
    {
      return false;
    }

    internal override void Execute(Renderer renderer, SceneState state, int phase)
    {
      int num = state.TranslucentGlobe || !(state.FlatteningFactor > 0.0 & state.FlatteningFactor < 1.0) ? 1 : 0;
      if (phase != num)
        return;
      renderer.Profiler.BeginSection("[Background] Rendering");
      this.topColorParameter.Value = this.TopColor;
      this.bottomColorParameter.Value = this.BottomColor;
      renderer.SetRasterizerState(this.rasterizerState);
      renderer.SetBlendState(this.blendState);
      renderer.SetDepthStencilState(this.depthState);
      renderer.SetEffect(this.effect);
      renderer.SetVertexSource(this.screenQuad);
      renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
      renderer.Profiler.EndSection();
    }

    public override void Dispose()
    {
      DisposableResource[] disposableResourceArray = new DisposableResource[5]
      {
        (DisposableResource) this.rasterizerState,
        (DisposableResource) this.depthState,
        (DisposableResource) this.blendState,
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
