using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class LayerChooserRenamer : UserControl
    {
        private bool uncheckingRenameToggle;

        public LayerChooserRenamer()
        {
            this.InitializeComponent();
        }

        private void LayerRenameButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            bool? isChecked = ((ToggleButton)sender).IsChecked;
            if (!isChecked.HasValue || !isChecked.Value)
                return;
            this.uncheckingRenameToggle = true;
        }

        private void LayerRenamerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!this.uncheckingRenameToggle)
                this.LayerRenameButton.IsChecked = new bool?(false);
            else
                this.uncheckingRenameToggle = false;
        }

        private void LayerRenamerTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ((UIElement)sender).Focus();
            ((TextBoxBase)sender).SelectAll();
        }

        private void LayerRenameButton_Checked(object sender, RoutedEventArgs e)
        {
            this.LayerSelectorBox.Focus();
        }

    }
}
