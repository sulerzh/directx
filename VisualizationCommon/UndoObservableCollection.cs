using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class UndoObservableCollection<T> : ObservableCollectionEx<T>
  {
    private readonly Queue<int> removeIndices = new Queue<int>();
    private Func<T, ChangeType, string> descriptionGenerator;

    public bool SuppressUndoActions { get; set; }

    public bool CollapseRemoveAndAdd { get; set; }

    public IUndoManager Manager { get; set; }

    public UndoObservableCollection(IUndoManager manager, Func<T, ChangeType, string> descriptionGenerator)
    {
      this.Init(manager, descriptionGenerator);
    }

    public UndoObservableCollection(List<T> list, IUndoManager manager, Func<T, ChangeType, string> descriptionGenerator)
      : base(list)
    {
      this.Init(manager, descriptionGenerator);
    }

    private void Init(IUndoManager manager, Func<T, ChangeType, string> descriptionGenerator)
    {
      this.SuppressUndoActions = false;
      this.ItemAdded += (ObservableCollectionExChangedHandler<T>) (item => {});
      this.ItemRemoved += (ObservableCollectionExChangedHandler<T>) (item => {});
      this.Manager = manager;
      if (descriptionGenerator == null)
        this.descriptionGenerator = (Func<T, ChangeType, string>) ((t, c) => string.Empty);
      else
        this.descriptionGenerator = descriptionGenerator;
    }

    protected override void ItemAddedTemplate(T item, int index)
    {
      if (this.Manager == null || this.SuppressUndoActions)
        return;
      if (this.CollapseRemoveAndAdd && this.removeIndices.Count > 0)
      {
        int startIndex = this.removeIndices.Dequeue();
        if (startIndex == index)
          return;
        this.Manager.Push((UndoItem) new CollectionUndoItem<T>(this.descriptionGenerator(item, ChangeType.Move), this, ChangeType.Move, item, startIndex, index, UndoItem.TransactionState.None));
      }
      else
      {
        this.Manager.Push((UndoItem) new CollectionUndoItem<T>(this.descriptionGenerator(item, ChangeType.Add), this, ChangeType.Add, item, index, -1, UndoItem.TransactionState.None));
        this.Manager.ResetRedo();
        this.removeIndices.Clear();
      }
    }

    protected override void ItemRemovedTemplate(T item, int index)
    {
      if (this.Manager == null || this.SuppressUndoActions)
        return;
      if (this.CollapseRemoveAndAdd)
      {
        this.removeIndices.Enqueue(index);
      }
      else
      {
        this.Manager.Push((UndoItem) new CollectionUndoItem<T>(this.descriptionGenerator(item, ChangeType.Remove), this, ChangeType.Remove, item, index, -1, UndoItem.TransactionState.None));
        this.Manager.ResetRedo();
        this.removeIndices.Clear();
      }
    }
  }
}
