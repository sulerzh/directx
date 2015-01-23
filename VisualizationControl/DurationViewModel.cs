using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DurationViewModel : ViewModelBase
  {
    private static long DurationIncrementTicks = TimeSpan.FromMilliseconds(250.0).Ticks;

    public SceneSettingsViewModel SceneSettings { get; private set; }

    public ICommand UpCommand { get; private set; }

    public ICommand DownCommand { get; private set; }

    public DurationViewModel(SceneSettingsViewModel sceneSettingsViewModel)
    {
      if (sceneSettingsViewModel == null)
        throw new ArgumentNullException("sceneSettingsViewModel");
      this.SceneSettings = sceneSettingsViewModel;
      this.UpCommand = (ICommand) new DelegatedCommand((Action) (() => this.SceneSettings.ParentScene.Scene.Duration = DurationViewModel.TimeSpanFromIncrements((int) Math.Floor(DurationViewModel.TimeSpanAsIncrements(this.SceneSettings.ParentScene.Scene.Duration)) + 1)), (Predicate) (() => this.SceneSettings.ParentScene.Scene.Duration < TimeSettingsViewModel.MaximumDuration));
      this.DownCommand = (ICommand) new DelegatedCommand((Action) (() => this.SceneSettings.ParentScene.Scene.Duration = DurationViewModel.TimeSpanFromIncrements((int) Math.Ceiling(DurationViewModel.TimeSpanAsIncrements(this.SceneSettings.ParentScene.Scene.Duration)) - 1)), (Predicate) (() => this.SceneSettings.ParentScene.Scene.Duration > TimeSettingsViewModel.MinimumDuration));
    }

    private static double TimeSpanAsIncrements(TimeSpan duration)
    {
      return (double) duration.Ticks / (double) DurationViewModel.DurationIncrementTicks;
    }

    private static TimeSpan TimeSpanFromIncrements(int durationIncrements)
    {
      return TimeSpan.FromTicks((long) durationIncrements * DurationViewModel.DurationIncrementTicks);
    }
  }
}
