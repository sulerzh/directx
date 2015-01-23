using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class Result
  {
    public long EntityID { get; set; }

    public Metadata __metadata { get; set; }

    public EntityMetadata EntityMetadata { get; set; }

    public Name Name { get; set; }

    public List<Primitive> Primitives { get; set; }

    public Copyright Copyright { get; set; }
  }
}
