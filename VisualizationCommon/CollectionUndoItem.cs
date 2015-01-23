using System;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
    public class CollectionUndoItem<T> : UndoItem
    {
        private readonly T changeItem;
        private int startIndex;
        private int endIndex;
        private ChangeType changeType;

        public ChangeType ChangeType
        {
            get
            {
                return this.changeType;
            }
        }

        public int StartIndex
        {
            get
            {
                return this.startIndex;
            }
        }

        public int EndIndex
        {
            get
            {
                return this.endIndex;
            }
        }

        public CollectionUndoItem(string description, UndoObservableCollection<T> target, ChangeType type, T item, int startIndex, int endIndex = -1, UndoItem.TransactionState state = UndoItem.TransactionState.None)
            : base(description, (object)target, state)
        {
            this.changeItem = item;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.changeType = type;
        }

        protected override UndoItem Execute()
        {
            UndoObservableCollection<T> observableCollection = this.Target as UndoObservableCollection<T>;
            try
            {
                observableCollection.SuppressUndoActions = true;
                switch (this.ChangeType)
                {
                    case ChangeType.Add:
                        observableCollection.RemoveAt(this.StartIndex);
                        this.changeType = ChangeType.Remove;
                        break;
                    case ChangeType.Remove:
                        observableCollection.Insert(this.StartIndex, this.changeItem);
                        this.changeType = ChangeType.Add;
                        break;
                    case ChangeType.Move:
                        observableCollection.Move(this.EndIndex, this.StartIndex);
                        int endIndex = this.EndIndex;
                        this.endIndex = this.StartIndex;
                        this.startIndex = endIndex;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Change Type", "Unsupported Undo action.");
                }
            }
            finally
            {
                observableCollection.SuppressUndoActions = false;
            }
            return base.Execute();
        }
    }
}
