using Microsoft.Data.Visualization.Engine.Graphics;
using System.IO;

namespace Microsoft.Data.Visualization.Engine
{
    internal class ShadowVolumeTechnique : EffectTechnique
    {
        private RenderParameterFloat shadowScale = new RenderParameterFloat("ShadowScale");
        private RenderParameterFloat frameIdParameter = new RenderParameterFloat("FrameId");
        private VertexFormat vertexFormat;
        private VertexFormat vertexFormatPie;
        private RenderParameters sharedParameters;
        private Effect shadowEffect;
        private Effect shadowEffectPie;

        public float ShadowScale
        {
            set
            {
                this.shadowScale.Value = value;
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

        public bool PieTechnique { get; set; }

        public ShadowVolumeTechnique(RenderParameters sharedParams)
        {
            this.sharedParameters = sharedParams;
        }

        public void SetRenderStates(DepthStencilState depthStencilState, BlendState blendState, RasterizerState rasterizerState)
        {
            this.DepthStencil = depthStencilState;
            this.Blend = blendState;
            this.Rasterizer = rasterizerState;
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
            this.shadowEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableShadow.vs"),
                GeometryShaderData = null,
                PixelShaderData = null,
                VertexFormat = this.vertexFormat,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[2]
                {
                    this.shadowScale,
                    this.frameIdParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
            this.shadowEffectPie = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstancedRenderableShadowPie.vs"),
                GeometryShaderData = null,
                PixelShaderData = null,
                VertexFormat = this.vertexFormatPie,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[2]
                {
                    this.shadowScale,
                    this.frameIdParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
        }

        protected override void Update()
        {
            this.Effect = this.PieTechnique ? this.shadowEffectPie : this.shadowEffect;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.shadowEffect != null)
                this.shadowEffect.Dispose();
            if (this.shadowEffectPie == null)
                return;
            this.shadowEffectPie.Dispose();
        }
    }
}
