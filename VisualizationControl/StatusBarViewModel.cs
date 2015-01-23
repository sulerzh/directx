using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class StatusBarViewModel : ViewModelBase
  {
    private string _status = Resources.StatusBar_Ready;
    private const int FailureThreshold = 4;
    private ILayerManagerViewModel _layerManagerVM;
    private LayerViewModel _selectedLayer;
    private CompletionStatsViewModel _CompletionStats;
    private SelectionStats _SelectionStats;
    private GlobeNavigationViewModel _GlobeNavigation;
    private HostControlViewModel hostControlViewModel;

    public string PropertyCompletionStats
    {
      get
      {
        return "CompletionStats";
      }
    }

    public CompletionStatsViewModel CompletionStats
    {
      get
      {
        return this._CompletionStats;
      }
      set
      {
        this.SetProperty<CompletionStatsViewModel>(this.PropertyCompletionStats, ref this._CompletionStats, value, false);
      }
    }

    public string PropertySelectionStats
    {
      get
      {
        return "SelectionStats";
      }
    }

    public SelectionStats SelectionStats
    {
      get
      {
        return this._SelectionStats;
      }
      set
      {
        if (this._SelectionStats != null)
          this._SelectionStats.SelectionUpdate -= new Action(this.OnSelectionStatsUpdated);
        this.SetProperty<SelectionStats>(this.PropertySelectionStats, ref this._SelectionStats, value, false);
        if (this._SelectionStats == null)
          return;
        this._SelectionStats.SelectionUpdate += new Action(this.OnSelectionStatsUpdated);
      }
    }

    public string PropertyGlobeNavigation
    {
      get
      {
        return "GlobeNavigation";
      }
    }

    public GlobeNavigationViewModel GlobeNavigation
    {
      get
      {
        return this._GlobeNavigation;
      }
      set
      {
        this.SetProperty<GlobeNavigationViewModel>(this.PropertyGlobeNavigation, ref this._GlobeNavigation, value, false);
      }
    }

    public static string PropertyStatus
    {
      get
      {
        return "Status";
      }
    }

    public string Status
    {
      get
      {
        return this._status;
      }
      set
      {
        this.SetProperty<string>(StatusBarViewModel.PropertyStatus, ref this._status, value, false);
      }
    }

    public StatusBarViewModel(ILayerManagerViewModel layerManagerVM, HostControlViewModel model)
    {
      StatusBarViewModel statusBarViewModel = this;
      if (layerManagerVM == null || model == null)
        return;
      this.hostControlViewModel = model;
      if (this.hostControlViewModel.BingMapResourceUri.UrlsFromSettings)
        this.Status = Resources.StatusBar_Offline;
      this._selectedLayer = (LayerViewModel) null;
      this._layerManagerVM = layerManagerVM;
      layerManagerVM.PropertyChanged += (PropertyChangedEventHandler) ((sender, args) =>
      {
        if (!(args.PropertyName == layerManagerVM.PropertySelectedLayer))
          return;
        statusBarViewModel.UpdateSelectedLayer();
      });
      this.hostControlViewModel.Globe.PropertyChanged += new PropertyChangedEventHandler(this.GlobeOnPropertyChanged);
      this.UpdateSelectedLayer();
    }

    private void GlobeOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (this.hostControlViewModel == null || this.hostControlViewModel.Globe == null || !propertyChangedEventArgs.PropertyName.Equals(this.hostControlViewModel.Globe.PropertyCopyrightRequestFailedCount))
        return;
      if (this.hostControlViewModel.Globe.CopyrightRequestFailedCount > 4)
      {
        this.Status = Resources.StatusBar_Offline;
      }
      else
      {
        if (this.hostControlViewModel.Globe.CopyrightRequestFailedCount != 0)
          return;
        this.Status = Resources.StatusBar_Ready;
      }
    }

    private void OnSelectionStatsUpdated()
    {
      this.GlobeNavigation.CanExecuteZoomToSelectionCommand = this.SelectionStats.HasValues;
    }

    private void SelectedLayerPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
      LayerViewModel layerViewModel = sender as LayerViewModel;
      if (layerViewModel == null || !(args.PropertyName == layerViewModel.PropertyVisible))
        return;
      this.UpdateCompletionStatsViewModel();
    }

    private void UpdateSelectedLayer()
    {
      if (this._selectedLayer != null)
      {
        GeoVisualization geoVisualization = this._selectedLayer.LayerDefinition.GeoVisualization;
        if (geoVisualization != null)
          geoVisualization.DataUpdateStarted -= new Action(this.DataUpdateStarted);
        this._selectedLayer.PropertyChanged -= new PropertyChangedEventHandler(this.SelectedLayerPropertyChanged);
      }
      if (this._layerManagerVM.SelectedLayer != null)
      {
        this._selectedLayer = this._layerManagerVM.SelectedLayer;
        this._selectedLayer.PropertyChanged += new PropertyChangedEventHandler(this.SelectedLayerPropertyChanged);
        GeoVisualization geoVisualization = this._selectedLayer.LayerDefinition.GeoVisualization;
        if (geoVisualization != null)
          geoVisualization.DataUpdateStarted += new Action(this.DataUpdateStarted);
      }
      else
        this._selectedLayer = (LayerViewModel) null;
      this.UpdateCompletionStatsViewModel();
    }

    private void UpdateCompletionStatsViewModel()
    {
      if (this._selectedLayer != null)
      {
        GeoVisualization geoVisualization = this._selectedLayer.LayerDefinition.GeoVisualization;
        if (geoVisualization == null || !this._selectedLayer.Visible)
          this.CompletionStats = new CompletionStatsViewModel((CompletionStats) null);
        else
          this.CompletionStats = new CompletionStatsViewModel(geoVisualization.CompletionStats);
      }
      else
        this.CompletionStats = new CompletionStatsViewModel((CompletionStats) null);
    }

    private void DataUpdateStarted()
    {
      this.UpdateCompletionStatsViewModel();
    }
  }
}
