namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class ModelQueryColumn
  {
    public TableMember TableColumn { get; set; }

    public dynamic Values { get; set; }

    public bool FetchValues { get; set; }

    public ModelQueryColumn AliasOf { get; set; }

    public virtual void Reset()
    {
      this.Values = (object) null;
      this.AliasOf = (ModelQueryColumn) null;
    }

    internal virtual void Shutdown()
    {
      this.Reset();
    }
  }
}
