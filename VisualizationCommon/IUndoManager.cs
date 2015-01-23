namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public interface IUndoManager
  {
    void Undo();

    void Redo();

    void Push(UndoItem item);

    void ResetRedo();
  }
}
