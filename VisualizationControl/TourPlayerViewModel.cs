using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public sealed class TourPlayerViewModel : ViewModelBase
  {
    private static readonly TimeSpan ControlVisibleInterval = TimeSpan.FromSeconds(3.0);
    private int refreshIntervalInMinutes = 10;
    private const int RefreshIntervalInMin = 10;
    private DispatcherTimer controlsVisibilityTimer;
    private bool _controlsVisible;
    private bool _optionsVisible;
    private bool _IsPreviousEnabled;
    private bool isLoopingEnabled;
    private bool _IsNextEnabled;
    private bool isRefreshEnabled;

    public ICommand PlayCommand { get; private set; }

    public ICommand PauseCommand { get; private set; }

    public ICommand NextSceneCommand { get; private set; }

    public ICommand PreviousSceneCommand { get; private set; }

    public ICommand ExitTourPlaybackModeCommand { get; set; }

    public ITourPlayer TourPlayer { get; private set; }

    public string PropertyControlsVisible
    {
      get
      {
        return "ControlsVisible";
      }
    }

    public bool ControlsVisible
    {
      get
      {
        return this._controlsVisible;
      }
      set
      {
        if (!this.SetProperty<bool>(this.PropertyControlsVisible, ref this._controlsVisible, value || !this.TourPlayer.IsPlaying, false))
          return;
        if (this._controlsVisible)
          this.RefreshVisibleTimer();
        this.OptionsVisible = false;
      }
    }

    public string PropertyOptionsVisible
    {
      get
      {
        return "OptionsVisible";
      }
    }

    public bool OptionsVisible
    {
      get
      {
        return this._optionsVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyOptionsVisible, ref this._optionsVisible, value, false);
      }
    }

    public string PropertyIsPreviousEnabled
    {
      get
      {
        return "IsPreviousEnabled";
      }
    }

    public bool IsPreviousEnabled
    {
      get
      {
        return this._IsPreviousEnabled;
      }
    }

    public string PropertyIsLoopingEnabled
    {
      get
      {
        return "IsLoopingEnabled";
      }
    }

    public bool IsLoopingEnabled
    {
      get
      {
        return this.isLoopingEnabled;
      }
      set
      {
        if (!this.SetProperty<bool>(this.PropertyIsLoopingEnabled, ref this.isLoopingEnabled, value, false))
          return;
        this.TourPlayer.Loop = this.isLoopingEnabled;
      }
    }

    public string PropertyIsNextEnabled
    {
      get
      {
        return "IsNextEnabled";
      }
    }

    public bool IsNextEnabled
    {
      get
      {
        return this._IsNextEnabled;
      }
    }

    public string PropertyIsRefreshEnabled
    {
      get
      {
        return "IsRefreshEnabled";
      }
    }

    public bool IsRefreshEnabled
    {
      get
      {
        return this.isRefreshEnabled;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyIsRefreshEnabled, ref this.isRefreshEnabled, value, false);
      }
    }

    public string PropertyRefreshIntervalInMinutes
    {
      get
      {
        return "RefreshIntervalInMinutes";
      }
    }

    public int RefreshIntervalInMinutes
    {
      get
      {
        return this.refreshIntervalInMinutes;
      }
      set
      {
        this.SetProperty<int>(this.PropertyRefreshIntervalInMinutes, ref this.refreshIntervalInMinutes, value, false);
      }
    }

    public TourPlayerViewModel(ITourPlayer tourPlayer)
    {
      this.TourPlayer = tourPlayer;
      this.ControlsVisible = true;
      this.TourPlayer.PropertyChanged += (PropertyChangedEventHandler) ((source, args) =>
      {
        if (args.PropertyName == "IsPlaying")
        {
          if (this.TourPlayer.IsPlaying)
            this.RefreshVisibleTimer();
          else
            this.ControlsVisible = true;
        }
        else
        {
          if (!(args.PropertyName == "CurrentSceneIndex"))
            return;
          this.SetProperty<bool>(this.PropertyIsPreviousEnabled, ref this._IsPreviousEnabled, this.TourPlayer.CurrentSceneIndex != 0, false);
          this.SetProperty<bool>(this.PropertyIsNextEnabled, ref this._IsNextEnabled, this.TourPlayer.CurrentSceneIndex != this.TourPlayer.SceneCount - 1, false);
        }
      });
      this.PlayCommand = (ICommand) new DelegatedCommand((Action) (() =>
      {
        int num = (int) Win32Helper.SetThreadExecutionState(Win32Helper.ExecutionState.ES_AWAYMODE_REQUIRED | Win32Helper.ExecutionState.ES_CONTINUOUS | Win32Helper.ExecutionState.ES_DISPLAY_REQUIRED | Win32Helper.ExecutionState.ES_SYSTEM_REQUIRED);
        this.OptionsVisible = false;
        this.TourPlayer.Play();
      }));
      this.PauseCommand = (ICommand) new DelegatedCommand((Action) (() =>
      {
        int num = (int) Win32Helper.SetThreadExecutionState(Win32Helper.ExecutionState.ES_CONTINUOUS);
        this.TourPlayer.Pause();
      }));
      this.NextSceneCommand = (ICommand) new DelegatedCommand(new Action(this.TourPlayer.NextScene));
      this.PreviousSceneCommand = (ICommand) new DelegatedCommand(new Action(this.TourPlayer.PreviousScene));
    }

    private void RefreshVisibleTimer()
    {
      if (this.controlsVisibilityTimer == null)
      {
        this.controlsVisibilityTimer = new DispatcherTimer();
        this.controlsVisibilityTimer.Interval = TourPlayerViewModel.ControlVisibleInterval;
        this.controlsVisibilityTimer.Tick += (EventHandler) ((x, y) =>
        {
          this.ControlsVisible = !this.TourPlayer.IsPlaying;
          this.controlsVisibilityTimer.Stop();
        });
      }
      this.controlsVisibilityTimer.Stop();
      this.controlsVisibilityTimer.Start();
    }
  }
}
