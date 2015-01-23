// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HeatMapTechnique
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class HeatMapTechnique : EffectTechnique
  {
    private RenderParameterFloat circleOfInfluenceParameter = new RenderParameterFloat("CircleOfInfluence");
    private RenderParameterBool variableCircleOfInfluenceParameter = new RenderParameterBool("VariableCircleOfInfluence");
    private RenderParameterFloat fadeTimeParameter = new RenderParameterFloat("FadeTime");
    private RenderParameterFloat visualTimeParameter = new RenderParameterFloat("VisualTime");
    private RenderParameterFloat visualTimeScaleParameter = new RenderParameterFloat("VisualTimeScale");
    private RenderParameterFloat visualTimeFreezeParameter = new RenderParameterFloat("VisualTimeFreeze");
    private RenderParameterBool visualTimeFreezeEnabledParameter = new RenderParameterBool("VisualTimeFreezeEnabled");
    private RenderParameterFloat maxHeatValueParameter = new RenderParameterFloat("MaxHeatValue");
    private RenderParameterFloat minHeatValueParameter = new RenderParameterFloat("MinHeatValue");
    private RenderParameterFloat heatMapAlphaParameter = new RenderParameterFloat("HeatMapAlpha");
    private RenderParameterFloat maxValueForAlphaParameter = new RenderParameterFloat("MaxValueForAlpha");
    private Effect step0Effect;
    private Effect step0TimeEffect;
    private Effect step1EffectHorizontal;
    private Effect step1EffectVertical;
    private Effect step2Effect;
    private BlendState step0Blend;
    private DepthStencilState step0Depth;
    private RasterizerState step0Rasterizer;
    private HeatMapBlendMode blendMode;
    private BlendState step1Blend;
    private DepthStencilState step1Depth;
    private RasterizerState step1Rasterizer;
    private BlendState step2Blend;
    private DepthStencilState step2Depth;
    private RasterizerState step2Rasterizer;

    public bool TimeEnabled { get; set; }

    public HeatMapTechniqueStep RenderStep { get; set; }

    public HeatMapBlendMode BlendMode
    {
      get
      {
        return this.blendMode;
      }
      set
      {
        if (this.blendMode == value)
          return;
        this.blendMode = value;
        this.CreateStep0BlendState();
      }
    }

    public float CircleOfInfluence
    {
      get
      {
        return this.circleOfInfluenceParameter.Value;
      }
      set
      {
        this.circleOfInfluenceParameter.Value = value;
      }
    }

    public bool IsVariableCircleOfInfluence
    {
      get
      {
        return this.variableCircleOfInfluenceParameter.Value;
      }
      set
      {
        this.variableCircleOfInfluenceParameter.Value = value;
      }
    }

    public float MinHeatValue
    {
      get
      {
        return this.minHeatValueParameter.Value;
      }
      set
      {
        this.minHeatValueParameter.Value = value;
      }
    }

    public float MaxHeatValue
    {
      get
      {
        return this.maxHeatValueParameter.Value;
      }
      set
      {
        this.maxHeatValueParameter.Value = value;
      }
    }

    public float Alpha
    {
      get
      {
        return this.heatMapAlphaParameter.Value;
      }
      set
      {
        this.heatMapAlphaParameter.Value = value;
      }
    }

    public float MaxValueForAlpha
    {
      get
      {
        return this.maxValueForAlphaParameter.Value;
      }
      set
      {
        this.maxValueForAlphaParameter.Value = value;
      }
    }

    public float FadeTime
    {
      get
      {
        return this.fadeTimeParameter.Value;
      }
      set
      {
        this.fadeTimeParameter.Value = value;
      }
    }

    public float VisualTime
    {
      get
      {
        return this.visualTimeParameter.Value;
      }
      set
      {
        this.visualTimeParameter.Value = value;
      }
    }

    public float VisualTimeScale
    {
      get
      {
        return this.visualTimeScaleParameter.Value;
      }
      set
      {
        this.visualTimeScaleParameter.Value = value;
      }
    }

    public float VisualTimeFreeze
    {
      get
      {
        return this.visualTimeFreezeParameter.Value;
      }
      set
      {
        this.visualTimeFreezeParameter.Value = value;
      }
    }

    public bool VisualTimeFreezeEnabled
    {
      get
      {
        return this.visualTimeFreezeEnabledParameter.Value;
      }
      set
      {
        this.visualTimeFreezeEnabledParameter.Value = value;
      }
    }

    protected override void Initialize()
    {
      this.step0Depth = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthEnable = false,
        StencilEnable = false
      });
      this.step0Rasterizer = RasterizerState.Create(new RasterizerStateDescription()
      {
        CullMode = CullMode.Front
      });
      this.blendMode = HeatMapBlendMode.Add;
      this.CreateStep0BlendState();
      this.step1Depth = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthEnable = false,
        StencilEnable = false
      });
      this.step1Rasterizer = RasterizerState.Create(new RasterizerStateDescription()
      {
        CullMode = CullMode.None
      });
      this.step1Blend = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = false
      });
      this.step2Depth = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthEnable = false,
        StencilEnable = false
      });
      this.step2Rasterizer = RasterizerState.Create(new RasterizerStateDescription()
      {
        CullMode = CullMode.None
      });
      this.step2Blend = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = true,
        BlendOp = BlendOperation.Add,
        SourceBlend = BlendFactor.SourceAlpha,
        DestBlend = BlendFactor.InvSourceAlpha,
        BlendOpAlpha = BlendOperation.Add,
        SourceBlendAlpha = BlendFactor.SourceAlpha,
        DestBlendAlpha = BlendFactor.One
      });
      TextureSampler textureSampler1 = TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(0.0f, 0.0f, 0.0f, 0.0f));
      TextureSampler textureSampler2 = TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(0.0f, 0.0f, 0.0f, 0.0f));
      RenderParameters renderParameters1 = RenderParameters.Create(new IRenderParameter[9]
      {
        (IRenderParameter) this.circleOfInfluenceParameter,
        (IRenderParameter) this.minHeatValueParameter,
        (IRenderParameter) this.maxHeatValueParameter,
        (IRenderParameter) this.variableCircleOfInfluenceParameter,
        (IRenderParameter) this.fadeTimeParameter,
        (IRenderParameter) this.visualTimeParameter,
        (IRenderParameter) this.visualTimeScaleParameter,
        (IRenderParameter) this.visualTimeFreezeParameter,
        (IRenderParameter) this.visualTimeFreezeEnabledParameter
      });
      RenderParameters renderParameters2 = RenderParameters.Create(new IRenderParameter[2]
      {
        (IRenderParameter) this.heatMapAlphaParameter,
        (IRenderParameter) this.maxValueForAlphaParameter
      });
      this.step0Effect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep0.vs"),
        GeometryShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep0.gs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep0.ps"),
        VertexFormat = VertexFormat.Create(HeatMapVertexFormat.Components[0], HeatMapVertexFormat.Components[1]),
        Parameters = renderParameters1,
        Samplers = (TextureSampler[]) null
      });
      this.step0TimeEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep0Time.vs"),
        GeometryShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep0.gs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep0.ps"),
        VertexFormat = VertexFormat.Create(HeatMapVertexFormat.Components),
        Parameters = renderParameters1,
        Samplers = (TextureSampler[]) null
      });
      this.step1EffectHorizontal = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep1.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep1Horizontal.ps"),
        VertexFormat = VertexFormats.Position2D,
        Parameters = (RenderParameters) null,
        Samplers = new TextureSampler[1]
        {
          textureSampler1
        }
      });
      this.step1EffectVertical = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep1.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep1Vertical.ps"),
        VertexFormat = VertexFormats.Position2D,
        Parameters = (RenderParameters) null,
        Samplers = new TextureSampler[1]
        {
          textureSampler1
        }
      });
      this.step2Effect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep2.vs"),
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.HeatMapStep2.ps"),
        VertexFormat = VertexFormats.Position2D,
        Parameters = renderParameters2,
        Samplers = new TextureSampler[2]
        {
          textureSampler1,
          textureSampler2
        }
      });
    }

    private void CreateStep0BlendState()
    {
      if (this.step0Blend != null)
        this.step0Blend.Dispose();
      this.step0Blend = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = true,
        BlendOp = this.blendMode == HeatMapBlendMode.Add ? BlendOperation.Add : BlendOperation.Max,
        SourceBlend = BlendFactor.One,
        DestBlend = BlendFactor.One,
        WriteMask = RenderTargetWriteMask.Color
      });
    }

    protected override void Update()
    {
      switch (this.RenderStep)
      {
        case HeatMapTechniqueStep.Step0:
          this.Effect = this.TimeEnabled ? this.step0TimeEffect : this.step0Effect;
          this.Blend = this.step0Blend;
          this.DepthStencil = this.step0Depth;
          this.Rasterizer = this.step0Rasterizer;
          break;
        case HeatMapTechniqueStep.Step1Part1:
          this.Effect = this.step1EffectHorizontal;
          this.Blend = this.step1Blend;
          this.DepthStencil = this.step1Depth;
          this.Rasterizer = this.step1Rasterizer;
          break;
        case HeatMapTechniqueStep.Step1Part2:
          this.Effect = this.step1EffectVertical;
          this.Blend = this.step1Blend;
          this.DepthStencil = this.step1Depth;
          this.Rasterizer = this.step1Rasterizer;
          break;
        case HeatMapTechniqueStep.Step2:
          this.Effect = this.step2Effect;
          this.Blend = this.step2Blend;
          this.DepthStencil = this.step2Depth;
          this.Rasterizer = this.step2Rasterizer;
          break;
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      DisposableResource[] disposableResourceArray = new DisposableResource[14]
      {
        (DisposableResource) this.step0Effect,
        (DisposableResource) this.step0TimeEffect,
        (DisposableResource) this.step1EffectHorizontal,
        (DisposableResource) this.step1EffectVertical,
        (DisposableResource) this.step2Effect,
        (DisposableResource) this.step0Depth,
        (DisposableResource) this.step0Blend,
        (DisposableResource) this.step0Rasterizer,
        (DisposableResource) this.step1Depth,
        (DisposableResource) this.step1Blend,
        (DisposableResource) this.step1Rasterizer,
        (DisposableResource) this.step2Depth,
        (DisposableResource) this.step2Blend,
        (DisposableResource) this.step2Rasterizer
      };
      foreach (DisposableResource disposableResource in disposableResourceArray)
      {
        if (disposableResource != null)
          disposableResource.Dispose();
      }
    }
  }
}
