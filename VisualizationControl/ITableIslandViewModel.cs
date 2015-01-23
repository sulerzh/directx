using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface ITableIslandViewModel
  {
    List<ITableViewModel> Tables { get; }
  }
}
