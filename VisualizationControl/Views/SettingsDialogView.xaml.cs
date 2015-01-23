using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class SettingsDialogView : UserControl
    {
        public SettingsDialogView()
        {
            this.InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialogViewModel settingsDialogViewModel = this.DataContext as SettingsDialogViewModel;
            if (settingsDialogViewModel == null)
                return;
            if (settingsDialogViewModel.StartedShrinkOp)
            {
                if (MessageBox.Show(Microsoft.Data.Visualization.VisualizationControls.Resources.SettingsDialog_CancelWarningMessage, Microsoft.Data.Visualization.VisualizationControls.Resources.SettingsDialog_CancelWarningCaption, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel, Microsoft.Data.Visualization.VisualizationControls.Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None) != MessageBoxResult.OK)
                    return;
                settingsDialogViewModel.CancelShrink();
            }
            settingsDialogViewModel.SetGraphicsLevel(e.Source == this.OkBtn);
            settingsDialogViewModel.CancelCommand.Execute((object)null);
        }

        private void ShrinkBtn_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsDialogViewModel settingsDialogViewModel = this.DataContext as SettingsDialogViewModel;
            if (settingsDialogViewModel == null)
                return;
            settingsDialogViewModel.ShrinkCache();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(this.Hyl.NavigateUri.ToString());
        }
    }
}
