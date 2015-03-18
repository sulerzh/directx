using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    internal class HitTestStep : EngineStep, IHitTestManager
    {
        private Color4F noHitColor = new Color4F(0.0f, 0.0f, 0.0f, 0.0f);
        private Dictionary<int, IHitTestable> hitTestables = new Dictionary<int, IHitTestable>();
        private List<int> hitTestableIds = new List<int>();
        private Dictionary<int, Queue<Tuple<int, object>>> hitTestableContext = new Dictionary<int, Queue<Tuple<int, object>>>();
        private object hitTestablesLock = new object();
        private List<Tuple<int, SceneState, object>> hitTestFailList = new List<Tuple<int, SceneState, object>>();
        private Vector3F currentHoveredObjectPosition = Vector3F.Empty;
        private const int HitTestTargetWidth = 800;
        private const int HitTestTargetHeight = 600;
        private const int HitTestAreaOfInterestWidth = 7;
        private const int HitTestAreaOfInterestHeight = 7;
        private Texture hitTestTexture;
        private RenderTarget hitTestTarget;
        private ReadableBitmap hitTestResultBitmap;
        private Effect hitTestEffect;
        private DepthStencilState hitTestDepthStencil;
        private BlendState hitTestBlend;
        private RasterizerState hitTestRasterizer;
        private FullScreenQuadViewer debugViewer;

        public HitTestStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher)
            : base(dispatcher, eventDispatcher)
        {
            this.hitTestResultBitmap = ReadableBitmap.Create(HitTestAreaOfInterestWidth, HitTestAreaOfInterestHeight, PixelFormat.Rgba32Bpp);
            this.hitTestDepthStencil = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                DepthEnable = true,
                DepthFunction = ComparisonFunction.Less,
                DepthWriteEnable = true,
                StencilEnable = false
            });
            this.hitTestBlend = BlendState.Create(new BlendStateDescription()
            {
                BlendEnable = false
            });
            this.hitTestRasterizer = RasterizerState.Create(new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
                AntialiasedLineEnable = false,
                MultisampleEnable = false,
                ScissorEnable = true
            });
            this.hitTestEffect = Effect.Create(new EffectDefinition()
            {
                VertexFormat = VertexFormats.PositionColorSeparateStreams,
                VertexShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.HitTest.vs"),
                PixelShaderData =
                    this.GetType()
                        .Assembly.GetManifestResourceStream(
                            "Microsoft.Data.Visualization.Engine.Shaders.Compiled.HitTest.ps"),
                Parameters = null,
                Samplers = null
            });
            for (int i = 31; i > 1; --i)
                this.hitTestableIds.Add(i);
            this.debugViewer = new FullScreenQuadViewer();
        }

        public int ReserveHitTestableId(int desiredId)
        {
            lock (this.hitTestablesLock)
            {
                if (this.hitTestableIds.Count > 0)
                {
                    if (this.hitTestableIds.Contains(desiredId))
                    {
                        this.hitTestableIds.Remove(desiredId);
                        return desiredId;
                    }
                    int id = this.hitTestableIds[0];
                    this.hitTestableIds.RemoveAt(0);
                    return id;
                }
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Over {0} hit-testable layers have been added to the visualization engine. Only {0} layers are supported.", (object)31);
            return 0;
        }

        public bool AddHitTestable(IHitTestable hitTestable)
        {
            if (hitTestable == null)
                return false;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Adding hit testable: {0}.", (object)hitTestable.Id);
            lock (this.hitTestablesLock)
            {
                if (this.hitTestables.ContainsKey(hitTestable.Id))
                    return false;
                this.hitTestables.Add(hitTestable.Id, hitTestable);
                return true;
            }
        }

        public bool RemoveHitTestable(int id)
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Removing hit testable: {0}.", (object)id);
            lock (this.hitTestablesLock)
            {
                this.hitTestableIds.Add(id);
                return this.hitTestables.Remove(id);
            }
        }

        internal override bool PreExecute(SceneState state, int phase)
        {
            state.HoveredObjectPosition = this.currentHoveredObjectPosition;
            return false;
        }

        internal override void Execute(Renderer renderer, SceneState state, int phase)
        {
            try
            {
                renderer.Profiler.BeginSection("[Hit Testing]");
                if (state.LastValidCursorPosition.HasValue && this.HitTestRequested())
                {
                    if (this.hitTestTarget == null)
                    {
                        using (Image textureData = new Image(IntPtr.Zero, HitTestTargetWidth, HitTestTargetHeight, PixelFormat.Rgba32Bpp))
                            this.hitTestTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                        this.hitTestTarget = RenderTarget.Create(this.hitTestTexture, RenderTargetDepthStencilMode.Enabled);
                    }
                    renderer.Profiler.BeginSection("[Hit Testing] Render to hit-test buffer");
                    this.RenderHitTest(renderer, state);
                    renderer.Profiler.EndSection();
                }
                renderer.Profiler.BeginSection("[Hit Testing] Process Intersection Data");
                if (this.AwaitingHitTestResults())
                    this.ProcessIntersectionData(state);
                renderer.Profiler.EndSection();
                renderer.Profiler.EndSection();
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail("Exception while executing hit testing step: {0}.", ex);
            }
        }

        private bool AwaitingHitTestResults()
        {
            foreach (Queue<Tuple<int, object>> result in this.hitTestableContext.Values)
            {
                if (result.Count > 0)
                    return true;
            }
            return false;
        }

        private bool HitTestRequested()
        {
            foreach (IHitTestable hitTestable in this.hitTestables.Values)
            {
                if (hitTestable.HitTestRequested)
                    return true;
            }
            return false;
        }

        private void RenderHitTest(Renderer renderer, SceneState state)
        {
            float num1 = HitTestTargetWidth / (float)state.ScreenWidth;
            float num2 = HitTestTargetHeight / (float)state.ScreenHeight;
            Point point = new Point(state.LastValidCursorPosition.Value.X * num1, state.LastValidCursorPosition.Value.Y * num2);
            if (point.X > this.hitTestTarget.RenderTargetTexture.Width || 
                point.Y > this.hitTestTarget.RenderTargetTexture.Height || 
                (point.X < 0.0 || point.Y < 0.0))
            {
                this.hitTestTarget.ScissorRect = new Microsoft.Data.Visualization.Engine.Graphics.Rect(0, 0, 0, 0);
            }
            else
            {
                this.hitTestTarget.ScissorRect = new Microsoft.Data.Visualization.Engine.Graphics.Rect((int)point.X - 3, (int)point.Y - 3, HitTestAreaOfInterestWidth, HitTestAreaOfInterestHeight);
                renderer.BeginRenderTargetFrame(this.hitTestTarget, new Color4F?(this.noHitColor));
                try
                {
                    lock (this.hitTestablesLock)
                    {
                        for (int i = 0; i < 2; ++i)
                        {
                            foreach (IHitTestable target in this.hitTestables.Values)
                            {
                                if (target.HitTestRequested && 
                                    (i != 0 || !target.RenderInFront) && 
                                    (i != 1 || target.RenderInFront))
                                {
                                    object status = target.DrawHitTest(renderer, state, this.hitTestDepthStencil, this.hitTestBlend, this.hitTestRasterizer, this);
                                    if (!this.hitTestableContext.ContainsKey(target.Id))
                                        this.hitTestableContext[target.Id] = new Queue<Tuple<int, object>>();
                                    int frameCount = renderer.FrameCount;
                                    this.hitTestableContext[target.Id].Enqueue(new Tuple<int, object>(frameCount, status));
                                    target.HitTestRequested = false;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    renderer.EndRenderTargetFrame();
                }
                renderer.CopyRenderTargetData(this.hitTestTarget, this.hitTestTarget.ScissorRect, this.hitTestResultBitmap);
            }
        }

        private unsafe void ProcessIntersectionData(SceneState state)
        {
            int sourceFrame;
            int pitch;
            IntPtr num1 = this.hitTestResultBitmap.LockData(out sourceFrame, out pitch);
            if (!(num1 != IntPtr.Zero))
                return;
            uint id = 0U;
            try
            {
                int col = (this.hitTestResultBitmap.Width - 1) / 2;
                int row = (this.hitTestResultBitmap.Height - 1) / 2;
                int num4 = pitch / 4;
                uint* pData = (uint*)num1.ToPointer();
                id = pData[row * num4 + col];
                if (id == 0 && (this.hitTestResultBitmap.Width > 1 || this.hitTestResultBitmap.Height > 1))
                {
                    for (int i = 0; i < this.hitTestResultBitmap.Height; ++i)
                    {
                        for (int j = 0; j < this.hitTestResultBitmap.Width; ++j)
                        {
                            uint num5 = pData[i * num4 + j];
                            if ((int)num5 != 0)
                            {
                                id = num5;
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                this.hitTestResultBitmap.Unlock();
            }
            InstanceId hitId = InstanceId.FromId(id);
            this.hitTestFailList.Clear();
            bool anyHitTestReturnedTrue = false;
            this.currentHoveredObjectPosition = Vector3F.Empty;
            foreach (int key in this.hitTestableContext.Keys)
            {
                while (this.hitTestableContext[key].Count > 0)
                {
                    Tuple<int, object> tuple = this.hitTestableContext[key].Peek();
                    if (tuple.Item1 <= sourceFrame)
                    {
                        this.hitTestableContext[key].Dequeue();
                        if (tuple.Item1 <= sourceFrame)
                        {
                            if (key == hitId.LayerId)
                            {
                                lock (this.hitTestablesLock)
                                {
                                    if (this.hitTestables.ContainsKey(key))
                                    {
                                        Vector3F pos;
                                        anyHitTestReturnedTrue = anyHitTestReturnedTrue | this.hitTestables[key].OnHitTestPass(hitId, state, tuple.Item2, out pos);
                                        if (pos != Vector3F.Empty)
                                            this.currentHoveredObjectPosition = pos;
                                    }
                                }
                            }
                            else
                                this.hitTestFailList.Add(new Tuple<int, SceneState, object>(key, state, tuple.Item2));
                        }
                    }
                    else
                        break;
                }
            }
            lock (this.hitTestablesLock)
            {
                for (int i = 0; i < this.hitTestFailList.Count; ++i)
                {
                    int key = this.hitTestFailList[i].Item1;
                    if (this.hitTestables.ContainsKey(key))
                        this.hitTestables[key].OnHitTestFail(this.hitTestFailList[i].Item2, this.hitTestFailList[i].Item3, anyHitTestReturnedTrue);
                }
            }
        }

        public override void Dispose()
        {
            if (this.hitTestTarget != null)
                this.hitTestTarget.Dispose();
            if (this.hitTestTexture != null)
                this.hitTestTexture.Dispose();
            this.hitTestResultBitmap.Dispose();
            this.hitTestRasterizer.Dispose();
            this.hitTestBlend.Dispose();
            this.hitTestDepthStencil.Dispose();
            this.hitTestEffect.Dispose();
            this.debugViewer.Dispose();
        }
    }
}
