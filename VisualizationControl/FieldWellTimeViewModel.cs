using System;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class FieldWellTimeViewModel : FieldWellEntryViewModel
  {
    private TimeChunkPeriod _TimeChunk;

    public static string PropertyTimeChunk
    {
      get
      {
        return "TimeChunk";
      }
    }

    public TimeChunkPeriod TimeChunk
    {
      get
      {
        return this._TimeChunk;
      }
      set
      {
        this.SetProperty<TimeChunkPeriod>(FieldWellTimeViewModel.PropertyTimeChunk, ref this._TimeChunk, value, false);
      }
    }

    public Action<FieldWellTimeViewModel> RemoveCallback { get; set; }

    public FieldWellTimeViewModel()
    {
      this.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
      this.DropDownOptions.Add((object) TimeChunkPeriod.None);
      this.DropDownOptions.Add((object) TimeChunkPeriod.Day);
      this.DropDownOptions.Add((object) TimeChunkPeriod.Month);
      this.DropDownOptions.Add((object) TimeChunkPeriod.Quarter);
      this.DropDownOptions.Add((object) TimeChunkPeriod.Year);
      this.AddDropDownOptionControls(true);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == this.PropertyTableField) && !(e.PropertyName == FieldWellTimeViewModel.PropertyTimeChunk))
        return;
      this.DisplayString = string.Format(Resources.FieldWellTimeDisplayStringFormat, (object) this.TableField.Name, (object) TimeChunkPeriodExtensions.DisplayString(this.TimeChunk));
    }

    protected override void OnSelectedDropDownOptionValueChanged()
    {
      if (!(this.SelectedDropDownOption is TimeChunkPeriod))
        return;
      this.TimeChunk = (TimeChunkPeriod) this.SelectedDropDownOption;
    }

    protected override void OnSelectedRemoveEntryOption()
    {
      if (this.RemoveCallback == null)
        return;
      this.RemoveCallback(this);
    }
  }
}
