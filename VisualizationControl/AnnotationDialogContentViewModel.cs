using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class AnnotationDialogContentViewModel : DialogViewModelBase
    {
        private const int NotFoundIndex = -1;
        private string _InstructionText;
        private AnnotationTemplateModel _Model;
        private DataRowModel _SelectedData;
        private RichTextModel _ActiveTextFormat;
        private FieldColumnSelectionViewModel _SelectedColumnForTitle;
        private ICommand _CreateCommand;

        public SelectedElementHelper PrimarySelectedElement { get; set; }

        public List<SelectedElementHelper> SelectedElements { get; private set; }

        public static string PropertyInstructionText
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
                return _InstructionText;
            }
            set
            {
                SetProperty(PropertyInstructionText, ref _InstructionText, value, false);
            }
        }

        public static string PropertyModel
        {
            get
            {
                return "Model";
            }
        }

        public AnnotationTemplateModel Model
        {
            get
            {
                return _Model;
            }
            set
            {
                if (_Model != null && value != _Model)
                {
                    _Model.PropertyChanged -= OnModelPropertyChanged;
                    _Model.DescendentPropertyChanged -= OnModelDescendentPropertyChanged;
                }
                if (!SetProperty(PropertyModel, ref _Model, value, false) || Model == null)
                    return;
                _Model.PropertyChanged += OnModelPropertyChanged;
                _Model.DescendentPropertyChanged += OnModelDescendentPropertyChanged;
                RefreshSelectedDataColumns();
                RefreshTemplateValues();
                ActiveTextFormat = _Model.Title;
            }
        }

        public static string PropertySelectedData
        {
            get
            {
                return "SelectedData";
            }
        }

        public DataRowModel SelectedData
        {
            get
            {
                return _SelectedData;
            }
            set
            {
                if (!SetProperty(PropertySelectedData, ref _SelectedData, value, false))
                    return;
                RefreshSelectedDataColumns();
                RefreshTemplateValues();
            }
        }

        public static string PropertyActiveTextFormat
        {
            get
            {
                return "ActiveTextFormat";
            }
        }

        public RichTextModel ActiveTextFormat
        {
            get
            {
                return _ActiveTextFormat;
            }
            set
            {
                SetProperty(PropertyActiveTextFormat, ref _ActiveTextFormat, value, false);
            }
        }

        public ObservableCollectionEx<FieldColumnSelectionViewModel> ColumnsForSelectedData { get; private set; }

        public string PropertySelectedColumnForTitle
        {
            get
            {
                return "SelectedColumnForTitle";
            }
        }

        public FieldColumnSelectionViewModel SelectedColumnForTitle
        {
            get
            {
                return _SelectedColumnForTitle;
            }
            set
            {
                base.SetProperty(PropertySelectedColumnForTitle, ref _SelectedColumnForTitle, value, OnSelectedColumnForTitleChanged);
            }
        }

        public static string PropertyCreateCommand
        {
            get
            {
                return "CreateCommand";
            }
        }

        public ICommand CreateCommand
        {
            get
            {
                return _CreateCommand;
            }
            set
            {
                SetProperty(PropertyCreateCommand, ref _CreateCommand, value, false);
            }
        }

        public AnnotationDialogContentViewModel(AnnotationTemplateModel model = null)
        {
            SelectedElements = new List<SelectedElementHelper>();
            ColumnsForSelectedData = new ObservableCollectionEx<FieldColumnSelectionViewModel>();
            ColumnsForSelectedData.ItemPropertyChanged += ColumnsForSelectedDataItemPropertyChanged;
            Model = model;
            if (Model != null)
                return;
            Model = new AnnotationTemplateModel();
        }

        public bool CanExecuteCreateCommand()
        {
            bool flag1 = !string.IsNullOrEmpty(Model.Title.Text);
            bool flag2 = false;
            bool flag3 = false;
            if (Model.DescriptionType == AnnotationDescriptionType.Custom)
                flag2 = !string.IsNullOrEmpty(Model.Description.Text);
            else if (Model.DescriptionType == AnnotationDescriptionType.Bound)
                flag2 = Model.FormattedFieldDisplayStrings.Count > 0;
            else if (Model.DescriptionType == AnnotationDescriptionType.Image)
                flag3 = Model.Image != null;
            if (!flag1 && !flag2)
                return flag3;
            else
                return true;
        }

        private void ColumnsForSelectedDataItemPropertyChanged(FieldColumnSelectionViewModel item, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == FieldColumnSelectionViewModel.PropertyIsSelected))
                return;
            string nameToPersist = item.NameToPersist;
            if (!item.IsSelected)
            {
                int indexOfNameColumn = GetIndexOfNameColumn(item);
                if (indexOfNameColumn == -1)
                    return;
                Model.ColumnAggregationFunctions.RemoveAt(indexOfNameColumn);
                Model.NamesOfColumnsToDisplay.RemoveAt(indexOfNameColumn);
                RefreshTemplateValues();
            }
            else
            {
                if (!item.IsSelected || GetIndexOfNameColumn(item) != -1)
                    return;
                Model.ColumnAggregationFunctions.Add(item.Model.Item1);
                Model.NamesOfColumnsToDisplay.Add(nameToPersist);
                RefreshTemplateValues();
            }
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == AnnotationTemplateModel.PropertyTitleField || e.PropertyName == AnnotationTemplateModel.PropertyTitleAF)
                Model.Title.FormatType = RichTextFormatType.Template;
            RefreshTemplateValues();
        }

        private void OnModelDescendentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Contains(RichTextModel.PropertyFormatType))
                return;
            RefreshTemplateValues();
        }

        private int GetIndexOfNameColumn(FieldColumnSelectionViewModel item)
        {
            int num = Math.Max(Model.NamesOfColumnsToDisplay.Count, Model.ColumnAggregationFunctions.Count);
            for (int index = 0; index < num; ++index)
            {
                if (string.Compare(item.NameToPersist, Model.NamesOfColumnsToDisplay[index], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    AggregationFunction? nullable1 = item.Model.Item1;
                    AggregationFunction? nullable2 = Model.ColumnAggregationFunctions[index];
                    if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 0 : (nullable1.HasValue == nullable2.HasValue ? 1 : 0)) != 0)
                        return index;
                }
            }
            return -1;
        }

        private void RefreshSelectedDataColumns()
        {
            ColumnsForSelectedData.Clear();
            if (SelectedData == null || Model == null)
                return;
            FieldColumnSelectionViewModel selectionViewModel1 = (FieldColumnSelectionViewModel)null;
            foreach (Tuple<AggregationFunction?, string, object> column in SelectedData.Fields)
            {
                bool flag1 = false;
                foreach (FieldColumnSelectionViewModel selectionViewModel2 in (Collection<FieldColumnSelectionViewModel>)ColumnsForSelectedData)
                {
                    AggregationFunction? nullable1 = column.Item1;
                    AggregationFunction? nullable2 = selectionViewModel2.Model.Item1;
                    if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 0 : (nullable1.HasValue == nullable2.HasValue ? 1 : 0)) != 0 && string.Compare(column.Item2, selectionViewModel2.NameToPersist, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        flag1 = true;
                        break;
                    }
                }
                if (!flag1)
                {
                    FieldColumnSelectionViewModel selectionViewModel2 = new FieldColumnSelectionViewModel()
                    {
                        Model = column,
                        DisplayName = !SelectedData.AnyMeasure || !SelectedData.IsMeasure(column) ? SelectedData.GetColumnName(column.Item2, column.Item1) : Resources.AnnotationDialog_DisplayedValue,
                        NameToPersist = !SelectedData.AnyMeasure || !SelectedData.IsMeasure(column) ? column.Item2 : ""
                    };
                    bool flag2 = GetIndexOfNameColumn(selectionViewModel2) != -1;
                    selectionViewModel2.IsSelected = flag2;
                    ColumnsForSelectedData.Add(selectionViewModel2);
                    if (selectionViewModel1 == null)
                        selectionViewModel1 = selectionViewModel2;
                    if (string.Compare(column.Item2, Model.TitleField, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AggregationFunction? nullable = column.Item1;
                        AggregationFunction? titleAf = Model.TitleAF;
                        if ((nullable.GetValueOrDefault() != titleAf.GetValueOrDefault() ? 0 : (nullable.HasValue == titleAf.HasValue ? 1 : 0)) != 0)
                            SelectedColumnForTitle = selectionViewModel2;
                    }
                }
            }
            if (SelectedColumnForTitle != null || selectionViewModel1 == null)
                return;
            string text = Model.Title.Text;
            RichTextFormatType formatType = Model.Title.FormatType;
            SelectedColumnForTitle = selectionViewModel1;
            Model.Title.FormatType = formatType;
            Model.Title.Text = text;
        }

        private void RefreshTemplateValues()
        {
            if (Model == null)
                return;
            Model.Apply(SelectedData);
        }

        private void OnSelectedColumnForTitleChanged()
        {
            if (SelectedColumnForTitle == null)
                return;
            Model.TitleField = SelectedColumnForTitle.NameToPersist;
            Model.TitleAF = SelectedColumnForTitle.Model.Item1;
            RefreshTemplateValues();
        }
    }
}
