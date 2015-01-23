using System.Collections;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ModelQueryIndexedKeyColumn : ModelQueryKeyColumn
  {
    public dynamic AllValues { get; set; }

    public bool PreserveValues { get; set; }

    public ArrayList AllPreservedValues { get; set; }

    public int[] PreservedValuesIndex { get; set; }

    public override void Reset()
    {
      this.AllValues = (object) null;
      this.PreservedValuesIndex = (int[]) null;
      base.Reset();
    }

    internal new virtual void Shutdown()
    {
      base.Shutdown();
      this.AllPreservedValues = (ArrayList) null;
    }
  }
}
