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
                return _Name;
            }
            set
            {
                SetProperty(PropertyName, ref _Name, value, false);
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
                return _IsSelected;
            }
            set
            {
                SetProperty(PropertyIsSelected, ref _IsSelected, value, false);
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
                return _IsTimeField;
            }
            set
            {
                SetProperty(PropertyIsTimeField, ref _IsTimeField, value, false);
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
                return _IsCategory;
            }
            set
            {
                SetProperty(PropertyIsCategory, ref _IsCategory, value, false);
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
                return _ColumnDataType;
            }
            set
            {
                SetProperty(PropertyDataType, ref _ColumnDataType, value, false);
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
                return _ColumnClassification;
            }
            set
            {
                SetProperty(PropertyClassification, ref _ColumnClassification, value, false);
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
                return _IsEnabled;
            }
            set
            {
                SetProperty(PropertyIsEnabled, ref _IsEnabled, value, false);
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
                return _DisplayContextMenu;
            }
            set
            {
                SetProperty(PropertyDisplayContextMenu, ref _DisplayContextMenu, value, false);
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
                return _HeightLabel;
            }
            set
            {
                if (!SetProperty(PropertyHeightLabel, ref _HeightLabel, value, false) || heightCommand == null)
                    return;
                heightCommand.Header = string.Format(Resources.FieldListPicker_TableField_AddHeight, (object)HeightLabel);
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
            Model = model;
            Name = model.Name;
            IsEnabled = !disabled;
            DisplayContextMenu = addHeight != null || addCategory != null || addTime != null;
            this.addHeight = addHeight;
            this.addCategory = addCategory;
            this.addTime = addTime;
            HeightLabel = Resources.FieldWellVisualization_ValueLabel_Height;
            TableColumn tableColumn = Model as TableColumn;
            TableMeasure tableMeasure = Model as TableMeasure;
            if (tableColumn != null)
            {
                ColumnDataType = tableColumn.DataType;
                IsTimeField = tableColumn != null && tableColumn.DataType == TableMemberDataType.DateTime;
                IsCategory = true;
                IsTableMeasure = false;
                ColumnClassification = TableMemberClassificationUtil.GeoFieldType(tableColumn.Classification);
            }
            else if (tableMeasure != null)
            {
                ColumnDataType = tableMeasure.DataType;
                IsTimeField = false;
                IsCategory = false;
                IsTableMeasure = true;
                ColumnClassification = GeoFieldMappingType.None;
            }
            if (!DisplayContextMenu)
                return;
            ContextCommands = new ObservableCollectionEx<ContextCommand>();
            if (this.addHeight != null)
            {
                heightCommand = new ContextCommand(string.Format(Resources.FieldListPicker_TableField_AddHeight, (object)HeightLabel), (ICommand)new DelegatedCommand(new Action(AddHeight)));
                ContextCommands.Add(heightCommand);
            }
            if (this.addCategory != null)
                ContextCommands.Add(new ContextCommand(Resources.FieldListPicker_TableField_AddCategory, (ICommand)new DelegatedCommand(new Action(AddCategory), (Predicate)(() =>
                {
                    if (tableFieldViewModel.IsCategory)
                        return chartAllowsCategories();
                    else
                        return false;
                }))));
            if (this.addTime == null)
                return;
            ContextCommands.Add(new ContextCommand(Resources.FieldListPicker_TableField_AddTime, (ICommand)new DelegatedCommand(new Action(AddTime), (Predicate)(() => IsTimeField))));
        }

        private void AddHeight()
        {
            addHeight(this);
        }

        private void AddCategory()
        {
            addCategory(this);
        }

        private void AddTime()
        {
            addTime(this);
        }

        public AggregationFunction DefaultAggregationFunction()
        {
            if (IsTableMeasure)
                return AggregationFunction.UserDefined;
            return ColumnDataType == TableMemberDataType.Double || ColumnDataType == TableMemberDataType.Long || ColumnDataType == TableMemberDataType.Currency ? AggregationFunction.Sum : AggregationFunction.Count;
        }
    }
}
