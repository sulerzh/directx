using Microsoft.Data.Visualization.Engine.Graphics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class GlobeView : UserControl
    {
        public GlobeView()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GlobeViewModel globeViewModel = this.DataContext as GlobeViewModel;
            if (globeViewModel == null)
                return;
            globeViewModel.SetGlobeInputElement((UIElement)this.GlobeInputCollector);
            globeViewModel.OnD3DImageUpdated += new GlobeViewModel.D3DImageUpdatedEvent(this.viewModel_OnD3DImageUpdated);
            globeViewModel.UpdateFrameSize((int)this.ActualWidth, (int)this.ActualHeight);
            globeViewModel.ResumeRendering();
        }

        private void viewModel_OnD3DImageUpdated(D3DImage11 latestFront)
        {
            this.D3DImageHost.Source = (ImageSource)latestFront;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            GlobeViewModel globeViewModel = this.DataContext as GlobeViewModel;
            if (globeViewModel == null)
                return;
            globeViewModel.StopRendering();
            globeViewModel.OnD3DImageUpdated -= new GlobeViewModel.D3DImageUpdatedEvent(this.viewModel_OnD3DImageUpdated);
            this.D3DImageHost.Source = (ImageSource)null;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            GlobeViewModel globeViewModel = this.DataContext as GlobeViewModel;
            if (globeViewModel == null)
                return;
            globeViewModel.UpdateFrameSize((int)e.NewSize.Width, (int)e.NewSize.Height);
        }
    }
}
