using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class FieldWellEntryViewModel : ViewModelBase
  {
    private RemoveItemPlaceholder removeHeightOption = new RemoveItemPlaceholder(Resources.FieldWellEntry_RemoveOption);
    private TableFieldViewModel _TableField;
    private string displayString;
    private object _SelectedDropDownOption;

    public string PropertyTableField
    {
      get
      {
        return "TableField";
      }
    }

    public TableFieldViewModel TableField
    {
      get
      {
        return this._TableField;
      }
      set
      {
        this.SetProperty<TableFieldViewModel>(this.PropertyTableField, ref this._TableField, value, false);
      }
    }

    public static string PropertyDisplayString
    {
      get
      {
        return "DisplayString";
      }
    }

    public string DisplayString
    {
      get
      {
        return this.displayString;
      }
      set
      {
        this.SetProperty<string>(FieldWellEntryViewModel.PropertyDisplayString, ref this.displayString, value, false);
      }
    }

    public string PropertySelectedDropDownOption
    {
      get
      {
        return "SelectedDropDownOption";
      }
    }

    public object SelectedDropDownOption
    {
      get
      {
        return this._SelectedDropDownOption;
      }
      set
      {
        base.SetProperty<object>(this.PropertySelectedDropDownOption, ref this._SelectedDropDownOption, value, new Action(this.OnSelectedDropDownOptionChanged));
      }
    }

    public ObservableCollectionEx<object> DropDownOptions { get; private set; }

    public FieldWellEntryViewModel()
    {
      this.DropDownOptions = new ObservableCollectionEx<object>();
    }

    public void RemoveEntry()
    {
      this.OnSelectedRemoveEntryOption();
    }

    protected void AddDropDownOptionControls(bool addSeparator = true)
    {
      if (addSeparator)
        this.AddDropDownSeparator();
      this.AddDropDownOptionRemove();
    }

    protected void AddDropDownSeparator()
    {
      this.DropDownOptions.Add(ListUtilities.Separator);
    }

    protected void AddDropDownOptionRemove()
    {
      this.DropDownOptions.Add((object) this.removeHeightOption);
    }

    protected abstract void OnSelectedRemoveEntryOption();

    protected abstract void OnSelectedDropDownOptionValueChanged();

    private void OnSelectedDropDownOptionChanged()
    {
      if (this.SelectedDropDownOption == this.removeHeightOption)
        this.OnSelectedRemoveEntryOption();
      else
        this.OnSelectedDropDownOptionValueChanged();
    }
  }
}
