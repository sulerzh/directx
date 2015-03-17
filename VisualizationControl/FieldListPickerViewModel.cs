using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class FieldListPickerViewModel : ViewModelBase
    {
        private bool _processGeographyIslandTableFieldSelectionChanges = true;
        private FieldListPickerState _state;
        private string _InstructionText;
        private ViewModelBase _FieldWell;
        private ViewModelBase _FieldWellSummary;
        private GeoVisualization _geoVisualization;
        private LayerViewModel _layerViewModel;

        public FieldWellGeographyViewModel FieldWellGeographyViewModel { get; set; }

        public FieldWellVisualizationViewModel FieldWellVisualizationViewModel { get; private set; }

        public Action<FieldWellGeographyViewModel, Filter, FieldWellVisualizationViewModel, bool, FieldListPickerState, bool> VisualizeCallback { get; set; }

        public Action CancelVisualizeCallback { get; set; }

        public ObservableCollectionEx<TableIslandViewModel> TableIslandsForGeography { get; private set; }

        public ObservableCollectionEx<TableIslandViewModel> TableIslandsForVisualization { get; private set; }

        public ObservableCollectionEx<TableIslandViewModel> TableIslandsForFiltering { get; private set; }

        public static string PropertyState
        {
            get
            {
                return "State";
            }
        }

        public FieldListPickerState State
        {
            get
            {
                return this._state;
            }
            set
            {
                this.SetProperty(PropertyState, ref this._state, value, false);
            }
        }

        public string PropertyInstructionText
        {
            get
            {
                return "InstructionText";
            }
        }

        public string InstructionText
        {
            get
            {
                return this._InstructionText;
            }
            set
            {
                this.SetProperty(this.PropertyInstructionText, ref this._InstructionText, value, false);
            }
        }

        public string PropertyFieldWell
        {
            get
            {
                return "FieldWell";
            }
        }

        public ViewModelBase FieldWell
        {
            get
            {
                return this._FieldWell;
            }
            set
            {
                this.SetProperty(this.PropertyFieldWell, ref this._FieldWell, value, false);
            }
        }

        public string PropertyFieldWellSummary
        {
            get
            {
                return "FieldWellSummary";
            }
        }

        public ViewModelBase FieldWellSummary
        {
            get
            {
                return this._FieldWellSummary;
            }
            private set
            {
                this.SetProperty(this.PropertyFieldWellSummary, ref this._FieldWellSummary, value, false);
            }
        }

        public LayerType VisualizationType
        {
            get
            {
                return this.FieldWellVisualizationViewModel.SelectedVisualizationType;
            }
        }

        public bool DisplayCategoryCounts
        {
            get
            {
                return this.FieldWellVisualizationViewModel.DisplayCategoryCounts;
            }
        }

        public static string PropertyCanVisualShapeBeChanged
        {
            get
            {
                return "CanVisualShapeBeChanged";
            }
        }

        public bool CanVisualShapeBeChanged
        {
            get
            {
                return this.FieldWellVisualizationViewModel.CanVisualShapeBeChanged;
            }
        }

        internal Filter Filter { get; private set; }

        internal IDialogServiceProvider DialogServiceProvider { get; private set; }

        public event EventHandler OnFilterClausesChanged;

        public event EventHandler OnFilterClausesCleared;

        public FieldListPickerViewModel(IEnumerable<TableIsland> tableIslands, LayerViewModel layerVM, GeoVisualization geoVis, IDialogServiceProvider dialogProvider)
            : this(geoVis, dialogProvider)
        {
            this._layerViewModel = layerVM;
            this.InitializeTableIslands(tableIslands);
            this.InitializeFromFieldWellDefinition(geoVis);
            this.FieldWellGeographyViewModel.ExecuteVisualizeCommandInChooseGeoFieldsState = this.ExecuteVisualizeCommandInChooseGeoFieldsState;
        }

        public FieldListPickerViewModel(GeoVisualization geoVis = null, IDialogServiceProvider dialogProvider = null)
        {
            FieldListPickerViewModel listPickerViewModel = this;
            this._geoVisualization = geoVis;
            this.Filter = new Filter();
            this.DialogServiceProvider = dialogProvider;
            this.FieldWellGeographyViewModel = new FieldWellGeographyViewModel(dialogProvider)
            {
                ChartTypeIsRegion = this.ChartTypeIsRegion
            };
            if (geoVis != null)
            {
                this.Filter.SetFilterClausesFrom(geoVis.GeoFieldWellDefinition == null ? null : geoVis.GeoFieldWellDefinition.Filter);
                this.FieldWellVisualizationViewModel = new FieldWellVisualizationViewModel(geoVis.CurrentRegionShadingMode);
                this.FieldWellVisualizationViewModel.SetRegionShadingSetting = setting => geoVis.SelectedRegionShadingMode = new RegionLayerShadingMode?(setting);
                this._geoVisualization.RegionShadingModeChanged += this.FieldWellVisualizationViewModel.OnRegionLayerShadingModeChanged;
            }
            else
                this.FieldWellVisualizationViewModel = new FieldWellVisualizationViewModel(RegionLayerShadingMode.FullBleed);
            this.FieldWellVisualizationViewModel.DetectMostAccurateMapByFieldForRegionChart = this.DetectMostAccurateMapByFieldForRegionChart;
            this.TableIslandsForGeography = new ObservableCollectionEx<TableIslandViewModel>();
            this.TableIslandsForGeography.ItemAdded += (ObservableCollectionExChangedHandler<TableIslandViewModel>)(item => item.IslandTableFieldSelectedChanged += this.OnGeographyIslandTableFieldSelectedChanged);
            this.TableIslandsForGeography.ItemRemoved += (ObservableCollectionExChangedHandler<TableIslandViewModel>)(item => item.IslandTableFieldSelectedChanged -= this.OnGeographyIslandTableFieldSelectedChanged);
            this.TableIslandsForVisualization = new ObservableCollectionEx<TableIslandViewModel>();
            this.TableIslandsForVisualization.ItemAdded += (ObservableCollectionExChangedHandler<TableIslandViewModel>)(item => item.IslandTableFieldSelectedChanged += this.OnVisualizationIslandTableFieldSelectedChanged);
            this.TableIslandsForVisualization.ItemRemoved += (ObservableCollectionExChangedHandler<TableIslandViewModel>)(item => item.IslandTableFieldSelectedChanged -= this.OnVisualizationIslandTableFieldSelectedChanged);
            this.TableIslandsForFiltering = new ObservableCollectionEx<TableIslandViewModel>();
            this.FieldWellVisualizationViewModel.VisualizeCommand = this.OnExecuteVisualizeCommand;
            this.FieldWellVisualizationViewModel.PropertyChanged += this.FieldWellVisualizationPropertyChanged;
            this.FieldWellVisualizationViewModel.GetGeoFieldMapping = this.GetGeoFieldMapping;
            this.InitializeFieldWellGeographyViewModel();
            this.SetState(FieldListPickerState.ChooseGeoFields);
        }

        internal void InitializeToAutoGeocode(IEnumerable<TableIsland> tableIslands, LayerDefinition layerDef, string modelTableNameForAutoGeocoding = null)
        {
            if (tableIslands == null || layerDef == null || (layerDef.Deserialized || layerDef.RefreshedAfterModelMetadataChanged))
                return;
            TableMetadata tableMetadata = null;
            if (modelTableNameForAutoGeocoding != null)
            {
                foreach (TableIsland tableIsland in tableIslands)
                {
                    tableMetadata = tableIsland.Tables.FirstOrDefault(table => string.Compare(table.ModelName, modelTableNameForAutoGeocoding, StringComparison.Ordinal) == 0);
                    if (tableMetadata != null)
                        break;
                }
            }
            else if (tableIslands.Count() == 1 && tableIslands.First().Tables.Count == 1)
                tableMetadata = tableIslands.First().Tables.First();
            if (tableMetadata == null)
                return;
            bool[] flagArray = new bool[13];
            bool flag1 = false;
            this.FieldWellVisualizationViewModel.DisallowVisualization();
            TableFieldViewModel tableFieldViewModel1 = null;
            TableFieldViewModel tableFieldViewModel2 = null;
            TableFieldViewModel tableFieldViewModel3 = null;
            TableFieldViewModel tableFieldViewModel4 = null;
            foreach (TableField tableField in tableMetadata.Fields)
            {
                TableMember tableMember = tableField as TableColumn;
                if (tableMember != null && tableMember.Classification != TableMemberClassification.None)
                {
                    GeoFieldMappingType fieldMappingType = tableMember.Classification.GeoFieldType();
                    if (!flagArray[(int)fieldMappingType])
                    {
                        flagArray[(int)fieldMappingType] = true;
                        TableFieldViewModel field = this.FindField(this.TableIslandsForGeography, tableField);
                        if (field != null)
                        {
                            bool flag2 = false;
                            bool flag3;
                            if (fieldMappingType == GeoFieldMappingType.Latitude)
                            {
                                tableFieldViewModel1 = field;
                                flag3 = tableFieldViewModel2 != null;
                                if (flag3)
                                    tableFieldViewModel2.IsSelected = true;
                            }
                            else if (fieldMappingType == GeoFieldMappingType.Longitude)
                            {
                                tableFieldViewModel2 = field;
                                flag3 = tableFieldViewModel1 != null;
                                if (flag3)
                                    tableFieldViewModel1.IsSelected = true;
                            }
                            else
                                flag3 = true;
                            if (fieldMappingType == GeoFieldMappingType.XCoord)
                            {
                                tableFieldViewModel3 = field;
                                flag2 = tableFieldViewModel4 != null;
                                if (flag3)
                                    tableFieldViewModel4.IsSelected = true;
                            }
                            else if (fieldMappingType == GeoFieldMappingType.YCoord)
                            {
                                tableFieldViewModel4 = field;
                                flag2 = tableFieldViewModel3 != null;
                                if (flag3)
                                    tableFieldViewModel3.IsSelected = true;
                            }
                            else
                                flag3 = true;
                            if (flag3 || flag2)
                            {
                                field.IsSelected = true;
                                flag1 = true;
                            }
                        }
                    }
                }
            }
            this.FieldWellVisualizationViewModel.AllowVisualization();
            if (!flag1)
                return;
            this.OnExecuteVisualizeCommand(true);
        }

        public void SetGeocodingReport(GeocodingReportViewModel geocodingReport)
        {
            this.FieldWellGeographyViewModel.GeocodingReport = geocodingReport;
        }

        public bool ChartTypeIsRegion()
        {
            return this.FieldWellVisualizationViewModel.ChartType == VisualizationChartType.Region;
        }

        public void UnInitialize()
        {
            if (this._geoVisualization != null)
                this._geoVisualization.RegionShadingModeChanged -= this.FieldWellVisualizationViewModel.OnRegionLayerShadingModeChanged;
            if (this.FieldWellVisualizationViewModel == null)
                return;
            this.FieldWellVisualizationViewModel.VisualizeCommand = null;
            this.FieldWellVisualizationViewModel.PropertyChanged -= this.FieldWellVisualizationPropertyChanged;
            this.FieldWellVisualizationViewModel.GetGeoFieldMapping = null;
        }

        internal void SetFilter(Filter filter)
        {
            if (!this.Filter.SetFilterClausesFrom(filter))
                return;
            this.FilterChanged();
            this.OnExecuteVisualizeCommand(false);
        }

        internal void AddFilterClause(FilterClause filter)
        {
            if (!this.Filter.AddFilterClause(filter))
                return;
            this.FilterChanged();
            this.OnExecuteVisualizeCommand(false);
        }

        internal void ReplaceFilterClause(FilterClause filterClause, FilterClause newFilterClause)
        {
            if (!this.Filter.ReplaceFilterClause(filterClause, newFilterClause))
                return;
            this.FilterChanged();
            this.OnExecuteVisualizeCommand(false);
        }

        internal void RemoveFilterClause(FilterClause filter)
        {
            if (!this.Filter.RemoveFilterClause(filter))
                return;
            this.FilterChanged();
            this.OnExecuteVisualizeCommand(false);
        }

        internal void ClearFilter()
        {
            if (!this.Filter.RemoveAllFilterClauses())
                return;
            this.FilterChanged();
            this.OnExecuteVisualizeCommand(false);
        }

        internal bool IsFiltered()
        {
            return this.Filter.HasFilterClauses;
        }

        private void FilterChanged()
        {
            if (this.OnFilterClausesChanged == null)
                return;
            this.OnFilterClausesChanged(this, EventArgs.Empty);
        }

        private void InitializeFieldWellGeographyViewModel()
        {
            this.FieldWellGeographyViewModel.Initialize();
            this.FieldWellGeographyViewModel.NextCommand = new DelegatedCommand(this.OnExecuteNextCommand, this.CanExecuteNextCommand);
            this.FieldWellGeographyViewModel.BackgroundChangeYesCommand = new DelegatedCommand(this.OnExecuteBackgroundChangeYesCommand);
            this.FieldWellGeographyViewModel.BackgroundChangeNoCommand = new DelegatedCommand(this.OnExecuteBackgroundChangeNoCommand);
            this.FieldWellGeographyViewModel.EditCommand = new DelegatedCommand(this.OnExecuteEditGeoFields);
            this.FieldWellGeographyViewModel.PropertyChanged += this.GeographyFieldPropertyChanged;
        }

        private void FieldWellVisualizationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == FieldWellVisualizationViewModel.PropertyCanVisualShapeBeChanged)
            {
                this.RaisePropertyChanged(FieldListPickerViewModel.PropertyCanVisualShapeBeChanged);
            }
            else
            {
                if (!(e.PropertyName == this.FieldWellVisualizationViewModel.PropertyChartType))
                    return;
                this.SetLabelInTableIslandForVisualization();
            }
        }

        private void SetLabelInTableIslandForVisualization()
        {
            string str = string.Empty;
            switch (this.FieldWellVisualizationViewModel.ChartType)
            {
                case VisualizationChartType.StackedColumn:
                case VisualizationChartType.ClusteredColumn:
                    str = Resources.FieldWellVisualization_ValueLabel_Height;
                    break;
                case VisualizationChartType.Bubble:
                    str = Resources.FieldWellVisualization_ValueLabel_Size;
                    break;
                case VisualizationChartType.HeatMap:
                case VisualizationChartType.Region:
                    str = Resources.FieldWellVisualization_ValueLabel_Value;
                    break;
            }
            foreach (TableIslandViewModel tableIslandViewModel in this.TableIslandsForVisualization)
            {
                foreach (TableViewModel tableViewModel in tableIslandViewModel.Tables)
                {
                    foreach (TableFieldViewModel tableFieldViewModel in tableViewModel.Fields)
                        tableFieldViewModel.HeightLabel = str;
                }
            }
        }

        private void GeographyFieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this._state != FieldListPickerState.ChooseVisField || !(e.PropertyName == FieldWellGeographyViewModel.PropertySelectedGeoMapping))
                return;
            this.FieldWellVisualizationViewModel.OnGeoMappingChanged();
            this.OnExecuteVisualizeCommand(false);
        }

        private void OnGeographyIslandTableFieldSelectedChanged(TableIslandViewModel island, TableViewModel table, TableFieldViewModel field)
        {
            if (!this._processGeographyIslandTableFieldSelectionChanges)
                return;
            if (field.IsSelected)
                this.FieldWellGeographyViewModel.AddGeoField(field);
            else
                this.FieldWellGeographyViewModel.RemoveGeoField(field);
        }

        private GeoFieldMappingViewModel GetGeoFieldMapping()
        {
            return this.FieldWellGeographyViewModel.SelectedGeoMapping;
        }

        private void OnVisualizationIslandTableFieldSelectedChanged(TableIslandViewModel island, TableViewModel table, TableFieldViewModel field)
        {
            if (!field.IsSelected)
                return;
            this.FieldWellVisualizationViewModel.ProcessNewSelectedField(field);
        }

        private void ExecuteVisualizeCommandInChooseGeoFieldsState()
        {
            if (this._state == FieldListPickerState.ChooseVisField)
                return;
            this.FieldWellVisualizationViewModel.DisallowVisualization();
            bool resetMapBy = true;
            foreach (GeoFieldMappingViewModel mappingViewModel in this.FieldWellGeographyViewModel.GeoFieldMappings)
            {
                if (mappingViewModel.UserSelectedMapByField)
                {
                    resetMapBy = false;
                    break;
                }
            }
            bool flag = !this.CanExecuteNextCommand() || !this.ValidateGeo(true, resetMapBy);
            this.FieldWellVisualizationViewModel.AllowVisualization();
            this.FieldWellGeographyViewModel.IsSettingMapByFieldEnabled = !flag;
            if (flag)
            {
                foreach (TableIslandViewModel tableIslandViewModel in this.TableIslandsForFiltering)
                    tableIslandViewModel.IsEnabled = false;
                this.OnExecuteClearVisualization();
            }
            else
            {
                this.ClearGeographyError();
                this.EliminateFieldsAndFiltersNotInTheGeoTableIsland();
                this.OnExecuteVisualizeCommand(true);
            }
        }

        private void OnExecuteNextCommand()
        {
            if (!this.CanExecuteNextCommand(true) || !this.ValidateGeo(true, false))
                return;
            this.EliminateFieldsAndFiltersNotInTheGeoTableIsland();
            this.OnExecuteNextCommandWithoutGeoValidation();
        }

        private bool CanExecuteNextCommand()
        {
            return this.CanExecuteNextCommand(false);
        }

        private bool CanExecuteNextCommand(bool displayErrorMessages)
        {
            if (this.FieldWellGeographyViewModel.GeoFieldMappings.Count == 0)
            {
                if (displayErrorMessages)
                    this.ShowGeographyError(Resources.FieldListPicker_Select_Geography);
                return false;
            }
            foreach (GeoFieldMappingViewModel mappingViewModel in this.FieldWellGeographyViewModel.GeoFieldMappings)
            {
                if (mappingViewModel.MappingType == GeoFieldMappingType.None)
                {
                    if (displayErrorMessages)
                        this.ShowGeographyError(Resources.FieldListPicker_Set_Geography_Type);
                    return false;
                }
            }
            return true;
        }

        private void OnExecuteNextCommandWithoutGeoValidation()
        {
            if (this.FieldWellGeographyViewModel.SelectedGeoMapping != null)
            {
                foreach (TableIslandViewModel tableIslandViewModel in this.TableIslandsForVisualization)
                    tableIslandViewModel.IsEnabled = tableIslandViewModel.ContainsField(this.FieldWellGeographyViewModel.SelectedGeoMapping.Field.Model);
                foreach (TableIslandViewModel tableIslandViewModel in this.TableIslandsForFiltering)
                    tableIslandViewModel.IsEnabled = tableIslandViewModel.ContainsField(this.FieldWellGeographyViewModel.SelectedGeoMapping.Field.Model);
            }
            this.SetState(FieldListPickerState.ChooseVisField);
            this.OnExecuteVisualizeCommand(false);
        }

        private void BringUpChangeMapTypeSelector()
        {
            HostControlViewModel hostViewModel = this._layerViewModel.LayerManagerViewModel.HostViewModel;
            if (hostViewModel == null)
                return;
            hostViewModel.ShowSceneGalleryDialog();
        }

        private void OnExecuteBackgroundChangeYesCommand()
        {
            this.FieldWellGeographyViewModel.IsBackgroundPromptApplicable = false;
            this.BringUpChangeMapTypeSelector();
        }

        private void OnExecuteBackgroundChangeNoCommand()
        {
            this.FieldWellGeographyViewModel.IsBackgroundPromptApplicable = false;
        }

        private void OnExecuteEditGeoFields()
        {
            this.SetState(FieldListPickerState.ChooseGeoFields);
            this.OnExecuteVisualizeCommand(false);
        }

        private void OnExecuteVisualizeCommand(bool zoomToData)
        {
            if (this.VisualizeCallback == null || !this.FieldWellVisualizationViewModel.VisualizationEnabled)
                return;
            this.VisualizeCallback(this.FieldWellGeographyViewModel, this.Filter, this.FieldWellVisualizationViewModel, zoomToData, this.State, false);
        }

        private void OnExecuteClearVisualization()
        {
            if (this.VisualizeCallback == null || !this.FieldWellVisualizationViewModel.VisualizationEnabled)
                return;
            this.VisualizeCallback(this.FieldWellGeographyViewModel, this.Filter, this.FieldWellVisualizationViewModel, false, this.State, true);
        }

        private void SetState(FieldListPickerState state)
        {
            if (state == FieldListPickerState.ChooseGeoFields)
            {
                this.State = FieldListPickerState.ChooseGeoFields;
                this.InstructionText = Resources.FieldListPicker_Instruction_ChooseGeographyField;
                this.FieldWell = this.FieldWellGeographyViewModel;
                this.FieldWellSummary = null;
            }
            else
            {
                if (state != FieldListPickerState.ChooseVisField)
                    return;
                this.State = FieldListPickerState.ChooseVisField;
                this.InstructionText = Resources.FieldListPicker_Instruction_ChooseVisualizationField;
                this.FieldWell = this.FieldWellVisualizationViewModel;
                this.FieldWellGeographyViewModel.Refresh();
                this.FieldWellSummary = this.FieldWellGeographyViewModel;
            }
        }

        private void ShowGeographyError(string message)
        {
            this.FieldWellGeographyViewModel.DisplayedErrorMessage = message;
        }

        private void ClearGeographyError()
        {
            this.FieldWellGeographyViewModel.DisplayedErrorMessage = null;
        }

        private void SetBackgroundEditable(bool isEditable)
        {
            this.FieldWellGeographyViewModel.IsBackgroundEditable = isEditable;
            this.FieldWellGeographyViewModel.IsBackgroundPromptApplicable = false;
            if (!isEditable)
                return;
            HostControlViewModel hostViewModel = this._layerViewModel.LayerManagerViewModel.HostViewModel;
            if (hostViewModel == null)
                return;
            this.FieldWellGeographyViewModel.IsBackgroundPromptApplicable = hostViewModel.IsGeoMapsToolsEnabled;
        }

        private bool ValidateGeo(bool displayErrorMessages, bool resetMapBy)
        {
            TableColumn tableColumn1 = null;
            TableColumn tableColumn2 = null;
            TableColumn tableColumn3 = null;
            TableColumn tableColumn4 = null;
            TableColumn tableColumn5 = null;
            TableColumn tableColumn6 = null;
            TableColumn tableColumn7 = null;
            TableColumn tableColumn8 = null;
            TableColumn tableColumn9 = null;
            TableColumn tableColumn10 = null;
            TableColumn tableColumn11 = null;
            TableColumn tableColumn12 = null;
            TableColumn tableColumn13 = null;
            TableColumn tableColumn14 = null;
            TableIsland tableIsland = null;
            bool flag1 = true;
            this.SetBackgroundEditable(false);
            if (this.FieldWellGeographyViewModel.GeoFieldMappings.Count > 0 && this.FieldWellGeographyViewModel.GeoFieldMappings[0].Field.Model is TableColumn)
                tableIsland = ((TableMember)this.FieldWellGeographyViewModel.GeoFieldMappings[0].Field.Model).Table.Island;
            foreach (GeoFieldMappingViewModel mappingViewModel in this.FieldWellGeographyViewModel.GeoFieldMappings)
            {
                TableColumn tableColumn15 = mappingViewModel.Field.Model as TableColumn;
                if (tableColumn15 == null)
                {
                    if (displayErrorMessages)
                        this.ShowGeographyError(Resources.FieldListPicker_Select_Geography);
                    return false;
                }
                switch (mappingViewModel.MappingType)
                {
                    case GeoFieldMappingType.Latitude:
                        if (tableColumn1 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_Latitude_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn1 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.Longitude:
                        if (tableColumn2 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_Longitude_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn2 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.Address:
                        if (tableColumn13 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_FullAddress_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn5 = tableColumn13 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.Other:
                        if (tableColumn14 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_OtherLocationDescription_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn5 = tableColumn14 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.Street:
                        if (tableColumn7 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_Street_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn6 = tableColumn7 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.City:
                        if (tableColumn8 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_City_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn6 = tableColumn8 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.County:
                        if (tableColumn9 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_County_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn6 = tableColumn9 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.State:
                        if (tableColumn10 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_State_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn6 = tableColumn10 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.Zip:
                        if (tableColumn11 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_Zip_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn6 = tableColumn11 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.Country:
                        if (tableColumn12 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_Country_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn6 = tableColumn12 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.XCoord:
                        if (tableColumn3 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_X_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn3 = tableColumn15;
                        continue;
                    case GeoFieldMappingType.YCoord:
                        if (tableColumn4 != null)
                        {
                            if (displayErrorMessages)
                                this.ShowGeographyError(Resources.FieldListPicker_Y_Duplicate);
                            return false;
                        }
                        flag1 = flag1 & tableColumn15.Table.Island == tableIsland;
                        tableColumn4 = tableColumn15;
                        continue;
                    default:
                        continue;
                }
            }
            if (tableColumn1 == null && tableColumn2 == null && (tableColumn6 == null && tableColumn5 == null) && (tableColumn3 == null && tableColumn4 == null))
            {
                if (displayErrorMessages)
                    this.ShowGeographyError(Resources.FieldListPicker_Instruction_ChooseGeographyField);
                return false;
            }
            if (tableColumn1 != null && tableColumn2 == null)
            {
                if (displayErrorMessages)
                    this.ShowGeographyError(Resources.FieldListPicker_Longitude_Needed);
                return false;
            }
            if (tableColumn2 != null && tableColumn1 == null)
            {
                if (displayErrorMessages)
                    this.ShowGeographyError(Resources.FieldListPicker_Latitude_Needed);
                return false;
            }
            if (tableColumn3 != null && tableColumn4 == null)
            {
                if (displayErrorMessages)
                    this.ShowGeographyError(Resources.FieldListPicker_Y_Needed);
                return false;
            }
            if (tableColumn4 != null && tableColumn3 == null)
            {
                if (displayErrorMessages)
                    this.ShowGeographyError(Resources.FieldListPicker_X_Needed);
                return false;
            }
            if (!flag1)
            {
                if (displayErrorMessages)
                    this.ShowGeographyError(Resources.FieldListPicker_GeoMustBeInSameTableIsland);
                return false;
            }
            if (this.ChartTypeIsRegion())
            {
                bool flag2 = false;
                if (!resetMapBy)
                {
                    foreach (GeoFieldMappingViewModel mappingViewModel in this.FieldWellGeographyViewModel.GeoFieldMappings)
                    {
                        if (mappingViewModel.IsMapByField && !mappingViewModel.MappingType.SupportsRegions())
                        {
                            flag2 = true;
                            break;
                        }
                    }
                }
                if (flag2 || !this.DetectMostAccurateMapByFieldForRegionChart(false, resetMapBy))
                {
                    this.FieldWellVisualizationViewModel.DisallowVisualization();
                    this.FieldWellVisualizationViewModel.ChartType = VisualizationChartType.StackedColumn;
                    this.FieldWellGeographyViewModel.DetectMostAccurateMapByField(resetMapBy);
                    this.FieldWellVisualizationViewModel.AllowVisualization();
                    int num = (int)MessageBox.Show(Resources.FieldWellGeography_RegionChart_IncompatibleMappingType_Changed, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK, Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
                }
            }
            else
                this.FieldWellGeographyViewModel.DetectMostAccurateMapByField(resetMapBy);
            if (tableColumn3 != null && tableColumn4 != null)
                this.SetBackgroundEditable(true);
            return true;
        }

        internal bool DetectMostAccurateMapByFieldForRegionChart(bool showErrorMessage, bool resetMapBy)
        {
            if (this.FieldWellGeographyViewModel.DetectMostAccurateMapByFieldForRegionChart(resetMapBy))
                return true;
            if (showErrorMessage)
            {
                int num = (int)MessageBox.Show(Resources.FieldWellGeography_RegionChart_IncompatibleMappingType, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK, Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            return false;
        }

        internal void InitializeTableIslands(IEnumerable<TableIsland> tableIslands)
        {
            this.TableIslandsForGeography.Clear();
            this.TableIslandsForVisualization.Clear();
            this.TableIslandsForFiltering.Clear();
            if (tableIslands == null)
                return;
            foreach (TableIsland model in tableIslands)
            {
                TableIslandViewModel tableIslandViewModel = new TableIslandViewModel(model, true, (Action<TableFieldViewModel>)null, (Action<TableFieldViewModel>)null, (Action<TableFieldViewModel>)null, (Func<bool>)null);
                if (tableIslandViewModel.Tables.Count > 0)
                {
                    this.TableIslandsForGeography.Add(tableIslandViewModel);
                    this.TableIslandsForVisualization.Add(new TableIslandViewModel(model, false, this.FieldWellVisualizationViewModel.AddHeight, this.FieldWellVisualizationViewModel.AddCategory, this.FieldWellVisualizationViewModel.AddTime, () => this.FieldWellVisualizationViewModel.ChartType != VisualizationChartType.HeatMap));
                    this.TableIslandsForFiltering.Add(new TableIslandViewModel(model, false, null, null, null, () => this.FieldWellVisualizationViewModel.ChartType != VisualizationChartType.HeatMap));
                }
            }
        }

        private void EliminateFieldsAndFiltersNotInTheGeoTableIsland()
        {
            this.FieldWellVisualizationViewModel.DisallowVisualization();
            TableIsland island = ((TableMember)this.FieldWellGeographyViewModel.GeoFieldMappings[0].Field.Model).Table.Island;
            List<FieldWellHeightViewModel> list = new List<FieldWellHeightViewModel>();
            foreach (FieldWellHeightViewModel wellHeightViewModel in this.FieldWellVisualizationViewModel.HeightFields)
            {
                if (!object.ReferenceEquals(island, ((TableMember)wellHeightViewModel.TableField.Model).Table.Island))
                    list.Add(wellHeightViewModel);
            }
            foreach (FieldWellHeightViewModel wellHeightViewModel in list)
                this.FieldWellVisualizationViewModel.HeightFields.Remove(wellHeightViewModel);
            if (this.FieldWellVisualizationViewModel.HasCategory && !object.ReferenceEquals(island, ((TableMember)this.FieldWellVisualizationViewModel.SelectedCategory.Value.TableField.Model).Table.Island))
                this.FieldWellVisualizationViewModel.OnFieldWellCategoryRemoved(this.FieldWellVisualizationViewModel.SelectedCategory.Value);
            if (this.FieldWellVisualizationViewModel.SelectedTimeField.Value != null && !object.ReferenceEquals(island, ((TableMember)this.FieldWellVisualizationViewModel.SelectedTimeField.Value.TableField.Model).Table.Island))
                this.FieldWellVisualizationViewModel.OnFieldWellTimeRemoved(this.FieldWellVisualizationViewModel.SelectedTimeField.Value);
            if (this.IsFiltered() && !object.ReferenceEquals(island, this.Filter.FilterClauses.First().TableMember.Table.Island))
            {
                this.ClearFilter();
                if (this.OnFilterClausesCleared != null)
                    this.OnFilterClausesCleared(this, EventArgs.Empty);
            }
            this.FieldWellVisualizationViewModel.AllowVisualization();
            if (this.FieldWellGeographyViewModel.SelectedGeoMapping == null || this.FieldWellGeographyViewModel.SelectedGeoMapping.Field == null)
                return;
            foreach (TableIslandViewModel tableIslandViewModel in this.TableIslandsForFiltering)
                tableIslandViewModel.IsEnabled = tableIslandViewModel.ContainsField(this.FieldWellGeographyViewModel.SelectedGeoMapping.Field.Model);
        }

        private void InitializeFromFieldWellDefinition(GeoVisualization geoVis)
        {
            if (geoVis == null || geoVis.GeoFieldWellDefinition == null || geoVis.GeoFieldWellDefinition.Geo == null)
                return;
            GeoFieldWellDefinition fieldWellDefinition = geoVis.GeoFieldWellDefinition;
            this.FieldWellVisualizationViewModel.DisallowVisualization();
            this._processGeographyIslandTableFieldSelectionChanges = false;
            GeoField geo = fieldWellDefinition.Geo;
            TableColumn tableColumn = geo.GeoColumns.Count > 0 ? geo.GeoColumns[0] : null;
            bool flag1 = tableColumn != null && !geoVis.GeoFieldWellDefinition.ChoosingGeoFields;
            bool selectedMapByField = geoVis.GeoFieldWellDefinition.UserSelectedMapByField;
            this.FieldWellGeographyViewModel.IsSettingMapByFieldEnabled = tableColumn != null;
            if (geoVis.GeoFieldWellDefinition.ChosenGeoFields.Count > 0)
            {
                int i = 0;
                int numMappings = geoVis.GeoFieldWellDefinition.ChosenGeoMappings.Count;
                geoVis.GeoFieldWellDefinition.ChosenGeoFields.ForEach(field =>
                {
                    if (i < numMappings)
                        this.AddGeoField(field, geoVis.GeoFieldWellDefinition.ChosenGeoMappings[i].FromGeoMappingType(), false, false);
                    ++i;
                });
            }
            else
            {
                if (geo is GeoEntityField)
                {
                    GeoEntityField geoEntityField = geo as GeoEntityField;
                    if (geoVis.GeoFieldWellDefinition.ChosenGeoMappings.Count > 0)
                    {
                        foreach (GeoMappingType geoMappingType in geoVis.GeoFieldWellDefinition.ChosenGeoMappings)
                        {
                            switch (geoMappingType.FromGeoMappingType())
                            {
                                case GeoFieldMappingType.Latitude:
                                    this.AddGeoField(geoEntityField.LongitudeNotUsedForGeocoding, GeoFieldMappingType.Latitude, false, false);
                                    continue;
                                case GeoFieldMappingType.Longitude:
                                    this.AddGeoField(geoEntityField.LatitudeNotUsedForGeocoding, GeoFieldMappingType.Longitude, false, false);
                                    continue;
                                case GeoFieldMappingType.Address:
                                    this.AddGeoField(geoEntityField.FullAddressNotUsedForGeocoding, GeoFieldMappingType.Address, false, false);
                                    continue;
                                case GeoFieldMappingType.Other:
                                    this.AddGeoField(geoEntityField.OtherLocationDescriptionNotUsedForGeocoding, GeoFieldMappingType.Other, false, false);
                                    continue;
                                case GeoFieldMappingType.Street:
                                    this.AddGeoField(geoEntityField.AddressLineNotUsedForGeocoding, GeoFieldMappingType.Street, false, false);
                                    this.AddGeoField(geoEntityField.AddressLine, GeoFieldMappingType.Street, TableMember.QuerySubstitutable(tableColumn, geoEntityField.AddressLine), selectedMapByField);
                                    continue;
                                case GeoFieldMappingType.City:
                                    this.AddGeoField(geoEntityField.LocalityNotUsedForGeocoding, GeoFieldMappingType.City, false, false);
                                    this.AddGeoField(geoEntityField.Locality, GeoFieldMappingType.City, TableMember.QuerySubstitutable(tableColumn, geoEntityField.Locality), selectedMapByField);
                                    continue;
                                case GeoFieldMappingType.County:
                                    this.AddGeoField(geoEntityField.AdminDistrict2NotUsedForGeocoding, GeoFieldMappingType.County, false, false);
                                    this.AddGeoField(geoEntityField.AdminDistrict2, GeoFieldMappingType.County, TableMember.QuerySubstitutable(tableColumn, geoEntityField.AdminDistrict2), selectedMapByField);
                                    continue;
                                case GeoFieldMappingType.State:
                                    this.AddGeoField(geoEntityField.AdminDistrictNotUsedForGeocoding, GeoFieldMappingType.State, false, false);
                                    this.AddGeoField(geoEntityField.AdminDistrict, GeoFieldMappingType.State, TableMember.QuerySubstitutable(tableColumn, geoEntityField.AdminDistrict), selectedMapByField);
                                    continue;
                                case GeoFieldMappingType.Zip:
                                    this.AddGeoField(geoEntityField.PostalCodeNotUsedForGeocoding, GeoFieldMappingType.Zip, false, false);
                                    this.AddGeoField(geoEntityField.PostalCode, GeoFieldMappingType.Zip, TableMember.QuerySubstitutable(tableColumn, geoEntityField.PostalCode), selectedMapByField);
                                    continue;
                                case GeoFieldMappingType.Country:
                                    this.AddGeoField(geoEntityField.CountryNotUsedForGeocoding, GeoFieldMappingType.Country, false, false);
                                    this.AddGeoField(geoEntityField.Country, GeoFieldMappingType.Country, TableMember.QuerySubstitutable(tableColumn, geoEntityField.Country), selectedMapByField);
                                    continue;
                                case GeoFieldMappingType.XCoord:
                                    this.AddGeoField(geoEntityField.XCoordNotUsedForGeocoding, GeoFieldMappingType.XCoord, false, false);
                                    continue;
                                case GeoFieldMappingType.YCoord:
                                    this.AddGeoField(geoEntityField.YCoordNotUsedForGeocoding, GeoFieldMappingType.YCoord, false, false);
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                    else
                    {
                        this.AddGeoField(geoEntityField.CountryNotUsedForGeocoding, GeoFieldMappingType.Country, false, false);
                        this.AddGeoField(geoEntityField.Country, GeoFieldMappingType.Country, TableMember.QuerySubstitutable(tableColumn, geoEntityField.Country), selectedMapByField);
                        this.AddGeoField(geoEntityField.PostalCodeNotUsedForGeocoding, GeoFieldMappingType.Zip, false, false);
                        this.AddGeoField(geoEntityField.PostalCode, GeoFieldMappingType.Zip, TableMember.QuerySubstitutable(tableColumn, geoEntityField.PostalCode), selectedMapByField);
                        this.AddGeoField(geoEntityField.AdminDistrictNotUsedForGeocoding, GeoFieldMappingType.State, false, false);
                        this.AddGeoField(geoEntityField.AdminDistrict, GeoFieldMappingType.State, TableMember.QuerySubstitutable(tableColumn, geoEntityField.AdminDistrict), selectedMapByField);
                        this.AddGeoField(geoEntityField.AdminDistrict2NotUsedForGeocoding, GeoFieldMappingType.County, false, false);
                        this.AddGeoField(geoEntityField.AdminDistrict2, GeoFieldMappingType.County, TableMember.QuerySubstitutable(tableColumn, geoEntityField.AdminDistrict2), selectedMapByField);
                        this.AddGeoField(geoEntityField.LocalityNotUsedForGeocoding, GeoFieldMappingType.City, false, false);
                        this.AddGeoField(geoEntityField.Locality, GeoFieldMappingType.City, TableMember.QuerySubstitutable(tableColumn, geoEntityField.Locality), selectedMapByField);
                        this.AddGeoField(geoEntityField.AddressLineNotUsedForGeocoding, GeoFieldMappingType.Street, false, false);
                        this.AddGeoField(geoEntityField.AddressLine, GeoFieldMappingType.Street, TableMember.QuerySubstitutable(tableColumn, geoEntityField.AddressLine), selectedMapByField);
                        this.AddGeoField(geoEntityField.LatitudeNotUsedForGeocoding, GeoFieldMappingType.Longitude, false, false);
                        this.AddGeoField(geoEntityField.LongitudeNotUsedForGeocoding, GeoFieldMappingType.Latitude, false, false);
                        this.AddGeoField(geoEntityField.XCoordNotUsedForGeocoding, GeoFieldMappingType.XCoord, false, false);
                        this.AddGeoField(geoEntityField.YCoordNotUsedForGeocoding, GeoFieldMappingType.YCoord, false, false);
                        this.AddGeoField(geoEntityField.FullAddressNotUsedForGeocoding, GeoFieldMappingType.Address, false, false);
                        this.AddGeoField(geoEntityField.OtherLocationDescriptionNotUsedForGeocoding, GeoFieldMappingType.Other, false, false);
                    }
                }
                else if (geo is GeoFullAddressField)
                {
                    GeoFullAddressField fullAddressField = geo as GeoFullAddressField;
                    if (geoVis.GeoFieldWellDefinition.ChosenGeoMappings.Count > 0)
                    {
                        foreach (GeoMappingType geoMappingType in geoVis.GeoFieldWellDefinition.ChosenGeoMappings)
                        {
                            switch (geoMappingType.FromGeoMappingType())
                            {
                                case GeoFieldMappingType.Latitude:
                                    this.AddGeoField(fullAddressField.LongitudeNotUsedForGeocoding, GeoFieldMappingType.Latitude, false, false);
                                    continue;
                                case GeoFieldMappingType.Longitude:
                                    this.AddGeoField(fullAddressField.LatitudeNotUsedForGeocoding, GeoFieldMappingType.Longitude, false, false);
                                    continue;
                                case GeoFieldMappingType.Address:
                                    this.AddGeoField(fullAddressField.FullAddressNotUsedForGeocoding, GeoFieldMappingType.Address, false, false);
                                    this.AddGeoField(fullAddressField.FullAddress, GeoFieldMappingType.Address, TableMember.QuerySubstitutable(tableColumn, fullAddressField.FullAddress), selectedMapByField);
                                    continue;
                                case GeoFieldMappingType.Other:
                                    this.AddGeoField(fullAddressField.OtherLocationDescriptionNotUsedForGeocoding, GeoFieldMappingType.Other, false, false);
                                    this.AddGeoField(fullAddressField.OtherLocationDescription, GeoFieldMappingType.Other, TableMember.QuerySubstitutable(tableColumn, fullAddressField.OtherLocationDescription), selectedMapByField);
                                    continue;
                                case GeoFieldMappingType.Street:
                                    this.AddGeoField(fullAddressField.AddressLineNotUsedForGeocoding, GeoFieldMappingType.Street, false, false);
                                    continue;
                                case GeoFieldMappingType.City:
                                    this.AddGeoField(fullAddressField.LocalityNotUsedForGeocoding, GeoFieldMappingType.City, false, false);
                                    continue;
                                case GeoFieldMappingType.County:
                                    this.AddGeoField(fullAddressField.AdminDistrict2NotUsedForGeocoding, GeoFieldMappingType.County, false, false);
                                    continue;
                                case GeoFieldMappingType.State:
                                    this.AddGeoField(fullAddressField.AdminDistrictNotUsedForGeocoding, GeoFieldMappingType.State, false, false);
                                    continue;
                                case GeoFieldMappingType.Zip:
                                    this.AddGeoField(fullAddressField.PostalCodeNotUsedForGeocoding, GeoFieldMappingType.Zip, false, false);
                                    continue;
                                case GeoFieldMappingType.Country:
                                    this.AddGeoField(fullAddressField.CountryNotUsedForGeocoding, GeoFieldMappingType.Country, false, false);
                                    continue;
                                case GeoFieldMappingType.XCoord:
                                    this.AddGeoField(fullAddressField.XCoordNotUsedForGeocoding, GeoFieldMappingType.XCoord, false, false);
                                    continue;
                                case GeoFieldMappingType.YCoord:
                                    this.AddGeoField(fullAddressField.YCoordNotUsedForGeocoding, GeoFieldMappingType.YCoord, false, false);
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                    else
                    {
                        this.AddGeoField(fullAddressField.FullAddressNotUsedForGeocoding, GeoFieldMappingType.Address, false, false);
                        this.AddGeoField(fullAddressField.FullAddress, GeoFieldMappingType.Address, TableMember.QuerySubstitutable(tableColumn, fullAddressField.FullAddress), selectedMapByField);
                        this.AddGeoField(fullAddressField.OtherLocationDescriptionNotUsedForGeocoding, GeoFieldMappingType.Other, false, false);
                        this.AddGeoField(fullAddressField.OtherLocationDescription, GeoFieldMappingType.Other, TableMember.QuerySubstitutable(tableColumn, fullAddressField.OtherLocationDescription), selectedMapByField);
                        this.AddGeoField(fullAddressField.CountryNotUsedForGeocoding, GeoFieldMappingType.Country, false, false);
                        this.AddGeoField(fullAddressField.PostalCodeNotUsedForGeocoding, GeoFieldMappingType.Zip, false, false);
                        this.AddGeoField(fullAddressField.AdminDistrictNotUsedForGeocoding, GeoFieldMappingType.State, false, false);
                        this.AddGeoField(fullAddressField.AdminDistrict2NotUsedForGeocoding, GeoFieldMappingType.County, false, false);
                        this.AddGeoField(fullAddressField.LocalityNotUsedForGeocoding, GeoFieldMappingType.City, false, false);
                        this.AddGeoField(fullAddressField.AddressLineNotUsedForGeocoding, GeoFieldMappingType.Street, false, false);
                        this.AddGeoField(fullAddressField.LatitudeNotUsedForGeocoding, GeoFieldMappingType.Longitude, false, false);
                        this.AddGeoField(fullAddressField.LongitudeNotUsedForGeocoding, GeoFieldMappingType.Latitude, false, false);
                        this.AddGeoField(fullAddressField.XCoordNotUsedForGeocoding, GeoFieldMappingType.XCoord, false, false);
                        this.AddGeoField(fullAddressField.YCoordNotUsedForGeocoding, GeoFieldMappingType.YCoord, false, false);
                    }
                }
                else if (geoVis.GeoFieldWellDefinition.ChosenGeoMappings.Count > 0)
                {
                    bool flag2 = false;
                    bool flag3 = false;
                    bool flag4 = false;
                    bool flag5 = false;
                    foreach (GeoMappingType geoMappingType in geoVis.GeoFieldWellDefinition.ChosenGeoMappings)
                    {
                        switch (geoMappingType.FromGeoMappingType())
                        {
                            case GeoFieldMappingType.Latitude:
                                this.AddGeoField(geo.LatitudeNotUsedForGeocoding, GeoFieldMappingType.Latitude, false, false);
                                if (!geo.IsUsingXY)
                                {
                                    flag2 = geo.Latitude != null && !geo.IsUsingXY;
                                    this.AddGeoField(geo.Latitude, GeoFieldMappingType.Latitude, flag2 && flag3, false);
                                    continue;
                                }
                                continue;
                            case GeoFieldMappingType.Longitude:
                                this.AddGeoField(geo.LongitudeNotUsedForGeocoding, GeoFieldMappingType.Longitude, false, false);
                                if (!geo.IsUsingXY)
                                {
                                    flag3 = geo.Longitude != null;
                                    this.AddGeoField(geo.Longitude, GeoFieldMappingType.Longitude, flag2 && flag3, false);
                                    continue;
                                }
                                continue;
                            case GeoFieldMappingType.Address:
                                this.AddGeoField(geo.FullAddressNotUsedForGeocoding, GeoFieldMappingType.Address, false, false);
                                continue;
                            case GeoFieldMappingType.Other:
                                this.AddGeoField(geo.OtherLocationDescriptionNotUsedForGeocoding, GeoFieldMappingType.Other, false, false);
                                continue;
                            case GeoFieldMappingType.Street:
                                this.AddGeoField(geo.AddressLineNotUsedForGeocoding, GeoFieldMappingType.Street, false, false);
                                continue;
                            case GeoFieldMappingType.City:
                                this.AddGeoField(geo.LocalityNotUsedForGeocoding, GeoFieldMappingType.City, false, false);
                                continue;
                            case GeoFieldMappingType.County:
                                this.AddGeoField(geo.AdminDistrict2NotUsedForGeocoding, GeoFieldMappingType.County, false, false);
                                continue;
                            case GeoFieldMappingType.State:
                                this.AddGeoField(geo.AdminDistrictNotUsedForGeocoding, GeoFieldMappingType.State, false, false);
                                continue;
                            case GeoFieldMappingType.Zip:
                                this.AddGeoField(geo.PostalCodeNotUsedForGeocoding, GeoFieldMappingType.Zip, false, false);
                                continue;
                            case GeoFieldMappingType.Country:
                                this.AddGeoField(geo.CountryNotUsedForGeocoding, GeoFieldMappingType.Country, false, false);
                                continue;
                            case GeoFieldMappingType.XCoord:
                                this.AddGeoField(geo.XCoordNotUsedForGeocoding, GeoFieldMappingType.XCoord, false, false);
                                if (geo.IsUsingXY)
                                {
                                    flag4 = geo.Longitude != null && geo.IsUsingXY;
                                    this.AddGeoField(geo.Longitude, GeoFieldMappingType.XCoord, flag4 && flag5, false);
                                    continue;
                                }
                                continue;
                            case GeoFieldMappingType.YCoord:
                                this.AddGeoField(geo.YCoordNotUsedForGeocoding, GeoFieldMappingType.YCoord, false, false);
                                if (geo.IsUsingXY)
                                {
                                    flag5 = geo.Latitude != null;
                                    this.AddGeoField(geo.Latitude, GeoFieldMappingType.YCoord, flag4 && flag5, false);
                                    continue;
                                }
                                continue;
                            default:
                                continue;
                        }
                    }
                }
                else
                {
                    this.AddGeoField(geo.CountryNotUsedForGeocoding, GeoFieldMappingType.Country, false, false);
                    this.AddGeoField(geo.PostalCodeNotUsedForGeocoding, GeoFieldMappingType.Zip, false, false);
                    this.AddGeoField(geo.AdminDistrictNotUsedForGeocoding, GeoFieldMappingType.State, false, false);
                    this.AddGeoField(geo.AdminDistrict2NotUsedForGeocoding, GeoFieldMappingType.County, false, false);
                    this.AddGeoField(geo.LocalityNotUsedForGeocoding, GeoFieldMappingType.City, false, false);
                    this.AddGeoField(geo.AddressLineNotUsedForGeocoding, GeoFieldMappingType.Street, false, false);
                    this.AddGeoField(geo.LongitudeNotUsedForGeocoding, GeoFieldMappingType.Longitude, false, false);
                    this.AddGeoField(geo.LatitudeNotUsedForGeocoding, GeoFieldMappingType.Latitude, false, false);
                    this.AddGeoField(geo.XCoordNotUsedForGeocoding, GeoFieldMappingType.XCoord, false, false);
                    this.AddGeoField(geo.YCoordNotUsedForGeocoding, GeoFieldMappingType.YCoord, false, false);
                    this.AddGeoField(geo.FullAddressNotUsedForGeocoding, GeoFieldMappingType.Address, false, false);
                    this.AddGeoField(geo.OtherLocationDescriptionNotUsedForGeocoding, GeoFieldMappingType.Other, false, false);
                    this.AddGeoField(geo.Longitude, GeoFieldMappingType.Longitude, false, false);
                    this.AddGeoField(geo.Latitude, GeoFieldMappingType.Latitude, geo.Longitude != null && geo.Latitude != null, selectedMapByField);
                }
                if (tableColumn == null)
                {
                    this.ShowGeographyError(Resources.MapByNotFoundAfterDataRefresh);
                    this.FieldWellGeographyViewModel.IsSettingMapByFieldEnabled = !(geo.Latitude == null ^ geo.Longitude == null);
                }
            }
            bool flag6 = false;
            if (!geoVis.HiddenMeasure)
            {
                int num = 0;
                foreach (Tuple<TableField, AggregationFunction> tuple in fieldWellDefinition.Measures)
                {
                    TableFieldViewModel field = this.FindField(this.TableIslandsForVisualization, tuple.Item1);
                    this.FieldWellVisualizationViewModel.AddToHeightField(num++, field, tuple.Item2);
                    flag6 = tuple.Item2 == AggregationFunction.None;
                }
                bool flag2 = flag6 && num == 1;
            }
            if (fieldWellDefinition.Category != null)
                this.FieldWellVisualizationViewModel.SetSelectedCategoryField(this.FindField(this.TableIslandsForVisualization, fieldWellDefinition.Category));
            if (fieldWellDefinition.Time != null)
            {
                this.FieldWellVisualizationViewModel.SetSelectedTimeField(this.FindField(this.TableIslandsForVisualization, fieldWellDefinition.Time));
                this.FieldWellVisualizationViewModel.SelectedTimeField.Value.TimeChunk = fieldWellDefinition.ChunkBy;
            }
            this.FieldWellVisualizationViewModel.UserSelectedTimeSetting = fieldWellDefinition.UserSelectedTimeSetting;
            if (fieldWellDefinition.ViewModelAccumulateResultsOverTime)
                this.FieldWellVisualizationViewModel.SetAccumulateResultsOverTime(true);
            else if (fieldWellDefinition.ViewModelPersistTimeData)
                this.FieldWellVisualizationViewModel.SetPersistTimeData(true);
            else
                this.FieldWellVisualizationViewModel.SetInstantTimeData();
            switch (geoVis.VisualType)
            {
                case LayerType.PointMarkerChart:
                    this.FieldWellVisualizationViewModel.ChartType = geoVis.VisualShape != InstancedShape.CircularCone ? VisualizationChartType.StackedColumn : VisualizationChartType.Bubble;
                    break;
                case LayerType.BubbleChart:
                case LayerType.PieChart:
                    this.FieldWellVisualizationViewModel.ChartType = VisualizationChartType.Bubble;
                    break;
                case LayerType.ColumnChart:
                    this.FieldWellVisualizationViewModel.ChartType = VisualizationChartType.StackedColumn;
                    break;
                case LayerType.ClusteredColumnChart:
                    this.FieldWellVisualizationViewModel.ChartType = VisualizationChartType.ClusteredColumn;
                    break;
                case LayerType.StackedColumnChart:
                    this.FieldWellVisualizationViewModel.ChartType = VisualizationChartType.StackedColumn;
                    break;
                case LayerType.HeatMapChart:
                    this.FieldWellVisualizationViewModel.ChartType = VisualizationChartType.HeatMap;
                    break;
                case LayerType.RegionChart:
                    this.FieldWellVisualizationViewModel.ChartType = VisualizationChartType.Region;
                    break;
            }
            this._processGeographyIslandTableFieldSelectionChanges = true;
            this.FieldWellVisualizationViewModel.AllowVisualization();
            if (!flag1)
                return;
            this.OnExecuteNextCommandWithoutGeoValidation();
        }

        private void AddGeoField(TableField geoField, GeoFieldMappingType mappingType, bool isMapByField = false, bool userSelectedMapByField = false)
        {
            if (geoField == null)
                return;
            TableFieldViewModel field = this.FindField(this.TableIslandsForGeography, geoField);
            if (field == null)
                return;
            GeoFieldMappingViewModel geoField1 = new GeoFieldMappingViewModel(field, mappingType, isMapByField, isMapByField && userSelectedMapByField);
            this.FieldWellGeographyViewModel.AddGeoField(field, this.FieldWellGeographyViewModel.GeoFieldMappings.Count, geoField1);
            if (!isMapByField)
                return;
            this.FieldWellGeographyViewModel.SelectedGeoMapping = geoField1;
        }

        private TableFieldViewModel FindField(ObservableCollectionEx<TableIslandViewModel> islands, TableField tableField)
        {
            if (tableField is TableMeasure)
                return this.FindMeasure(islands, (TableMeasure)tableField);
            TableColumn tableColumn = tableField as TableColumn;
            if (tableColumn == null)
                return null;
            foreach (TableIslandViewModel tableIslandViewModel in islands)
            {
                foreach (TableViewModel tableViewModel in tableIslandViewModel.Tables)
                {
                    foreach (TableFieldViewModel tableFieldViewModel in tableViewModel.Fields)
                    {
                        if (tableColumn.RefersToTheSameMemberAs(tableFieldViewModel.Model as TableColumn))
                            return tableFieldViewModel;
                    }
                }
            }
            return null;
        }

        private TableFieldViewModel FindMeasure(ObservableCollectionEx<TableIslandViewModel> islands, TableMeasure measure)
        {
            if (measure == null)
                return null;
            foreach (TableIslandViewModel tableIslandViewModel in islands)
            {
                foreach (TableViewModel tableViewModel in tableIslandViewModel.Tables)
                {
                    foreach (TableFieldViewModel tableFieldViewModel in tableViewModel.Fields)
                    {
                        if (measure.RefersToTheSameMemberAs(tableFieldViewModel.Model as TableMeasure))
                            return tableFieldViewModel;
                    }
                }
            }
            return null;
        }
    }
}
