using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public abstract class PropertyChangedNotificationBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged(string property)
    {
      if (this.PropertyChanged == null)
        return;
      this.PropertyChanged((object) this, new PropertyChangedEventArgs(property));
    }

    protected virtual bool SetProperty<T>(string propertyName, ref T property, T newValue)
    {
      property = newValue;
      this.RaisePropertyChanged(propertyName);
      return true;
    }

    protected virtual void RemoveSubscriptions()
    {
      this.PropertyChanged = (PropertyChangedEventHandler) null;
    }
  }
}
