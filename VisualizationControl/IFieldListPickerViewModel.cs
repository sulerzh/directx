using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface IFieldListPickerViewModel
  {
    List<ITableIslandViewModel> TableIslands { get; }
  }
}
