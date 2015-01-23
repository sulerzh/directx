using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public sealed class UndoItemViewModel : ViewModelBase
  {
    private readonly UndoManager manager = new UndoManager(25);
    private const int MaxUndoDepth = 25;

    public ICommand UndoCommand { get; private set; }

    public ICommand RedoCommand { get; private set; }

    public UndoManager UndoManager
    {
      get
      {
        return this.manager;
      }
    }

    public ObservableCollection<UndoItem> UndoRecords
    {
      get
      {
        return this.manager.UndoRecords;
      }
    }

    public ObservableCollection<UndoItem> RedoRecords
    {
      get
      {
        return this.manager.RedoRecords;
      }
    }

    public UndoItemViewModel()
    {
      this.UndoCommand = (ICommand) new DelegatedCommand((Action) (() => this.manager.Undo()), (Predicate) (() => this.manager.IsUndoAvailable));
      this.RedoCommand = (ICommand) new DelegatedCommand((Action) (() => this.manager.Redo()), (Predicate) (() => this.manager.IsRedoAvailable));
      this.manager.IsUndoRedoAvailableChanged += new EventHandler(this.OnCanExecuteChanged);
    }

    private void OnCanExecuteChanged(object sender, EventArgs e)
    {
      CommandManager.InvalidateRequerySuggested();
    }
  }
}
