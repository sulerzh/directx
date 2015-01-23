using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TaskPanelLayersTabViewModel : ViewModelBase
  {
    private ILayerManagerViewModel _Model;

    public string PropertyModel
    {
      get
      {
        return "Model";
      }
    }

    public ILayerManagerViewModel Model
    {
      get
      {
        return this._Model;
      }
      set
      {
        this.SetProperty<ILayerManagerViewModel>(this.PropertyModel, ref this._Model, value, false);
      }
    }

    public ObservableCollectionEx<LayerViewModel> Layers { get; private set; }

    public ICommand AddNewLayerCommand { get; private set; }

    public event Action OnLayerAdded;

    public TaskPanelLayersTabViewModel(ILayerManagerViewModel layerManagerViewModel)
    {
      this.Model = layerManagerViewModel;
      this.Layers = this.Model == null ? new ObservableCollectionEx<LayerViewModel>() : this.Model.Layers;
      this.AddNewLayerCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteAddLayer), new Predicate(this.CanExecuteAddLayer));
    }

    private void OnExecuteAddLayer()
    {
      this.Model.AddLayerDefinition();
      this.OnLayerAdded();
    }

    private bool CanExecuteAddLayer()
    {
      if (this.Model != null)
        return this.Model.CanAddLayers;
      else
        return false;
    }
  }
}
