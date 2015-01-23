// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceLayer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Data.Visualization.Engine
{
  public abstract class InstanceLayer : HitTestableLayer, IAnnotatable, IShadowCaster, IDisposable
  {
    private static long IncrementalDataUpdateInternal = 1000L;
    private bool firstDraw = true;
    private bool needsDataUpdate = true;
    private long lastIncrementalUpdateTick = -1L;
    private Dictionary<int, int> colorOverrides = new Dictionary<int, int>();
    private object syncLock = new object();
    private HashSet<InstanceId> annotationIds = new HashSet<InstanceId>();
    private HashSet<InstanceId> pendingAnnotations = new HashSet<InstanceId>();
    private List<InstanceId> invalidatedAnnotationIds = new List<InstanceId>();
    protected int instanceCountUsedForScale;
    internal List<InstanceData> instanceList;
    private Clusters dataClusters;
    private InstanceRenderer instanceRenderer;
    private InstancedVisual.Alignment startingAlignment;
    private bool hasIncrementalData;
    private bool isColorUpdate;
    protected bool scaleUpdateNeeded;
    private bool dataUpdated;
    private bool dataUpdatedSinceClear;
    protected DateTime? minVisualTime;
    protected DateTime? maxVisualTime;
    protected double visualTimeRange;
    private volatile bool displayNullValues;
    private volatile bool displayZeroValues;
    private volatile bool displayNegativeValues;
    private AnnotationCache annotationTextureCache;
    private bool annotationsUpdated;
    private bool annotationDirty;

    protected abstract bool OptimizeSpatialIndex { get; }

    internal virtual float HorizontalSpacing
    {
      get
      {
        return 0.0f;
      }
    }

    protected float Scale { get; set; }

    protected float HeightOffset
    {
      get
      {
        return this.instanceRenderer.HeightOffset;
      }
      set
      {
        this.instanceRenderer.HeightOffset = value;
      }
    }

    protected float ShadowScale
    {
      get
      {
        return this.instanceRenderer.ShadowScale;
      }
      set
      {
        this.instanceRenderer.ShadowScale = value;
      }
    }

    protected float FixedDimension
    {
      get
      {
        return this.instanceRenderer.FixedDimension;
      }
      set
      {
        this.instanceRenderer.FixedDimension = value;
      }
    }

    protected Vector2F VariableScale
    {
      get
      {
        return this.instanceRenderer.VariableScale;
      }
      set
      {
        this.instanceRenderer.VariableScale = value;
      }
    }

    protected Vector2F FixedScale
    {
      get
      {
        return this.instanceRenderer.FixedScale;
      }
      set
      {
        this.instanceRenderer.FixedScale = value;
      }
    }

    protected bool ViewScaleEnabled
    {
      get
      {
        return this.instanceRenderer.ViewScaleEnabled;
      }
      set
      {
        this.instanceRenderer.ViewScaleEnabled = value;
      }
    }

    protected bool UseTextureForNegativeValues
    {
      get
      {
        return this.instanceRenderer.UseTextureForNegativeValues;
      }
      set
      {
        this.instanceRenderer.UseTextureForNegativeValues = value;
      }
    }

    protected bool UseSqrtValue
    {
      get
      {
        return this.instanceRenderer.UseSqrtValue;
      }
      set
      {
        this.instanceRenderer.UseSqrtValue = value;
      }
    }

    protected bool ShowOnlyMaxValueEnabled
    {
      get
      {
        return this.instanceRenderer.ShowOnlyMaxValueEnabled;
      }
      set
      {
        this.instanceRenderer.ShowOnlyMaxValueEnabled = value;
      }
    }

    protected float DesaturateFactor
    {
      get
      {
        return this.instanceRenderer.DesaturateFactor;
      }
      set
      {
        this.instanceRenderer.DesaturateFactor = value;
      }
    }

    protected bool DesaturationEnabled
    {
      get
      {
        return this.instanceRenderer.DesaturationEnabled;
      }
    }

    internal List<InstanceBlock> InstanceBlocks
    {
      get
      {
        if (this.instanceRenderer != null)
          return this.instanceRenderer.InstanceBlocks;
        else
          return (List<InstanceBlock>) null;
      }
    }

    public bool DataIntakeComplete { get; protected set; }

    protected bool ResetSpatialIndex { get; set; }

    protected int InstanceClusterCount { get; private set; }

    protected bool OffsetNegativeValues { get; set; }

    internal InstancedVisual.Alignment Alignment
    {
      get
      {
        return this.instanceRenderer.Alignment;
      }
      set
      {
        if (this.instanceRenderer != null)
          this.instanceRenderer.Alignment = value;
        else
          this.startingAlignment = value;
      }
    }

    public RenderParameters SharedRenderParameters
    {
      get
      {
        return this.instanceRenderer.SharedRenderParameters;
      }
    }

    public RenderParameters ColorRenderParameters
    {
      get
      {
        int count;
        return this.instanceRenderer.ColorManager.GetColors(out count);
      }
    }

    public override bool DisplayNullValues
    {
      get
      {
        return this.displayNullValues;
      }
      set
      {
        this.displayNullValues = value;
        this.UpdateDisplayOptions();
      }
    }

    public override bool DisplayZeroValues
    {
      get
      {
        return this.displayZeroValues;
      }
      set
      {
        this.displayZeroValues = value;
        this.scaleUpdateNeeded = true;
        this.UpdateDisplayOptions();
      }
    }

    public override bool DisplayNegativeValues
    {
      get
      {
        return this.displayNegativeValues;
      }
      set
      {
        this.displayNegativeValues = value;
        this.scaleUpdateNeeded = true;
        this.UpdateDisplayOptions();
      }
    }

    public virtual float AnnotationAnchorHeight
    {
      get
      {
        return 0.0f;
      }
    }

    public override LayerType LayerType
    {
      get
      {
        return base.LayerType;
      }
      protected set
      {
        if (this.LayerType == value)
          return;
        if (this.EngineDispatcher != null)
        {
          this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod) (() =>
          {
            base.LayerType = value;
            this.Scaling.SetCreationTime();
          }));
        }
        else
        {
          base.LayerType = value;
          this.Scaling.SetCreationTime();
        }
      }
    }

    public override sealed int DataCount
    {
      get
      {
        return this.instanceList.Count;
      }
    }

    protected bool IgnoreData { get; private set; }

    internal virtual int BlockSize
    {
      get
      {
        return 4096;
      }
    }

    internal virtual bool InflatesBounds
    {
      get
      {
        return false;
      }
    }

    AnnotationStyle IAnnotatable.Style
    {
      get
      {
        return this.instanceRenderer.AnnotationStyle;
      }
      set
      {
        if (this.instanceRenderer == null)
          return;
        this.instanceRenderer.AnnotationStyle = value;
      }
    }

    bool IAnnotatable.IsAnnotationDirty
    {
      get
      {
        return this.annotationDirty;
      }
    }

    public InstanceLayer(LayerType layerType, IInstanceIdRelationshipProvider idProvider)
      : base(idProvider)
    {
      this.LayerType = layerType;
      this.instanceList = new List<InstanceData>();
      this.dataClusters = new Clusters(this.instanceList);
      this.displayNullValues = false;
      this.displayZeroValues = true;
      this.displayNegativeValues = true;
      this.instanceCountUsedForScale = 0;
      this.Scale = 1f;
    }

    protected abstract void OnUpdate(SceneState state);

    protected abstract bool DrawColor(Renderer renderer, SceneState state, DrawParameters parameters);

    protected abstract bool DrawSelection(Renderer renderer, SceneState state, DrawParameters parameters);

    protected abstract bool DrawShadowVolume(Renderer renderer, SceneState state, DrawParameters parameters);

    protected abstract bool RenderOnPreDraw();

    protected abstract DrawMode GetDrawMode(bool preRender, bool hitTest);

    internal abstract RenderQuery GetSpatialQuery(SceneState state, List<int> queryResult);

    internal abstract double GetMaxInstanceExtent(float scale, int maxCount);

    public override Vector3D GetDataPosition(int bufferPosition, bool flatMap)
    {
      try
      {
        return this.instanceList[bufferPosition].Location.To3D(flatMap);
      }
      catch (ArgumentOutOfRangeException ex)
      {
        return Vector3D.Empty;
      }
    }

    public override Coordinates GetDataLocation(int bufferPosition)
    {
      return this.instanceList[bufferPosition].Location;
    }

    internal bool HasNegativeValues()
    {
      if (this.instanceRenderer != null)
        return this.instanceRenderer.HasNegativeValues();
      else
        return false;
    }

    protected virtual void OverrideDimensionAndScales(InstanceBlockQueryType queryType, ref float dimension, ref Vector2F fixedScale, ref Vector2F variableScale)
    {
    }

    internal virtual void UpdateBounds(InstanceBlock block, ref Box2D latLongBounds, ref Box3D bounds3D)
    {
    }

    private void UpdateDisplayOptions()
    {
      lock (this.syncLock)
        this.annotationsUpdated = true;
      if (this.EngineDispatcher != null)
        this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod) (() =>
        {
          if (this.instanceRenderer == null)
            return;
          this.instanceRenderer.UpdateDisplayOptions(this.displayNullValues, this.displayZeroValues, this.displayNegativeValues);
        }));
      this.IsDirty = true;
    }

    public override void BeginDataInput(int estimate, bool progressiveDataInput, bool ignoreData, CustomSpaceTransform dataCustomSpace, double minInstanceValue, double maxInstanceValue, DateTime? minTime, DateTime? maxTime)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Begin data input, progressive=" + progressiveDataInput.ToString());
      this.IgnoreData = ignoreData;
      lock (this.syncLock)
      {
        this.DataInputInProgress = true;
        this.DataInputCustomSpace = dataCustomSpace;
        this.DataIntakeComplete = false;
        this.InstanceClusterCount = 0;
        this.instanceCountUsedForScale = 0;
        this.ResetSpatialIndex = true;
        this.dataClusters.Reset();
        if (estimate > 0 && (this.instanceList == null || this.instanceList.Capacity < estimate))
        {
          this.instanceList = new List<InstanceData>(estimate);
          this.dataClusters.SetInstanceList(this.instanceList);
        }
        else
          this.instanceList.Clear();
        this.incrementalDataUpdate = progressiveDataInput;
        this.hasIncrementalData = progressiveDataInput;
        this.lastIncrementalUpdateTick = 0L;
      }
      this.minVisualTime = minTime;
      this.maxVisualTime = maxTime;
      this.UpdateMinMaxValues(minInstanceValue, maxInstanceValue);
      if (this.minVisualTime.HasValue && this.maxVisualTime.HasValue)
        this.visualTimeRange = (this.maxVisualTime.Value - this.minVisualTime.Value).TotalMilliseconds;
      else
        this.visualTimeRange = 0.0;
    }

    public override sealed void AddData(double latitude, double longitude, IEnumerable<IInstanceParameter> parameters)
    {
      if (parameters == null || !Enumerable.Any<IInstanceParameter>(parameters))
        return;
      lock (this.syncLock)
      {
        ++this.InstanceClusterCount;
        int local_0 = Enumerable.First<IInstanceParameter>(parameters).ShiftValue;
        int local_1 = 0;
        bool local_2 = true;
        int local_3 = 0;
        foreach (IInstanceParameter item_0 in parameters)
        {
          if (item_0.ShiftValue != local_0)
          {
            local_0 = item_0.ShiftValue;
            ++local_1;
          }
          float local_5 = this.GetLayerClampedValue(item_0.RealNumberValue);
          InstanceData local_6 = new InstanceData()
          {
            Id = item_0.Id,
            Location = this.CoordinatesFromLongLatDegrees(longitude, latitude),
            Color = (short) item_0.ColorValue,
            Value = local_5,
            Shift = (short) local_1,
            SourceShift = (short) item_0.ShiftValue,
            StartTime = item_0.StartTime,
            EndTime = item_0.EndTime,
            FirstInstance = local_2
          };
          local_2 = false;
          this.instanceList.Add(local_6);
          ++local_3;
        }
        if (local_3 > 0)
          this.dataClusters.AddCluster(local_3);
        this.dataUpdated = true;
        if (!this.incrementalDataUpdate)
          return;
        this.IsDirty = true;
      }
    }

    public override sealed void EndDataInput()
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "End data input");
      lock (this.syncLock)
      {
        if (this.EngineDispatcher != null)
        {
          this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod) (() =>
          {
            lock (this.syncLock)
            {
              this.incrementalDataUpdate = false;
              this.needsDataUpdate = true;
              this.dataUpdated = true;
            }
          }));
        }
        else
        {
          this.incrementalDataUpdate = false;
          this.needsDataUpdate = true;
        }
        this.IsDirty = true;
        this.DataInputInProgress = false;
        this.DataInputCustomSpace = (CustomSpaceTransform) null;
      }
    }

    public override void EraseAllData()
    {
      if (this.EngineDispatcher == null)
        return;
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Erasing all data");
      lock (this.syncLock)
      {
        this.dataUpdatedSinceClear = false;
        this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod) (() =>
        {
          bool flag;
          lock (this.syncLock)
          {
            flag = !this.dataUpdatedSinceClear;
            this.dataUpdatedSinceClear = false;
          }
          if (this.instanceRenderer == null || !flag)
            return;
          this.instanceRenderer.ClearInstanceData();
          this.Scale = 1f;
        }));
        this.instanceCountUsedForScale = 0;
        this.ResetSpatialIndex = true;
        this.dataClusters.Reset();
        this.instanceList.Clear();
        this.DataInputInProgress = false;
        this.DataIntakeComplete = false;
        this.InstanceClusterCount = 0;
      }
      this.ClearAnnotations();
      this.SetSelected((IList<InstanceId>) null, true);
    }

    internal override sealed void ResetGraphicsData()
    {
      lock (this.syncLock)
      {
        if (this.instanceRenderer != null)
          this.instanceRenderer.SetHitTestId(this.Id);
        InstanceId[] local_0 = Enumerable.ToArray<InstanceId>((IEnumerable<InstanceId>) this.annotationIds);
        this.annotationIds.Clear();
        foreach (InstanceId item_0 in local_0)
          this.AddAnnotation(new InstanceId((uint) this.Id, item_0));
        this.SetSelected((IList<InstanceId>) null, true);
        this.needsDataUpdate = true;
      }
    }

    public override sealed Vector3F GetInstancePosition(InstanceId id)
    {
      if (this.instanceRenderer != null)
        return this.instanceRenderer.GetInstancePosition(id);
      else
        return Vector3F.Empty;
    }

    public override bool SetColor(int shift, Color4F color)
    {
      lock (this.syncLock)
      {
        int local_0 = 0;
        if (this.colorOverrides.TryGetValue(shift, out local_0))
          this.ColorManager.RemoveColor(local_0);
        int local_1 = this.ColorManager.AddColor(color);
        if (local_1 == 0)
          return false;
        this.colorOverrides[shift] = local_1;
        this.UpdateColors();
      }
      return true;
    }

    private void UpdateColors()
    {
      this.dataUpdated = true;
      this.needsDataUpdate = true;
      this.isColorUpdate = true;
      this.IsDirty = true;
    }

    public override bool ResetColor(int shift)
    {
      lock (this.syncLock)
      {
        int local_0;
        if (this.colorOverrides.TryGetValue(shift, out local_0))
        {
          if (this.ColorManager.RemoveColor(local_0))
          {
            this.colorOverrides.Remove(shift);
            this.UpdateColors();
            return true;
          }
        }
      }
      return false;
    }

    public override bool ResetAllColors()
    {
      lock (this.syncLock)
      {
        foreach (int item_0 in this.colorOverrides.Values)
          this.ColorManager.RemoveColor(item_0);
        this.colorOverrides.Clear();
        this.UpdateColors();
      }
      return true;
    }

    internal override sealed void OnAddLayer()
    {
      base.OnAddLayer();
      this.AddPendingAnnotations();
      this.annotationsUpdated = true;
      this.SetSelected((IList<InstanceId>) null, true);
    }

    private void UpdateScales()
    {
      if (this.instanceRenderer == null)
        return;
      this.instanceRenderer.MaxValue = (float) this.MaxAbsValue;
      this.instanceRenderer.MaxPositiveValue = (float) this.MaxValue;
      this.scaleUpdateNeeded = false;
    }

    private void StoreInstanceData(bool planar)
    {
      this.instanceRenderer.SetInstanceData(this.ResetSpatialIndex, this.instanceList, this.dataClusters, this.colorOverrides, this.OptimizeSpatialIndex, planar, this.minVisualTime, this.maxVisualTime);
      this.ResetSpatialIndex = false;
      this.DataIntakeComplete = !this.DataInputInProgress;
    }

    private void UpdateBuffers(SceneState state)
    {
      bool flag1 = this.scaleUpdateNeeded && !this.DataInputInProgress;
      bool flag2 = this.lastIncrementalUpdateTick == 0L || state.ElapsedMilliseconds - this.lastIncrementalUpdateTick > InstanceLayer.IncrementalDataUpdateInternal;
      if (this.incrementalDataUpdate && flag2 || (this.needsDataUpdate || flag1) || this.OptimizeSpatialIndex && !this.instanceRenderer.IsOptimized)
      {
        lock (this.syncLock)
        {
          bool local_2 = state.FlatteningFactor >= 1.0;
          if (this.needsDataUpdate || flag1 || this.incrementalDataUpdate && flag2)
          {
            if (this.instanceRenderer == null)
            {
              this.instanceRenderer = new InstanceRenderer(this, this.Id, this.Scaling, this.TimeScaling);
              this.instanceRenderer.Alignment = this.startingAlignment;
              this.instanceRenderer.DrawBufferColor = new DrawBufferDelegate(this.DrawColor);
              this.instanceRenderer.DrawBufferSelection = new DrawBufferDelegate(this.DrawSelection);
              this.instanceRenderer.DrawBufferShadow = new DrawBufferDelegate(this.DrawShadowVolume);
              this.instanceRenderer.OverrideDimensionAndScales = new OverrideDimensionAndScalesDelegate(this.OverrideDimensionAndScales);
              this.instanceRenderer.ColorManager = this.ColorManager;
              this.instanceRenderer.UpdateDisplayOptions(this.displayNullValues, this.displayZeroValues, this.displayNegativeValues);
            }
            this.UpdateScales();
            if (this.dataUpdated)
            {
              this.dataUpdated = false;
              this.dataUpdatedSinceClear = true;
              this.StoreInstanceData(local_2);
              if (!this.hasIncrementalData && !this.isColorUpdate)
                this.Scaling.SetCreationTime();
              this.isColorUpdate = false;
            }
            this.lastIncrementalUpdateTick = state.ElapsedMilliseconds;
            this.instanceRenderer.UpdateDisplayOptions(this.displayNullValues, this.displayZeroValues, this.displayNegativeValues);
            this.annotationsUpdated = true;
            this.needsDataUpdate = false;
          }
          else if (this.OptimizeSpatialIndex && !this.instanceRenderer.IsOptimized)
          {
            this.StoreInstanceData(local_2);
            this.annotationsUpdated = true;
          }
          this.OnUpdate(state);
          this.instanceRenderer.ValueOffset = (float) this.GetInstanceValueOffset();
        }
      }
      else
      {
        lock (this.syncLock)
        {
          this.OnUpdate(state);
          this.instanceRenderer.ValueOffset = (float) this.GetInstanceValueOffset();
        }
      }
    }

    protected double GetInstanceValueOffset()
    {
      if (!this.OffsetNegativeValues)
        return 0.0;
      if (this.MinValue < 0.0 && this.DisplayNegativeValues)
        return (double) this.GetLayerClampedValue(-this.MinValue);
      else
        return 0.0001;
    }

    private void UpdateSelectionSubsets()
    {
      if (!this.SelectedItemsUpdated || this.instanceRenderer == null)
        return;
      this.instanceRenderer.SetSelectedItems((IEnumerable<InstanceId>) this.SelectedItems);
      this.SelectedItemsUpdated = false;
    }

    internal override sealed void PreDraw(Renderer renderer, SceneState state, LayerRenderingParameters options)
    {
      if (!this.DrawCommon(state, options) || !this.RenderOnPreDraw() || !this.instanceRenderer.DrawInstances(renderer, state, this.GetDrawMode(true, false), DrawStyle.Color, (DepthStencilState) null, (BlendState) null, (RasterizerState) null))
        return;
      this.IsDirty = true;
    }

    internal override sealed void Draw(Renderer renderer, SceneState state, LayerRenderingParameters options)
    {
      if (!this.DrawCommon(state, options))
        return;
      this.UpdateSelectionSubsets();
      this.instanceRenderer.RenderingOptions = options;
      if (this.instanceRenderer.DrawInstances(renderer, state, this.GetDrawMode(false, false), DrawStyle.Color, (DepthStencilState) null, (BlendState) null, (RasterizerState) null))
        this.IsDirty = true;
      if (!this.annotationsUpdated)
        return;
      lock (this.syncLock)
      {
        if (!this.annotationsUpdated)
          return;
        this.UpdateAnnotations();
        this.annotationsUpdated = false;
      }
    }

    private bool DrawCommon(SceneState state, LayerRenderingParameters options)
    {
      if (this.firstDraw)
      {
        this.firstDraw = false;
        if (this.annotationIds.Count > 0)
        {
          lock (this.syncLock)
          {
            if (this.annotationIds.Count > 0)
            {
              InstanceId[] local_0 = Enumerable.ToArray<InstanceId>((IEnumerable<InstanceId>) this.annotationIds);
              this.annotationIds.Clear();
              for (int local_1 = 0; local_1 < local_0.Length; ++local_1)
                this.annotationIds.Add(new InstanceId((uint) this.Id, local_0[local_1]));
            }
          }
        }
      }
      this.UpdateBuffers(state);
      if (this.instanceRenderer == null)
        return false;
      this.instanceRenderer.UseLogScale = this.UseLogarithmicClampedValue;
      this.instanceRenderer.FixedScaleFactor = this.FixedDimensionScale * (options == null ? 1f : options.InstanceFixedScaleFactor);
      this.instanceRenderer.VariableScaleFactor = this.DataDimensionScale * (options == null ? 1f : options.InstanceVariableScaleFactor);
      this.instanceRenderer.SetDesaturationMode(this.SelectionStyle == SelectionStyle.Saturation);
      this.instanceRenderer.Opacity = this.Opacity;
      return true;
    }

    public override sealed object DrawHitTest(Renderer renderer, SceneState state, DepthStencilState depthStencil, BlendState blend, RasterizerState rasterizer, IHitTestManager hitTestManager)
    {
      if (!this.DrawCommon(state, (LayerRenderingParameters) null))
        return (object) null;
      if (this.instanceRenderer.DrawInstances(renderer, state, this.GetDrawMode(false, true), DrawStyle.HitTest, depthStencil, blend, rasterizer))
        this.IsDirty = true;
      return (object) this.GetSelectionMode();
    }

    public bool DrawShadowVolume(Renderer renderer, SceneState state, DepthStencilState depthStencil, BlendState blend, RasterizerState rasterizer)
    {
      this.UpdateBuffers(state);
      if (this.instanceRenderer == null || (double) this.Opacity <= 0.0)
        return false;
      else
        return this.instanceRenderer.DrawInstances(renderer, state, this.GetDrawMode(false, false), DrawStyle.Shadow, depthStencil, blend, rasterizer);
    }

    public override sealed void GeoSelect(double latitude, double longitude, double distance, bool flatMap)
    {
      Vector3D c = new Coordinates(longitude, latitude).To3D(flatMap);
      List<InstanceId> list = new List<InstanceId>();
      Cap cap = Cap.Construct(c, distance * distance, flatMap);
      for (int index = 1; index < this.instanceList.Count; ++index)
      {
        if (cap.Contains(this.instanceList[index].Location.To3D(flatMap)))
          list.Add(this.instanceList[index].Id);
      }
      this.SetSelected((IList<InstanceId>) list.ToArray(), false);
    }

    public override void Dispose()
    {
      base.Dispose();
      this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod) (() =>
      {
        if (this.instanceRenderer != null)
          this.instanceRenderer.Dispose();
        if (this.annotationTextureCache == null)
          return;
        this.annotationTextureCache.Dispose();
      }));
    }

    public bool AddAnnotation(InstanceId annotationId)
    {
      lock (this.syncLock)
      {
        if (this.Id == 0)
          return this.pendingAnnotations.Add(annotationId);
        bool local_1 = this.annotationIds.Add(new InstanceId((uint) this.Id, annotationId));
        if (local_1)
        {
          this.annotationsUpdated = true;
          this.IsDirty = true;
        }
        return local_1;
      }
    }

    public void SetAnnotation(InstanceId annotationId)
    {
      lock (this.syncLock)
      {
        this.invalidatedAnnotationIds.Add(new InstanceId((uint) this.Id, annotationId));
        this.annotationDirty = true;
      }
    }

    public bool RemoveAnnotation(InstanceId annotationId)
    {
      lock (this.syncLock)
      {
        bool local_0 = this.annotationIds.Remove(new InstanceId((uint) this.Id, annotationId));
        if (local_0)
        {
          this.invalidatedAnnotationIds.Add(new InstanceId((uint) this.Id, annotationId));
          this.annotationsUpdated = true;
          this.IsDirty = true;
        }
        return local_0;
      }
    }

    public void SetAnnotationImageSource(IAnnotationImageSource imageSource)
    {
      if (this.annotationTextureCache == null)
        this.annotationTextureCache = new AnnotationCache(imageSource, this.InstanceIdRelationshipProvider);
      else
        this.annotationTextureCache.SetImageSource(imageSource);
      this.annotationDirty = true;
    }

    private void InvalidateAnnotationImage(InstanceId instanceId)
    {
      this.annotationTextureCache.InvalidateTexture(instanceId);
      this.annotationDirty = true;
    }

    void IAnnotatable.DrawAnnotation(Renderer renderer, SceneState state, bool blockUntilComplete)
    {
      if (this.instanceRenderer == null || this.annotationIds.Count == 0)
      {
        this.annotationDirty = false;
      }
      else
      {
        this.DrawAnnotationCommon(state);
        this.annotationDirty = this.instanceRenderer.DrawAnnotation(renderer, state, this.GetDrawMode(false, false), blockUntilComplete);
      }
    }

    void IAnnotatable.DrawAnnotationHitTest(Renderer renderer, SceneState state, DepthStencilState depthStencil, BlendState blendState, RasterizerState rasterizer)
    {
      if (this.instanceRenderer == null || this.annotationIds.Count == 0)
        return;
      this.DrawAnnotationCommon(state);
      this.instanceRenderer.DrawAnnotationHitTest(renderer, state, this.GetDrawMode(false, true), depthStencil, blendState, rasterizer);
    }

    private void DrawAnnotationCommon(SceneState state)
    {
      this.UpdateBuffers(state);
      IList<InstanceId> updatedAnnotationIds = this.GetUpdatedAnnotationIds();
      if (updatedAnnotationIds != null)
      {
        foreach (InstanceId instance in (IEnumerable<InstanceId>) updatedAnnotationIds)
          this.InvalidateAnnotationImage(new InstanceId(0U, instance));
      }
      this.instanceRenderer.AnnotationTextureCache = this.annotationTextureCache;
    }

    private void ClearAnnotations()
    {
      foreach (InstanceId annotationId in Enumerable.ToArray<InstanceId>((IEnumerable<InstanceId>) this.annotationIds))
        this.RemoveAnnotation(annotationId);
      this.annotationIds.Clear();
    }

    private void AddPendingAnnotations()
    {
      foreach (InstanceId annotationId in this.pendingAnnotations)
        this.AddAnnotation(annotationId);
      this.pendingAnnotations.Clear();
    }

    private void UpdateAnnotations()
    {
      lock (this.syncLock)
        this.instanceRenderer.SetAnnotations((IEnumerable<InstanceId>) this.annotationIds);
    }

    private IList<InstanceId> GetUpdatedAnnotationIds()
    {
      List<InstanceId> list = new List<InstanceId>();
      lock (this.syncLock)
      {
        list.Clear();
        foreach (InstanceId item_0 in this.invalidatedAnnotationIds)
          list.Add(new InstanceId(item_0.LayerId, item_0));
      }
      this.invalidatedAnnotationIds.Clear();
      return (IList<InstanceId>) list;
    }
  }
}
