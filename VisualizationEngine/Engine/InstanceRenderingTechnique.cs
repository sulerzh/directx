// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceRenderingTechnique
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System.IO;

namespace Microsoft.Data.Visualization.Engine
{
  internal class InstanceRenderingTechnique : EffectTechnique
  {
    private RenderParameterFloat frameIdParameter = new RenderParameterFloat("FrameId");
    private RenderParameterFloat desaturateFactorParameter = new RenderParameterFloat("DesaturateFactor");
    private RenderParameterBool useRenderPriorityParameter = new RenderParameterBool("UseRenderPriority");
    private RenderParameterInt overrideRenderPriorityParameter = new RenderParameterInt("OverrideRenderPriority");
    private Effect renderEffect;
    private Effect renderEffectPie;
    private Effect renderEffectColorOnly;
    private Effect renderEffectPieColorOnly;
    private Effect hitTestEffect;
    private Effect hitTestEffectPie;
    private RenderParameters sharedParameters;
    private RenderParameters colorParameters;
    private static VertexFormat renderVertexFormat;
    private static VertexFormat renderVertexFormatPie;
    private static VertexFormat hitTestVertexFormat;
    private static VertexFormat hitTestVertexFormatPie;

    public InstanceRenderingTechnique.RenderMode Mode { get; set; }

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
        if (this.renderEffectColorOnly != null)
          this.renderEffectColorOnly.SharedEffectParameters = new RenderParameters[2]
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
        if (this.renderEffectPieColorOnly == null)
          return;
        this.renderEffectPieColorOnly.SharedEffectParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        };
      }
    }

    public bool RenderDepthWidthInfo { get; set; }

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

    public bool UseRenderPriority
    {
      get
      {
        return this.useRenderPriorityParameter.Value;
      }
      set
      {
        this.useRenderPriorityParameter.Value = value;
      }
    }

    public int OverrideRenderPriority
    {
      get
      {
        return this.overrideRenderPriorityParameter.Value;
      }
      set
      {
        this.overrideRenderPriorityParameter.Value = value;
      }
    }

    public bool PieTechnique { get; set; }

    static InstanceRenderingTechnique()
    {
      VertexComponent[] components = InstanceVertexFormat.Components;
      InstanceRenderingTechnique.renderVertexFormat = VertexFormat.Create(new VertexComponent[6]
      {
        components[0],
        components[1],
        new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float, VertexComponentClassification.PerInstanceData, 1)
      });
      InstanceRenderingTechnique.renderVertexFormatPie = VertexFormat.Create(new VertexComponent[7]
      {
        components[0],
        components[1],
        new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float, VertexComponentClassification.PerInstanceData, 1)
      });
      InstanceRenderingTechnique.hitTestVertexFormat = VertexFormat.Create(new VertexComponent[7]
      {
        components[0],
        components[1],
        new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4, VertexComponentClassification.PerInstanceData, 2)
      });
      InstanceRenderingTechnique.hitTestVertexFormatPie = VertexFormat.Create(new VertexComponent[8]
      {
        components[0],
        components[1],
        new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float, VertexComponentClassification.PerInstanceData, 1),
        new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4, VertexComponentClassification.PerInstanceData, 2)
      });
    }

    public InstanceRenderingTechnique(RenderParameters sharedParams, RenderParameters colorParams)
    {
      this.sharedParameters = sharedParams;
      this.colorParameters = colorParams;
    }

    public void SetRenderStates(DepthStencilState depthStencilState, BlendState blendState, RasterizerState rasterizerState)
    {
      this.DepthStencil = depthStencilState;
      this.Blend = blendState;
      this.Rasterizer = rasterizerState;
    }

    protected override void Initialize()
    {
      this.renderEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderable.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderable.ps"),
        VertexFormat = InstanceRenderingTechnique.renderVertexFormat,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[4]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.useRenderPriorityParameter,
          (IRenderParameter) this.overrideRenderPriorityParameter
        }),
        SharedParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        }
      });
      this.renderEffectColorOnly = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderable.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableColorOnly.ps"),
        VertexFormat = InstanceRenderingTechnique.renderVertexFormat,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[4]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.useRenderPriorityParameter,
          (IRenderParameter) this.overrideRenderPriorityParameter
        }),
        SharedParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        }
      });
      this.renderEffectPie = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderablePie.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderable.ps"),
        VertexFormat = InstanceRenderingTechnique.renderVertexFormatPie,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[4]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.useRenderPriorityParameter,
          (IRenderParameter) this.overrideRenderPriorityParameter
        }),
        SharedParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        }
      });
      this.renderEffectPieColorOnly = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderablePie.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableColorOnly.ps"),
        VertexFormat = InstanceRenderingTechnique.renderVertexFormatPie,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[4]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.useRenderPriorityParameter,
          (IRenderParameter) this.overrideRenderPriorityParameter
        }),
        SharedParameters = new RenderParameters[2]
        {
          this.sharedParameters,
          this.colorParameters
        }
      });
      this.hitTestEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableHitTest.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableHitTest.ps"),
        VertexFormat = InstanceRenderingTechnique.hitTestVertexFormat,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[4]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.useRenderPriorityParameter,
          (IRenderParameter) this.overrideRenderPriorityParameter
        }),
        SharedParameters = new RenderParameters[1]
        {
          this.sharedParameters
        }
      });
      this.hitTestEffectPie = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableHitTestPie.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableHitTest.ps"),
        VertexFormat = InstanceRenderingTechnique.hitTestVertexFormatPie,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[4]
        {
          (IRenderParameter) this.frameIdParameter,
          (IRenderParameter) this.desaturateFactorParameter,
          (IRenderParameter) this.useRenderPriorityParameter,
          (IRenderParameter) this.overrideRenderPriorityParameter
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
        case InstanceRenderingTechnique.RenderMode.Color:
          if (this.RenderDepthWidthInfo)
          {
            this.Effect = this.PieTechnique ? this.renderEffectPie : this.renderEffect;
            break;
          }
          else
          {
            this.Effect = this.PieTechnique ? this.renderEffectPieColorOnly : this.renderEffectColorOnly;
            break;
          }
        case InstanceRenderingTechnique.RenderMode.HitTest:
          this.Effect = this.PieTechnique ? this.hitTestEffectPie : this.hitTestEffect;
          break;
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      DisposableResource[] disposableResourceArray = new DisposableResource[6]
      {
        (DisposableResource) this.renderEffect,
        (DisposableResource) this.renderEffectPie,
        (DisposableResource) this.renderEffectColorOnly,
        (DisposableResource) this.renderEffectPieColorOnly,
        (DisposableResource) this.hitTestEffect,
        (DisposableResource) this.hitTestEffectPie
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
    }
  }
}
