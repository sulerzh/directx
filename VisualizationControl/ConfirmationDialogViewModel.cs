using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ConfirmationDialogViewModel : DialogViewModelBase
  {
    public ObservableCollectionEx<DelegatedCommand> Commands { get; private set; }

    public ConfirmationDialogViewModel()
    {
      this.Commands = new ObservableCollectionEx<DelegatedCommand>();
    }
  }
}
