using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class CompositePropertyChangeNotificationBase : PropertyChangeNotificationBase, ICompositeProperty, INotifyPropertyChanged, INotifyPropertyChanging, IDescendentPropertyChanged, IDescendentPropertyChanging
  {
    private ConcurrentDictionary<PropertyInfo, object> _childViewModelProperties = new ConcurrentDictionary<PropertyInfo, object>();
    private ConcurrentDictionary<string, PropertyInfo> _properties = new ConcurrentDictionary<string, PropertyInfo>();
    private readonly WeakEventListener<CompositePropertyChangeNotificationBase, object, PropertyChangedEventArgs> childPropertyChanged;
    private readonly WeakEventListener<CompositePropertyChangeNotificationBase, object, PropertyChangingEventArgs> childPropertyChanging;

    protected ConcurrentDictionary<string, PropertyInfo> Properties
    {
      get
      {
        return this._properties;
      }
    }

    public event PropertyChangedEventHandler DescendentPropertyChanged;

    public event PropertyChangingEventHandler DescendentPropertyChanging;

    protected CompositePropertyChangeNotificationBase()
    {
      this.childPropertyChanged = new WeakEventListener<CompositePropertyChangeNotificationBase, object, PropertyChangedEventArgs>(this);
      this.childPropertyChanged.OnEventAction += new Action<CompositePropertyChangeNotificationBase, object, PropertyChangedEventArgs>(CompositePropertyChangeNotificationBase.OnChildPropertyChanged);
      this.childPropertyChanging = new WeakEventListener<CompositePropertyChangeNotificationBase, object, PropertyChangingEventArgs>(this);
      this.childPropertyChanging.OnEventAction += new Action<CompositePropertyChangeNotificationBase, object, PropertyChangingEventArgs>(CompositePropertyChangeNotificationBase.OnChildPropertyChanging);
      this.AddDescendentPropertyChangedEventHandlers();
    }

    private void AddDescendentPropertyChangedEventHandlers()
    {
      foreach (PropertyInfo propInfo in this.GetType().GetProperties())
      {
        if (this.Properties.TryAdd(propInfo.Name, propInfo) && TypeUtility.PropertyImplementsInterface(propInfo, typeof (ICompositeProperty)))
        {
          ICompositeProperty childVM = propInfo.GetValue((object) this, (object[]) null) as ICompositeProperty;
          this.AddChildViewModelPropertyMapping(propInfo, childVM);
        }
      }
    }

    private void AddChildViewModelPropertyMapping(PropertyInfo propInfo, ICompositeProperty childVM)
    {
      this._childViewModelProperties.AddOrUpdate(propInfo, (object) childVM, (Func<PropertyInfo, object, object>) ((info, o) =>
      {
        this.RemoveChildViewModelPropertyMapping(propInfo, o);
        return (object) childVM;
      }));
      if (childVM == null)
        return;
      childVM.PropertyChanged += new PropertyChangedEventHandler(this.childPropertyChanged.OnEvent);
      childVM.PropertyChanging += new PropertyChangingEventHandler(this.childPropertyChanging.OnEvent);
      childVM.DescendentPropertyChanged += new PropertyChangedEventHandler(this.childPropertyChanged.OnEvent);
      childVM.DescendentPropertyChanging += new PropertyChangingEventHandler(this.childPropertyChanging.OnEvent);
    }

    private void RemoveChildViewModelPropertyMapping(PropertyInfo propertyInfo, object val)
    {
      ICompositeProperty compositeProperty = val as ICompositeProperty;
      if (compositeProperty != null)
      {
        compositeProperty.PropertyChanged -= new PropertyChangedEventHandler(this.childPropertyChanged.OnEvent);
        compositeProperty.PropertyChanging -= new PropertyChangingEventHandler(this.childPropertyChanging.OnEvent);
        compositeProperty.DescendentPropertyChanged -= new PropertyChangedEventHandler(this.childPropertyChanged.OnEvent);
        compositeProperty.DescendentPropertyChanging -= new PropertyChangingEventHandler(this.childPropertyChanging.OnEvent);
      }
      this._childViewModelProperties.TryUpdate(propertyInfo, (object) null, val);
    }

    private static void OnChildPropertyChanging(CompositePropertyChangeNotificationBase compositePropertyChangeNotificationBase, object sender, PropertyChangingEventArgs e)
    {
      compositePropertyChangeNotificationBase.OnChildPropertyChangingTemplate(sender, e);
      foreach (KeyValuePair<PropertyInfo, object> keyValuePair in compositePropertyChangeNotificationBase._childViewModelProperties)
      {
        if (keyValuePair.Value == sender)
        {
          string property = keyValuePair.Key.Name + "." + e.PropertyName;
          compositePropertyChangeNotificationBase.RaiseDescendentPropertyChanging(property);
        }
      }
    }

    protected virtual void OnChildPropertyChangingTemplate(object sender, PropertyChangingEventArgs e)
    {
    }

    private static void OnChildPropertyChanged(CompositePropertyChangeNotificationBase compositePropertyChangeNotificationBase, object sender, PropertyChangedEventArgs e)
    {
      compositePropertyChangeNotificationBase.OnChildPropertyChangedTemplate(sender, e);
      foreach (KeyValuePair<PropertyInfo, object> keyValuePair in compositePropertyChangeNotificationBase._childViewModelProperties)
      {
        if (keyValuePair.Value == sender)
        {
          string property = keyValuePair.Key.Name + "." + e.PropertyName;
          compositePropertyChangeNotificationBase.RaiseDescendentPropertyChanged(property);
        }
      }
    }

    protected virtual void OnChildPropertyChangedTemplate(object sender, PropertyChangedEventArgs e)
    {
    }

    protected void RaiseDescendentPropertyChanging(string property)
    {
      if (this.DescendentPropertyChanging == null)
        return;
      this.DescendentPropertyChanging((object) this, new PropertyChangingEventArgs(property));
    }

    protected void RaiseDescendentPropertyChanged(string property)
    {
      if (this.DescendentPropertyChanged == null)
        return;
      this.DescendentPropertyChanged((object) this, new PropertyChangedEventArgs(property));
    }

    protected override bool BeforePropertyChange<T>(string propertyName, ref T property, T newValue)
    {
      if ((object) property != null && (object) property is ICompositeProperty)
      {
        PropertyInfo property1 = this.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object val;
        if ((PropertyInfo) null != property1 && this._childViewModelProperties.TryGetValue(property1, out val))
          this.RemoveChildViewModelPropertyMapping(property1, val);
      }
      return base.BeforePropertyChange<T>(propertyName, ref property, newValue);
    }

    protected override void AfterPropertyChange<T>(string propertyName, ref T property, T newValue)
    {
      if ((object) property == null || !((object) property is ICompositeProperty))
        return;
      this.AddChildViewModelPropertyMapping(this.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), (object) property as ICompositeProperty);
    }

    protected override void RemoveSubscriptions()
    {
      base.RemoveSubscriptions();
      this.DescendentPropertyChanged = (PropertyChangedEventHandler) null;
      this.DescendentPropertyChanging = (PropertyChangingEventHandler) null;
      foreach (KeyValuePair<PropertyInfo, object> keyValuePair in this._childViewModelProperties)
        this.RemoveChildViewModelPropertyMapping(keyValuePair.Key, keyValuePair.Value);
      this._childViewModelProperties.Clear();
    }
  }
}
