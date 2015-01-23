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
        private double[] opacityFactors = new double[4]
    {
      1.0,
      1.0,
      1.0,
      1.0
    };
        private double[] dataDimensionScales = new double[4]
    {
      1.0,
      1.0,
      1.0,
      1.0
    };
        private double[] fixedDimensionScales = new double[4]
    {
      1.0,
      1.0,
      1.0,
      1.0
    };
        private double[] lockedViewScales = new double[4]
    {
      double.NaN,
      double.NaN,
      double.NaN,
      double.NaN
    };
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
                    return (IEnumerable<ChartDecoratorModel>)null;
                lock (list)
                    return Enumerable.Select<ChartVisualization, ChartDecoratorModel>((IEnumerable<ChartVisualization>)list, (Func<ChartVisualization, ChartDecoratorModel>)(cvis => cvis.Model));
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
                else
                    return 1.0;
            }
            set
            {
                value = Math.Min(value, GeoVisualization.MaximumOpacityFactorValue);
                value = Math.Max(value, GeoVisualization.MinimumOpacityFactorValue);
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
                else
                    return 1.0;
            }
            set
            {
                value = Math.Min(value, GeoVisualization.MaximumDataDimensionScaleValue);
                value = Math.Max(value, GeoVisualization.MinimumDataDimensionScaleValue);
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
                else
                    return 1.0;
            }
            set
            {
                value = Math.Min(value, GeoVisualization.MaximumFixedDimensionScaleValue);
                value = Math.Max(value, GeoVisualization.MinimumFixedDimensionScaleValue);
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
                else
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
                RegionLayer regionLayer = layerDataBinding == null ? (RegionLayer)null : layerDataBinding.Layer as RegionLayer;
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
                ChartLayer chartLayer = layerDataBinding == null ? (ChartLayer)null : layerDataBinding.Layer as ChartLayer;
                if (chartLayer == null)
                    return;
                InstancedShape[] newShapes = new InstancedShape[1]
        {
          value.Value
        };
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
                else
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
                else
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
                        foreach (object item_0 in (IEnumerable)hashtable.Values)
                        {
                            GeoVisualizationInstanceProperties local_2 = item_0 as GeoVisualizationInstanceProperties;
                            if (local_2 != null && local_2.ColorSet)
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
                    return (IEnumerable<string>)null;
                else
                    return (IEnumerable<string>)geoDataSource.AllCategories;
            }
        }

        public List<Tuple<TableField, AggregationFunction>> Measures
        {
            get
            {
                GeoDataSource geoDataSource = this.GeoDataSource;
                if (geoDataSource == null)
                    return (List<Tuple<TableField, AggregationFunction>>)null;
                List<Tuple<TableField, AggregationFunction>> measures = geoDataSource.Measures;
                List<Tuple<TableField, AggregationFunction>> measuresRet = new List<Tuple<TableField, AggregationFunction>>(measures == null ? 0 : measures.Count);
                if (measures != null)
                {
                    try
                    {
                        measures.ForEach((Action<Tuple<TableField, AggregationFunction>>)(measure => measuresRet.Add(new Tuple<TableField, AggregationFunction>(measure.Item1, measure.Item2))));
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
                this.chartVisualizations.ForEach((Action<ChartVisualization>)(cv => cv.LayerDefinition = this.LayerDefinition));
            }
        }

        protected bool HasTime
        {
            get
            {
                GeoDataSource geoDataSource = this.GeoDataSource;
                if (geoDataSource != null)
                    return geoDataSource.Time != null;
                else
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

        internal GeoVisualization(GeoVisualization.SerializableGeoVisualization state, LayerDefinition layerDefinition, GeoDataSource dataSource, CultureInfo modelCulture)
            : base((Visualization.SerializableVisualization)state, layerDefinition, (DataSource)dataSource)
        {
            this.Unwrap(state, modelCulture);
            this.onTimeControllerPropertyChanged = new WeakEventListener<GeoVisualization, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = new Action<GeoVisualization, object, PropertyChangedEventArgs>(GeoVisualization.TimeControllerPropertyChanged)
            };
            this.VisualizationModel.TimeController.PropertyChanged += new PropertyChangedEventHandler(this.onTimeControllerPropertyChanged.OnEvent);
        }

        internal GeoVisualization(LayerDefinition layerDefinition, DataSource dataSource)
            : base(layerDefinition, dataSource)
        {
            this.FieldWellDefinition = (FieldWellDefinition)new GeoFieldWellDefinition(this);
            this.Initialize();
            this.onTimeControllerPropertyChanged = new WeakEventListener<GeoVisualization, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = new Action<GeoVisualization, object, PropertyChangedEventArgs>(GeoVisualization.TimeControllerPropertyChanged)
            };
            this.VisualizationModel.TimeController.PropertyChanged += new PropertyChangedEventHandler(this.onTimeControllerPropertyChanged.OnEvent);
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
                ChartVisualization local_3 = Enumerable.FirstOrDefault<ChartVisualization>((IEnumerable<ChartVisualization>)list, (Func<ChartVisualization, bool>)(cvis => cvis.Id == model.Id));
                if (local_3 != null)
                {
                    local_3.Model = model;
                }
                else
                {
                    ChartVisualization local_3_1 = new ChartVisualization(model, layerDefinition, (DataSource)geoDataSource);
                    list.Add(local_3_1);
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
                ChartVisualization local_2 = Enumerable.FirstOrDefault<ChartVisualization>((IEnumerable<ChartVisualization>)list, (Func<ChartVisualization, bool>)(cv => cv.Model.Id == model.Id));
                if (local_2 == null)
                    return false;
                list.Remove(local_2);
                if (object.ReferenceEquals((object)dataSource, (object)local_2.DataSource))
                    dataSource.IncrementReuseCount();
                local_2.Remove();
                return true;
            }
        }

        public void SetSelected(InstanceId[] ids, bool replaceSelection, SelectionStyle style = SelectionStyle.Outline, object tag = null)
        {
            HitTestableLayer hitTestableLayer = this.Layer as HitTestableLayer;
            if (hitTestableLayer == null)
                return;
            hitTestableLayer.SetSelected((IList<InstanceId>)ids, replaceSelection, style, tag);
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
            else
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
                return GeoVisualization.DefaultColor;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (geoDataSource == null)
                return GeoVisualization.DefaultColor;
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            int seriesIndex;
            int colorIndex;
            if (layerDataBinding == null || !geoDataSource.IndexForCategoryColor(category, out seriesIndex, out colorIndex))
                return GeoVisualization.DefaultColor;
            Color4F? nullable = layerDataBinding.ColorForSeriesIndex(seriesIndex, colorIndex, out userOverride);
            if (nullable.HasValue)
                return nullable.Value;
            else
                return GeoVisualization.DefaultColor;
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
            else
            {
                userOverride = false;
                LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
                if (layerDataBinding == null)
                    return new Color4F?();
                else
                    return layerDataBinding.LayerColorForCurrentTheme();
            }
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
                return GeoVisualization.DefaultColor;
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null)
                return GeoVisualization.DefaultColor;
            int index = measures.FindIndex((Predicate<Tuple<TableField, AggregationFunction>>)(m =>
            {
                if (tableColumn.QuerySubstitutable(m.Item1 as TableMember))
                    return m.Item2 == aggregation;
                else
                    return false;
            }));
            if (index < 0)
                return GeoVisualization.DefaultColor;
            GeoDataSource geoDataSource = this.GeoDataSource;
            if (geoDataSource == null)
                return GeoVisualization.DefaultColor;
            int colorIndex = geoDataSource.ColorIndexForSeriesIndex(index);
            if (colorIndex < 0)
                return GeoVisualization.DefaultColor;
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            if (layerDataBinding == null)
                return GeoVisualization.DefaultColor;
            Color4F? nullable = layerDataBinding.ColorForSeriesIndex(index, colorIndex, out userOverride);
            if (nullable.HasValue)
                return nullable.Value;
            else
                return GeoVisualization.DefaultColor;
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
            int index = measures.FindIndex((Predicate<Tuple<TableField, AggregationFunction>>)(m =>
            {
                if (tableColumn.QuerySubstitutable(m.Item1 as TableMember))
                    return m.Item2 == aggregation;
                else
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
            return (List<GeoAmbiguity>)null;
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
                        if (hashtable.ContainsKey((object)modelId))
                        {
                            properties = hashtable[(object)modelId] as GeoVisualizationInstanceProperties;
                            properties.Color = color;
                            properties.ColorSet = true;
                        }
                        else
                        {
                            properties = new GeoVisualizationInstanceProperties(modelId);
                            properties.Color = color;
                            properties.ColorSet = true;
                            hashtable.Add((object)modelId, (object)properties);
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
                    GeoVisualizationInstanceProperties properties = (GeoVisualizationInstanceProperties)null;
                    bool flag = false;
                    lock (hashtable.SyncRoot)
                    {
                        if (hashtable.ContainsKey((object)str))
                        {
                            properties = hashtable[(object)str] as GeoVisualizationInstanceProperties;
                            properties.ColorSet = false;
                            if (properties.IsEmpty())
                                hashtable.Remove((object)str);
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
                    foreach (GeoVisualizationInstanceProperties item_0 in this.GetInstancePropertiesArray())
                    {
                        int? local_5 = new int?();
                        if (item_0.ColorSet)
                        {
                            item_0.ColorSet = false;
                            if (item_0.IsEmpty())
                                hashtable.Remove((object)item_0.ModelId);
                            local_5 = geoDataSource == null ? new int?() : geoDataSource.GetSeriesIndexForModelDataId(item_0.ModelId);
                        }
                        if (local_5.HasValue)
                        {
                            LayerDataBinding local_6 = this.DataBinding as LayerDataBinding;
                            if (local_6 != null)
                            {
                                local_6.UpdateSeriesProperties(local_5.Value, item_0, false);
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        VisualizationModel local_7 = this.VisualizationModel;
                        if (local_7 != null)
                            local_7.ColorSelector.NotifyColorsChanged();
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
            else
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
            else
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
                        if (hashtable.ContainsKey((object)modelId))
                        {
                            properties = hashtable[(object)modelId] as GeoVisualizationInstanceProperties;
                            properties.Annotation = annotation;
                        }
                        else
                        {
                            properties = new GeoVisualizationInstanceProperties(modelId);
                            properties.Annotation = annotation;
                            hashtable.Add((object)modelId, (object)properties);
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
                    GeoVisualizationInstanceProperties properties = (GeoVisualizationInstanceProperties)null;
                    bool flag = false;
                    lock (hashtable.SyncRoot)
                    {
                        if (hashtable.ContainsKey((object)str))
                        {
                            properties = hashtable[(object)str] as GeoVisualizationInstanceProperties;
                            properties.Annotation = (AnnotationTemplateModel)null;
                            if (properties.IsEmpty())
                                hashtable.Remove((object)str);
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
                keys = (ICollection)new object[0];
                values = (ICollection)new object[0];
            }
            else
            {
                lock (hashtable.SyncRoot)
                {
                    int local_1 = hashtable.Keys.Count;
                    object[] local_2 = new object[local_1];
                    object[] local_3 = new object[local_1];
                    hashtable.Keys.CopyTo((Array)local_2, 0);
                    hashtable.Values.CopyTo((Array)local_3, 0);
                    keys = (ICollection)local_2;
                    values = (ICollection)local_3;
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
                string local_0 = this.ModelDataIdForId(instanceId);
                return !string.IsNullOrEmpty(local_0) && hashtable.ContainsKey((object)local_0);
            }
        }

        public AnnotationTemplateModel GetAnnotation(params InstanceId[] idArray)
        {
            if (idArray == null)
                return (AnnotationTemplateModel)null;
            AnnotationTemplateModel annotationTemplateModel = (AnnotationTemplateModel)null;
            bool flag1 = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            if (hashtable == null)
                return (AnnotationTemplateModel)null;
            lock (hashtable.SyncRoot)
            {
                for (int local_11 = 0; local_11 < idArray.Length; ++local_11)
                {
                    string local_2 = this.ModelDataIdForId(idArray[local_11]);
                    if (!string.IsNullOrEmpty(local_2) && hashtable.ContainsKey((object)local_2))
                    {
                        AnnotationTemplateModel local_1_1 = (hashtable[(object)local_2] as GeoVisualizationInstanceProperties).Annotation;
                        if (local_1_1 != null)
                        {
                            if (!flag1)
                            {
                                annotationTemplateModel = local_1_1.Clone();
                                flag1 = true;
                            }
                            else
                            {
                                if (!flag2 && !annotationTemplateModel.Title.Equals(local_1_1.Title))
                                {
                                    annotationTemplateModel.Title = new RichTextModel();
                                    flag2 = true;
                                }
                                if (!flag3)
                                {
                                    if (string.Compare(annotationTemplateModel.TitleField, local_1_1.TitleField, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        AggregationFunction? local_17 = annotationTemplateModel.TitleAF;
                                        AggregationFunction? local_18 = local_1_1.TitleAF;
                                        if ((local_17.GetValueOrDefault() != local_18.GetValueOrDefault() ? 1 : (local_17.HasValue != local_18.HasValue ? 1 : 0)) == 0)
                                            goto label_16;
                                    }
                                    annotationTemplateModel.TitleField = (string)null;
                                    annotationTemplateModel.TitleAF = new AggregationFunction?();
                                    flag3 = true;
                                }
                            label_16:
                                if (!flag4 && !annotationTemplateModel.Description.Equals(local_1_1.Description))
                                {
                                    annotationTemplateModel.Description = new RichTextModel();
                                    flag4 = true;
                                }
                                if (!flag5)
                                {
                                    bool local_12 = true;
                                    if (annotationTemplateModel.NamesOfColumnsToDisplay.Count == local_1_1.NamesOfColumnsToDisplay.Count && annotationTemplateModel.ColumnAggregationFunctions.Count == local_1_1.ColumnAggregationFunctions.Count)
                                    {
                                        for (int local_13 = 0; local_13 < annotationTemplateModel.NamesOfColumnsToDisplay.Count; ++local_13)
                                        {
                                            if (string.Compare(annotationTemplateModel.NamesOfColumnsToDisplay[local_13], local_1_1.NamesOfColumnsToDisplay[local_13], StringComparison.OrdinalIgnoreCase) != 0)
                                            {
                                                local_12 = false;
                                                break;
                                            }
                                        }
                                        if (local_12)
                                        {
                                            for (int local_14 = 0; local_14 < annotationTemplateModel.ColumnAggregationFunctions.Count; ++local_14)
                                            {
                                                AggregationFunction? local_20 = annotationTemplateModel.ColumnAggregationFunctions[local_14];
                                                AggregationFunction? local_21 = local_1_1.ColumnAggregationFunctions[local_14];
                                                if ((local_20.GetValueOrDefault() != local_21.GetValueOrDefault() ? 1 : (local_20.HasValue != local_21.HasValue ? 1 : 0)) != 0)
                                                {
                                                    local_12 = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                        local_12 = false;
                                    if (!local_12)
                                    {
                                        annotationTemplateModel.NamesOfColumnsToDisplay.Clear();
                                        annotationTemplateModel.ColumnAggregationFunctions.Clear();
                                        flag5 = true;
                                    }
                                }
                                if (!flag6 && !annotationTemplateModel.FieldFormat.Equals(local_1_1.FieldFormat))
                                {
                                    annotationTemplateModel.FieldFormat = new RichTextModel();
                                    flag6 = true;
                                }
                                if (!flag7 && annotationTemplateModel.BackgroundColor != local_1_1.BackgroundColor)
                                {
                                    annotationTemplateModel.BackgroundColor = new System.Windows.Media.Color();
                                    flag7 = true;
                                }
                                if (flag2 && flag3 && (flag4 && flag5) && flag6)
                                {
                                    if (flag7)
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return annotationTemplateModel;
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
                        return (List<Tuple<AggregationFunction?, string, object>>)null;
                    }
                }
            }
            return (List<Tuple<AggregationFunction?, string, object>>)null;
        }

        public override bool Remove()
        {
            if (!this.initialized)
                throw new InvalidOperationException("Object has not been initialized.");
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null)
                return false;
            else
                return layerDefinition.RemoveGeoVisualization();
        }

        public void SetOpacityFactor()
        {
            Layer layer = this.Layer;
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = layerDefinition == null ? (LayerManager)null : layerDefinition.LayerManager;
            if (layer == null || layerManager == null)
                return;
            double num = Math.Max(Math.Min(this.OpacityFactor, 1.0), 0.0);
            layer.Opacity = (float)num;
        }

        public void SetDataDimensionScale()
        {
            Layer layer = this.Layer;
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = layerDefinition == null ? (LayerManager)null : layerDefinition.LayerManager;
            if (layer == null || layerManager == null)
                return;
            double num = Math.Max(Math.Min(this.DataDimensionScale * layerManager.DataDimensionScale, 10.0), 0.1);
            layer.DataDimensionScale = (float)num;
        }

        public void SetFixedDimensionScale()
        {
            Layer layer = this.Layer;
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = layerDefinition == null ? (LayerManager)null : layerDefinition.LayerManager;
            if (layer == null || layerManager == null)
                return;
            double val1 = this.FixedDimensionScale * layerManager.FixedDimensionScale;
            double val2 = 0.02;
            if (this.ChartTypeFromLayerType(this.VisualType) == GeoVisualization.ChartType.Bubble)
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
            this.chartVisualizations.ForEach((Action<ChartVisualization>)(cv =>
            {
                if (object.ReferenceEquals((object)this.DataSource, (object)cv.DataSource))
                    this.DataSource.IncrementReuseCount();
                cv.Removed();
            }));
            this.chartVisualizations.Clear();
            base.Removed();
        }

        internal override void OnVisibleChanged(bool visible)
        {
            LayerDataBinding layerDataBinding = this.DataBinding as LayerDataBinding;
            this.chartVisualizations.ForEach((Action<ChartVisualization>)(cvis => cvis.OnVisibleChanged(visible)));
            if (layerDataBinding != null && (!visible || !this.ShouldRefreshDisplay))
            {
                if (visible)
                    this.Show(CancellationToken.None);
                else
                    this.Hide();
                LayerDefinition layerDefinition = this.LayerDefinition;
                LayerManager layerManager = layerDefinition == null ? (LayerManager)null : layerDefinition.LayerManager;
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
                    foreach (GeoVisualizationInstanceProperties item_0 in newGeo.GetInstancePropertiesArray())
                    {
                        int? local_4 = new int?();
                        if (item_0.ColorSet)
                        {
                            GeoVisualizationInstanceProperties local_5 = hashtable[(object)item_0.ModelId] as GeoVisualizationInstanceProperties;
                            if (local_5 != null)
                            {
                                local_5.Color = item_0.Color;
                                local_5.ColorSet = true;
                            }
                            else
                                hashtable.Add((object)item_0.ModelId, (object)new GeoVisualizationInstanceProperties(item_0.ModelId)
                                {
                                    Color = item_0.Color,
                                    ColorSet = true
                                });
                            local_4 = geoDataSource == null ? new int?() : geoDataSource.GetSeriesIndexForModelDataId(item_0.ModelId);
                        }
                        if (local_4.HasValue)
                        {
                            LayerDataBinding local_7 = this.DataBinding as LayerDataBinding;
                            if (local_7 != null)
                                local_7.UpdateSeriesProperties(local_4.Value, item_0, false);
                        }
                    }
                }
            }
            this.ChartVisualizations = newGeo.ChartVisualizations;
            this.SelectedRegionShadingMode = newGeo.SelectedRegionShadingMode;
            this.SetSelected((InstanceId[])null, true, SelectionStyle.Outline, (object)null);
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
                TableMetadata table1 = fieldWellDefinition.Category is TableColumn ? ((TableMember)fieldWellDefinition.Category).Table : (TableMetadata)null;
                if (table1 != null && fieldWellDefinition.Geo != null && fieldWellDefinition.Geo.GeoColumns.Count > 0 && (fieldWellDefinition.Geo.GeoColumns[0].Table.ContainsLookupTable(table1) || table1.ContainsLookupTable(fieldWellDefinition.Geo.GeoColumns[0].Table)))
                {
                    TableMetadata table2 = fieldWellDefinition.Time is TableColumn ? ((TableMember)fieldWellDefinition.Time).Table : (TableMetadata)null;
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
            GeoQueryResults geoQueryResults = geoDataSource == null ? (GeoQueryResults)null : geoDataSource.QueryResults;
            ModelQueryIndexedKeyColumn indexedKeyColumn = geoQueryResults == null ? (ModelQueryIndexedKeyColumn)null : geoQueryResults.Category;
            List<int> colorIndices = this.ColorIndices;
            if (colorIndices == null)
                return Enumerable.Empty<int>();
            if (geoQueryResults == null || geoDataSource.seriesToColorIndex.Count == 0)
                return Enumerable.Take<int>((IEnumerable<int>)colorIndices, 64);
            int numColorIndices = colorIndices.Count;
            if (indexedKeyColumn == null)
                return Enumerable.Take<int>(Enumerable.Select<int, int>(Enumerable.TakeWhile<int>((IEnumerable<int>)geoDataSource.seriesToColorIndex, (Func<int, bool>)(index => index < numColorIndices)), (Func<int, int>)(index => colorIndices[index])), 64);
            if (indexedKeyColumn.PreservedValuesIndex == null)
                return Enumerable.Empty<int>();
            else
                return Enumerable.Take<int>(Enumerable.Select<int, int>(Enumerable.TakeWhile<int>((IEnumerable<int>)indexedKeyColumn.PreservedValuesIndex, (Func<int, bool>)(index => index < numColorIndices)), (Func<int, int>)(index => colorIndices[index])), 64);
        }

        internal GeoVisualization.SerializableGeoVisualization Wrap()
        {
            GeoVisualization.SerializableGeoVisualization geoVisualization1 = new GeoVisualization.SerializableGeoVisualization();
            geoVisualization1.VisualType = this.VisualType;
            geoVisualization1.VisualShape = this.VisualShape;
            geoVisualization1.VisualShapeForLayerSet = this.VisualShapeForLayer.HasValue;
            GeoVisualization.SerializableGeoVisualization geoVisualization2 = geoVisualization1;
            InstancedShape? visualShapeForLayer = this.VisualShapeForLayer;
            int num1 = visualShapeForLayer.HasValue ? (int)visualShapeForLayer.GetValueOrDefault() : 0;
            geoVisualization2.VisualShapeForLayer = (InstancedShape)num1;
            geoVisualization1.DisplayNegativeValues = this.DisplayNegativeValues;
            geoVisualization1.DisplayNullValues = this.DisplayNullValues;
            geoVisualization1.DisplayZeroValues = this.DisplayZeroValues;
            geoVisualization1.OpacityFactors = this.opacityFactors;
            geoVisualization1.DataDimensionScales = this.dataDimensionScales;
            geoVisualization1.FixedDimensionScales = this.fixedDimensionScales;
            geoVisualization1.LockedViewScales = this.lockedViewScales;
            geoVisualization1.SelectedRegionShadingModeSet = this.SelectedRegionShadingMode.HasValue;
            GeoVisualization.SerializableGeoVisualization geoVisualization3 = geoVisualization1;
            RegionLayerShadingMode? regionShadingMode = this.SelectedRegionShadingMode;
            int num2 = regionShadingMode.HasValue ? (int)regionShadingMode.GetValueOrDefault() : 1;
            geoVisualization3.SelectedRegionShadingMode = (RegionLayerShadingMode)num2;
            geoVisualization1.LayerColorOverrideSet = this.LayerColorOverride.HasValue;
            geoVisualization1.LayerColorOverride = this.LayerColorOverride ?? new Color4F();
            geoVisualization1.HiddenMeasure = this.HiddenMeasure;
            geoVisualization1.GeoWellDefn = this.GeoFieldWellDefinition.Wrap() as GeoFieldWellDefinition.SerializableGeoFieldWellDefinition;
            geoVisualization1.ColorIndices = Enumerable.ToList<int>(this.WrapColorIndices());
            geoVisualization1.GeoInstanceProperties = this.GetInstancePropertiesArray();
            geoVisualization1.ChartVisualizations = Enumerable.ToList<ChartVisualization.SerializableChartVisualization>(Enumerable.Select<ChartVisualization, ChartVisualization.SerializableChartVisualization>((IEnumerable<ChartVisualization>)this.chartVisualizations, (Func<ChartVisualization, ChartVisualization.SerializableChartVisualization>)(cv => cv.Wrap())));
            GeoVisualization.SerializableGeoVisualization geoVisualization4 = geoVisualization1;
            this.SnapState((Visualization.SerializableVisualization)geoVisualization4);
            return geoVisualization4;
        }

        internal void Unwrap(GeoVisualization.SerializableGeoVisualization state, CultureInfo modelCulture)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            if (state.GeoWellDefn == null)
                throw new ArgumentException("state.GeoWellDefn must not be null");
            if (this.FieldWellDefinition != null)
                throw new InvalidOperationException("A FieldWellDefinition has already been set");
            this.SetStateTo((Visualization.SerializableVisualization)state);
            this.DisplayNegativeValues = state.DisplayNegativeValues;
            this.DisplayNullValues = state.DisplayNullValues;
            this.DisplayZeroValues = state.DisplayZeroValues;
            this.VisualType = state.VisualType;
            this.VisualShapeForLayer = !state.VisualShapeForLayerSet ? new InstancedShape?() : new InstancedShape?(state.VisualShapeForLayer);
            this.VisualShape = state.VisualShape;
            if (state.OpacityFactors != null)
                Array.Copy((Array)state.OpacityFactors, (Array)this.opacityFactors, Math.Min(this.opacityFactors.Length, state.OpacityFactors.Length));
            Array.Copy((Array)state.DataDimensionScales, (Array)this.dataDimensionScales, Math.Min(this.dataDimensionScales.Length, state.DataDimensionScales.Length));
            Array.Copy((Array)state.FixedDimensionScales, (Array)this.fixedDimensionScales, Math.Min(this.fixedDimensionScales.Length, state.FixedDimensionScales.Length));
            if (state.LockedViewScales != null)
                Array.Copy((Array)state.LockedViewScales, (Array)this.lockedViewScales, Math.Min(this.lockedViewScales.Length, state.LockedViewScales.Length));
            this.OpacityFactor = this.opacityFactors[(int)this.ChartTypeFromLayerType(this.VisualType)];
            this.DataDimensionScale = this.dataDimensionScales[(int)this.ChartTypeFromLayerType(this.VisualType)];
            this.FixedDimensionScale = this.fixedDimensionScales[(int)this.ChartTypeFromLayerType(this.VisualType)];
            this.SelectedRegionShadingMode = !state.SelectedRegionShadingModeSet ? new RegionLayerShadingMode?() : new RegionLayerShadingMode?(state.SelectedRegionShadingMode);
            this.LayerColorOverride = !state.LayerColorOverrideSet ? new Color4F?() : new Color4F?(state.LayerColorOverride);
            this.HiddenMeasure = state.HiddenMeasure;
            this.FieldWellDefinition = state.GeoWellDefn.Unwrap((Visualization)this, modelCulture);
            this.ColorIndices = state.ColorIndices;
            this.SetInstancePropertiesFromArray(state.GeoInstanceProperties);
            this.Initialize();
            if (state.ChartVisualizations == null)
                return;
            this.chartVisualizations.AddRange(Enumerable.Select<ChartVisualization.SerializableChartVisualization, ChartVisualization>((IEnumerable<ChartVisualization.SerializableChartVisualization>)state.ChartVisualizations, (Func<ChartVisualization.SerializableChartVisualization, ChartVisualization>)(cv => cv.Unwrap(this.LayerDefinition, (DataSource)this.GeoDataSource, modelCulture))));
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
                this.chartVisualizations = (List<ChartVisualization>)null;
            }
            if (this.VisualizationModel != null && this.VisualizationModel.TimeController != null)
                this.VisualizationModel.TimeController.PropertyChanged -= new PropertyChangedEventHandler(this.onTimeControllerPropertyChanged.OnEvent);
            Hashtable hashtable = this.instancePropertiesLookUpByModelID;
            if (hashtable != null)
            {
                lock (hashtable.SyncRoot)
                    hashtable.Clear();
            }
            this.instancePropertiesLookUpByModelID = (Hashtable)null;
            this.visualShapeForLayer = new InstancedShape?();
            this.selectionStats = (SelectionStats)null;
            this.opacityFactors = (double[])null;
            this.dataDimensionScales = (double[])null;
            this.fixedDimensionScales = (double[])null;
            this.lockedViewScales = (double[])null;
            this.LayerAdded = (Action<Layer>)null;
            this.LayerRemoved = (Action<Layer>)null;
            this.DataUpdateStarted = (Action)null;
            this.DataUpdateCompleted = (Action<bool>)null;
            this.DisplayPropertiesChanged = (Action<LayerManager.Settings>)null;
            this.ColorsChanged = (Action)null;
            this.RegionShadingModeChanged = (Action<RegionLayerShadingMode>)null;
            this.LayerScalesChanged = (Action)null;
            if (this.CompletionStats != null)
            {
                this.CompletionStats.Shutdown();
                this.CompletionStats = (CompletionStats)null;
            }
            this.ColorIndices = (List<int>)null;
            base.Shutdown();
        }

        protected override DataBinding CreateDataBinding()
        {
            if (this.ColorIndices == null)
                this.ColorIndices = new List<int>();
            return (DataBinding)new LayerDataBinding(this, (Layer)null, this.DataSource.CreateDataView(DataView.DataViewType.Excel), this.VisualizationModel.LatLonProvider, this.VisualizationModel.TwoDRenderThread, this.ColorIndices);
        }

        protected override object OnBeginRefresh(bool visible, LayerManager.Settings layerManagerSettings)
        {
            this.CompletionStats = new CompletionStats();
            this.NotifyDataUpdateStarted();
            return (object)new
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
                LayerDefinition local_0 = this.LayerDefinition;
                if (local_0 == null || !this.Visible || !local_0.Visible)
                    return;
                LayerDataBinding local_1 = this.DataBinding as LayerDataBinding;
                if (local_1 == null)
                    return;
                Layer local_2 = local_1.Layer;
                bool local_3 = local_2 == null || !local_2.CanSetLayerType(this.VisualType);
                bool local_4 = !local_3;
                if (local_2 != null && local_3)
                {
                    if (this.LayerRemoved != null)
                        this.LayerRemoved(local_2);
                    this.HideWorker();
                    local_1.ClearVisualElement();
                }
                cancellationToken.ThrowIfCancellationRequested();
                if (local_4)
                {
                    local_2.TrySetLayerType(this.VisualType);
                    this.SetOpacityFactor();
                    this.SetDataDimensionScale();
                    this.SetFixedDimensionScale();
                    this.SetLockScales();
                    ChartLayer local_6 = local_2 as ChartLayer;
                    if (local_6 != null)
                        local_6.SetShapes(new InstancedShape[1]
            {
              this.VisualShape
            });
                }
                else
                {
                    Layer local_7;
                    if (this.VisualType == LayerType.HeatMapChart)
                        local_7 = (Layer)new HeatMapLayer();
                    else if (this.VisualType == LayerType.RegionChart)
                        local_7 = (Layer)new RegionLayer(this.VisualizationModel.RegionProvider, (IInstanceIdRelationshipProvider)this.GeoDataSource);
                    else
                        local_7 = (Layer)new ChartLayer(new InstancedShape[1]
            {
              this.VisualShape
            }, this.VisualType, (IInstanceIdRelationshipProvider)this.GeoDataSource);
                    cancellationToken.ThrowIfCancellationRequested();
                    local_1.Layer = local_7;
                    local_7.DisplayNullValues = this.DisplayNullValues;
                    local_7.DisplayZeroValues = this.DisplayZeroValues;
                    local_7.DisplayNegativeValues = this.DisplayNegativeValues;
                    this.SetOpacityFactor();
                    this.SetDataDimensionScale();
                    this.SetFixedDimensionScale();
                    int temp_66 = (int)this.ChartTypeFromLayerType(this.VisualType);
                    this.SetLockScales();
                    if (this.LayerAdded != null)
                        this.LayerAdded(local_7);
                    HitTestableLayer local_9 = local_1.Layer as HitTestableLayer;
                    if (local_9 != null)
                        local_9.OnSelectionChanged += new EventHandler<SelectionEventArgs>(this.SelectionsChanged);
                }
                cancellationToken.ThrowIfCancellationRequested();
                local_1.CompletionStats = this.CompletionStats;
                cancellationToken.ThrowIfCancellationRequested();
                VisualizationModel local_10 = this.VisualizationModel;
                if (local_10 == null)
                    return;
                local_10.Engine.AddLayer(local_1.Layer);
            }
        }

        protected override void Hide()
        {
            lock (this.showHideLock)
            {
                LayerDefinition local_0 = this.LayerDefinition;
                if (local_0 == null || this.Visible && local_0.Visible)
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
                hitTestableLayer.OnSelectionChanged -= new EventHandler<SelectionEventArgs>(this.SelectionsChanged);
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
                        selectedIds = (IList<InstanceId>)hitTestableLayer.GetSelected();
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
                                foreach (InstanceId id1 in (IEnumerable<InstanceId>)selectedIds)
                                {
                                    foreach (InstanceId id2 in geoDataSource.GetRelatedIdsOverTime(id1))
                                    {
                                        double? valueForIdAtTime = geoDataSource.GetValueForIdAtTime(id2, currentTime);
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
                            LayerManager layerManager = layerDefinition == null ? (LayerManager)null : layerDefinition.LayerManager;
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
            LayerManager layerManager = layerDefinition == null ? (LayerManager)null : layerDefinition.LayerManager;
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
            geoViz.UpdateSelectionStatistics((IList<InstanceId>)null);
        }

        private GeoVisualization.ChartType ChartTypeFromLayerType(LayerType layerType)
        {
            switch (layerType)
            {
                case LayerType.PointMarkerChart:
                case LayerType.ColumnChart:
                case LayerType.ClusteredColumnChart:
                case LayerType.StackedColumnChart:
                    return GeoVisualization.ChartType.Column;
                case LayerType.BubbleChart:
                case LayerType.PieChart:
                    return GeoVisualization.ChartType.Bubble;
                case LayerType.HeatMapChart:
                    return GeoVisualization.ChartType.HeatMap;
                case LayerType.RegionChart:
                    return GeoVisualization.ChartType.Region;
                default:
                    return GeoVisualization.ChartType.NumChartTypes;
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
                hashtable.Values.CopyTo((Array)instancePropertiesArray, 0);
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
                foreach (GeoVisualizationInstanceProperties item_0 in instancePropertiesArray)
                {
                    item_0.Repair();
                    hashtable.Add((object)item_0.ModelId, (object)item_0);
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
        public class SerializableGeoVisualization : Visualization.SerializableVisualization
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
                else
                    return new GeoVisualization(this, layerDefinition, dataSource as GeoDataSource, modelCulture);
            }
        }
    }
}
