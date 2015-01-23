using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class EditTimeDecoratorDialogViewModel : DialogViewModelBase
  {
    public static DateTime SampleTime = new DateTime(2010, 6, 18, 8, 30, 21, 8);
    private List<TimeStringFormat> _Formats = new List<TimeStringFormat>();
    private TimeDecoratorModel _model;
    private ICommand _AcceptCommand;
    private TimeStringFormat _SelectedFormat;

    public static string PropertyModel
    {
      get
      {
        return "Model";
      }
    }

    public TimeDecoratorModel Model
    {
      get
      {
        return this._model;
      }
      set
      {
        this.SetProperty<TimeDecoratorModel>(EditTimeDecoratorDialogViewModel.PropertyModel, ref this._model, value, false);
      }
    }

    public static string PropertyCreateCommand
    {
      get
      {
        return "AcceptCommand";
      }
    }

    public ICommand AcceptCommand
    {
      get
      {
        return this._AcceptCommand;
      }
      set
      {
        this.SetProperty<ICommand>(EditTimeDecoratorDialogViewModel.PropertyCreateCommand, ref this._AcceptCommand, value, false);
      }
    }

    public string PropertyFormats
    {
      get
      {
        return "Formats";
      }
    }

    public List<TimeStringFormat> Formats
    {
      get
      {
        return this._Formats;
      }
    }

    public string PropertySelectedFormat
    {
      get
      {
        return "SelectedFormat";
      }
    }

    public TimeStringFormat SelectedFormat
    {
      get
      {
        return this._SelectedFormat;
      }
      set
      {
        base.SetProperty<TimeStringFormat>(this.PropertySelectedFormat, ref this._SelectedFormat, value, new Action(this.Refresh));
      }
    }

    public EditTimeDecoratorDialogViewModel(TimeDecoratorModel model)
      : this()
    {
      this.Model = model;
      if (this.Model != null)
        return;
      this.Model = new TimeDecoratorModel();
    }

    public EditTimeDecoratorDialogViewModel()
    {
      this.SelectedFormat = new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "g");
      this.Formats.Add(this.SelectedFormat);
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "d"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "D"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "f"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "F"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "G"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "M"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "O"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "R"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "s"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "t"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "T"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "u"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "U"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "Y"));
      this.Formats.Add(new TimeStringFormat(EditTimeDecoratorDialogViewModel.SampleTime, "yyyy"));
    }

    private void Refresh()
    {
      if (this.Model == null)
        return;
      this.Model.Format = this.SelectedFormat.Format;
    }
  }
}
