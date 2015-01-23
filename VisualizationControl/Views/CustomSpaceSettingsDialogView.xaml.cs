using System;
using System.ComponentModel;
using System.Windows;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class CustomSpaceSettingsDialogView : Window
    {
        internal CustomSpaceSettingsDialogView()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CustomSpaceSettingsViewModel settingsViewModel = this.DataContext as CustomSpaceSettingsViewModel;
            if (settingsViewModel != null)
                settingsViewModel.CustomFinalClosedCommand = (Action)(() =>
                {
                    Window owner = this.Owner;
                    this.Close();
                    if (owner == null)
                        return;
                    owner.Focus();
                });
            this.Width = this.SettingsViewer.ActualWidth;
            this.Height = this.SettingsViewer.ActualHeight;
            this.ResizeMode = ResizeMode.NoResize;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            CustomSpaceSettingsViewModel settingsViewModel = this.DataContext as CustomSpaceSettingsViewModel;
            if (settingsViewModel != null)
                settingsViewModel.UnregisterMapCallbacks();
            base.OnClosing(e);
        }
    }
}
