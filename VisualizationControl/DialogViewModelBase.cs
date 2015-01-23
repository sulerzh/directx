using Microsoft.Data.Visualization.WpfExtensions;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DialogViewModelBase : ViewModelBase, IDialog
  {
    private string _Title;
    private string _Description;
    private ICommand _CancelCommand;

    public string PropertyTitle
    {
      get
      {
        return "Title";
      }
    }

    public string Title
    {
      get
      {
        return this._Title;
      }
      set
      {
        this.SetProperty<string>(this.PropertyTitle, ref this._Title, value, false);
      }
    }

    public string PropertyDescription
    {
      get
      {
        return "Description";
      }
    }

    public string Description
    {
      get
      {
        return this._Description;
      }
      set
      {
        this.SetProperty<string>(this.PropertyDescription, ref this._Description, value, false);
      }
    }

    public string PropertyCancelCommand
    {
      get
      {
        return "CancelCommand";
      }
    }

    public ICommand CancelCommand
    {
      get
      {
        return this._CancelCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyCancelCommand, ref this._CancelCommand, value, false);
      }
    }
  }
}
