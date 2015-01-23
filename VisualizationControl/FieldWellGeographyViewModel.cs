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
        return this._NextCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyNextCommand, ref this._NextCommand, value, false);
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
        return this._BackgroundChangeYesCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyBackgroundChangeYesCommand, ref this._BackgroundChangeYesCommand, value, false);
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
        return this._BackgroundChangeNoCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyBackgroundChangeNoCommand, ref this._BackgroundChangeNoCommand, value, false);
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
        return this._EditCommand;
      }
      set
      {
        this.SetProperty<ICommand>(FieldWellGeographyViewModel.PropertyEditCommand, ref this._EditCommand, value, false);
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
        return this._SelectedGeoMapping;
      }
      set
      {
        this.SetProperty<GeoFieldMappingViewModel>(FieldWellGeographyViewModel.PropertySelectedGeoMapping, ref this._SelectedGeoMapping, value, false);
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
        return this._GeocodingReport;
      }
      set
      {
        this.SetProperty<GeocodingReportViewModel>(this.PropertyGeocodingReport, ref this._GeocodingReport, value, false);
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
        return this._MapByDisplayString;
      }
      private set
      {
        this.SetProperty<string>(FieldWellGeographyViewModel.PropertyMapByDisplayString, ref this._MapByDisplayString, value, false);
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
        return this._DisplayedErrorMessage;
      }
      set
      {
        this.SetProperty<string>(FieldWellGeographyViewModel.PropertyDisplayedErrorMessage, ref this._DisplayedErrorMessage, value, false);
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
        return this._IsSettingMapByFieldEnabled;
      }
      set
      {
        this.SetProperty<bool>(FieldWellGeographyViewModel.PropertyIsSettingMapByFieldEnabled, ref this._IsSettingMapByFieldEnabled, value, false);
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
        return this._IsBackgroundEditable;
      }
      set
      {
        this.SetProperty<bool>(FieldWellGeographyViewModel.PropertyIsBackgroundEditable, ref this._IsBackgroundEditable, value, false);
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
        return this._IsBackgroundPromptApplicable;
      }
      set
      {
        this.SetProperty<bool>(FieldWellGeographyViewModel.PropertyIsBackgroundPromptApplicable, ref this._IsBackgroundPromptApplicable, value, false);
      }
    }

    public ICommand DismissErrorCommand
    {
      get
      {
        return this._DismissErrorCommand;
      }
    }

    public ObservableCollectionEx<GeoFieldMappingViewModel> GeoFieldMappings { get; private set; }

    public ICommand ViewReportCommand { get; private set; }

    public DropItemsHandler GeoFieldMappingsDropHandler { get; private set; }

    public FieldWellGeographyViewModel(IDialogServiceProvider dialogProvider)
    {
      FieldWellGeographyViewModel geographyViewModel = this;
      this.GeoFieldMappings = new ObservableCollectionEx<GeoFieldMappingViewModel>();
      this.GeoFieldMappingsDropHandler = new DropItemsHandler();
      this.GeoFieldMappingsDropHandler.AddDroppableTypeHandlers<TableFieldViewModel>((DropItemIntoCollectionDelegate<TableFieldViewModel>) ((item, index) => this.AddGeoField(item, index)), (ValidateDropItemDelegate<TableFieldViewModel>) (item =>
      {
        if (item.IsTableMeasure)
          return Resources.FieldListPicker_Select_Geography;
        foreach (GeoFieldMappingViewModel mappingViewModel in (Collection<GeoFieldMappingViewModel>) this.GeoFieldMappings)
        {
          if (mappingViewModel.Field == item)
            return Resources.FieldWellGeography_DuplicateFieldExists;
        }
        return (string) null;
      }));
      this.ViewReportCommand = (ICommand) new DelegatedCommand((Action) (() => geographyViewModel.OnExecuteViewReport(dialogProvider)));
      this._DismissErrorCommand = (ICommand) new DelegatedCommand((Action) (() => this.DisplayedErrorMessage = (string) null));
    }

    public void Initialize()
    {
      this.GeoFieldMappings.ItemRemoved += (ObservableCollectionExChangedHandler<GeoFieldMappingViewModel>) (item =>
      {
        item.IsMapByField = false;
        if (item == this.SelectedGeoMapping)
          this.SelectedGeoMapping = (GeoFieldMappingViewModel) null;
        if (this.ExecuteVisualizeCommandInChooseGeoFieldsState == null)
          return;
        this.ExecuteVisualizeCommandInChooseGeoFieldsState();
      });
      this.GeoFieldMappings.ItemAdded += (ObservableCollectionExChangedHandler<GeoFieldMappingViewModel>) (item =>
      {
        if (this.ExecuteVisualizeCommandInChooseGeoFieldsState == null)
          return;
        this.ExecuteVisualizeCommandInChooseGeoFieldsState();
      });
      this.GeoFieldMappings.ItemPropertyChanged += new ObservableCollectionExItemChangedHandler<GeoFieldMappingViewModel>(this.GeoFieldMappingItemPropertyChanged);
    }

    public void Refresh()
    {
      GeoFieldMappingViewModel mappingViewModel1 = (GeoFieldMappingViewModel) null;
      GeoFieldMappingViewModel mappingViewModel2 = (GeoFieldMappingViewModel) null;
      GeoFieldMappingViewModel mappingViewModel3 = (GeoFieldMappingViewModel) null;
      GeoFieldMappingViewModel mappingViewModel4 = (GeoFieldMappingViewModel) null;
      foreach (GeoFieldMappingViewModel mappingViewModel5 in (Collection<GeoFieldMappingViewModel>) this.GeoFieldMappings)
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
      if (this.SelectedGeoMapping == null)
        return;
      if (object.ReferenceEquals((object) this.SelectedGeoMapping, (object) mappingViewModel1) || object.ReferenceEquals((object) this.SelectedGeoMapping, (object) mappingViewModel2))
        this.MapByDisplayString = string.Format(Resources.FieldWellSummary_MapByDisplayString, (object) string.Format(Resources.GeoFieldMappingViewModel_LatLong, (object) mappingViewModel1.Field.Name, (object) mappingViewModel2.Field.Name));
      else if (object.ReferenceEquals((object) this.SelectedGeoMapping, (object) mappingViewModel3) || object.ReferenceEquals((object) this.SelectedGeoMapping, (object) mappingViewModel4))
        this.MapByDisplayString = string.Format(Resources.FieldWellSummary_MapByDisplayString, (object) string.Format(Resources.GeoFieldMappingViewModel_XY, (object) mappingViewModel3.Field.Name, (object) mappingViewModel4.Field.Name));
      else
        this.MapByDisplayString = string.Format(Resources.FieldWellSummary_MapByDisplayString, (object) this.SelectedGeoMapping.MapByDisplayString);
    }

    private void GeoFieldMappingItemPropertyChanged(GeoFieldMappingViewModel changedItem, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == GeoFieldMappingViewModel.PropertyIsMapByField)
      {
        if (!changedItem.IsMapByField)
          return;
        if (this.SelectedGeoMapping != null)
          this.SelectedGeoMapping.IsMapByField = false;
        this.SelectedGeoMapping = changedItem;
        this.Refresh();
        if (this.ExecuteVisualizeCommandInChooseGeoFieldsState == null)
          return;
        this.ExecuteVisualizeCommandInChooseGeoFieldsState();
      }
      else
      {
        if (!(e.PropertyName == GeoFieldMappingViewModel.PropertyMappingType) || this.ExecuteVisualizeCommandInChooseGeoFieldsState == null)
          return;
        this.ExecuteVisualizeCommandInChooseGeoFieldsState();
      }
    }

    public void AddGeoField(TableFieldViewModel field)
    {
      this.AddGeoField(field, this.GeoFieldMappings.Count);
    }

    private void AddGeoField(TableFieldViewModel field, int index)
    {
      this.AddGeoField(field, index, new GeoFieldMappingViewModel(field, true)
      {
        ChartTypeIsRegion = this.ChartTypeIsRegion
      });
    }

    public void AddGeoField(TableFieldViewModel field, int index, GeoFieldMappingViewModel geoField)
    {
      if (field == null || !this.GeoFieldMappingsDropHandler.CanDropItem((object) field))
        return;
      geoField.RemoveOptionSelected += (Action<GeoFieldMappingViewModel>) (removedGeoField => removedGeoField.Field.IsSelected = false);
      this.GeoFieldMappings.Insert(index, geoField);
      field.IsSelected = true;
      CommandManager.InvalidateRequerySuggested();
    }

    public void RemoveGeoField(TableFieldViewModel field)
    {
      GeoFieldMappingViewModel mappingViewModel1 = (GeoFieldMappingViewModel) null;
      foreach (GeoFieldMappingViewModel mappingViewModel2 in (Collection<GeoFieldMappingViewModel>) this.GeoFieldMappings)
      {
        if (mappingViewModel2.Field == field)
        {
          mappingViewModel1 = mappingViewModel2;
          break;
        }
      }
      if (mappingViewModel1 != null)
        this.GeoFieldMappings.Remove(mappingViewModel1);
      CommandManager.InvalidateRequerySuggested();
    }

    public void DetectMostAccurateMapByField(bool resetMapBy)
    {
      GeoFieldMappingViewModel mappingViewModel1 = (GeoFieldMappingViewModel) null;
      foreach (GeoFieldMappingViewModel mappingViewModel2 in (Collection<GeoFieldMappingViewModel>) this.GeoFieldMappings)
      {
        if (!resetMapBy && mappingViewModel2.IsMapByField)
        {
          mappingViewModel1 = (GeoFieldMappingViewModel) null;
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
      GeoFieldMappingViewModel mappingViewModel1 = (GeoFieldMappingViewModel) null;
      foreach (GeoFieldMappingViewModel mappingViewModel2 in (Collection<GeoFieldMappingViewModel>) this.GeoFieldMappings)
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
      dialogProvider.ShowDialog((IDialog) this.GeocodingReport);
    }
  }
}
