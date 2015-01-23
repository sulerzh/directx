using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal class AnnotationStep : EngineStep, IHitTestable
  {
    private List<IAnnotatable> annotatables = new List<IAnnotatable>();
    private List<IAnnotatable> pending = new List<IAnnotatable>();
    private object annotationLock = new object();
    private const int HitTestId = 1;
    private AnnotationStyle style;
    private bool annotationDirty;

    public bool HitTestRequested { get; set; }

    public int Id
    {
      get
      {
        return 1;
      }
    }

    public bool RenderInFront
    {
      get
      {
        return true;
      }
    }

    public AnnotationStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher)
      : base(dispatcher, eventDispatcher)
    {
    }

    public override void OnInitialized()
    {
      base.OnInitialized();
      this.HitTestManager.AddHitTestable((IHitTestable) this);
    }

    public bool AddAnnotatable(IAnnotatable annotatable)
    {
      lock (this.annotationLock)
      {
        if (this.annotatables.Contains(annotatable) || this.pending.Contains(annotatable))
          return false;
        this.pending.Add(annotatable);
      }
      return true;
    }

    public bool RemoveAnnotatable(IAnnotatable annotatable)
    {
      lock (this.annotationLock)
      {
        if (this.pending.Remove(annotatable))
          return true;
        if (this.annotatables.Contains(annotatable))
        {
          this.annotatables.Remove(annotatable);
          return true;
        }
      }
      return false;
    }

    public void SetStyle(AnnotationStyle annotationStyle)
    {
      if (this.style == null)
      {
        this.style = annotationStyle;
      }
      else
      {
        if (annotationStyle == null)
          return;
        this.style.CopyFrom(annotationStyle);
      }
    }

    internal override bool PreExecute(SceneState state, int phase)
    {
      if (state.VisualTimeFreeze.HasValue && !state.CameraMoved)
        this.HitTestRequested = true;
      if (this.pending.Count > 0)
        return true;
      lock (this.annotationLock)
      {
        for (int local_0 = 0; local_0 < this.annotatables.Count; ++local_0)
        {
          if (this.annotatables[local_0].IsAnnotationDirty)
            return true;
        }
      }
      return false;
    }

    internal override void Execute(Renderer renderer, SceneState state, int phase)
    {
      if (state.IsSkipLayerRelatedSteps)
        return;
      this.Render(renderer, state, false, (DepthStencilState) null, (BlendState) null, (RasterizerState) null, state.OfflineRender);
    }

    private void Render(Renderer renderer, SceneState state, bool hitTestPass, DepthStencilState depthStencil, BlendState blend, RasterizerState rasterizer, bool blockUntilComplete)
    {
      if (this.style == null)
        this.style = AnnotationStyle.MakeDefault();
      lock (this.annotationLock)
      {
        if (this.pending.Count > 0 && hitTestPass)
          return;
        this.AddPendingAnnotatables();
      }
      renderer.ClearCurrentDepthTarget(1f);
      renderer.SetTexture(1, state.GlobeDepth);
      lock (this.annotationLock)
      {
        for (int local_0 = 0; local_0 < this.annotatables.Count; ++local_0)
        {
          this.annotatables[local_0].Style = this.style;
          if (hitTestPass)
            this.annotatables[local_0].DrawAnnotationHitTest(renderer, state, depthStencil, blend, rasterizer);
          else
            this.annotatables[local_0].DrawAnnotation(renderer, state, blockUntilComplete);
        }
      }
    }

    private void AddPendingAnnotatables()
    {
      foreach (IAnnotatable annotatable in this.pending)
        this.annotatables.Add(annotatable);
      this.pending.Clear();
    }

    public object DrawHitTest(Renderer renderer, SceneState state, DepthStencilState depthStencil, BlendState blend, RasterizerState rasterizer, IHitTestManager hitTestManager)
    {
      this.Render(renderer, state, true, depthStencil, blend, rasterizer, false);
      return (object) null;
    }

    bool IHitTestable.OnHitTestPass(InstanceId hitId, SceneState state, object context, out Vector3F objectPos)
    {
      objectPos = Vector3F.Empty;
      return false;
    }

    void IHitTestable.OnHitTestFail(SceneState state, object context, bool anyHitTestReturnedTrue)
    {
    }

    public override void Dispose()
    {
    }
  }
}
