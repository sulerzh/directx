using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class CustomSpaceGalleryViewModel : DialogViewModelBase
  {
    private static string PropertyMapOptionsList = "MapOptionsList";
    private List<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel> mMapOptionsList = new List<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel>();
    private string cancelOrClose = Resources.Dialog_CancelText;
    private readonly HostControlViewModel hostControlViewModelOriginal;
    private Scene currentScene;
    private Scene mostRecentScene;

    public List<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel> MapOptionsList
    {
      get
      {
        return this.mMapOptionsList;
      }
      private set
      {
        if (!this.SetProperty<List<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel>>(CustomSpaceGalleryViewModel.PropertyMapOptionsList, ref this.mMapOptionsList, value, false))
          return;
        this.RaisePropertyChanged("MapOptionsWithoutLines");
      }
    }

    public IEnumerable<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel> MapOptionsWithoutLines
    {
      get
      {
        return Enumerable.Where<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel>((IEnumerable<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel>) this.MapOptionsList, (Func<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel, bool>) (n => n.IsMoreThanLine));
      }
    }

    public ICommand CreateNewMapCommand { get; set; }

    public ICommand MapGalleryItemCommand { get; set; }

    public CustomSpaceGalleryViewModel.CustomCreationDelegate CustomCreationCallback { get; private set; }

    public bool IsBeingUsedForCreation
    {
      get
      {
        return this.CustomCreationCallback != null;
      }
    }

    public bool IsADialog
    {
      get
      {
        return !this.IsBeingUsedForCreation;
      }
    }

    public string GalleryTitle
    {
      get
      {
        if (!this.IsBeingUsedForCreation)
          return Resources.CustomSpaceGallery_DialogTitle;
        else
          return Resources.CustomSpaceGallery_DialogTitleNewScene;
      }
    }

    public static string CancelOrCloseProperty
    {
      get
      {
        return "CancelOrClose";
      }
    }

    public string CancelOrClose
    {
      get
      {
        return this.cancelOrClose;
      }
      set
      {
        this.SetProperty<string>(CustomSpaceGalleryViewModel.CancelOrCloseProperty, ref this.cancelOrClose, value, false);
      }
    }

    public event Action GalleryClosing;

    public CustomSpaceGalleryViewModel(HostControlViewModel hostControlViewModel, Scene theScene, CustomSpaceGalleryViewModel.CustomCreationDelegate creationCallback = null)
    {
      try
      {
        this.CustomCreationCallback = creationCallback;
        this.hostControlViewModelOriginal = hostControlViewModel;
        this.CancelCommand = (ICommand) new DelegatedCommand((Action) (() =>
        {
          this.hostControlViewModelOriginal.DismissDialog((IDialog) this);
          if (this.GalleryClosing == null)
            return;
          this.GalleryClosing();
        }));
        this.CreateNewMapCommand = (ICommand) new DelegatedCommand((Action) (() => this.Command_AddCustomSpace()));
        this.MapGalleryItemCommand = (ICommand) new DelegatedCommand<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel>(new Action<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel>(this.Command_ItemSelected));
        this.SetCurrentScene(theScene);
      }
      catch
      {
        VisualizationTraceSource.Current.Fail("Custom Space Gallery encountered an error.");
      }
    }

    public void SetCurrentScene(Scene theScene)
    {
      this.currentScene = theScene;
      if (theScene != null)
        this.mostRecentScene = theScene;
      List<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel> mapOptionsList = this.MapOptionsList;
      List<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel> list = new List<CustomSpaceGalleryViewModel.MapGalleryOptionViewModel>();
      if (theScene != null)
      {
        if (this.IsBeingUsedForCreation)
        {
          list.Add(new CustomSpaceGalleryViewModel.MapGalleryOptionViewModel()
          {
            Name = string.Format(Resources.CustomSpaceGallery_DuplicateScene, (object) theScene.Name),
            Desciption = Resources.CustomSpaceGallery_DuplicateSceneDesc,
            IsInUse = true,
            ImagePathOrSource = (ImageSource) theScene.Frame.Image,
            MapOptionClickAction = (ICommand) new DelegatedCommand((Action) (() => this.Command_Duplicate()))
          });
          list.Add(new CustomSpaceGalleryViewModel.MapGalleryOptionViewModel()
          {
            IsJustALine = true
          });
        }
        list.Add(new CustomSpaceGalleryViewModel.MapGalleryOptionViewModel()
        {
          Name = Resources.CustomSpaceGallery_EarthMapName,
          IsInUse = !this.currentScene.HasCustomMap && !this.IsBeingUsedForCreation,
          ImagePathOrSource = this.ForIcon("/VisualizationControl;component/Images/MapType_Globe.png"),
          MapOptionClickAction = (ICommand) new DelegatedCommand((Action) (() => this.Command_GlobeView())),
          Desciption = this.IsBeingUsedForCreation ? Resources.CustomSpaceGallery_EarthMapName_CreateDesc : Resources.CustomSpaceGallery_EarthMapName_SelectDesc
        });
        list.Add(new CustomSpaceGalleryViewModel.MapGalleryOptionViewModel()
        {
          Name = Resources.CustomSpaceGallery_NewCustomMap,
          ImagePathOrSource = this.ForIcon("/VisualizationControl;component/Images/MapType_Alt.png"),
          MapOptionClickAction = (ICommand) new DelegatedCommand((Action) (() => this.Command_AddCustomSpace())),
          Desciption = this.IsBeingUsedForCreation ? Resources.CustomSpaceGallery_NewCustomMap_CreateDesc : Resources.CustomSpaceGallery_NewCustomMap_SelectDesc
        });
        bool flag = false;
        int count = list.Count;
        foreach (CustomMap customMap in this.hostControlViewModelOriginal.Model.CustomMapProvider.MapCollection.AvailableMaps)
        {
          CustomMap cm = customMap;
          bool isActiveMap = this.currentScene.CustomMapId == cm.UniqueCustomMapId;
          if (!cm.IsTemporary || !this.IsBeingUsedForCreation && isActiveMap)
          {
            CustomSpaceGalleryViewModel.MapGalleryOptionViewModel galleryOptionViewModel = new CustomSpaceGalleryViewModel.MapGalleryOptionViewModel();
            galleryOptionViewModel.Name = cm.Name;
            galleryOptionViewModel.CanDelete = true;
            galleryOptionViewModel.CanEdit = isActiveMap && !this.IsBeingUsedForCreation;
            galleryOptionViewModel.IsInUse = isActiveMap && !this.IsBeingUsedForCreation;
            galleryOptionViewModel.ImagePathOrSource = (ImageSource) cm.ImageForUI;
            galleryOptionViewModel.MapOptionClickAction = (ICommand) new DelegatedCommand((Action) (() => this.Command_SetCustomSpace(cm)));
            galleryOptionViewModel.MapOptionEditAction = (ICommand) new DelegatedCommand((Action) (() => this.Command_EditCustomSpace(cm)));
            galleryOptionViewModel.MapOptionRemoveAction = (ICommand) new DelegatedCommand((Action) (() => this.Command_RemoveCustomSpace(cm, isActiveMap)));
            galleryOptionViewModel.Desciption = string.Format(this.IsBeingUsedForCreation ? Resources.CustomSpaceGallery_SelectCustomMap_CreateDesc : Resources.CustomSpaceGallery_SelectCustomMap_SelectDesc, (object) cm.Name);
            if (isActiveMap && !this.IsBeingUsedForCreation)
            {
              list.Insert(0, galleryOptionViewModel);
              list.Insert(1, new CustomSpaceGalleryViewModel.MapGalleryOptionViewModel()
              {
                IsJustALine = true
              });
            }
            else
            {
              if (!flag)
              {
                list.Add(new CustomSpaceGalleryViewModel.MapGalleryOptionViewModel()
                {
                  IsJustALine = true
                });
                flag = true;
              }
              list.Add(galleryOptionViewModel);
            }
          }
        }
      }
      this.MapOptionsList = list;
    }

    private void Command_Close()
    {
      this.SetCurrentScene((Scene) null);
      this.CancelCommand.Execute((object) this);
    }

    private void Command_Duplicate()
    {
      this.InnerSetCustomSpace(true, (CustomMap) null, false);
      this.Command_Close();
    }

    private void Command_GlobeView()
    {
      this.InnerSetCustomSpace(false, (CustomMap) null, false);
      this.Command_Close();
    }

    private void Command_FlatGlobeView()
    {
      this.InnerSetCustomSpace(false, (CustomMap) null, false);
      this.Command_Close();
    }

    private void Command_AddCustomSpace()
    {
      CustomMap customMap = this.hostControlViewModelOriginal.Model.CustomMapProvider.MapCollection.CreateCustomMap();
      this.InnerSetCustomSpace(false, customMap, true);
      this.Command_Close();
      this.InnerBringupMapEdit(customMap);
    }

    private void Command_EditCustomSpace(CustomMap cs)
    {
      this.Command_Close();
      this.InnerBringupMapEdit(cs);
    }

    private void Command_SetCustomSpace(CustomMap cs)
    {
      this.InnerSetCustomSpace(false, cs, false);
      this.Command_Close();
    }

    private void Command_ItemSelected(CustomSpaceGalleryViewModel.MapGalleryOptionViewModel ob)
    {
      if (ob == null || ob.MapOptionClickAction == null)
        return;
      ob.MapOptionClickAction.Execute((object) ob);
    }

    private void ForceChangeMapType(CustomMap mapOrNull, bool isNewMap)
    {
      HostControlViewModel controlViewModel = this.hostControlViewModelOriginal;
      Guid guid = mapOrNull != null ? mapOrNull.UniqueCustomMapId : CustomMap.InvalidMapId;
      Scene scene = this.currentScene ?? this.mostRecentScene;
      if (scene == null || !((scene.HasCustomMap ? scene.CustomMapId : CustomMap.InvalidMapId) != guid))
        return;
      scene.CustomMapId = guid;
      controlViewModel.Model.Engine.SetCustomMap(guid);
      if (mapOrNull != null)
      {
        scene.FlatModeEnabled = true;
        controlViewModel.Model.Engine.FlatMode = true;
      }
      CameraSnapshot newCameraSnapshot = mapOrNull != null ? CameraSnapshot.DefaultForCustomMap() : controlViewModel.Globe.InitialCameraPosition();
      controlViewModel.Model.Engine.MoveCamera(newCameraSnapshot, CameraMoveStyle.FlyTo, false);
      controlViewModel.IsGeoMapsToolsEnabled = mapOrNull == null;
      controlViewModel.Model.Engine.ForceUpdate();
      this.RemapDisplayedData(guid);
      controlViewModel.TourEditor.UpdateSelectedScene();
      if (mapOrNull == null)
      {
        scene.FlatModeEnabled = false;
        controlViewModel.Model.Engine.FlatMode = false;
      }
      controlViewModel.ResetUndoStack();
    }

    private void InnerSetCustomSpace(bool isDuplicate, CustomMap mapOrNull, bool isNewMap)
    {
      if (this.CustomCreationCallback != null)
        this.CustomCreationCallback(isDuplicate, mapOrNull, isNewMap);
      else
        this.ForceChangeMapType(mapOrNull, isNewMap);
    }

    private void InnerBringupMapEdit(CustomMap cm)
    {
      if (this.CustomCreationCallback != null)
        return;
      this.hostControlViewModelOriginal.ShowSceneBackgroundSettingsDialog(cm);
    }

    private void Command_RemoveCustomSpace(CustomMap cs, bool isActiveSpace)
    {
      if (MessageBox.Show(Resources.CustomSpaceGallery_DeleteMapText, Resources.CustomSpaceGallery_DeleteMap_Desc, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None) != MessageBoxResult.Yes)
        return;
      if (isActiveSpace)
        this.ForceChangeMapType((CustomMap) null, false);
      this.hostControlViewModelOriginal.Model.CustomMapProvider.MapCollection.PermanentlyDeleteCustomMap(cs);
      this.SetCurrentScene(!isActiveSpace ? this.currentScene : (Scene) null);
      if (!isActiveSpace)
        return;
      this.Command_Close();
    }

    private ImageSource ForIcon(string path)
    {
      return (ImageSource) new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
    }

    private void RemapDisplayedData(Guid customMapId)
    {
      VisualizationModel model = this.hostControlViewModelOriginal.Model;
      LayerManager layerManager = model == null ? (LayerManager) null : model.LayerManager;
      if (layerManager == null)
        return;
      layerManager.ForciblyRefreshDisplay(true);
      foreach (LayerDefinition layerDefinition in (Collection<LayerDefinition>) layerManager.LayerDefinitions)
        layerDefinition.DataLoadedCustomSpaceId = customMapId;
    }

    public delegate void CustomCreationDelegate(bool isDuplicate, CustomMap selectedMap, bool isANewMap);

    public class MapGalleryOptionViewModel
    {
      public string Name { get; set; }

      public string Desciption { get; set; }

      public ImageSource ImagePathOrSource { get; set; }

      public bool IsInUse { get; set; }

      public bool CanDelete { get; set; }

      public bool CanEdit { get; set; }

      public bool IsJustALine { get; set; }

      public ICommand MapOptionClickAction { get; set; }

      public ICommand MapOptionEditAction { get; set; }

      public ICommand MapOptionRemoveAction { get; set; }

      public bool IsMissingPicture
      {
        get
        {
          return this.ImagePathOrSource == null;
        }
      }

      public bool IsMoreThanLine
      {
        get
        {
          return !this.IsJustALine;
        }
      }
    }
  }
}
