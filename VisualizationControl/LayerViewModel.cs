using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class LayerViewModel : ViewModelBase
    {
        private bool _Visible = true;
        private LayerLegendDecoratorModel _Legend = new LayerLegendDecoratorModel();
        private int refreshCount;
        private string _Name;
        private FieldListPickerViewModel _FieldListPicker;
        private LayerDefinition _LayerDefinition;
        private Tuple<long, InstanceId?> _HoveredElement;
        private IList<InstanceId> _SelectedElements;
        private ICommand _DeleteLayerCommand;
        private ICommand _LayerSettingsCommand;
        private bool _UserClosedLegend;
        private LayerManagerViewModel _layerManagerVM;
        private IDialogServiceProvider _dialogProvider;

        public string PropertyName
        {
            get
            {
                return "Name";
            }
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (!this.SetProperty(this.PropertyName, ref this._Name, value, false))
                    return;
                if (this.LayerDefinition != null)
                    this.LayerDefinition.Name = value;
                if (this.Legend != null)
                    this.Legend.LayerName = value;
                if (this.FieldListPicker == null || this.FieldListPicker.FieldWellGeographyViewModel == null || this.FieldListPicker.FieldWellGeographyViewModel.GeocodingReport == null)
                    return;
                this.FieldListPicker.FieldWellGeographyViewModel.GeocodingReport.LayerName = value;
            }
        }

        public string PropertyFieldListPicker
        {
            get
            {
                return "FieldListPicker";
            }
        }

        public FieldListPickerViewModel FieldListPicker
        {
            get
            {
                return this._FieldListPicker;
            }
            set
            {
                this.SetProperty(this.PropertyFieldListPicker, ref this._FieldListPicker, value, false);
            }
        }

        public string PropertyLayerDefinition
        {
            get
            {
                return "LayerDefinition";
            }
        }

        public LayerDefinition LayerDefinition
        {
            get
            {
                return this._LayerDefinition;
            }
            private set
            {
                if (this._LayerDefinition != null && this._LayerDefinition != value)
                {
                    GeoVisualization geoVisualization = this._LayerDefinition.GeoVisualization;
                    this.DetachOnHoverHandler();
                    if (geoVisualization != null)
                    {
                        geoVisualization.LayerAdded -= this.AttachOnHoverHandler;
                        geoVisualization.LayerRemoved -= this.DetachOnHoverHandler;
                        geoVisualization.DataUpdateCompleted -= this.DataUpdateCompleted;
                        geoVisualization.LayerScalesChanged -= this.LayerScalesChanged;
                        geoVisualization.DisplayPropertiesChanged -= this.DisplayPropertiesChanged;
                        geoVisualization.ColorsChanged -= this.ColorsChanged;
                    }
                }
                if (!this.SetProperty(this.PropertyLayerDefinition, ref this._LayerDefinition, value, false) || value == null)
                    return;
                GeoVisualization geoVisualization1 = this._LayerDefinition.GeoVisualization;
                this.AttachOnHoverHandler(geoVisualization1 == null ? null : geoVisualization1.Layer);
                if (geoVisualization1 != null)
                {
                    geoVisualization1.LayerAdded += this.AttachOnHoverHandler;
                    geoVisualization1.LayerRemoved += this.DetachOnHoverHandler;
                    geoVisualization1.DataUpdateCompleted += this.DataUpdateCompleted;
                    geoVisualization1.LayerScalesChanged += this.LayerScalesChanged;
                    geoVisualization1.DisplayPropertiesChanged += this.DisplayPropertiesChanged;
                    geoVisualization1.ColorsChanged += this.ColorsChanged;
                }
                this.Name = value.Name;
                GeoVisualization geoVisualization2 = value.GeoVisualization;
                bool flag = geoVisualization2 == null || geoVisualization2.Visible;
                this.Visible = value.Visible && flag;
                this.Legend.LayerId = value.Id;
            }
        }

        public static string PropertyHoveredElement
        {
            get
            {
                return "HoveredElement";
            }
        }

        public Tuple<long, InstanceId?> HoveredElement
        {
            get
            {
                return this._HoveredElement;
            }
            set
            {
                this.SetProperty(LayerViewModel.PropertyHoveredElement, ref this._HoveredElement, value, false);
            }
        }

        public static string PropertySelectedElements
        {
            get
            {
                return "SelectedElements";
            }
        }

        public IList<InstanceId> SelectedElements
        {
            get
            {
                return this._SelectedElements;
            }
            set
            {
                this.SetProperty(LayerViewModel.PropertySelectedElements, ref this._SelectedElements, value, false);
            }
        }

        public string PropertyVisible
        {
            get
            {
                return "Visible";
            }
        }

        public bool Visible
        {
            get
            {
                return this._Visible;
            }
            set
            {
                this.SetVisibility(value, null);
            }
        }

        public string PropertyDeleteLayerCommand
        {
            get
            {
                return "DeleteLayerCommand";
            }
        }

        public ICommand DeleteLayerCommand
        {
            get
            {
                return this._DeleteLayerCommand;
            }
            set
            {
                this.SetProperty(this.PropertyDeleteLayerCommand, ref this._DeleteLayerCommand, value, false);
            }
        }

        public string PropertyLayerSettingsCommand
        {
            get
            {
                return "LayerSettingsCommand";
            }
        }

        public ICommand LayerSettingsCommand
        {
            get
            {
                return this._LayerSettingsCommand;
            }
            set
            {
                this.SetProperty(this.PropertyLayerSettingsCommand, ref this._LayerSettingsCommand, value, false);
            }
        }

        public InstancedShape? VisualShape
        {
            get
            {
                LayerDefinition layerDefinition = this.LayerDefinition;
                GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
                if (geoVisualization != null)
                    return geoVisualization.VisualShapeForLayer;
                return new InstancedShape?();
            }
            set
            {
                LayerDefinition layerDefinition = this.LayerDefinition;
                GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
                if (geoVisualization == null)
                    return;
                geoVisualization.VisualShapeForLayer = value;
            }
        }

        public bool CanVisualShapeBeChanged
        {
            get
            {
                return this.FieldListPicker.CanVisualShapeBeChanged;
            }
        }

        public string PropertyLegend
        {
            get
            {
                return "Legend";
            }
        }

        public LayerLegendDecoratorModel Legend
        {
            get
            {
                return this._Legend;
            }
        }

        public string PropertyUserClosedLegend
        {
            get
            {
                return "UserClosedLegend";
            }
        }

        public bool UserClosedLegend
        {
            get
            {
                return this._UserClosedLegend;
            }
            set
            {
                this.SetProperty(this.PropertyUserClosedLegend, ref this._UserClosedLegend, value, false);
            }
        }

        public StatusBarViewModel StatusBar
        {
            get
            {
                if (this._layerManagerVM != null)
                    return this._layerManagerVM.StatusBar;
                return null;
            }
        }

        public LayerManagerViewModel LayerManagerViewModel
        {
            get
            {
                return this._layerManagerVM;
            }
        }

        public bool HasTables { get; private set; }

        internal CancellationTokenSource CancellationSource { get; private set; }

        public event Action OnBeforeVisualizing;

        public LayerViewModel(LayerDefinition layerDef, LayerManagerViewModel layerManagerVM, IEnumerable<TableIsland> tableIslands, IDialogServiceProvider dialogProvider = null)
        {
            if (layerDef == null)
                throw new ArgumentNullException("layerDef");
            if (layerManagerVM == null)
                throw new ArgumentNullException("layerManagerVM");
            if (layerDef.GeoVisualization == null)
                throw new ArgumentException("GeoVisualization is null");
            this._dialogProvider = dialogProvider;
            this._layerManagerVM = layerManagerVM;
            this.LayerDefinition = layerDef;
            this.VisualShape = layerDef.GeoVisualization.VisualShapeForLayer;
            this._layerManagerVM.Model.Model.ColorSelector.ColorsChanged += this.ColorsChanged;
            this.HasTables = tableIslands != null && tableIslands.Any();
            this.FieldListPicker = new FieldListPickerViewModel(tableIslands, this, layerDef.GeoVisualization, dialogProvider);
            this.FieldListPicker.VisualizeCallback = this.OnVisualize;
            this.FieldListPicker.CancelVisualizeCallback = this.CancelVisualize;
            this.FieldListPicker.InitializeToAutoGeocode(tableIslands, layerDef, layerDef.ModelTableNameForAutoGeocoding);
            this._layerManagerVM.Model.Model.UIDispatcher.CheckedInvoke(() => this.RefreshLegend(), false);
            this.UpdateGeocodingReport(false);
        }

        public LayerViewModel()
        {
            this.FieldListPicker = new FieldListPickerViewModel(null, null);
            this.FieldListPicker.VisualizeCallback = this.OnVisualize;
            this.FieldListPicker.CancelVisualizeCallback = this.CancelVisualize;
        }

        public void Removed()
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
            if (geoVisualization != null)
            {
                geoVisualization.LayerAdded -= this.AttachOnHoverHandler;
                geoVisualization.LayerRemoved -= this.DetachOnHoverHandler;
                geoVisualization.DataUpdateCompleted -= this.DataUpdateCompleted;
                geoVisualization.DisplayPropertiesChanged -= this.DisplayPropertiesChanged;
                geoVisualization.LayerScalesChanged -= this.LayerScalesChanged;
                geoVisualization.ColorsChanged -= this.ColorsChanged;
            }
            this.FieldListPicker.UnInitialize();
            this._layerManagerVM.Model.Model.ColorSelector.ColorsChanged -= this.ColorsChanged;
            this.DetachOnHoverHandler();
        }

        public bool HasLegendData()
        {
            if (this.FieldListPicker.FieldWellVisualizationViewModel.SelectedCategory.Value == null)
                return this.FieldListPicker.FieldWellVisualizationViewModel.HeightFields.Count >= 1;
            return true;
        }

        private void SetVisibility(bool value, LayerManager.Settings settings = null)
        {
            if (!this.SetProperty(this.PropertyVisible, ref this._Visible, value, false))
                return;
            LayerDefinition layerDefinition = this.LayerDefinition;
            GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
            if (geoVisualization == null)
                return;
            geoVisualization.Visible = value;
            if (!value || !geoVisualization.ShouldRefreshDisplay)
                return;
            this.RefreshDisplay(false, settings);
        }

        private void CancelVisualize()
        {
            CancellationTokenSource cancellationSource = this.CancellationSource;
            if (cancellationSource == null)
                return;
            cancellationSource.Cancel();
        }

        private void OnVisualize(FieldWellGeographyViewModel geographyVM, Filter filters,
            FieldWellVisualizationViewModel visualizationVM, bool zoomToData, FieldListPickerState fieldListPickerState,
            bool clearMap)
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            GeoVisualization visualization = (layerDefinition == null) ? null : layerDefinition.GeoVisualization;
            GeoFieldWellDefinition wellDefinition = (visualization == null) ? null : visualization.GeoFieldWellDefinition;
            if (wellDefinition == null) return;
            GeoField field;
            if (this.OnBeforeVisualizing != null)
            {
                this.OnBeforeVisualizing();
            }
            wellDefinition.ChosenGeoMappings.Clear();
            wellDefinition.ChosenGeoFields.Clear();
            GeoFieldMappingViewModel mapBy = null;
            if (!clearMap)
            {
                field = this.BuildGeoField(geographyVM.GeoFieldMappings, out mapBy);
                foreach (GeoFieldMappingViewModel model2 in geographyVM.GeoFieldMappings)
                {
                    wellDefinition.ChosenGeoMappings.Add(model2.MappingType.ToGeoMappingType());
                }
            }
            else
            {
                field = new GeoEntityField("Unused");
                foreach (GeoFieldMappingViewModel model3 in geographyVM.GeoFieldMappings)
                {
                    TableColumn model = model3.Field.Model as TableColumn;
                    if (model != null)
                    {
                        wellDefinition.ChosenGeoFields.Add(model);
                        wellDefinition.ChosenGeoMappings.Add(model3.MappingType.ToGeoMappingType());
                    }
                }
            }
            wellDefinition.UserSelectedMapByField = (mapBy != null) && mapBy.UserSelectedMapByField;
            if (field == null) return;
            InstancedShape? visualShape = this.VisualShape;
            switch (this.FieldListPicker.VisualizationType)
            {
                case LayerType.PointMarkerChart:
                    visualShape =
                        new InstancedShape?((visualizationVM.ChartType == VisualizationChartType.Bubble)
                            ? InstancedShape.CircularCone
                            : InstancedShape.InvertedPyramid);
                    break;

                case LayerType.BubbleChart:
                    {
                        InstancedShape? shape = visualShape;
                        visualShape =
                            new InstancedShape?(shape.HasValue
                                ? shape.GetValueOrDefault()
                                : InstancedShape.Circle);
                        break;
                    }
                case LayerType.ColumnChart:
                    {
                        InstancedShape? shape = visualShape;
                        visualShape =
                            new InstancedShape?(shape.HasValue
                                ? shape.GetValueOrDefault()
                                : InstancedShape.Square);
                        break;
                    }
                default:
                    {
                        InstancedShape? shape = visualShape;
                        visualShape =
                            new InstancedShape?(shape.HasValue
                                ? shape.GetValueOrDefault()
                                : InstancedShape.Square);
                        break;
                    }
            }
            visualization.BeginSettingsUpdates();
            if ((visualization.VisualType != this.FieldListPicker.VisualizationType) ||
                (visualization.VisualShape != visualShape.Value))
            {
                visualization.VisualType = this.FieldListPicker.VisualizationType;
                visualization.VisualShape = visualShape.Value;
            }
            wellDefinition.SetGeo(field);
            List<Tuple<TableField, AggregationFunction>> second =
                new List<Tuple<TableField, AggregationFunction>>(visualizationVM.HeightFields.Count);
            foreach (FieldWellHeightViewModel height in visualizationVM.HeightFields)
            {
                second.Add(new Tuple<TableField, AggregationFunction>(height.TableField.Model,
                    height.AggregationFunction));
            }
            if (((second.Count == 0) && visualizationVM.HasCategory) && this.FieldListPicker.DisplayCategoryCounts)
            {
                visualization.HiddenMeasure = true;
                second.Add(
                    new Tuple<TableField, AggregationFunction>(
                        visualizationVM.SelectedCategory.Value.TableField.Model, AggregationFunction.Count));
            }
            else
            {
                visualization.HiddenMeasure = false;
            }
            if (
                wellDefinition.Measures.Except(second).Any() ||
                second.Except(wellDefinition.Measures).Any())
            {
                wellDefinition.RemoveAllMeasures();
                second.ForEach(
                    measure => wellDefinition.AddMeasure(measure));
            }
            wellDefinition.SetTime((visualizationVM.SelectedTimeField.Value == null)
                ? null
                : visualizationVM.SelectedTimeField.Value.TableField.Model);
            if (visualizationVM.SelectedTimeField.Value != null)
            {
                bool flag = visualizationVM.AccumulateResultsOverTime && (second.Count > 0);
                bool flag2 = visualizationVM.PersistTimeData || flag;
                wellDefinition.AccumulateResultsOverTime = flag;
                wellDefinition.Decay = flag2
                    ? GeoFieldWellDefinition.PlaybackValueDecayType.HoldTillReplaced
                    : GeoFieldWellDefinition.PlaybackValueDecayType.None;
                wellDefinition.ChunkBy = visualizationVM.SelectedTimeField.Value.TimeChunk;
            }
            else
            {
                wellDefinition.AccumulateResultsOverTime = false;
                wellDefinition.Decay = GeoFieldWellDefinition.PlaybackValueDecayType.None;
                wellDefinition.ChunkBy = TimeChunkPeriod.None;
            }
            wellDefinition.ViewModelPersistTimeData = visualizationVM.PersistTimeData;
            wellDefinition.ViewModelAccumulateResultsOverTime = visualizationVM.AccumulateResultsOverTime;
            wellDefinition.UserSelectedTimeSetting = visualizationVM.UserSelectedTimeSetting;
            wellDefinition.SetCategory((visualizationVM.SelectedCategory.Value == null)
                ? null
                : visualizationVM.SelectedCategory.Value.TableField.Model);
            wellDefinition.ChoosingGeoFields = fieldListPickerState == FieldListPickerState.ChooseGeoFields;
            wellDefinition.SetFilter(filters);
            visualization.EndSettingsUpdates();
            this.RefreshDisplay(zoomToData, null);
        }

        private void RefreshDisplay(bool zoomToData, LayerManager.Settings settings = null)
        {
            this.CancelVisualize();
            this.CancellationSource = new CancellationTokenSource();
            int num = Interlocked.Increment(ref this.refreshCount);
            this.FieldListPicker.SetGeocodingReport(null);
            this.LayerDefinition.RefreshDisplay(zoomToData, this.CancellationSource, false, false, settings, this.VisualizeCompleted, num);
        }

        private void VisualizeCompleted(object context, bool succeeded, bool cancelled, Exception exception)
        {
            if ((succeeded || exception == null || (int)context != this.refreshCount) ||
                this._dialogProvider == null) return;
            ConfirmationDialogViewModel dialog = new ConfirmationDialogViewModel
            {
                Title = Resources.Product,
                Description = Resources.QueryExecutionError
            };
            DelegatedCommand item = new DelegatedCommand(() => this._dialogProvider.DismissDialog(dialog))
            {
                Name = Resources.Dialog_OkayText
            };
            dialog.Commands.Add(item);
            this._dialogProvider.ShowDialog(dialog);
        }

        private void RefreshLegend()
        {
            this.Legend.LegendItems.RemoveAll();
            LayerDefinition layerDefinition = this.LayerDefinition;
            GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
            if (geoVisualization == null || this.FieldListPicker == null || this.FieldListPicker.FieldWellVisualizationViewModel == null)
                return;
            this.Legend.LayerName = this.Name;
            this.Legend.ChartType = this.FieldListPicker.FieldWellVisualizationViewModel.ChartType;
            this.Legend.RegionShadingMode = this.FieldListPicker.FieldWellVisualizationViewModel.SelectedRegionShadingSetting;
            bool displayValueAsPercentage = this.Legend.RegionShadingMode == RegionLayerShadingMode.Local;
            HeatMapLayer heatMapLayer = geoVisualization.Layer as HeatMapLayer;
            if (heatMapLayer != null)
            {
                this.Legend.Minimum = heatMapLayer.HeatmapMinValue;
                this.Legend.Maximum = heatMapLayer.HeatmapMaxValue;
            }
            else if (geoVisualization.Layer != null)
            {
                this.Legend.Minimum = geoVisualization.Layer.MinValue;
                this.Legend.Maximum = geoVisualization.Layer.MaxValue;
            }
            else
            {
                this.Legend.Minimum = double.NaN;
                this.Legend.Maximum = double.NaN;
            }
            IEnumerable<string> categories = geoVisualization.Categories;
            if (this.FieldListPicker.FieldWellVisualizationViewModel.SelectedCategory.Value != null && categories != null)
            {
                foreach (string str in categories)
                {
                    System.Windows.Media.Color color = geoVisualization.ColorForCategory(str).ToWindowsColor();
                    double? min;
                    double? max;
                    geoVisualization.RegionChartBoundsForCategory(str, out min, out max);
                    if (!this.Legend.TryAddLegendItem(new LayerLegendItemModel(str, color, min, max, displayValueAsPercentage, false)))
                        break;
                }
            }
            else if (this.FieldListPicker.FieldWellVisualizationViewModel.HeightFields.Count > 0)
            {
                foreach (FieldWellHeightViewModel wellHeightViewModel in this.FieldListPicker.FieldWellVisualizationViewModel.HeightFields)
                {
                    System.Windows.Media.Color color = geoVisualization.ColorForMeasure(wellHeightViewModel.TableField.Model, wellHeightViewModel.AggregationFunction).ToWindowsColor();
                    double? min;
                    double? max;
                    geoVisualization.RegionChartBoundsForMeasure(wellHeightViewModel.TableField.Model, wellHeightViewModel.AggregationFunction, out min, out max);
                    this.Legend.TryAddLegendItem(new LayerLegendItemModel(wellHeightViewModel.DisplayString, color, min, max, displayValueAsPercentage, false));
                }
            }
            else
            {
                Color4F? nullable = geoVisualization.LayerColor();
                if (!nullable.HasValue)
                    return;
                this.Legend.TryAddLegendItem(new LayerLegendItemModel(this.Name, nullable.Value.ToWindowsColor(), new double?(), new double?(), false, true));
            }
        }

        private void DataUpdateCompleted(bool geocodingCancelled)
        {
            this.UpdateGeocodingReport(geocodingCancelled);
            this._layerManagerVM.Model.Model.UIDispatcher.CheckedInvoke(() => this.RefreshLegend(), true);
        }

        private void LayerScalesChanged()
        {
            this._layerManagerVM.Model.Model.UIDispatcher.CheckedInvoke(() => this.RefreshLegend(), true);
        }

        private void DisplayPropertiesChanged(LayerManager.Settings settings)
        {
            this._layerManagerVM.Model.Model.UIDispatcher.CheckedInvoke(() => this.RefreshLegend(), true);
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null)
                return;
            GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
            this.Name = layerDefinition.Name;
            this.SetVisibility(layerDefinition.Visible && (geoVisualization == null || geoVisualization.Visible), settings);
        }

        private void ColorsChanged()
        {
            this._layerManagerVM.Model.Model.UIDispatcher.CheckedInvoke(() => this.RefreshLegend(), true);
        }

        private void UpdateGeocodingReport(bool geocodingCancelled)
        {
            FieldListPickerViewModel fieldListPicker = this.FieldListPicker;
            if (fieldListPicker == null)
            {
                Thread.Sleep(50);
                fieldListPicker = this.FieldListPicker;
                if (fieldListPicker == null)
                    return;
            }
            if (!geocodingCancelled)
            {
                GeocodingReportViewModel geocodingReport = null;
                float confidencePercetage = 0.0f;
                LayerDefinition layerDefinition = this.LayerDefinition;
                GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
                List<GeoAmbiguity> ambiguities = geoVisualization == null ? null : geoVisualization.GetGeoAmbiguities(out confidencePercetage);
                if (ambiguities != null)
                    geocodingReport = new GeocodingReportViewModel(ambiguities, confidencePercetage)
                    {
                        LayerName = this.Name
                    };
                fieldListPicker.SetGeocodingReport(geocodingReport);
            }
            else
                fieldListPicker.SetGeocodingReport(null);
        }

        private void DetachOnHoverHandler()
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
            if (geoVisualization == null)
                return;
            this.DetachOnHoverHandler(geoVisualization.Layer);
        }

        private void DetachOnHoverHandler(Layer layer)
        {
            HitTestableLayer hitTestableLayer = layer as HitTestableLayer;
            this.SelectedElements = null;
            if (hitTestableLayer == null)
                return;
            hitTestableLayer.OnHoveredElementChanged -= this.Layer_OnHoveredElementChanged;
            hitTestableLayer.OnSelectionChanged -= this.Layer_OnSelectionChanged;
        }

        private void AttachOnHoverHandler(Layer layer)
        {
            HitTestableLayer hitTestableLayer = layer as HitTestableLayer;
            if (hitTestableLayer == null)
                return;
            hitTestableLayer.OnHoveredElementChanged += this.Layer_OnHoveredElementChanged;
            hitTestableLayer.OnSelectionChanged += this.Layer_OnSelectionChanged;
        }

        private void Layer_OnSelectionChanged(object sender, SelectionEventArgs e)
        {
            if (e != null)
            {
                this.SelectedElements = e.SelectedIds;
                if (!this.SelectedElements.Any())
                    return;
                LayerDefinition layerDefinition = this.LayerDefinition;
                GeoVisualization geoVisualization = layerDefinition == null ? null : layerDefinition.GeoVisualization;
                if (geoVisualization == null)
                    return;
                this._layerManagerVM.SetSelectionContext(this, geoVisualization.GetSeriesIndexForInstanceId(this.SelectedElements.Last(), false));
            }
            else
                this.SelectedElements = null;
        }

        private void Layer_OnHoveredElementChanged(object sender, HoveredElementEventArgs e)
        {
            this.HoveredElement = Tuple.Create(e.CurrentFrame, e.HoveredElement);
        }

        private GeoField BuildGeoField(IEnumerable<GeoFieldMappingViewModel> geoFieldMappings, out GeoFieldMappingViewModel mapBy)
        {
            TableColumn tableColumn1 = null;
            TableColumn tableColumn2 = null;
            TableColumn xCoord = null;
            TableColumn yCoord = null;
            TableColumn addressLine = null;
            TableColumn locality = null;
            TableColumn adminDistrict2 = null;
            TableColumn adminDistrict = null;
            TableColumn postalCode = null;
            TableColumn country = null;
            TableColumn tableColumn3 = null;
            TableColumn tableColumn4 = null;
            mapBy = null;
            foreach (GeoFieldMappingViewModel mappingViewModel in geoFieldMappings)
            {
                TableColumn tableColumn5 = mappingViewModel.Field.Model as TableColumn;
                switch (mappingViewModel.MappingType)
                {
                    case GeoFieldMappingType.Latitude:
                        tableColumn1 = tableColumn5;
                        break;
                    case GeoFieldMappingType.Longitude:
                        tableColumn2 = tableColumn5;
                        break;
                    case GeoFieldMappingType.Address:
                        tableColumn3 = tableColumn5;
                        break;
                    case GeoFieldMappingType.Other:
                        tableColumn4 = tableColumn5;
                        break;
                    case GeoFieldMappingType.Street:
                        addressLine = tableColumn5;
                        break;
                    case GeoFieldMappingType.City:
                        locality = tableColumn5;
                        break;
                    case GeoFieldMappingType.County:
                        adminDistrict2 = tableColumn5;
                        break;
                    case GeoFieldMappingType.State:
                        adminDistrict = tableColumn5;
                        break;
                    case GeoFieldMappingType.Zip:
                        postalCode = tableColumn5;
                        break;
                    case GeoFieldMappingType.Country:
                        country = tableColumn5;
                        break;
                    case GeoFieldMappingType.XCoord:
                        xCoord = tableColumn5;
                        break;
                    case GeoFieldMappingType.YCoord:
                        yCoord = tableColumn5;
                        break;
                }
                if (mappingViewModel.IsMapByField)
                {
                    if (mapBy != null)
                        return null;
                    mapBy = mappingViewModel;
                }
            }
            if (mapBy == null)
                return null;
            if (mapBy.Field.Model == tableColumn2 || mapBy.Field.Model == tableColumn1)
            {
                if (tableColumn1 == null || tableColumn2 == null)
                    return null;
                return new LatLongField("LatLon", false, tableColumn1, tableColumn2, xCoord, yCoord, addressLine, locality, adminDistrict2, adminDistrict, postalCode, country, tableColumn3, tableColumn4, false);
            }
            if (mapBy.Field.Model == xCoord || mapBy.Field.Model == yCoord)
            {
                if (xCoord == null || yCoord == null)
                    return null;
                return new LatLongField("GeoXY", true, tableColumn1, tableColumn2, xCoord, yCoord, addressLine, locality, adminDistrict2, adminDistrict, postalCode, country, tableColumn3, tableColumn4, false);
            }
            if (mapBy.Field.Model == tableColumn3)
                return new GeoFullAddressField("GeoFullAddress", tableColumn3, null, addressLine, locality, adminDistrict2, adminDistrict, postalCode, country, tableColumn1, tableColumn2, xCoord, yCoord, null, tableColumn4, false);
            if (mapBy.Field.Model == tableColumn4)
                return new GeoFullAddressField("GeoOtherLocationDescription", null, tableColumn4, addressLine, locality, adminDistrict2, adminDistrict, postalCode, country, tableColumn1, tableColumn2, xCoord, yCoord, tableColumn3, null, false);
            return new GeoEntityField("GeoEntity", mapBy.MappingType.GeoEntityLevel(), addressLine, locality, adminDistrict2, adminDistrict, postalCode, country, tableColumn1, tableColumn2, xCoord, yCoord, tableColumn3, tableColumn4, false);
        }
    }
}
