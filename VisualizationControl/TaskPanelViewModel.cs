using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TaskPanelViewModel : ViewModelBase
  {
    public static int IndexOfLayersTab = 0;
    public static int IndexOfFieldsTab = 1;
    public static int IndexOfFilterTab = 2;
    public static int IndexOfSettingsTab = 3;
    private bool _Visible = true;
    private ICommand _CloseCommand;
    private TaskPanelLayersTabViewModel _LayersTab;
    private TaskPanelFieldsTabViewModel _FieldsTab;
    private TaskPanelSettingsTabViewModel _SettingsTab;
    private TaskPanelFiltersTabViewModel _FiltersTab;
    private int _SelectedIndex;

    public string PropertyCloseCommand
    {
      get
      {
        return "CloseCommand";
      }
    }

    public ICommand CloseCommand
    {
      get
      {
        return this._CloseCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyCloseCommand, ref this._CloseCommand, value, false);
      }
    }

    public string PropertyVisible
    {
      get
      {
        return "Visible";
      }
    }

    public bool Visible
    {
      get
      {
        return this._Visible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyVisible, ref this._Visible, value, false);
      }
    }

    public string PropertyLayersTab
    {
      get
      {
        return "LayersTab";
      }
    }

    public TaskPanelLayersTabViewModel LayersTab
    {
      get
      {
        return this._LayersTab;
      }
      set
      {
        this.SetProperty<TaskPanelLayersTabViewModel>(this.PropertyLayersTab, ref this._LayersTab, value, false);
      }
    }

    public string PropertyFieldsTab
    {
      get
      {
        return "FieldsTab";
      }
    }

    public TaskPanelFieldsTabViewModel FieldsTab
    {
      get
      {
        return this._FieldsTab;
      }
      set
      {
        this.SetProperty<TaskPanelFieldsTabViewModel>(this.PropertyFieldsTab, ref this._FieldsTab, value, false);
      }
    }

    public string PropertySettingsTab
    {
      get
      {
        return "SettingsTab";
      }
    }

    public TaskPanelSettingsTabViewModel SettingsTab
    {
      get
      {
        return this._SettingsTab;
      }
      set
      {
        this.SetProperty<TaskPanelSettingsTabViewModel>(this.PropertySettingsTab, ref this._SettingsTab, value, false);
      }
    }

    public string PropertyFiltersTab
    {
      get
      {
        return "FiltersTab";
      }
    }

    public TaskPanelFiltersTabViewModel FiltersTab
    {
      get
      {
        return this._FiltersTab;
      }
      set
      {
        this.SetProperty<TaskPanelFiltersTabViewModel>(this.PropertyFiltersTab, ref this._FiltersTab, value, false);
      }
    }

    public string PropertySelectedIndex
    {
      get
      {
        return "SelectedIndex";
      }
    }

    public int SelectedIndex
    {
      get
      {
        return this._SelectedIndex;
      }
      set
      {
        this.SetProperty<int>(this.PropertySelectedIndex, ref this._SelectedIndex, value, false);
      }
    }

    public TaskPanelViewModel(ILayerManagerViewModel layerManagerViewModel, SceneSettingsViewModel sceneSettingsViewModel)
    {
      if (layerManagerViewModel != null)
      {
        this.LayersTab = new TaskPanelLayersTabViewModel(layerManagerViewModel);
        this.FieldsTab = new TaskPanelFieldsTabViewModel(layerManagerViewModel);
        this.LayersTab.OnLayerAdded += (Action) (() => this.SelectedIndex = TaskPanelViewModel.IndexOfFieldsTab);
        layerManagerViewModel.OpenSettingsAction = (Action) (() => this.OpenSettings(TaskPanelSettingsSubhead.LayerSettings));
        layerManagerViewModel.ChangeCurrentSettingsAction = (Action) (() => this.ChangeCurrentSettings(TaskPanelSettingsSubhead.LayerSettings));
        this.SettingsTab = new TaskPanelSettingsTabViewModel(layerManagerViewModel.Settings, sceneSettingsViewModel);
        this.FiltersTab = new TaskPanelFiltersTabViewModel(layerManagerViewModel);
      }
      this.SelectedIndex = TaskPanelViewModel.IndexOfFieldsTab;
      this.CloseCommand = (ICommand) new DelegatedCommand(new Action(this.OnTaskPanelClose));
    }

    internal void OpenSettings(TaskPanelSettingsSubhead subhead)
    {
      this.ChangeCurrentSettings(subhead);
      this.SelectedIndex = TaskPanelViewModel.IndexOfSettingsTab;
      this.Visible = true;
    }

    internal void ChangeCurrentSettings(TaskPanelSettingsSubhead subhead)
    {
      this.SettingsTab.SelectedSubheadIndex = (int) subhead;
    }

    private void OnTaskPanelClose()
    {
      this.Visible = false;
    }
  }
}
