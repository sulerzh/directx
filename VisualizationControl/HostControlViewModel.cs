using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class HostControlViewModel : WindowViewModel, IDisposable
    {
        private bool _RefreshDataEnabled = true;
        private bool _IsGeoMapsToolsEnabled = true;
        private bool saveBeforeClose = true;
        private long lastFrame = -1L;
        private System.Timers.Timer tooltipShowTimer = new System.Timers.Timer(333.0);
        private TourEditorViewModel _TourEditor;
        private bool _TourEditorVisible;
        private ChartShape _ChartShape;
        private HostWindowMode _Mode;
        private object _TempToolTipContent;
        private object _ToolTipContent;
        private bool _ToolTipVisible;
        private bool _RibbonMinimized;
        private bool _RibbonCollapsed;
        private AnnotationDialogContentViewModel _addAnnotationDialog;
        private Window findLocationDialogWindow;
        private CustomSpaceSettingsDialogView customSpaceSettingsDialogWindow;
        private SceneSettingsViewModel sceneSettingsVM;
        private bool disposed;
        private IHelpViewer helpViewer;
        private long ticksAtPreviousRefresh;

        public List<Color4F> CustomColors { get; private set; }

        public ICommand TourEditorOpenCommand { get; private set; }

        public ICommand PlayTourCommand { get; private set; }

        public ICommand AddLegendCommand { get; private set; }

        public ICommand AddLabelCommand { get; private set; }

        public ICommand AddChartCommand { get; private set; }

        public ICommand FindLocationCommand { get; internal set; }

        public ICommand EasyDebugCommand { get; internal set; }

        public ICommand CustomSpaceCommand { get; internal set; }

        public ICommand UpdateMapTypesCommand { get; internal set; }

        public ICommand RefreshDataCommand { get; internal set; }

        public ICommand CaptureSceneCommand { get; private set; }

        public ICommand CaptureCleanSceneCommand { get; private set; }

        public ICommand SendASmileCommand { get; private set; }

        public ICommand TimeDecoratorCommand { get; private set; }

        public ICommand CreateVideoCommand { get; private set; }

        public ICommand OpenSettingsDialogCommand { get; private set; }

        public ICommand ShowSceneSettings { get; private set; }

        public ICommand ShowLayerSettings { get; private set; }

        public ICommand ShowLayerFields { get; private set; }

        public ObservableCollectionEx<ContextCommand> ContextCommands { get; private set; }

        public StatusBarViewModel StatusBar { get; private set; }

        public static string PropertyTourEditor
        {
            get
            {
                return "TourEditor";
            }
        }

        public TourEditorViewModel TourEditor
        {
            get
            {
                return this._TourEditor;
            }
            set
            {
                this.SetProperty<TourEditorViewModel>(HostControlViewModel.PropertyTourEditor, ref this._TourEditor, value, false);
            }
        }

        public static string PropertyTourEditorVisible
        {
            get
            {
                return "TourEditorVisible";
            }
        }

        public bool TourEditorVisible
        {
            get
            {
                return this._TourEditorVisible;
            }
            set
            {
                this.SetProperty<bool>(HostControlViewModel.PropertyTourEditorVisible, ref this._TourEditorVisible, value, false);
            }
        }

        public static string PropertyRefreshDataEnabled
        {
            get
            {
                return "RefreshDataEnabled";
            }
        }

        public bool RefreshDataEnabled
        {
            get
            {
                return this._RefreshDataEnabled;
            }
            set
            {
                this.SetProperty<bool>(HostControlViewModel.PropertyRefreshDataEnabled, ref this._RefreshDataEnabled, value, false);
            }
        }

        public string PropertyChartShape
        {
            get
            {
                return "ChartShape";
            }
        }

        public ChartShape ChartShape
        {
            get
            {
                return this._ChartShape;
            }
            set
            {
                base.SetProperty<ChartShape>(this.PropertyChartShape, ref this._ChartShape, value, (Action)(() =>
                {
                    if (value == null)
                        return;
                    this.ShapesGallery.ApplyShapeCommand.Execute((object)value.Shape);
                }));
            }
        }

        public string PropertyFlatMapEnabled
        {
            get
            {
                return "FlatMapEnabled";
            }
        }

        public bool FlatMapEnabled
        {
            get
            {
                if (this.Model.Engine.FlatMode)
                    return this.IsGeoMapsToolsEnabled;
                else
                    return false;
            }
            set
            {
                if (!this.IsGeoMapsToolsEnabled)
                    return;
                this.Model.Engine.FlatMode = value;
            }
        }

        private string PropertyGeoMapsToolsEnabled
        {
            get
            {
                return "IsGeoMapsToolsEnabled";
            }
        }

        public bool IsGeoMapsToolsEnabled
        {
            get
            {
                return this._IsGeoMapsToolsEnabled;
            }
            set
            {
                bool flatMapEnabled = this.FlatMapEnabled;
                this.SetProperty<bool>(this.PropertyGeoMapsToolsEnabled, ref this._IsGeoMapsToolsEnabled, value, false);
                if (flatMapEnabled == this.FlatMapEnabled)
                    return;
                this.RaisePropertyChanged(this.PropertyFlatMapEnabled);
            }
        }

        public static string PropertyMode
        {
            get
            {
                return "Mode";
            }
        }

        public HostWindowMode Mode
        {
            get
            {
                return this._Mode;
            }
            set
            {
                base.SetProperty<HostWindowMode>(HostControlViewModel.PropertyMode, ref this._Mode, value, (Action)(() =>
                {
                    this.ChromeBarVisible = value == HostWindowMode.Exploration;
                    this.TakeEntireScreenInNextFullScreen = value == HostWindowMode.Playback;
                    this.FullScreenMode = value == HostWindowMode.Playback;
                }));
            }
        }

        public static string PropertyToolTipContent
        {
            get
            {
                return "ToolTipContent";
            }
        }

        public object ToolTipContent
        {
            get
            {
                return this._ToolTipContent;
            }
            set
            {
                this.tooltipShowTimer.Enabled = false;
                this._TempToolTipContent = value;
            }
        }

        public static string PropertyToolTipVisible
        {
            get
            {
                return "ToolTipVisible";
            }
        }

        public bool ToolTipVisible
        {
            get
            {
                return this._ToolTipVisible;
            }
            set
            {
                if (!value)
                {
                    this.tooltipShowTimer.Enabled = false;
                    this.SetProperty<bool>(HostControlViewModel.PropertyToolTipVisible, ref this._ToolTipVisible, value, false);
                }
                else
                    this.tooltipShowTimer.Enabled = true;
            }
        }

        public string PropertyRibbonMinimized
        {
            get
            {
                return "RibbonMinimized";
            }
        }

        public bool RibbonMinimized
        {
            get
            {
                return this._RibbonMinimized;
            }
            set
            {
                this.SetProperty<bool>(this.PropertyRibbonMinimized, ref this._RibbonMinimized, value, false);
            }
        }

        public string PropertyRibbonCollapsed
        {
            get
            {
                return "RibbonCollapsed";
            }
        }

        public bool RibbonCollapsed
        {
            get
            {
                return this._RibbonCollapsed;
            }
            set
            {
                this.SetProperty<bool>(this.PropertyRibbonCollapsed, ref this._RibbonCollapsed, value, false);
            }
        }

        public DropItemHandler GlobeDropHandler { get; private set; }

        public TaskPanelViewModel TaskPanel { get; private set; }

        public TimeScrubberViewModel TimePlayer { get; private set; }

        public GlobeViewModel Globe { get; private set; }

        public GlobeNavigationViewModel GlobeNavigation { get; private set; }

        public ThemeGalleryViewModel ThemeGallery { get; private set; }

        public CustomSpaceGalleryViewModel MapTypesGalleryViewModel { get; private set; }

        public TourPlayerViewModel TourPlayer { get; private set; }

        public ShapesGalleryViewModel ShapesGallery { get; private set; }

        public FindLocationViewModel FindLocationViewModel { get; private set; }

        public SettingsDialogViewModel SettingsDialog { get; private set; }

        public Func<int, int, BitmapSource> CaptureSceneImage { get; set; }

        public LayerManagerViewModel LayerManagerViewModel { get; private set; }

        public VisualizationModel Model { get; private set; }

        internal BingMapResourceUri BingMapResourceUri { get; private set; }

        private SelectedElementHelper PrimarySelectedElement { get; set; }

        private SelectedElementHelper HoveredElement { get; set; }

        public HostControlViewModel(VisualizationModel model, IHelpViewer helpViewer = null, BingMapResourceUri mapResourceUri = null, List<Color4F> customColors = null)
        {
            this.BingMapResourceUri = mapResourceUri ?? new BingMapResourceUri();
            this.CustomColors = customColors;
            this.Model = model;
            this.helpViewer = helpViewer;
            this.MinWidth = 320.0;
            this.MinHeight = 364.0;
            this.Width = 1024.0;
            this.Height = 720.0;
            this.HelpCommand = (ICommand)new DelegatedCommand(new Action(this.OnExecuteHelpCommand));
            this.FullScreenCommand = (ICommand)new DelegatedCommand(new Action(this.OnExecuteFullScreen));
            this.FindLocationCommand = (ICommand)new DelegatedCommand(new Action(this.OnExecuteFindLocation));
            this.EasyDebugCommand = (ICommand)new DelegatedCommand((Action)(() => VisualizationTraceSource.Current.Fail("TODO: attach debugger now...")));
            this.RefreshDataCommand = (ICommand)new DelegatedCommand(new Action(this.OnExecuteRefreshData));
            this.TimeDecoratorCommand = (ICommand)new DelegatedCommand((Action)(() =>
            {
                if (this.TourEditor.SelectedItem == null)
                    return;
                this.TourEditor.SelectedItem.IsDirty = true;
            }));
            this.GlobeDropHandler = new DropItemHandler();
            this.GlobeDropHandler.AddDroppableTypeHandlers<TableFieldViewModel>((Action<TableFieldViewModel>)(item => item.IsSelected = true), (ValidateDropItemDelegate<TableFieldViewModel>)null);
            this.GlobeDropHandler.AddDroppableTypeHandlers<FieldWellHeightViewModel>((Action<FieldWellHeightViewModel>)(item => item.RemoveCallback(item)), (ValidateDropItemDelegate<FieldWellHeightViewModel>)null);
            this.GlobeDropHandler.AddDroppableTypeHandlers<FieldWellCategoryViewModel>((Action<FieldWellCategoryViewModel>)(item => item.RemoveCallback(item)), (ValidateDropItemDelegate<FieldWellCategoryViewModel>)null);
            this.GlobeDropHandler.AddDroppableTypeHandlers<FieldWellTimeViewModel>((Action<FieldWellTimeViewModel>)(item => item.RemoveCallback(item)), (ValidateDropItemDelegate<FieldWellTimeViewModel>)null);
            if (this.Model == null)
                return;
            this.Globe = new GlobeViewModel(model, mapResourceUri);
            this.GlobeNavigation = new GlobeNavigationViewModel(model.Engine);
            this.LayerManagerViewModel = new LayerManagerViewModel(model, this, this.CustomColors, (IDialogServiceProvider)this);
            this.TimePlayer = new TimeScrubberViewModel(this, this.Model.Engine.TimeControl, this.LayerManagerViewModel, (Action)(() => this.TaskPanel.OpenSettings(TaskPanelSettingsSubhead.SceneSettings)));
            SceneSettingsViewModel sceneSettingsViewModel = new SceneSettingsViewModel(this, new TimeSettingsViewModel(model.Engine.TimeControl, this.LayerManagerViewModel.Model), this.TimePlayer);
            this.sceneSettingsVM = sceneSettingsViewModel;
            this.TourEditor = new TourEditorViewModel(this, sceneSettingsViewModel);
            this.TourEditor.CloseCommand = (ICommand)new DelegatedCommand(new Action(this.OnTourEditorClose));
            this.TourEditor.DuplicateSceneCommand = (ICommand)new DelegatedCommand(new Action(this.OnCaptureScene));
            this.TourEditor.SceneHasChanged += (Action<SceneViewModel>)(svm =>
            {
                this.CloseDialogPopups();
                if (svm == null || svm.Scene == null)
                    return;
                this.IsGeoMapsToolsEnabled = !svm.Scene.HasCustomMap;
            });
            this.PlayTourCommand = (ICommand)new DelegatedCommand(new Action(this.OnExecutePlayTour), new Predicate(this.CanExecutePlayTour));
            this.CaptureSceneCommand = (ICommand)new DelegatedCommand(new Action(this.OnCaptureScene));
            this.CaptureCleanSceneCommand = (ICommand)new DelegatedCommand(new Action(this.OnCaptureCleanScene));
            this.CreateVideoCommand = (ICommand)new DelegatedCommand(new Action(this.OnStartVideoCapture));
            this.OpenSettingsDialogCommand = (ICommand)new DelegatedCommand(new Action(this.OnOpenSettingsDialog));
            this.ShowSceneSettings = (ICommand)new DelegatedCommand(new Action(this.OnShowSceneSettings));
            this.ShowLayerSettings = (ICommand)new DelegatedCommand(new Action(this.OnShowLayerSettings));
            this.ShowLayerFields = (ICommand)new DelegatedCommand(new Action(this.OnShowLayerFields));
            this.StatusBar = new StatusBarViewModel((ILayerManagerViewModel)this.LayerManagerViewModel, this);
            this.StatusBar.SelectionStats = model.LayerManager.SelectionStats;
            this.StatusBar.GlobeNavigation = this.GlobeNavigation;
            this.TaskPanel = new TaskPanelViewModel((ILayerManagerViewModel)this.LayerManagerViewModel, sceneSettingsViewModel);
            this.TaskPanel.FieldsTab.Model.PropertyChanging += new PropertyChangingEventHandler(this.OnFieldsTabPropertyChanging);
            this.TaskPanel.FieldsTab.Model.PropertyChanged += new PropertyChangedEventHandler(this.OnFieldsTabPropertyChanged);
            this.ContextCommands = new ObservableCollectionEx<ContextCommand>();
            ContextCommand contextCommand1 = new ContextCommand(Resources.Context_AddAnnotation, (ICommand)new DelegatedCommand(new Action(this.OnExecuteAddAnnotation), new Predicate(this.CanExecuteAddAnnotation)));
            ContextCommand contextCommand2 = new ContextCommand(Resources.Context_EditAnnotation, (ICommand)new DelegatedCommand(new Action(this.OnExecuteEditAnnotation), new Predicate(this.CanExecuteEditAnnotation)));
            ContextCommand contextCommand3 = new ContextCommand(Resources.Context_RemoveAnnotation, (ICommand)new DelegatedCommand(new Action(this.OnExecuteRemoveAnnotation), new Predicate(this.CanExecuteRemoveAnnotation)));
            ContextCommand contextCommand4 = new ContextCommand(Resources.SceneSettings_ContextMenu_ChangeBackground, (ICommand)new DelegatedCommand(new Action(this.ShowSceneGalleryDialog)));
            this.ContextCommands.Add(contextCommand1);
            this.ContextCommands.Add(contextCommand2);
            this.ContextCommands.Add(contextCommand3);
            this.ContextCommands.Add(this.LayerManagerViewModel.DecoratorLayer.AddLabelCommand);
            this.ContextCommands.Add(this.LayerManagerViewModel.DecoratorLayer.AddLegendCommand);
            this.ContextCommands.Add(this.LayerManagerViewModel.DecoratorLayer.AddCurrentLayerLegendCommand);
            this.ContextCommands.Add(this.LayerManagerViewModel.DecoratorLayer.AddChartCommand);
            this.ContextCommands.Add(contextCommand4);
            this.LayerManagerViewModel.Layers.ItemPropertyChanged += new ObservableCollectionExItemChangedHandler<LayerViewModel>(this.LayerPropertyChanged);
            this.LayerManagerViewModel.DecoratorLayer.DecoratorImageWidth = 212;
            this.LayerManagerViewModel.DecoratorLayer.DecoratorImageHeight = 117;
            this.LayerManagerViewModel.DecoratorLayer.PropertyChanged += new PropertyChangedEventHandler(this.DecoratorLayerChanged);
            this.AddLegendCommand = this.LayerManagerViewModel.DecoratorLayer.AddLegendCommand.Command;
            this.AddLabelCommand = this.LayerManagerViewModel.DecoratorLayer.AddLabelCommand.Command;
            this.AddChartCommand = this.LayerManagerViewModel.DecoratorLayer.AddChartCommand.Command;
            this.FindLocationViewModel = new FindLocationViewModel(this.Model.LatLonProvider, this.Model.Engine);
            this.SettingsDialog = new SettingsDialogViewModel();
            this.TourPlayer = new TourPlayerViewModel(this.Model.Engine.TourPlayer)
            {
                ExitTourPlaybackModeCommand = (ICommand)new DelegatedCommand(new Action(this.OnExitTourPlaybackMode))
            };
            this.UpdateMapTypesCommand = (ICommand)new DelegatedCommand((Action)(() => this.MapTypesGalleryViewModel.SetCurrentScene(sceneSettingsViewModel.ParentScene.Scene)));
            this.CustomSpaceCommand = (ICommand)new DelegatedCommand((Action)(() => this.ShowSceneGalleryDialog()));
            this.ThemeGallery = new ThemeGalleryViewModel(model.Engine, this.BingMapResourceUri);
            this.ShapesGallery = new ShapesGalleryViewModel(this.TaskPanel.FieldsTab);
            this.MapTypesGalleryViewModel = new CustomSpaceGalleryViewModel(this, (Scene)null, new CustomSpaceGalleryViewModel.CustomCreationDelegate(this.OnCaptureCleanSceneTyped));
            this.Model.Engine.FlatModeChanged += (Action<bool>)(b => this.RaisePropertyChanged(this.PropertyFlatMapEnabled));
            this.Model.Engine.TourSceneStateChanged += new TourSceneStateChangeHandler(this.Engine_TourSceneStateChanged);
            Action action = delegate
            {
                if (this.SetProperty<object>(HostControlViewModel.PropertyToolTipContent, ref this._ToolTipContent,
                    this._TempToolTipContent, false))
                    this.SetProperty<bool>(HostControlViewModel.PropertyToolTipVisible, ref this._ToolTipVisible, true,
                        false);
                this.tooltipShowTimer.Enabled = false;
            };
            this.tooltipShowTimer.Elapsed += (ElapsedEventHandler)((s, e) => this.Model.UIDispatcher.Invoke(action, new object[0]));
            this.SendASmileCommand = (ICommand)new DelegatedCommand(new Action(this.OnSendASmile));
            this.UndoCommand = (ICommand)new DelegatedCommand<object>((Action<object>)(context => this.TourEditor.UndoItems.UndoCommand.Execute(context)), (Predicate<object>)(context => this.TourEditor.UndoItems.UndoCommand.CanExecute(context)));
            this.RedoCommand = (ICommand)new DelegatedCommand<object>((Action<object>)(context => this.TourEditor.UndoItems.RedoCommand.Execute(context)), (Predicate<object>)(context => this.TourEditor.UndoItems.RedoCommand.CanExecute(context)));
            this.AppIconVisible = true;
            this.UndoRedoVisible = true;
        }

        private void Engine_TourSceneStateChanged(object sender, TourSceneStateChangedEventArgs eventArgs)
        {
            long ticks = DateTime.UtcNow.Ticks;
            if (!this.TourPlayer.IsRefreshEnabled || eventArgs.LoopCount == 0)
            {
                this.ticksAtPreviousRefresh = ticks;
            }
            else
            {
                if (eventArgs.SceneIndex != 0 || eventArgs.TourSceneState != TourSceneState.NotStarted || ticks - this.ticksAtPreviousRefresh < TimeSpan.FromMinutes((double)this.TourPlayer.RefreshIntervalInMinutes).Ticks)
                    return;
                VisualizationTraceSource.Current.TraceInformation("Invoking Excel refresh before start of tour playback loop # {0}.", (object)eventArgs.LoopCount);
                try
                {
                    this.Model.RefreshAll();
                    this.ticksAtPreviousRefresh = ticks;
                }
                catch (COMException ex)
                {
                    VisualizationTraceSource.Current.Fail("Failed on execution of Excel refresh for tour playback loop # {0}.", (Exception)ex);
                }
            }
        }

        private void OnStartVideoCapture()
        {
            Dispatcher dispatcher = Dispatcher.FromThread(this.Model.TwoDRenderThread);
            VisualizationModel model = (VisualizationModel)null;
            this.TourEditor.UpdateSelectedScene();
            dispatcher.Invoke((Action)(() =>
            {
                model = this.Model.Clone();
                model.SetCurrentTour(this.Model.CurrentTour);
            }));
            this.ShowDialog((IDialog)new CreateVideoViewModel(model, dispatcher, this)
            {
                DefaultFileName = this.Model.CurrentTour.Name
            });
        }

        private void OnOpenSettingsDialog()
        {
            if (this.SettingsDialog == null)
                return;
            this.ShowDialog((IDialog)this.SettingsDialog);
        }

        private void OnShowSceneSettings()
        {
            this.TaskPanel.Visible = true;
            this.TaskPanel.OpenSettings(TaskPanelSettingsSubhead.SceneSettings);
        }

        private void OnShowLayerSettings()
        {
            this.TaskPanel.Visible = true;
            this.TaskPanel.OpenSettings(TaskPanelSettingsSubhead.LayerSettings);
        }

        private void OnShowLayerFields()
        {
            this.TaskPanel.Visible = true;
            this.TaskPanel.SelectedIndex = TaskPanelViewModel.IndexOfFieldsTab;
        }

        private void OnCaptureScene()
        {
            this.TourEditorVisible = true;
            this.TourEditor.OnCaptureScene();
        }

        private void OnCaptureCleanSceneTyped(bool isDuplicate, CustomMap userSelectedMap, bool isUserSelectedANewMap)
        {
            this.MapTypesGalleryViewModel.SetCurrentScene((Scene)null);
            this.TourEditorVisible = true;
            this.TaskPanel.Visible = true;
            this.TaskPanel.SelectedIndex = TaskPanelViewModel.IndexOfFieldsTab;
            if (isDuplicate)
            {
                this.TourEditor.OnCaptureScene();
            }
            else
            {
                this.TourEditor.OnCaptureCleanScene(userSelectedMap != null ? userSelectedMap.UniqueCustomMapId : CustomMap.InvalidMapId);
                this.TaskPanel.LayersTab.AddNewLayerCommand.Execute((object)this);
                if (!isUserSelectedANewMap || userSelectedMap == null)
                    return;
                this.ShowSceneBackgroundSettingsDialog(userSelectedMap);
            }
        }

        private void OnCaptureCleanScene()
        {
            bool isDuplicate = false;
            bool isUserSelectedMap = false;
            bool isUserSelectedANewMap = false;
            CustomMap userSelectedMap = (CustomMap)null;
            CustomSpaceGalleryViewModel galleryViewModel = new CustomSpaceGalleryViewModel(this, this.TourEditor.SelectedItem.Scene, (CustomSpaceGalleryViewModel.CustomCreationDelegate)((isDupe, customMap, isNewMap) =>
            {
                isUserSelectedMap = true;
                isDuplicate = isDupe;
                userSelectedMap = customMap;
                isUserSelectedANewMap = isNewMap;
            }));
            galleryViewModel.GalleryClosing += (Action)(() =>
            {
                if (!isUserSelectedMap)
                    return;
                this.OnCaptureCleanSceneTyped(isDuplicate, userSelectedMap, isUserSelectedANewMap);
            });
            this.ShowDialog((IDialog)galleryViewModel);
        }

        private void DecoratorLayerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == DecoratorLayerViewModel.PropertyDecoratorImage))
                return;
            this.TourEditor.UpdateDecoratorLayerImageInSelectedScene(this.LayerManagerViewModel.DecoratorLayer.DecoratorImage);
        }

        private void OnSendASmile()
        {
            try
            {
                Process.Start("mailto:GeoFlowFeedback@Microsoft.com?body=" + HttpUtility.UrlPathEncode(Resources.Product + " - " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion + (Environment.Is64BitProcess ? ".64" : ".32")));
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Can't Send a smile, exception: {0}", (object)ex);
            }
        }

        private void ProcessPendingDispatcherOps()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Action action = delegate
            {
                frame.Continue = false;
            };
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, action);
            Dispatcher.PushFrame(frame);
        }

        private void OnExecuteAddAnnotation()
        {
            this.ProcessPendingDispatcherOps();
            if (this.PrimarySelectedElement == null)
                return;
            List<Tuple<AggregationFunction?, string, object>> columns = this.PrimarySelectedElement.GetColumns();
            bool displayAnyMeasure = this.PrimarySelectedElement.GetDisplayAnyMeasure();
            this._addAnnotationDialog = new AnnotationDialogContentViewModel((AnnotationTemplateModel)null)
            {
                SelectedData = new DataRowModel(columns, displayAnyMeasure),
                PrimarySelectedElement = this.PrimarySelectedElement,
                InstructionText = Resources.AnnotationDialog_AddAnnotation
            };
            this._addAnnotationDialog.CreateCommand = (ICommand)new DelegatedCommand(new Action(this.OnAddAnnotationOkay), new Predicate(this._addAnnotationDialog.CanExecuteCreateCommand));
            this.ShowDialog((IDialog)this._addAnnotationDialog);
        }

        private void OnAddAnnotationOkay()
        {
            this._addAnnotationDialog.PrimarySelectedElement.AddOrUpdateAnnotationForSelectedElements(this._addAnnotationDialog.Model);
            this.DismissDialog((IDialog)this._addAnnotationDialog);
        }

        private void OnExecuteEditAnnotation()
        {
            this.ProcessPendingDispatcherOps();
            if (this.PrimarySelectedElement == null)
                return;
            List<Tuple<AggregationFunction?, string, object>> columns = this.PrimarySelectedElement.GetColumns();
            bool displayAnyMeasure = this.PrimarySelectedElement.GetDisplayAnyMeasure();
            this._addAnnotationDialog = new AnnotationDialogContentViewModel(this.PrimarySelectedElement.GetAnnotationForSelectedElements())
            {
                SelectedData = new DataRowModel(columns, displayAnyMeasure),
                PrimarySelectedElement = this.PrimarySelectedElement,
                InstructionText = Resources.AnnotationDialog_EditAnnotation
            };
            this._addAnnotationDialog.CreateCommand = (ICommand)new DelegatedCommand(new Action(this.OnAddAnnotationOkay), new Predicate(this._addAnnotationDialog.CanExecuteCreateCommand));
            this.ShowDialog((IDialog)this._addAnnotationDialog);
        }

        private void OnExecuteRemoveAnnotation()
        {
            this.ProcessPendingDispatcherOps();
            if (this.PrimarySelectedElement == null)
                return;
            this.PrimarySelectedElement.DeleteAnnotationForSelectedElements();
        }

        private bool CanExecuteAddAnnotation()
        {
            if (this.PrimarySelectedElement != null)
                return !this.PrimarySelectedElement.IsSelectedElementAnnotated();
            else
                return false;
        }

        private bool CanExecuteEditAnnotation()
        {
            if (this.PrimarySelectedElement != null)
                return this.PrimarySelectedElement.IsSelectedElementAnnotated();
            else
                return false;
        }

        private bool CanExecuteRemoveAnnotation()
        {
            return this.CanExecuteEditAnnotation();
        }

        private void LayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            LayerViewModel sender1 = sender as LayerViewModel;
            if (sender1 == null)
                return;
            if (e.PropertyName == LayerViewModel.PropertyHoveredElement)
            {
                this.OnLayerHoveredElementChanged(sender1);
            }
            else
            {
                if (!(e.PropertyName == LayerViewModel.PropertySelectedElements))
                    return;
                this.OnLayerSelectedElementChanged(sender1);
            }
        }

        private void OnLayerHoveredElementChanged(LayerViewModel sender)
        {
            if (this.Dialog != null)
                return;
            InstanceId? nullable = sender.HoveredElement.Item2;
            if (this.lastFrame == sender.HoveredElement.Item1 && !nullable.HasValue)
                return;
            this.lastFrame = sender.HoveredElement.Item1;
            this.ToolTipVisible = false;
            this.HoveredElement = (SelectedElementHelper)null;
            if (!nullable.HasValue || this.TourPlayer.TourPlayer.IsPlaying)
                return;
            GeoVisualization geoVisualization = sender.LayerDefinition.GeoVisualization;
            this.ToolTipContent = (object)new TableFieldToolTipViewModel(geoVisualization, nullable.Value, geoVisualization != null && geoVisualization.VisualType == LayerType.RegionChart);
            this.ToolTipVisible = true;
            this.HoveredElement = new SelectedElementHelper()
            {
                Layer = sender,
                DataInstance = nullable.Value
            };
        }

        private void OnLayerSelectedElementChanged(LayerViewModel sender)
        {
            if (this.Dialog != null)
                return;
            InstanceId? nullable = sender.SelectedElements == null || sender.SelectedElements.Count <= 0 ? new InstanceId?() : new InstanceId?(sender.SelectedElements[sender.SelectedElements.Count - 1]);
            if (this.PrimarySelectedElement != null && this.PrimarySelectedElement.Layer == sender)
                this.PrimarySelectedElement = (SelectedElementHelper)null;
            if (!nullable.HasValue || this.TourPlayer.TourPlayer.IsPlaying)
                return;
            GeoVisualization geoVisualization = sender.LayerDefinition.GeoVisualization;
            this.PrimarySelectedElement = new SelectedElementHelper()
            {
                Layer = sender,
                DataInstance = nullable.Value
            };
        }

        private void OnTourEditorClose()
        {
            this.TourEditorVisible = false;
        }

        private void OnExitTourPlaybackMode()
        {
            int currentSceneIndex = this.Model.Engine.TourPlayer.CurrentSceneIndex;
            this.Model.Engine.TourPlayer.Stop();
            this.Mode = HostWindowMode.Exploration;
            this.Globe.IsLogoVisible = true;
            this.TourPlayer.OptionsVisible = false;
            this.TourEditor.SetSelectedItemValue(currentSceneIndex, true, false);
            int num = (int)Win32Helper.SetThreadExecutionState(Win32Helper.ExecutionState.ES_CONTINUOUS);
        }

        public void OnExecutePlayTour()
        {
            if (this.Model.CurrentTour == null)
                return;
            this.CloseFindLocationWindow();
            this.TourEditor.SelectedItem = (SceneViewModel)null;
            this.Globe.IsLogoVisible = false;
            this.Mode = HostWindowMode.Playback;
            this.Model.Engine.TourPlayer.SetTour(this.Model.CurrentTour, (ITourLayerManager)this.Model.LayerManager, this.Model.Engine.TimeControl);
            this.Model.Engine.TourPlayer.Play();
            int num = (int)Win32Helper.SetThreadExecutionState(Win32Helper.ExecutionState.ES_AWAYMODE_REQUIRED | Win32Helper.ExecutionState.ES_CONTINUOUS | Win32Helper.ExecutionState.ES_DISPLAY_REQUIRED | Win32Helper.ExecutionState.ES_SYSTEM_REQUIRED);
        }

        private bool CanExecutePlayTour()
        {
            return !this.TourEditor.SceneList.IsEmpty;
        }

        public void CreateNewTour(string tourName)
        {
            this.SetTour(new Tour()
            {
                Name = tourName,
                Description = Resources.DefaultTourDescription
            });
            this.Model.Reset();
            this.Globe.Reset();
            SceneViewModel sceneViewModel = this.TourEditor.AddSceneViewModel(this.TourEditor.CreateScene((BitmapSource)null, this.Model.Engine.GetCurrentSceneState().CameraSnapshot, false), false);
            sceneViewModel.IsDirty = true;
            this.TourEditor.SetSelectedItemValue(sceneViewModel, false, false);
            this.SaveState();
            this.TourEditor.UndoItems.UndoManager.Reset();
        }

        public void ResetUndoStack()
        {
            this.TourEditor.UndoItems.UndoManager.Reset();
        }

        public void SetTour(Tour tour)
        {
            this.Model.SetCurrentTour(tour);
            this.TourEditor.Tour = tour;
            this.TourEditorVisible = true;
            this.TaskPanel.SelectedIndex = TaskPanelViewModel.IndexOfFieldsTab;
            this.TourEditor.UndoItems.UndoManager.Reset();
        }

        public void SaveState()
        {
            this.Model.TourPersist.PersistTour(this.Model.CurrentTour);
        }

        private void OnExecuteHelpCommand()
        {
            if (this.helpViewer == null)
                return;
            this.helpViewer.ShowHelp(0);
        }

        protected override void OnExecuteEscape()
        {
            if (this.Mode != HostWindowMode.Playback)
                return;
            this.OnExitTourPlaybackMode();
        }

        private void OnExecuteFullScreen()
        {
            if (this.Window == null)
                return;
            if (this.Window.WindowState == WindowState.Maximized)
            {
                this.RibbonMinimized = false;
                this.RibbonCollapsed = !this.RibbonCollapsed;
            }
            else
            {
                this.RibbonCollapsed = false;
                this.RibbonMinimized = !this.RibbonMinimized;
            }
        }

        public void OnExecuteRefreshData()
        {
            base.BusyOperation(delegate
            {
                try
                {
                    this.RefreshDataEnabled = false;
                    if (this.Model.ConnectionsDisabled)
                    {
                        ConfirmationDialogViewModel dialog = new ConfirmationDialogViewModel
                        {
                            Title = Resources.Product,
                            Description = Resources.Refresh_Error_ConnectionsDisabled
                        };
                        DelegatedCommand item = new DelegatedCommand(() => this.DismissDialog(dialog))
                        {
                            Name = Resources.Dialog_OkayText
                        };
                        dialog.Commands.Add(item);
                        base.ShowDialog(dialog);
                    }
                    else
                    {
                        this.Model.RefreshAll();
                    }
                }
                catch (COMException exception)
                {
                    string str;
                    if (exception.ErrorCode == -2146777998)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Error 0x800AC472 calling Workbook.RefreshAll() - dialog box displayed in Excel");
                        str = Resources.Refresh_Error_RetryAfterCloseExcelDialog;
                    }
                    else
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Error calling Workbook.RefreshAll(), ignoring: {0}", new object[] { exception });
                        str = string.Format(Resources.Refresh_Error_Retry, exception.Message);
                    }
                    ConfirmationDialogViewModel dialog = new ConfirmationDialogViewModel
                    {
                        Title = Resources.Product,
                        Description = str
                    };
                    DelegatedCommand command2 = new DelegatedCommand(() => this.DismissDialog(dialog))
                    {
                        Name = Resources.Dialog_OkayText
                    };
                    dialog.Commands.Add(command2);
                    base.ShowDialog(dialog);
                }
                finally
                {
                    this.RefreshDataEnabled = true;
                }
            });
        }

        public void ShowSceneGalleryDialog()
        {
            if (this.sceneSettingsVM == null)
                return;
            this.ShowDialog((IDialog)new CustomSpaceGalleryViewModel(this, this.sceneSettingsVM.ParentScene.Scene, (CustomSpaceGalleryViewModel.CustomCreationDelegate)null));
        }

        public void ShowSceneBackgroundSettingsDialog(CustomMap cm)
        {
            if (this.customSpaceSettingsDialogWindow == null)
            {
                CustomSpaceSettingsViewModel settingsViewModel = new CustomSpaceSettingsViewModel(this, cm);
                CustomSpaceSettingsDialogView settingsDialogView = new CustomSpaceSettingsDialogView();
                settingsDialogView.DataContext = (object)settingsViewModel;
                settingsDialogView.Title = Resources.CustomSpaceSettings_DialogTitle;
                settingsDialogView.Owner = this.Window;
                this.customSpaceSettingsDialogWindow = settingsDialogView;
                this.customSpaceSettingsDialogWindow.FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                this._childWindows.Add((Window)this.customSpaceSettingsDialogWindow);
                this.customSpaceSettingsDialogWindow.Closed += (EventHandler)((s, e) =>
                {
                    this._childWindows.Remove((Window)this.customSpaceSettingsDialogWindow);
                    this.customSpaceSettingsDialogWindow = (CustomSpaceSettingsDialogView)null;
                });
            }
            this.customSpaceSettingsDialogWindow.Show();
            this.customSpaceSettingsDialogWindow.Activate();
        }

        private void OnExecuteFindLocation()
        {
            if (this.findLocationDialogWindow == null)
            {
                FindLocationDialogView locationDialogView = new FindLocationDialogView();
                locationDialogView.DataContext = (object)this.FindLocationViewModel;
                locationDialogView.Title = Resources.FindLocationDialog_Title;
                locationDialogView.Owner = this.Window;
                this.findLocationDialogWindow = (Window)locationDialogView;
                this.findLocationDialogWindow.FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                this._childWindows.Add(this.findLocationDialogWindow);
                this.findLocationDialogWindow.Closed += (EventHandler)((s, e) =>
                {
                    this._childWindows.Remove(this.findLocationDialogWindow);
                    this.findLocationDialogWindow = (Window)null;
                });
            }
            this.findLocationDialogWindow.Show();
            this.findLocationDialogWindow.Activate();
        }

        protected override void OnBeforeClose()
        {
            if (this.TourEditor == null || this.TourEditor.SelectedItem == null || (!this.TourEditor.SelectedItem.IsDirty || this.Model == null) || this.Model.UIDispatcher == null)
                return;
            this.Model.UIDispatcher.Invoke((Action)(() =>
            {
                if (this.saveBeforeClose)
                    this.TourEditor.UpdateSelectedScene();
                base.OnBeforeClose();
            }));
        }

        public void CloseWindow(bool saveContent)
        {
            this.saveBeforeClose = saveContent;
            this.OnExecuteClose();
            this.saveBeforeClose = true;
        }

        protected override void CloseDialogPopups()
        {
            base.CloseDialogPopups();
            if (this.findLocationDialogWindow != null)
            {
                this.findLocationDialogWindow.Close();
                this.findLocationDialogWindow = (Window)null;
            }
            if (this.customSpaceSettingsDialogWindow == null)
                return;
            this.customSpaceSettingsDialogWindow.Close();
            this.customSpaceSettingsDialogWindow = (CustomSpaceSettingsDialogView)null;
        }

        private void CloseFindLocationWindow()
        {
            this.CloseDialogPopups();
        }

        public void BeforeShow()
        {
            this.LayerManagerViewModel.OnNewView();
            this.TourPlayer.IsLoopingEnabled = false;
        }

        private void OnFieldsTabPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!(e.PropertyName == this.TaskPanel.FieldsTab.Model.PropertySelectedLayer) || this.TaskPanel.FieldsTab.Model.SelectedLayer == null)
                return;
            this.TaskPanel.FieldsTab.Model.SelectedLayer.OnBeforeVisualizing -= new Action(this.OnBeforeVisualizing);
        }

        private void OnFieldsTabPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == this.TaskPanel.FieldsTab.Model.PropertySelectedLayer) || this.TaskPanel.FieldsTab.Model.SelectedLayer == null)
                return;
            this.TaskPanel.FieldsTab.Model.SelectedLayer.OnBeforeVisualizing += new Action(this.OnBeforeVisualizing);
        }

        private void OnBeforeVisualizing()
        {
            ITimeController timeControl = this.Model.Engine.TimeControl;
            if (timeControl == null)
                return;
            timeControl.VisualTimeEnabled = false;
            DateTime? playFromTime = this.Model.LayerManager.PlayFromTime;
            DateTime? playToTime = this.Model.LayerManager.PlayToTime;
            if (!playFromTime.HasValue || !playToTime.HasValue)
                return;
            timeControl.SetVisualTimeRange(playFromTime.Value, playToTime.Value, false);
            timeControl.CurrentVisualTime = playToTime.Value;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;
            if (disposing)
            {
                this.UnhookEventHandlers();
                if (this.Globe != null)
                    this.Globe.Dispose();
            }
            this.Globe = (GlobeViewModel)null;
            this.disposed = true;
        }

        private void UnhookEventHandlers()
        {
            VisualizationModel model = this.Model;
            if (model == null)
                return;
            VisualizationEngine engine = model.Engine;
            if (engine == null)
                return;
            engine.TourSceneStateChanged -= new TourSceneStateChangeHandler(this.Engine_TourSceneStateChanged);
        }
    }
}
