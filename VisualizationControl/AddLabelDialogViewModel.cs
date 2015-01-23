using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class AddLabelDialogViewModel : DialogViewModelBase
  {
    private LabelDecoratorModel _Label;
    private RichTextModel _ActiveTextFormat;
    private ICommand _CreateCommand;

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
        if (!this.SetProperty<LabelDecoratorModel>(AddLabelDialogViewModel.PropertyLabel, ref this._Label, value, false) || this.Label == null)
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
        this.SetProperty<RichTextModel>(AddLabelDialogViewModel.PropertyActiveTextFormat, ref this._ActiveTextFormat, value, false);
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
        this.SetProperty<ICommand>(AddLabelDialogViewModel.PropertyCreateCommand, ref this._CreateCommand, value, false);
      }
    }

    public AddLabelDialogViewModel(LabelDecoratorModel label)
      : this()
    {
      this.Label = label;
      if (this.Label != null)
        return;
      this.Label = new LabelDecoratorModel();
    }

    public AddLabelDialogViewModel()
    {
    }

    public bool CanExecuteCreateCommand()
    {
      return this.Label != null && (this.Label.Title != null && !string.IsNullOrEmpty(this.Label.Title.Text) || this.Label.Description != null && !string.IsNullOrEmpty(this.Label.Description.Text));
    }
  }
}
