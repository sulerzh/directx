using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class ExpressionFilterView : UserControl
    {
        public ExpressionFilterView()
        {
            this.InitializeComponent();
        }

        private void AndOp_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ExpressionFilterViewModel expressionFilterViewModel = this.DataContext as ExpressionFilterViewModel;
            if (expressionFilterViewModel == null)
                return;
            expressionFilterViewModel.IsAndConnector = true;
        }

        private void OrOp_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ExpressionFilterViewModel expressionFilterViewModel = this.DataContext as ExpressionFilterViewModel;
            if (expressionFilterViewModel == null)
                return;
            expressionFilterViewModel.IsAndConnector = false;
        }

    }
}
