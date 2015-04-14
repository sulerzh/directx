using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal class OutlineTechnique : EffectTechnique
    {
        private RenderParameterFloat outlineWidth = new RenderParameterFloat("OutlineWidth");
        private RenderParameterFloat outlineOffset = new RenderParameterFloat("OutlineOffset");
        private RenderParameterColor4F outlineColor = new RenderParameterColor4F("OutlineColor");
        private RenderParameterColor4F outlineColor2 = new RenderParameterColor4F("OutlineColor2");
        private RenderParameterBool depthEnabled = new RenderParameterBool("DepthEnabled");
        private RenderParameterFloat frameIdParameter = new RenderParameterFloat("FrameId");
        private RenderParameters sharedParameters;
        private Effect doubleOutline;
        private Effect doubleOutlinePie;
        private Effect doubleOutlineRegion;
        private RasterizerState rasterizerState;
        private DepthStencilState depthStencilState;
        private VertexFormat vertexFormat;
        private VertexFormat vertexFormatPie;
        private VertexFormat vertexFormatRegion;

        public float OutlineWidth
        {
            set
            {
                this.outlineWidth.Value = value;
            }
        }

        public float OutlineOffset
        {
            set
            {
                this.outlineOffset.Value = value;
            }
        }

        public Color4F OutlineColor
        {
            set
            {
                this.outlineColor.Value = value;
            }
        }

        public Color4F OutlineSecondaryColor
        {
            set
            {
                this.outlineColor2.Value = value;
            }
        }

        public bool DepthEnabled
        {
            set
            {
                this.depthEnabled.Value = value;
            }
        }

        public bool PieTechnique { get; set; }

        public bool RegionsMode { get; set; }

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

        public OutlineTechnique(RenderParameters sharedParams)
        {
            this.sharedParameters = sharedParams;
        }

        protected override void Initialize()
        {
            VertexComponent[] components = InstanceVertexFormat.Components;
            this.vertexFormat = VertexFormat.Create(new VertexComponent[6]
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
            this.vertexFormatPie = VertexFormat.Create(new VertexComponent[7]
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
            this.vertexFormatRegion = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerVertexData, 0));
            TextureSampler textureSampler = TextureSampler.Create(TextureFilter.Point, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(1f, 1f, 0.0f, 0.0f), ComparisonFunction.LessEqual);
            this.doubleOutline = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutline.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutlineDoubleLine.gs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutlineDoubleLine.ps"),
                VertexFormat = this.vertexFormat,
                Samplers = new TextureSampler[1]
                {
                    textureSampler
                },
                Parameters = RenderParameters.Create(new IRenderParameter[6]
                {
                    this.outlineColor,
                    this.outlineColor2,
                    this.outlineWidth,
                    this.outlineOffset,
                    this.depthEnabled,
                    this.frameIdParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
            this.doubleOutlineRegion = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutlineRegion.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutlineDoubleLineRegion.gs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutlineDoubleLine.ps"),
                VertexFormat = this.vertexFormatRegion,
                Samplers = new TextureSampler[1]
                {
                    textureSampler
                },
                Parameters = RenderParameters.Create(new IRenderParameter[6]
                {
                    this.outlineColor,
                    this.outlineColor2,
                    this.outlineWidth,
                    this.outlineOffset,
                    this.depthEnabled,
                    this.frameIdParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
            this.doubleOutlinePie = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutlinePie.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutlineDoubleLine.gs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableOutlineDoubleLine.ps"),
                VertexFormat = this.vertexFormatPie,
                Samplers = new TextureSampler[1]
                {
                    textureSampler
                },
                Parameters = RenderParameters.Create(new IRenderParameter[6]
                {
                    this.outlineColor,
                    this.outlineColor2,
                    this.outlineWidth,
                    this.outlineOffset,
                    this.depthEnabled,
                    this.frameIdParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
            this.rasterizerState = RasterizerState.Create(new RasterizerStateDescription()
            {
                MultisampleEnable = true,
                CullMode = CullMode.None
            });
            this.depthStencilState = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                DepthEnable = true,
                DepthFunction = ComparisonFunction.Always,
                DepthWriteEnable = true,
                StencilEnable = false
            });
            this.Rasterizer = this.rasterizerState;
            this.DepthStencil = this.depthStencilState;
        }

        protected override void Update()
        {
            this.Effect = this.RegionsMode ? this.doubleOutlineRegion : (this.PieTechnique ? this.doubleOutlinePie : this.doubleOutline);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            DisposableResource[] disposableResourceArray = new DisposableResource[5]
            {
                this.rasterizerState,
                this.depthStencilState,
                this.doubleOutline,
                this.doubleOutlinePie,
                this.doubleOutlineRegion
            };
            foreach (DisposableResource res in disposableResourceArray)
            {
                if (res != null)
                    res.Dispose();
            }
        }
    }
}
