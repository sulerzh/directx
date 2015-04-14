using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
    internal class AnnotationTechnique : EffectTechnique
    {
        private RenderParameterVector4F dimensionsParameter = new RenderParameterVector4F("AnnotationDimensions");
        private RenderParameterVector2F textureDimensionsParameter = new RenderParameterVector2F("TextureDimensions");
        private RenderParameterFloat scaleParameter = new RenderParameterFloat("AnnotationScale");
        private RenderParameterColor4F outlineColorParameter = new RenderParameterColor4F("OutlineAnnotationColor");
        private RenderParameterColor4F fillColorParameter = new RenderParameterColor4F("FillAnnotationColor");
        private RenderParameterFloat fadeFactorParameter = new RenderParameterFloat("FadeFactor");
        private RenderParameterFloat frameIdParameter = new RenderParameterFloat("FrameId");
        private RenderParameterBool textureFilteringParameter = new RenderParameterBool("TextureFiltering");
        private RenderParameterBool renderOnTopParameter = new RenderParameterBool("RenderOnTop");
        private VertexFormat vertexFormat;
        private VertexFormat hitTestVertexFormat;
        private Effect effect;
        private Effect hitTestEffect;
        private RasterizerState rasterizerState;
        private BlendState blendState;
        private DepthStencilState depthStencilState;
        private RenderParameters sharedParameters;
        private AnnotationStyle annotationStyle;

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

        public bool HitTestEnabled { get; set; }

        public AnnotationStyle Style
        {
            get
            {
                return this.annotationStyle;
            }
            set
            {
                this.annotationStyle = value;
                this.dimensionsParameter.Value = new Vector4F(this.annotationStyle.TailDimensions.X, this.annotationStyle.TailDimensions.Y, this.annotationStyle.TailDimensions.Z, this.annotationStyle.OutlineWidth);
                this.scaleParameter.Value = this.annotationStyle.AnnotationScale;
                this.outlineColorParameter.Value = this.annotationStyle.OutlineColor;
                this.fillColorParameter.Value = this.annotationStyle.FillColor;
            }
        }

        public Vector2F TextureDimensions
        {
            get
            {
                return this.textureDimensionsParameter.Value;
            }
            set
            {
                this.textureDimensionsParameter.Value = value;
            }
        }

        public bool TextureFiltering
        {
            get
            {
                return this.textureFilteringParameter.Value;
            }
            set
            {
                this.textureFilteringParameter.Value = value;
            }
        }

        public bool RenderOnTop
        {
            get
            {
                return this.renderOnTopParameter.Value;
            }
            set
            {
                this.renderOnTopParameter.Value = value;
            }
        }

        public AnnotationTechnique(RenderParameters sharedParams)
        {
            this.sharedParameters = sharedParams;
        }

        public void SetRenderStates(DepthStencilState depthStencilState, BlendState blendState, RasterizerState rasterizerState)
        {
            this.DepthStencil = depthStencilState;
            this.Blend = blendState;
            this.Rasterizer = rasterizerState;
        }

        protected override void Update()
        {
            this.fadeFactorParameter.Value = 1f;
            if (!this.HitTestEnabled)
            {
                this.DepthStencil = this.depthStencilState;
                this.Rasterizer = this.rasterizerState;
                this.Blend = this.blendState;
            }
            this.Effect = this.HitTestEnabled ? this.hitTestEffect : this.effect;
        }

        protected override void Initialize()
        {
            this.vertexFormat = VertexFormat.Create(new VertexComponent[4]
            {
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3,
                    VertexComponentClassification.PerVertexData, 0),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerVertexData, 0),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerVertexData, 0),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float,
                    VertexComponentClassification.PerVertexData, 0)
            });
            this.hitTestVertexFormat = VertexFormat.Create(new VertexComponent[5]
            {
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3,
                    VertexComponentClassification.PerVertexData, 0),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerVertexData, 0),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerVertexData, 0),
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float,
                    VertexComponentClassification.PerVertexData, 0),
                new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4,
                    VertexComponentClassification.PerVertexData, 1)
            });
            TextureSampler textureSampler1 = TextureSampler.Create(TextureFilter.Point, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(0.0f, 0.0f, 0.0f, 0.0f));
            TextureSampler textureSampler2 = TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(0.0f, 0.0f, 0.0f, 0.0f));
            TextureSampler textureSampler3 = TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(), ComparisonFunction.LessEqual);
            this.effect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.Annotation.vs"),
                GeometryShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.Annotation.gs"),
                PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.Annotation.ps"),
                VertexFormat = this.vertexFormat,
                Samplers = new TextureSampler[]
                {
                    textureSampler1,
                    textureSampler2,
                    textureSampler3
                },
                Parameters = RenderParameters.Create(new IRenderParameter[]
                {
                    this.outlineColorParameter,
                    this.fillColorParameter,
                    this.dimensionsParameter,
                    this.textureDimensionsParameter,
                    this.scaleParameter,
                    this.fadeFactorParameter,
                    this.frameIdParameter,
                    this.textureFilteringParameter,
                    this.renderOnTopParameter
                }),
                SharedParameters = new RenderParameters[]
                {
                    this.sharedParameters
                }
            });
            this.hitTestEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.AnnotationHitTest.vs"),
                GeometryShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.AnnotationHitTest.gs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.AnnotationHitTest.ps"),
                VertexFormat = this.hitTestVertexFormat,
                Samplers = new TextureSampler[]
                {
                    textureSampler1,
                    textureSampler2,
                    textureSampler3
                },
                Parameters = RenderParameters.Create(new IRenderParameter[]
                {
                    this.outlineColorParameter,
                    this.fillColorParameter,
                    this.dimensionsParameter,
                    this.textureDimensionsParameter,
                    this.scaleParameter,
                    this.fadeFactorParameter,
                    this.frameIdParameter,
                    this.textureFilteringParameter,
                    this.renderOnTopParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
            this.rasterizerState = RasterizerState.Create(new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
                MultisampleEnable = true,
                ScissorEnable = false
            });
            this.blendState = BlendState.Create(new BlendStateDescription()
            {
                BlendEnable = true,
                SourceBlend = BlendFactor.SourceAlpha,
                DestBlend = BlendFactor.InvSourceAlpha,
                SourceBlendAlpha = BlendFactor.SourceAlpha,
                DestBlendAlpha = BlendFactor.One
            });
            this.depthStencilState = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                DepthEnable = true,
                DepthWriteEnable = true,
                DepthFunction = ComparisonFunction.Less,
                StencilEnable = false
            });
            this.Effect = this.effect;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            DisposableResource[] disposableResourceArray = new DisposableResource[]
            {
                this.effect,
                this.hitTestEffect,
                this.rasterizerState,
                this.blendState,
                this.depthStencilState
            };
            foreach (DisposableResource res in disposableResourceArray)
            {
                if (res != null)
                {
                    res.Dispose();
                }
            }
        }
    }
}
