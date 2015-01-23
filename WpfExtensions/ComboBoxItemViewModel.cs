namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class ComboBoxItemViewModel : ViewModelBase
  {
    private bool _IsEnabled = true;
    private object _Content;

    public string PropertyContent
    {
      get
      {
        return "Content";
      }
    }

    public object Content
    {
      get
      {
        return this._Content;
      }
      set
      {
        this.SetProperty<object>(this.PropertyContent, ref this._Content, value, false);
      }
    }

    public string PropertyIsEnabled
    {
      get
      {
        return "IsEnabled";
      }
    }

    public bool IsEnabled
    {
      get
      {
        return this._IsEnabled;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyIsEnabled, ref this._IsEnabled, value, false);
      }
    }
  }
}
