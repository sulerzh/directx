using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class DecoratorCollectionView : UserControl
    {
        private DecoratorCollectionViewModel _viewModel;

        private DecoratorCollectionViewModel ViewModel
        {
            get
            {
                if (this._viewModel == null)
                    this._viewModel = this.DataContext as DecoratorCollectionViewModel;
                return this._viewModel;
            }
        }

        public DecoratorCollectionView()
        {
            this.InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            this.ViewModel.UpdateSize(e.PreviousSize.Width, e.PreviousSize.Height, e.NewSize.Width, e.NewSize.Height);
        }

    }
}
