using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TaskPanelSettingsTabViewModel : ViewModelBase
  {
    private int _SelectedSubheadIndex;

    public LayerSettingsViewModel LayerSettings { get; private set; }

    public SceneSettingsViewModel SceneSettings { get; private set; }

    public static string PropertySelectedSubheadIndex
    {
      get
      {
        return "SelectedSubheadIndex";
      }
    }

    public int SelectedSubheadIndex
    {
      get
      {
        return this._SelectedSubheadIndex;
      }
      set
      {
        if (!this.SetProperty<int>(TaskPanelSettingsTabViewModel.PropertySelectedSubheadIndex, ref this._SelectedSubheadIndex, value, false))
          return;
        this.RaisePropertyChanged(TaskPanelSettingsTabViewModel.PropertySelectedSubheadIndex);
      }
    }

    public TaskPanelSettingsTabViewModel(LayerSettingsViewModel layerSettings, SceneSettingsViewModel sceneSettings)
    {
      this.LayerSettings = layerSettings;
      this.LayerSettings.PropertyChanged += new PropertyChangedEventHandler(this.LayerSettings_PropertyChanged);
      this.SceneSettings = sceneSettings;
    }

    private void LayerSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == this.LayerSettings.PropertyLayerExists) || this.LayerSettings.LayerExists || this.SelectedSubheadIndex != 0)
        return;
      this.SelectedSubheadIndex = 1 % Enum.GetValues(typeof (TaskPanelSettingsSubhead)).Length;
    }
  }
}
