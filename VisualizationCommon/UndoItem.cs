using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public abstract class UndoItem
  {
    private bool isCommitted;
    protected List<UndoItem> TransactionList;

    public string Description { get; private set; }

    public object Target { get; private set; }

    public UndoItem.TransactionState State { get; private set; }

    protected UndoItem(string description, object target, UndoItem.TransactionState state = UndoItem.TransactionState.None)
    {
      this.Target = target;
      this.Description = description;
      this.State = state;
      this.isCommitted = state == UndoItem.TransactionState.None || state == UndoItem.TransactionState.Commit;
      if (this.State != UndoItem.TransactionState.Begin)
        return;
      this.TransactionList = new List<UndoItem>();
    }

    public virtual UndoItem Undo()
    {
      if (!this.isCommitted && this.State != UndoItem.TransactionState.Begin && this.State != UndoItem.TransactionState.None)
        throw new InvalidOperationException("Failed to execute undo operation. Item is not in a committed state or is not the start of a transactional undo item.");
      if (this.TransactionList != null)
      {
        for (int index = this.TransactionList.Count - 1; index >= 0; --index)
          this.TransactionList[index] = this.TransactionList[index].Execute();
      }
      return this.Execute();
    }

    internal bool AddTransactionItem(UndoItem item)
    {
      if (item.State == UndoItem.TransactionState.Begin || item.State == UndoItem.TransactionState.None)
        throw new ArgumentException("Undo item must be of type Commit or Payload in order to be added to the current undo transaction.");
      if (this.isCommitted)
        throw new ArgumentException("Undo item can not be added to a undo transaction in committed state.");
      if (item.State == UndoItem.TransactionState.Commit)
      {
        this.Description = item.Description;
        this.isCommitted = true;
        this.TransactionList.ForEach((Action<UndoItem>) (ti => ti.isCommitted = true));
      }
      this.TransactionList.Add(item);
      return this.isCommitted;
    }

    protected virtual UndoItem Execute()
    {
      switch (this.State)
      {
        case UndoItem.TransactionState.None:
          return this;
        case UndoItem.TransactionState.Begin:
          int count = this.TransactionList.Count - 1;
          this.State = UndoItem.TransactionState.Commit;
          this.TransactionList.Reverse(0, count);
          UndoItem undoItem = this.TransactionList[count];
          this.TransactionList[count] = this;
          undoItem.TransactionList = this.TransactionList;
          this.TransactionList = (List<UndoItem>) null;
          return undoItem;
        case UndoItem.TransactionState.Payload:
          return this;
        case UndoItem.TransactionState.Commit:
          this.State = UndoItem.TransactionState.Begin;
          return this;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public enum TransactionState
    {
      None,
      Begin,
      Payload,
      Commit,
    }
  }
}
