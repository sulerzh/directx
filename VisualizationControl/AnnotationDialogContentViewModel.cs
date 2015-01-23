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
        return this._InstructionText;
      }
      set
      {
        this.SetProperty<string>(AnnotationDialogContentViewModel.PropertyInstructionText, ref this._InstructionText, value, false);
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
        return this._Model;
      }
      set
      {
        if (this._Model != null && value != this._Model)
        {
          this._Model.PropertyChanged -= new PropertyChangedEventHandler(this.OnModelPropertyChanged);
          this._Model.DescendentPropertyChanged -= new PropertyChangedEventHandler(this.OnModelDescendentPropertyChanged);
        }
        if (!this.SetProperty<AnnotationTemplateModel>(AnnotationDialogContentViewModel.PropertyModel, ref this._Model, value, false) || this.Model == null)
          return;
        this._Model.PropertyChanged += new PropertyChangedEventHandler(this.OnModelPropertyChanged);
        this._Model.DescendentPropertyChanged += new PropertyChangedEventHandler(this.OnModelDescendentPropertyChanged);
        this.RefreshSelectedDataColumns();
        this.RefreshTemplateValues();
        this.ActiveTextFormat = this._Model.Title;
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
        return this._SelectedData;
      }
      set
      {
        if (!this.SetProperty<DataRowModel>(AnnotationDialogContentViewModel.PropertySelectedData, ref this._SelectedData, value, false))
          return;
        this.RefreshSelectedDataColumns();
        this.RefreshTemplateValues();
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
        return this._ActiveTextFormat;
      }
      set
      {
        this.SetProperty<RichTextModel>(AnnotationDialogContentViewModel.PropertyActiveTextFormat, ref this._ActiveTextFormat, value, false);
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
        return this._SelectedColumnForTitle;
      }
      set
      {
        base.SetProperty<FieldColumnSelectionViewModel>(this.PropertySelectedColumnForTitle, ref this._SelectedColumnForTitle, value, new Action(this.OnSelectedColumnForTitleChanged));
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
        return this._CreateCommand;
      }
      set
      {
        this.SetProperty<ICommand>(AnnotationDialogContentViewModel.PropertyCreateCommand, ref this._CreateCommand, value, false);
      }
    }

    public AnnotationDialogContentViewModel(AnnotationTemplateModel model = null)
    {
      this.SelectedElements = new List<SelectedElementHelper>();
      this.ColumnsForSelectedData = new ObservableCollectionEx<FieldColumnSelectionViewModel>();
      this.ColumnsForSelectedData.ItemPropertyChanged += new ObservableCollectionExItemChangedHandler<FieldColumnSelectionViewModel>(this.ColumnsForSelectedDataItemPropertyChanged);
      this.Model = model;
      if (this.Model != null)
        return;
      this.Model = new AnnotationTemplateModel();
    }

    public bool CanExecuteCreateCommand()
    {
      bool flag1 = !string.IsNullOrEmpty(this.Model.Title.Text);
      bool flag2 = false;
      bool flag3 = false;
      if (this.Model.DescriptionType == AnnotationDescriptionType.Custom)
        flag2 = !string.IsNullOrEmpty(this.Model.Description.Text);
      else if (this.Model.DescriptionType == AnnotationDescriptionType.Bound)
        flag2 = this.Model.FormattedFieldDisplayStrings.Count > 0;
      else if (this.Model.DescriptionType == AnnotationDescriptionType.Image)
        flag3 = this.Model.Image != null;
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
        int indexOfNameColumn = this.GetIndexOfNameColumn(item);
        if (indexOfNameColumn == -1)
          return;
        this.Model.ColumnAggregationFunctions.RemoveAt(indexOfNameColumn);
        this.Model.NamesOfColumnsToDisplay.RemoveAt(indexOfNameColumn);
        this.RefreshTemplateValues();
      }
      else
      {
        if (!item.IsSelected || this.GetIndexOfNameColumn(item) != -1)
          return;
        this.Model.ColumnAggregationFunctions.Add(item.Model.Item1);
        this.Model.NamesOfColumnsToDisplay.Add(nameToPersist);
        this.RefreshTemplateValues();
      }
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == AnnotationTemplateModel.PropertyTitleField || e.PropertyName == AnnotationTemplateModel.PropertyTitleAF)
        this.Model.Title.FormatType = RichTextFormatType.Template;
      this.RefreshTemplateValues();
    }

    private void OnModelDescendentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!e.PropertyName.Contains(RichTextModel.PropertyFormatType))
        return;
      this.RefreshTemplateValues();
    }

    private int GetIndexOfNameColumn(FieldColumnSelectionViewModel item)
    {
      int num = Math.Max(this.Model.NamesOfColumnsToDisplay.Count, this.Model.ColumnAggregationFunctions.Count);
      for (int index = 0; index < num; ++index)
      {
        if (string.Compare(item.NameToPersist, this.Model.NamesOfColumnsToDisplay[index], StringComparison.OrdinalIgnoreCase) == 0)
        {
          AggregationFunction? nullable1 = item.Model.Item1;
          AggregationFunction? nullable2 = this.Model.ColumnAggregationFunctions[index];
          if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 0 : (nullable1.HasValue == nullable2.HasValue ? 1 : 0)) != 0)
            return index;
        }
      }
      return -1;
    }

    private void RefreshSelectedDataColumns()
    {
      this.ColumnsForSelectedData.Clear();
      if (this.SelectedData == null || this.Model == null)
        return;
      FieldColumnSelectionViewModel selectionViewModel1 = (FieldColumnSelectionViewModel) null;
      foreach (Tuple<AggregationFunction?, string, object> column in this.SelectedData.Fields)
      {
        bool flag1 = false;
        foreach (FieldColumnSelectionViewModel selectionViewModel2 in (Collection<FieldColumnSelectionViewModel>) this.ColumnsForSelectedData)
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
            DisplayName = !this.SelectedData.AnyMeasure || !this.SelectedData.IsMeasure(column) ? this.SelectedData.GetColumnName(column.Item2, column.Item1) : Resources.AnnotationDialog_DisplayedValue,
            NameToPersist = !this.SelectedData.AnyMeasure || !this.SelectedData.IsMeasure(column) ? column.Item2 : ""
          };
          bool flag2 = this.GetIndexOfNameColumn(selectionViewModel2) != -1;
          selectionViewModel2.IsSelected = flag2;
          this.ColumnsForSelectedData.Add(selectionViewModel2);
          if (selectionViewModel1 == null)
            selectionViewModel1 = selectionViewModel2;
          if (string.Compare(column.Item2, this.Model.TitleField, StringComparison.OrdinalIgnoreCase) == 0)
          {
            AggregationFunction? nullable = column.Item1;
            AggregationFunction? titleAf = this.Model.TitleAF;
            if ((nullable.GetValueOrDefault() != titleAf.GetValueOrDefault() ? 0 : (nullable.HasValue == titleAf.HasValue ? 1 : 0)) != 0)
              this.SelectedColumnForTitle = selectionViewModel2;
          }
        }
      }
      if (this.SelectedColumnForTitle != null || selectionViewModel1 == null)
        return;
      string text = this.Model.Title.Text;
      RichTextFormatType formatType = this.Model.Title.FormatType;
      this.SelectedColumnForTitle = selectionViewModel1;
      this.Model.Title.FormatType = formatType;
      this.Model.Title.Text = text;
    }

    private void RefreshTemplateValues()
    {
      if (this.Model == null)
        return;
      this.Model.Apply(this.SelectedData);
    }

    private void OnSelectedColumnForTitleChanged()
    {
      if (this.SelectedColumnForTitle == null)
        return;
      this.Model.TitleField = this.SelectedColumnForTitle.NameToPersist;
      this.Model.TitleAF = this.SelectedColumnForTitle.Model.Item1;
      this.RefreshTemplateValues();
    }
  }
}
