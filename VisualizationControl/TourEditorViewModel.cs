using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TourEditorViewModel : ViewModelBase
  {
    public static readonly TimeSpan MinimumTransitionDuration = new TimeSpan(0, 0, 0, 0, 0);
    public static readonly TimeSpan MaximumTransitionDuration = new TimeSpan(0, 0, 30, 0, 0);
    private object sceneSync = new object();
    private object syncSelectedItem = new object();
    public const int FrameImageWidth = 212;
    public const int FrameImageHeight = 117;
    public const int NameMaxLength = 32;
    private const int DefaultSceneDurationWithoutTimeInSeconds = 6;
    private const int DefaultSceneTransitionDurationWithoutTimeInSeconds = 3;
    private const double DefaultSceneEffectSpeed = 0.5;
    private readonly WeakEventListener<TourEditorViewModel, object, PropertyChangedEventArgs> tourPropertyChanged;
    private int changingScenesCameraStartMoves;
    private int changingScenesCameraEndMoves;
    private int changingScenesCameraTargetChange;
    private int changingScenesDecoratorChanges;
    private int changingScenesThemeChanges;
    private int changingScenesModelChanges;
    private int changingScenesFlatModeChange;
    private ICommand _CloseCommand;
    private ICommand _DuplicateSceneCommand;
    private Tour _Tour;
    private UndoObservableCollection<SceneViewModel> _SceneList;
    private UndoItemViewModel _UndoItems;
    private SceneViewModel _SelectedItem;
    private SceneSettingsViewModel settings;
    private HostControlViewModel hostControlViewModel;

    public static string PropertyCloseCommand
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
        this.SetProperty<ICommand>(TourEditorViewModel.PropertyCloseCommand, ref this._CloseCommand, value, false);
      }
    }

    public static string PropertyDuplicateSceneCommand
    {
      get
      {
        return "DuplicateSceneCommand";
      }
    }

    public ICommand DuplicateSceneCommand
    {
      get
      {
        return this._DuplicateSceneCommand;
      }
      set
      {
        this.SetProperty<ICommand>(TourEditorViewModel.PropertyDuplicateSceneCommand, ref this._DuplicateSceneCommand, value, false);
      }
    }

    public static string PropertyTour
    {
      get
      {
        return "Tour";
      }
    }

    public Tour Tour
    {
      get
      {
        return this._Tour;
      }
      set
      {
        if (this._Tour != null)
        {
          this.UpdateSelectedScene();
          if (this.hostControlViewModel.TimePlayer != null)
          {
            this.hostControlViewModel.TimePlayer.TimeController.VisualTimeEnabled = false;
            this.hostControlViewModel.TimePlayer.TimeController.Looping = false;
          }
          this.hostControlViewModel.Model.Engine.TourPlayer.Stop();
          this.hostControlViewModel.Mode = HostWindowMode.Exploration;
          this._Tour.PropertyChanged -= new PropertyChangedEventHandler(this.tourPropertyChanged.OnEvent);
          this.SelectedItem = (SceneViewModel) null;
        }
        if (!this.SetProperty<Tour>(TourEditorViewModel.PropertyTour, ref this._Tour, value, false))
          return;
        this.SceneList.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.SceneList_CollectionChanged);
        this.SceneList.Clear();
        if (this._Tour != null && this._Tour.Scenes.Count > 0)
        {
          for (int index = 0; index < this._Tour.Scenes.Count; ++index)
            this.AddSceneViewModel(this._Tour.Scenes[index], false).IsDeleteEnabled = this._Tour.Scenes.Count > 1;
          this.SetSelectedItemValue(0, true, false);
        }
        if (this._Tour != null)
          this._Tour.PropertyChanged += new PropertyChangedEventHandler(this.tourPropertyChanged.OnEvent);
        this.SceneList.CollectionChanged += new NotifyCollectionChangedEventHandler(this.SceneList_CollectionChanged);
      }
    }

    public static string PropertySceneList
    {
      get
      {
        return "SceneList";
      }
    }

    public UndoObservableCollection<SceneViewModel> SceneList
    {
      get
      {
        return this._SceneList;
      }
      private set
      {
        this.SetProperty<UndoObservableCollection<SceneViewModel>>(TourEditorViewModel.PropertySceneList, ref this._SceneList, value, false);
      }
    }

    public static string PropertyUndoItems
    {
      get
      {
        return "UndoItems";
      }
    }

    public UndoItemViewModel UndoItems
    {
      get
      {
        return this._UndoItems;
      }
      set
      {
        this.SetProperty<UndoItemViewModel>(TourEditorViewModel.PropertyUndoItems, ref this._UndoItems, value, false);
      }
    }

    public static string PropertySelectedItem
    {
      get
      {
        return "SelectedItem";
      }
    }

    public SceneViewModel SelectedItem
    {
      get
      {
        return this._SelectedItem;
      }
      set
      {
        lock (this.sceneSync)
        {
          if (this._SelectedItem != null && this._SelectedItem.IsDirty)
            this.UpdateSceneViewModel(this._SelectedItem);
          this.SetSelectedItemValue(value, true, false);
        }
      }
    }

    public DragItemsHandler<SceneViewModel> ScenesDragHandler { get; private set; }

    public DropItemsHandler ScenesDropHandler { get; private set; }

    public event Action<SceneViewModel> SceneHasChanged;

    public TourEditorViewModel(HostControlViewModel hostControlViewModel, SceneSettingsViewModel settings)
    {
      this.hostControlViewModel = hostControlViewModel;
      this.UndoItems = new UndoItemViewModel();
      this.settings = settings;
      this.SceneList = new UndoObservableCollection<SceneViewModel>((IUndoManager) this._UndoItems.UndoManager, new Func<SceneViewModel, ChangeType, string>(this.DescriptionGenerator));
      this.ScenesDragHandler = new DragItemsHandler<SceneViewModel>((Collection<SceneViewModel>) this.SceneList, false);
      this.ScenesDragHandler.CanDragItemCallback = new CanDragItemDelegate<SceneViewModel>(this.OnDragScene);
      this.ScenesDropHandler = new DropItemsHandler();
      this.ScenesDropHandler.AddDroppableTypeHandlers<SceneViewModel>(new DropItemIntoCollectionDelegate<SceneViewModel>(this.OnDropScene), (ValidateDropItemDelegate<SceneViewModel>) (item => (string) null));
      this.SceneList.CollectionChanged += new NotifyCollectionChangedEventHandler(this.SceneList_CollectionChanged);
      this.hostControlViewModel.Model.Engine.CameraMoveStarted += new Action<CameraSnapshot>(this.OnCameraMoveStarted);
      this.hostControlViewModel.Model.Engine.CameraMoveEnded += new Action<CameraSnapshot>(this.OnCameraMoveEnded);
      this.hostControlViewModel.Model.Engine.CameraTargetChanged += new Action<CameraSnapshot>(this.OnCameraTargetChanged);
      this.hostControlViewModel.Model.Engine.ThemeChanged += new Action<BuiltinTheme, VisualizationTheme, bool>(this.OnThemeChanged);
      this.hostControlViewModel.Model.Engine.FlatModeChanged += new Action<bool>(this.OnFlatModeChanged);
      this.hostControlViewModel.Model.LayerManager.StateChanged += new Action(this.OnLayerManagerStateChanged);
      this.hostControlViewModel.Model.Engine.TimeControl.PropertyChanged += new PropertyChangedEventHandler(this.TimeControl_PropertyChanged);
      this.tourPropertyChanged = new WeakEventListener<TourEditorViewModel, object, PropertyChangedEventArgs>(this);
      this.tourPropertyChanged.OnEventAction = new Action<TourEditorViewModel, object, PropertyChangedEventArgs>(TourEditorViewModel.OnTourPropertyChanged);
    }

    private void TimeControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.SelectedItem == null || this.SelectedItem.Scene == null || (this.hostControlViewModel == null || this.hostControlViewModel.Model == null) || (this.hostControlViewModel.Model.Engine == null || !e.PropertyName.Equals(this.hostControlViewModel.Model.Engine.TimeControl.PropertyDuration) || !(this.SelectedItem.Scene.Duration != this.hostControlViewModel.Model.Engine.TimeControl.Duration)))
        return;
      this.SelectedItem.Scene.Duration = this.hostControlViewModel.Model.Engine.TimeControl.Duration;
    }

    private void OnCameraMoveStarted(CameraSnapshot cameraSnapShot)
    {
      if (this.SelectedItem == null || this.SelectedItem.Scene == null || (this.SelectedItem.Scene.Frame == null || Interlocked.Exchange(ref this.changingScenesCameraStartMoves, 0) != 0))
        return;
      this.SelectedItem.IsDirty = true;
    }

    private void OnCameraMoveEnded(CameraSnapshot cameraSnapShot)
    {
      if (this.SelectedItem == null || this.SelectedItem.Scene == null || (this.SelectedItem.Scene.Frame == null || Interlocked.Exchange(ref this.changingScenesCameraEndMoves, 0) != 0))
        return;
      this.SelectedItem.Scene.Frame.Camera = cameraSnapShot;
      this.SelectedItem.IsDirty = true;
    }

    private void OnCameraTargetChanged(CameraSnapshot cameraSnapShot)
    {
      if (this.SelectedItem == null || this.SelectedItem.Scene == null || (this.SelectedItem.Scene.Frame == null || Interlocked.Exchange(ref this.changingScenesCameraTargetChange, 0) != 0))
        return;
      this.SelectedItem.IsDirty = true;
    }

    private void OnThemeChanged(BuiltinTheme themeId, VisualizationTheme theme, bool labels)
    {
      if (this.SelectedItem == null || this.SelectedItem.Scene == null || Interlocked.Exchange(ref this.changingScenesThemeChanges, 0) != 0)
        return;
      this.SelectedItem.IsDirty = true;
    }

    private void OnFlatModeChanged(bool newFlatMode)
    {
      if (this.SelectedItem == null || this.SelectedItem.Scene == null || Interlocked.Exchange(ref this.changingScenesFlatModeChange, 0) != 0)
        return;
      this.SelectedItem.IsDirty = true;
    }

    private void OnLayerManagerStateChanged()
    {
      if (this.SelectedItem == null || this.SelectedItem.Scene == null || this.changingScenesModelChanges != 0)
        return;
      this.SelectedItem.IsDirty = true;
    }

    public void UpdateDecoratorLayerImageInSelectedScene(BitmapSource decoratorLayerImage)
    {
      if (this.SelectedItem == null)
        return;
      this.SelectedItem.DecoratorImage = decoratorLayerImage;
      if (Interlocked.Decrement(ref this.changingScenesDecoratorChanges) >= 0)
        return;
      this.SelectedItem.IsDirty = true;
    }

    private static void OnTourPropertyChanged(TourEditorViewModel viewModel, object sender, PropertyChangedEventArgs e)
    {
      if (!e.PropertyName.Equals(Tour.PropertyName))
        return;
      viewModel.hostControlViewModel.SaveState();
    }

    private void OnDropScene(SceneViewModel item, int index)
    {
      this.SceneList.Insert(index, item);
      this.SceneList.CollapseRemoveAndAdd = false;
      this.SelectedItem = item;
    }

    private bool OnDragScene(SceneViewModel item)
    {
      this.SceneList.CollapseRemoveAndAdd = true;
      return true;
    }

    private string DescriptionGenerator(SceneViewModel sceneViewModel, ChangeType changeType)
    {
      switch (changeType)
      {
        case ChangeType.Add:
          return Resources.Undo_NewScene;
        case ChangeType.Remove:
          return Resources.Undo_DeleteScene;
        case ChangeType.Move:
          return Resources.Undo_MoveScene;
        default:
          return string.Empty;
      }
    }

    public void OnCaptureCleanScene(Guid customSpaceId)
    {
      this.AddCapturedScene(true, customSpaceId);
    }

    public void OnCaptureScene()
    {
      this.AddCapturedScene(false, CustomMap.InvalidMapId);
    }

    private void AddCapturedScene(bool dropExistingLayers, Guid customSpaceId)
    {
      if (this.hostControlViewModel.CaptureSceneImage == null)
        return;
      BitmapSource bitmap = this.hostControlViewModel.CaptureSceneImage(212, 117);
      if (bitmap == null)
        return;
      Scene scene = this.CreateScene(bitmap, this.hostControlViewModel.Model.Engine.GetCurrentSceneState().CameraSnapshot, dropExistingLayers);
      if (dropExistingLayers)
      {
        bool flag = customSpaceId != CustomMap.InvalidMapId;
        scene.CustomMapId = customSpaceId;
        scene.Frame.Camera = flag ? CameraSnapshot.DefaultForCustomMap() : this.hostControlViewModel.Globe.InitialCameraPosition();
        scene.FlatModeEnabled = flag;
      }
      SceneViewModel sceneViewModel = this.AddSceneViewModel(scene, true);
      if (this.SelectedItem != null)
        this.UpdateSceneViewModel(this.SelectedItem);
      this.SetSelectedItemValue(sceneViewModel, dropExistingLayers, false);
      this.hostControlViewModel.SaveState();
    }

    public void SetSelectedItemValue(int sceneIndex, bool unpackNewScene, bool openSettings)
    {
      if (sceneIndex < 0 || sceneIndex >= this.SceneList.Count)
        return;
      this.SetSelectedItemValue(this.SceneList[sceneIndex], unpackNewScene, openSettings);
    }

    public void SetSelectedItemValue(SceneViewModel sceneViewModel, bool unpackNewScene, bool openSettings)
    {
      if (this._SelectedItem == null && sceneViewModel == null || this._SelectedItem != null && this._SelectedItem.Equals((object) sceneViewModel) || !this.BeforePropertyChange<SceneViewModel>(TourEditorViewModel.PropertySelectedItem, ref this._SelectedItem, sceneViewModel))
        return;
      this.RaisePropertyChanging(TourEditorViewModel.PropertySelectedItem);
      if (sceneViewModel != null && sceneViewModel.Scene != null)
      {
        if (unpackNewScene)
        {
          Interlocked.Exchange(ref this.changingScenesDecoratorChanges, 0);
          SceneState currentSceneState = this.hostControlViewModel.Model.Engine.GetCurrentSceneState();
          if (currentSceneState == null || currentSceneState.CameraSnapshot == null || currentSceneState.CameraSnapshot.Equals(sceneViewModel.Scene.Frame.Camera))
          {
            Interlocked.Exchange(ref this.changingScenesCameraStartMoves, 1);
            Interlocked.Exchange(ref this.changingScenesCameraEndMoves, 1);
            Interlocked.Exchange(ref this.changingScenesCameraTargetChange, 1);
          }
          if (this.hostControlViewModel.Model.Engine.CurrentTheme != sceneViewModel.Scene.ThemeId || this.hostControlViewModel.Model.Engine.CurrentThemeWithLabels != sceneViewModel.Scene.ThemeWithLabel)
            Interlocked.Exchange(ref this.changingScenesThemeChanges, 1);
          if (this.hostControlViewModel.Model.Engine.FlatMode != sceneViewModel.Scene.FlatModeEnabled)
            Interlocked.Exchange(ref this.changingScenesFlatModeChange, 1);
          if (this.hostControlViewModel.LayerManagerViewModel.DecoratorLayer.Decorators.Decorators.Count > 0)
            Interlocked.Increment(ref this.changingScenesDecoratorChanges);
          this.changingScenesModelChanges = 1;
          this.UnpackSceneView(sceneViewModel);
          this.changingScenesModelChanges = 0;
          if (this.hostControlViewModel.LayerManagerViewModel.DecoratorLayer.Decorators.Decorators.Count > 0)
            Interlocked.Increment(ref this.changingScenesDecoratorChanges);
        }
        if (this._SelectedItem != null)
        {
          this._SelectedItem.IsDirty = false;
          if (this._SelectedItem.Scene != null)
            this._SelectedItem.Scene.PropertyChanged -= new PropertyChangedEventHandler(this.Scene_PropertyChanged);
        }
        this._SelectedItem = sceneViewModel;
        this.settings.ParentScene = sceneViewModel;
        if (openSettings)
          this.hostControlViewModel.TaskPanel.OpenSettings(TaskPanelSettingsSubhead.SceneSettings);
        if (this._SelectedItem.Scene != null)
        {
          this._SelectedItem.Scene.PropertyChanged += new PropertyChangedEventHandler(this.Scene_PropertyChanged);
          if (this._SelectedItem.Scene.Duration != this.hostControlViewModel.Model.Engine.TimeControl.Duration)
            this.hostControlViewModel.Model.Engine.TimeControl.Duration = this._SelectedItem.Scene.Duration;
        }
      }
      else
        this._SelectedItem = sceneViewModel;
      this.AfterPropertyChange<SceneViewModel>(TourEditorViewModel.PropertySelectedItem, ref this._SelectedItem, sceneViewModel);
      this.RaisePropertyChanged(TourEditorViewModel.PropertySelectedItem);
      if (this.SceneHasChanged == null)
        return;
      this.SceneHasChanged(sceneViewModel);
    }

    private void Scene_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      try
      {
        if (!e.PropertyName.Equals(Scene.PropertyDuration) || this.SelectedItem == null || (this.SelectedItem.Scene == null || !(this.SelectedItem.Scene.Duration != this.hostControlViewModel.Model.Engine.TimeControl.Duration)))
          return;
        this.hostControlViewModel.Model.Engine.TimeControl.Duration = this.SelectedItem.Scene.Duration;
      }
      catch (NullReferenceException ex)
      {
      }
    }

    private void UnpackSceneView(SceneViewModel sceneViewModel)
    {
      if (sceneViewModel == null || sceneViewModel.Scene == null)
        return;
      this.hostControlViewModel.Model.Engine.MoveCamera(sceneViewModel.Scene.Frame.Camera, CameraMoveStyle.JumpTo, true);
      this.hostControlViewModel.Model.Engine.TimeControl.Looping = false;
      this.hostControlViewModel.Model.Engine.TimeControl.SetVisualTimeRange(DateTime.MinValue, DateTime.MinValue, false);
      this.hostControlViewModel.Model.Engine.TimeControl.CurrentVisualTime = DateTime.MinValue;
      this.hostControlViewModel.Model.Engine.TimeControl.VisualTimeEnabled = false;
      if (sceneViewModel.Scene.CustomMapId != this.hostControlViewModel.Model.Engine.CurrentCustomMapId)
      {
        this.hostControlViewModel.Model.Engine.SetCustomMap(sceneViewModel.Scene.CustomMapId);
        this.hostControlViewModel.IsGeoMapsToolsEnabled = !sceneViewModel.Scene.HasCustomMap;
      }
      if (!string.IsNullOrEmpty(sceneViewModel.Scene.LayersContent))
        this.hostControlViewModel.Model.LayerManager.SetSceneLayersContent(sceneViewModel.Scene.LayersContent, sceneViewModel.Scene.CustomMapId, new Action<object, Exception>(this.UpdateSceneContentCallBack), (object) sceneViewModel);
      if (sceneViewModel.Scene.ThemeId != BuiltinTheme.None && (sceneViewModel.Scene.ThemeId != this.hostControlViewModel.Model.Engine.CurrentTheme || sceneViewModel.Scene.ThemeWithLabel != this.hostControlViewModel.Model.Engine.CurrentThemeWithLabels))
        this.hostControlViewModel.Model.Engine.SetTheme(sceneViewModel.Scene.ThemeId, sceneViewModel.Scene.ThemeWithLabel);
      if (sceneViewModel.Scene.FlatModeEnabled == this.hostControlViewModel.Model.Engine.FlatMode)
        return;
      this.hostControlViewModel.Model.Engine.FlatMode = sceneViewModel.Scene.FlatModeEnabled;
      this.hostControlViewModel.Model.Engine.FlatModeTransitionTime = 0.0;
    }

    private void UpdateSceneContentCallBack(object completionContext, Exception ex)
    {
      SceneViewModel sceneViewModel = completionContext as SceneViewModel;
      if (sceneViewModel == null || sceneViewModel != this.SelectedItem || !this.hostControlViewModel.Model.LayerManager.PlayToTime.HasValue)
        return;
      this.hostControlViewModel.Model.Engine.TimeControl.Duration = sceneViewModel.Scene.Duration;
    }

    private void OnDeleteScene(SceneViewModel sceneViewModel)
    {
      if (this.SceneList.Count <= 1)
        return;
      this.SceneList.Remove(sceneViewModel);
    }

    private void OnSceneSettings(SceneViewModel sceneViewModel)
    {
      this.SelectedItem = sceneViewModel;
      this.hostControlViewModel.TaskPanel.OpenSettings(TaskPanelSettingsSubhead.SceneSettings);
    }

    private void SceneList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Remove && !this.SceneList.CollapseRemoveAndAdd && (SceneViewModel) e.OldItems[0] == this.SelectedItem)
        this.SetSelectedItemValue(e.OldStartingIndex == 0 ? 0 : e.OldStartingIndex - 1, true, false);
      this.SyncTourScenes();
    }

    private void SyncTourScenes()
    {
      if (this.Tour == null)
        return;
      lock (this.sceneSync)
      {
        this.Tour.Scenes.Clear();
        for (int local_0 = 0; local_0 < this.SceneList.Count; ++local_0)
        {
          this.SceneList[local_0].IsDeleteEnabled = this.SceneList.Count > 1;
          this.SceneList[local_0].SetIndex(local_0);
          this.Tour.Scenes.Add(this.SceneList[local_0].Scene);
        }
        if (this.SelectedItem == null)
          return;
        this.settings.ParentScene = this.SelectedItem;
      }
    }

    public SceneViewModel AddSceneViewModel(Scene scene, bool insertBelowCurrunt = false)
    {
      SceneViewModel sceneViewModel = new SceneViewModel(scene, this.SceneList.Count, this.hostControlViewModel.Globe);
      sceneViewModel.DeleteCommand = (ICommand) new DelegatedCommand<SceneViewModel>(new Action<SceneViewModel>(this.OnDeleteScene));
      sceneViewModel.SettingsCommand = (ICommand) new DelegatedCommand<SceneViewModel>(new Action<SceneViewModel>(this.OnSceneSettings));
      if (insertBelowCurrunt && this.SelectedItem != null)
      {
        int num = this.SceneList.IndexOf(this.SelectedItem);
        try
        {
          this.SceneList.Insert(num + 1, sceneViewModel);
        }
        catch
        {
          this.SceneList.Add(sceneViewModel);
        }
      }
      else
        this.SceneList.Add(sceneViewModel);
      return sceneViewModel;
    }

    private void UpdateSceneViewModel(SceneViewModel sceneViewModel)
    {
      if (sceneViewModel != null)
      {
        BitmapSource bitmap = this.hostControlViewModel.CaptureSceneImage(212, 117);
        if (bitmap != null)
        {
          Scene scene = this.CreateScene(bitmap, this.hostControlViewModel.Model.Engine.GetCurrentSceneState().CameraSnapshot, false);
          if (this.SelectedItem != null)
          {
            scene.Name = this.SelectedItem.Scene.Name;
            scene.Duration = this.SelectedItem.Scene.Duration;
            scene.EffectType = this.SelectedItem.Scene.EffectType;
            scene.TransitionType = this.SelectedItem.Scene.TransitionType;
            scene.TransitionDuration = this.SelectedItem.Scene.TransitionDuration;
            scene.EffectSpeed = this.SelectedItem.Scene.EffectSpeed;
            scene.CustomMapId = this.SelectedItem.Scene.CustomMapId;
          }
          sceneViewModel.Scene = scene;
          sceneViewModel.IsDirty = false;
        }
      }
      this.SyncTourScenes();
      this.hostControlViewModel.SaveState();
    }

    public Scene CreateScene(BitmapSource bitmap, CameraSnapshot camera, bool dropLayers)
    {
      Frame frame = new Frame();
      frame.Camera = camera;
      frame.Image = bitmap;
      Scene scene = new Scene();
      scene.Name = (string) null;
      scene.TransitionType = Transition.MoveTo;
      scene.EffectType = SceneEffect.Station;
      scene.EffectSpeed = 0.5;
      DateTime? playToTime = this.hostControlViewModel.Model.LayerManager.PlayToTime;
      scene.Duration = !playToTime.HasValue ? new TimeSpan(0, 0, 6) : this.hostControlViewModel.Model.Engine.TimeControl.Duration;
      scene.TransitionDuration = new TimeSpan(0, 0, 3);
      scene.Frame = frame;
      scene.ThemeId = this.hostControlViewModel.Model.Engine.CurrentTheme;
      scene.ThemeWithLabel = this.hostControlViewModel.Model.Engine.CurrentThemeWithLabels;
      scene.FlatModeEnabled = this.hostControlViewModel.Model.Engine.FlatMode;
      scene.CustomMapId = this.hostControlViewModel.Model.Engine.CurrentCustomMapId;
      scene.LayersContent = !dropLayers ? this.hostControlViewModel.Model.LayerManager.GetSceneLayersContent() : this.hostControlViewModel.Model.LayerManager.CreateDefaultSceneLayerContent();
      return scene;
    }

    public void UpdateSelectedScene()
    {
      if (this.SelectedItem == null)
        return;
      this.UpdateSceneViewModel(this.SelectedItem);
    }
  }
}
