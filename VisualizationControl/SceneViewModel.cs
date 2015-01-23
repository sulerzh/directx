using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class SceneViewModel : ViewModelBase
  {
    public static readonly int NameMaxLength = 32;
    private string _SceneNumber;
    private bool _IsDeleteEnabled;
    private bool _IsDirty;
    private BitmapSource _DecoratorImage;
    private Scene _Scene;

    public static string PropertySceneNumber
    {
      get
      {
        return "SceneNumber";
      }
    }

    public string SceneNumber
    {
      get
      {
        return this._SceneNumber;
      }
      set
      {
        this.SetProperty<string>(SceneViewModel.PropertySceneNumber, ref this._SceneNumber, value, false);
      }
    }

    public static string PropertyIsDeleteEnabled
    {
      get
      {
        return "IsDeleteEnabled";
      }
    }

    public bool IsDeleteEnabled
    {
      get
      {
        return this._IsDeleteEnabled;
      }
      set
      {
        this.SetProperty<bool>(SceneViewModel.PropertyIsDeleteEnabled, ref this._IsDeleteEnabled, value, false);
      }
    }

    public static string PropertyIsDirty
    {
      get
      {
        return "IsDirty";
      }
    }

    public bool IsDirty
    {
      get
      {
        return this._IsDirty;
      }
      set
      {
        this.SetProperty<bool>(SceneViewModel.PropertyIsDirty, ref this._IsDirty, value, false);
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
        this.SetProperty<BitmapSource>(SceneViewModel.PropertyDecoratorImage, ref this._DecoratorImage, value, false);
      }
    }

    public ICommand DeleteCommand { get; set; }

    public ICommand SettingsCommand { get; set; }

    public static string PropertyScene
    {
      get
      {
        return "Scene";
      }
    }

    public Scene Scene
    {
      get
      {
        return this._Scene;
      }
      set
      {
        if (value == null)
          return;
        this.SetProperty<Scene>(SceneViewModel.PropertyScene, ref this._Scene, value, false);
      }
    }

    public GlobeViewModel GlobeViewModel { get; private set; }

    public SceneViewModel(Scene scene, int index, GlobeViewModel globeViewModel)
    {
      if (scene == null)
        throw new ArgumentNullException("scene");
      this.Scene = scene;
      this.SetIndex(index);
      this.GlobeViewModel = globeViewModel;
    }

    public void SetIndex(int i)
    {
      this.SceneNumber = (i + 1).ToString((IFormatProvider) CultureInfo.CurrentCulture);
    }
  }
}
