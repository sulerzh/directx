using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface ILayerManagerViewModel : INotifyPropertyChanges, INotifyPropertyChanged, INotifyPropertyChanging
  {
    ObservableCollectionEx<LayerViewModel> Layers { get; }

    string PropertySelectedLayer { get; }

    LayerViewModel SelectedLayer { get; set; }

    bool CanAddLayers { get; }

    Action OpenSettingsAction { set; }

    Action ChangeCurrentSettingsAction { set; }

    LayerSettingsViewModel Settings { get; }

    void AddLayerDefinition();

    void AddLayerDefinition(LayerDefinition initialLayer);
  }
}
