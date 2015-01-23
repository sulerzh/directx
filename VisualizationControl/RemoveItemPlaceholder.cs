namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class RemoveItemPlaceholder
  {
    private string resourceString;

    public RemoveItemPlaceholder(string resource)
    {
      this.resourceString = resource;
    }

    public override string ToString()
    {
      return this.resourceString;
    }
  }
}
