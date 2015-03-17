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

        public RenderMode Mode { get; set; }

        public RenderParameters ColorParameters
        {
            get
            {
                return colorParameters;
            }
            set
            {
                colorParameters = value;
                if (renderEffect != null)
                    renderEffect.SharedEffectParameters = new RenderParameters[2]
                    {
                        sharedParameters,
                        colorParameters
                    };
                if (renderEffectColorOnly != null)
                    renderEffectColorOnly.SharedEffectParameters = new RenderParameters[2]
                    {
                        sharedParameters,
                        colorParameters
                    };
                if (renderEffectPie != null)
                    renderEffectPie.SharedEffectParameters = new RenderParameters[2]
                    {
                        sharedParameters,
                        colorParameters
                    };
                if (renderEffectPieColorOnly == null)
                    return;
                renderEffectPieColorOnly.SharedEffectParameters = new RenderParameters[2]
                {
                    sharedParameters,
                    colorParameters
                };
            }
        }

        public bool RenderDepthWidthInfo { get; set; }

        public float FrameId
        {
            get
            {
                return frameIdParameter.Value;
            }
            set
            {
                frameIdParameter.Value = value;
            }
        }

        public float DesaturateFactor
        {
            get
            {
                return desaturateFactorParameter.Value;
            }
            set
            {
                desaturateFactorParameter.Value = value;
            }
        }

        public bool UseRenderPriority
        {
            get
            {
                return useRenderPriorityParameter.Value;
            }
            set
            {
                useRenderPriorityParameter.Value = value;
            }
        }

        public int OverrideRenderPriority
        {
            get
            {
                return overrideRenderPriorityParameter.Value;
            }
            set
            {
                overrideRenderPriorityParameter.Value = value;
            }
        }

        public bool PieTechnique { get; set; }

        static InstanceRenderingTechnique()
        {
            VertexComponent[] components = InstanceVertexFormat.Components;
            renderVertexFormat = VertexFormat.Create(new VertexComponent[6]
            {
                components[0],
                components[1],
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float,
                    VertexComponentClassification.PerInstanceData, 1)
            });
            renderVertexFormatPie = VertexFormat.Create(new VertexComponent[7]
            {
                components[0],
                components[1],
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float,
                    VertexComponentClassification.PerInstanceData, 1)
            });
            hitTestVertexFormat = VertexFormat.Create(new VertexComponent[7]
            {
                components[0],
                components[1],
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4,
                    VertexComponentClassification.PerInstanceData, 2)
            });
            hitTestVertexFormatPie = VertexFormat.Create(new VertexComponent[8]
            {
                components[0],
                components[1],
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4,
                    VertexComponentClassification.PerInstanceData, 2)
            });
        }

        public InstanceRenderingTechnique(RenderParameters sharedParams, RenderParameters colorParams)
        {
            sharedParameters = sharedParams;
            colorParameters = colorParams;
        }

        public void SetRenderStates(DepthStencilState depthStencilState, BlendState blendState, RasterizerState rasterizerState)
        {
            DepthStencil = depthStencilState;
            Blend = blendState;
            Rasterizer = rasterizerState;
        }

        protected override void Initialize()
        {
            renderEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderable.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderable.ps"),
                VertexFormat = renderVertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[4]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    useRenderPriorityParameter,
                    overrideRenderPriorityParameter
                }),
                SharedParameters = new RenderParameters[2]
                {
                    sharedParameters,
                    colorParameters
                }
            });
            renderEffectColorOnly = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderable.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableColorOnly.ps"),
                VertexFormat = renderVertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[4]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    useRenderPriorityParameter,
                    overrideRenderPriorityParameter
                }),
                SharedParameters = new RenderParameters[2]
                {
                    sharedParameters,
                    colorParameters
                }
            });
            renderEffectPie = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderablePie.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderable.ps"),
                VertexFormat = renderVertexFormatPie,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[4]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    useRenderPriorityParameter,
                    overrideRenderPriorityParameter
                }),
                SharedParameters = new RenderParameters[2]
                {
                    sharedParameters,
                    colorParameters
                }
            });
            renderEffectPieColorOnly = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderablePie.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableColorOnly.ps"),
                VertexFormat = renderVertexFormatPie,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[4]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    useRenderPriorityParameter,
                    overrideRenderPriorityParameter
                }),
                SharedParameters = new RenderParameters[2]
                {
                    sharedParameters,
                    colorParameters
                }
            });
            hitTestEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableHitTest.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableHitTest.ps"),
                VertexFormat = hitTestVertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[4]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    useRenderPriorityParameter,
                    overrideRenderPriorityParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    sharedParameters
                }
            });
            hitTestEffectPie = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableHitTestPie.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableHitTest.ps"),
                VertexFormat = hitTestVertexFormatPie,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[4]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    useRenderPriorityParameter,
                    overrideRenderPriorityParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    sharedParameters
                }
            });
            Effect = renderEffect;
        }

        protected override void Update()
        {
            switch (Mode)
            {
                case RenderMode.Color:
                    if (RenderDepthWidthInfo)
                    {
                        Effect = PieTechnique ? renderEffectPie : renderEffect;
                        break;
                    }
                    Effect = PieTechnique ? renderEffectPieColorOnly : renderEffectColorOnly;
                    break;
                case RenderMode.HitTest:
                    Effect = PieTechnique ? hitTestEffectPie : hitTestEffect;
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
                renderEffect,
                renderEffectPie,
                renderEffectColorOnly,
                renderEffectPieColorOnly,
                hitTestEffect,
                hitTestEffectPie
            };
            foreach (DisposableResource res in disposableResourceArray)
            {
                if (res != null)
                    res.Dispose();
            }
        }

        public enum RenderMode
        {
            Color,
            HitTest,
        }
    }
}
