using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class EditLabelDialogViewModel : DialogViewModelBase
  {
    private LabelDecoratorModel _Label;
    private RichTextModel _ActiveTextFormat;
    private ICommand _AcceptCommand;

    public static string PropertyLabel
    {
      get
      {
        return "Label";
      }
    }

    public LabelDecoratorModel Label
    {
      get
      {
        return this._Label;
      }
      set
      {
        if (!this.SetProperty<LabelDecoratorModel>(EditLabelDialogViewModel.PropertyLabel, ref this._Label, value, false) || this.Label == null)
          return;
        this.ActiveTextFormat = this._Label.Title;
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
        this.SetProperty<RichTextModel>(EditLabelDialogViewModel.PropertyActiveTextFormat, ref this._ActiveTextFormat, value, false);
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
        this.SetProperty<ICommand>(EditLabelDialogViewModel.PropertyCreateCommand, ref this._AcceptCommand, value, false);
      }
    }

    public EditLabelDialogViewModel(LabelDecoratorModel label)
      : this()
    {
      this.Label = label;
      if (this.Label != null)
        return;
      this.Label = new LabelDecoratorModel();
    }

    public EditLabelDialogViewModel()
    {
    }
  }
}
