using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface ITableViewModel
  {
    List<ITableFieldViewModel> Fields { get; }
  }
}
