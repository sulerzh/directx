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
  public class FieldWellVisualizationViewModel : ViewModelBase
  {
    public static Dictionary<RegionLayerShadingMode, string> RegionShadingSettings = new Dictionary<RegionLayerShadingMode, string>()
    {
      {
        RegionLayerShadingMode.Local,
        Resources.RegionShadingModeDisplayString_Local
      },
      {
        RegionLayerShadingMode.Global,
        Resources.RegionShadingModeDisplayString_Global
      },
      {
        RegionLayerShadingMode.ShiftGlobal,
        Resources.RegionShadingModeDisplayString_ShiftGlobal
      },
      {
        RegionLayerShadingMode.FullBleed,
        Resources.RegionShadingModeDisplayString_FullBleed
      }
    };
    private List<TableFieldViewModel> _droppedTableFields = new List<TableFieldViewModel>();
    private LayerType _SelectedVisualizationType = LayerType.PointMarkerChart;
    private bool _CategoryEnabled = true;
    private bool _CategoryVisible = true;
    private ComboBoxItemViewModel TimeSetting_Instant = new ComboBoxItemViewModel()
    {
      Content = (object) Resources.TimeSettings_Instant
    };
    private ComboBoxItemViewModel TimeSetting_TimeAccumulation = new ComboBoxItemViewModel()
    {
      Content = (object) Resources.TimeSettings_TimeAccumulation
    };
    private ComboBoxItemViewModel TimeSetting_PersistLast = new ComboBoxItemViewModel()
    {
      Content = (object) Resources.TimeSettings_PersistLast
    };
    private List<FieldWellHeightViewModel> _removeHeightFieldsCache = new List<FieldWellHeightViewModel>();
    private bool timeSettingChangedInternally;
    private int _deferredVisualizationCounterSync;
    private VisualizationChartType _ChartType;
    public Action<bool> VisualizeCommand;
    public Func<bool, bool, bool> DetectMostAccurateMapByFieldForRegionChart;
    public Func<GeoFieldMappingViewModel> GetGeoFieldMapping;
    private bool _CanVisualShapeBeChanged;
    private bool _HasCategory;
    private bool _CategorySettingsVisible;
    private bool _HasMoreThanOneHeightField;
    private RegionLayerShadingMode _SelectedRegionShadingSetting;
    private ComboBoxItemViewModel _SelectedTimeSetting;

    public bool VisualizationEnabled
    {
      get
      {
        return this._deferredVisualizationCounterSync == 0;
      }
    }

    public TimeSetting UserSelectedTimeSetting { get; set; }

    public string PropertyChartType
    {
      get
      {
        return "ChartType";
      }
    }

    public VisualizationChartType ChartType
    {
      get
      {
        return this._ChartType;
      }
      set
      {
        if (value != this._ChartType && value == VisualizationChartType.Region)
        {
          this.DisallowVisualization();
          if (this.DetectMostAccurateMapByFieldForRegionChart != null && !this.DetectMostAccurateMapByFieldForRegionChart(true, false))
          {
            this.AllowVisualization();
            return;
          }
          else
            this.AllowVisualization();
        }
        if (!this.SetProperty<VisualizationChartType>(this.PropertyChartType, ref this._ChartType, value, false))
          return;
        if (value != VisualizationChartType.HeatMap && value != VisualizationChartType.Region && this.SelectedVisualizationType == LayerType.PointMarkerChart)
          this.InvokeDeferredVisualizationAction((Action) null);
        else
          this.InvokeDeferredVisualizationAction((Action) (() =>
          {
            if (value == VisualizationChartType.HeatMap)
            {
              if (this.SelectedCategory.Value != null)
                this.SelectedCategory.Value.RemoveEntry();
              while (this.HeightFields.Count > 1)
                this.HeightFields[1].RemoveEntry();
            }
            else if (value == VisualizationChartType.Region)
            {
              foreach (FieldWellHeightViewModel wellHeightViewModel in (Collection<FieldWellHeightViewModel>) this.HeightFields)
              {
                if (wellHeightViewModel.AggregationFunction == AggregationFunction.None)
                {
                  AggregationFunction aggregationFunction = wellHeightViewModel.TableField.DefaultAggregationFunction();
                  wellHeightViewModel.SelectedDropDownOption = (object) aggregationFunction;
                }
                wellHeightViewModel.RemoveAggregationFunction(AggregationFunction.None);
              }
            }
            this.RefreshConditionalControls();
          }));
        if (value == VisualizationChartType.Region)
          return;
        foreach (FieldWellHeightViewModel wellHeightViewModel in (Collection<FieldWellHeightViewModel>) this.HeightFields)
          wellHeightViewModel.RestoreAllowedAggregationFunctions();
      }
    }

    public bool DisplayCategoryCounts { get; private set; }

    public ObservableCollectionEx<FieldWellHeightViewModel> HeightFields { get; private set; }

    public string PropertySelectedVisualizationType
    {
      get
      {
        return "SelectedVisualizationType";
      }
    }

    public LayerType SelectedVisualizationType
    {
      get
      {
        return this._SelectedVisualizationType;
      }
      set
      {
        if (!this.SetProperty<LayerType>(this.PropertySelectedVisualizationType, ref this._SelectedVisualizationType, value, false))
          return;
        this.CanVisualShapeBeChanged = value != LayerType.RegionChart && value != LayerType.PieChart && value != LayerType.HeatMapChart && value != LayerType.PointMarkerChart;
        this.InvokeDeferredVisualizationAction((Action) null);
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
        return this._CanVisualShapeBeChanged;
      }
      private set
      {
        this.SetProperty<bool>(FieldWellVisualizationViewModel.PropertyCanVisualShapeBeChanged, ref this._CanVisualShapeBeChanged, value, false);
      }
    }

    public NPCContainer<FieldWellCategoryViewModel> SelectedCategory { get; private set; }

    public NPCContainer<FieldWellTimeViewModel> SelectedTimeField { get; private set; }

    public DropItemHandler SelectedCategoryDropHandler { get; private set; }

    public IDragItemHandler SelectedCategoryDragHandler { get; private set; }

    public DropItemHandler SelectedTimeFieldDropHandler { get; private set; }

    public IDragItemHandler SelectedTimeFieldDragHandler { get; private set; }

    public DropItemHandler SelectedColorFieldDropHandler { get; private set; }

    public bool AccumulateResultsOverTime
    {
      get
      {
        return this.SelectedTimeSetting == this.TimeSetting_TimeAccumulation;
      }
    }

    public bool PersistTimeData
    {
      get
      {
        return this.SelectedTimeSetting == this.TimeSetting_PersistLast;
      }
    }

    public DropItemsHandler HeightFieldsDropHandler { get; private set; }

    public IDragHandler HeightFieldsDragHandler { get; private set; }

    public string PropertyCategoryEnabled
    {
      get
      {
        return "CategoryEnabled";
      }
    }

    public bool CategoryEnabled
    {
      get
      {
        return this._CategoryEnabled;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyCategoryEnabled, ref this._CategoryEnabled, value, false);
      }
    }

    public string PropertyCategoryVisible
    {
      get
      {
        return "CategoryVisible";
      }
    }

    public bool CategoryVisible
    {
      get
      {
        return this._CategoryVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyCategoryVisible, ref this._CategoryVisible, value, false);
      }
    }

    public string PropertyHasCategory
    {
      get
      {
        return "HasCategory";
      }
    }

    public bool HasCategory
    {
      get
      {
        return this._HasCategory;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasCategory, ref this._HasCategory, value, false);
      }
    }

    public string PropertyCategorySettingsVisible
    {
      get
      {
        return "CategorySettingsVisible";
      }
    }

    public bool CategorySettingsVisible
    {
      get
      {
        return this._CategorySettingsVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyCategorySettingsVisible, ref this._CategorySettingsVisible, value, false);
      }
    }

    public string PropertyHasMoreThanOneHeightField
    {
      get
      {
        return "HasMoreThanOneHeightField";
      }
    }

    public bool HasMoreThanOneHeightField
    {
      get
      {
        return this._HasMoreThanOneHeightField;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasMoreThanOneHeightField, ref this._HasMoreThanOneHeightField, value, false);
      }
    }

    public ObservableCollectionEx<GeoFieldMappingViewModel> GeoMappings { get; set; }

    public ObservableCollectionEx<ComboBoxItemViewModel> TimeSettings { get; private set; }

    internal Action<RegionLayerShadingMode> SetRegionShadingSetting { private get; set; }

    public string PropertySelectedRegionShadingSetting
    {
      get
      {
        return "SelectedRegionShadingSetting";
      }
    }

    public RegionLayerShadingMode SelectedRegionShadingSetting
    {
      get
      {
        return this._SelectedRegionShadingSetting;
      }
      set
      {
        this.SetRegionShadingSetting(value);
      }
    }

    public string PropertySelectedTimeSetting
    {
      get
      {
        return "SelectedTimeSetting";
      }
    }

    public ComboBoxItemViewModel SelectedTimeSetting
    {
      get
      {
        return this._SelectedTimeSetting;
      }
      set
      {
        if (value == this._SelectedTimeSetting)
          return;
        if (value == this.TimeSetting_PersistLast || value == this.TimeSetting_Instant)
        {
          this.SetProperty<ComboBoxItemViewModel>(this.PropertySelectedTimeSetting, ref this._SelectedTimeSetting, value, false);
          if (this.SelectedTimeField != null && this.SelectedTimeField.Value != null)
            this.InvokeDeferredVisualizationAction((Action) null);
          if (this.timeSettingChangedInternally)
            return;
          if (value == this.TimeSetting_PersistLast)
          {
            this.UserSelectedTimeSetting = TimeSetting.PersistLast;
          }
          else
          {
            if (value != this.TimeSetting_Instant)
              return;
            this.UserSelectedTimeSetting = TimeSetting.Instant;
          }
        }
        else
        {
          if (value != this.TimeSetting_TimeAccumulation)
            return;
          bool flag = false;
          if (this.HeightFields.Count > 0)
            flag = Queryable.Any<FieldWellHeightViewModel>(Queryable.AsQueryable<FieldWellHeightViewModel>((IEnumerable<FieldWellHeightViewModel>) this.HeightFields), (System.Linq.Expressions.Expression<Func<FieldWellHeightViewModel, bool>>) (heightField => (int) heightField.AggregationFunction == 0 || (int) heightField.AggregationFunction == 6 || (int) heightField.AggregationFunction == 7 || (int) heightField.AggregationFunction == 2));
          if (flag || !this.SetProperty<ComboBoxItemViewModel>(this.PropertySelectedTimeSetting, ref this._SelectedTimeSetting, value, false))
            return;
          if (this.SelectedTimeField != null && this.SelectedTimeField.Value != null)
            this.InvokeDeferredVisualizationAction((Action) null);
          if (this.timeSettingChangedInternally)
            return;
          this.UserSelectedTimeSetting = TimeSetting.Accumulate;
        }
      }
    }

    public event Action CategoryAdded;

    public event Action<int> HeightFieldAdded;

    public FieldWellVisualizationViewModel(RegionLayerShadingMode shadingMode = RegionLayerShadingMode.FullBleed)
    {
      this._SelectedRegionShadingSetting = shadingMode;
      this.HeightFields = new ObservableCollectionEx<FieldWellHeightViewModel>();
      this.HeightFields.ItemDescendentPropertyChanged += (ObservableCollectionExItemChangedHandler<FieldWellHeightViewModel>) ((sender, e) => this.InvokeDeferredVisualizationAction((Action) (() => this.OnSelectedTableFieldsChanged())));
      this.HeightFields.ItemAdded += (ObservableCollectionExChangedHandler<FieldWellHeightViewModel>) (item =>
      {
        this.InvokeDeferredVisualizationAction((Action) (() =>
        {
          this.OnHeightFieldAggregationFunctionChanged(item);
          this.RefreshConditionalControls();
        }));
        if (this.HeightFieldAdded == null)
          return;
        this.HeightFieldAdded(this.HeightFields.Count);
      });
      this.HeightFields.ItemPropertyChanged += (ObservableCollectionExItemChangedHandler<FieldWellHeightViewModel>) ((sender, e) =>
      {
        if (!(e.PropertyName == FieldWellHeightViewModel.PropertyAggregationFunction))
          return;
        this.InvokeDeferredVisualizationAction((Action) (() => this.OnHeightFieldAggregationFunctionChanged(sender)));
      });
      this.HeightFields.ItemRemoved += (ObservableCollectionExChangedHandler<FieldWellHeightViewModel>) (item => this.InvokeDeferredVisualizationAction((Action) (() =>
      {
        this.RefreshConditionalControls();
        this.RefreshTimeSettings();
      })));
      this.SelectedTimeField = new NPCContainer<FieldWellTimeViewModel>();
      this.SelectedTimeField.ValueDescendentPropertyChanged += (PropertyChangedEventHandler) ((s, e) => this.InvokeDeferredVisualizationAction((Action) (() => this.OnSelectedTableFieldsChanged())));
      this.SelectedTimeField.ValuePropertyChanged += (PropertyChangedEventHandler) ((s, e) =>
      {
        if (!(e.PropertyName == FieldWellTimeViewModel.PropertyTimeChunk))
          return;
        this.InvokeDeferredVisualizationAction((Action) null);
      });
      this.SelectedTimeField.PropertyChanged += (PropertyChangedEventHandler) ((s, e) => this.InvokeDeferredVisualizationAction((Action) null));
      this.SelectedCategory = new NPCContainer<FieldWellCategoryViewModel>();
      this.SelectedCategory.ValueDescendentPropertyChanged += (PropertyChangedEventHandler) ((s, e) => this.InvokeDeferredVisualizationAction((Action) (() => this.OnSelectedTableFieldsChanged())));
      this.SelectedCategory.PropertyChanged += (PropertyChangedEventHandler) ((s, e) => this.InvokeDeferredVisualizationAction((Action) (() =>
      {
        this.RefreshConditionalControls();
        this.RefreshTimeSettings();
      })));
      this.TimeSettings = new ObservableCollectionEx<ComboBoxItemViewModel>();
      this.TimeSettings.Add(this.TimeSetting_Instant);
      this.TimeSettings.Add(this.TimeSetting_TimeAccumulation);
      this.TimeSettings.Add(this.TimeSetting_PersistLast);
      this.SetUpDragDropBehavior();
    }

    public void SetAccumulateResultsOverTime(bool overrideUserSetting)
    {
      this.timeSettingChangedInternally = true;
      if (overrideUserSetting || this.UserSelectedTimeSetting == TimeSetting.None || this.UserSelectedTimeSetting == TimeSetting.Accumulate)
        this.SelectedTimeSetting = this.TimeSetting_TimeAccumulation;
      this.timeSettingChangedInternally = false;
    }

    public void SetPersistTimeData(bool overrideUserSetting)
    {
      this.timeSettingChangedInternally = true;
      if (overrideUserSetting || this.UserSelectedTimeSetting != TimeSetting.Instant)
        this.SelectedTimeSetting = this.TimeSetting_PersistLast;
      this.timeSettingChangedInternally = false;
    }

    public void SetInstantTimeData()
    {
      this.timeSettingChangedInternally = true;
      this.SelectedTimeSetting = this.TimeSetting_Instant;
      this.timeSettingChangedInternally = false;
    }

    internal void OnRegionLayerShadingModeChanged(RegionLayerShadingMode value)
    {
      this._SelectedRegionShadingSetting = value;
      this.RaisePropertyChanged(this.PropertySelectedRegionShadingSetting);
    }

    private void SetUpDragDropBehavior()
    {
      this.SelectedTimeFieldDropHandler = new DropItemHandler();
      this.SelectedTimeFieldDropHandler.AddDroppableTypeHandlers<TableFieldViewModel>((Action<TableFieldViewModel>) (item => this.SetSelectedTimeField(item)), (ValidateDropItemDelegate<TableFieldViewModel>) (item => this.ValidateDropItemIntoTime(item)));
      this.SelectedTimeFieldDropHandler.AddDroppableTypeHandlers<FieldWellHeightViewModel>((Action<FieldWellHeightViewModel>) (item => this.SetSelectedTimeField(item.TableField)), (ValidateDropItemDelegate<FieldWellHeightViewModel>) (item => this.ValidateDropItemIntoTime(item.TableField)));
      this.SelectedTimeFieldDropHandler.AddDroppableTypeHandlers<FieldWellCategoryViewModel>((Action<FieldWellCategoryViewModel>) (item => this.SetSelectedTimeField(item.TableField)), (ValidateDropItemDelegate<FieldWellCategoryViewModel>) (item => this.ValidateDropItemIntoTime(item.TableField)));
      this.SelectedTimeFieldDragHandler = (IDragItemHandler) new DragItemHandler<FieldWellTimeViewModel>((ProcessDraggedItemDelegate<FieldWellTimeViewModel>) ((item, effects) => this.InvokeDeferredVisualizationAction((Action) (() => this.SelectedTimeField.Value = (FieldWellTimeViewModel) null))));
      this.SelectedCategoryDropHandler = new DropItemHandler();
      this.SelectedCategoryDropHandler.AddDroppableTypeHandlers<TableFieldViewModel>((Action<TableFieldViewModel>) (item => this.SetSelectedCategoryField(item)), (ValidateDropItemDelegate<TableFieldViewModel>) (item => this.ValidateDropItemIntoCategory(item)));
      this.SelectedCategoryDropHandler.AddDroppableTypeHandlers<FieldWellHeightViewModel>((Action<FieldWellHeightViewModel>) (item => this.SetSelectedCategoryField(item)), (ValidateDropItemDelegate<FieldWellHeightViewModel>) (item => this.ValidateDropItemIntoCategory(item)));
      this.SelectedCategoryDropHandler.AddDroppableTypeHandlers<FieldWellTimeViewModel>((Action<FieldWellTimeViewModel>) (item => this.SetSelectedCategoryField(item.TableField)), (ValidateDropItemDelegate<FieldWellTimeViewModel>) (item => this.ValidateDropItemIntoCategory(item.TableField)));
      this.SelectedCategoryDragHandler = (IDragItemHandler) new DragItemHandler<FieldWellCategoryViewModel>((ProcessDraggedItemDelegate<FieldWellCategoryViewModel>) ((item, effects) => this.InvokeDeferredVisualizationAction((Action) (() =>
      {
        this.SelectedCategory.Value = (FieldWellCategoryViewModel) null;
        this.RefreshTimeSettings();
      }))));
      this.HeightFieldsDropHandler = new DropItemsHandler();
      this.HeightFieldsDropHandler.AddDroppableTypeHandlers<TableFieldViewModel>((DropItemIntoCollectionDelegate<TableFieldViewModel>) ((item, index) => this.AddToHeightField(index, item)), (ValidateDropItemDelegate<TableFieldViewModel>) (item => this.ValidateDropItemIntoHeights(item)));
      this.HeightFieldsDropHandler.AddDroppableTypeHandlers<FieldWellCategoryViewModel>((DropItemIntoCollectionDelegate<FieldWellCategoryViewModel>) ((item, index) => this.AddToHeightField(index, (FieldWellEntryViewModel) item)), (ValidateDropItemDelegate<FieldWellCategoryViewModel>) (item => this.ValidateDropItemIntoHeights(item)));
      this.HeightFieldsDropHandler.AddDroppableTypeHandlers<FieldWellTimeViewModel>((DropItemIntoCollectionDelegate<FieldWellTimeViewModel>) ((item, index) => this.AddToHeightField(index, (FieldWellEntryViewModel) item)), (ValidateDropItemDelegate<FieldWellTimeViewModel>) (item => this.ValidateDropItemIntoTime(item.TableField)));
      this.HeightFieldsDropHandler.AddDroppableTypeHandlers<FieldWellHeightViewModel>((DropItemIntoCollectionDelegate<FieldWellHeightViewModel>) ((item, index) => this.HeightFields.Insert(index, item)), (ValidateDropItemDelegate<FieldWellHeightViewModel>) null);
      this.HeightFieldsDragHandler = (IDragHandler) new DragItemsHandler<FieldWellHeightViewModel>((Collection<FieldWellHeightViewModel>) this.HeightFields, false);
    }

    public void AddHeight(TableFieldViewModel tableField)
    {
      string messageBoxText = this.ValidateDropItemIntoHeights(tableField);
      if (messageBoxText != null)
      {
        int num = (int) MessageBox.Show(messageBoxText, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK, Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
      }
      else
        this.AddToHeightField(this.HeightFields.Count, tableField);
    }

    public void AddCategory(TableFieldViewModel tableField)
    {
      string messageBoxText = this.ValidateDropItemIntoCategory(tableField);
      if (messageBoxText != null)
      {
        int num = (int) MessageBox.Show(messageBoxText, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK, Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
      }
      else
        this.SetSelectedCategoryField(tableField);
    }

    public void AddTime(TableFieldViewModel tableField)
    {
      string messageBoxText = this.ValidateDropItemIntoTime(tableField);
      if (messageBoxText != null)
      {
        int num = (int) MessageBox.Show(messageBoxText, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK, Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
      }
      else
        this.SetSelectedTimeField(tableField);
    }

    private string ValidateDropItemIntoTime(TableFieldViewModel tableField)
    {
      if (!tableField.IsTimeField)
        return Resources.FieldWellVisualization_InvalidTimeField;
      else
        return (string) null;
    }

    private string ValidateDropItemIntoCategory(TableFieldViewModel tableField)
    {
      if (!tableField.IsCategory)
        return Resources.FieldWellVisualization_InvalidCategoryField;
      if (this.ChartType == VisualizationChartType.HeatMap)
        return Resources.FieldWellVisualization_Error_CannotDropCategoryIntoHeatMapChart;
      if (this.HeightFields.Count >= 2)
      {
        switch (this.ChartType)
        {
          case VisualizationChartType.StackedColumn:
          case VisualizationChartType.ClusteredColumn:
            return Resources.FieldWellVisualization_Error_CannotDropCategoryWhenThereAreTwoOrMoreHeights;
          case VisualizationChartType.Bubble:
            return Resources.FieldWellVisualization_Error_CannotDropCategoryWhenThereAreTwoOrMoreSizes;
          case VisualizationChartType.Region:
            return Resources.FieldWellVisualization_Error_CannotDropCategoryWhenThereAreTwoOrMoreValues;
        }
      }
      return (string) null;
    }

    private string ValidateDropItemIntoCategory(FieldWellHeightViewModel item)
    {
      if (item == null)
        return string.Empty;
      if (!item.TableField.IsCategory)
        return Resources.FieldWellVisualization_InvalidCategoryField;
      if (this.ChartType == VisualizationChartType.HeatMap)
        return Resources.FieldWellVisualization_Error_CannotDropCategoryIntoHeatMapChart;
      if (this.HeightFields.Count >= 3)
      {
        switch (this.ChartType)
        {
          case VisualizationChartType.StackedColumn:
          case VisualizationChartType.ClusteredColumn:
            return Resources.FieldWellVisualization_Error_CannotDropCategoryWhenThereAreTwoOrMoreHeights;
          case VisualizationChartType.Bubble:
            return Resources.FieldWellVisualization_Error_CannotDropCategoryWhenThereAreTwoOrMoreSizes;
          case VisualizationChartType.Region:
            return Resources.FieldWellVisualization_Error_CannotDropCategoryWhenThereAreTwoOrMoreValues;
        }
      }
      return (string) null;
    }

    private string ValidateDropItemIntoHeights(TableFieldViewModel tableField)
    {
      if (this.ChartType == VisualizationChartType.HeatMap && this.HeightFields.Count == 1)
        return Resources.FieldWellVisualization_Error_CannotDropSecondHeightInHeatMap;
      if (this.SelectedCategory.Value != null && this.HeightFields.Count == 1)
      {
        switch (this.ChartType)
        {
          case VisualizationChartType.StackedColumn:
          case VisualizationChartType.ClusteredColumn:
            return Resources.FieldWellVisualization_Error_CannotDropSecondHeightWhenCategoryIsSelected;
          case VisualizationChartType.Bubble:
            return Resources.FieldWellVisualization_Error_CannotDropSecondSizeWhenCategoryIsSelected;
          case VisualizationChartType.Region:
            return Resources.FieldWellVisualization_Error_CannotDropSecondValueWhenCategoryIsSelected;
        }
      }
      return (string) null;
    }

    private string ValidateDropItemIntoHeights(FieldWellCategoryViewModel categoryField)
    {
      if (this.ChartType == VisualizationChartType.HeatMap && this.HeightFields.Count == 1)
        return Resources.FieldWellVisualization_Error_CannotDropSecondHeightInHeatMap;
      else
        return (string) null;
    }

    public void SuppressSetIsSelected(TableFieldViewModel field)
    {
      if (field == null || field.IsSelected)
        return;
      this._droppedTableFields.Add(field);
      field.IsSelected = true;
    }

    private void AddToHeightField(int index, FieldWellEntryViewModel fieldWellEntry)
    {
      if (fieldWellEntry == null)
        return;
      FieldWellCategoryViewModel categoryViewModel = fieldWellEntry as FieldWellCategoryViewModel;
      if (categoryViewModel != null)
        categoryViewModel.RemoveEntry();
      this.AddToHeightField(index, fieldWellEntry.TableField);
    }

    private void AddToHeightField(int index, TableFieldViewModel item)
    {
      if (item == null)
        return;
      this.AddToHeightField(index, item, item.DefaultAggregationFunction());
    }

    public void AddToHeightField(int index, TableFieldViewModel item, AggregationFunction aggFn)
    {
      FieldWellHeightViewModel wellHeightViewModel = this.CreateFieldWellHeightViewModel(item, aggFn);
      this.HeightFields.Insert(index, wellHeightViewModel);
      this.SuppressSetIsSelected(item);
      CommandManager.InvalidateRequerySuggested();
      if (this.HeightFieldAdded == null)
        return;
      this.HeightFieldAdded(this.HeightFields.Count);
    }

    public void SetSelectedTimeField(TableFieldViewModel item)
    {
      this.InvokeDeferredVisualizationAction((Action) (() =>
      {
        if (this.SelectedTimeField.Value != null)
          this.SelectedTimeField.Value.RemoveCallback(this.SelectedTimeField.Value);
        NPCContainer<FieldWellTimeViewModel> selectedTimeField = this.SelectedTimeField;
        FieldWellTimeViewModel wellTimeViewModel = new FieldWellTimeViewModel()
        {
          TableField = item,
          RemoveCallback = new Action<FieldWellTimeViewModel>(this.OnFieldWellTimeRemoved)
        };
        selectedTimeField.Value = wellTimeViewModel;
        this.RefreshTimeSettings();
        this.SuppressSetIsSelected(item);
      }));
    }

    public void SetSelectedCategoryField(FieldWellHeightViewModel item)
    {
      this.InvokeDeferredVisualizationAction((Action) (() =>
      {
        item.RemoveCallback(item);
        this.SetSelectedCategoryField(item.TableField);
      }));
    }

    public void SetSelectedCategoryField(TableFieldViewModel item)
    {
      this.InvokeDeferredVisualizationAction((Action) (() =>
      {
        if (this.SelectedCategory.Value != null)
          this.SelectedCategory.Value.RemoveCallback(this.SelectedCategory.Value);
        NPCContainer<FieldWellCategoryViewModel> selectedCategory = this.SelectedCategory;
        FieldWellCategoryViewModel categoryViewModel = new FieldWellCategoryViewModel()
        {
          TableField = item,
          RemoveCallback = new Action<FieldWellCategoryViewModel>(this.OnFieldWellCategoryRemoved)
        };
        selectedCategory.Value = categoryViewModel;
        this.RefreshTimeSettings();
        this.SuppressSetIsSelected(item);
        if (this.CategoryAdded == null)
          return;
        this.CategoryAdded();
      }));
    }

    public void DisallowVisualization()
    {
      Interlocked.Increment(ref this._deferredVisualizationCounterSync);
    }

    public void AllowVisualization()
    {
      Interlocked.Decrement(ref this._deferredVisualizationCounterSync);
    }

    private void RefreshConditionalControls()
    {
      this.CategoryEnabled = this.HeightFields.Count <= 1 && this.ChartType != VisualizationChartType.HeatMap;
      this.CategoryVisible = this.ChartType != VisualizationChartType.HeatMap;
      this.HasCategory = this.SelectedCategory.Value != null;
      this.HasMoreThanOneHeightField = this.HeightFields.Count > 1;
      if (!this.HasCategory)
      {
        this.CategorySettingsVisible = false;
        this.DisplayCategoryCounts = false;
      }
      if (this.HasCategory)
      {
        if (this.HeightFields.Count > 0)
        {
          this.SelectedVisualizationType = this.ChartType != VisualizationChartType.Region ? (this.ChartType != VisualizationChartType.StackedColumn ? (this.ChartType != VisualizationChartType.ClusteredColumn ? LayerType.PieChart : LayerType.ClusteredColumnChart) : LayerType.StackedColumnChart) : LayerType.RegionChart;
          this.CategorySettingsVisible = this.ChartType == VisualizationChartType.Region;
          this.DisplayCategoryCounts = false;
        }
        else
        {
          this.DisplayCategoryCounts = this.OkayToDisplayCategoryCounts();
          if (this.DisplayCategoryCounts)
          {
            this.SelectedVisualizationType = this.ChartType != VisualizationChartType.Region ? (this.ChartType != VisualizationChartType.StackedColumn ? (this.ChartType != VisualizationChartType.ClusteredColumn ? LayerType.PieChart : LayerType.ClusteredColumnChart) : LayerType.StackedColumnChart) : LayerType.RegionChart;
            this.CategorySettingsVisible = this.ChartType == VisualizationChartType.Region;
          }
          else
          {
            this.SelectedVisualizationType = this.ChartType != VisualizationChartType.Region ? LayerType.PointMarkerChart : LayerType.RegionChart;
            this.CategorySettingsVisible = false;
          }
        }
      }
      else if (this.HasMoreThanOneHeightField)
      {
        if (this.ChartType == VisualizationChartType.Region)
          this.SelectedVisualizationType = LayerType.RegionChart;
        else if (this.ChartType == VisualizationChartType.StackedColumn)
          this.SelectedVisualizationType = LayerType.StackedColumnChart;
        else if (this.ChartType == VisualizationChartType.ClusteredColumn)
          this.SelectedVisualizationType = LayerType.ClusteredColumnChart;
        else
          this.SelectedVisualizationType = LayerType.PieChart;
      }
      else if (this.HeightFields.Count == 0)
      {
        if (this.ChartType == VisualizationChartType.Region)
          this.SelectedVisualizationType = LayerType.RegionChart;
        else if (this.ChartType == VisualizationChartType.HeatMap)
          this.SelectedVisualizationType = LayerType.HeatMapChart;
        else
          this.SelectedVisualizationType = LayerType.PointMarkerChart;
      }
      else
      {
        if (this.HeightFields.Count != 1)
          return;
        switch (this.ChartType)
        {
          case VisualizationChartType.StackedColumn:
          case VisualizationChartType.ClusteredColumn:
            this.SelectedVisualizationType = LayerType.ColumnChart;
            break;
          case VisualizationChartType.Bubble:
            this.SelectedVisualizationType = LayerType.BubbleChart;
            break;
          case VisualizationChartType.HeatMap:
            this.SelectedVisualizationType = LayerType.HeatMapChart;
            break;
          case VisualizationChartType.Region:
            this.SelectedVisualizationType = LayerType.RegionChart;
            break;
        }
      }
    }

    private void Visualize()
    {
      if (this.VisualizeCommand == null)
        return;
      this.VisualizeCommand(false);
    }

    private void RefreshTimeSettings()
    {
      if (this.SelectedTimeField.Value == null)
        return;
      this.SetInstantTimeData();
      this.TimeSetting_TimeAccumulation.IsEnabled = true;
      if (this.HeightFields.Count > 0)
      {
        if (Queryable.Any<FieldWellHeightViewModel>(Queryable.AsQueryable<FieldWellHeightViewModel>((IEnumerable<FieldWellHeightViewModel>) this.HeightFields), (System.Linq.Expressions.Expression<Func<FieldWellHeightViewModel, bool>>) (heightField => (int) heightField.AggregationFunction == 0 || (int) heightField.AggregationFunction == 6 || (int) heightField.AggregationFunction == 7 || (int) heightField.AggregationFunction == 2)))
        {
          this.SetPersistTimeData(false);
          this.TimeSetting_TimeAccumulation.IsEnabled = false;
        }
        else
          this.SetAccumulateResultsOverTime(false);
      }
      else if (this.SelectedCategory.Value != null)
      {
        if (this.OkayToDisplayCategoryCounts())
        {
          this.SetAccumulateResultsOverTime(false);
        }
        else
        {
          this.SetPersistTimeData(false);
          this.TimeSetting_TimeAccumulation.IsEnabled = false;
        }
      }
      else
      {
        this.SetPersistTimeData(false);
        this.TimeSetting_TimeAccumulation.IsEnabled = false;
      }
    }

    private void OnHeightFieldAggregationFunctionChanged(FieldWellHeightViewModel item)
    {
      if (this.HeightFields.Contains(item))
        this.RefreshTimeSettings();
      this.InvokeDeferredVisualizationAction((Action) (() =>
      {
        if (item.AggregationFunction == AggregationFunction.None)
        {
          List<FieldWellHeightViewModel> list = new List<FieldWellHeightViewModel>();
          foreach (FieldWellHeightViewModel wellHeightViewModel in (Collection<FieldWellHeightViewModel>) this.HeightFields.Clone())
          {
            if (wellHeightViewModel.AllowsAggregationFunction(AggregationFunction.None))
              wellHeightViewModel.AggregationFunction = AggregationFunction.None;
            else
              list.Add(wellHeightViewModel);
          }
          foreach (FieldWellEntryViewModel wellEntryViewModel in list)
            wellEntryViewModel.RemoveEntry();
        }
        else
        {
          foreach (FieldWellHeightViewModel wellHeightViewModel in (Collection<FieldWellHeightViewModel>) this.HeightFields)
          {
            if (wellHeightViewModel.AggregationFunction == AggregationFunction.None)
              wellHeightViewModel.AggregationFunction = wellHeightViewModel.TableField.DefaultAggregationFunction();
          }
        }
      }));
    }

    private void OnSelectedTableFieldsChanged()
    {
      if (this.SelectedTimeField.Value != null && !this.SelectedTimeField.Value.TableField.IsSelected)
        this.SelectedTimeField.Value = (FieldWellTimeViewModel) null;
      if (this.SelectedCategory.Value != null && !this.SelectedCategory.Value.TableField.IsSelected)
      {
        this.SelectedCategory.Value = (FieldWellCategoryViewModel) null;
        this.RefreshTimeSettings();
      }
      this._removeHeightFieldsCache.Clear();
      foreach (FieldWellHeightViewModel wellHeightViewModel in (Collection<FieldWellHeightViewModel>) this.HeightFields)
      {
        if (!wellHeightViewModel.TableField.IsSelected)
          this._removeHeightFieldsCache.Add(wellHeightViewModel);
      }
      bool flag = false;
      foreach (FieldWellEntryViewModel wellEntryViewModel in this._removeHeightFieldsCache)
      {
        wellEntryViewModel.RemoveEntry();
        flag = true;
      }
      if (!flag)
        return;
      this.RefreshTimeSettings();
    }

    public void ProcessNewSelectedField(TableFieldViewModel field)
    {
      if (!field.IsSelected || this._droppedTableFields.Remove(field))
        return;
      bool flag = false;
      switch (field.ColumnDataType)
      {
        case TableMemberDataType.String:
        case TableMemberDataType.Bool:
          flag = this.ProcessSelectedTextOrBooleanField(field);
          break;
        case TableMemberDataType.Double:
        case TableMemberDataType.Long:
        case TableMemberDataType.Currency:
          flag = this.ProcessSelectedNumberOrCurrencyField(field);
          break;
        case TableMemberDataType.DateTime:
          flag = this.ProcessSelectedDateTimeField(field);
          break;
        default:
          field.IsSelected = false;
          break;
      }
      if (flag)
        return;
      string str = string.Empty;
      string messageBoxText;
      switch (this.ChartType)
      {
        case VisualizationChartType.StackedColumn:
        case VisualizationChartType.ClusteredColumn:
          messageBoxText = Resources.FieldWellVisualization_Error_ColumnChart_CategorytInUse_OneHeight;
          break;
        case VisualizationChartType.Bubble:
          messageBoxText = Resources.FieldWellVisualization_Error_BubbleChart_CategorytInUse_OneHeight;
          break;
        case VisualizationChartType.HeatMap:
          messageBoxText = Resources.FieldWellVisualization_Error_HeatMapChart_OneHeight;
          break;
        case VisualizationChartType.Region:
          messageBoxText = Resources.FieldWellVisualization_Error_RegionChart_CategorytInUse_OneHeight;
          break;
        default:
          return;
      }
      int num = (int) MessageBox.Show(messageBoxText, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK, Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
    }

    private bool ProcessSelectedNumberOrCurrencyField(TableFieldViewModel field)
    {
      if (this.HeightFields.Count > 0 && this.ChartType == VisualizationChartType.HeatMap)
      {
        field.IsSelected = false;
        return false;
      }
      else if (this.HeightFields.Count > 0 && this.SelectedCategory.Value != null)
      {
        field.IsSelected = false;
        return false;
      }
      else
      {
        this.HeightFields.Add(this.CreateFieldWellHeightViewModel(field));
        return true;
      }
    }

    private bool ProcessSelectedTextOrBooleanField(TableFieldViewModel field)
    {
      if (this.SelectedCategory.Value == null && this.ChartType != VisualizationChartType.HeatMap)
      {
        if (!this.TrySetCategoryFieldValue(field))
          this.HeightFields.Add(this.CreateFieldWellHeightViewModel(field));
      }
      else if (this.ChartType == VisualizationChartType.HeatMap)
      {
        if (this.HeightFields.Count > 0)
        {
          field.IsSelected = false;
          return false;
        }
        else
          this.HeightFields.Add(this.CreateFieldWellHeightViewModel(field));
      }
      else if (this.SelectedCategory.Value != null)
      {
        if (this.HeightFields.Count > 0)
        {
          field.IsSelected = false;
          return false;
        }
        else
          this.HeightFields.Add(this.CreateFieldWellHeightViewModel(field));
      }
      else
      {
        field.IsSelected = false;
        return false;
      }
      return true;
    }

    private bool ProcessSelectedDateTimeField(TableFieldViewModel field)
    {
      if (this.SelectedTimeField.Value == null)
        this.InvokeDeferredVisualizationAction((Action) (() =>
        {
          NPCContainer<FieldWellTimeViewModel> selectedTimeField = this.SelectedTimeField;
          FieldWellTimeViewModel wellTimeViewModel = new FieldWellTimeViewModel()
          {
            TableField = field,
            RemoveCallback = new Action<FieldWellTimeViewModel>(this.OnFieldWellTimeRemoved)
          };
          selectedTimeField.Value = wellTimeViewModel;
          this.RefreshTimeSettings();
        }));
      else if (this.ChartType == VisualizationChartType.HeatMap)
      {
        if (this.HeightFields.Count > 0)
        {
          field.IsSelected = false;
          return false;
        }
        else
          this.HeightFields.Add(this.CreateFieldWellHeightViewModel(field));
      }
      else if (this.CategoryEnabled && this.SelectedTimeField.Value != null && (this.SelectedCategory.Value == null && this.ChartType != VisualizationChartType.HeatMap))
        this.InvokeDeferredVisualizationAction((Action) (() =>
        {
          NPCContainer<FieldWellCategoryViewModel> selectedCategory = this.SelectedCategory;
          FieldWellCategoryViewModel categoryViewModel = new FieldWellCategoryViewModel()
          {
            TableField = field,
            RemoveCallback = new Action<FieldWellCategoryViewModel>(this.OnFieldWellCategoryRemoved)
          };
          selectedCategory.Value = categoryViewModel;
          this.RefreshTimeSettings();
          if (this.CategoryAdded == null)
            return;
          this.CategoryAdded();
        }));
      else if (this.SelectedTimeField.Value != null && this.SelectedCategory.Value != null)
      {
        if (this.HeightFields.Count > 0)
        {
          field.IsSelected = false;
          return false;
        }
        else
          this.HeightFields.Add(this.CreateFieldWellHeightViewModel(field));
      }
      else
      {
        field.IsSelected = false;
        return false;
      }
      return true;
    }

    private FieldWellHeightViewModel CreateFieldWellHeightViewModel(TableFieldViewModel field)
    {
      return this.CreateFieldWellHeightViewModel(field, field.DefaultAggregationFunction());
    }

    private FieldWellHeightViewModel CreateFieldWellHeightViewModel(TableFieldViewModel field, AggregationFunction aggn)
    {
      if (field == null)
        return (FieldWellHeightViewModel) null;
      FieldWellHeightViewModel wellHeightViewModel1 = new FieldWellHeightViewModel(!field.IsTableMeasure);
      wellHeightViewModel1.TableField = field;
      wellHeightViewModel1.AggregationFunction = aggn;
      wellHeightViewModel1.RemoveCallback = new Action<FieldWellHeightViewModel>(this.OnFieldWellHeightRemoved);
      FieldWellHeightViewModel wellHeightViewModel2 = wellHeightViewModel1;
      if (field.DefaultAggregationFunction() == AggregationFunction.Count)
        wellHeightViewModel2.SetAggregationFunctions((IEnumerable<AggregationFunction>) new AggregationFunction[2]
        {
          AggregationFunction.Count,
          AggregationFunction.DistinctCount
        });
      if (this.ChartType == VisualizationChartType.Region)
        wellHeightViewModel2.RemoveAggregationFunction(AggregationFunction.None);
      return wellHeightViewModel2;
    }

    private bool TrySetCategoryFieldValue(TableFieldViewModel field)
    {
      if (!this.CategoryEnabled)
        return false;
      this.InvokeDeferredVisualizationAction((Action) (() =>
      {
        NPCContainer<FieldWellCategoryViewModel> selectedCategory = this.SelectedCategory;
        FieldWellCategoryViewModel categoryViewModel = new FieldWellCategoryViewModel()
        {
          TableField = field,
          RemoveCallback = new Action<FieldWellCategoryViewModel>(this.OnFieldWellCategoryRemoved)
        };
        selectedCategory.Value = categoryViewModel;
        this.RefreshTimeSettings();
        if (this.CategoryAdded == null)
          return;
        this.CategoryAdded();
      }));
      return true;
    }

    private void InvokeDeferredVisualizationAction(Action action)
    {
      if (action != null)
      {
        this.DisallowVisualization();
        action();
        this.AllowVisualization();
      }
      if (!this.VisualizationEnabled)
        return;
      this.Visualize();
    }

    private void OnFieldWellHeightRemoved(FieldWellHeightViewModel heightField)
    {
      this.HeightFields.Remove(heightField);
      this.UnselectTableFieldIfNoOtherOccurrences(heightField.TableField);
    }

    public void OnFieldWellTimeRemoved(FieldWellTimeViewModel timeField)
    {
      if (this.SelectedTimeField.Value != null)
        this.InvokeDeferredVisualizationAction((Action) (() => this.SelectedTimeField.Value = (FieldWellTimeViewModel) null));
      this.UnselectTableFieldIfNoOtherOccurrences(timeField.TableField);
    }

    public void OnFieldWellCategoryRemoved(FieldWellCategoryViewModel categoryField)
    {
      if (this.SelectedCategory.Value != null)
        this.InvokeDeferredVisualizationAction((Action) (() =>
        {
          this.SelectedCategory.Value = (FieldWellCategoryViewModel) null;
          this.RefreshTimeSettings();
        }));
      this.UnselectTableFieldIfNoOtherOccurrences(categoryField.TableField);
    }

    private void UnselectTableFieldIfNoOtherOccurrences(TableFieldViewModel tableField)
    {
      if (tableField == null || this.GetNumberOfOccurrencesForTableField(tableField) != 0)
        return;
      tableField.IsSelected = false;
    }

    private int GetNumberOfOccurrencesForTableField(TableFieldViewModel tableField)
    {
      int num = 0;
      if (this.SelectedCategory.Value != null && this.SelectedCategory.Value.TableField == tableField)
        ++num;
      if (this.SelectedTimeField.Value != null && this.SelectedTimeField.Value.TableField == tableField)
        ++num;
      foreach (FieldWellEntryViewModel wellEntryViewModel in (Collection<FieldWellHeightViewModel>) this.HeightFields)
      {
        if (wellEntryViewModel.TableField == tableField)
          ++num;
      }
      return num;
    }

    public void OnGeoMappingChanged()
    {
      this.RefreshConditionalControls();
      this.RefreshTimeSettings();
    }

    private bool OkayToDisplayCategoryCounts()
    {
      if (this.SelectedCategory == null || this.SelectedCategory.Value == null || this.SelectedCategory.Value.TableField == null)
        return false;
      TableColumn tableColumn1 = this.SelectedCategory.Value.TableField.Model as TableColumn;
      if (tableColumn1 == null)
        return false;
      TableMetadata table1 = tableColumn1.Table;
      if (table1 == null)
        return false;
      GeoFieldMappingViewModel mappingViewModel = this.GetGeoFieldMapping();
      if (mappingViewModel == null)
        return false;
      TableColumn tableColumn2 = mappingViewModel.Field == null ? (TableColumn) null : mappingViewModel.Field.Model as TableColumn;
      if (tableColumn2 == null)
        return false;
      TableMetadata table2 = tableColumn2.Table;
      if (table2 == null || !table1.ContainsLookupTable(table2) && !table2.ContainsLookupTable(table1))
        return false;
      TableColumn tableColumn3 = this.SelectedTimeField == null || this.SelectedTimeField.Value == null || this.SelectedTimeField.Value.TableField == null ? (TableColumn) null : this.SelectedTimeField.Value.TableField.Model as TableColumn;
      TableMetadata table3 = tableColumn3 == null ? (TableMetadata) null : tableColumn3.Table;
      return table3 == null || table1.ContainsLookupTable(table3) || table3.ContainsLookupTable(table1);
    }
  }
}
