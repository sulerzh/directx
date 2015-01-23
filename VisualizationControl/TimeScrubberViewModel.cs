using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TimeScrubberViewModel : ViewModelBase
  {
    private double _SmallStep;
    private double _LargeStep;
    private LayerManager _LayerManager;
    private LayerManagerViewModel _LayerManagerViewModel;
    private ITimeController _TimeController;
    private bool _IsActive;
    private Action openSettingsTabAction;
    private HostControlViewModel hostWindow;

    public ICommand PlayButtonCommand { get; private set; }

    public ICommand CloseButtonCommand { get; private set; }

    public ICommand LoopButtonCommand { get; private set; }

    public ICommand SettingsCommand { get; private set; }

    public string PropertySmallStep
    {
      get
      {
        return "SmallStep";
      }
    }

    public double SmallStep
    {
      get
      {
        return this._SmallStep;
      }
      private set
      {
        this.SetProperty<double>(this.PropertySmallStep, ref this._SmallStep, value, false);
      }
    }

    public string PropertyLargeStep
    {
      get
      {
        return "LargeStep";
      }
    }

    public double LargeStep
    {
      get
      {
        return this._LargeStep;
      }
      private set
      {
        this.SetProperty<double>(this.PropertyLargeStep, ref this._LargeStep, value, false);
      }
    }

    public static string PropertyLayerManager
    {
      get
      {
        return "LayerManager";
      }
    }

    public LayerManager LayerManager
    {
      get
      {
        return this._LayerManager;
      }
      private set
      {
        this.SetProperty<LayerManager>(TimeScrubberViewModel.PropertyLayerManager, ref this._LayerManager, value, false);
      }
    }

    public string PropertyLayerManagerViewModel
    {
      get
      {
        return "LayerManagerViewModel";
      }
    }

    public LayerManagerViewModel LayerManagerViewModel
    {
      get
      {
        return this._LayerManagerViewModel;
      }
      private set
      {
        this.SetProperty<LayerManagerViewModel>(this.PropertyLayerManagerViewModel, ref this._LayerManagerViewModel, value, false);
      }
    }

    public static string PropertyTimeController
    {
      get
      {
        return "TimeController";
      }
    }

    public ITimeController TimeController
    {
      get
      {
        return this._TimeController;
      }
      private set
      {
        this.SetProperty<ITimeController>(TimeScrubberViewModel.PropertyTimeController, ref this._TimeController, value, false);
      }
    }

    public static string PropertyIsActive
    {
      get
      {
        return "IsActive";
      }
    }

    public bool IsActive
    {
      get
      {
        return this._IsActive;
      }
      set
      {
        this.SetProperty<bool>(TimeScrubberViewModel.PropertyIsActive, ref this._IsActive, value, false);
      }
    }

    public string IsTimeDataAvailableProperty
    {
      get
      {
        return "IsTimeDataAvailable";
      }
    }

    public bool IsTimeDataAvailable
    {
      get
      {
        if (this._LayerManager.PlayFromTime.HasValue)
          return this._LayerManager.PlayToTime.HasValue;
        else
          return false;
      }
    }

    public TimeScrubberViewModel(HostControlViewModel hostWindow, ITimeController timeController, LayerManagerViewModel layerManagerVM, Action openSettingsTabAction)
    {
      this.TimeController = timeController;
      if (this.TimeController != null)
      {
        this.TimeController.PropertyChanged += new PropertyChangedEventHandler(this.TimeControllerPropertyChanged);
        this.TimeController.Looping = false;
        this.TimeController.VisualTimeEnabled = false;
      }
      this.openSettingsTabAction = openSettingsTabAction;
      this.PlayButtonCommand = (ICommand) new DelegatedCommand(new Action(this.PlayPause));
      this.CloseButtonCommand = (ICommand) new DelegatedCommand(new Action(this.Close));
      this.LoopButtonCommand = (ICommand) new DelegatedCommand(new Action(this.LoopToggle));
      this.SettingsCommand = (ICommand) new DelegatedCommand(new Action(this.InvokeSettings));
      this.hostWindow = hostWindow;
      if (layerManagerVM == null)
        return;
      this.LayerManagerViewModel = layerManagerVM;
      if (this.LayerManagerViewModel == null)
        return;
      this.LayerManager = layerManagerVM.Model;
      if (this.LayerManager == null)
        return;
      this.LayerManager.PropertyChanged += new PropertyChangedEventHandler(this.LayerManager_PropertyChanged);
      if (!this.IsTimeDataAvailable)
        return;
      this.UpdateScrubberRangePositionAndSteps(true);
    }

    private void TimeControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.LayerManager == null)
        return;
      this.LayerManagerViewModel.DecoratorLayer.UpdateTimeDecorator(this.TimeController.CurrentVisualTime);
    }

    private void LayerManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == LayerManager.MinTimeProperty) && !(e.PropertyName == LayerManager.MaxTimeProperty) && (!(e.PropertyName == LayerManager.PlayFromTimeProperty) && !(e.PropertyName == LayerManager.PlayToTimeProperty)))
        return;
        if (this.IsTimeDataAvailable)
        {
            this.UpdateScrubberRangePositionAndSteps(e.PropertyName == LayerManager.MinTimeProperty ||
                                                     e.PropertyName == LayerManager.MaxTimeProperty);
            Action action = delegate
            {
                this.LayerManagerViewModel.DecoratorLayer.AddTimeDecorator();
            };
            this.LayerManager.Model.UIDispatcher.Invoke(action, new object[0]);
        }
        else
        {
            Action action = delegate
            {
                this.LayerManagerViewModel.DecoratorLayer.RemoveTimeDecorator();
            };
            this.LayerManager.Model.UIDispatcher.Invoke(action, new object[0]);
        }
        this.RaisePropertyChanged(this.IsTimeDataAvailableProperty);
      this.IsActive = this.IsTimeDataAvailable;
    }

    private void UpdateScrubberRangePositionAndSteps(bool setToEndTimeIfPaused)
    {
      if (this.hostWindow.Mode == HostWindowMode.Exploration)
      {
        this.TimeController.SetVisualTimeRange(this.LayerManager.PlayFromTime.Value, this.LayerManager.PlayToTime.Value, false);
        if (this.TimeController.Duration.Equals(TimeSpan.Zero))
          this.TimeController.Duration = TimeSettingsViewModel.DefaultDuration;
        else if (this.TimeController.Duration.CompareTo(TimeSettingsViewModel.MinimumDuration) < 0)
          this.TimeController.Duration = TimeSettingsViewModel.MinimumDuration;
        else if (this.TimeController.Duration.CompareTo(TimeSettingsViewModel.MaximumDuration) > 0)
          this.TimeController.Duration = TimeSettingsViewModel.MaximumDuration;
        if (this.TimeController.CurrentVisualTime < this.LayerManager.PlayFromTime.Value)
          this.TimeController.CurrentVisualTime = this.LayerManager.PlayFromTime.Value;
        else if (this.TimeController.CurrentVisualTime > this.LayerManager.PlayToTime.Value)
          this.TimeController.CurrentVisualTime = this.LayerManager.PlayToTime.Value;
        else if (setToEndTimeIfPaused && !this.TimeController.VisualTimeEnabled)
          this.TimeController.CurrentVisualTime = this.LayerManager.PlayToTime.Value;
      }
      double num = (double) (this.LayerManager.PlayToTime.Value - this.LayerManager.PlayFromTime.Value).Ticks;
      this.SmallStep = num * 0.01;
      this.LargeStep = num * 0.1;
    }

    private void PlayPause()
    {
      if (!this.TimeController.VisualTimeEnabled)
      {
        if (!this.LayerManager.PlayFromTime.HasValue || !this.LayerManager.PlayToTime.HasValue)
          return;
        this.TimeController.SetVisualTimeRange(this.LayerManager.PlayFromTime.Value, this.LayerManager.PlayToTime.Value, false);
        if (this.TimeController.CurrentVisualTime >= this.LayerManager.PlayToTime.Value)
          this.TimeController.CurrentVisualTime = this.LayerManager.PlayFromTime.Value;
        this.TimeController.VisualTimeEnabled = true;
      }
      else
        this.TimeController.VisualTimeEnabled = false;
    }

    private void Close()
    {
      this.IsActive = false;
    }

    private void LoopToggle()
    {
      this.TimeController.Looping = !this.TimeController.Looping;
    }

    private void InvokeSettings()
    {
      this.openSettingsTabAction();
    }
  }
}
