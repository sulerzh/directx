using System;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public abstract class PropertyChangeNotificationBase : PropertyChangedNotificationBase, INotifyPropertyChanges, INotifyPropertyChanged, INotifyPropertyChanging
  {
    public event PropertyChangingEventHandler PropertyChanging;

    protected void RaisePropertyChanging(string property)
    {
      if (this.PropertyChanging == null)
        return;
      this.PropertyChanging((object) this, new PropertyChangingEventArgs(property));
    }

    protected override bool SetProperty<T>(string propertyName, ref T property, T newValue)
    {
      if ((object) property == null && (object) newValue == null || (object) property != null && property.Equals((object) newValue) || !this.BeforePropertyChange<T>(propertyName, ref property, newValue))
        return false;
      this.RaisePropertyChanging(propertyName);
      property = newValue;
      this.AfterPropertyChange<T>(propertyName, ref property, newValue);
      this.RaisePropertyChanged(propertyName);
      return true;
    }

    protected bool SetProperty<T>(string propertyName, ref T property, T newValue, Action callbackIfChanged)
    {
      bool flag = base.SetProperty<T>(propertyName, ref property, newValue);
      if (callbackIfChanged != null)
        callbackIfChanged();
      return flag;
    }

    protected virtual bool BeforePropertyChange<T>(string propertyName, ref T property, T newValue)
    {
      return true;
    }

    protected virtual void AfterPropertyChange<T>(string propertyName, ref T property, T newValue)
    {
    }
  }
}
