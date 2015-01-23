using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DecoratorLayerViewModel : ViewModelBase
  {
    private DecoratorCollectionViewModel _Decorators = new DecoratorCollectionViewModel();
    private ObservableCollectionEx<DecoratorModel> _decoratorModels = new ObservableCollectionEx<DecoratorModel>();
    private const double DefaultX = 300.0;
    private const double DefaultY = 300.0;
    private const double DecoratorVerticalPaddingPx = 12.0;
    private const double DecoratorHorizontalPaddingPx = 12.0;
    private const double DefaultChartHeight = 288.0;
    private const double DefaultChartWidth = 470.0;
    private const double DefaultTimeDecoratorWidth = 300.0;
    private AddLabelDialogViewModel _addLabelDialog;
    private AddChartDialogViewModel _addChartDialog;
    private LayerManagerViewModel _layerManager;
    private IDialogServiceProvider _dialogService;
    private readonly WeakEventListener<DecoratorLayerViewModel, LayerViewModel> _layerRemoved;
    private readonly WeakEventListener<DecoratorLayerViewModel, DecoratorModel> _decoratorModelRemoved;
    private readonly WeakEventListener<DecoratorLayerViewModel, DecoratorModel> _decoratorModelAdded;
    private readonly WeakEventListener<DecoratorLayerViewModel, LayerViewModel, PropertyChangedEventArgs> _layerPropertyChanged;
    private BitmapSource _DecoratorImage;
    private DecoratorViewModel _TimeDecoratorVM;

    public int DecoratorImageWidth { get; set; }

    public int DecoratorImageHeight { get; set; }

    public ContextCommand AddChartCommand { get; private set; }

    public ContextCommand AddLabelCommand { get; private set; }

    public ContextCommand AddLegendCommand { get; private set; }

    public ContextCommand AddCurrentLayerLegendCommand { get; private set; }

    public string PropertyDecorators
    {
      get
      {
        return "Decorators";
      }
    }

    public DecoratorCollectionViewModel Decorators
    {
      get
      {
        return this._Decorators;
      }
      set
      {
        this.SetProperty<DecoratorCollectionViewModel>(this.PropertyDecorators, ref this._Decorators, value, false);
      }
    }

    public static string PropertyDecoratorImage
    {
      get
      {
        return "DecoratorImage";
      }
    }

    public BitmapSource DecoratorImage
    {
      get
      {
        return this._DecoratorImage;
      }
      set
      {
        this.SetProperty<BitmapSource>(DecoratorLayerViewModel.PropertyDecoratorImage, ref this._DecoratorImage, value, false);
      }
    }

    public static string PropertyTimeDecoratorVM
    {
      get
      {
        return "TimeDecoratorVM";
      }
    }

    public DecoratorViewModel TimeDecoratorVM
    {
      get
      {
        return this._TimeDecoratorVM;
      }
      set
      {
        this.SetProperty<DecoratorViewModel>(DecoratorLayerViewModel.PropertyTimeDecoratorVM, ref this._TimeDecoratorVM, value, false);
      }
    }

    public DecoratorLayerViewModel(LayerManagerViewModel layerManager, IDialogServiceProvider dialogService)
    {
      this._dialogService = dialogService;
      this.AddChartCommand = new ContextCommand(Resources.Context_AddChart, (ICommand) new DelegatedCommand<string>(new Action<string>(this.AddChart), new Predicate<string>(this.CanExecuteAddChart)));
      this.AddLabelCommand = new ContextCommand(Resources.Context_AddTextBox, (ICommand) new DelegatedCommand(new Action(this.OnExecuteAddLabel)));
      this.AddLegendCommand = new ContextCommand(Resources.Command_ShowAllLegends, (ICommand) new DelegatedCommand(new Action(this.AddAllLayerLegends), new Predicate(this.CanExecuteAddLegend)));
      this.AddCurrentLayerLegendCommand = new ContextCommand(Resources.Context_AddLegendForCurrentLayer, (ICommand) new DelegatedCommand(new Action(this.OnExecuteAddCurrentLayerLegend), new Predicate(this.CanExecuteAddCurrentLayerLegend)));
      this._addLabelDialog = new AddLabelDialogViewModel();
      this._addLabelDialog.CreateCommand = (ICommand) new DelegatedCommand(new Action(this.OnAddLabelDialogOkay), new Predicate(this._addLabelDialog.CanExecuteCreateCommand));
      this._addChartDialog = new AddChartDialogViewModel();
      this._addChartDialog.CreateCommand = (ICommand) new DelegatedCommand(new Action(this.OnAddChartDialogInsert));
      if (layerManager != null)
      {
        this._layerManager = layerManager;
        this._layerRemoved = new WeakEventListener<DecoratorLayerViewModel, LayerViewModel>(this);
        this._layerRemoved.OnEventAction = new Action<DecoratorLayerViewModel, LayerViewModel>(DecoratorLayerViewModel.LayerRemoved);
        this._layerManager.Layers.ItemRemoved += new ObservableCollectionExChangedHandler<LayerViewModel>(this._layerRemoved.OnEvent);
        this._layerPropertyChanged = new WeakEventListener<DecoratorLayerViewModel, LayerViewModel, PropertyChangedEventArgs>(this);
        this._layerPropertyChanged.OnEventAction = new Action<DecoratorLayerViewModel, LayerViewModel, PropertyChangedEventArgs>(DecoratorLayerViewModel.LayersOnItemPropertyChanged);
        this._layerManager.Layers.ItemPropertyChanged += new ObservableCollectionExItemChangedHandler<LayerViewModel>(this._layerPropertyChanged.OnEvent);
        this._decoratorModels = layerManager.Model.Decorators;
        this._decoratorModelAdded = new WeakEventListener<DecoratorLayerViewModel, DecoratorModel>(this);
        this._decoratorModelAdded.OnEventAction = new Action<DecoratorLayerViewModel, DecoratorModel>(DecoratorLayerViewModel.DecoratorModelAdded);
        this._decoratorModels.ItemAdded += new ObservableCollectionExChangedHandler<DecoratorModel>(this._decoratorModelAdded.OnEvent);
        this._decoratorModelRemoved = new WeakEventListener<DecoratorLayerViewModel, DecoratorModel>(this);
        this._decoratorModelRemoved.OnEventAction = new Action<DecoratorLayerViewModel, DecoratorModel>(DecoratorLayerViewModel.DecoratorModelRemoved);
        this._decoratorModels.ItemRemoved += new ObservableCollectionExChangedHandler<DecoratorModel>(this._decoratorModelRemoved.OnEvent);
      }
      this.Decorators.DecoratorClosed += new Action<DecoratorViewModel>(this.OnDecoratorClosed);
      this.Decorators.DecoratorEditted += new Action<DecoratorViewModel>(this.OnDecoratorEditted);
    }

    public bool Contains(DecoratorViewModel decoratorVM)
    {
      return this.Decorators.Decorators.Contains(decoratorVM);
    }

    public void UpdateTimeDecorator(DateTime time)
    {
      if (this.TimeDecoratorVM == null)
        return;
      TimeDecoratorModel timeDecoratorModel = this.TimeDecoratorVM.Model.Content as TimeDecoratorModel;
      if (timeDecoratorModel == null)
        return;
      timeDecoratorModel.Time = time;
    }

    public void AddTimeDecorator()
    {
      bool flag = false;
      foreach (DecoratorViewModel decoratorViewModel in (Collection<DecoratorViewModel>) this.Decorators.Decorators)
      {
        if (decoratorViewModel.Model.Content is TimeDecoratorModel)
        {
          this.TimeDecoratorVM = decoratorViewModel;
          flag = true;
          break;
        }
      }
      if (flag)
        return;
      DecoratorModel decoratorModel = this.AddDecorator((DecoratorContentBase) new TimeDecoratorModel(), false);
      decoratorModel.X = 12.0;
      decoratorModel.Y = 12.0;
      decoratorModel.DistanceToNearestCornerX = 12.0;
      decoratorModel.DistanceToNearestCornerY = 12.0;
      decoratorModel.Width = 300.0;
      decoratorModel.Height = double.NaN;
      foreach (DecoratorViewModel decoratorViewModel in (Collection<DecoratorViewModel>) this.Decorators.Decorators)
      {
        if (decoratorViewModel.Model.Content is TimeDecoratorModel)
        {
          this.TimeDecoratorVM = decoratorViewModel;
          break;
        }
      }
    }

    public void RemoveTimeDecorator()
    {
      TimeDecoratorModel timeDecoratorModel = (TimeDecoratorModel) null;
      foreach (DecoratorViewModel decoratorViewModel in (Collection<DecoratorViewModel>) this.Decorators.Decorators)
      {
        timeDecoratorModel = decoratorViewModel.Model.Content as TimeDecoratorModel;
        if (timeDecoratorModel != null)
          break;
      }
      if (timeDecoratorModel != null)
        this.RemoveDecorator((DecoratorContentBase) timeDecoratorModel);
      this.TimeDecoratorVM = (DecoratorViewModel) null;
    }

    public void AddAllLayerLegends()
    {
      int offsetIndex = 0;
      foreach (LayerViewModel layer in (Collection<LayerViewModel>) this._layerManager.Layers)
      {
        this.AddLayerLegend(layer, true, offsetIndex);
        ++offsetIndex;
      }
    }

    public void AddLayerLegend(LayerViewModel layer, bool focusOnLoadView, int offsetIndex = 0)
    {
      if (layer == null || !layer.HasLegendData() || this.GetLegendDecoratorForLayer(layer.LayerDefinition.Id) != null)
        return;
      DecoratorModel decoratorModel = this.AddDecorator((DecoratorContentBase) layer.Legend, focusOnLoadView);
      decoratorModel.IsVisible = layer.Visible;
      decoratorModel.Dock = DecoratorDock.TopRight;
      decoratorModel.DistanceToNearestCornerX = 12.0 * (double) (1 + offsetIndex);
      decoratorModel.DistanceToNearestCornerY = 12.0 * (double) (1 + offsetIndex);
      decoratorModel.UpdatePositionFromScreenSize(this.Decorators.Width, this.Decorators.Height);
    }

    private void OnDecoratorEditted(DecoratorViewModel decorator)
    {
      LabelDecoratorModel label = decorator.Model.Content as LabelDecoratorModel;
      if (label != null)
      {
        EditLabelDialogViewModel editDialog = new EditLabelDialogViewModel();
        editDialog.Label = label.Clone();
        editDialog.AcceptCommand = (ICommand) new DelegatedCommand((Action) (() =>
        {
          label.Title = editDialog.Label.Title;
          label.Description = editDialog.Label.Description;
          this._dialogService.DismissDialog((IDialog) editDialog);
        }));
        this._dialogService.ShowDialog((IDialog) editDialog);
      }
      else
      {
        TimeDecoratorModel time = decorator.Model.Content as TimeDecoratorModel;
        if (time == null)
          return;
        EditTimeDecoratorDialogViewModel editDialog = new EditTimeDecoratorDialogViewModel();
        editDialog.Model = time.Clone();
        foreach (TimeStringFormat timeStringFormat in editDialog.Formats)
        {
          if (timeStringFormat.Format == time.Format)
          {
            editDialog.SelectedFormat = timeStringFormat;
            break;
          }
        }
        editDialog.AcceptCommand = (ICommand) new DelegatedCommand((Action) (() =>
        {
          time.Text = editDialog.Model.Text;
          time.Format = editDialog.Model.Format;
          this._dialogService.DismissDialog((IDialog) editDialog);
        }));
        this._dialogService.ShowDialog((IDialog) editDialog);
      }
    }

    private void OnDecoratorClosed(DecoratorViewModel decorator)
    {
      if (decorator == this.TimeDecoratorVM)
      {
        decorator.Model.IsVisible = false;
      }
      else
      {
        this.Decorators.Decorators.Remove(decorator);
        this._decoratorModels.Remove(decorator.Model);
      }
    }

    private static void DecoratorModelAdded(DecoratorLayerViewModel decoratorLayerViewModel, DecoratorModel item)
    {
      if (item == null || decoratorLayerViewModel == null || decoratorLayerViewModel.Decorators == null || decoratorLayerViewModel.Decorators.Decorators == null)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Failed to execute DecoratorModelAdded.");
      }
      else
      {
        DecoratorViewModel decoratorViewModel = new DecoratorViewModel()
        {
          Model = item
        };
        if (item.Content is TimeDecoratorModel)
          decoratorLayerViewModel.TimeDecoratorVM = decoratorViewModel;
        item.UpdatePositionFromScreenSize(decoratorLayerViewModel.Decorators.Width, decoratorLayerViewModel.Decorators.Height);
        if (decoratorLayerViewModel._layerManager != null && decoratorLayerViewModel._layerManager.Layers != null)
        {
          LayerLegendDecoratorModel legendDecoratorModel = item.Content as LayerLegendDecoratorModel;
          if (legendDecoratorModel != null)
          {
            foreach (LayerViewModel layerViewModel in (Collection<LayerViewModel>) decoratorLayerViewModel._layerManager.Layers)
            {
              if (layerViewModel.LayerDefinition.Id == legendDecoratorModel.LayerId)
              {
                item.Content = (DecoratorContentBase) layerViewModel.Legend;
                item.IsVisible = layerViewModel.Visible;
                break;
              }
            }
          }
          else
          {
            ChartDecoratorModel model = item.Content as ChartDecoratorModel;
            if (model != null)
            {
              LayerViewModel layerViewModel = Enumerable.FirstOrDefault<LayerViewModel>((IEnumerable<LayerViewModel>) decoratorLayerViewModel._layerManager.Layers, (Func<LayerViewModel, bool>) (lvm => lvm.LayerDefinition.Id == model.LayerId));
              if (layerViewModel != null && layerViewModel.LayerDefinition != null && layerViewModel.LayerDefinition.GeoVisualization != null)
                layerViewModel.LayerDefinition.GeoVisualization.AddChartVisualization(model);
              else
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Failed to add chart visualization in DecoratorModelAdded.");
            }
          }
        }
        else
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "DecoratorModelAdded: LayerManagerViewModel or LayerViewModel collection is null.");
        decoratorLayerViewModel.Decorators.Decorators.Add(decoratorViewModel);
      }
    }

    private static void DecoratorModelRemoved(DecoratorLayerViewModel decoratorLayerViewModel, DecoratorModel item)
    {
      if (item == null || decoratorLayerViewModel == null || decoratorLayerViewModel.Decorators == null || decoratorLayerViewModel.Decorators.Decorators == null)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Failed to execute DecoratorModelRemoved.");
      }
      else
      {
        DecoratorViewModel decoratorViewModel1 = (DecoratorViewModel) null;
        foreach (DecoratorViewModel decoratorViewModel2 in (Collection<DecoratorViewModel>) decoratorLayerViewModel.Decorators.Decorators)
        {
          if (decoratorViewModel2.Model == item)
          {
            decoratorViewModel1 = decoratorViewModel2;
            break;
          }
        }
        if (decoratorViewModel1 != null)
          decoratorLayerViewModel.Decorators.Decorators.Remove(decoratorViewModel1);
        ChartDecoratorModel model = item.Content as ChartDecoratorModel;
        if (model != null && decoratorLayerViewModel._layerManager != null && decoratorLayerViewModel._layerManager.Layers != null)
        {
          LayerViewModel layerViewModel = Enumerable.FirstOrDefault<LayerViewModel>((IEnumerable<LayerViewModel>) decoratorLayerViewModel._layerManager.Layers, (Func<LayerViewModel, bool>) (lvm => lvm.LayerDefinition.Id == model.LayerId));
          if (layerViewModel != null)
            layerViewModel.LayerDefinition.GeoVisualization.RemoveChartVisualizationForModel(model);
        }
        LayerLegendDecoratorModel legendDecoratorModel = item.Content as LayerLegendDecoratorModel;
        if (legendDecoratorModel != null && decoratorLayerViewModel._layerManager != null && decoratorLayerViewModel._layerManager.Layers != null)
        {
          foreach (LayerViewModel layerViewModel in (Collection<LayerViewModel>) decoratorLayerViewModel._layerManager.Layers)
          {
            if (layerViewModel.LayerDefinition != null && layerViewModel.LayerDefinition.Id == legendDecoratorModel.LayerId)
              layerViewModel.UserClosedLegend = true;
          }
        }
        if (!(item.Content is TimeDecoratorModel))
          return;
        decoratorLayerViewModel.TimeDecoratorVM = (DecoratorViewModel) null;
      }
    }

    private void OnExecuteAddLabel()
    {
      if (this._dialogService == null)
        return;
      this._addLabelDialog.Label = new LabelDecoratorModel();
      this._dialogService.ShowDialog((IDialog) this._addLabelDialog);
    }

    private void OnAddLabelDialogOkay()
    {
      if (this._dialogService == null)
        return;
      this._dialogService.DismissDialog((IDialog) this._addLabelDialog);
      this.AddDecorator((DecoratorContentBase) this._addLabelDialog.Label, true);
      this._addLabelDialog.Label = (LabelDecoratorModel) null;
    }

    private bool LegendExistsForLayer(Guid layerId)
    {
      return this.GetLegendDecoratorForLayer(layerId) != null;
    }

    private DecoratorModel GetLegendDecoratorForLayer(Guid layerId)
    {
      foreach (DecoratorModel decoratorModel in (Collection<DecoratorModel>) this._decoratorModels)
      {
        LayerLegendDecoratorModel legendDecoratorModel = decoratorModel.Content as LayerLegendDecoratorModel;
        if (legendDecoratorModel != null && legendDecoratorModel.LayerId == layerId)
          return decoratorModel;
      }
      return (DecoratorModel) null;
    }

    private void OnAddChartDialogInsert()
    {
      if (this._dialogService == null)
        return;
      this._dialogService.DismissDialog((IDialog) this._addChartDialog);
      this.AddDecorator((DecoratorContentBase) new ChartDecoratorModel()
      {
        Type = ChartVisualization.ChartVisualizationType.Top,
        LayerId = this._addChartDialog.SelectedLayer.LayerDefinition.Id
      }, true);
      this._addChartDialog.SelectedLayer = (LayerViewModel) null;
      this._addChartDialog.Layers = (ObservableCollectionEx<LayerViewModel>) null;
    }

    private bool CanExecuteAddChart(string layer)
    {
      if (this._layerManager != null && this._layerManager.Layers != null && this._layerManager.Layers.Count > 0)
        return !Enumerable.Any<LayerViewModel>((IEnumerable<LayerViewModel>) this._layerManager.Layers, (Func<LayerViewModel, bool>) (lvm => lvm.LayerDefinition.ForInstructionsOnly));
      else
        return false;
    }

    private void AddChart(string layer)
    {
      if (this._dialogService == null)
        return;
      if (this._layerManager.Layers.Count == 1)
      {
        this.AddDecorator((DecoratorContentBase) new ChartDecoratorModel()
        {
          Type = ChartVisualization.ChartVisualizationType.Top,
          LayerId = this._layerManager.SelectedLayer.LayerDefinition.Id
        }, true);
      }
      else
      {
        this._addChartDialog.Layers = this._layerManager.Layers;
        this._addChartDialog.SelectedLayer = Enumerable.FirstOrDefault<LayerViewModel>((IEnumerable<LayerViewModel>) this._addChartDialog.Layers);
        this._dialogService.ShowDialog((IDialog) this._addChartDialog);
      }
    }

    private bool CanExecuteAddLegend()
    {
      bool flag = false;
      if (this._layerManager != null)
      {
        foreach (LayerViewModel layerViewModel in (Collection<LayerViewModel>) this._layerManager.Layers)
        {
          if (!this.LegendExistsForLayer(layerViewModel.LayerDefinition.Id))
            return true;
        }
      }
      return flag;
    }

    private void OnExecuteAddCurrentLayerLegend()
    {
      if (!this.CanExecuteAddCurrentLayerLegend())
        return;
      this.AddLayerLegend(this._layerManager.SelectedLayer, true, 0);
    }

    private bool CanExecuteAddCurrentLayerLegend()
    {
      if (this._layerManager.SelectedLayer != null && this._layerManager.SelectedLayer.HasLegendData())
        return !this.LegendExistsForLayer(this._layerManager.SelectedLayer.LayerDefinition.Id);
      else
        return false;
    }

    private DecoratorModel AddDecorator(DecoratorContentBase content, bool wantsFocus)
    {
      if (content == null)
        return (DecoratorModel) null;
      DecoratorModel decoratorModel = new DecoratorModel();
      decoratorModel.Dock = DecoratorDock.TopLeft;
      double num1 = 300.0;
      double num2 = 300.0;
      if (content is LabelDecoratorModel || content is ChartDecoratorModel)
      {
        num1 = (this.Decorators.Width - decoratorModel.Width) / 2.0;
        num2 = (this.Decorators.Height - decoratorModel.Height) / 2.0;
      }
      if (content is ChartDecoratorModel)
      {
        decoratorModel.Width = 470.0;
        decoratorModel.Height = 288.0;
      }
      decoratorModel.DistanceToNearestCornerX = num1;
      decoratorModel.DistanceToNearestCornerY = num2;
      decoratorModel.Content = content;
      this._decoratorModels.Add(decoratorModel);
      if (wantsFocus)
        decoratorModel.Focus();
      return decoratorModel;
    }

    private static void LayersOnItemPropertyChanged(DecoratorLayerViewModel decoratorLayerViewModel, LayerViewModel item, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (!propertyChangedEventArgs.PropertyName.Equals(item.PropertyVisible))
        return;
      foreach (DecoratorViewModel decoratorViewModel in (Collection<DecoratorViewModel>) decoratorLayerViewModel._Decorators.Decorators)
      {
        ChartDecoratorModel chartDecoratorModel = decoratorViewModel.Model.Content as ChartDecoratorModel;
        if (chartDecoratorModel != null && chartDecoratorModel.LayerId == item.LayerDefinition.Id)
        {
          decoratorViewModel.Model.IsVisible = item.Visible;
        }
        else
        {
          LayerLegendDecoratorModel legendDecoratorModel = decoratorViewModel.Model.Content as LayerLegendDecoratorModel;
          if (legendDecoratorModel != null && legendDecoratorModel.LayerId == item.LayerDefinition.Id)
            decoratorViewModel.Model.IsVisible = item.Visible;
        }
      }
    }

    private static void LayerRemoved(DecoratorLayerViewModel decoratorLayerViewModel, LayerViewModel item)
    {
      GeoVisualization geoVisualization = item.LayerDefinition.GeoVisualization;
      IEnumerable<ChartDecoratorModel> enumerable = geoVisualization == null ? (IEnumerable<ChartDecoratorModel>) null : geoVisualization.ChartDecoratorModels;
      if (enumerable != null)
        decoratorLayerViewModel.RemoveDecorators((IEnumerable<DecoratorContentBase>) new HashSet<DecoratorContentBase>((IEnumerable<DecoratorContentBase>) enumerable)
        {
          (DecoratorContentBase) item.Legend
        });
      else
        decoratorLayerViewModel.RemoveDecorators((IEnumerable<DecoratorContentBase>) new HashSet<DecoratorContentBase>()
        {
          (DecoratorContentBase) item.Legend
        });
    }

    private void RemoveDecorator(DecoratorContentBase decoratorContentBase)
    {
      this.RemoveDecorators((IEnumerable<DecoratorContentBase>) new List<DecoratorContentBase>()
      {
        decoratorContentBase
      });
    }

    private void RemoveDecorators(IEnumerable<DecoratorContentBase> models)
    {
      foreach (DecoratorContentBase decoratorContentBase in models)
      {
        DecoratorContentBase model = decoratorContentBase;
        DecoratorModel decoratorModel = Enumerable.FirstOrDefault<DecoratorModel>((IEnumerable<DecoratorModel>) this._decoratorModels, (Func<DecoratorModel, bool>) (m => m.Content == model));
        if (decoratorModel != null)
          this._decoratorModels.Remove(decoratorModel);
      }
    }
  }
}
