using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class LayerDataBinding : DataBinding
  {
    private Dictionary<InstanceId, GeoVisualizationInstanceProperties> layerInstanceProperties = new Dictionary<InstanceId, GeoVisualizationInstanceProperties>();
    private Dictionary<int, GeoVisualizationInstanceProperties> layerSeriesProperties = new Dictionary<int, GeoVisualizationInstanceProperties>();
    private object layerLock = new object();
    private const int MaxQueryItemsForSynchronousUpdate = 500;
    private GeoVisualization geoVisualization;
    private CompletionStats completionStatistics;
    private Layer layerThatNeedsDisplayUpdate;
    private CancellationTokenSource cancellationSource;

    public Layer Layer
    {
      get
      {
        lock (this.layerLock)
          return this.VisualElement as Layer;
      }
      internal set
      {
        lock (this.layerLock)
        {
          if (this.Layer != null)
          {
            RegionLayer local_0 = this.Layer as RegionLayer;
            if (local_0 != null)
            {
              local_0.OnShadingScaleChanged -= new EventHandler<RegionScaleEventArgs>(this.RegionLayerScalesChanged);
              this.UnsubscribeFromRegionCompletionEvents(local_0);
            }
            HeatMapLayer local_1 = this.Layer as HeatMapLayer;
            if (local_1 != null)
              local_1.OnScaleChanged -= new EventHandler<HeatMapScaleEventArgs>(this.HeatMapLayerScaleChanged);
            this.ClearDisplay();
            this.Layer.Dispose();
          }
          this.VisualElement = (object) value;
          Interlocked.Exchange<Layer>(ref this.layerThatNeedsDisplayUpdate, value);
          InstanceLayer local_2 = value as InstanceLayer;
          if (local_2 != null)
            local_2.SetAnnotationImageSource((IAnnotationImageSource) new AnnotationImageSource(this, this.AnnotationRenderingThread));
          RegionLayer local_3 = value as RegionLayer;
          if (local_3 != null)
            local_3.OnShadingScaleChanged += new EventHandler<RegionScaleEventArgs>(this.RegionLayerScalesChanged);
          HeatMapLayer local_4 = value as HeatMapLayer;
          if (local_4 == null)
            return;
          local_4.OnScaleChanged += new EventHandler<HeatMapScaleEventArgs>(this.HeatMapLayerScaleChanged);
        }
      }
    }

    public CompletionStats CompletionStats
    {
      get
      {
        lock (this.SourceDataVersionLock)
          return this.completionStatistics;
      }
      internal set
      {
        lock (this.SourceDataVersionLock)
        {
          if (this.completionStatistics != null && !object.ReferenceEquals((object) this.completionStatistics, (object) value))
            this.completionStatistics.Shutdown();
          this.completionStatistics = value;
        }
      }
    }

    public ILatLonProvider LatLonProvider { get; private set; }

    public GeoAmbiguity[] Ambiguities { get; private set; }

    protected double HeatMapScaleMinValue { get; private set; }

    protected double HeatMapScaleMaxValue { get; private set; }

    protected double[] RegionScaleMinValues { get; private set; }

    protected double[] RegionScaleMaxValues { get; private set; }

    protected double[] Lat { get; private set; }

    protected double[] Lon { get; private set; }

    protected BitArray AddedToEngine { get; private set; }

    protected bool AllDataAddedToEngine
    {
      get
      {
        BitArray addedToEngine = this.AddedToEngine;
        if (addedToEngine == null)
          return true;
        lock (addedToEngine)
        {
          foreach (bool item_0 in addedToEngine)
          {
            if (!item_0)
              return false;
          }
        }
        return true;
      }
    }

    public GeoDataView GeoDataView
    {
      get
      {
        return this.DataView as GeoDataView;
      }
    }

    internal List<int> ColorIndices { get; private set; }

    private Thread AnnotationRenderingThread { get; set; }

    private CancellationTokenSource CancellationSource
    {
      get
      {
        return this.cancellationSource;
      }
      set
      {
        this.cancellationSource = value;
      }
    }

    public LayerDataBinding(GeoVisualization geoVisualization, Layer layer, DataView dataView, ILatLonProvider latLonProvider, Thread annotationRenderingThread, List<int> colors)
      : base((object) layer, dataView)
    {
      if (latLonProvider == null)
        throw new ArgumentNullException("latLonProvider");
      if (annotationRenderingThread == null)
        throw new ArgumentNullException("annotationRenderingThread");
      if (colors == null)
        throw new ArgumentNullException("colors");
      this.geoVisualization = geoVisualization;
      this.LatLonProvider = latLonProvider;
      this.AnnotationRenderingThread = annotationRenderingThread;
      this.ColorIndices = colors;
      this.Lat = (double[]) null;
      this.Lon = (double[]) null;
      this.Ambiguities = (GeoAmbiguity[]) null;
      this.AddedToEngine = (BitArray) null;
      this.CompletionStats = (CompletionStats) null;
      this.CancellationSource = (CancellationTokenSource) null;
      this.layerThatNeedsDisplayUpdate = (Layer) null;
      this.RegionScaleMinValues = (double[]) null;
      this.RegionScaleMaxValues = (double[]) null;
      this.HeatMapScaleMinValue = double.NaN;
      this.HeatMapScaleMaxValue = double.NaN;
    }

    public bool GetAnyMeasure()
    {
      GeoVisualization geoVisualization = this.geoVisualization;
      if (geoVisualization != null)
        return geoVisualization.GetDisplayAnyMeasure();
      else
        return false;
    }

    public Color4F? ColorOverrideForSeriesIndex(int seriesIndex, out bool overridden)
    {
      Dictionary<int, GeoVisualizationInstanceProperties> dictionary = this.layerSeriesProperties;
      overridden = false;
      if (dictionary != null)
      {
        lock (dictionary)
        {
          GeoVisualizationInstanceProperties local_1;
          if (dictionary.TryGetValue(seriesIndex, out local_1))
          {
            if (local_1.ColorSet)
            {
              overridden = true;
              return new Color4F?(local_1.Color);
            }
          }
        }
      }
      return new Color4F?();
    }

    public Color4F? ColorForSeriesIndex(int seriesIndex, int colorIndex, out bool overridden)
    {
      Color4F? nullable = this.ColorOverrideForSeriesIndex(seriesIndex, out overridden);
      if (nullable.HasValue)
        return nullable;
      List<int> colorIndices = this.ColorIndices;
      GeoVisualization geoVisualization = this.geoVisualization;
      VisualizationModel visualizationModel = geoVisualization == null ? (VisualizationModel) null : geoVisualization.VisualizationModel;
      if (colorIndices == null || visualizationModel == null)
        return new Color4F?();
      lock (colorIndices)
      {
        if (colorIndex < 0 || colorIndex >= colorIndices.Count)
          return new Color4F?();
        else
          return new Color4F?(visualizationModel.ColorSelector.GetColor(colorIndices[colorIndex]));
      }
    }

    public Color4F? LayerColorForCurrentTheme()
    {
      List<int> colorIndices = this.ColorIndices;
      GeoVisualization geoVisualization = this.geoVisualization;
      VisualizationModel visualizationModel = geoVisualization == null ? (VisualizationModel) null : geoVisualization.VisualizationModel;
      if (colorIndices == null || visualizationModel == null)
        return new Color4F?();
      lock (colorIndices)
      {
        if (colorIndices.Count == 0)
          return new Color4F?();
        else
          return new Color4F?(visualizationModel.ColorSelector.GetColor(colorIndices[0]));
      }
    }

    public void RegionChartBoundsForSeriesIndex(int seriesIndex, out double? min, out double? max)
    {
        min = null;
        max = null;

        double[] regionScaleMinValues = this.RegionScaleMinValues;
        double[] regionScaleMaxValues = this.RegionScaleMaxValues;
        if (regionScaleMinValues == null || regionScaleMaxValues == null || (seriesIndex >= regionScaleMinValues.Length || seriesIndex >= regionScaleMaxValues.Length))
          return;
        min = new double?(regionScaleMinValues[seriesIndex]);
        if (double.IsNaN(min.Value))
          min = new double?();
        max = new double?(regionScaleMaxValues[seriesIndex]);
        if (!double.IsNaN(max.Value))
          return;
        max = new double?();
    }

    public List<GeoAmbiguity> GetGeoAmbiguities(out float confidencePercentage)
    {
      GeoAmbiguity[] ambiguities = this.Ambiguities;
      if (ambiguities == null)
      {
        confidencePercentage = 1f;
        return (List<GeoAmbiguity>) null;
      }
      else
      {
        float num1 = (float) Enumerable.Count<GeoAmbiguity>(Enumerable.Where<GeoAmbiguity>((IEnumerable<GeoAmbiguity>) ambiguities, (Func<GeoAmbiguity, bool>) (ambiguity => ambiguity != null)));
        float num2 = (float) ((double) Enumerable.Count<GeoAmbiguity>(Enumerable.Where<GeoAmbiguity>((IEnumerable<GeoAmbiguity>) ambiguities, (Func<GeoAmbiguity, bool>) (ambiguity =>
        {
          if (ambiguity == null)
            return false;
          if (ambiguity.ResolutionType != GeoAmbiguity.Resolution.SingleMatchHighConf && ambiguity.ResolutionType != GeoAmbiguity.Resolution.TopCountCountryClosestMatch)
            return ambiguity.ResolutionType == GeoAmbiguity.Resolution.TopCountCountrySingleMatch;
          else
            return true;
        }))) + (double) Enumerable.Count<GeoAmbiguity>(Enumerable.Where<GeoAmbiguity>((IEnumerable<GeoAmbiguity>) ambiguities, (Func<GeoAmbiguity, bool>) (ambiguity =>
        {
          if (ambiguity != null)
            return ambiguity.ResolutionType == GeoAmbiguity.Resolution.EntityTypeAndValueMatch;
          else
            return false;
        }))) * 0.899999976158142 + (double) Enumerable.Count<GeoAmbiguity>(Enumerable.Where<GeoAmbiguity>((IEnumerable<GeoAmbiguity>) ambiguities, (Func<GeoAmbiguity, bool>) (ambiguity =>
        {
          if (ambiguity != null)
            return ambiguity.ResolutionType == GeoAmbiguity.Resolution.EntityTypeMatch;
          else
            return false;
        }))) * 0.75);
        confidencePercentage = (double) num1 == 0.0 ? 1f : num2 / num1;
        return Enumerable.ToList<GeoAmbiguity>(Enumerable.Where<GeoAmbiguity>((IEnumerable<GeoAmbiguity>) ambiguities, (Func<GeoAmbiguity, bool>) (ambiguity =>
        {
          if (ambiguity != null)
            return ambiguity.ResolutionType != GeoAmbiguity.Resolution.SingleMatchHighConf;
          else
            return false;
        })));
      }
    }

    public void UpdateInstanceAnnotation(InstanceId id, GeoVisualizationInstanceProperties properties)
    {
      InstanceLayer instanceLayer = this.Layer as InstanceLayer;
      GeoDataView geoDataView = this.GeoDataView;
      bool flag = instanceLayer is RegionLayer;
      if (instanceLayer == null || geoDataView == null)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "UpdateInstanceAnnotation called when this.Layer {0}, GeoDataView {1} returning", this.Layer == null ? (object) "is null" : (object) " is not an InstancedVisualLayer", geoDataView == null ? (object) "is null" : (object) "is not null");
      }
      else
      {
        InstanceId?[] nullableArray = (InstanceId?[]) null;
        InstanceId? nullable;
        if (!flag)
        {
          nullable = geoDataView.GetCanonicalInstanceId(this.LastSourceDataVersion, new InstanceId(id.ElementId));
          if (!nullable.HasValue)
            return;
        }
        else
        {
          nullableArray = geoDataView.GetCanonicalInstanceIdsForAllSeries(this.LastSourceDataVersion, new InstanceId(id.ElementId));
          if (nullableArray == null || Enumerable.All<InstanceId?>((IEnumerable<InstanceId?>) nullableArray, (Func<InstanceId?, bool>) (instId => !instId.HasValue)))
            return;
          nullable = new InstanceId?(Enumerable.First<InstanceId?>((IEnumerable<InstanceId?>) nullableArray, (Func<InstanceId?, bool>) (instId => instId.HasValue)).Value);
        }
        Dictionary<InstanceId, GeoVisualizationInstanceProperties> dictionary = this.layerInstanceProperties;
        if (dictionary == null)
          return;
        InstanceId index = nullable.Value;
        lock (dictionary)
        {
          GeoVisualizationInstanceProperties local_7;
          if (dictionary.TryGetValue(index, out local_7))
          {
            bool local_8 = local_7.Annotation == null;
            dictionary[index] = properties;
            if (properties.Annotation != null)
            {
              if (local_8)
              {
                if (nullableArray == null)
                {
                  instanceLayer.AddAnnotation(index);
                }
                else
                {
                  foreach (InstanceId? item_0 in nullableArray)
                  {
                    if (item_0.HasValue)
                      instanceLayer.AddAnnotation(item_0.Value);
                  }
                }
              }
              else if (nullableArray == null)
              {
                instanceLayer.SetAnnotation(index);
              }
              else
              {
                foreach (InstanceId? item_1 in nullableArray)
                {
                  if (item_1.HasValue)
                    instanceLayer.SetAnnotation(item_1.Value);
                }
              }
            }
            else if (nullableArray == null)
            {
              instanceLayer.RemoveAnnotation(index);
            }
            else
            {
              foreach (InstanceId? item_2 in nullableArray)
              {
                if (item_2.HasValue)
                  instanceLayer.RemoveAnnotation(item_2.Value);
              }
            }
          }
          else
          {
            dictionary.Add(index, properties);
            if (nullableArray == null)
            {
              instanceLayer.AddAnnotation(index);
            }
            else
            {
              foreach (InstanceId? item_3 in nullableArray)
              {
                if (item_3.HasValue)
                  instanceLayer.AddAnnotation(item_3.Value);
              }
            }
          }
        }
      }
    }

    public void UpdateSeriesProperties(int seriesIndex, GeoVisualizationInstanceProperties properties, bool notifyColorsChanged)
    {
      InstanceLayer layer = this.Layer as InstanceLayer;
      if (layer == null)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "UpdateSeriesProperties called when this.Layer {0}, returning", this.Layer == null ? (object) "is null" : (object) " is not an InstancedVisualLayer");
      }
      else
      {
        bool colorsChanged;
        this.SetSeriesProperties(layer, properties, seriesIndex, out colorsChanged);
        if (!colorsChanged || !notifyColorsChanged)
          return;
        GeoVisualization geoVisualization = this.geoVisualization;
        VisualizationModel visualizationModel = geoVisualization == null ? (VisualizationModel) null : geoVisualization.VisualizationModel;
        if (visualizationModel == null)
          return;
        visualizationModel.ColorSelector.NotifyColorsChanged();
      }
    }

    public bool HasSeriesColorOverrides()
    {
      Dictionary<int, GeoVisualizationInstanceProperties> dictionary = this.layerSeriesProperties;
      if (dictionary == null)
        return false;
      lock (dictionary)
        return Enumerable.Any<GeoVisualizationInstanceProperties>((IEnumerable<GeoVisualizationInstanceProperties>) this.layerInstanceProperties.Values, (Func<GeoVisualizationInstanceProperties, bool>) (properties => properties.ColorSet));
    }

    public void UpdateLayerColorOverride(int sourceDataVersion = 0)
    {
      GeoVisualization geoVisualization1 = this.geoVisualization;
      Layer layer = this.Layer;
      GeoDataView geoDataView = this.GeoDataView;
      if (geoVisualization1 == null || layer == null || geoDataView == null)
        return;
      int sourceDataVersion1 = sourceDataVersion == 0 ? this.LastDisplayedSourceDataVersion : sourceDataVersion;
      if (geoDataView.QueryResultsHaveCategory(sourceDataVersion1) || geoDataView.QueryResultsHaveMeasures(sourceDataVersion1))
        return;
      Color4F? layerColorOverride = geoVisualization1.LayerColorOverride;
      if (layerColorOverride.HasValue)
        layer.SetColor(0, layerColorOverride.Value);
      else
        layer.ResetColor(0);
      GeoVisualization geoVisualization2 = this.geoVisualization;
      VisualizationModel visualizationModel = geoVisualization2 == null ? (VisualizationModel) null : geoVisualization2.VisualizationModel;
      if (visualizationModel == null)
        return;
      visualizationModel.ColorSelector.NotifyColorsChanged();
    }

    public AnnotationTemplateModel GetAnnotation(InstanceId id)
    {
      InstanceId instanceId = new InstanceId(id.ElementId);
      GeoDataView geoDataView = this.GeoDataView;
      bool flag = this.Layer is RegionLayer;
      if (geoDataView == null)
        return (AnnotationTemplateModel) null;
      InstanceId? nullable;
      if (flag)
      {
        InstanceId?[] instanceIdsForAllSeries = geoDataView.GetCanonicalInstanceIdsForAllSeries(this.LastSourceDataVersion, new InstanceId(id.ElementId));
        nullable = instanceIdsForAllSeries != null ? Enumerable.FirstOrDefault<InstanceId?>((IEnumerable<InstanceId?>) instanceIdsForAllSeries, (Func<InstanceId?, bool>) (instId => instId.HasValue)) : new InstanceId?();
      }
      else
        nullable = geoDataView.GetCanonicalInstanceId(this.LastSourceDataVersion, new InstanceId(id.ElementId));
      if (!nullable.HasValue)
        return (AnnotationTemplateModel) null;
      InstanceId key = nullable.Value;
      Dictionary<InstanceId, GeoVisualizationInstanceProperties> dictionary = this.layerInstanceProperties;
      if (dictionary == null)
        return (AnnotationTemplateModel) null;
      lock (dictionary)
      {
        GeoVisualizationInstanceProperties local_0;
        if (dictionary.TryGetValue(key, out local_0))
          return local_0.Annotation;
        else
          return (AnnotationTemplateModel) null;
      }
    }

    internal override void ClearVisualElement()
    {
      this.Layer = (Layer) null;
      base.ClearVisualElement();
    }

    internal override void Shutdown()
    {
      this.geoVisualization = (GeoVisualization) null;
      this.LatLonProvider = (ILatLonProvider) null;
      this.AnnotationRenderingThread = (Thread) null;
      List<int> colorIndices = this.ColorIndices;
      if (colorIndices != null)
      {
        lock (colorIndices)
          this.ColorIndices = (List<int>) null;
      }
      this.Lat = (double[]) null;
      this.Lon = (double[]) null;
      this.Ambiguities = (GeoAmbiguity[]) null;
      this.AddedToEngine = (BitArray) null;
      this.RegionScaleMinValues = (double[]) null;
      this.RegionScaleMaxValues = (double[]) null;
      this.HeatMapScaleMinValue = double.NaN;
      this.HeatMapScaleMaxValue = double.NaN;
      lock (this.SourceDataVersionLock)
      {
        if (this.CompletionStats != null)
        {
          this.CompletionStats.Shutdown();
          this.CompletionStats = (CompletionStats) null;
        }
      }
      this.CancellationSource = (CancellationTokenSource) null;
      if (this.layerInstanceProperties != null)
      {
        lock (this.layerInstanceProperties)
        {
          foreach (GeoVisualizationInstanceProperties item_0 in this.layerInstanceProperties.Values)
            item_0.Shutdown();
          this.layerInstanceProperties.Clear();
          this.layerInstanceProperties = (Dictionary<InstanceId, GeoVisualizationInstanceProperties>) null;
        }
      }
      if (this.layerSeriesProperties != null)
      {
        lock (this.layerSeriesProperties)
        {
          foreach (GeoVisualizationInstanceProperties item_1 in this.layerSeriesProperties.Values)
            item_1.Shutdown();
          this.layerSeriesProperties.Clear();
          this.layerSeriesProperties = (Dictionary<int, GeoVisualizationInstanceProperties>) null;
        }
      }
      Interlocked.Exchange<Layer>(ref this.layerThatNeedsDisplayUpdate, (Layer) null);
      base.Shutdown();
    }

    protected override void ClearDisplay()
    {
      Layer layer = this.Layer;
      if (layer == null)
        return;
      this.UnsubscribeFromRegionCompletionEvents(layer as RegionLayer);
      layer.EraseAllData();
      layer.ResetAllColors();
    }

    protected override void ClearCachedData()
    {
      CancellationTokenSource cancellationTokenSource = Interlocked.Exchange<CancellationTokenSource>(ref this.cancellationSource, (CancellationTokenSource) null);
      if (cancellationTokenSource != null)
      {
        try
        {
          cancellationTokenSource.Cancel();
        }
        catch (ObjectDisposedException ex)
        {
        }
      }
      this.Lat = (double[]) null;
      this.Lon = (double[]) null;
      this.Ambiguities = (GeoAmbiguity[]) null;
      this.AddedToEngine = (BitArray) null;
      this.RegionScaleMinValues = (double[]) null;
      this.RegionScaleMaxValues = (double[]) null;
      this.HeatMapScaleMinValue = double.NaN;
      this.HeatMapScaleMaxValue = double.NaN;
      Dictionary<InstanceId, GeoVisualizationInstanceProperties> dictionary1 = this.layerInstanceProperties;
      if (dictionary1 != null)
      {
        lock (dictionary1)
          dictionary1.Clear();
      }
      Dictionary<int, GeoVisualizationInstanceProperties> dictionary2 = this.layerSeriesProperties;
      if (dictionary2 != null)
      {
        lock (dictionary2)
          dictionary2.Clear();
      }
      base.ClearCachedData();
    }

    protected override void RefreshData(CancellationToken cancellationToken, int sourceDataVersion, bool updateDisplay, DataChangedEventArgs dataChangedEventArgs)
    {
      CancellationTokenSource comparand = (CancellationTokenSource) null;
      bool flag1 = false;
      Layer layer;
      bool flag2;
      bool flag3;
      bool flag4;
      lock (this.layerLock)
      {
        layer = this.Layer;
        flag2 = layer == this.layerThatNeedsDisplayUpdate;
        flag3 = layer is InstanceLayer;
        flag4 = layer is RegionLayer;
      }
      if (layer == null)
        return;
      GeoDataView geoDataView = this.GeoDataView;
      GeoVisualization geoVisualization = this.geoVisualization;
      LayerDefinition layerDefinition = geoVisualization == null ? (LayerDefinition) null : geoVisualization.LayerDefinition;
      LayerManager layerManager = layerDefinition == null ? (LayerManager) null : layerDefinition.LayerManager;
      VisualizationModel visualizationModel = layerManager == null ? (VisualizationModel) null : layerManager.Model;
      VisualizationEngine eng = visualizationModel == null ? (VisualizationEngine) null : visualizationModel.Engine;
      if (geoVisualization == null || geoDataView == null || eng == null)
        return;
      cancellationToken.ThrowIfCancellationRequested();
      GeoField geo = geoDataView.GetGeo(sourceDataVersion);
      CompletionStats stats;
      lock (this.SourceDataVersionLock)
      {
        stats = !updateDisplay || this.CompletionStats == null ? new CompletionStats() : this.CompletionStats;
        if (geo == null)
        {
          stats.Requested = 0;
          stats.Completed = 0;
          stats.RegionsRequested = 0;
          stats.RegionsCompleted = 0;
          stats.Pending = false;
          stats.Cancelled = false;
          if (updateDisplay && sourceDataVersion >= this.LastDisplayedSourceDataVersion)
            geoVisualization.NotifyDataUpdateCompleted(false);
          if (!updateDisplay || !flag2)
            return;
          Interlocked.CompareExchange<Layer>(ref this.layerThatNeedsDisplayUpdate, (Layer) null, layer);
          return;
        }
        else if (!updateDisplay && sourceDataVersion <= this.LastSourceDataVersion)
        {
          stats.Requested = 0;
          stats.Completed = 0;
          stats.RegionsRequested = 0;
          stats.RegionsCompleted = 0;
          stats.Pending = false;
          stats.Cancelled = false;
          return;
        }
        else
        {
          if (updateDisplay && sourceDataVersion <= this.LastDisplayedSourceDataVersion)
          {
            if (sourceDataVersion < this.LastDisplayedSourceDataVersion || !flag2 && this.AllDataAddedToEngine)
            {
              stats.Requested = 0;
              stats.Completed = 0;
              stats.RegionsRequested = 0;
              stats.RegionsCompleted = 0;
              stats.Pending = false;
              stats.Cancelled = false;
              if (sourceDataVersion != this.LastDisplayedSourceDataVersion)
                return;
              geoVisualization.NotifyDataUpdateCompleted(false);
              return;
            }
            else if (!flag2)
              flag1 = true;
          }
          if (!flag1)
            this.ClearCachedData();
          if (geo.HasLatLongOrXY)
          {
            CancellationTokenSource local_14 = Interlocked.Exchange<CancellationTokenSource>(ref this.cancellationSource, (CancellationTokenSource) null);
            if (local_14 != null)
            {
              try
              {
                local_14.Cancel();
              }
              catch (ObjectDisposedException exception_0)
              {
              }
            }
            comparand = new CancellationTokenSource();
            this.CancellationSource = comparand;
          }
          else
          {
            comparand = new CancellationTokenSource();
            this.CancellationSource = comparand;
          }
        }
      }
      cancellationToken.ThrowIfCancellationRequested();
      int rowCount = geoDataView.GetRowCount(sourceDataVersion);
      if (rowCount < 0)
      {
        stats.Requested = 0;
        stats.Completed = 0;
        stats.RegionsRequested = 0;
        stats.RegionsCompleted = 0;
        stats.Pending = false;
        stats.Cancelled = false;
        if (comparand == null)
          return;
        Interlocked.CompareExchange<CancellationTokenSource>(ref this.cancellationSource, (CancellationTokenSource) null, comparand);
        comparand.Dispose();
      }
      else
      {
        if (updateDisplay && !flag1)
        {
          this.AssignColorIndicesForSeries(geoDataView.GetColorCount(sourceDataVersion));
          if (flag3)
          {
            this.UpdateLayerColorOverride(sourceDataVersion);
            ICollection keys;
            ICollection values;
            geoVisualization.GetVisualizationInstanceModelDataIdsAndProperties(out keys, out values);
            IEnumerator enumerator = values.GetEnumerator();
            foreach (string modelId in keys)
            {
              enumerator.MoveNext();
              int? indexForModelDataId = geoDataView.GetSeriesIndexForModelDataId(sourceDataVersion, modelId);
              if (indexForModelDataId.HasValue)
              {
                bool colorsChanged;
                this.SetSeriesProperties(layer as InstanceLayer, enumerator.Current as GeoVisualizationInstanceProperties, indexForModelDataId.Value, out colorsChanged);
              }
              else if (!flag4)
              {
                InstanceId? idForModelDataId = geoDataView.GetInstanceIdForModelDataId(sourceDataVersion, modelId);
                if (!idForModelDataId.HasValue)
                  VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unable to get InstanceId and seriesIndex for ModelId: {0}", (object) modelId);
                else
                  this.AddProperties(layer as InstanceLayer, enumerator.Current as GeoVisualizationInstanceProperties, idForModelDataId.Value, (InstanceId?[]) null);
              }
              else
              {
                InstanceId?[] seriesForModelDataId = geoDataView.GetCanonicalInstanceIdsForAllSeriesForModelDataId(sourceDataVersion, modelId, geoVisualization.GetDisplayAnyMeasure(), geoVisualization.GetDisplayAnyCategoryValue());
                if (seriesForModelDataId == null)
                  VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unable to get InstanceId for all series for ModelId: {0}", (object) modelId);
                else if (Enumerable.All<InstanceId?>((IEnumerable<InstanceId?>) seriesForModelDataId, (Func<InstanceId?, bool>) (id => !id.HasValue)))
                  VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "InstanceIds for all series are null for ModelId: {0}", (object) modelId);
                else
                  this.AddProperties(layer as InstanceLayer, enumerator.Current as GeoVisualizationInstanceProperties, Enumerable.First<InstanceId?>((IEnumerable<InstanceId?>) seriesForModelDataId, (Func<InstanceId?, bool>) (id => id.HasValue)).Value, seriesForModelDataId);
              }
            }
            if (cancellationToken.IsCancellationRequested)
            {
              if (comparand != null)
              {
                Interlocked.CompareExchange<CancellationTokenSource>(ref this.cancellationSource, (CancellationTokenSource) null, comparand);
                comparand.Dispose();
              }
              cancellationToken.ThrowIfCancellationRequested();
            }
          }
        }
        if (flag4)
          stats.RegionsRequested = rowCount;
        stats.Requested = rowCount;
        stats.Pending = false;
        if (geo.HasLatLongOrXY)
        {
          if (updateDisplay)
          {
            if (!flag1)
            {
              this.AddedToEngine = new BitArray(rowCount);
              lock (this.AddedToEngine)
                this.AddedToEngine.SetAll(false);
            }
            bool flag5 = rowCount > 500;
            CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, comparand.Token);
            LayerDataBinding.LatLonUpdateDisplayContext updateDisplayContext = new LayerDataBinding.LatLonUpdateDisplayContext()
            {
              AsyncUpdate = flag5,
              Count = rowCount,
              SourceDataVersion = sourceDataVersion,
              Layer = layer,
              AddedToEngine = this.AddedToEngine,
              ContinueAddingData = flag1,
              CompletionStats = stats,
              ZoomToData = dataChangedEventArgs.ZoomToData,
              Engine = eng,
              CancellationSource = comparand,
              LinkedCancellationSource = linkedTokenSource
            };
            if (flag5)
              ThreadPool.QueueUserWorkItem(new WaitCallback(this.UpdateDisplayForLatLon), (object) updateDisplayContext);
            else
              this.UpdateDisplayForLatLon((object) updateDisplayContext);
          }
          else
          {
            stats.Resolved = rowCount;
            stats.Completed = rowCount;
          }
        }
        else
        {
          int firstGeoCol;
          Func<int, int, string> geoValuesAccessor = geoDataView.GetGeoValuesAccessor(sourceDataVersion, out firstGeoCol);
          Func<int, int> geoRowsAccessor = geoDataView.GetGeoRowsAccessor(sourceDataVersion);
          int geoBucketCount = geoDataView.GetGeoBucketCount(sourceDataVersion);
          ILatLonProvider latLonProvider = this.LatLonProvider;
          if (geoValuesAccessor == null || geoBucketCount < 0 || latLonProvider == null)
          {
            stats.Requested = 0;
            stats.Completed = 0;
            stats.RegionsRequested = 0;
            stats.RegionsCompleted = 0;
            stats.Pending = false;
            stats.Cancelled = false;
            if (comparand == null)
              return;
            Interlocked.CompareExchange<CancellationTokenSource>(ref this.cancellationSource, (CancellationTokenSource) null, comparand);
            comparand.Dispose();
            return;
          }
          else
          {
            if (!flag1)
            {
              if (flag4)
              {
                RegionLayer regionLayer = layer as RegionLayer;
                GeoEntityField.GeoEntityLevel? geoEntityLevel = geoDataView.GetGeoEntityLevel(sourceDataVersion);
                EntityType? nullable = geoEntityLevel.HasValue ? GeoLevelToEntityTypeConverter.GetEntityType(geoEntityLevel.Value) : new EntityType?();
                RegionLayerShadingMode? regionShadingMode = geoDataView.GetRegionShadingMode(sourceDataVersion, geoVisualization.SelectedRegionShadingMode);
                if (!nullable.HasValue || !regionShadingMode.HasValue)
                {
                  stats.Requested = 0;
                  stats.Completed = 0;
                  stats.RegionsRequested = 0;
                  stats.RegionsCompleted = 0;
                  stats.Pending = false;
                  stats.Cancelled = false;
                  if (comparand == null)
                    return;
                  Interlocked.CompareExchange<CancellationTokenSource>(ref this.cancellationSource, (CancellationTokenSource) null, comparand);
                  comparand.Dispose();
                  return;
                }
                else
                {
                  regionLayer.RegionEntityType = nullable.Value;
                  regionLayer.ShadingMode = regionShadingMode.Value;
                  geoVisualization.CurrentRegionShadingMode = regionShadingMode.Value;
                  this.SubscribeToRegionCompletionEvents(regionLayer);
                }
              }
              DateTime? beginTime;
              DateTime? endtime;
              geoDataView.GetTimeBounds(sourceDataVersion, out beginTime, out endtime);
              double min;
              double max;
              geoDataView.GetMinMaxInstanceValues(sourceDataVersion, out min, out max);
              CustomSpaceTransform transformForGeoDataView = this.GetCustomSpaceTransformForGeoDataView(sourceDataVersion, eng, geoDataView);
              layer.BeginDataInput(geoDataView.GetMaxInstanceCount(sourceDataVersion), !flag4, !geoDataView.QueryResultsHaveMeasures(sourceDataVersion), transformForGeoDataView, min, max, beginTime, endtime);
              this.Lat = new double[geoBucketCount];
              this.Lon = new double[geoBucketCount];
              this.Ambiguities = new GeoAmbiguity[geoBucketCount];
              this.AddedToEngine = new BitArray(geoBucketCount);
              lock (this.AddedToEngine)
                this.AddedToEngine.SetAll(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, comparand.Token);
            latLonProvider.GetLatLonAsync(geo, firstGeoCol, geoBucketCount, geoValuesAccessor, geoRowsAccessor, linkedTokenSource.Token, this.Lat, this.Lon, (GeoResolutionBorder[]) null, this.Ambiguities, stats, updateDisplay ? new Action<object, int, int, int>(this.LatLonResolvedCallback) : (Action<object, int, int, int>) null, new Action<object>(this.LatLonResolutionCompletedCallback), (object) new LayerDataBinding.LatLonResolutionContext()
            {
              UpdateDisplay = updateDisplay,
              Layer = layer,
              Lat = this.Lat,
              Lon = this.Lon,
              AddedToEngine = this.AddedToEngine,
              SourceDataVersion = sourceDataVersion,
              CancellationSource = comparand,
              LinkedCancellationSource = linkedTokenSource,
              Stats = stats,
              ZoomToData = dataChangedEventArgs.ZoomToData,
              Engine = eng
            });
          }
        }
        if (!updateDisplay || !flag2)
          return;
        Interlocked.CompareExchange<Layer>(ref this.layerThatNeedsDisplayUpdate, (Layer) null, layer);
      }
    }

    private CustomSpaceTransform GetCustomSpaceTransformForGeoDataView(int sourceDataVersion, VisualizationEngine eng, GeoDataView gdv)
    {
      CustomMap currentCustomMap = eng.CurrentCustomMap;
      if (currentCustomMap == null)
        return (CustomSpaceTransform) null;
      CustomSpaceTransform transformOrNullForAuto = currentCustomMap.GetTransformOrNullForAuto();
      if (transformOrNullForAuto != null)
        return transformOrNullForAuto;
      RangeOf<double> latRange;
      RangeOf<double> longRange;
      gdv.GetMinMaxLatLong(sourceDataVersion, out latRange, out longRange);
      return currentCustomMap.GetTransformFromAutoRanges(latRange, longRange);
    }

      private void UpdateDisplayForLatLon(object parameter)
      {
          LayerDataBinding.LatLonUpdateDisplayContext updateDisplayContext =
              parameter as LayerDataBinding.LatLonUpdateDisplayContext;
          CompletionStats completionStats = updateDisplayContext.CompletionStats;
          bool abort = false;
          try
          {
              Layer layer = updateDisplayContext.Layer;
              int count = updateDisplayContext.Count;
              int sourceDataVersion = updateDisplayContext.SourceDataVersion;
              IEnumerable<IInstanceParameter> enumerable = (IEnumerable<IInstanceParameter>) null;
              GeoDataView geoDataView = this.GeoDataView;
              List<int> colorIndices = this.ColorIndices;
              abort = colorIndices == null || geoDataView == null || (completionStats == null || layer == null) ||
                      updateDisplayContext.LinkedCancellationSource.IsCancellationRequested;
              if (abort)
              {
                  if (completionStats == null)
                      return;
                  completionStats.Requested = 0;
                  completionStats.Completed = 0;
                  completionStats.RegionsRequested = 0;
                  completionStats.RegionsCompleted = 0;
                  completionStats.Pending = false;
                  completionStats.Cancelled = true;
              }
              else
              {
                  if (!updateDisplayContext.ContinueAddingData)
                  {
                      DateTime? beginTime;
                      DateTime? endtime;
                      geoDataView.GetTimeBounds(sourceDataVersion, out beginTime, out endtime);
                      double min;
                      double max;
                      geoDataView.GetMinMaxInstanceValues(sourceDataVersion, out min, out max);
                      CustomSpaceTransform transformForGeoDataView =
                          this.GetCustomSpaceTransformForGeoDataView(sourceDataVersion, updateDisplayContext.Engine,
                              geoDataView);
                      layer.BeginDataInput(geoDataView.GetMaxInstanceCount(sourceDataVersion), false,
                          !geoDataView.QueryResultsHaveMeasures(sourceDataVersion), transformForGeoDataView, min, max,
                          beginTime, endtime);
                  }
                  int nextRow;
                  for (int i = 0; i < count; i = nextRow)
                  {
                      if (updateDisplayContext.LinkedCancellationSource.IsCancellationRequested)
                      {
                          abort = true;
                          break;
                      }
                      double latitude;
                      double longitude;
                      bool flag =
                          !geoDataView.GetRowData(sourceDataVersion, i, colorIndices, out latitude, out longitude,
                              out enumerable, out nextRow, out abort);
                      if (abort) break;

                      if (flag)
                      {
                          completionStats.InvalidArgs += nextRow - i;
                          completionStats.Completed += nextRow - i;
                      }
                      else
                      {
                          if (!double.IsNaN(latitude) && !double.IsNaN(longitude) &&
                              !updateDisplayContext.AddedToEngine[i])
                          {
                              if (updateDisplayContext.LinkedCancellationSource.IsCancellationRequested)
                              {
                                  abort = true;
                                  break;
                              }
                              else
                                  layer.AddData(latitude, longitude, enumerable);
                          }
                          completionStats.Resolved += nextRow - i;
                          completionStats.Completed += nextRow - i;
                      }
                      lock (updateDisplayContext.AddedToEngine)
                      {
                          for (int j = i; j < nextRow; ++j)
                              updateDisplayContext.AddedToEngine.Set(j, true);
                      }

                  }
                  if (!abort)
                  {
                      layer.EndDataInput();
                      if (updateDisplayContext.ZoomToData && updateDisplayContext.Engine != null)
                          updateDisplayContext.Engine.MoveTo(layer.GetDataEnvelope(updateDisplayContext.Engine.FlatMode));
                      GeoVisualization geoVisualization = this.geoVisualization;
                      if (geoVisualization == null)
                          return;
                      geoVisualization.NotifyDataUpdateCompleted(false);
                  }
              }
          }
          catch (Exception ex)
          {
              try
              {
                  VisualizationTraceSource.Current.Fail(ex);
                  GeoVisualization geoVisualization = this.geoVisualization;
                  LayerDefinition layerDefinition = geoVisualization == null ? null : geoVisualization.LayerDefinition;
                  LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
                  VisualizationModel model = layerManager == null ? null : layerManager.Model;
                  if (model != null)
                      model.RaiseInternalError(ex);
                  abort = true;
              }
              catch (Exception)
              {
              }
          }
          finally
          {
              try
              {
                  if (abort && completionStats != null)
                  {
                      completionStats.Requested = 0;
                      completionStats.Completed = 0;
                      completionStats.RegionsRequested = 0;
                      completionStats.RegionsCompleted = 0;
                      completionStats.Pending = false;
                      completionStats.Cancelled = true;
                  }
                  Interlocked.CompareExchange<CancellationTokenSource>(ref this.cancellationSource,
                      (CancellationTokenSource) null, updateDisplayContext.CancellationSource);
                  updateDisplayContext.CancellationSource.Dispose();
              }
              catch (Exception)
              {
              }
          }
      }

      private void AddProperties(InstanceLayer layer, GeoVisualizationInstanceProperties properties, InstanceId instanceId, InstanceId?[] seriesIds)
    {
      if (properties == null)
        return;
      Dictionary<InstanceId, GeoVisualizationInstanceProperties> dictionary = this.layerInstanceProperties;
      if (dictionary == null)
        return;
      lock (dictionary)
      {
        dictionary.Add(instanceId, properties);
        if (properties.Annotation == null)
          return;
        if (seriesIds == null)
        {
          layer.AddAnnotation(instanceId);
        }
        else
        {
          foreach (InstanceId? item_0 in seriesIds)
          {
            if (item_0.HasValue)
              layer.AddAnnotation(item_0.Value);
          }
        }
      }
    }

    private void SetSeriesProperties(InstanceLayer layer, GeoVisualizationInstanceProperties properties, int seriesIndex, out bool colorsChanged)
    {
      colorsChanged = false;
      if (properties == null)
        return;
      Dictionary<int, GeoVisualizationInstanceProperties> dictionary = this.layerSeriesProperties;
      if (dictionary == null)
        return;
      lock (dictionary)
      {
        dictionary[seriesIndex] = properties;
        if (properties.ColorSet)
          layer.SetColor(seriesIndex, properties.Color);
        else
          layer.ResetColor(seriesIndex);
        colorsChanged = true;
      }
    }

    private void SubscribeToRegionCompletionEvents(RegionLayer regionLayer)
    {
      if (regionLayer == null)
        return;
      this.UnsubscribeFromRegionCompletionEvents(regionLayer);
      regionLayer.OnRegionReady += new EventHandler<RegionEventArgs>(this.OnRegionReady);
      regionLayer.OnRegionCompleted += new EventHandler<RegionEventArgs>(this.OnRegionCompleted);
    }

    private void UnsubscribeFromRegionCompletionEvents(RegionLayer regionLayer)
    {
      if (regionLayer == null)
        return;
      regionLayer.OnRegionReady -= new EventHandler<RegionEventArgs>(this.OnRegionReady);
      regionLayer.OnRegionCompleted -= new EventHandler<RegionEventArgs>(this.OnRegionCompleted);
    }

    private void RegionLayerScalesChanged(object sender, RegionScaleEventArgs args)
    {
      if (args == null)
        return;
      this.RegionScaleMinValues = args.MinValues;
      this.RegionScaleMaxValues = args.MaxValues;
      GeoVisualization geoVisualization = this.geoVisualization;
      if (geoVisualization == null)
        return;
      geoVisualization.NotifyLayerScalesChanged();
    }

    private void OnRegionReady(object sender, RegionEventArgs args)
    {
      if (args == null)
        return;
      this.CompletionStats.RegionsCompleted = Math.Min(this.CompletionStats.RegionsRequested, args.EmptyRegions.Length + args.FailedRegions.Length + args.TessellatedCount);
    }

    private void OnRegionCompleted(object sender, EventArgs args)
    {
      if (args == null)
        return;
      this.UnsubscribeFromRegionCompletionEvents(this.Layer as RegionLayer);
      this.CompletionStats.RegionsCompleted = this.CompletionStats.RegionsRequested;
      RegionEventArgs regionEventArgs = args as RegionEventArgs;
      if (regionEventArgs != null)
      {
        this.UpdateRegionAmbiguities(regionEventArgs.FailedRegions);
        this.UpdateRegionAmbiguities(regionEventArgs.EmptyRegions);
      }
      GeoVisualization geoVisualization = this.geoVisualization;
      if (geoVisualization == null)
        return;
      geoVisualization.NotifyDataUpdateCompleted(false);
    }

    private void HeatMapLayerScaleChanged(object sender, HeatMapScaleEventArgs args)
    {
      if (args == null)
        return;
      this.HeatMapScaleMinValue = (double) args.MinValue;
      this.HeatMapScaleMaxValue = (double) args.MaxValue;
      GeoVisualization geoVisualization = this.geoVisualization;
      if (geoVisualization == null)
        return;
      geoVisualization.NotifyLayerScalesChanged();
    }

    private void UpdateRegionAmbiguities(InstanceId[] instanceIds)
    {
      if (instanceIds == null)
        return;
      GeoDataView geoDataView = this.GeoDataView;
      if (geoDataView == null)
        return;
      GeoAmbiguity[] ambiguities = this.Ambiguities;
      if (ambiguities == null)
        return;
      int length = ambiguities.Length;
      foreach (InstanceId id in instanceIds)
      {
        int bucketForInstanceId = geoDataView.GetBucketForInstanceId(this.LastDisplayedSourceDataVersion, id);
        if (bucketForInstanceId >= 0 && bucketForInstanceId < length)
          ambiguities[bucketForInstanceId].ResolutionType = GeoAmbiguity.Resolution.NoRegionPolygon;
      }
    }

    private void LatLonResolutionCompletedCallback(object context)
    {
      LayerDataBinding.LatLonResolutionContext resolutionContext = context as LayerDataBinding.LatLonResolutionContext;
      if (resolutionContext.UpdateDisplay && !resolutionContext.Stats.Cancelled)
      {
        resolutionContext.Layer.EndDataInput();
        if (resolutionContext.Stats.Completed == resolutionContext.Stats.Requested && resolutionContext.Stats.QueryFailed == 0)
        {
          lock (resolutionContext.AddedToEngine)
            resolutionContext.AddedToEngine.SetAll(true);
        }
      }
      else if (resolutionContext.UpdateDisplay && resolutionContext.Stats.Cancelled)
        this.UnsubscribeFromRegionCompletionEvents(resolutionContext.Layer as RegionLayer);
      if (resolutionContext.UpdateDisplay && resolutionContext.Layer is RegionLayer && (object.ReferenceEquals((object) resolutionContext.Stats, (object) this.CompletionStats) && resolutionContext.Stats.RegionsCompleted == resolutionContext.Stats.RegionsRequested))
        this.OnRegionCompleted((object) this, EventArgs.Empty);
      if (resolutionContext.ZoomToData && Interlocked.Decrement(ref resolutionContext.ZoomCount) >= 0)
        resolutionContext.Engine.MoveTo(resolutionContext.Layer.GetDataEnvelope(resolutionContext.Engine.FlatMode));
      GeoVisualization geoVisualization = this.geoVisualization;
      if (geoVisualization != null && (resolutionContext.Stats.Cancelled || !(resolutionContext.Layer is RegionLayer)))
        geoVisualization.NotifyDataUpdateCompleted(resolutionContext.Stats.Cancelled);
      Interlocked.CompareExchange<CancellationTokenSource>(ref this.cancellationSource, (CancellationTokenSource) null, resolutionContext.CancellationSource);
      resolutionContext.CancellationSource.Dispose();
      this.LogGeocodingQueryResults((IEnumerable<GeoAmbiguity>) this.Ambiguities);
    }

    private void LatLonResolvedCallback(object context, int inputRowIndex, int nextRowToGeoCode, int geoAccessorIndex)
    {
      LayerDataBinding.LatLonResolutionContext resolutionContext = context as LayerDataBinding.LatLonResolutionContext;
      if (resolutionContext.LinkedCancellationSource.IsCancellationRequested)
        return;
      if (resolutionContext.AddedToEngine[geoAccessorIndex])
      {
        lock (this.CompletionStats.WriterLock)
          this.CompletionStats.RegionsCompleted += nextRowToGeoCode - inputRowIndex;
      }
      else
      {
        bool abort = false;
        int nextRow = inputRowIndex;
        GeoDataView geoDataView = this.GeoDataView;
        List<int> colorIndices = this.ColorIndices;
        if (geoDataView == null || colorIndices == null)
          return;
        do
        {
          IEnumerable<IInstanceParameter> enumerable = (IEnumerable<IInstanceParameter>) null;
          int row = nextRow;
          double latitude1;
          double longitude1;
          bool flag = !this.GeoDataView.GetRowData(resolutionContext.SourceDataVersion, row, colorIndices, out latitude1, out longitude1, out enumerable, out nextRow, out abort);
          if (abort)
          {
            resolutionContext.CancellationSource.Cancel();
            break;
          }
          else if (!flag)
          {
            double latitude2 = resolutionContext.Lat[geoAccessorIndex];
            double longitude2 = resolutionContext.Lon[geoAccessorIndex];
            resolutionContext.Layer.AddData(latitude2, longitude2, enumerable);
            if (resolutionContext.ZoomToData && Interlocked.Decrement(ref resolutionContext.ZoomCount) == 0)
              resolutionContext.Engine.MoveTo(resolutionContext.Layer.GetDataEnvelope(resolutionContext.Engine.FlatMode));
          }
        }
        while (nextRow < nextRowToGeoCode);
        lock (resolutionContext.AddedToEngine)
          resolutionContext.AddedToEngine[geoAccessorIndex] = true;
      }
    }

    private void AssignColorIndicesForSeries(int numSeries)
    {
      List<int> colorIndices = this.ColorIndices;
      GeoVisualization geoVisualization = this.geoVisualization;
      VisualizationModel visualizationModel = geoVisualization == null ? (VisualizationModel) null : geoVisualization.VisualizationModel;
      if (colorIndices == null || visualizationModel == null)
        return;
      lock (colorIndices)
      {
        while (colorIndices.Count < numSeries)
          colorIndices.Add(visualizationModel.ColorSelector.NextColorIndex);
      }
      geoVisualization.NotifyColorsChanged();
    }

    private void LogGeocodingQueryResults(IEnumerable<GeoAmbiguity> ambiguities)
    {
      if (ambiguities == null)
        return;
      GeoVisualization geoVisualization = this.geoVisualization;
      LayerDefinition layerDefinition = geoVisualization == null ? (LayerDefinition) null : geoVisualization.LayerDefinition;
      VisualizationTraceSource resultsTraceSource = this.CreateGeocodingQueryResultsTraceSource(layerDefinition == null ? (string) null : layerDefinition.Name + " Geocode Queries");
      if (resultsTraceSource == null)
        return;
      StringBuilder sb = new StringBuilder();
      foreach (GeoAmbiguity geoAmbiguity in ambiguities)
      {
        sb.Append("\t");
        geoAmbiguity.LogString(sb, "\t");
        resultsTraceSource.TraceInformation(((object) sb).ToString());
        sb.Clear();
      }
      resultsTraceSource.Flush();
      resultsTraceSource.Close();
    }

    private VisualizationTraceSource CreateGeocodingQueryResultsTraceSource(string sourceName)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(sourceName))
          return (VisualizationTraceSource) null;
        string environmentVariable = Environment.GetEnvironmentVariable("SodoQueryResultsLogFile");
        if (environmentVariable == null || !Directory.Exists(environmentVariable))
          return (VisualizationTraceSource) null;
        VisualizationTraceSource visualizationTraceSource = new VisualizationTraceSource("Microsoft.Data.GeocodingQueryResultsLogger", SourceLevels.All);
        visualizationTraceSource.AddFileTraceListener(string.Concat(new object[4]
        {
          (object) environmentVariable,
          (object) Path.DirectorySeparatorChar,
          (object) sourceName,
          (object) ".log"
        }));
        visualizationTraceSource.RemoveDefaultTraceListeners();
        visualizationTraceSource.AssertUIEnabled = false;
        foreach (TraceListener traceListener in visualizationTraceSource.Listeners)
          traceListener.TraceOutputOptions = TraceOptions.None;
        return visualizationTraceSource;
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "CreateLogGeocodingQueryResultsTraceSource(): Caught exception, will not log query results, ex={0}", (object) ex);
        return (VisualizationTraceSource) null;
      }
    }

    private class LatLonResolutionContext
    {
      public int ZoomCount = 40;
      private const int NumberOfLocationsToBeDisplzyedBeforeZoomingIntoData = 40;
      public BitArray AddedToEngine;

      public bool UpdateDisplay { get; set; }

      public Layer Layer { get; set; }

      public double[] Lat { get; set; }

      public double[] Lon { get; set; }

      public int SourceDataVersion { get; set; }

      public CancellationTokenSource CancellationSource { get; set; }

      public CancellationTokenSource LinkedCancellationSource { get; set; }

      public CompletionStats Stats { get; set; }

      public bool ZoomToData { get; set; }

      public VisualizationEngine Engine { get; set; }
    }

    private class LatLonUpdateDisplayContext
    {
      public bool AsyncUpdate { get; set; }

      public int Count { get; set; }

      public int SourceDataVersion { get; set; }

      public Layer Layer { get; set; }

      public BitArray AddedToEngine { get; set; }

      public bool ContinueAddingData { get; set; }

      public CompletionStats CompletionStats { get; set; }

      public bool ZoomToData { get; set; }

      public VisualizationEngine Engine { get; set; }

      public CancellationTokenSource CancellationSource { get; set; }

      public CancellationTokenSource LinkedCancellationSource { get; set; }
    }
  }
}
