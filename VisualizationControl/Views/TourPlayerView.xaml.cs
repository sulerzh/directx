using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class TourPlayerView : UserControl
    {
        public TourPlayerView()
        {
            this.InitializeComponent();
        }

        private void OnShowControls(object sender, InputEventArgs e)
        {
            ((TourPlayerViewModel)this.DataContext).ControlsVisible = true;
        }

        private void OptionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            TourPlayerViewModel tourPlayerViewModel = this.DataContext as TourPlayerViewModel;
            if (tourPlayerViewModel == null || tourPlayerViewModel.TourPlayer == null)
                return;
            tourPlayerViewModel.TourPlayer.Pause();
            tourPlayerViewModel.OptionsVisible = !tourPlayerViewModel.OptionsVisible;
        }

        private void OptionsButton_OnTouchDown(object sender, TouchEventArgs e)
        {
            e.Handled = true;
        }
    }
}
