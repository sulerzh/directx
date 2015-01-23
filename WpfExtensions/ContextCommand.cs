using Microsoft.Data.Visualization.VisualizationCommon;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class ContextCommand : PropertyChangeNotificationBase
  {
    private string _Header;
    private ICommand _Command;

    public string PropertyHeader
    {
      get
      {
        return "Header";
      }
    }

    public string Header
    {
      get
      {
        return this._Header;
      }
      set
      {
        base.SetProperty<string>(this.PropertyHeader, ref this._Header, value);
      }
    }

    public string PropertyCommand
    {
      get
      {
        return "Command";
      }
    }

    public ICommand Command
    {
      get
      {
        return this._Command;
      }
      set
      {
        base.SetProperty<ICommand>(this.PropertyCommand, ref this._Command, value);
      }
    }

    public ContextCommand(string header, ICommand command)
    {
      this.Header = header;
      this.Command = command;
    }

    public override string ToString()
    {
      return this.Header;
    }
  }
}
