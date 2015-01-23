using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class NPCContainer<T> : PropertyChangeNotificationBase where T : INotifyPropertyChanged
  {
    private T _Value;

    public string PropertyValue
    {
      get
      {
        return "Value";
      }
    }

    public T Value
    {
      get
      {
        return this._Value;
      }
      set
      {
        if ((object) this._Value != null && !this._Value.Equals((object) value))
        {
          this._Value.PropertyChanged -= new PropertyChangedEventHandler(this.Value_PropertyChanged);
          ICompositeProperty compositeProperty = (object) this._Value as ICompositeProperty;
          if (compositeProperty != null)
            compositeProperty.DescendentPropertyChanged -= new PropertyChangedEventHandler(this.Value_DescendentPropertyChanged);
        }
        if (!base.SetProperty<T>(this.PropertyValue, ref this._Value, value) || (object) this._Value == null)
          return;
        this._Value.PropertyChanged += new PropertyChangedEventHandler(this.Value_PropertyChanged);
        ICompositeProperty compositeProperty1 = (object) this._Value as ICompositeProperty;
        if (compositeProperty1 == null)
          return;
        compositeProperty1.DescendentPropertyChanged += new PropertyChangedEventHandler(this.Value_DescendentPropertyChanged);
      }
    }

    public event PropertyChangedEventHandler ValuePropertyChanged;

    public event PropertyChangedEventHandler ValueDescendentPropertyChanged;

    private void Value_DescendentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.ValueDescendentPropertyChanged == null)
        return;
      this.ValueDescendentPropertyChanged(sender, e);
    }

    private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.ValuePropertyChanged == null)
        return;
      this.ValuePropertyChanged(sender, e);
    }
  }
}
