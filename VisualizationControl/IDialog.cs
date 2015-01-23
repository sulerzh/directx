using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface IDialog
  {
    string Title { get; set; }

    string Description { get; set; }

    ICommand CancelCommand { get; set; }
  }
}
