namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface IDataSourceFactory
  {
    DataSource CreateDataSource(string name);
  }
}
