using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class SceneSettingsViewModel : ViewModelBase
  {
    public static Dictionary<SceneEffect, string> SceneEffects = new Dictionary<SceneEffect, string>()
    {
      {
        SceneEffect.Station,
        Resources.SceneEffect_Station
      },
      {
        SceneEffect.Circle,
        Resources.SceneEffect_Circle
      },
      {
        SceneEffect.Dolly,
        Resources.SceneEffect_Dolly
      },
      {
        SceneEffect.Figure8,
        Resources.SceneEffect_Figure8
      },
      {
        SceneEffect.FlyOver,
        Resources.SceneEffect_FlyOver
      },
      {
        SceneEffect.PushIn,
        Resources.SceneEffect_PushIn
      },
      {
        SceneEffect.RotateGlobe,
        Resources.SceneEffect_RotateGlobe
      }
    };
    private DurationViewModel _Duration;
    private TransitionDurationViewModel _TransitionDuration;
    private SceneViewModel _ParentScene;
    private TimeScrubberViewModel _TimeScrubber;
    private TimeSettingsViewModel _TimeSettings;
    private bool _IsCustomMapEditable;

    public static string PropertyDuration
    {
      get
      {
        return "Duration";
      }
    }

    public DurationViewModel Duration
    {
      get
      {
        return this._Duration;
      }
      set
      {
        this.SetProperty<DurationViewModel>(SceneSettingsViewModel.PropertyDuration, ref this._Duration, value, false);
      }
    }

    public TransitionDurationViewModel TransitionDuration
    {
      get
      {
        return this._TransitionDuration;
      }
      set
      {
        this.SetProperty<TransitionDurationViewModel>(SceneSettingsViewModel.PropertyDuration, ref this._TransitionDuration, value, false);
      }
    }

    public string PropertyParentScene
    {
      get
      {
        return "ParentScene";
      }
    }

    public SceneViewModel ParentScene
    {
      get
      {
        return this._ParentScene;
      }
      set
      {
        this.SetProperty<SceneViewModel>(this.PropertyParentScene, ref this._ParentScene, value, false);
        this.IsCustomMapEditable = value != null && value.Scene.HasCustomMap;
      }
    }

    public string PropertyTimeScrubber
    {
      get
      {
        return "TimeScrubber";
      }
    }

    public TimeScrubberViewModel TimeScrubber
    {
      get
      {
        return this._TimeScrubber;
      }
      set
      {
        this.SetProperty<TimeScrubberViewModel>(this.PropertyTimeScrubber, ref this._TimeScrubber, value, false);
      }
    }

    public string PropertyTimeSettings
    {
      get
      {
        return "TimeSettings";
      }
    }

    public TimeSettingsViewModel TimeSettings
    {
      get
      {
        return this._TimeSettings;
      }
      set
      {
        this.SetProperty<TimeSettingsViewModel>(this.PropertyTimeSettings, ref this._TimeSettings, value, false);
      }
    }

    public string PropertyIsCustomMapEditable
    {
      get
      {
        return "IsCustomMapEditable";
      }
    }

    public bool IsCustomMapEditable
    {
      get
      {
        return this._IsCustomMapEditable;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyIsCustomMapEditable, ref this._IsCustomMapEditable, value, false);
      }
    }

    public ICommand ChangeSceneTypeCommand { get; private set; }

    public ICommand EditCustomMapCommand { get; private set; }

    public SceneSettingsViewModel(HostControlViewModel host, TimeSettingsViewModel timeSettings, TimeScrubberViewModel timeScrubber)
    {
      SceneSettingsViewModel settingsViewModel = this;
      this.TimeSettings = timeSettings;
      this.TimeScrubber = timeScrubber;
      this.Duration = new DurationViewModel(this);
      this.TransitionDuration = new TransitionDurationViewModel(this);
      this.ChangeSceneTypeCommand = (ICommand) new DelegatedCommand((Action) (() => host.ShowDialog((IDialog) new CustomSpaceGalleryViewModel(host, settingsViewModel._ParentScene.Scene, (CustomSpaceGalleryViewModel.CustomCreationDelegate) null))));
      this.EditCustomMapCommand = (ICommand) new DelegatedCommand((Action) (() =>
      {
        Scene scene = settingsViewModel._ParentScene.Scene;
        CustomMap cm = (CustomMap) null;
        if (scene.HasCustomMap)
          cm = host.Model.CustomMapProvider.MapCollection.FindOrCreateMapFromId(scene.CustomMapId);
        if (cm == null)
          return;
        host.ShowSceneBackgroundSettingsDialog(cm);
      }));
    }
  }
}
