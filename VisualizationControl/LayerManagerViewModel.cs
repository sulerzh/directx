using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class LayerManagerViewModel : PropertyChangeNotificationBase, ILayerManagerViewModel, INotifyPropertyChanges, INotifyPropertyChanged, INotifyPropertyChanging
  {
    private static int MaxLayers = 30;
    private bool _CanAddLayers = true;
    private DecoratorLayerViewModel _DecoratorLayer = new DecoratorLayerViewModel((LayerManagerViewModel) null, (IDialogServiceProvider) null);
    private Dictionary<LayerViewModel, Action> _categoryAddedCallbacks = new Dictionary<LayerViewModel, Action>();
    private Dictionary<LayerViewModel, Action<int>> _heightFieldAddedCallbacks = new Dictionary<LayerViewModel, Action<int>>();
    private LayerViewModel _SelectedLayer;
    private LayerManager _Model;
    private Action openSettingsAction;
    private Action changeCurrentSettingsAction;
    private IDialogServiceProvider _dialogProvider;
    private HostControlViewModel _hostWindowVM;

    public ObservableCollectionEx<LayerViewModel> Layers { get; private set; }

    public string PropertySelectedLayer
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
        return this._SelectedLayer;
      }
      set
      {
        if (!base.SetProperty<LayerViewModel>(this.PropertySelectedLayer, ref this._SelectedLayer, value) || this.Settings == null)
          return;
        this.Settings.SetParentLayer(this._SelectedLayer);
      }
    }

    public string PropertyCanAddLayers
    {
      get
      {
        return "CanAddLayers";
      }
    }

    public bool CanAddLayers
    {
      get
      {
        return this._CanAddLayers;
      }
      private set
      {
        base.SetProperty<bool>(this.PropertyCanAddLayers, ref this._CanAddLayers, value);
      }
    }

    public string PropertyDecoratorLayer
    {
      get
      {
        return "DecoratorLayer";
      }
    }

    public DecoratorLayerViewModel DecoratorLayer
    {
      get
      {
        return this._DecoratorLayer;
      }
      private set
      {
        base.SetProperty<DecoratorLayerViewModel>(this.PropertyDecoratorLayer, ref this._DecoratorLayer, value);
      }
    }

    public string PropertyModel
    {
      get
      {
        return "Model";
      }
    }

    public LayerManager Model
    {
      get
      {
        return this._Model;
      }
      set
      {
        base.SetProperty<LayerManager>(this.PropertyModel, ref this._Model, value);
      }
    }

    public LayerSettingsViewModel Settings { get; private set; }

    public Action OpenSettingsAction
    {
      set
      {
        this.openSettingsAction = value;
      }
    }

    public Action ChangeCurrentSettingsAction
    {
      set
      {
        this.changeCurrentSettingsAction = value;
      }
    }

    public StatusBarViewModel StatusBar
    {
      get
      {
        if (this._hostWindowVM != null)
          return this._hostWindowVM.StatusBar;
        else
          return (StatusBarViewModel) null;
      }
    }

    public HostControlViewModel HostViewModel
    {
      get
      {
        return this._hostWindowVM;
      }
    }

    public LayerManagerViewModel()
    {
      this.Layers = new ObservableCollectionEx<LayerViewModel>();
      this.Layers.CollectionChanged += (NotifyCollectionChangedEventHandler) ((s, e) => this.RefreshCanAddLayers());
      this.Layers.ItemAdded += (ObservableCollectionExChangedHandler<LayerViewModel>) (newLayer =>
      {
        this.SelectedLayer = newLayer;
        this.AddHandlersForAutomaticallyShowingLayerLegend(newLayer);
      });
      this.Layers.ItemRemoved += (ObservableCollectionExChangedHandler<LayerViewModel>) (removedLayer =>
      {
        if (this.SelectedLayer == removedLayer)
          this.SelectedLayer = this.Layers.Count == 0 ? (LayerViewModel) null : this.Layers[0];
        removedLayer.Removed();
        this.RemoveHandlersForAutomaticallyShowingLayerLegend(removedLayer);
      });
      this._dialogProvider = (IDialogServiceProvider) null;
      this._hostWindowVM = (HostControlViewModel) null;
    }

    public LayerManagerViewModel(VisualizationModel visModel, List<Color4F> customColors = null)
      : this()
    {
      if (visModel == null)
        throw new ArgumentNullException("visModel");
      this.Model = visModel.LayerManager;
      foreach (LayerDefinition layerDef in (Collection<LayerDefinition>) this.Model.LayerDefinitions)
        this.AddLayerDefinition(layerDef);
      this.Model.LayerDefinitions.ItemAdded += new ObservableCollectionExChangedHandler<LayerDefinition>(this.AddLayerDefinition);
      this.Model.LayerDefinitions.ItemRemoved += new ObservableCollectionExChangedHandler<LayerDefinition>(this.RemoveLayerDefinition);
      this.Settings = new LayerSettingsViewModel((ILayerManagerViewModel) this, (IThemeService) new ThemeService(visModel.Engine), customColors);
    }

    public LayerManagerViewModel(VisualizationModel visModel, HostControlViewModel hostWindowVM, List<Color4F> customColors = null, IDialogServiceProvider dialogProvider = null)
      : this(visModel, customColors)
    {
      if (hostWindowVM == null)
        throw new ArgumentNullException("hostWindowVM");
      this._hostWindowVM = hostWindowVM;
      this._dialogProvider = dialogProvider;
      this._DecoratorLayer = new DecoratorLayerViewModel(this, dialogProvider);
    }

    public void OnNewView()
    {
      this.changeCurrentSettingsAction();
    }

    private void RemoveLayerDefinition(LayerDefinition item)
    {
      LayerViewModel layerViewModel1 = (LayerViewModel) null;
      foreach (LayerViewModel layerViewModel2 in (Collection<LayerViewModel>) this.Layers)
      {
        if (layerViewModel2.LayerDefinition == item)
        {
          layerViewModel1 = layerViewModel2;
          break;
        }
      }
      if (layerViewModel1 == null)
        return;
      this.Layers.Remove(layerViewModel1);
    }

    public void AddLayerDefinition()
    {
      if (this.Model != null)
        this.Model.AddLayerDefinition((string) null, (string) null, true, false);
      if (this.changeCurrentSettingsAction == null)
        return;
      this.changeCurrentSettingsAction();
    }

    public void AddLayerDefinition(LayerDefinition layerDef)
    {
      this.Layers.Add(new LayerViewModel(layerDef, this, this.Model.GetTableIslands(), this._dialogProvider)
      {
        DeleteLayerCommand = (ICommand) new DelegatedCommand<LayerViewModel>(new Action<LayerViewModel>(this.OnExecuteDeleteLayer)),
        LayerSettingsCommand = (ICommand) new DelegatedCommand<LayerViewModel>(new Action<LayerViewModel>(this.OnLayerSettings))
      });
    }

    private void OnExecuteDeleteLayer(LayerViewModel sender)
    {
        if (this._dialogProvider != null)
        {
            ConfirmationDialogViewModel dialog = new ConfirmationDialogViewModel
            {
                Title = Resources.DeleteLayerDialog_Title,
                Description = Resources.DeleteLayerDialog_Description
            };
            DelegatedCommand item = new DelegatedCommand(() => this.OnConfirmDeleteLayer(sender, dialog))
            {
                Name = Resources.DeleteLayerDialog_Delete
            };
            DelegatedCommand command2 = new DelegatedCommand(() => this._dialogProvider.DismissDialog(dialog))
            {
                Name = Resources.Dialog_CancelText
            };
            dialog.Commands.Add(item);
            dialog.Commands.Add(command2);
            this._dialogProvider.ShowDialog(dialog);
        }

    }

    private void OnConfirmDeleteLayer(LayerViewModel sender, ConfirmationDialogViewModel dialog)
    {
      if (this.Model == null || sender == null || this._dialogProvider == null)
        return;
      this.Layers.Remove(sender);
      this.Model.RemoveLayerDefinition(sender.LayerDefinition);
      this._dialogProvider.DismissDialog((IDialog) dialog);
    }

    private void OnLayerSettings(LayerViewModel sender)
    {
      this.SelectedLayer = sender;
      this.openSettingsAction();
    }

    private void RefreshCanAddLayers()
    {
      this.CanAddLayers = this.Layers.Count < LayerManagerViewModel.MaxLayers;
    }

    private void AddHandlersForAutomaticallyShowingLayerLegend(LayerViewModel layer)
    {
      if (layer == null)
        return;
      Action action1 = (Action) (() =>
      {
        if (layer.UserClosedLegend)
          return;
        this.DecoratorLayer.AddLayerLegend(layer, false, 0);
      });
      this._categoryAddedCallbacks.Add(layer, action1);
      Action<int> action2 = (Action<int>) (fieldCount =>
      {
        if (layer.UserClosedLegend)
          return;
        this.DecoratorLayer.AddLayerLegend(layer, false, 0);
      });
      this._heightFieldAddedCallbacks.Add(layer, action2);
      layer.FieldListPicker.FieldWellVisualizationViewModel.CategoryAdded += action1;
      layer.FieldListPicker.FieldWellVisualizationViewModel.HeightFieldAdded += action2;
    }

    private void RemoveHandlersForAutomaticallyShowingLayerLegend(LayerViewModel layer)
    {
      if (layer == null)
        return;
      layer.FieldListPicker.FieldWellVisualizationViewModel.CategoryAdded -= this._categoryAddedCallbacks[layer];
      layer.FieldListPicker.FieldWellVisualizationViewModel.HeightFieldAdded -= this._heightFieldAddedCallbacks[layer];
      this._categoryAddedCallbacks.Remove(layer);
      this._heightFieldAddedCallbacks.Remove(layer);
    }

    internal void SetSelectionContext(LayerViewModel layer, int colorScopeIndex)
    {
      this.SelectedLayer = layer;
      this.Settings.SetSelectedColorScopeIndex(colorScopeIndex);
    }
  }
}
