using Microsoft.Data.Visualization.VisualizationCommon;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class AddChartDialogViewModel : DialogViewModelBase
  {
    private ICommand _CreateCommand;
    private ObservableCollectionEx<LayerViewModel> _layers;
    private LayerViewModel _selectedLayer;

    public static string PropertyCreateCommand
    {
      get
      {
        return "CreateCommand";
      }
    }

    public ICommand CreateCommand
    {
      get
      {
        return this._CreateCommand;
      }
      set
      {
        this.SetProperty<ICommand>(AddChartDialogViewModel.PropertyCreateCommand, ref this._CreateCommand, value, false);
      }
    }

    public static string PropertyLayers
    {
      get
      {
        return "Layers";
      }
    }

    public ObservableCollectionEx<LayerViewModel> Layers
    {
      get
      {
        return this._layers;
      }
      set
      {
        this.SetProperty<ObservableCollectionEx<LayerViewModel>>(AddChartDialogViewModel.PropertyLayers, ref this._layers, value, false);
      }
    }

    public static string PropertySelectedLayer
    {
      get
      {
        return "SelectedLayer";
      }
    }

    public LayerViewModel SelectedLayer
    {
      get
      {
        return this._selectedLayer;
      }
      set
      {
        this.SetProperty<LayerViewModel>(AddChartDialogViewModel.PropertySelectedLayer, ref this._selectedLayer, value, false);
      }
    }
  }
}
