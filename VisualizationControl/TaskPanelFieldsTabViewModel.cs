using Microsoft.Data.Visualization.WpfExtensions;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TaskPanelFieldsTabViewModel : ViewModelBase
  {
    public ILayerManagerViewModel Model { get; private set; }

    public TaskPanelFieldsTabViewModel(ILayerManagerViewModel layerManagerViewModel)
    {
      this.Model = layerManagerViewModel;
    }
  }
}
