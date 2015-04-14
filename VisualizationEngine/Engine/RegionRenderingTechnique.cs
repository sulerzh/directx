using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;

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

        public float MinValue
        {
            get
            {
                return minValueParameter.Value;
            }
            set
            {
                minValueParameter.Value = value;
            }
        }

        public float ValueScale
        {
            get
            {
                return valueScaleParameter.Value;
            }
            set
            {
                valueScaleParameter.Value = value;
            }
        }

        public bool ColorShadingEnabled
        {
            get
            {
                return colorShadingEnabledParameter.Value;
            }
            set
            {
                colorShadingEnabledParameter.Value = value;
            }
        }

        public bool BrightnessModeEnabled
        {
            get
            {
                return brightnessModeEnabledParameter.Value;
            }
            set
            {
                brightnessModeEnabledParameter.Value = value;
            }
        }

        public Color4F OutlineColor
        {
            get
            {
                return outlineColorParameter.Value;
            }
            set
            {
                outlineColorParameter.Value = value;
            }
        }

        public bool LocalShadingEnabled { get; set; }

        public bool ShiftGlobalShadingEnabled { get; set; }

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
                if (renderEffectPie != null)
                    renderEffectPie.SharedEffectParameters = new RenderParameters[2]
                    {
                        sharedParameters,
                        colorParameters
                    };
                if (renderEffectPerShift == null)
                    return;
                renderEffectPerShift.SharedEffectParameters = new RenderParameters[2]
                {
                    sharedParameters,
                    colorParameters
                };
            }
        }

        public RegionRenderingTechnique(RenderParameters sharedParams, RenderParameters colorParams)
        {
            sharedParameters = sharedParams;
            colorParameters = colorParams;
            for (int i = 0; i < MaxShiftCount; ++i)
            {
                minValueAndScalePerShiftParameter[i] = new RenderParameterVector4F("MinValueAndScalePerShift");
            }
        }

        public void SetMinValueAndScalePerShift(double[] values, double[] scales, double valueOffset)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                minValueAndScalePerShiftParameter[i].Value = new Vector4F((float) (values[i] + valueOffset),
                    (float) scales[i], 0.0f, 0.0f);
            }
        }

        public void SetRenderStates(DepthStencilState depthStencilState, BlendState blendState, RasterizerState rasterizerState)
        {
            DepthStencil = depthStencilState;
            Blend = blendState;
            Rasterizer = rasterizerState;
        }

        protected override void Initialize()
        {
            IList<VertexComponent> components = new RegionVertex().Format.Components;
            renderVertexFormat = VertexFormat.Create(new VertexComponent[5]
            {
                components[0],
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerInstanceData, 1),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float,
                    VertexComponentClassification.PerInstanceData, 1)
            });
            renderVertexFormatPie = VertexFormat.Create(new VertexComponent[6]
            {
                components[0],
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
            hitTestVertexFormat = VertexFormat.Create(new VertexComponent[6]
            {
                components[0],
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
            renderEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRendering.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRendering.ps"),
                VertexFormat = renderVertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[6]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    minValueParameter,
                    valueScaleParameter,
                    colorShadingEnabledParameter,
                    brightnessModeEnabledParameter
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
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingPie.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRendering.ps"),
                VertexFormat = renderVertexFormatPie,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[6]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    minValueParameter,
                    valueScaleParameter,
                    colorShadingEnabledParameter,
                    brightnessModeEnabledParameter
                }),
                SharedParameters = new RenderParameters[2]
                {
                    sharedParameters,
                    colorParameters
                }
            });
            IRenderParameter[] parameters = new IRenderParameter[2054];
            for (int i = 0; i < MaxShiftCount; ++i)
                parameters[i] = minValueAndScalePerShiftParameter[i];
            parameters[2048] = frameIdParameter;
            parameters[2049] = desaturateFactorParameter;
            parameters[2050] = minValueParameter;
            parameters[2051] = valueScaleParameter;
            parameters[2052] = colorShadingEnabledParameter;
            parameters[2053] = brightnessModeEnabledParameter;
            renderEffectPerShift = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingPerShiftScale.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingPerShiftScale.ps"),
                VertexFormat = renderVertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(parameters),
                SharedParameters = new RenderParameters[]
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
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingHitTest.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingHitTest.ps"),
                VertexFormat = hitTestVertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[]
                {
                    frameIdParameter,
                    desaturateFactorParameter,
                    minValueParameter,
                    valueScaleParameter,
                    colorShadingEnabledParameter,
                    brightnessModeEnabledParameter
                }),
                SharedParameters = new RenderParameters[]
                {
                    sharedParameters
                }
            });
            outlineEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingOutline.vs"),
                GeometryShaderData = null,
                PixelShaderData =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.RegionRenderingOutline.ps"),
                VertexFormat = renderVertexFormat,
                Samplers = new TextureSampler[1]
                {
                    TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp,
                        new Color4F(0.0f, 0.0f, 0.0f, 0.0f), ComparisonFunction.Less)
                },
                Parameters = RenderParameters.Create(new IRenderParameter[]
                {
                    outlineColorParameter,
                    frameIdParameter,
                    desaturateFactorParameter,
                    minValueParameter,
                    valueScaleParameter,
                    colorShadingEnabledParameter,
                    brightnessModeEnabledParameter
                }),
                SharedParameters = new RenderParameters[]
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
                    Effect = ShiftGlobalShadingEnabled ? renderEffectPerShift : (LocalShadingEnabled ? renderEffectPie : renderEffect);
                    break;
                case RenderMode.HitTest:
                    Effect = hitTestEffect;
                    break;
                case RenderMode.Outline:
                    Effect = outlineEffect;
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
                renderEffect,
                renderEffectPerShift,
                hitTestEffect,
                renderEffectPie,
                outlineEffect
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
            Outline,
        }
    }
}
