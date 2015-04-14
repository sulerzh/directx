using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal class InstanceProcessingTechnique : EffectTechnique
    {
        private RenderParameterFloat comparisonPositiveValueParameter = new RenderParameterFloat("ComparisonPositiveValue");
        private RenderParameterFloat comparisonNegativeValueParameter = new RenderParameterFloat("ComparisonNegativeValue");
        private RenderParameterBool useGatherAccumulateParameter = new RenderParameterBool("UseGatherAccumulate");
        private RenderParameterBool useNegativesParameter = new RenderParameterBool("UseNegatives");
        private RenderParameterBool negativeValuesParameter = new RenderParameterBool("NegativeValues");
        private RenderParameterInt maxShiftParameter = new RenderParameterInt("MaxShift");
        private RenderParameterFloat frameIdParameter = new RenderParameterFloat("FrameId");
        private RenderParameterBool useSqrtValueParameter = new RenderParameterBool("UseSqrtValue");
        private RenderParameterBool ignoreValuesParameter = new RenderParameterBool("IgnoreValues");
        private RenderParameterInt shiftOffsetParameter = new RenderParameterInt("ShiftOffset");
        private RenderParameterInt colorCountParameter = new RenderParameterInt("ColorCount");
        private RenderParameterBool annotationEnabledParameter = new RenderParameterBool("AnnotationEnabled");
        private RenderParameterBool showOnlyMaxValueEnabledParameter = new RenderParameterBool("ShowOnlyMaxShiftEnabled");
        private RenderParameterFloat valueOffsetParameter = new RenderParameterFloat("ValueOffsetParameter");
        private RenderParameterBool useLogScaleParameter = new RenderParameterBool("UseLogScale");
        private VertexFormat vertexFormat;
        private VertexFormat outputVertexFormat;
        private VertexFormat outputVertexFormatPie;
        private VertexFormat outputVertexFormatHitTest;
        private RenderParameters sharedParameters;
        private Effect effect;
        private Effect effectPie;
        private Effect effectComp;
        private Effect effectHitTest;
        private Effect effectCompHitTest;

        public bool UseNegatives
        {
            get
            {
                return this.useNegativesParameter.Value;
            }
            set
            {
                this.useNegativesParameter.Value = value;
            }
        }

        public bool NegativeValues
        {
            get
            {
                return this.negativeValuesParameter.Value;
            }
            set
            {
                this.negativeValuesParameter.Value = value;
            }
        }

        public int MaxShift
        {
            get
            {
                return this.maxShiftParameter.Value;
            }
            set
            {
                this.maxShiftParameter.Value = value;
            }
        }

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

        public bool UseSqrtValue
        {
            get
            {
                return this.useSqrtValueParameter.Value;
            }
            set
            {
                this.useSqrtValueParameter.Value = value;
            }
        }

        public bool UseGatherAccumulate
        {
            get
            {
                return this.useGatherAccumulateParameter.Value;
            }
            set
            {
                this.useGatherAccumulateParameter.Value = value;
            }
        }

        public bool IgnoreValues
        {
            get
            {
                return this.ignoreValuesParameter.Value;
            }
            set
            {
                this.ignoreValuesParameter.Value = value;
            }
        }

        public int ShiftOffset
        {
            get
            {
                return this.shiftOffsetParameter.Value;
            }
            set
            {
                this.shiftOffsetParameter.Value = value;
            }
        }

        public int ColorCount
        {
            get
            {
                return this.colorCountParameter.Value;
            }
            set
            {
                this.colorCountParameter.Value = value;
            }
        }

        public bool AnnotationPlacementEnabled
        {
            get
            {
                return this.annotationEnabledParameter.Value;
            }
            set
            {
                this.annotationEnabledParameter.Value = value;
            }
        }

        public bool ShowOnlyMaxValueEnabled
        {
            get
            {
                return this.showOnlyMaxValueEnabledParameter.Value;
            }
            set
            {
                this.showOnlyMaxValueEnabledParameter.Value = value;
            }
        }

        public bool UseLogarithmicScale
        {
            get
            {
                return this.useLogScaleParameter.Value;
            }
            set
            {
                this.useLogScaleParameter.Value = value;
            }
        }

        public float ValueOffset
        {
            get
            {
                return this.valueOffsetParameter.Value;
            }
            set
            {
                this.valueOffsetParameter.Value = value;
            }
        }

        public InstanceProcessingTechniqueMode Mode { get; set; }

        public VertexFormat OutputVertexFormat
        {
            get
            {
                if (this.Mode != InstanceProcessingTechniqueMode.Pie)
                    return this.outputVertexFormat;
                return this.outputVertexFormatPie;
            }
        }

        public InstanceProcessingTechnique(RenderParameters sharedParams)
        {
            this.sharedParameters = sharedParams;
            this.outputVertexFormat = VertexFormat.Create(new VertexComponent[4]
            {
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float)
            });
            this.outputVertexFormatPie = VertexFormat.Create(new VertexComponent[5]
            {
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float)
            });
            this.outputVertexFormatHitTest = VertexFormat.Create(new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4));
        }

        protected override void Initialize()
        {
            VertexComponent[] components = InstanceVertexFormat.Components;
            this.vertexFormat = new InstanceBlockVertex().Format;
            this.effect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceProcessing.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceProcessing.gs"),
                VertexFormat = this.vertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[]
                {
                    this.useGatherAccumulateParameter,
                    this.frameIdParameter,
                    this.useSqrtValueParameter,
                    this.maxShiftParameter,
                    this.useNegativesParameter,
                    this.negativeValuesParameter,
                    this.ignoreValuesParameter,
                    this.shiftOffsetParameter,
                    this.colorCountParameter,
                    this.annotationEnabledParameter,
                    this.showOnlyMaxValueEnabledParameter,
                    this.valueOffsetParameter,
                    this.useLogScaleParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                },
                StreamFormat = this.outputVertexFormat
            });
            this.effectPie = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceProcessingPie.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceProcessingPie.gs"),
                VertexFormat = this.vertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[13]
                {
                    this.useGatherAccumulateParameter,
                    this.frameIdParameter,
                    this.useSqrtValueParameter,
                    this.maxShiftParameter,
                    this.useNegativesParameter,
                    this.negativeValuesParameter,
                    this.ignoreValuesParameter,
                    this.shiftOffsetParameter,
                    this.colorCountParameter,
                    this.annotationEnabledParameter,
                    this.showOnlyMaxValueEnabledParameter,
                    this.valueOffsetParameter,
                    this.useLogScaleParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                },
                StreamFormat = this.outputVertexFormatPie
            });
            this.effectComp = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceCompProcessing.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceCompProcessing.gs"),
                VertexFormat = this.vertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[11]
                {
                    this.comparisonPositiveValueParameter,
                    this.comparisonNegativeValueParameter,
                    this.useNegativesParameter,
                    this.maxShiftParameter,
                    this.frameIdParameter,
                    this.useGatherAccumulateParameter,
                    this.shiftOffsetParameter,
                    this.colorCountParameter,
                    this.annotationEnabledParameter,
                    this.valueOffsetParameter,
                    this.showOnlyMaxValueEnabledParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                },
                StreamFormat = this.outputVertexFormat
            });
            this.effectHitTest = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceProcessingHitTest.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceProcessingHitTest.gs"),
                VertexFormat = this.vertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[13]
                {
                    this.useGatherAccumulateParameter,
                    this.frameIdParameter,
                    this.useSqrtValueParameter,
                    this.maxShiftParameter,
                    this.useNegativesParameter,
                    this.negativeValuesParameter,
                    this.ignoreValuesParameter,
                    this.shiftOffsetParameter,
                    this.colorCountParameter,
                    this.annotationEnabledParameter,
                    this.showOnlyMaxValueEnabledParameter,
                    this.valueOffsetParameter,
                    this.useLogScaleParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                },
                StreamFormat = this.outputVertexFormatHitTest
            });
            this.effectCompHitTest = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceCompProcessingHitTest.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceCompProcessingHitTest.gs"),
                VertexFormat = this.vertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[11]
                {
                    this.comparisonPositiveValueParameter,
                    this.comparisonNegativeValueParameter,
                    this.useNegativesParameter,
                    this.maxShiftParameter,
                    this.frameIdParameter,
                    this.useGatherAccumulateParameter,
                    this.shiftOffsetParameter,
                    this.colorCountParameter,
                    this.annotationEnabledParameter,
                    this.valueOffsetParameter,
                    this.showOnlyMaxValueEnabledParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                },
                StreamFormat = this.outputVertexFormatHitTest
            });
        }

        protected override void Update()
        {
            switch (this.Mode)
            {
                case InstanceProcessingTechniqueMode.Zero:
                    this.comparisonPositiveValueParameter.Value = 0.0f;
                    this.comparisonNegativeValueParameter.Value = -1f;
                    this.Effect = this.effectComp;
                    break;
                case InstanceProcessingTechniqueMode.Null:
                    this.comparisonPositiveValueParameter.Value = -2f;
                    this.comparisonNegativeValueParameter.Value = -1f;
                    this.Effect = this.effectComp;
                    break;
                case InstanceProcessingTechniqueMode.Pie:
                    this.Effect = this.effectPie;
                    break;
                case InstanceProcessingTechniqueMode.HitTest:
                    this.Effect = this.effectHitTest;
                    break;
                case InstanceProcessingTechniqueMode.ZeroHitTest:
                    this.comparisonPositiveValueParameter.Value = 0.0f;
                    this.comparisonNegativeValueParameter.Value = -1f;
                    this.Effect = this.effectCompHitTest;
                    break;
                case InstanceProcessingTechniqueMode.NullHitTest:
                    this.comparisonPositiveValueParameter.Value = -2f;
                    this.comparisonNegativeValueParameter.Value = -1f;
                    this.Effect = this.effectCompHitTest;
                    break;
                default:
                    this.Effect = this.effect;
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            DisposableResource[] disposableResourceArray = new DisposableResource[5]
            {
                this.effect,
                this.effectPie,
                this.effectComp,
                this.effectHitTest,
                this.effectCompHitTest
            };
            foreach (DisposableResource res in disposableResourceArray)
            {
                if (res != null)
                    res.Dispose();
            }
        }
    }
}
