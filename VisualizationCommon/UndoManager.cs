using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class UndoManager : IUndoManager
  {
    private readonly ObservableCollection<UndoItem> undoItems = new ObservableCollection<UndoItem>();
    private readonly ObservableCollection<UndoItem> redoItems = new ObservableCollection<UndoItem>();
    private UndoItem transactionItem;
    private readonly int maxDepth;

    public bool Suspend { get; set; }

    public bool IsUndoAvailable
    {
      get
      {
        return this.undoItems.Count > 0;
      }
    }

    public bool IsRedoAvailable
    {
      get
      {
        return this.redoItems.Count > 0;
      }
    }

    public ObservableCollection<UndoItem> UndoRecords
    {
      get
      {
        return this.undoItems;
      }
    }

    public ObservableCollection<UndoItem> RedoRecords
    {
      get
      {
        return this.redoItems;
      }
    }

    public event EventHandler IsUndoRedoAvailableChanged;

    public UndoManager(int maxDepth)
    {
      this.maxDepth = maxDepth < 10 ? 10 : maxDepth;
    }

    public void Push(UndoItem item)
    {
      this.Push(item, false);
    }

    public void Push(UndoItem item, bool resetRedoStack)
    {
      if (this.Suspend)
        return;
      if (item.State == UndoItem.TransactionState.Begin)
        this.transactionItem = item;
      else if (item.State != UndoItem.TransactionState.None && this.transactionItem.AddTransactionItem(item))
      {
        item = this.transactionItem;
        this.transactionItem = (UndoItem) null;
      }
      if (this.undoItems.Count == this.maxDepth)
        this.undoItems.RemoveAt(0);
      this.undoItems.Add(item);
      if (resetRedoStack)
        this.redoItems.Clear();
      this.UndoRedoAvailableChanged();
    }

    public void Undo()
    {
      if (this.undoItems.Count == 0)
        throw new InvalidOperationException("No undo operation to execute.");
      UndoItem undoItem = Enumerable.Last<UndoItem>((IEnumerable<UndoItem>) this.undoItems);
      this.undoItems.RemoveAt(this.undoItems.Count - 1);
      if (this.redoItems.Count == this.maxDepth)
        this.redoItems.RemoveAt(0);
      this.redoItems.Add(undoItem.Undo());
      this.UndoRedoAvailableChanged();
    }

    public void Undo(int count)
    {
      if (count > this.undoItems.Count || count < 1)
        throw new ArgumentOutOfRangeException("count");
      for (int index = 0; index < count; ++index)
        this.Undo();
    }

    public void Redo()
    {
      if (this.redoItems.Count == 0)
        throw new InvalidOperationException("No redo operation to execute.");
      UndoItem undoItem = Enumerable.Last<UndoItem>((IEnumerable<UndoItem>) this.redoItems);
      this.redoItems.RemoveAt(this.redoItems.Count - 1);
      if (this.undoItems.Count == this.maxDepth)
        this.undoItems.RemoveAt(0);
      this.undoItems.Add(undoItem.Undo());
      this.UndoRedoAvailableChanged();
    }

    public void Reset()
    {
      this.undoItems.Clear();
      this.redoItems.Clear();
      this.UndoRedoAvailableChanged();
    }

    public void Redo(int count)
    {
      if (count > this.redoItems.Count || count < 1)
        throw new ArgumentOutOfRangeException("count");
      for (int index = 0; index < count; ++index)
        this.Redo();
    }

    private void UndoRedoAvailableChanged()
    {
      EventHandler eventHandler = this.IsUndoRedoAvailableChanged;
      if (eventHandler == null)
        return;
      eventHandler((object) this, EventArgs.Empty);
    }

    public void ResetRedo()
    {
      this.redoItems.Clear();
      this.UndoRedoAvailableChanged();
    }
  }
}
