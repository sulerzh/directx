using System;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class FieldWellCategoryViewModel : FieldWellEntryViewModel
  {
    public Action<FieldWellCategoryViewModel> RemoveCallback { get; set; }

    public FieldWellCategoryViewModel()
    {
      this.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
      this.AddDropDownOptionControls(false);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == this.PropertyTableField))
        return;
      this.DisplayString = this.TableField.Name;
    }

    protected override void OnSelectedDropDownOptionValueChanged()
    {
    }

    protected override void OnSelectedRemoveEntryOption()
    {
      if (this.RemoveCallback == null)
        return;
      this.RemoveCallback(this);
    }
  }
}
