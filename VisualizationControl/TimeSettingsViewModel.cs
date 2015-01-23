using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TimeSettingsViewModel : ViewModelBase
  {
    public static readonly TimeSpan MinimumDuration = new TimeSpan(0, 0, 0, 0, 250);
    public static readonly TimeSpan MaximumDuration = new TimeSpan(0, 0, 30, 0, 0);
    public static readonly TimeSpan DefaultDuration = new TimeSpan(0, 0, 0, 20, 0);
    public const double MaximumSpeedValue = -0.757858283255199;
    public const double MinimumSpeedValue = -4.47769492694043;
    public const double SmallStepFraction = 0.01;
    public const double LargeStepFraction = 0.1;
    private ITimeController _TimeController;
    private LayerManager _LayerManager;
    private DateTimeEditorViewModel _Start;
    private DateTimeEditorViewModel _End;

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
        this.SetProperty<ITimeController>(TimeSettingsViewModel.PropertyTimeController, ref this._TimeController, value, false);
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
        this.SetProperty<LayerManager>(TimeSettingsViewModel.PropertyLayerManager, ref this._LayerManager, value, false);
      }
    }

    public static string PropertyStart
    {
      get
      {
        return "Start";
      }
    }

    public DateTimeEditorViewModel Start
    {
      get
      {
        return this._Start;
      }
      private set
      {
        this.SetProperty<DateTimeEditorViewModel>(TimeSettingsViewModel.PropertyStart, ref this._Start, value, false);
      }
    }

    public static string PropertyEnd
    {
      get
      {
        return "End";
      }
    }

    public DateTimeEditorViewModel End
    {
      get
      {
        return this._End;
      }
      private set
      {
        this.SetProperty<DateTimeEditorViewModel>(TimeSettingsViewModel.PropertyEnd, ref this._End, value, false);
      }
    }

    internal TimeSettingsViewModel()
    {
      this.Start = new DateTimeEditorViewModel();
      this.End = new DateTimeEditorViewModel();
    }

    public TimeSettingsViewModel(ITimeController timeController, LayerManager layerManager)
      : this()
    {
      this.TimeController = timeController;
      this.LayerManager = layerManager;
      this.LayerManager.PropertyChanged += new PropertyChangedEventHandler(this.LayerManager_PropertyChanged);
      this.AdjustMinMaxOfStartAndEnd();
      this.Start.PropertyChanged += new PropertyChangedEventHandler(this.Start_PropertyChanged);
      this.End.PropertyChanged += new PropertyChangedEventHandler(this.End_PropertyChanged);
    }

    private void LayerManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == LayerManager.MinTimeProperty) && !(e.PropertyName == LayerManager.MaxTimeProperty))
        return;
      this.AdjustMinMaxOfStartAndEnd();
    }

    private void AdjustMinMaxOfStartAndEnd()
    {
      if (!this.LayerManager.MinTime.HasValue || !this.LayerManager.MaxTime.HasValue)
        return;
      this.Start.MinimumCalendarDate = this.LayerManager.MinTime.Value;
      this.End.MaximumCalendarDate = this.LayerManager.MaxTime.Value;
      this.Start.ComposedDate = this.LayerManager.PlayFromTime.HasValue ? this.LayerManager.PlayFromTime.Value : this.LayerManager.MinTime.Value;
      this.End.ComposedDate = this.LayerManager.PlayToTime.HasValue ? this.LayerManager.PlayToTime.Value : this.LayerManager.MaxTime.Value;
    }

    private void End_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == this.End.PropertyComposedDate))
        return;
      DateTime? playToTime = this.LayerManager.PlayToTime;
      DateTime composedDate = this.End.ComposedDate;
      if ((!playToTime.HasValue ? 1 : (playToTime.GetValueOrDefault() != composedDate ? 1 : 0)) != 0)
        this.LayerManager.PlayToTime = new DateTime?(this.End.ComposedDate);
      this.Start.MaximumCalendarDate = this.End.ComposedDate;
    }

    private void Start_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == this.Start.PropertyComposedDate))
        return;
      DateTime? playFromTime = this.LayerManager.PlayFromTime;
      DateTime composedDate = this.Start.ComposedDate;
      if ((!playFromTime.HasValue ? 1 : (playFromTime.GetValueOrDefault() != composedDate ? 1 : 0)) != 0)
        this.LayerManager.PlayFromTime = new DateTime?(this.Start.ComposedDate);
      this.End.MinimumCalendarDate = this.Start.ComposedDate;
    }
  }
}
