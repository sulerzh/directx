using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class TimeScrubberView : UserControl
    {
        public TimeScrubberView()
        {
            this.InitializeComponent();
        }

        private void PlayButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.PauseButton.IsKeyboardFocused || !(bool)e.NewValue)
                return;
            this.PlayButton.Focus();
        }

        private void PauseButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.PlayButton.IsKeyboardFocused || !(bool)e.NewValue)
                return;
            this.PauseButton.Focus();
        }
    }
}
