// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionRenderingTechnique
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Data.Visualization.Engine
{
  internal class RegionRenderingTechnique : EffectTechnique
  {
    private RenderParameterColor4F outlineColorParameter = new RenderParameterColor4F("OutlineColor");
    private RenderParameterFloat frameIdParameter = new RenderParameterFloat("FrameId");
    private RenderParameterFloat desaturateFactorParameter = new RenderParameterFloat("DesaturateFactor");
    private RenderParameterFloat minValueParameter = new RenderParameterFloat("MinValue");
    private RenderParameterFloat valueScaleParameter = new RenderParameterFloat("ValueScale");
    private RenderParameterBool colorShadingEnabledParameter = new RenderParameterBool("ColorShadingEnabled");
    private RenderParameterBool brightnessModeEnabledParameter = new RenderParameterBool("BrightnessModeEnabled");
    private RenderParameterVector4F[] minValueAndScalePerShiftParameter = new RenderParameterVector4F[2048];
    public const int MaxShiftCount = 2048;
    private Effect renderEffect;
    private Effect renderEffectPie;
    private Effect renderEffectPerShift;
    private Effect hitTestEffect;
    private Effect outlineEffect;
    private RenderParameters sharedParameters;
    private RenderParameters colorParameters;
    private VertexFormat renderVertexFormat;
    private VertexFormat renderVertexFormatPie;
    private VertexFormat hitTestVertexFormat;

    public float FrameId
    {
      get
      {
        return this.frameIdParameter.Value;
      }
      set
      {
        this.frameIdParameter.Value = value;
      }
    }

    public float DesaturateFactor
    {
      get
      {
        return this.desaturateFactorParameter.Value;
      }
      set
      {
        this.desaturateFactorParameter.Value = value;
      }
    }

    public float MinValue
    {
      get
      {
        return this.minValueParameter.Value;
      }
      set
      {
        this.minValueParameter.Value = value;
      }
    }

    public float ValueScale
    {
      get
      {
        return this.valueScaleParameter.Value;
      }
      set
      {
        this.valueScaleParameter.Value = value;
      }
    }

    public bool ColorShadingEnabled
    {
      get
      {
        return this.colorShadingEnabledParameter.Value;
      }
      set
      {
        this.colorShadingEnabledParameter.Value = value;
      }
    }

    public bool BrightnessModeEnabled
    {
      get
      {
        return this.brightnessModeEnabledParameter.Value;
      }
      set
      {
        this.brightnessModeEnabledParameter.Value = value;
      }
    }

    public Color4F OutlineColor
    {
      get
      {
        return this.outlineColorParameter.Value;
      }
      set
      {
        this.outlineColorParameter.Value = value;
      }
    }

    public bool LocalShadingEnabled { get; set; }

    public bool ShiftGlobalShadingEnabled { get; set; }

    public RegionRenderingTechnique.RenderMode Mode { get; set; }

    public RenderParameters ColorParameters
    {
      get
      {
        return this.colorParameters;
      }
      set
      {
        this.colorParameters = value;
        if (this.renderEffect != null)
          this.renderEffect.SharedEffectParameters = new RenderParameters[2]
          {
            this.sharedParameters,
            this.colorParameters
          };
        if (this.renderEffectPie != null)
          this.renderEffectPie.SharedEffectParameters = new RenderParameters[2]
          {
            this.sharedParameters,
            this.colorParameters
          };
        if (this.renderEffectPerShift == null)
          return;
        this.renderEffectPerShift.SharedEffectParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        };
      }
    }

    public RegionRenderingTechnique(RenderParameters sharedParams, RenderParameters colorParams)
    {
      this.sharedParameters = sharedParams;
      this.colorParameters = colorParams;
      for (int index = 0; index < 2048; ++index)
        this.minValueAndScalePerShiftParameter[index] = new RenderParameterVector4F("MinValueAndScalePerShift");
    }

    public void SetMinValueAndScalePerShift(double[] values, double[] scales, double valueOffset)
    {
      for (int index = 0; index < values.Length; ++index)
        this.minValueAndScalePerShiftParameter[index].Value = new Vector4F((float) (values[index] + valueOffset), (float) scales[index], 0.0f, 0.0f);
    }

    public void SetRenderStates(DepthStencilState depthStencilState, BlendState blendState, RasterizerState rasterizerState)
    {
      this.DepthStencil = depthStencilState;
      this.Blend = blendState;
      this.Rasterizer = rasterizerState;
    }

    protected override void Initialize()
    {
      IList<VertexComponent> components = new RegionVertex().Format.Components;
      this.renderVertexFormat = VertexFormat.Create(new VertexComponent[5]
      {
        components[0],
        new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float, VertexComponentClassification.PerInstanceData, 1)
      });
      this.renderVertexFormatPie = VertexFormat.Create(new VertexComponent[6]
      {
        components[0],
        new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float, VertexComponentClassification.PerInstanceData, 1)
      });
      this.hitTestVertexFormat = VertexFormat.Create(new VertexComponent[6]
      {
        components[0],
        new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4, VertexComponentClassification.PerInstanceData, 2)
      });
      this.renderEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRendering.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRendering.ps"),
        VertexFormat = this.renderVertexFormat,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[6]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.minValueParameter,
          (IRenderParameter) this.valueScaleParameter,
          (IRenderParameter) this.colorShadingEnabledParameter,
          (IRenderParameter) this.brightnessModeEnabledParameter
        }),
        SharedParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        }
      });
      this.renderEffectPie = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingPie.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRendering.ps"),
        VertexFormat = this.renderVertexFormatPie,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[6]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.minValueParameter,
          (IRenderParameter) this.valueScaleParameter,
          (IRenderParameter) this.colorShadingEnabledParameter,
          (IRenderParameter) this.brightnessModeEnabledParameter
        }),
        SharedParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        }
      });
      IRenderParameter[] parameters = new IRenderParameter[2054];
      for (int index = 0; index < 2048; ++index)
        parameters[index] = (IRenderParameter) this.minValueAndScalePerShiftParameter[index];
      parameters[2048] = (IRenderParameter) this.frameIdParameter;
      parameters[2049] = (IRenderParameter) this.desaturateFactorParameter;
      parameters[2050] = (IRenderParameter) this.minValueParameter;
      parameters[2051] = (IRenderParameter) this.valueScaleParameter;
      parameters[2052] = (IRenderParameter) this.colorShadingEnabledParameter;
      parameters[2053] = (IRenderParameter) this.brightnessModeEnabledParameter;
      this.renderEffectPerShift = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingPerShiftScale.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingPerShiftScale.ps"),
        VertexFormat = this.renderVertexFormat,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(parameters),
        SharedParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        }
      });
      this.hitTestEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingHitTest.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingHitTest.ps"),
        VertexFormat = this.hitTestVertexFormat,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[6]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.minValueParameter,
          (IRenderParameter) this.valueScaleParameter,
          (IRenderParameter) this.colorShadingEnabledParameter,
          (IRenderParameter) this.brightnessModeEnabledParameter
        }),
        SharedParameters = new RenderParameters[1]
        {
          this.sharedParameters
        }
      });
      this.outlineEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingOutline.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingOutline.ps"),
        VertexFormat = this.renderVertexFormat,
        Samplers = new TextureSampler[1]
        {
          TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(0.0f, 0.0f, 0.0f, 0.0f), ComparisonFunction.Less)
        },
        Parameters = RenderParameters.Create(new IRenderParameter[7]
        {
          (IRenderParameter) this.outlineColorParameter,
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.minValueParameter,
          (IRenderParameter) this.valueScaleParameter,
          (IRenderParameter) this.colorShadingEnabledParameter,
          (IRenderParameter) this.brightnessModeEnabledParameter
        }),
        SharedParameters = new RenderParameters[1]
        {
          this.sharedParameters
        }
      });
      this.Effect = this.renderEffect;
    }

    protected override void Update()
    {
      switch (this.Mode)
      {
        case RegionRenderingTechnique.RenderMode.Color:
          this.Effect = this.ShiftGlobalShadingEnabled ? this.renderEffectPerShift : (this.LocalShadingEnabled ? this.renderEffectPie : this.renderEffect);
          break;
        case RegionRenderingTechnique.RenderMode.HitTest:
          this.Effect = this.hitTestEffect;
          break;
        case RegionRenderingTechnique.RenderMode.Outline:
          this.Effect = this.outlineEffect;
          break;
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      DisposableResource[] disposableResourceArray = new DisposableResource[5]
      {
        (DisposableResource) this.renderEffect,
        (DisposableResource) this.renderEffectPerShift,
        (DisposableResource) this.hitTestEffect,
        (DisposableResource) this.renderEffectPie,
        (DisposableResource) this.outlineEffect
      };
      foreach (DisposableResource disposableResource in disposableResourceArray)
      {
        if (disposableResource != null)
          disposableResource.Dispose();
      }
    }

    public enum RenderMode
    {
      Color,
      HitTest,
      Outline,
    }
  }
}
