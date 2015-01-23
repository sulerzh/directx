using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
    public delegate void ObservableCollectionExChangedHandler<T>(T item);
    public delegate void ObservableCollectionExItemChangedHandler<T>(T item, PropertyChangedEventArgs e);
    public delegate void ObservableCollectionExItemChangingHandler<T>(T item, PropertyChangingEventArgs e);

  [Serializable]
  public class ObservableCollectionEx<T> : ObservableCollection<T>
  {
    private bool _IsEmpty = true;
    private WeakEventListener<ObservableCollectionEx<T>, object, PropertyChangedEventArgs> onItemPropertyChanged;
    private WeakEventListener<ObservableCollectionEx<T>, object, PropertyChangingEventArgs> onItemPropertyChanging;
    private WeakEventListener<ObservableCollectionEx<T>, object, PropertyChangedEventArgs> onItemDescendentPropertyChanged;
    private WeakEventListener<ObservableCollectionEx<T>, object, PropertyChangingEventArgs> onItemDescendentPropertyChanging;
    private WeakEventListener<ObservableCollectionEx<T>, object, NotifyCollectionChangedEventArgs> onCollectionChanged;

    public string PropertyIsEmpty
    {
      get
      {
        return "IsEmpty";
      }
    }

    public bool IsEmpty
    {
      get
      {
        return this._IsEmpty;
      }
      private set
      {
        if (this._IsEmpty == value)
          return;
        this._IsEmpty = value;
        this.OnPropertyChanged(new PropertyChangedEventArgs(this.PropertyIsEmpty));
      }
    }

    public event ObservableCollectionExChangedHandler<T> ItemAdded;

    public event ObservableCollectionExChangedHandler<T> ItemRemoved;

    public event ObservableCollectionExItemChangedHandler<T> ItemPropertyChanged;

    public event ObservableCollectionExItemChangingHandler<T> ItemPropertyChanging;

    public event ObservableCollectionExItemChangedHandler<T> ItemDescendentPropertyChanged;

    public event ObservableCollectionExItemChangingHandler<T> ItemDescendentPropertyChanging;

    public ObservableCollectionEx()
    {
      this.initWeakListeners();
    }

    public ObservableCollectionEx(List<T> list)
      : base(list)
    {
      this.initWeakListeners();
    }

    public static ObservableCollectionEx<T> Clone(ObservableCollectionEx<T> source)
    {
      if (source != null)
        return source.Clone();
      else
        return (ObservableCollectionEx<T>) null;
    }

    private void initWeakListeners()
    {
      this.onItemDescendentPropertyChanged = new WeakEventListener<ObservableCollectionEx<T>, object, PropertyChangedEventArgs>(this)
      {
        OnEventAction = new Action<ObservableCollectionEx<T>, object, PropertyChangedEventArgs>(ObservableCollectionEx<T>.OnItemDescendentPropertyChanged)
      };
      this.onItemDescendentPropertyChanging = new WeakEventListener<ObservableCollectionEx<T>, object, PropertyChangingEventArgs>(this)
      {
        OnEventAction = new Action<ObservableCollectionEx<T>, object, PropertyChangingEventArgs>(ObservableCollectionEx<T>.OnItemDescendentPropertyChanging)
      };
      this.onItemPropertyChanged = new WeakEventListener<ObservableCollectionEx<T>, object, PropertyChangedEventArgs>(this)
      {
        OnEventAction = new Action<ObservableCollectionEx<T>, object, PropertyChangedEventArgs>(ObservableCollectionEx<T>.OnItemPropertyChanged)
      };
      this.onItemPropertyChanging = new WeakEventListener<ObservableCollectionEx<T>, object, PropertyChangingEventArgs>(this)
      {
        OnEventAction = new Action<ObservableCollectionEx<T>, object, PropertyChangingEventArgs>(ObservableCollectionEx<T>.OnItemPropertyChanging)
      };
      this.onCollectionChanged = new WeakEventListener<ObservableCollectionEx<T>, object, NotifyCollectionChangedEventArgs>(this)
      {
        OnEventAction = new Action<ObservableCollectionEx<T>, object, NotifyCollectionChangedEventArgs>(ObservableCollectionEx<T>.OnCollectionChanged)
      };
      this.CollectionChanged += new NotifyCollectionChangedEventHandler(this.onCollectionChanged.OnEvent);
    }

    private static void OnCollectionChanged(ObservableCollectionEx<T> collection, object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
      {
        int newStartingIndex = e.NewStartingIndex;
        foreach (T obj in (IEnumerable) e.NewItems)
        {
          if ((object) obj != null)
          {
            if ((object) obj is INotifyPropertyChanged)
              ((INotifyPropertyChanged) (object) obj).PropertyChanged += new PropertyChangedEventHandler(collection.onItemPropertyChanged.OnEvent);
            if ((object) obj is INotifyPropertyChanging)
              ((INotifyPropertyChanging) (object) obj).PropertyChanging += new PropertyChangingEventHandler(collection.onItemPropertyChanging.OnEvent);
            if ((object) obj is ICompositeProperty)
            {
              ((IDescendentPropertyChanged) (object) obj).DescendentPropertyChanged += new PropertyChangedEventHandler(collection.onItemDescendentPropertyChanged.OnEvent);
              ((IDescendentPropertyChanging) (object) obj).DescendentPropertyChanging += new PropertyChangingEventHandler(collection.onItemDescendentPropertyChanging.OnEvent);
            }
            collection.RaiseItemAdded(obj, newStartingIndex++);
          }
        }
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
      {
        int oldStartingIndex = e.OldStartingIndex;
        foreach (T obj in (IEnumerable) e.OldItems)
        {
          if ((object) obj != null)
          {
            if ((object) obj is INotifyPropertyChanged)
              ((INotifyPropertyChanged) (object) obj).PropertyChanged -= new PropertyChangedEventHandler(collection.onItemPropertyChanged.OnEvent);
            if ((object) obj is INotifyPropertyChanging)
              ((INotifyPropertyChanging) (object) obj).PropertyChanging -= new PropertyChangingEventHandler(collection.onItemPropertyChanging.OnEvent);
            if ((object) obj is ICompositeProperty)
            {
              ((IDescendentPropertyChanged) (object) obj).DescendentPropertyChanged -= new PropertyChangedEventHandler(collection.onItemDescendentPropertyChanged.OnEvent);
              ((IDescendentPropertyChanging) (object) obj).DescendentPropertyChanging -= new PropertyChangingEventHandler(collection.onItemDescendentPropertyChanging.OnEvent);
            }
            collection.RaiseItemRemoved(obj, oldStartingIndex++);
          }
        }
      }
      collection.IsEmpty = collection.Count == 0;
    }

    private static void OnItemDescendentPropertyChanging(ObservableCollectionEx<T> collection, object sender, PropertyChangingEventArgs e)
    {
      if (collection.ItemDescendentPropertyChanging == null || !(sender is T))
        return;
      collection.ItemDescendentPropertyChanging((T) sender, e);
    }

    private static void OnItemPropertyChanged(ObservableCollectionEx<T> collection, object sender, PropertyChangedEventArgs e)
    {
      if (collection.ItemPropertyChanged == null || !(sender is T))
        return;
      collection.ItemPropertyChanged((T) sender, e);
    }

    private static void OnItemPropertyChanging(ObservableCollectionEx<T> collection, object sender, PropertyChangingEventArgs e)
    {
      if (collection.ItemPropertyChanging == null || !(sender is T))
        return;
      collection.ItemPropertyChanging((T) sender, e);
    }

    private static void OnItemDescendentPropertyChanged(ObservableCollectionEx<T> collection, object sender, PropertyChangedEventArgs e)
    {
      if (collection.ItemDescendentPropertyChanged == null || !(sender is T))
        return;
      collection.ItemDescendentPropertyChanged((T) sender, e);
    }

    private void RaiseItemAdded(T item, int index)
    {
      if (this.ItemAdded == null)
        return;
      this.ItemAddedTemplate(item, index);
      this.ItemAdded(item);
    }

    protected virtual void ItemAddedTemplate(T item, int index)
    {
    }

    private void RaiseItemRemoved(T item, int index)
    {
      if (this.ItemRemoved == null)
        return;
      this.ItemRemovedTemplate(item, index);
      this.ItemRemoved(item);
    }

    protected virtual void ItemRemovedTemplate(T item, int index)
    {
    }

    public ObservableCollectionEx<T> Clone()
    {
      ObservableCollectionEx<T> observableCollectionEx = new ObservableCollectionEx<T>();
      foreach (T obj in (Collection<T>) this)
        observableCollectionEx.Add(obj);
      return observableCollectionEx;
    }

    public void RemoveAll()
    {
      while (this.Count > 0)
        this.RemoveAt(0);
    }
  }
}
