namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface IModelWrapper
  {
    bool ConnectionsDisabled { get; }

    ModelMetadata GetTableMetadata();

    void RefreshAll();
  }
}
