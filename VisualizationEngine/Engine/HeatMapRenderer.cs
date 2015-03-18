using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    internal class HeatMapRenderer : DisposableResource
    {
        private List<VertexBuffer> valueBuffers = new List<VertexBuffer>();
        private List<VertexBuffer> timeBuffers = new List<VertexBuffer>();
        private List<Tuple<int, int>> positiveSubsets = new List<Tuple<int, int>>();
        private List<Tuple<int, int>> negativeSubsets = new List<Tuple<int, int>>();
        private List<Tuple<int, int>> zeroSubsets = new List<Tuple<int, int>>();
        private List<Tuple<int, int>> nullSubsets = new List<Tuple<int, int>>();
        private bool firstDataUpdate = true;
        private const int MaxBufferSize = 65536;
        private int totalVertexCount;
        private int totalVertexCapacity;
        private int totalTimeVertexCapacity;
        private VertexBuffer screenQuad;
        private Texture rampTexture;
        private RenderTarget dataRenderTarget;
        private Texture dataRenderTargetTexture;
        private RenderTarget dataRenderTargetSecondary;
        private Texture dataRenderTargetTextureSecondary;
        private HeatMapTechnique renderTechnique;
        private LayerScaling scaling;
        private LayerTimeScaling timeScaling;
        private float minHeatValue;
        private float minPositiveHeatValue;
        private float maxHeatValue;
        private DateTime? minHeatTime;
        private DateTime? maxHeatTime;

        public float Alpha
        {
            get
            {
                return this.renderTechnique.Alpha;
            }
            set
            {
                this.renderTechnique.Alpha = value;
            }
        }

        public float MaxValueForAlpha
        {
            get
            {
                return this.renderTechnique.MaxValueForAlpha;
            }
            set
            {
                this.renderTechnique.MaxValueForAlpha = value;
            }
        }

        public float MaxHeatFactor { get; set; }

        public float CircleOfInfluence { get; set; }

        public bool IsVariableCircleOfInfluence { get; set; }

        public HeatMapBlendMode BlendMode
        {
            get
            {
                return this.renderTechnique.BlendMode;
            }
            set
            {
                this.renderTechnique.BlendMode = value;
            }
        }

        public bool GaussianBlurEnable { get; set; }

        public bool ShowNegatives { get; set; }

        public bool ShowZeros { get; set; }

        public bool ShowNulls { get; set; }

        public bool IsDirty
        {
            get
            {
                return this.scaling.EaseInScale < 1.0;
            }
        }

        public HeatMapRenderer(LayerScaling layerScaling, LayerTimeScaling layerTimeScaling)
        {
            this.renderTechnique = new HeatMapTechnique();
            this.scaling = layerScaling;
            this.timeScaling = layerTimeScaling;
            this.screenQuad = VertexBuffer.Create(new Vertex.Position2D[4]
            {
                new Vertex.Position2D(-1f, 1f),
                new Vertex.Position2D(1f, 1f),
                new Vertex.Position2D(-1f, -1f),
                new Vertex.Position2D(1f, -1f)
            }, false);
        }

        public unsafe void SetVertexData(HeatMapVertex[] valueVertices, HeatMapTimeVertex[] timeVertices, int startVertex, int vCount, float minValue, float minPositiveValue, float maxValue, DateTime? minTime, DateTime? maxTime, SceneState state)
        {
            if (timeVertices != null && minTime == maxTime)
            {
                timeVertices = null;
                minTime = null;
                maxTime = null;
            }
            this.nullSubsets.Clear();
            this.negativeSubsets.Clear();
            this.zeroSubsets.Clear();
            this.positiveSubsets.Clear();
            this.totalVertexCount = 0;
            for (int i = 0; i <= vCount / MaxBufferSize; ++i)
            {
                if (i > this.valueBuffers.Count - 1)
                {
                    this.valueBuffers.Add(VertexBuffer.Create<HeatMapVertex>(null, MaxBufferSize, false));
                    this.totalVertexCapacity += MaxBufferSize;
                }
                int num1 = i * MaxBufferSize;
                int num2 = Math.Min(vCount - num1, MaxBufferSize);
                this.totalVertexCount += num2;
                Tuple<int, int>[] subsets = HeatMapRenderer.ComputeSubsets(valueVertices, timeVertices, num1, num2);
                this.nullSubsets.Add(subsets[0]);
                this.negativeSubsets.Add(subsets[1]);
                this.zeroSubsets.Add(subsets[2]);
                this.positiveSubsets.Add(subsets[3]);
                this.valueBuffers[i].Update(valueVertices, num1, 0, num2);
                if (subsets[0] != null)
                {
                    HeatMapVertex* pValueBuffer = (HeatMapVertex*)this.valueBuffers[i].GetData().ToPointer();
                    for (int j = subsets[0].Item1; j < subsets[0].Item1 + subsets[0].Item2; ++j)
                        pValueBuffer[j].Value = 0.0f;
                    this.valueBuffers[i].SetDirty();
                }
            }
            if (timeVertices != null)
            {
                for (int i = 0; i <= vCount / MaxBufferSize; ++i)
                {
                    if (i > this.timeBuffers.Count - 1)
                    {
                        this.timeBuffers.Add(VertexBuffer.Create<HeatMapTimeVertex>(null, MaxBufferSize, false));
                        this.totalTimeVertexCapacity += MaxBufferSize;
                    }
                    int sourceIndex = i * MaxBufferSize;
                    int vertexCount = Math.Min(vCount - sourceIndex, MaxBufferSize);
                    this.timeBuffers[i].Update(timeVertices, sourceIndex, 0, vertexCount);
                }
            }
            this.maxHeatValue = maxValue;
            this.minHeatValue = (double)minValue >= (double)maxValue ? this.maxHeatValue - 1f : minValue;
            this.minPositiveHeatValue = (double)minPositiveValue >= (double)maxValue ? this.maxHeatValue - 1f : minPositiveValue;
            this.minHeatTime = minTime;
            this.maxHeatTime = maxTime;
            if (this.firstDataUpdate)
                this.scaling.SetCreationTime();
            this.firstDataUpdate = startVertex == 0;
        }

        private static Tuple<int, int>[] ComputeSubsets(HeatMapVertex[] valueVertices, HeatMapTimeVertex[] timeVertices, int start, int count)
        {
            if (timeVertices != null)
                Array.Sort(valueVertices, timeVertices, start, count, new HeatMapRenderer.VertexComparer());
            else
                Array.Sort(valueVertices, start, count, new HeatMapRenderer.VertexComparer());
            Tuple<int, int> tuple1 = null;
            Tuple<int, int> tuple2 = null;
            Tuple<int, int> tuple3 = null;
            Tuple<int, int> tuple4 = null;
            int index = start;
            if (valueVertices[index].Value < 0.0)
            {
                int num = index;
                while (index < start + count && valueVertices[index].Value < 0.0)
                    ++index;
                tuple2 = new Tuple<int, int>(num - start, index - num);
            }
            if (index < start + count && valueVertices[index].Value == 0.0)
            {
                int num = index;
                while (index < start + count && valueVertices[index].Value == 0.0)
                    ++index;
                tuple3 = new Tuple<int, int>(num - start, index - num);
            }
            if (index < start + count && valueVertices[index].Value > 0.0)
            {
                int num = index;
                while (index < start + count && valueVertices[index].Value > 0.0)
                    ++index;
                tuple4 = new Tuple<int, int>(num - start, index - num);
            }
            if (index < start + count)
                tuple1 = new Tuple<int, int>(index - start, count - index);
            return new Tuple<int, int>[4]
            {
                tuple1,
                tuple2,
                tuple3,
                tuple4
            };
        }

        public void Draw(Renderer renderer, SceneState state)
        {
            if (this.rampTexture == null)
            {
                using (Image textureData = new Image(this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Resources.ColorRamp.png")))
                {
                    this.rampTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.Static);
                    this.rampTexture.OnReset += this.rampTexture_OnReset;
                }
            }
            renderer.Profiler.BeginSection("[HeatMap] Step 0");
            this.DrawDataStep(renderer, state);
            renderer.Profiler.EndSection();
            renderer.Profiler.BeginSection("[HeatMap] Step 1");
            if (this.GaussianBlurEnable)
                this.DrawGaussianBlurStep(renderer, state);
            renderer.Profiler.EndSection();
            renderer.Profiler.BeginSection("[HeatMap] Step 2");
            this.DrawColorStep(renderer, state);
            renderer.Profiler.EndSection();
        }

        private void rampTexture_OnReset(object sender, EventArgs e)
        {
            this.rampTexture.Update(new Image(this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Resources.ColorRamp.png")), true);
        }

        private void DrawDataStep(Renderer renderer, SceneState state)
        {
            this.scaling.Update(state);
            this.timeScaling.Update(this.minHeatTime, this.maxHeatTime, state);
            this.renderTechnique.CircleOfInfluence = this.CircleOfInfluence * this.scaling.ViewScale;
            this.renderTechnique.IsVariableCircleOfInfluence = this.IsVariableCircleOfInfluence;
            this.renderTechnique.FadeTime = 0.25f;
            this.renderTechnique.TimeEnabled = this.timeScaling.TimeEnabled;
            this.renderTechnique.VisualTime = this.timeScaling.VisualTime;
            this.renderTechnique.VisualTimeFreeze = this.timeScaling.VisualTimeFreeze;
            this.renderTechnique.VisualTimeFreezeEnabled = this.timeScaling.VisualTimeFreezeEnabled;
            this.renderTechnique.VisualTimeScale = this.timeScaling.VisualTimeScale;
            this.SetMinMaxValues();
            this.renderTechnique.RenderStep = HeatMapTechniqueStep.Step0;
            renderer.SetEffect(this.renderTechnique);
            this.EnsureRenderTarget(renderer, state);
            renderer.BeginRenderTargetFrame(this.dataRenderTarget, new Color4F?(new Color4F(0.0f, 0.0f, 0.0f, 0.0f)));
            try
            {
                int val2 = this.totalVertexCount;
                for (int i = 0; i < this.valueBuffers.Count; ++i)
                {
                    if (this.timeScaling.TimeEnabled)
                        renderer.SetVertexSource(new VertexBuffer[2]
                        {
                            this.valueBuffers[i],
                            this.timeBuffers[i]
                        });
                    else
                        renderer.SetVertexSource(this.valueBuffers[i]);
                    int num = Math.Min(MaxBufferSize, val2);
                    val2 -= num;
                    if (this.ShowNulls && this.nullSubsets[i] != null)
                        renderer.Draw(this.nullSubsets[i].Item1, this.nullSubsets[i].Item2, PrimitiveTopology.PointList);
                    if (this.ShowNegatives && this.negativeSubsets[i] != null)
                        renderer.Draw(this.negativeSubsets[i].Item1, this.negativeSubsets[i].Item2, PrimitiveTopology.PointList);
                    if (this.ShowZeros && this.zeroSubsets[i] != null)
                        renderer.Draw(this.zeroSubsets[i].Item1, this.zeroSubsets[i].Item2, PrimitiveTopology.PointList);
                    if (this.positiveSubsets[i] != null)
                        renderer.Draw(this.positiveSubsets[i].Item1, this.positiveSubsets[i].Item2, PrimitiveTopology.PointList);
                    if (val2 == 0)
                        break;
                }
            }
            finally
            {
                renderer.EndRenderTargetFrame();
            }
        }

        private void EnsureRenderTarget(Renderer renderer, SceneState state)
        {
            int width = (int)state.ScreenWidth / 2;
            int height = (int)state.ScreenHeight / 2;
            if (this.dataRenderTargetTexture != null && (this.dataRenderTargetTexture.Width != width || this.dataRenderTargetTexture.Height != height))
            {
                this.dataRenderTarget.Dispose();
                this.dataRenderTarget = null;
                this.dataRenderTargetTexture.Dispose();
                this.dataRenderTargetTexture = null;
                this.dataRenderTargetSecondary.Dispose();
                this.dataRenderTargetSecondary = null;
                this.dataRenderTargetTextureSecondary.Dispose();
                this.dataRenderTargetTextureSecondary = null;
            }
            if (this.dataRenderTargetTexture != null)
                return;
            using (Image textureData = new Image(IntPtr.Zero, width, height, PixelFormat.Float32Bpp))
                this.dataRenderTargetTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
            if (this.dataRenderTargetTexture != null)
                this.dataRenderTarget = RenderTarget.Create(this.dataRenderTargetTexture, RenderTargetDepthStencilMode.None);
            using (Image textureData = new Image(IntPtr.Zero, width, height, PixelFormat.Float32Bpp))
                this.dataRenderTargetTextureSecondary = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
            if (this.dataRenderTargetTextureSecondary == null)
                return;
            this.dataRenderTargetSecondary = RenderTarget.Create(this.dataRenderTargetTextureSecondary, RenderTargetDepthStencilMode.None);
        }

        private void DrawGaussianBlurStep(Renderer renderer, SceneState state)
        {
            this.renderTechnique.RenderStep = HeatMapTechniqueStep.Step1Part1;
            renderer.SetEffect(this.renderTechnique);
            renderer.SetTexture(0, this.dataRenderTargetTexture);
            renderer.SetVertexSource(this.screenQuad);
            renderer.BeginRenderTargetFrame(this.dataRenderTargetSecondary, new Color4F?());
            try
            {
                renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
            }
            finally
            {
                renderer.EndRenderTargetFrame();
            }
            this.renderTechnique.RenderStep = HeatMapTechniqueStep.Step1Part2;
            renderer.SetEffect(this.renderTechnique);
            renderer.SetTexture(0, this.dataRenderTargetTextureSecondary);
            renderer.BeginRenderTargetFrame(this.dataRenderTarget, new Color4F?());
            try
            {
                renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
            }
            finally
            {
                renderer.EndRenderTargetFrame();
            }
        }

        private void DrawColorStep(Renderer renderer, SceneState state)
        {
            this.SetMinMaxValues();
            this.renderTechnique.RenderStep = HeatMapTechniqueStep.Step2;
            renderer.SetEffect(this.renderTechnique);
            renderer.SetTexture(0, this.dataRenderTargetTexture);
            renderer.SetTexture(1, this.rampTexture);
            renderer.SetVertexSource(this.screenQuad);
            renderer.Draw(0, 4, PrimitiveTopology.TriangleStrip);
        }

        private void SetMinMaxValues()
        {
            float num1 = this.ShowNegatives ? this.minHeatValue : (this.ShowZeros || this.ShowNulls ? 0.0f : this.minPositiveHeatValue);
            float num2 = !this.ShowNegatives ? this.maxHeatValue : 1f;
            this.renderTechnique.MinHeatValue = num1 * this.MaxHeatFactor * num2;
            this.renderTechnique.MaxHeatValue = this.maxHeatValue * this.MaxHeatFactor * num2;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            foreach (DisposableResource valueBuffer in this.valueBuffers)
                valueBuffer.Dispose();
            this.valueBuffers = null;
            foreach (DisposableResource timeBuffer in this.timeBuffers)
                timeBuffer.Dispose();
            this.timeBuffers = null;
            DisposableResource[] disposableResourceArray = new DisposableResource[7]
            {
                this.screenQuad,
                this.renderTechnique,
                this.dataRenderTargetTexture,
                this.dataRenderTarget,
                this.dataRenderTargetTextureSecondary,
                this.dataRenderTargetSecondary,
                this.rampTexture
            };
            foreach (DisposableResource res in disposableResourceArray)
            {
                if (res != null)
                    res.Dispose();
            }
        }

        private class VertexComparer : IComparer<HeatMapVertex>
        {
            public int Compare(HeatMapVertex a, HeatMapVertex b)
            {
                if (a.Value == b.Value)
                    return 0;
                if (a.Value < b.Value)
                    return -1;
                if (a.Value > b.Value)
                    return 1;
                if (a.Value != b.Value)
                {
                    if (!float.IsNaN(a.Value))
                        return -1;
                    if (!float.IsNaN(b.Value))
                        return 1;
                }
                return 0;
            }
        }
    }
}
