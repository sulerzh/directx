using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    /// <summary>
    /// 设置图层的空间可视化属性
    /// </summary>
    public class GeoVisualization : Visualization
    {
        public static readonly double MinimumDataDimensionScaleValue = LayerManager.MinimumDataDimensionScaleValue;
        public static readonly double MaximumDataDimensionScaleValue = LayerManager.MaximumDataDimensionScaleValue;
        public static readonly double MinimumFixedDimensionScaleValue = LayerManager.MinimumFixedDimensionScaleValue;
        public static readonly double MaximumFixedDimensionScaleValue = LayerManager.MaximumFixedDimensionScaleValue;
        public static readonly double MinimumOpacityFactorValue = 0.0;
        public static readonly double MaximumOpacityFactorValue = 1.0;
        public static readonly Color4F DefaultColor = new Color4F(0.0f, 0.0f, 0.0f, 0.0f);
        private Hashtable instancePropertiesLookUpByModelID = new Hashtable();
        private LayerType visualType = LayerType.PointMarkerChart;
        private bool displayZeroValues = true;
        private bool displayNegativeValues = true;
        private RegionLayerShadingMode? selectedRegionShadingMode = new RegionLayerShadingMode?();
        private RegionLayerShadingMode currentShadingMode = RegionLayerShadingMode.Global;
        private SelectionStats selectionStats = new SelectionStats();
        private object showHideLock = new object();
        private List<ChartVisualization> chartVisualizations = new List<ChartVisualization>();
        private double[] opacityFactors = { 1.0, 1.0, 1.0, 1.0 };
        private double[] dataDimensionScales = { 1.0, 1.0, 1.0, 1.0 };
        private double[] fixedDimensionScales = { 1.0, 1.0, 1.0, 1.0 };
        private double[] lockedViewScales = { double.NaN, double.NaN, double.NaN, double.NaN };
        private InstancedShape visualShape;
        private InstancedShape? visualShapeForLayer;
        private bool displayNullValues;
        private Color4F? layerColorOverride;
        private readonly WeakEventListener<GeoVisualization, object, PropertyChangedEventArgs> onTimeControllerPropertyChanged;

        public GeoDataSource GeoDataSource
        {
            get
            {
                return this.DataSource as GeoDataSource;
            }
        }

        public Layer Layer
        {
            get
            {
                return this.VisualElement as Layer;
            }
        }

        public GeoFieldWellDefinition GeoFieldWellDefinition
        {
            get
            {
                return this.FieldWellDefinition as GeoFieldWellDefinition;
            }
        }

        public LayerType VisualType
        {
            get
            {
                return this.visualType;
            }
            set
            {
                if (value == this.visualType)
                    return;
                bool refreshDisplayRequired = this.Layer == null || !this.Layer.CanSetLayerType(value);
                this.visualType = value;
                this.DisplayPropertiesUpdated(refreshDisplayRequired);
            }
        }

        public bool DisplayNullValues
        {
            get
            {
                return this.displayNullValues;
            }
            set
            {
                if (this.displayNullValues == value)
                    return;
                this.displayNullValues = value;
                this.DisplayPropertiesUpdated(false);
                if (this.Layer == null)
                    return;
                this.Layer.DisplayNullValues = value;
            }
        }

        public bool DisplayZeroValues
        {
            get
            {
                return this.displayZeroValues;
            }
            set
            {
                if (this.displayZeroValues == value)
                    return;
                this.displayZeroValues = value;
                this.DisplayPropertiesUpdated(false);
                if (this.Layer == null)
                    return;
                this.Layer.DisplayZeroValues = value;
            }
        }

        public bool DisplayNegativeValues
        {
            get
            {
                return this.displayNegativeValues;
            }
            set
            {
                if (this.displayNegativeValues == value)
                    return;
                this.displayNegativeValues = value;
                this.DisplayPropertiesUpdated(false);
                if (this.Layer == null)
                    return;
                this.Layer.DisplayNegativeValues = value;
            }
        }

        public IEnumerable<ChartDecoratorModel> ChartDecoratorModels
        {
            get
            {
                List<ChartVisualization> list = this.chartVisualizations;
                if (list == null)
                    return null;
                lock (list)
                    return list.Select(cvis => cvis.Model);
            }
        }

        public double OpacityFactor
        {
            get
            {
                double[] numArray = this.opacityFactors;
                double d = numArray == null ? double.NaN : numArray[(int)this.ChartTypeFromLayerType(this.VisualType)];
                if (!double.IsNaN(d))
                    return d;
                return 1.0;
            }
            set
            {
                value = Math.Min(value, MaximumOpacityFactorValue);
                value = Math.Max(value, MinimumOpacityFactorValue);
                int index = (int)this.ChartTypeFromLayerType(this.VisualType);
                double[] numArray = this.opacityFactors;
                if (numArray == null || numArray[index] == value)
                    return;
                numArray[index] = value;
                this.DisplayPropertiesUpdated(false);
                this.SetOpacityFactor();
            }
        }

        public double DataDimensionScale
        {
            get
            {
                double[] numArray = this.dataDimensionScales;
                double d = numArray == null ? double.NaN : numArray[(int)this.ChartTypeFromLayerType(this.VisualType)];
                if (!double.IsNaN(d))
                    return d;
                return 1.0;
            }
            set
            {
                value = Math.Min(value, MaximumDataDimensionScaleValue);
                value = Math.Max(value, MinimumDataDimensionScaleValue);
                int index = (int)this.ChartTypeFromLayerType(this.VisualType);
                double[] numArray = this.dataDimensionScales;
                if (numArray == null || numArray[index] == value)
                    return;
                numArray[index] = value;
                this.DisplayPropertiesUpdated(false);
                this.SetDataDimensionScale();
            }
        }

        public double FixedDimensionScale
        {
            get
            {
                double[] numArray = this.fixedDimensionScales;
                double d = numArray == null ? double.NaN : numArray[(int)this.ChartTypeFromLayerType(this.VisualType)];
                if (!double.IsNaN(d))
                    return d;
                return 1.0;
            }
            set
            {
                value = Math.Min(value, MaximumFixedDimensionScaleValue);
                value = Math.Max(value, MinimumFixedDimensionScaleValue);
                int index = (int)this.ChartTypeFromLayerType(this.VisualType);
                double[] numArray = this.fixedDimensionScales;
                if (numArray == null || numArray[index] == value)
                    return;
                numArray[index] = value;
                this.DisplayPropertiesUpdated(false);
                this.SetFixedDimensionScale();
            }
        }

        public bool CanLockScales
        {
            get
            {
                if (this.Layer != null)
                    return this.lockedViewScales != null;
                return false;
            }
        }

        public bool LockScales
        {
            get
            {
                double[] numArray = this.lockedViewScales;
                return !double.IsNaN(numArray == null ? double.NaN : numArray[(int)this.ChartTypeFromLayerType(this.VisualType)]);
            }
            set
            {
                if (!this.CanLockScales || value == this.LockScales)
                    return;
                double[] numArray = this.lockedViewScales;
                if (numArray == null)
                    return;
                int index = (int)this.ChartTypeFromLayerType(this.VisualType);
                if (!value)
                    numArray[index] = double.NaN;
                else if (this.Layer.ViewScale >= 0.0)
                    numArray[index] = this.Layer.ViewScale;
                this.DisplayPropertiesUpdated(false);
                this.SetLockScales();
            }
        }

        public RegionLayerShadingMode CurrentRegionShadingMode
        {
            get
            {
                return this.currentShadingMode;
            }
            internal set
            {
                if (value == this.currentShadingMode)
                    return;
                this.currentShadingMode = value;
                Action<RegionLayerShadingMode> action = this.RegionShadingModeChanged;
                if (action == null)
                    return;
                action(this.currentShadingMode);
            }
        }

        public RegionLayerShadingMode? SelectedRegionShadingMode
        {
            get
            {
                return this.selectedRegionShadingMode;
            }
            set
            {
                if (!value.HasValue)
                    return;
                RegionLayerShadingMode? nullable1 = value;
                RegionLayerShadingMode? nullable2 = this.selectedRegionShadingMode;
                if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 0 : (nullable1.HasValue == nullable2.HasValue ? 1 : 0)) != 0)
                    return;
                this.selectedRegionShadingMode = value;
                this.DisplayPropertiesUpdated(false);
                this.CurrentRegionShadingMode = value.Value;
                LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                RegionLayer regionLayer = layerDataBinding == null ? null : layerDataBinding.Layer as RegionLayer;
                if (regionLayer == null)
                    return;
                regionLayer.ShadingMode = value.Value;
            }
        }

        public InstancedShape VisualShape
        {
            get
            {
                return this.visualShape;
            }
            set
            {
                if (value == this.visualShape)
                    return;
                this.visualShape = value;
                this.DisplayPropertiesUpdated(false);
            }
        }

        public InstancedShape? VisualShapeForLayer
        {
            get
            {
                return this.visualShapeForLayer;
            }
            set
            {
                InstancedShape? nullable1 = value;
                InstancedShape? nullable2 = this.visualShapeForLayer;
                if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 0 : (nullable1.HasValue == nullable2.HasValue ? 1 : 0)) != 0)
                    return;
                this.visualShapeForLayer = value;
                this.DisplayPropertiesUpdated(false);
                if (!value.HasValue)
                    return;
                this.VisualShape = value.Value;
                LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                ChartLayer chartLayer = layerDataBinding == null ? null : layerDataBinding.Layer as ChartLayer;
                if (chartLayer == null)
                    return;
                InstancedShape[] newShapes = new InstancedShape[1] { value.Value };
                chartLayer.SetShapes(newShapes);
            }
        }

        public DateTime? PlayFromTime
        {
            get
            {
                GeoDataSource geoDataSource = this.GeoDataSource;
                if (geoDataSource == null)
                    return new DateTime?();
                return geoDataSource.PlayFromTime;
            }
        }

        public DateTime? PlayToTime
        {
            get
            {
                LayerDefinition layerDefinition = this.LayerDefinition;
                if (layerDefinition == null)
                    return new DateTime?();
                if (!this.Visible || !layerDefinition.Visible)
                    return new DateTime?();
                GeoDataSource geoDataSource = this.GeoDataSource;
                if (geoDataSource == null)
                    return new DateTime?();
                return geoDataSource.PlayToTime;
            }
        }

        public Color4F? LayerColorOverride
        {
            get
            {
                return this.layerColorOverride;
            }
            set
            {
                this.layerColorOverride = value;
                LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                if (layerDataBinding != null)
                    layerDataBinding.UpdateLayerColorOverride(0);
                this.DisplayPropertiesUpdated(false);
            }
        }

        public bool ColorsOverridden
        {
            get
            {
                if (this.LayerColorOverride.HasValue)
                    return true;
                Hashtable hashtable = this.instancePropertiesLookUpByModelID;
                GeoDataSource geoDataSource = this.GeoDataSource;
                if (hashtable != null)
                {
                    lock (hashtable.SyncRoot)
                    {
                        foreach (object val in hashtable.Values)
                        {
                            GeoVisualizationInstanceProperties prop = val as GeoVisualizationInstanceProperties;
                            if (prop != null && prop.ColorSet)
                                return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool HiddenMeasure { get; set; }

        public CompletionStats CompletionStats { get; protected set; }

        public SelectionStats SelectionStats
        {
            get
            {
                return this.selectionStats;
            }
        }

        public IEnumerable<string> Categories
        {
            get
            {
                GeoDataSource geoDataSource = this.GeoDataSource;
                if (geoDataSource == null)
                    return null;
                return geoDataSource.AllCategories;
            }
        }

        public List<Tuple<TableField, AggregationFunction>> Measures
        {
            get
            {
                GeoDataSource geoDataSource = this.GeoDataSource;
                if (geoDataSource == null)
                    return null;
                List<Tuple<TableField, AggregationFunction>> measures = geoDataSource.Measures;
                List<Tuple<TableField, AggregationFunction>> measuresRet = new List<Tuple<TableField, AggregationFunction>>(measures == null ? 0 : measures.Count);
                if (measures != null)
                {
                    try
                    {
                        measures.ForEach((measure => measuresRet.Add(new Tuple<TableField, AggregationFunction>(measure.Item1, measure.Item2))));
                    }
                    catch (InvalidOperationException ex)
                    {
                        measuresRet.Clear();
                    }
                }
                return measuresRet;
            }
        }

        internal List<int> ColorIndices { get; private set; }

        internal List<ChartVisualization> ChartVisualizations
        {
            get
            {
                return this.chartVisualizations;
            }
            set
            {
                this.chartVisualizations = value;
                if (this.chartVisualizations == null)
                    return;
                this.chartVisualizations.ForEach((cv => cv.LayerDefinition = this.LayerDefinition));
            }
        }

        protected bool HasTime
        {
            get
            {
                GeoDataSource geoDataSource = this.GeoDataSource;
                if (geoDataSource != null)
                    return geoDataSource.Time != null;
                return false;
            }
        }

        public event Action<Layer> LayerRemoved;

        public event Action<Layer> LayerAdded;

        public event Action DataUpdateStarted;

        public event Action<bool> DataUpdateCompleted;

        public event Action<LayerManager.Settings> DisplayPropertiesChanged;

        public event Action ColorsChanged;

        public event Action LayerScalesChanged;

        public event Action<RegionLayerShadingMode> RegionShadingModeChanged;

        internal GeoVisualization(SerializableGeoVisualization state, LayerDefinition layerDefinition, GeoDataSource dataSource, CultureInfo modelCulture)
            : base(state, layerDefinition, dataSource)
        {
            this.Unwrap(state, modelCulture);
            this.onTimeControllerPropertyChanged = new WeakEventListener<GeoVisualization, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = TimeControllerPropertyChanged
            };
            this.VisualizationModel.TimeController.PropertyChanged += this.onTimeControllerPropertyChanged.OnEvent;
        }

        internal GeoVisualization(LayerDefinition layerDefinition, DataSource dataSource)
            : base(layerDefinition, dataSource)
        {
            this.FieldWellDefinition = new GeoFieldWellDefinition(this);
            this.Initialize();
            this.onTimeControllerPropertyChanged = new WeakEventListener<GeoVisualization, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = TimeControllerPropertyChanged
            };
            this.VisualizationModel.TimeController.PropertyChanged += this.onTimeControllerPropertyChanged.OnEvent;
        }

        public void AddChartVisualization(ChartDecoratorModel model)
        {
            List<ChartVisualization> list = this.chartVisualizations;
            GeoDataSource geoDataSource = this.GeoDataSource;
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (geoDataSource == null || list == null || layerDefinition == null)
                return;
            lock (list)
            {
                ChartVisualization chartVis = list.FirstOrDefault(cvis => cvis.Id == model.Id);
                if (chartVis != null)
                {
                    chartVis.Model = model;
                }
                else
                {
                    ChartVisualization chartVisItem = new ChartVisualization(model, layerDefinition, geoDataSource);
                    list.Add(chartVisItem);
                }
            }
        }

        public bool RemoveChartVisualizationForModel(ChartDecoratorModel model)
        {
            List<ChartVisualization> list = this.chartVisualizations;
            DataSource dataSource = this.DataSource;
            if (list == null || dataSource == null)
                return false;
            lock (list)
            {
                ChartVisualization chartVis = list.FirstOrDefault(cv => cv.Model.Id == model.Id);
                if (chartVis == null)
                    return false;
                list.Remove(chartVis);
                if (object.ReferenceEquals(dataSource, chartVis.DataSource))
                    dataSource.IncrementReuseCount();
                chartVis.Remove();
                return true;
            }
        }

        public void SetSelected(InstanceId[] ids, bool replaceSelection, SelectionStyle style = SelectionStyle.Outline, object tag = null)
        {
            HitTestableLayer hitTestableLayer = this.Layer as HitTestableLayer;
            if (hitTestableLayer == null)
                return;
            hitTestableLayer.SetSelected(ids, replaceSelection, style, tag);
        }

        public void FocusOnSelection()
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null || layerDefinition.LayerManager == null)
                return;
            VisualizationModel visualizationModel = this.VisualizationModel;
            if (visualizationModel == null)
                return;
            visualizationModel.Engine.FocusOnSelection(false);
        }

        public int GetSeriesIndexForInstanceId(InstanceId id, bool considerPointMarkersAsSeries)
        {
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (geoDataSource == null)
                return -1;
            return geoDataSource.GetSeriesIndexForInstanceId(id, considerPointMarkersAsSeries);
        }

        public Color4F ColorForCategory(string category)
        {
            bool userOverride;
            return this.ColorForCategory(category, out userOverride);
        }

        public Color4F ColorForCategory(string category, out bool userOverride)
        {
            userOverride = false;
            if (category == null)
                return DefaultColor;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (geoDataSource == null)
                return DefaultColor;
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            int seriesIndex;
            int colorIndex;
            if (layerDataBinding == null || !geoDataSource.IndexForCategoryColor(category, out seriesIndex, out colorIndex))
                return DefaultColor;
            Color4F? color = layerDataBinding.ColorForSeriesIndex(seriesIndex, colorIndex, out userOverride);
            if (color.HasValue)
                return color.Value;
            return DefaultColor;
        }

        public Color4F? LayerColor()
        {
            bool userOverride;
            return this.LayerColor(out userOverride);
        }

        public Color4F? LayerColor(out bool userOverride)
        {
            if (this.LayerColorOverride.HasValue)
            {
                userOverride = true;
                return this.LayerColorOverride;
            }

            userOverride = false;
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (layerDataBinding == null)
                return new Color4F?();

            return layerDataBinding.LayerColorForCurrentTheme();

        }

        public Color4F ColorForMeasure(TableField tableField, AggregationFunction aggregation)
        {
            bool userOverride;
            return this.ColorForMeasure(tableField, aggregation, out userOverride);
        }

        public Color4F ColorForMeasure(TableField tableField, AggregationFunction aggregation, out bool userOverride)
        {
            userOverride = false;
            TableMember tableColumn = tableField as TableMember;
            if (tableColumn == null)
                return DefaultColor;
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null)
                return DefaultColor;
            int index = measures.FindIndex((m =>
            {
                if (tableColumn.QuerySubstitutable(m.Item1 as TableMember))
                    return m.Item2 == aggregation;
                return false;
            }));
            if (index < 0)
                return DefaultColor;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (geoDataSource == null)
                return DefaultColor;
            int colorIndex = geoDataSource.ColorIndexForSeriesIndex(index);
            if (colorIndex < 0)
                return DefaultColor;
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (layerDataBinding == null)
                return DefaultColor;
            Color4F? color = layerDataBinding.ColorForSeriesIndex(index, colorIndex, out userOverride);
            if (color.HasValue)
                return color.Value;
            return DefaultColor;
        }

        public void RegionChartBoundsForCategory(string category, out double? min, out double? max)
        {
            min = null;
            max = null;

            if (category == null)
                return;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (geoDataSource == null)
                return;
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (layerDataBinding == null)
                return;
            int seriesIndex = geoDataSource.IndexForCategory(category);
            if (seriesIndex < 0)
                return;
            layerDataBinding.RegionChartBoundsForSeriesIndex(seriesIndex, out min, out max);
        }

        public void RegionChartBoundsForMeasure(TableField tableField, AggregationFunction aggregation, out double? min, out double? max)
        {
            min = null;
            max = null;
            TableMember tableColumn = tableField as TableMember;
            if (tableColumn == null)
                return;
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null)
                return;
            int index = measures.FindIndex((m =>
            {
                if (tableColumn.QuerySubstitutable(m.Item1 as TableMember))
                    return m.Item2 == aggregation;
                return false;
            }));
            if (index < 0)
                return;
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (layerDataBinding == null)
                return;
            layerDataBinding.RegionChartBoundsForSeriesIndex(index, out min, out max);
        }

        public List<GeoAmbiguity> GetGeoAmbiguities(out float confidencePercetage)
        {
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (layerDataBinding != null)
                return layerDataBinding.GetGeoAmbiguities(out confidencePercetage);
            confidencePercetage = 1f;
            return null;
        }

        public void SetColorForSeries(int seriesIndex, Color4F color)
        {
            string modelId = this.ModelDataIdForSeriesIndex(seriesIndex);
            if (modelId == null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "SetColorForSeries(): Unable to get ModelDataId for seriesIndex {0}", (object)seriesIndex);
            }
            else
            {
                Hashtable hashtable = this.instancePropertiesLookUpByModelID;
                if (hashtable == null)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "SetColorForSeries(): instancePropertiesLookUpByModelID is null, GeoVis may have been shut down, seriesIndex = {0}", (object)seriesIndex);
                }
                else
                {
                    GeoVisualizationInstanceProperties properties;
                    lock (hashtable.SyncRoot)
                    {
                        if (hashtable.ContainsKey(modelId))
                        {
                            properties = hashtable[modelId] as GeoVisualizationInstanceProperties;
                            properties.Color = color;
                            properties.ColorSet = true;
                        }
                        else
                        {
                            properties = new GeoVisualizationInstanceProperties(modelId);
                            properties.Color = color;
                            properties.ColorSet = true;
                            hashtable.Add(modelId, properties);
                        }
                    }
                    LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                    if (layerDataBinding != null)
                        layerDataBinding.UpdateSeriesProperties(seriesIndex, properties, true);
                    this.DisplayPropertiesUpdated(false);
                }
            }
        }

        public void ResetColorForSeries(int seriesIndex)
        {
            string str = this.ModelDataIdForSeriesIndex(seriesIndex);
            if (str == null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "ResetColorForSeries(): Unable to get ModelDataId for seriesIndex {0}", (object)seriesIndex);
            }
            else
            {
                Hashtable hashtable = this.instancePropertiesLookUpByModelID;
                if (hashtable == null)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "ResetColorForSeries(): instancePropertiesLookUpByModelID is null, GeoVis may have been shut down, seriesIndex = {0}", (object)seriesIndex);
                }
                else
                {
                    GeoVisualizationInstanceProperties properties = null;
                    bool flag = false;
                    lock (hashtable.SyncRoot)
                    {
                        if (hashtable.ContainsKey(str))
                        {
                            properties = hashtable[str] as GeoVisualizationInstanceProperties;
                            properties.ColorSet = false;
                            if (properties.IsEmpty())
                                hashtable.Remove(str);
                            flag = true;
                        }
                    }
                    if (!flag)
                        return;
                    LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                    if (layerDataBinding != null)
                        layerDataBinding.UpdateSeriesProperties(seriesIndex, properties, true);
                    this.DisplayPropertiesUpdated(false);
                }
            }
        }

        public void ResetAllColors()
        {
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (hashtable != null)
            {
                bool flag = false;
                lock (hashtable.SyncRoot)
                {
                    foreach (GeoVisualizationInstanceProperties prop in this.GetInstancePropertiesArray())
                    {
                        int? seriesIndex = new int?();
                        if (prop.ColorSet)
                        {
                            prop.ColorSet = false;
                            if (prop.IsEmpty())
                                hashtable.Remove(prop.ModelId);
                            seriesIndex = geoDataSource == null ? new int?() : geoDataSource.GetSeriesIndexForModelDataId(prop.ModelId);
                        }
                        if (seriesIndex.HasValue)
                        {
                            LayerDataBinding binding = this.DataBinding as LayerDataBinding;
                            if (binding != null)
                            {
                                binding.UpdateSeriesProperties(seriesIndex.Value, prop, false);
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        VisualizationModel visModel = this.VisualizationModel;
                        if (visModel != null)
                            visModel.ColorSelector.NotifyColorsChanged();
                    }
                }
            }
            this.LayerColorOverride = new Color4F?();
        }

        public string ModelDataIdForId(InstanceId id)
        {
            return base.ModelDataIdForId(id, this.GetDisplayAnyMeasure(), this.GetDisplayAnyCategoryValue());
        }

        public bool GetDisplayAnyMeasure()
        {
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (!((layerDataBinding == null ? (Layer)null : layerDataBinding.Layer) is RegionLayer))
                return false;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (geoDataSource == null)
                return false;
            List<Tuple<TableField, AggregationFunction>> measures = geoDataSource.Measures;
            if (geoDataSource.Category == null && measures != null)
                return measures.Count > 1;
            return false;
        }

        public bool GetDisplayAnyCategoryValue()
        {
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (!((layerDataBinding == null ? (Layer)null : layerDataBinding.Layer) is RegionLayer))
                return false;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (geoDataSource == null)
                return false;
            return geoDataSource.Category != null;
        }

        public void AddOrUpdateAnnotation(InstanceId id, AnnotationTemplateModel annotation)
        {
            string modelId = this.ModelDataIdForId(id);
            if (modelId == null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "AddOrUpdateAnnotation*(: Unable to get ModelDataId for InstanceId.ElementId : {0}", (object)id.ElementId.ToString());
            }
            else
            {
                Hashtable hashtable = this.instancePropertiesLookUpByModelID;
                if (hashtable == null)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "AddOrUpdateAnnotation(): instancePropertiesLookUpByModelID is null, GeoVis may have been shut down, InstanceId = {0}", (object)id.ElementId.ToString());
                }
                else
                {
                    GeoVisualizationInstanceProperties properties;
                    lock (hashtable.SyncRoot)
                    {
                        if (hashtable.ContainsKey(modelId))
                        {
                            properties = hashtable[modelId] as GeoVisualizationInstanceProperties;
                            properties.Annotation = annotation;
                        }
                        else
                        {
                            properties = new GeoVisualizationInstanceProperties(modelId);
                            properties.Annotation = annotation;
                            hashtable.Add(modelId, properties);
                        }
                    }
                    LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                    if (layerDataBinding != null)
                        layerDataBinding.UpdateInstanceAnnotation(id, properties);
                    this.DisplayPropertiesUpdated(true);
                }
            }
        }

        public void DeleteAnnotation(InstanceId id)
        {
            string str = this.ModelDataIdForId(id);
            if (str == null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "DeleteAnnotation(): Unable to get ModelDataId for InstanceId.ElementId : {0}", (object)id.ElementId.ToString());
            }
            else
            {
                Hashtable hashtable = this.instancePropertiesLookUpByModelID;
                if (hashtable == null)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "DeleteAnnotation(): instancePropertiesLookUpByModelID is null, GeoVis may have been shut down, InstanceId = {0}", (object)id.ElementId.ToString());
                }
                else
                {
                    GeoVisualizationInstanceProperties properties = null;
                    bool flag = false;
                    lock (hashtable.SyncRoot)
                    {
                        if (hashtable.ContainsKey(str))
                        {
                            properties = hashtable[str] as GeoVisualizationInstanceProperties;
                            properties.Annotation = null;
                            if (properties.IsEmpty())
                                hashtable.Remove(str);
                            flag = true;
                        }
                    }
                    if (!flag)
                        return;
                    LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                    if (layerDataBinding != null)
                        layerDataBinding.UpdateInstanceAnnotation(id, properties);
                    this.DisplayPropertiesUpdated(true);
                }
            }
        }

        public void GetVisualizationInstanceModelDataIdsAndProperties(out ICollection keys, out ICollection values)
        {
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            if (hashtable == null)
            {
                keys = new object[0];
                values = new object[0];
            }
            else
            {
                lock (hashtable.SyncRoot)
                {
                    int count = hashtable.Keys.Count;
                    object[] keyArray = new object[count];
                    object[] valArray = new object[count];
                    hashtable.Keys.CopyTo(keyArray, 0);
                    hashtable.Values.CopyTo(valArray, 0);
                    keys = keyArray;
                    values = valArray;
                }
            }
        }

        public bool IsInstanceIdAnnotated(InstanceId instanceId)
        {
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            if (hashtable == null)
                return false;
            lock (hashtable.SyncRoot)
            {
                string modelDataId = this.ModelDataIdForId(instanceId);
                return !string.IsNullOrEmpty(modelDataId) && hashtable.ContainsKey(modelDataId);
            }
        }

        /// <summary>
        /// 获取点击查询弹出注记信息
        /// </summary>
        /// <param name="idArray"></param>
        /// <returns></returns>
        public AnnotationTemplateModel GetAnnotation(params InstanceId[] idArray)
        {
            if (idArray == null)
            {
                return null;
            }
            AnnotationTemplateModel result = null;
            bool cloneFlag = false;
            bool titleFlag = false;
            bool titleAFFlag = false;
            bool descFlag = false;
            bool columnFlag = false;
            bool fieldFormatFlag = false;
            bool backgroundColorFlag = false;
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            if (hashtable == null)
            {
                return null;
            }
            lock (hashtable.SyncRoot)
            {
                for (int i = 0; i < idArray.Length; i++)
                {
                    string modelDataId = this.ModelDataIdForId(idArray[i]);
                    if (!string.IsNullOrEmpty(modelDataId) && hashtable.ContainsKey(modelDataId))
                    {
                        AnnotationTemplateModel anno = (hashtable[modelDataId] as GeoVisualizationInstanceProperties).Annotation;
                        if (anno != null)
                        {
                            if (!cloneFlag)
                            {
                                result = anno.Clone();
                                cloneFlag = true;
                                continue;
                            }
                            if (!titleFlag && !result.Title.Equals(anno.Title))
                            {
                                result.Title = new RichTextModel();
                                titleFlag = true;
                            }
                            if (!titleAFFlag &&
                                (0 != string.Compare(result.TitleField, anno.TitleField, StringComparison.OrdinalIgnoreCase) ||
                                result.TitleAF != anno.TitleAF))
                            {
                                result.TitleField = null;
                                result.TitleAF = null;
                                titleAFFlag = true;
                            }
                            if (!descFlag && !result.Description.Equals(anno.Description))
                            {
                                result.Description = new RichTextModel();
                                descFlag = true;
                            }
                            if (!columnFlag)
                            {
                                bool isEquel = true;
                                if (result.NamesOfColumnsToDisplay.Count == anno.NamesOfColumnsToDisplay.Count &&
                                    result.ColumnAggregationFunctions.Count == anno.ColumnAggregationFunctions.Count)
                                {
                                    for (int j = 0; j < result.NamesOfColumnsToDisplay.Count; j++)
                                    {
                                        if (0 != string.Compare(result.NamesOfColumnsToDisplay[j], anno.NamesOfColumnsToDisplay[j], StringComparison.OrdinalIgnoreCase))
                                        {
                                            isEquel = false;
                                            break;
                                        }
                                    }
                                    if (isEquel)
                                    {
                                        for (int k = 0; k < result.ColumnAggregationFunctions.Count; k++)
                                        {
                                            if (result.ColumnAggregationFunctions[k] != anno.ColumnAggregationFunctions[k])
                                            {
                                                isEquel = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    isEquel = false;
                                }
                                if (!isEquel)
                                {
                                    result.NamesOfColumnsToDisplay.Clear();
                                    result.ColumnAggregationFunctions.Clear();
                                    columnFlag = true;
                                }
                            }
                            if (!fieldFormatFlag && !result.FieldFormat.Equals(anno.FieldFormat))
                            {
                                result.FieldFormat = new RichTextModel();
                                fieldFormatFlag = true;
                            }
                            if (!backgroundColorFlag && (result.BackgroundColor != anno.BackgroundColor))
                            {
                                result.BackgroundColor = new System.Windows.Media.Color();
                                ;
                                backgroundColorFlag = true;
                            }
                            if (titleFlag && titleAFFlag && descFlag && columnFlag && fieldFormatFlag && backgroundColorFlag)
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            return result;
        }

        public List<Tuple<AggregationFunction?, string, object>> TableColumnsWithValuesForId(InstanceId id, bool showRelatedCategories)
        {
            GeoDataSource geoDataSource = this.GeoDataSource;
            VisualizationModel visualizationModel = this.VisualizationModel;
            if (geoDataSource != null)
            {
                if (visualizationModel != null)
                {
                    try
                    {
                        DateTime dateTime = geoDataSource.QueryResultsHaveTimeData(geoDataSource.DataVersion) ? visualizationModel.TimeController.CurrentVisualTime : DateTime.MaxValue;
                        return geoDataSource.TableColumnsWithValuesForId(id, showRelatedCategories, new DateTime?(dateTime));
                    }
                    catch (DataSource.InvalidQueryResultsException ex)
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        public override bool Remove()
        {
            if (!this.initialized)
                throw new InvalidOperationException("Object has not been initialized.");
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null)
                return false;
            return layerDefinition.RemoveGeoVisualization();
        }

        public void SetOpacityFactor()
        {
            Layer layer = this.Layer;
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
            if (layer == null || layerManager == null)
                return;
            double num = Math.Max(Math.Min(this.OpacityFactor, 1.0), 0.0);
            layer.Opacity = (float)num;
        }

        public void SetDataDimensionScale()
        {
            Layer layer = this.Layer;
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
            if (layer == null || layerManager == null)
                return;
            double num = Math.Max(Math.Min(this.DataDimensionScale * layerManager.DataDimensionScale, 10.0), 0.1);
            layer.DataDimensionScale = (float)num;
        }

        public void SetFixedDimensionScale()
        {
            Layer layer = this.Layer;
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
            if (layer == null || layerManager == null)
                return;
            double val1 = this.FixedDimensionScale * layerManager.FixedDimensionScale;
            double val2 = 0.02;
            if (this.ChartTypeFromLayerType(this.VisualType) == ChartType.Bubble)
                val2 = 0.22;
            double num = Math.Max(Math.Min(val1, 10.0), val2);
            layer.FixedDimensionScale = (float)num;
        }

        public void SetLockScales()
        {
            Layer layer = this.Layer;
            if (layer == null)
                return;
            int index = (int)this.ChartTypeFromLayerType(this.VisualType);
            if (this.LockScales)
                layer.LockViewScale(this.lockedViewScales[index]);
            else
                layer.UnlockViewScale();
        }

        internal override void Removed()
        {
            this.Visible = false;
            this.Hide();
            this.chartVisualizations.ForEach((cv =>
            {
                if (object.ReferenceEquals(this.DataSource, cv.DataSource))
                    this.DataSource.IncrementReuseCount();
                cv.Removed();
            }));
            this.chartVisualizations.Clear();
            base.Removed();
        }

        internal override void OnVisibleChanged(bool visible)
        {
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            this.chartVisualizations.ForEach((cvis => cvis.OnVisibleChanged(visible)));
            if (layerDataBinding != null && (!visible || !this.ShouldRefreshDisplay))
            {
                if (visible)
                    this.Show(CancellationToken.None);
                else
                    this.Hide();
                LayerDefinition layerDefinition = this.LayerDefinition;
                LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
                if (this.HasTime && layerManager != null)
                    layerManager.ResetPlayTimes();
            }
            this.DisplayPropertiesUpdated(false);
        }

        internal void UpdateDisplayProperties(GeoVisualization newGeo, LayerManager.Settings settings, Action visibilityChangedCallback, bool wasVisible)
        {
            this.GeoFieldWellDefinition.UpdateDisplayProperties(newGeo.GeoFieldWellDefinition);
            this.VisualType = newGeo.VisualType;
            this.VisualShapeForLayer = newGeo.VisualShapeForLayer;
            this.VisualShape = newGeo.VisualShape;
            this.opacityFactors = newGeo.opacityFactors;
            this.OpacityFactor = newGeo.OpacityFactor;
            this.dataDimensionScales = newGeo.dataDimensionScales;
            this.fixedDimensionScales = newGeo.fixedDimensionScales;
            this.lockedViewScales = newGeo.lockedViewScales;
            this.DataDimensionScale = newGeo.DataDimensionScale;
            this.FixedDimensionScale = newGeo.FixedDimensionScale;
            this.SetLockScales();
            this.DisplayNegativeValues = newGeo.DisplayNegativeValues;
            this.DisplayNullValues = newGeo.DisplayNullValues;
            this.DisplayZeroValues = newGeo.DisplayZeroValues;
            this.ResetAllColors();
            this.LayerColorOverride = newGeo.LayerColorOverride;
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (hashtable != null)
            {
                lock (hashtable.SyncRoot)
                {
                    foreach (GeoVisualizationInstanceProperties item in newGeo.GetInstancePropertiesArray())
                    {
                        int? seriesIndex = new int?();
                        if (item.ColorSet)
                        {
                            GeoVisualizationInstanceProperties props = hashtable[item.ModelId] as GeoVisualizationInstanceProperties;
                            if (props != null)
                            {
                                props.Color = item.Color;
                                props.ColorSet = true;
                            }
                            else
                                hashtable.Add(item.ModelId, new GeoVisualizationInstanceProperties(item.ModelId)
                                {
                                    Color = item.Color,
                                    ColorSet = true
                                });
                            seriesIndex = geoDataSource == null ? new int?() : geoDataSource.GetSeriesIndexForModelDataId(item.ModelId);
                        }
                        if (seriesIndex.HasValue)
                        {
                            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                            if (layerDataBinding != null)
                                layerDataBinding.UpdateSeriesProperties(seriesIndex.Value, item, false);
                        }
                    }
                }
            }
            this.ChartVisualizations = newGeo.ChartVisualizations;
            this.SelectedRegionShadingMode = newGeo.SelectedRegionShadingMode;
            this.SetSelected(null, true);
            this.Visible = newGeo.Visible;
            if (this.Visible && this.LayerDefinition.Visible && !wasVisible && this.ShouldRefreshDisplay)
                visibilityChangedCallback();
            if (this.Layer != null)
            {
                if (this.Visible && this.LayerDefinition.Visible)
                    this.Show(CancellationToken.None);
                else
                    this.Hide();
            }
            this.NotifyDisplayPropertiesChanged(settings);
        }

        internal override void ModelMetadataChanged(ModelMetadata modelMetadata, List<string> tablesWithUpdatedData, ref bool requery, ref bool queryChanged)
        {
            base.ModelMetadataChanged(modelMetadata, tablesWithUpdatedData, ref requery, ref queryChanged);
            GeoFieldWellDefinition fieldWellDefinition = this.GeoFieldWellDefinition;
            if (fieldWellDefinition == null)
                return;
            List<Tuple<TableField, AggregationFunction>> measures = fieldWellDefinition.Measures;
            if (measures == null)
                return;
            if (fieldWellDefinition.Category == null && this.HiddenMeasure)
            {
                this.HiddenMeasure = false;
                this.DisplayPropertiesUpdated(true);
            }
            if (fieldWellDefinition.Category != null && measures.Count == 0)
            {
                bool flag = false;
                TableMetadata table1 = fieldWellDefinition.Category is TableColumn ? ((TableMember)fieldWellDefinition.Category).Table : null;
                if (table1 != null && fieldWellDefinition.Geo != null && fieldWellDefinition.Geo.GeoColumns.Count > 0 && (fieldWellDefinition.Geo.GeoColumns[0].Table.ContainsLookupTable(table1) || table1.ContainsLookupTable(fieldWellDefinition.Geo.GeoColumns[0].Table)))
                {
                    TableMetadata table2 = fieldWellDefinition.Time is TableColumn ? ((TableMember)fieldWellDefinition.Time).Table : null;
                    if (table2 == null || table2.ContainsLookupTable(table1) || table1.ContainsLookupTable(table2))
                        flag = true;
                }
                if (flag)
                {
                    fieldWellDefinition.AddMeasure(new Tuple<TableField, AggregationFunction>(fieldWellDefinition.Category, AggregationFunction.Count));
                    this.HiddenMeasure = true;
                    this.DisplayPropertiesUpdated(true);
                }
                else if (this.HiddenMeasure)
                {
                    this.HiddenMeasure = false;
                    this.DisplayPropertiesUpdated(true);
                }
            }
            if (measures.Count == 0)
            {
                if (this.VisualType == LayerType.HeatMapChart || this.VisualType == LayerType.PointMarkerChart || this.VisualType == LayerType.RegionChart)
                    return;
                this.VisualShape = this.VisualType == LayerType.BubbleChart || this.VisualType == LayerType.PieChart ? InstancedShape.CircularCone : InstancedShape.InvertedPyramid;
                this.VisualType = LayerType.PointMarkerChart;
            }
            else
            {
                if (measures.Count != 1 || fieldWellDefinition.Category != null)
                    return;
                if (this.VisualType == LayerType.PieChart)
                {
                    this.VisualType = LayerType.BubbleChart;
                }
                else
                {
                    if (this.VisualType != LayerType.StackedColumnChart && this.VisualType != LayerType.ClusteredColumnChart)
                        return;
                    this.VisualType = LayerType.ColumnChart;
                }
            }
        }

        private IEnumerable<int> WrapColorIndices()
        {
            GeoDataSource geoDataSource = this.GeoDataSource;
            GeoQueryResults geoQueryResults = geoDataSource == null ? null : geoDataSource.QueryResults;
            ModelQueryIndexedKeyColumn indexedKeyColumn = geoQueryResults == null ? null : geoQueryResults.Category;
            List<int> colorIndices = this.ColorIndices;
            if (colorIndices == null)
                return Enumerable.Empty<int>();
            if (geoQueryResults == null || geoDataSource.seriesToColorIndex.Count == 0)
                return colorIndices.Take(64);
            int numColorIndices = colorIndices.Count;
            if (indexedKeyColumn == null)
                return geoDataSource.seriesToColorIndex.TakeWhile(index => index < numColorIndices).Select(index => colorIndices[index]).Take(64);
            if (indexedKeyColumn.PreservedValuesIndex == null)
                return Enumerable.Empty<int>();
            return indexedKeyColumn.PreservedValuesIndex.TakeWhile(index => index < numColorIndices).Select(index => colorIndices[index]).Take(64);
        }

        internal SerializableGeoVisualization Wrap()
        {
            SerializableGeoVisualization geoVis = new SerializableGeoVisualization();
            geoVis.VisualType = this.VisualType;
            geoVis.VisualShape = this.VisualShape;
            geoVis.VisualShapeForLayerSet = this.VisualShapeForLayer.HasValue;
            SerializableGeoVisualization geoVisualization2 = geoVis;
            InstancedShape? visualShapeForLayer = this.VisualShapeForLayer;
            int num1 = visualShapeForLayer.HasValue ? (int)visualShapeForLayer.GetValueOrDefault() : 0;
            geoVisualization2.VisualShapeForLayer = (InstancedShape)num1;
            geoVis.DisplayNegativeValues = this.DisplayNegativeValues;
            geoVis.DisplayNullValues = this.DisplayNullValues;
            geoVis.DisplayZeroValues = this.DisplayZeroValues;
            geoVis.OpacityFactors = this.opacityFactors;
            geoVis.DataDimensionScales = this.dataDimensionScales;
            geoVis.FixedDimensionScales = this.fixedDimensionScales;
            geoVis.LockedViewScales = this.lockedViewScales;
            geoVis.SelectedRegionShadingModeSet = this.SelectedRegionShadingMode.HasValue;
            SerializableGeoVisualization geoVisualization3 = geoVis;
            RegionLayerShadingMode? regionShadingMode = this.SelectedRegionShadingMode;
            int num2 = regionShadingMode.HasValue ? (int)regionShadingMode.GetValueOrDefault() : 1;
            geoVisualization3.SelectedRegionShadingMode = (RegionLayerShadingMode)num2;
            geoVis.LayerColorOverrideSet = this.LayerColorOverride.HasValue;
            geoVis.LayerColorOverride = this.LayerColorOverride ?? new Color4F();
            geoVis.HiddenMeasure = this.HiddenMeasure;
            geoVis.GeoWellDefn = this.GeoFieldWellDefinition.Wrap() as GeoFieldWellDefinition.SerializableGeoFieldWellDefinition;
            geoVis.ColorIndices = this.WrapColorIndices().ToList();
            geoVis.GeoInstanceProperties = this.GetInstancePropertiesArray();
            geoVis.ChartVisualizations = this.chartVisualizations.Select(cv => cv.Wrap()).ToList();
            SerializableGeoVisualization geoVisualization4 = geoVis;
            this.SnapState(geoVisualization4);
            return geoVisualization4;
        }

        internal void Unwrap(SerializableGeoVisualization state, CultureInfo modelCulture)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            if (state.GeoWellDefn == null)
                throw new ArgumentException("state.GeoWellDefn must not be null");
            if (this.FieldWellDefinition != null)
                throw new InvalidOperationException("A FieldWellDefinition has already been set");
            this.SetStateTo(state);
            this.DisplayNegativeValues = state.DisplayNegativeValues;
            this.DisplayNullValues = state.DisplayNullValues;
            this.DisplayZeroValues = state.DisplayZeroValues;
            this.VisualType = state.VisualType;
            this.VisualShapeForLayer = !state.VisualShapeForLayerSet ? new InstancedShape?() : state.VisualShapeForLayer;
            this.VisualShape = state.VisualShape;
            if (state.OpacityFactors != null)
                Array.Copy(state.OpacityFactors, this.opacityFactors, Math.Min(this.opacityFactors.Length, state.OpacityFactors.Length));
            Array.Copy(state.DataDimensionScales, this.dataDimensionScales, Math.Min(this.dataDimensionScales.Length, state.DataDimensionScales.Length));
            Array.Copy(state.FixedDimensionScales, this.fixedDimensionScales, Math.Min(this.fixedDimensionScales.Length, state.FixedDimensionScales.Length));
            if (state.LockedViewScales != null)
                Array.Copy(state.LockedViewScales, this.lockedViewScales, Math.Min(this.lockedViewScales.Length, state.LockedViewScales.Length));
            this.OpacityFactor = this.opacityFactors[(int)this.ChartTypeFromLayerType(this.VisualType)];
            this.DataDimensionScale = this.dataDimensionScales[(int)this.ChartTypeFromLayerType(this.VisualType)];
            this.FixedDimensionScale = this.fixedDimensionScales[(int)this.ChartTypeFromLayerType(this.VisualType)];
            this.SelectedRegionShadingMode = !state.SelectedRegionShadingModeSet ? new RegionLayerShadingMode?() : state.SelectedRegionShadingMode;
            this.LayerColorOverride = !state.LayerColorOverrideSet ? new Color4F?() : state.LayerColorOverride;
            this.HiddenMeasure = state.HiddenMeasure;
            this.FieldWellDefinition = state.GeoWellDefn.Unwrap(this, modelCulture);
            this.ColorIndices = state.ColorIndices;
            this.SetInstancePropertiesFromArray(state.GeoInstanceProperties);
            this.Initialize();
            if (state.ChartVisualizations == null)
                return;
            this.chartVisualizations.AddRange(state.ChartVisualizations.Select(cv => cv.Unwrap(this.LayerDefinition, this.GeoDataSource, modelCulture)));
            this.DisplayPropertiesUpdated(true);
        }

        internal void NotifyDataUpdateCompleted(bool cancelled)
        {
            Action<bool> action = this.DataUpdateCompleted;
            if (action == null)
                return;
            action(cancelled);
        }

        internal void NotifyColorsChanged()
        {
            Action action = this.ColorsChanged;
            if (action == null)
                return;
            action();
        }

        protected void NotifyDataUpdateStarted()
        {
            Action action = this.DataUpdateStarted;
            if (action == null)
                return;
            action();
        }

        protected void NotifyDisplayPropertiesChanged(LayerManager.Settings settings)
        {
            Action<LayerManager.Settings> action = this.DisplayPropertiesChanged;
            if (action == null)
                return;
            action(settings);
        }

        internal void NotifyLayerScalesChanged()
        {
            Action action = this.LayerScalesChanged;
            if (action == null)
                return;
            action();
        }

        internal override void Shutdown()
        {
            if (this.chartVisualizations != null)
            {
                this.chartVisualizations.Clear();
                this.chartVisualizations = null;
            }
            if (this.VisualizationModel != null && this.VisualizationModel.TimeController != null)
                this.VisualizationModel.TimeController.PropertyChanged -= this.onTimeControllerPropertyChanged.OnEvent;
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            if (hashtable != null)
            {
                lock (hashtable.SyncRoot)
                    hashtable.Clear();
            }
            this.instancePropertiesLookUpByModelID = null;
            this.visualShapeForLayer = new InstancedShape?();
            this.selectionStats = null;
            this.opacityFactors = null;
            this.dataDimensionScales = null;
            this.fixedDimensionScales = null;
            this.lockedViewScales = null;
            this.LayerAdded = null;
            this.LayerRemoved = null;
            this.DataUpdateStarted = null;
            this.DataUpdateCompleted = null;
            this.DisplayPropertiesChanged = null;
            this.ColorsChanged = null;
            this.RegionShadingModeChanged = null;
            this.LayerScalesChanged = null;
            if (this.CompletionStats != null)
            {
                this.CompletionStats.Shutdown();
                this.CompletionStats = null;
            }
            this.ColorIndices = null;
            base.Shutdown();
        }

        protected override DataBinding CreateDataBinding()
        {
            if (this.ColorIndices == null)
                this.ColorIndices = new List<int>();
            return new LayerDataBinding(this, null, this.DataSource.CreateDataView(DataView.DataViewType.Excel), this.VisualizationModel.LatLonProvider, this.VisualizationModel.TwoDRenderThread, this.ColorIndices);
        }

        protected override object OnBeginRefresh(bool visible, LayerManager.Settings layerManagerSettings)
        {
            this.CompletionStats = new CompletionStats();
            this.NotifyDataUpdateStarted();
            return new
            {
                LayerManagerSettings = layerManagerSettings,
                HasTime = this.HasTime,
                PlayFromTime = this.PlayFromTime,
                PlayToTime = this.PlayToTime,
                CompletionStats = this.CompletionStats
            };
        }

        protected override void OnEndRefresh(bool visible, dynamic context, Exception ex)
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null)
                return;
            if (ex != null && !(ex is OperationCanceledException))
            {
                CompletionStats completionStats = (CompletionStats)context.CompletionStats;
                if (completionStats != null)
                    completionStats.Failed = true;
            }

            if (context.LayerManagerSettings != null)
            {
                layerDefinition.LayerManager.RefreshSettings(visible, context.LayerManagerSettings, ex);
            }
            else
            {
                if ((this.HasTime != context.HasTime || this.PlayFromTime != context.PlayFromTime || this.PlayToTime != context.PlayToTime) && ex == null)
                    layerDefinition.LayerManager.ResetPlayTimes();
            }
        }

        protected override void Show(CancellationToken cancellationToken)
        {
            lock (this.showHideLock)
            {
                LayerDefinition layerDef = this.LayerDefinition;
                if (layerDef == null || !this.Visible || !layerDef.Visible)
                    return;
                LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                if (layerDataBinding == null)
                    return;
                Layer bindingLayer = layerDataBinding.Layer;
                bool matching = bindingLayer == null || !bindingLayer.CanSetLayerType(this.VisualType);
                bool notMatching = !matching;
                if (bindingLayer != null && matching)
                {
                    if (this.LayerRemoved != null)
                        this.LayerRemoved(bindingLayer);
                    this.HideWorker();
                    layerDataBinding.ClearVisualElement();
                }
                cancellationToken.ThrowIfCancellationRequested();
                if (notMatching)
                {
                    bindingLayer.TrySetLayerType(this.VisualType);
                    this.SetOpacityFactor();
                    this.SetDataDimensionScale();
                    this.SetFixedDimensionScale();
                    this.SetLockScales();
                    ChartLayer chartLayer = bindingLayer as ChartLayer;
                    if (chartLayer != null)
                        chartLayer.SetShapes(new InstancedShape[1] { this.VisualShape });
                }
                else
                {
                    Layer layer;
                    if (this.VisualType == LayerType.HeatMapChart)
                        layer = new HeatMapLayer();
                    else if (this.VisualType == LayerType.RegionChart)
                        layer = new RegionLayer(this.VisualizationModel.RegionProvider, this.GeoDataSource);
                    else
                        layer = new ChartLayer(new InstancedShape[1] { this.VisualShape }, this.VisualType, this.GeoDataSource);
                    cancellationToken.ThrowIfCancellationRequested();
                    layerDataBinding.Layer = layer;
                    layer.DisplayNullValues = this.DisplayNullValues;
                    layer.DisplayZeroValues = this.DisplayZeroValues;
                    layer.DisplayNegativeValues = this.DisplayNegativeValues;
                    this.SetOpacityFactor();
                    this.SetDataDimensionScale();
                    this.SetFixedDimensionScale();
                    int chartType = (int)this.ChartTypeFromLayerType(this.VisualType);
                    this.SetLockScales();
                    if (this.LayerAdded != null)
                        this.LayerAdded(layer);
                    HitTestableLayer hitTestableLayer = layerDataBinding.Layer as HitTestableLayer;
                    if (hitTestableLayer != null)
                        hitTestableLayer.OnSelectionChanged += this.SelectionsChanged;
                }
                cancellationToken.ThrowIfCancellationRequested();
                layerDataBinding.CompletionStats = this.CompletionStats;
                cancellationToken.ThrowIfCancellationRequested();
                VisualizationModel visModel = this.VisualizationModel;
                if (visModel == null)
                    return;
                visModel.Engine.AddLayer(layerDataBinding.Layer);
            }
        }

        protected override void Hide()
        {
            lock (this.showHideLock)
            {
                LayerDefinition layerDef = this.LayerDefinition;
                if (layerDef == null || this.Visible && layerDef.Visible)
                    return;
                this.HideWorker();
            }
        }

        protected void HideWorker()
        {
            this.ClearSelectionStats();
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (layerDataBinding == null)
                return;
            Layer layer = layerDataBinding.Layer;
            if (layer == null)
                return;
            HitTestableLayer hitTestableLayer = layer as HitTestableLayer;
            if (hitTestableLayer != null)
                hitTestableLayer.OnSelectionChanged -= this.SelectionsChanged;
            VisualizationModel visualizationModel = this.VisualizationModel;
            if (visualizationModel == null)
                return;
            visualizationModel.Engine.RemoveLayer(layer);
        }

        protected void UpdateSelectionStatistics(IList<InstanceId> selectedIds = null)
        {
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            SelectionStats selectionStats = this.SelectionStats;
            if (selectionStats == null)
                return;
            if (layerDataBinding == null)
            {
                this.ClearSelectionStats();
            }
            else
            {
                HitTestableLayer hitTestableLayer = layerDataBinding.Layer as HitTestableLayer;
                if (hitTestableLayer == null)
                {
                    this.ClearSelectionStats();
                }
                else
                {
                    if (selectedIds == null)
                        selectedIds = hitTestableLayer.GetSelected();
                    if (selectedIds.Count == 0)
                    {
                        this.ClearSelectionStats();
                    }
                    else
                    {
                        GeoDataSource geoDataSource = this.GeoDataSource;
                        VisualizationModel visualizationModel = this.VisualizationModel;
                        if (geoDataSource == null || visualizationModel == null)
                        {
                            this.ClearSelectionStats();
                        }
                        else
                        {
                            selectionStats.Clear();
                            try
                            {
                                DateTime currentTime = geoDataSource.QueryResultsHaveTimeData(geoDataSource.DataVersion) ? visualizationModel.TimeController.CurrentVisualTime : DateTime.MaxValue;
                                foreach (InstanceId selectedId in selectedIds)
                                {
                                    foreach (InstanceId relatedIdOverTime in geoDataSource.GetRelatedIdsOverTime(selectedId))
                                    {
                                        double? valueForIdAtTime = geoDataSource.GetValueForIdAtTime(relatedIdOverTime, currentTime);
                                        if (valueForIdAtTime.HasValue)
                                        {
                                            selectionStats.UpdateWithValue(valueForIdAtTime.Value);
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (DataSource.InvalidQueryResultsException ex)
                            {
                                selectionStats.Clear();
                            }
                            LayerDefinition layerDefinition = this.LayerDefinition;
                            LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
                            if (layerManager == null)
                                return;
                            layerManager.SelectionStatsChanged();
                        }
                    }
                }
            }
        }

        protected void ClearSelectionStats()
        {
            SelectionStats selectionStats = this.SelectionStats;
            if (selectionStats == null)
                return;
            selectionStats.Clear();
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
            if (layerManager == null)
                return;
            layerManager.SelectionStatsChanged();
        }

        private void SelectionsChanged(object sender, SelectionEventArgs selectionEventArgs)
        {
            if (selectionEventArgs == null)
                return;
            this.UpdateSelectionStatistics(selectionEventArgs.SelectedIds);
        }

        private static void TimeControllerPropertyChanged(GeoVisualization geoViz, object sender, PropertyChangedEventArgs args)
        {
            if (args == null)
                return;
            VisualizationModel visualizationModel = geoViz.VisualizationModel;
            if (visualizationModel != null)
            {
                ITimeController timeController = visualizationModel.TimeController;
            }
            if (visualizationModel.TimeController == null || !(args.PropertyName == visualizationModel.TimeController.PropertyVisualTimeEnabled) && !(args.PropertyName == visualizationModel.TimeController.PropertyCurrentVisualTime))
                return;
            geoViz.UpdateSelectionStatistics();
        }

        private ChartType ChartTypeFromLayerType(LayerType layerType)
        {
            switch (layerType)
            {
                case LayerType.PointMarkerChart:
                case LayerType.ColumnChart:
                case LayerType.ClusteredColumnChart:
                case LayerType.StackedColumnChart:
                    return ChartType.Column;
                case LayerType.BubbleChart:
                case LayerType.PieChart:
                    return ChartType.Bubble;
                case LayerType.HeatMapChart:
                    return ChartType.HeatMap;
                case LayerType.RegionChart:
                    return ChartType.Region;
                default:
                    return ChartType.NumChartTypes;
            }
        }

        private GeoVisualizationInstanceProperties[] GetInstancePropertiesArray()
        {
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            if (hashtable == null)
                return new GeoVisualizationInstanceProperties[0];
            GeoVisualizationInstanceProperties[] instancePropertiesArray;
            lock (hashtable.SyncRoot)
            {
                instancePropertiesArray = new GeoVisualizationInstanceProperties[hashtable.Count];
                hashtable.Values.CopyTo(instancePropertiesArray, 0);
            }
            return instancePropertiesArray;
        }

        private void SetInstancePropertiesFromArray(GeoVisualizationInstanceProperties[] instancePropertiesArray)
        {
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            if (hashtable == null)
                return;
            lock (hashtable.SyncRoot)
            {
                foreach (GeoVisualizationInstanceProperties prop in instancePropertiesArray)
                {
                    prop.Repair();
                    hashtable.Add(prop.ModelId, prop);
                }
            }
        }

        private enum ChartType
        {
            Column,
            Bubble,
            HeatMap,
            Region,
            NumChartTypes,
        }

        [Serializable]
        public class SerializableGeoVisualization : SerializableVisualization
        {
            [XmlArrayItem("LockedViewScale", typeof(double))]
            [XmlArray("LockedViewScales")]
            public double[] LockedViewScales;
            [XmlAttribute("LayerColorSet")]
            public bool LayerColorOverrideSet;
            [XmlElement("LayerColor")]
            public Color4F LayerColorOverride;
            [XmlAttribute("RegionShadingModeSet")]
            public bool SelectedRegionShadingModeSet;
            [XmlAttribute("RegionShadingMode")]
            public RegionLayerShadingMode SelectedRegionShadingMode;
            [XmlArray("ColorIndices")]
            [XmlArrayItem("ColorIndex", typeof(int))]
            public List<int> ColorIndices;
            [XmlElement("GeoFieldWellDefinition")]
            public GeoFieldWellDefinition.SerializableGeoFieldWellDefinition GeoWellDefn;
            [XmlArrayItem("InstanceProperty", typeof(GeoVisualizationInstanceProperties))]
            [XmlArray("Properties")]
            public GeoVisualizationInstanceProperties[] GeoInstanceProperties;
            [XmlArray("ChartVisualizations")]
            [XmlArrayItem("ChartVisualization", typeof(ChartVisualization.SerializableChartVisualization))]
            public List<ChartVisualization.SerializableChartVisualization> ChartVisualizations;

            [XmlAttribute]
            public LayerType VisualType { get; set; }

            [XmlAttribute("Nulls")]
            public bool DisplayNullValues { get; set; }

            [XmlAttribute("Zeros")]
            public bool DisplayZeroValues { get; set; }

            [XmlAttribute("Negatives")]
            public bool DisplayNegativeValues { get; set; }

            [XmlArray("OpacityFactors")]
            [XmlArrayItem("OpacityFactor", typeof(double))]
            public double[] OpacityFactors { get; set; }

            [XmlArrayItem("DataScale", typeof(double))]
            [XmlArray("DataScales")]
            public double[] DataDimensionScales { get; set; }

            [XmlArrayItem("DimnScale", typeof(double))]
            [XmlArray("DimnScales")]
            public double[] FixedDimensionScales { get; set; }

            [XmlAttribute]
            public InstancedShape VisualShape { get; set; }

            [XmlAttribute("LayerShapeSet")]
            public bool VisualShapeForLayerSet { get; set; }

            [XmlAttribute("LayerShape")]
            public InstancedShape VisualShapeForLayer { get; set; }

            [XmlAttribute]
            public bool HiddenMeasure { get; set; }

            internal GeoVisualization Unwrap(LayerDefinition layerDefinition, DataSource dataSource, CultureInfo modelCulture)
            {
                if (!(dataSource is GeoDataSource))
                    throw new ArgumentException("dataSource is not a GeoDataSource");
                return new GeoVisualization(this, layerDefinition, dataSource as GeoDataSource, modelCulture);
            }
        }
    }
}
