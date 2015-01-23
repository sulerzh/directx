using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class ListFilterView : UserControl
    {
        public ListFilterView()
        {
            this.InitializeComponent();
        }

        private void SearchBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            ListFilterViewModel listFilterViewModel = this.DataContext as ListFilterViewModel;
            if (listFilterViewModel == null)
                return;
            listFilterViewModel.InSearchMode = true;
        }

        private void SearchBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            ListFilterViewModel listFilterViewModel = this.DataContext as ListFilterViewModel;
            if (listFilterViewModel == null || !string.IsNullOrEmpty(listFilterViewModel.SearchString) || listFilterViewModel.ShowingSearchResults)
                return;
            listFilterViewModel.InSearchMode = false;
        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            ListFilterViewModel listFilterViewModel = this.DataContext as ListFilterViewModel;
            if (listFilterViewModel == null || e.Key != Key.Escape)
                return;
            if (listFilterViewModel.ShowingSearchResults && listFilterViewModel.ClearSearchCommand.CanExecute((object)listFilterViewModel))
                listFilterViewModel.ClearSearchCommand.Execute((object)listFilterViewModel);
            this.SelectablesViewer.Focus();
        }
    }
}
