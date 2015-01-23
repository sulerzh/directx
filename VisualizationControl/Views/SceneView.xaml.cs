using Microsoft.Data.Visualization.Engine.Graphics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class SceneView : UserControl
    {
        public SceneView()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SceneViewModel sceneViewModel = this.DataContext as SceneViewModel;
            if (sceneViewModel == null || sceneViewModel.GlobeViewModel == null)
                return;
            sceneViewModel.GlobeViewModel.OnD3DImageUpdated += new GlobeViewModel.D3DImageUpdatedEvent(this.GlobeViewModel_OnD3DImageUpdated);
        }

        private void GlobeViewModel_OnD3DImageUpdated(D3DImage11 latestFront)
        {
            this.LiveImage.Source = (ImageSource)latestFront;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SceneViewModel sceneViewModel = this.DataContext as SceneViewModel;
            if (sceneViewModel == null || sceneViewModel.GlobeViewModel == null)
                return;
            sceneViewModel.GlobeViewModel.OnD3DImageUpdated -= new GlobeViewModel.D3DImageUpdatedEvent(this.GlobeViewModel_OnD3DImageUpdated);
            this.LiveImage.Source = (ImageSource)null;
        }
    }
}
