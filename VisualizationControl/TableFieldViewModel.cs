using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TableFieldViewModel : ViewModelBase
  {
    private bool _IsEnabled = true;
    private bool _DisplayContextMenu = true;
    private string _Name;
    private bool _IsSelected;
    private bool _IsTimeField;
    private bool _IsCategory;
    private TableMemberDataType _ColumnDataType;
    private GeoFieldMappingType _ColumnClassification;
    private string _HeightLabel;
    private ContextCommand heightCommand;
    private Action<TableFieldViewModel> addHeight;
    private Action<TableFieldViewModel> addCategory;
    private Action<TableFieldViewModel> addTime;

    public static string PropertyName
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
        this.SetProperty<string>(TableFieldViewModel.PropertyName, ref this._Name, value, false);
      }
    }

    public static string PropertyIsSelected
    {
      get
      {
        return "IsSelected";
      }
    }

    public bool IsSelected
    {
      get
      {
        return this._IsSelected;
      }
      set
      {
        this.SetProperty<bool>(TableFieldViewModel.PropertyIsSelected, ref this._IsSelected, value, false);
      }
    }

    public string PropertyIsTimeField
    {
      get
      {
        return "IsTimeField";
      }
    }

    public bool IsTimeField
    {
      get
      {
        return this._IsTimeField;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyIsTimeField, ref this._IsTimeField, value, false);
      }
    }

    public string PropertyIsCategory
    {
      get
      {
        return "IsCategory";
      }
    }

    public bool IsCategory
    {
      get
      {
        return this._IsCategory;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyIsCategory, ref this._IsCategory, value, false);
      }
    }

    public string PropertyDataType
    {
      get
      {
        return "ColumnDataType";
      }
    }

    public TableMemberDataType ColumnDataType
    {
      get
      {
        return this._ColumnDataType;
      }
      set
      {
        this.SetProperty<TableMemberDataType>(this.PropertyDataType, ref this._ColumnDataType, value, false);
      }
    }

    public string PropertyClassification
    {
      get
      {
        return "ColumnClassification";
      }
    }

    public GeoFieldMappingType ColumnClassification
    {
      get
      {
        return this._ColumnClassification;
      }
      set
      {
        this.SetProperty<GeoFieldMappingType>(this.PropertyClassification, ref this._ColumnClassification, value, false);
      }
    }

    public string PropertyIsEnabled
    {
      get
      {
        return "IsEnabled";
      }
    }

    public bool IsEnabled
    {
      get
      {
        return this._IsEnabled;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyIsEnabled, ref this._IsEnabled, value, false);
      }
    }

    public string PropertyDisplayContextMenu
    {
      get
      {
        return "DisplayContextMenu";
      }
    }

    public bool DisplayContextMenu
    {
      get
      {
        return this._DisplayContextMenu;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyDisplayContextMenu, ref this._DisplayContextMenu, value, false);
      }
    }

    public string PropertyHeightLabel
    {
      get
      {
        return "HeightLabel";
      }
    }

    public string HeightLabel
    {
      get
      {
        return this._HeightLabel;
      }
      set
      {
        if (!this.SetProperty<string>(this.PropertyHeightLabel, ref this._HeightLabel, value, false) || this.heightCommand == null)
          return;
        this.heightCommand.Header = string.Format(Resources.FieldListPicker_TableField_AddHeight, (object) this.HeightLabel);
      }
    }

    public TableField Model { get; private set; }

    public bool IsTableMeasure { get; private set; }

    public ObservableCollectionEx<ContextCommand> ContextCommands { get; private set; }

    public TableFieldViewModel()
    {
    }

    public TableFieldViewModel(TableField model, bool disabled, Action<TableFieldViewModel> addHeight = null, Action<TableFieldViewModel> addCategory = null, Action<TableFieldViewModel> addTime = null, Func<bool> chartAllowsCategories = null)
      : this()
    {
      TableFieldViewModel tableFieldViewModel = this;
      this.Model = model;
      this.Name = model.Name;
      this.IsEnabled = !disabled;
      this.DisplayContextMenu = addHeight != null || addCategory != null || addTime != null;
      this.addHeight = addHeight;
      this.addCategory = addCategory;
      this.addTime = addTime;
      this.HeightLabel = Resources.FieldWellVisualization_ValueLabel_Height;
      TableColumn tableColumn = this.Model as TableColumn;
      TableMeasure tableMeasure = this.Model as TableMeasure;
      if (tableColumn != null)
      {
        this.ColumnDataType = tableColumn.DataType;
        this.IsTimeField = tableColumn != null && tableColumn.DataType == TableMemberDataType.DateTime;
        this.IsCategory = true;
        this.IsTableMeasure = false;
        this.ColumnClassification = TableMemberClassificationUtil.GeoFieldType(tableColumn.Classification);
      }
      else if (tableMeasure != null)
      {
        this.ColumnDataType = tableMeasure.DataType;
        this.IsTimeField = false;
        this.IsCategory = false;
        this.IsTableMeasure = true;
        this.ColumnClassification = GeoFieldMappingType.None;
      }
      if (!this.DisplayContextMenu)
        return;
      this.ContextCommands = new ObservableCollectionEx<ContextCommand>();
      if (this.addHeight != null)
      {
        this.heightCommand = new ContextCommand(string.Format(Resources.FieldListPicker_TableField_AddHeight, (object) this.HeightLabel), (ICommand) new DelegatedCommand(new Action(this.AddHeight)));
        this.ContextCommands.Add(this.heightCommand);
      }
      if (this.addCategory != null)
        this.ContextCommands.Add(new ContextCommand(Resources.FieldListPicker_TableField_AddCategory, (ICommand) new DelegatedCommand(new Action(this.AddCategory), (Predicate) (() =>
        {
          if (tableFieldViewModel.IsCategory)
            return chartAllowsCategories();
          else
            return false;
        }))));
      if (this.addTime == null)
        return;
      this.ContextCommands.Add(new ContextCommand(Resources.FieldListPicker_TableField_AddTime, (ICommand) new DelegatedCommand(new Action(this.AddTime), (Predicate) (() => this.IsTimeField))));
    }

    private void AddHeight()
    {
      this.addHeight(this);
    }

    private void AddCategory()
    {
      this.addCategory(this);
    }

    private void AddTime()
    {
      this.addTime(this);
    }

    public AggregationFunction DefaultAggregationFunction()
    {
      if (this.IsTableMeasure)
        return AggregationFunction.UserDefined;
      return this.ColumnDataType == TableMemberDataType.Double || this.ColumnDataType == TableMemberDataType.Long || this.ColumnDataType == TableMemberDataType.Currency ? AggregationFunction.Sum : AggregationFunction.Count;
    }
  }
}
