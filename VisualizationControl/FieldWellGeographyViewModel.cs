using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class FieldWellGeographyViewModel : ViewModelBase
    {
        public Func<bool> ChartTypeIsRegion;
        public Action ExecuteVisualizeCommandInChooseGeoFieldsState;
        private ICommand _NextCommand;
        private ICommand _BackgroundChangeYesCommand;
        private ICommand _BackgroundChangeNoCommand;
        private ICommand _EditCommand;
        private GeoFieldMappingViewModel _SelectedGeoMapping;
        private GeocodingReportViewModel _GeocodingReport;
        private string _MapByDisplayString;
        private string _DisplayedErrorMessage;
        private bool _IsSettingMapByFieldEnabled;
        private bool _IsBackgroundEditable;
        private bool _IsBackgroundPromptApplicable;
        private ICommand _DismissErrorCommand;

        public string PropertyNextCommand
        {
            get
            {
                return "NextCommand";
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return _NextCommand;
            }
            set
            {
                SetProperty(PropertyNextCommand, ref _NextCommand, value, false);
            }
        }

        public string PropertyBackgroundChangeYesCommand
        {
            get
            {
                return "BackgroundChangeYesCommand";
            }
        }

        public ICommand BackgroundChangeYesCommand
        {
            get
            {
                return _BackgroundChangeYesCommand;
            }
            set
            {
                SetProperty(PropertyBackgroundChangeYesCommand, ref _BackgroundChangeYesCommand, value, false);
            }
        }

        public string PropertyBackgroundChangeNoCommand
        {
            get
            {
                return "BackgroundChangeNoCommand";
            }
        }

        public ICommand BackgroundChangeNoCommand
        {
            get
            {
                return _BackgroundChangeNoCommand;
            }
            set
            {
                SetProperty(PropertyBackgroundChangeNoCommand, ref _BackgroundChangeNoCommand, value, false);
            }
        }

        public static string PropertyEditCommand
        {
            get
            {
                return "EditCommand";
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return _EditCommand;
            }
            set
            {
                SetProperty(PropertyEditCommand, ref _EditCommand, value, false);
            }
        }

        public static string PropertySelectedGeoMapping
        {
            get
            {
                return "SelectedGeoMapping";
            }
        }

        public GeoFieldMappingViewModel SelectedGeoMapping
        {
            get
            {
                return _SelectedGeoMapping;
            }
            set
            {
                SetProperty(PropertySelectedGeoMapping, ref _SelectedGeoMapping, value, false);
            }
        }

        public string PropertyGeocodingReport
        {
            get
            {
                return "GeocodingReport";
            }
        }

        public GeocodingReportViewModel GeocodingReport
        {
            get
            {
                return _GeocodingReport;
            }
            set
            {
                SetProperty(PropertyGeocodingReport, ref _GeocodingReport, value, false);
            }
        }

        public static string PropertyMapByDisplayString
        {
            get
            {
                return "MapByDisplayString";
            }
        }

        public string MapByDisplayString
        {
            get
            {
                return _MapByDisplayString;
            }
            private set
            {
                SetProperty(PropertyMapByDisplayString, ref _MapByDisplayString, value, false);
            }
        }

        public static string PropertyDisplayedErrorMessage
        {
            get
            {
                return "DisplayedErrorMessage";
            }
        }

        public string DisplayedErrorMessage
        {
            get
            {
                return _DisplayedErrorMessage;
            }
            set
            {
                SetProperty(PropertyDisplayedErrorMessage, ref _DisplayedErrorMessage, value, false);
            }
        }

        public static string PropertyIsSettingMapByFieldEnabled
        {
            get
            {
                return "IsSettingMapByFieldEnabled";
            }
        }

        public bool IsSettingMapByFieldEnabled
        {
            get
            {
                return _IsSettingMapByFieldEnabled;
            }
            set
            {
                SetProperty(PropertyIsSettingMapByFieldEnabled, ref _IsSettingMapByFieldEnabled, value, false);
            }
        }

        public static string PropertyIsBackgroundEditable
        {
            get
            {
                return "IsBackgroundEditable";
            }
        }

        public bool IsBackgroundEditable
        {
            get
            {
                return _IsBackgroundEditable;
            }
            set
            {
                SetProperty(PropertyIsBackgroundEditable, ref _IsBackgroundEditable, value, false);
            }
        }

        public static string PropertyIsBackgroundPromptApplicable
        {
            get
            {
                return "IsBackgroundPromptApplicable";
            }
        }

        public bool IsBackgroundPromptApplicable
        {
            get
            {
                return _IsBackgroundPromptApplicable;
            }
            set
            {
                SetProperty(PropertyIsBackgroundPromptApplicable, ref _IsBackgroundPromptApplicable, value, false);
            }
        }

        public ICommand DismissErrorCommand
        {
            get
            {
                return _DismissErrorCommand;
            }
        }

        public ObservableCollectionEx<GeoFieldMappingViewModel> GeoFieldMappings { get; private set; }

        public ICommand ViewReportCommand { get; private set; }

        public DropItemsHandler GeoFieldMappingsDropHandler { get; private set; }

        public FieldWellGeographyViewModel(IDialogServiceProvider dialogProvider)
        {
            FieldWellGeographyViewModel geographyViewModel = this;
            GeoFieldMappings = new ObservableCollectionEx<GeoFieldMappingViewModel>();
            GeoFieldMappingsDropHandler = new DropItemsHandler();
            GeoFieldMappingsDropHandler.AddDroppableTypeHandlers<TableFieldViewModel>((item, index) => AddGeoField(item, index), item =>
            {
                if (item.IsTableMeasure)
                    return Resources.FieldListPicker_Select_Geography;
                foreach (GeoFieldMappingViewModel mappingViewModel in GeoFieldMappings)
                {
                    if (mappingViewModel.Field == item)
                        return Resources.FieldWellGeography_DuplicateFieldExists;
                }
                return (string)null;
            });
            ViewReportCommand = new DelegatedCommand(() => geographyViewModel.OnExecuteViewReport(dialogProvider));
            _DismissErrorCommand = new DelegatedCommand(() => DisplayedErrorMessage = (string)null);
        }

        public void Initialize()
        {
            GeoFieldMappings.ItemRemoved += (ObservableCollectionExChangedHandler<GeoFieldMappingViewModel>)(item =>
            {
                item.IsMapByField = false;
                if (item == SelectedGeoMapping)
                    SelectedGeoMapping = null;
                if (ExecuteVisualizeCommandInChooseGeoFieldsState == null)
                    return;
                ExecuteVisualizeCommandInChooseGeoFieldsState();
            });
            GeoFieldMappings.ItemAdded += (ObservableCollectionExChangedHandler<GeoFieldMappingViewModel>)(item =>
            {
                if (ExecuteVisualizeCommandInChooseGeoFieldsState == null)
                    return;
                ExecuteVisualizeCommandInChooseGeoFieldsState();
            });
            GeoFieldMappings.ItemPropertyChanged += GeoFieldMappingItemPropertyChanged;
        }

        public void Refresh()
        {
            GeoFieldMappingViewModel mappingViewModel1 = null;
            GeoFieldMappingViewModel mappingViewModel2 = null;
            GeoFieldMappingViewModel mappingViewModel3 = null;
            GeoFieldMappingViewModel mappingViewModel4 = null;
            foreach (GeoFieldMappingViewModel mappingViewModel5 in GeoFieldMappings)
            {
                if (mappingViewModel5.MappingType == GeoFieldMappingType.Latitude)
                    mappingViewModel1 = mappingViewModel5;
                else if (mappingViewModel5.MappingType == GeoFieldMappingType.Longitude)
                    mappingViewModel2 = mappingViewModel5;
                else if (mappingViewModel5.MappingType == GeoFieldMappingType.XCoord)
                    mappingViewModel3 = mappingViewModel5;
                else if (mappingViewModel5.MappingType == GeoFieldMappingType.YCoord)
                    mappingViewModel4 = mappingViewModel5;
            }
            if (SelectedGeoMapping == null)
                return;
            if (ReferenceEquals(SelectedGeoMapping, mappingViewModel1) || ReferenceEquals(SelectedGeoMapping, mappingViewModel2))
                MapByDisplayString = string.Format(Resources.FieldWellSummary_MapByDisplayString, string.Format(Resources.GeoFieldMappingViewModel_LatLong, mappingViewModel1.Field.Name, mappingViewModel2.Field.Name));
            else if (ReferenceEquals(SelectedGeoMapping, mappingViewModel3) || ReferenceEquals(SelectedGeoMapping, mappingViewModel4))
                MapByDisplayString = string.Format(Resources.FieldWellSummary_MapByDisplayString, string.Format(Resources.GeoFieldMappingViewModel_XY, mappingViewModel3.Field.Name, mappingViewModel4.Field.Name));
            else
                MapByDisplayString = string.Format(Resources.FieldWellSummary_MapByDisplayString, SelectedGeoMapping.MapByDisplayString);
        }

        private void GeoFieldMappingItemPropertyChanged(GeoFieldMappingViewModel changedItem, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == GeoFieldMappingViewModel.PropertyIsMapByField)
            {
                if (!changedItem.IsMapByField)
                    return;
                if (SelectedGeoMapping != null)
                    SelectedGeoMapping.IsMapByField = false;
                SelectedGeoMapping = changedItem;
                Refresh();
                if (ExecuteVisualizeCommandInChooseGeoFieldsState == null)
                    return;
                ExecuteVisualizeCommandInChooseGeoFieldsState();
            }
            else
            {
                if (!(e.PropertyName == GeoFieldMappingViewModel.PropertyMappingType) || ExecuteVisualizeCommandInChooseGeoFieldsState == null)
                    return;
                ExecuteVisualizeCommandInChooseGeoFieldsState();
            }
        }

        public void AddGeoField(TableFieldViewModel field)
        {
            AddGeoField(field, GeoFieldMappings.Count);
        }

        private void AddGeoField(TableFieldViewModel field, int index)
        {
            AddGeoField(field, index, new GeoFieldMappingViewModel(field, true)
            {
                ChartTypeIsRegion = ChartTypeIsRegion
            });
        }

        public void AddGeoField(TableFieldViewModel field, int index, GeoFieldMappingViewModel geoField)
        {
            if (field == null || !GeoFieldMappingsDropHandler.CanDropItem(field))
                return;
            geoField.RemoveOptionSelected += (Action<GeoFieldMappingViewModel>)(removedGeoField => removedGeoField.Field.IsSelected = false);
            GeoFieldMappings.Insert(index, geoField);
            field.IsSelected = true;
            CommandManager.InvalidateRequerySuggested();
        }

        public void RemoveGeoField(TableFieldViewModel field)
        {
            GeoFieldMappingViewModel mappingViewModel1 = null;
            foreach (GeoFieldMappingViewModel mappingViewModel2 in GeoFieldMappings)
            {
                if (mappingViewModel2.Field == field)
                {
                    mappingViewModel1 = mappingViewModel2;
                    break;
                }
            }
            if (mappingViewModel1 != null)
                GeoFieldMappings.Remove(mappingViewModel1);
            CommandManager.InvalidateRequerySuggested();
        }

        public void DetectMostAccurateMapByField(bool resetMapBy)
        {
            GeoFieldMappingViewModel mappingViewModel1 = null;
            foreach (GeoFieldMappingViewModel mappingViewModel2 in GeoFieldMappings)
            {
                if (!resetMapBy && mappingViewModel2.IsMapByField)
                {
                    mappingViewModel1 = null;
                    break;
                }
                else if (mappingViewModel1 == null)
                    mappingViewModel1 = mappingViewModel2;
                else if (GeoFieldMappingTypeUtil.MappingOrder(mappingViewModel2.MappingType) < GeoFieldMappingTypeUtil.MappingOrder(mappingViewModel1.MappingType))
                    mappingViewModel1 = mappingViewModel2;
            }
            if (mappingViewModel1 == null)
                return;
            mappingViewModel1.IsMapByField = true;
            mappingViewModel1.UserSelectedMapByField = false;
        }

        public bool DetectMostAccurateMapByFieldForRegionChart(bool resetMapBy)
        {
            GeoFieldMappingViewModel mappingViewModel1 = null;
            foreach (GeoFieldMappingViewModel mappingViewModel2 in GeoFieldMappings)
            {
                if (!resetMapBy && mappingViewModel2.IsMapByField && GeoFieldMappingTypeUtil.SupportsRegions(mappingViewModel2.MappingType))
                    return true;
                if (GeoFieldMappingTypeUtil.SupportsRegions(mappingViewModel2.MappingType))
                {
                    if (mappingViewModel1 == null)
                        mappingViewModel1 = mappingViewModel2;
                    else if (GeoFieldMappingTypeUtil.MappingOrder(mappingViewModel2.MappingType) < GeoFieldMappingTypeUtil.MappingOrder(mappingViewModel1.MappingType))
                        mappingViewModel1 = mappingViewModel2;
                }
            }
            if (mappingViewModel1 == null)
                return false;
            mappingViewModel1.IsMapByField = true;
            mappingViewModel1.UserSelectedMapByField = false;
            return true;
        }

        private void OnExecuteViewReport(IDialogServiceProvider dialogProvider)
        {
            if (dialogProvider == null)
                return;
            dialogProvider.ShowDialog(GeocodingReport);
        }
    }
}
