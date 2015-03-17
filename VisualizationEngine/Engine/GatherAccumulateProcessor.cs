using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    internal class GatherAccumulateProcessor : DisposableResource
    {
        private RenderParameterInt geoIdOffsetParameter = new RenderParameterInt("GeoIdOffset");
        private RenderParameterInt accumulateOffsetParameter = new RenderParameterInt("AccumulateOffset");
        private RenderParameterInt shiftOffsetParameter = new RenderParameterInt("ShiftOffset");
        private RenderParameterInt shiftCountParameter = new RenderParameterInt("ShiftCount");
        private RenderParameterBool ignoreValueParameter = new RenderParameterBool("IgnoreValue");
        private RenderParameterBool accumulateEnabledParameter = new RenderParameterBool("AccumulateEnabled");
        private RenderParameterBool discardZerosParameter = new RenderParameterBool("DiscardZeros");
        private RenderParameterBool discardNullsParameter = new RenderParameterBool("DiscardNulls");
        private RenderParameterBool constantModeEnabledParameter = new RenderParameterBool("ConstantModeEnabled");
        private RenderParameterBool constantModeNegativesEnabledParameter = new RenderParameterBool("ConstantModeNegativesEnabled");
        private RenderParameterBool noSustainTimeEnabledParameter = new RenderParameterBool("NoSustainTimeEnabled");
        private RenderParameterBool showNegativesParameter = new RenderParameterBool("ShowNegatives");
        private RenderParameterBool useAbsValueParameter = new RenderParameterBool("UseAbsValue");
        private RenderParameterBool useLogScaleParameter = new RenderParameterBool("UseLogScale");
        private Dictionary<InstanceBlock, int> cachedBlocks = new Dictionary<InstanceBlock, int>();
        private Dictionary<InstanceBlock, int> hitTestBlocks = new Dictionary<InstanceBlock, int>();
        private const int BatchSize = 4096;
        private const int MaxTextureWidth = 4096;
        private Texture gatherPositiveTexture;
        private RenderTarget gatherPositiveTarget;
        private Texture hitIdPositiveTexture;
        private RenderTarget hitIdPositiveTarget;
        private Texture gatherNegativeTexture;
        private Texture hitIdNegativeTexture;
        private RenderTarget gatherNegativeTarget;
        private RenderTarget hitIdNegativeTarget;
        private Texture accumulatePositiveTexture0;
        private Texture accumulatePositiveTexture1;
        private RenderTarget accumulatePositiveTarget0;
        private RenderTarget accumulatePositiveTarget1;
        private Texture accumulateNegativeTexture0;
        private Texture accumulateNegativeTexture1;
        private RenderTarget accumulateNegativeTarget0;
        private RenderTarget accumulateNegativeTarget1;
        private Texture selectPositiveTexture;
        private RenderTarget selectPositiveTarget;
        private Effect gatherEffect;
        private Effect gatherEffectTime;
        private Effect gatherHitTestEffect;
        private Effect gatherHitTestEffectTime;
        private Effect accumulateEffect;
        private Effect selectEffect;
        private RenderParameters sharedParameters;
        private DepthStencilState gatherState;
        private DepthStencilState accumulateState;
        private RasterizerState rasterizerState;
        private BlendState blendState;
        private bool ignoreZeroValues;
        private bool ignoreNullValues;
        private bool noSustainTimeScaleMode;
        private bool useLogScale;
        private bool selectStepEnabled;
        private bool selectNegativeValues;
        private bool selectAbsoluteValues;

        public Texture GatherPositiveTexture { get; private set; }

        public Texture GatherNegativeTexture { get; private set; }

        public Texture AccumulatePositiveTexture { get; private set; }

        public Texture AccumulateNegativeTexture { get; private set; }

        public Texture SelectPositiveTexture { get; private set; }

        public Texture GatherPositiveHitTestTexture { get; private set; }

        public Texture GatherNegativeHitTestTexture { get; private set; }

        public GatherAccumulateProcessor(RenderParameters sharedParams)
        {
            this.sharedParameters = sharedParams;
        }

        public void BeginFrame(bool hitTest, bool ignoreZeros, bool ignoreNulls, bool noSustainMode, bool maxValueSelect, bool showNegatives, bool useAbsoluteValuesForSelection, bool useLogarithmicScale)
        {
            if (hitTest)
                this.hitTestBlocks.Clear();
            else
                this.cachedBlocks.Clear();
            this.ignoreZeroValues = ignoreZeros;
            this.ignoreNullValues = ignoreNulls;
            this.selectStepEnabled = maxValueSelect;
            this.noSustainTimeScaleMode = noSustainMode;
            this.useLogScale = useLogarithmicScale;
            this.selectNegativeValues = showNegatives;
            this.selectAbsoluteValues = useAbsoluteValuesForSelection;
            if (hitTest)
            {
                this.GatherPositiveHitTestTexture = null;
                this.GatherNegativeHitTestTexture = null;
            }
            else
            {
                this.GatherPositiveTexture = null;
                this.GatherNegativeTexture = null;
                this.AccumulatePositiveTexture = null;
                this.AccumulateNegativeTexture = null;
                this.SelectPositiveTexture = null;
            }
        }

        public bool Process(IList<GatherAccumulateProcessBlock> blocks, Renderer renderer, bool ignoreValues, bool constantMode)
        {
            renderer.Profiler.BeginSection("[Gather]");
            bool flag = this.Gather(blocks, renderer, ignoreValues, false, constantMode);
            renderer.Profiler.EndSection();
            if (!flag)
                return false;
            renderer.Profiler.BeginSection("[Accumulate]");
            this.Accumulate(blocks, renderer, this.GatherNegativeTexture != null, constantMode);
            renderer.Profiler.EndSection();
            if (this.selectStepEnabled)
            {
                renderer.Profiler.BeginSection("[Select MaxShift]");
                this.Select(blocks, renderer);
                renderer.Profiler.EndSection();
            }
            return true;
        }

        public int Process(GatherAccumulateProcessBlock block, Renderer renderer, bool ignoreValues, bool constantMode)
        {
            if (this.cachedBlocks.ContainsKey(block.Owner))
                return this.cachedBlocks[block.Owner];
            this.Process(new GatherAccumulateProcessBlock[1]
            {
                block
            }, renderer, ignoreValues, constantMode);
            return 0;
        }

        public bool ProcessHitTest(IList<GatherAccumulateProcessBlock> blocks, Renderer renderer, bool ignoreValues, bool constantMode)
        {
            return this.Gather(blocks, renderer, ignoreValues, true, constantMode);
        }

        public int ProcessHitTest(GatherAccumulateProcessBlock block, Renderer renderer, bool ignoreValues, bool constantMode)
        {
            if (this.hitTestBlocks.ContainsKey(block.Owner))
                return this.hitTestBlocks[block.Owner];
            this.ProcessHitTest(new GatherAccumulateProcessBlock[1]
            {
                block
            }, renderer, ignoreValues, constantMode);
            return 0;
        }

        private unsafe void SetGeoIdOffset(uint startVertex, VertexBuffer instances)
        {
            InstanceBlockVertex* pData = (InstanceBlockVertex*)instances.GetData().ToPointer();
            this.geoIdOffsetParameter.Value = -pData[startVertex].GeoIndex;
        }

        private unsafe void SetGeoIdCount(GatherAccumulateProcessBlock block)
        {
            int geoIndex = 0;
            InstanceBlockVertex* instanceBlockVertexPtr = (InstanceBlockVertex*)block.Instances.GetData().ToPointer();
            if (block.PositiveSubset.Item2 > 0U)
            {
                uint positiveIndex = block.PositiveSubset.Item1 + block.PositiveSubset.Item2 - 1;
                uint* positiveIndexArray = (uint*)block.PositiveIndices.GetData().ToPointer();
                uint num2 = positiveIndexArray[positiveIndex];
                geoIndex = instanceBlockVertexPtr[num2].GeoIndex;
            }
            if (block.NegativeSubset == null || block.NegativeSubset.Item2 <= 0U)
                return;
            uint num3 = block.NegativeSubset.Item1 + block.NegativeSubset.Item2 - 1;
            uint num4 = *((uint*)block.NegativeIndices.GetData().ToPointer() + num3);
            Math.Max(geoIndex, instanceBlockVertexPtr[num4].GeoIndex);
        }

        private void EnsureGatherResources(Renderer renderer, int maxWidth, bool needsNegativeResources)
        {
            if (this.gatherPositiveTexture != null && this.gatherPositiveTexture.Width < maxWidth)
            {
                this.gatherPositiveTexture.Dispose();
                this.gatherPositiveTarget.Dispose();
                this.hitIdPositiveTexture.Dispose();
                this.hitIdPositiveTarget.Dispose();
                this.gatherPositiveTexture = null;
                this.gatherPositiveTarget = null;
                this.hitIdPositiveTexture = null;
                this.hitIdPositiveTarget = null;
                this.GatherPositiveTexture = null;
            }
            if (this.gatherPositiveTexture == null)
            {
                using (Image textureData = new Image(IntPtr.Zero, maxWidth, BatchSize, PixelFormat.Float32Bpp))
                {
                    this.gatherPositiveTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.gatherPositiveTarget = RenderTarget.Create(this.gatherPositiveTexture, RenderTargetDepthStencilMode.FloatDepthEnabled);
                }
                using (Image textureData = new Image(IntPtr.Zero, maxWidth, BatchSize, PixelFormat.Rgba32Bpp))
                {
                    this.hitIdPositiveTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.hitIdPositiveTarget = RenderTarget.Create(this.hitIdPositiveTexture, RenderTargetDepthStencilMode.FloatDepthEnabled);
                }
            }
            if (this.gatherNegativeTexture != null && this.gatherNegativeTexture.Width < maxWidth)
            {
                this.gatherNegativeTexture.Dispose();
                this.gatherNegativeTarget.Dispose();
                this.gatherNegativeTexture = null;
                this.gatherNegativeTarget = null;
                this.hitIdNegativeTexture.Dispose();
                this.hitIdNegativeTarget.Dispose();
                this.hitIdNegativeTexture = null;
                this.hitIdNegativeTarget = null;
                this.GatherNegativeTexture = null;
            }
            if (this.gatherNegativeTexture == null && needsNegativeResources)
            {
                using (Image textureData = new Image(IntPtr.Zero, maxWidth, BatchSize, PixelFormat.Float32Bpp))
                {
                    this.gatherNegativeTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.gatherNegativeTarget = RenderTarget.Create(this.gatherNegativeTexture, RenderTargetDepthStencilMode.FloatDepthEnabled);
                }
                using (Image textureData = new Image(IntPtr.Zero, maxWidth, BatchSize, PixelFormat.Rgba32Bpp))
                {
                    this.hitIdNegativeTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.hitIdNegativeTarget = RenderTarget.Create(this.hitIdNegativeTexture, RenderTargetDepthStencilMode.FloatDepthEnabled);
                }
            }
            if (this.gatherState == null)
            {
                this.gatherState = DepthStencilState.Create(new DepthStencilStateDescription()
                {
                    DepthEnable = true,
                    DepthFunction = ComparisonFunction.GreaterEqual,
                    DepthWriteEnable = true,
                    StencilEnable = false
                });
                this.rasterizerState = RasterizerState.Create(new RasterizerStateDescription()
                {
                    CullMode = CullMode.None,
                    DepthClipEnable = false
                });
                this.blendState = BlendState.Create(new BlendStateDescription()
                {
                    BlendEnable = false
                });
            }
            if (this.gatherEffect != null)
                return;
            this.gatherEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceGather.vs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceGather.ps"),
                VertexFormat = new InstanceBlockVertex().Format,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[8]
                {
                    this.geoIdOffsetParameter,
                    this.shiftOffsetParameter,
                    this.ignoreValueParameter,
                    this.discardZerosParameter,
                    this.discardNullsParameter,
                    this.constantModeEnabledParameter,
                    this.noSustainTimeEnabledParameter,
                    this.useLogScaleParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
            VertexFormat vertexFormat1 = VertexFormat.Create(new VertexComponent[5]
            {
                this.gatherEffect.VertexFormat.Components[0],
                this.gatherEffect.VertexFormat.Components[1],
                this.gatherEffect.VertexFormat.Components[2],
                this.gatherEffect.VertexFormat.Components[3],
                new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats,
                    VertexComponentClassification.PerVertexData, 1)
            });
            this.gatherHitTestEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceGatherHitTest.vs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceGatherHitTest.ps"),
                VertexFormat = vertexFormat1,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[8]
                {
                    this.geoIdOffsetParameter,
                    this.shiftOffsetParameter,
                    this.ignoreValueParameter,
                    this.discardZerosParameter,
                    this.discardNullsParameter,
                    this.constantModeEnabledParameter,
                    this.noSustainTimeEnabledParameter,
                    this.useLogScaleParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
            VertexFormat vertexFormat2 = VertexFormat.Create(new VertexComponent[5]
            {
                this.gatherEffect.VertexFormat.Components[0],
                this.gatherEffect.VertexFormat.Components[1],
                this.gatherEffect.VertexFormat.Components[2],
                this.gatherEffect.VertexFormat.Components[3],
                new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                    VertexComponentClassification.PerVertexData, 1)
            });
            this.gatherEffectTime = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceGatherTime.vs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceGather.ps"),
                VertexFormat = vertexFormat2,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[8]
                {
                    this.geoIdOffsetParameter,
                    this.shiftOffsetParameter,
                    this.ignoreValueParameter,
                    this.discardZerosParameter,
                    this.discardNullsParameter,
                    this.constantModeEnabledParameter,
                    this.noSustainTimeEnabledParameter,
                    this.useLogScaleParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
            VertexComponent vertexComponent = new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats, VertexComponentClassification.PerVertexData, 2);
            VertexFormat vertexFormat3 = VertexFormat.Create(new VertexComponent[6]
            {
                vertexFormat2.Components[0],
                vertexFormat2.Components[1],
                vertexFormat2.Components[2],
                vertexFormat2.Components[3],
                vertexFormat2.Components[4],
                vertexComponent
            });
            this.gatherHitTestEffectTime = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceGatherTimeHitTest.vs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceGatherHitTest.ps"),
                VertexFormat = vertexFormat3,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[8]
                {
                    this.geoIdOffsetParameter,
                    this.shiftOffsetParameter,
                    this.ignoreValueParameter,
                    this.discardZerosParameter,
                    this.discardNullsParameter,
                    this.constantModeEnabledParameter,
                    this.noSustainTimeEnabledParameter,
                    this.useLogScaleParameter
                }),
                SharedParameters = new RenderParameters[1]
                {
                    this.sharedParameters
                }
            });
        }

        private void EnsureAccumulateResources(Renderer renderer, int maxWidth, bool needsNegativeResources)
        {
            if (this.accumulatePositiveTexture0 != null && this.accumulatePositiveTexture0.Width < maxWidth)
            {
                this.accumulatePositiveTexture0.Dispose();
                this.accumulatePositiveTexture1.Dispose();
                this.accumulatePositiveTarget0.Dispose();
                this.accumulatePositiveTarget1.Dispose();
                this.accumulatePositiveTexture0 = null;
                this.accumulatePositiveTexture1 = null;
                this.accumulatePositiveTarget0 = null;
                this.accumulatePositiveTarget1 = null;
                this.AccumulatePositiveTexture = null;
            }
            if (this.accumulatePositiveTexture0 == null)
            {
                using (Image textureData = new Image(IntPtr.Zero, maxWidth, BatchSize, PixelFormat.Float32Bpp))
                {
                    this.accumulatePositiveTexture0 = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.accumulatePositiveTexture1 = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.accumulatePositiveTarget0 = RenderTarget.Create(this.accumulatePositiveTexture0, RenderTargetDepthStencilMode.None);
                    this.accumulatePositiveTarget1 = RenderTarget.Create(this.accumulatePositiveTexture1, RenderTargetDepthStencilMode.None);
                }
            }
            if (this.accumulateNegativeTexture0 != null && this.accumulateNegativeTexture0.Width < maxWidth && needsNegativeResources)
            {
                this.accumulateNegativeTexture0.Dispose();
                this.accumulateNegativeTexture1.Dispose();
                this.accumulateNegativeTarget0.Dispose();
                this.accumulateNegativeTarget1.Dispose();
                this.accumulateNegativeTexture0 = null;
                this.accumulateNegativeTexture1 = null;
                this.accumulateNegativeTarget0 = null;
                this.accumulateNegativeTarget1 = null;
                this.AccumulateNegativeTexture = null;
            }
            if (this.accumulateNegativeTexture0 == null && needsNegativeResources)
            {
                using (Image textureData = new Image(IntPtr.Zero, maxWidth, BatchSize, PixelFormat.Float32Bpp))
                {
                    this.accumulateNegativeTexture0 = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.accumulateNegativeTexture1 = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.accumulateNegativeTarget0 = RenderTarget.Create(this.accumulateNegativeTexture0, RenderTargetDepthStencilMode.None);
                    this.accumulateNegativeTarget1 = RenderTarget.Create(this.accumulateNegativeTexture1, RenderTargetDepthStencilMode.None);
                }
            }
            if (this.accumulateState == null)
                this.accumulateState = DepthStencilState.Create(new DepthStencilStateDescription()
                {
                    DepthEnable = false,
                    StencilEnable = false
                });
            if (this.accumulateEffect != null)
                return;
            this.accumulateEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceAccumulate.vs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceAccumulate.ps"),
                VertexFormat = null,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[6]
                {
                    this.accumulateOffsetParameter,
                    this.accumulateEnabledParameter,
                    this.shiftOffsetParameter,
                    this.shiftCountParameter,
                    this.constantModeEnabledParameter,
                    this.constantModeNegativesEnabledParameter
                })
            });
        }

        private void EnsureSelectResources(Renderer renderer, int maxWidth)
        {
            if (this.selectPositiveTexture != null && this.selectPositiveTexture.Width < maxWidth)
            {
                this.selectPositiveTexture.Dispose();
                this.selectPositiveTarget.Dispose();
                this.selectPositiveTexture = null;
                this.selectPositiveTarget = null;
            }
            if (this.selectPositiveTexture == null)
            {
                using (Image textureData = new Image(IntPtr.Zero, maxWidth, BatchSize, PixelFormat.Float32Bpp))
                {
                    this.selectPositiveTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                    this.selectPositiveTarget = RenderTarget.Create(this.selectPositiveTexture, RenderTargetDepthStencilMode.None);
                }
            }
            if (this.selectEffect != null)
                return;
            this.selectEffect = Effect.Create(new EffectDefinition()
            {
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceSelect.vs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.InstanceSelect.ps"),
                VertexFormat = null,
                Samplers = null,
                Parameters = RenderParameters.Create(new IRenderParameter[4]
                {
                    this.shiftOffsetParameter,
                    this.shiftCountParameter,
                    this.showNegativesParameter,
                    this.useAbsValueParameter
                })
            });
        }

        private bool Gather(IList<GatherAccumulateProcessBlock> processBlocks, Renderer renderer, bool ignoreValues, bool hitTest, bool useConstantMode)
        {
            if (processBlocks == null || processBlocks.Count == 0)
                return false;
            if (hitTest)
                this.hitTestBlocks.Clear();
            else
                this.cachedBlocks.Clear();
            if (hitTest)
            {
                this.GatherPositiveHitTestTexture = null;
                this.GatherNegativeHitTestTexture = null;
            }
            else
            {
                this.GatherPositiveTexture = null;
                this.GatherNegativeTexture = null;
            }
            int maxWidth = 0;
            bool needsNegativeResources = false;
            bool flag1 = false;
            foreach (GatherAccumulateProcessBlock accumulateProcessBlock in processBlocks)
            {
                maxWidth += accumulateProcessBlock.MaxShift + 1;
                needsNegativeResources = needsNegativeResources | accumulateProcessBlock.NegativeSubset != null;
                flag1 = flag1 | accumulateProcessBlock.Owner.TimeEnabled;
            }
            if (maxWidth > MaxTextureWidth)
                return false;
            this.EnsureGatherResources(renderer, maxWidth, needsNegativeResources);
            this.EnsureAccumulateResources(renderer, maxWidth, needsNegativeResources);
            if (this.selectStepEnabled)
                this.EnsureSelectResources(renderer, processBlocks.Count);
            this.ignoreValueParameter.Value = ignoreValues;
            this.discardZerosParameter.Value = this.ignoreZeroValues;
            this.discardNullsParameter.Value = this.ignoreNullValues;
            this.constantModeEnabledParameter.Value = useConstantMode;
            this.noSustainTimeEnabledParameter.Value = this.noSustainTimeScaleMode;
            this.useLogScaleParameter.Value = this.useLogScale;
            this.showNegativesParameter.Value = this.selectNegativeValues;
            if (hitTest)
                renderer.SetEffect(flag1 ? this.gatherHitTestEffectTime : this.gatherHitTestEffect);
            else
                renderer.SetEffect(flag1 ? this.gatherEffectTime : this.gatherEffect);
            renderer.SetRasterizerState(this.rasterizerState);
            renderer.SetBlendState(this.blendState);
            renderer.SetDepthStencilState(this.gatherState);
            bool flag2 = false;
            for (int index = 0; index < 2; ++index)
            {
                int num1 = 0;
                if (hitTest)
                    renderer.BeginRenderTargetFrame(flag2 ? this.hitIdNegativeTarget : this.hitIdPositiveTarget, new Color4F?(new Color4F(0.0f, 0.0f, 0.0f, 0.0f)));
                else
                    renderer.BeginRenderTargetFrame(flag2 ? this.gatherNegativeTarget : this.gatherPositiveTarget, new Color4F?(new Color4F(0.0f, -1f, 0.0f, 0.0f)));
                try
                {
                    foreach (GatherAccumulateProcessBlock accumulateProcessBlock in processBlocks)
                    {
                        if (flag2 && accumulateProcessBlock.NegativeSubset == null)
                        {
                            num1 += accumulateProcessBlock.MaxShift + 1;
                        }
                        else
                        {
                            if (hitTest)
                            {
                                Renderer renderer1 = renderer;
                                VertexBuffer[] vertexBuffers;
                                if (!flag1)
                                    vertexBuffers = new VertexBuffer[2]
                  {
                    accumulateProcessBlock.Instances,
                    accumulateProcessBlock.InstancesHitId
                  };
                                else
                                    vertexBuffers = new VertexBuffer[3]
                  {
                    accumulateProcessBlock.Instances,
                    accumulateProcessBlock.InstancesTime,
                    accumulateProcessBlock.InstancesHitId
                  };
                                renderer1.SetVertexSource(vertexBuffers);
                            }
                            else
                            {
                                Renderer renderer1 = renderer;
                                VertexBuffer[] vertexBuffers;
                                if (!flag1)
                                    vertexBuffers = new VertexBuffer[1]
                  {
                    accumulateProcessBlock.Instances
                  };
                                else
                                    vertexBuffers = new VertexBuffer[2]
                  {
                    accumulateProcessBlock.Instances,
                    accumulateProcessBlock.InstancesTime
                  };
                                renderer1.SetVertexSource(vertexBuffers);
                            }
                            renderer.SetIndexSource(flag2 ? accumulateProcessBlock.NegativeIndices : accumulateProcessBlock.PositiveIndices);
                            this.SetGeoIdOffset(accumulateProcessBlock.PositiveSubset.Item1, accumulateProcessBlock.Instances);
                            this.shiftOffsetParameter.Value = num1;
                            uint num2 = flag2 ? accumulateProcessBlock.NegativeSubset.Item1 : accumulateProcessBlock.PositiveSubset.Item1;
                            uint num3 = flag2 ? accumulateProcessBlock.NegativeSubset.Item2 : accumulateProcessBlock.PositiveSubset.Item2;
                            renderer.DrawIndexed((int)num2, (int)num3, PrimitiveTopology.PointList);
                            if (index == 0)
                            {
                                if (hitTest)
                                    this.hitTestBlocks.Add(accumulateProcessBlock.Owner, num1);
                                else
                                    this.cachedBlocks.Add(accumulateProcessBlock.Owner, num1);
                            }
                            num1 += accumulateProcessBlock.MaxShift + 1;
                        }
                    }
                }
                finally
                {
                    renderer.EndRenderTargetFrame();
                }
                if (needsNegativeResources)
                    flag2 = true;
                else
                    break;
            }
            if (hitTest)
            {
                this.GatherPositiveHitTestTexture = this.hitIdPositiveTexture;
                if (needsNegativeResources)
                    this.GatherNegativeHitTestTexture = this.hitIdNegativeTexture;
            }
            else
            {
                this.GatherPositiveTexture = this.gatherPositiveTarget.RenderTargetTexture;
                if (needsNegativeResources)
                    this.GatherNegativeTexture = this.gatherNegativeTarget.RenderTargetTexture;
            }
            return true;
        }

        private void Accumulate(IEnumerable<GatherAccumulateProcessBlock> processBlocks, Renderer renderer, bool needsNegativeValues, bool useConstantMode)
        {
            renderer.SetEffect(this.accumulateEffect);
            renderer.SetDepthStencilState(this.accumulateState);
            this.constantModeEnabledParameter.Value = useConstantMode;
            renderer.SetIndexSource(null);
            renderer.SetVertexSource((VertexBuffer)null);
            int num1 = 0;
            foreach (GatherAccumulateProcessBlock accumulateProcessBlock in processBlocks)
                num1 = Math.Max(num1, (int)Math.Ceiling(Math.Log(accumulateProcessBlock.MaxShift + 1, 2.0)));
            if (useConstantMode)
                num1 = Math.Max(1, num1);
            bool flag = false;
            for (int index1 = 0; index1 < (needsNegativeValues ? 2 : 1); ++index1)
            {
                RenderTarget[] renderTargetArray1;
                if (!flag)
                    renderTargetArray1 = new RenderTarget[2]
          {
            this.accumulatePositiveTarget0,
            this.accumulatePositiveTarget1
          };
                else
                    renderTargetArray1 = new RenderTarget[2]
          {
            this.accumulateNegativeTarget0,
            this.accumulateNegativeTarget1
          };
                RenderTarget[] renderTargetArray2 = renderTargetArray1;
                renderer.SetTexture(0, flag ? this.gatherNegativeTexture : this.gatherPositiveTexture);
                renderer.SetTexture(1, flag ? null : this.gatherNegativeTexture);
                if (flag)
                    this.AccumulateNegativeTexture = this.gatherNegativeTarget.RenderTargetTexture;
                else
                    this.AccumulatePositiveTexture = this.gatherPositiveTarget.RenderTargetTexture;
                for (int index2 = 0; index2 < num1; ++index2)
                {
                    try
                    {
                        int num2 = 0;
                        renderer.BeginRenderTargetFrame(renderTargetArray2[index2 % 2], new Color4F?());
                        foreach (GatherAccumulateProcessBlock block in processBlocks)
                        {
                            int val2 = (int)Math.Ceiling(Math.Log(block.MaxShift + 1, 2.0));
                            if (useConstantMode)
                                val2 = Math.Max(1, val2);
                            if (index2 <= val2)
                            {
                                this.SetGeoIdCount(block);
                                this.accumulateOffsetParameter.Value = (int)Math.Pow(2.0, index2);
                                this.shiftOffsetParameter.Value = num2;
                                this.shiftCountParameter.Value = block.MaxShift + 1;
                                this.accumulateEnabledParameter.Value = index2 < val2;
                                this.constantModeNegativesEnabledParameter.Value = block.NegativeSubset != null;
                                renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
                            }
                            num2 += block.MaxShift + 1;
                        }
                    }
                    finally
                    {
                        renderer.EndRenderTargetFrame();
                    }
                    if (flag)
                        this.AccumulateNegativeTexture = renderTargetArray2[index2 % 2].RenderTargetTexture;
                    else
                        this.AccumulatePositiveTexture = renderTargetArray2[index2 % 2].RenderTargetTexture;
                    renderer.SetTexture(0, renderTargetArray2[index2 % 2].RenderTargetTexture);
                    renderer.SetTexture(1, null);
                }
                flag = true;
                if (useConstantMode)
                {
                    this.AccumulateNegativeTexture = this.AccumulatePositiveTexture;
                    break;
                }
            }
            renderer.SetTexture(0, null);
        }

        private void Select(IEnumerable<GatherAccumulateProcessBlock> processBlocks, Renderer renderer)
        {
            renderer.SetEffect(this.selectEffect);
            renderer.SetDepthStencilState(this.accumulateState);
            renderer.SetIndexSource(null);
            renderer.SetVertexSource((VertexBuffer)null);
            renderer.SetTexture(0, this.gatherPositiveTexture);
            renderer.SetTexture(1, this.gatherNegativeTexture);
            this.SelectPositiveTexture = this.selectPositiveTarget.RenderTargetTexture;
            try
            {
                int num = 0;
                renderer.BeginRenderTargetFrame(this.selectPositiveTarget, new Color4F?());
                foreach (GatherAccumulateProcessBlock block in processBlocks)
                {
                    this.SetGeoIdCount(block);
                    this.shiftOffsetParameter.Value = num;
                    this.shiftCountParameter.Value = block.MaxShift + 1;
                    this.useAbsValueParameter.Value = this.selectAbsoluteValues;
                    renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
                    num += block.MaxShift + 1;
                }
            }
            finally
            {
                renderer.EndRenderTargetFrame();
            }
            renderer.SetTexture(0, null);
            renderer.SetTexture(1, null);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            DisposableResource[] disposableResourceArray = new DisposableResource[28]
      {
        this.gatherPositiveTexture,
        this.hitIdPositiveTexture,
        this.gatherPositiveTarget,
        this.hitIdPositiveTarget,
        this.accumulatePositiveTexture0,
        this.accumulatePositiveTexture1,
        this.gatherNegativeTexture,
        this.hitIdNegativeTexture,
        this.gatherNegativeTarget,
        this.hitIdNegativeTarget,
        this.accumulatePositiveTarget0,
        this.accumulatePositiveTarget1,
        this.accumulateNegativeTexture0,
        this.accumulateNegativeTexture1,
        this.accumulateNegativeTarget0,
        this.accumulateNegativeTarget1,
        this.gatherEffect,
        this.gatherEffectTime,
        this.accumulateEffect,
        this.gatherState,
        this.accumulateState,
        this.rasterizerState,
        this.blendState,
        this.gatherHitTestEffect,
        this.gatherHitTestEffectTime,
        this.selectEffect,
        this.selectPositiveTexture,
        this.selectPositiveTarget
      };
            foreach (DisposableResource disposableResource in disposableResourceArray)
            {
                if (disposableResource != null && !disposableResource.Disposed)
                    disposableResource.Dispose();
            }
        }
    }
}
