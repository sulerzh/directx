using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TransitionDurationViewModel : ViewModelBase
  {
    private static long DurationIncrementTicks = TimeSpan.FromMilliseconds(250.0).Ticks;

    public SceneSettingsViewModel SceneSettings { get; private set; }

    public ICommand UpCommand { get; private set; }

    public ICommand DownCommand { get; private set; }

    public TransitionDurationViewModel(SceneSettingsViewModel sceneSettingsViewModel)
    {
      if (sceneSettingsViewModel == null)
        throw new ArgumentNullException("sceneSettingsViewModel");
      this.SceneSettings = sceneSettingsViewModel;
      this.UpCommand = (ICommand) new DelegatedCommand((Action) (() => this.SceneSettings.ParentScene.Scene.TransitionDuration = TransitionDurationViewModel.TimeSpanFromIncrements((int) Math.Floor(TransitionDurationViewModel.TimeSpanAsIncrements(this.SceneSettings.ParentScene.Scene.TransitionDuration)) + 1)), (Predicate) (() => this.SceneSettings.ParentScene.Scene.TransitionDuration < TourEditorViewModel.MaximumTransitionDuration));
      this.DownCommand = (ICommand) new DelegatedCommand((Action) (() => this.SceneSettings.ParentScene.Scene.TransitionDuration = TransitionDurationViewModel.TimeSpanFromIncrements((int) Math.Ceiling(TransitionDurationViewModel.TimeSpanAsIncrements(this.SceneSettings.ParentScene.Scene.TransitionDuration)) - 1)), (Predicate) (() => this.SceneSettings.ParentScene.Scene.TransitionDuration > TourEditorViewModel.MinimumTransitionDuration));
    }

    private static double TimeSpanAsIncrements(TimeSpan duration)
    {
      return (double) duration.Ticks / (double) TransitionDurationViewModel.DurationIncrementTicks;
    }

    private static TimeSpan TimeSpanFromIncrements(int durationIncrements)
    {
      return TimeSpan.FromTicks((long) durationIncrements * TransitionDurationViewModel.DurationIncrementTicks);
    }
  }
}
