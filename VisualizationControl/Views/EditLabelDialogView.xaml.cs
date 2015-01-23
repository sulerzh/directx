using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class EditLabelDialogView : UserControl
    {
        private EditLabelDialogViewModel _viewModel;

        private EditLabelDialogViewModel ViewModel
        {
            get
            {
                if (this._viewModel == null)
                    this._viewModel = this.DataContext as EditLabelDialogViewModel;
                return this._viewModel;
            }
        }

        public EditLabelDialogView()
        {
            this.InitializeComponent();
        }

        private void MouseClick_Title(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Label.Title;
        }

        private void MouseClick_Description(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Label.Description;
        }

        private void Focus_Title(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Label.Title;
        }

        private void Focus_Description(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Label.Description;
        }

    }
}
