using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class FindLocationDialogView : Window
    {

        public FindLocationDialogView()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus((IInputElement)this.LocationTextBox);
            this.LocationTextBox.SelectAll();
        }

        private void LocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((FindLocationViewModel)this.DataContext).ClearStatus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ((FindLocationViewModel)this.DataContext).Cancel();
            base.OnClosing(e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
