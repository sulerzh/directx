using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class CreateVideo : System.Windows.Controls.UserControl
    {
        private string soundtrack;
        private bool loop;
        private bool fadeIn;
        private bool fadeOut;

        public CreateVideo()
        {
            this.InitializeComponent();
        }

        private void OnAudioFileSelectClick(object sender, RoutedEventArgs e)
        {
            CreateVideoViewModel createVideoViewModel = this.DataContext as CreateVideoViewModel;
            if (createVideoViewModel == null)
                return;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = Microsoft.Data.Visualization.VisualizationControls.Resources.AudioSelectionDialog_FilePickerTitle;
            openFileDialog1.AddExtension = true;
            openFileDialog1.Filter = string.Format("{0}{1}",
                Microsoft.Data.Visualization.VisualizationControls.Resources.CreateVideo_SupportedAudioFormats,
                Microsoft.Data.Visualization.VisualizationControls.Resources.CreateVideo_SupportedAudioFormatFilter);
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            OpenFileDialog openFileDialog2 = openFileDialog1;
            if (openFileDialog2.ShowDialog() == DialogResult.OK && !createVideoViewModel.TrySetSoundtrack(openFileDialog2.FileName))
            {
                int num = (int)System.Windows.MessageBox.Show(
                    Microsoft.Data.Visualization.VisualizationControls.Resources.AudioSelectionDialog_UnsupportedAudioFormatErrorMessage,
                    Microsoft.Data.Visualization.VisualizationControls.Resources.AudioSelectionDialog_UnsupportedAudioFormatErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Microsoft.Data.Visualization.VisualizationControls.Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                this.OnAudioFileSelectClick(sender, e);
            }
            this.SoundtrackSelectionDoneButton.Focus();
        }

        private void OnCreateClick(object sender, RoutedEventArgs e)
        {
            CreateVideoViewModel createVideoViewModel = this.DataContext as CreateVideoViewModel;
            if (createVideoViewModel == null)
                return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.Title = Microsoft.Data.Visualization.VisualizationControls.Resources.CreateVideo_FilePickerTitle;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.Filter = Microsoft.Data.Visualization.VisualizationControls.Resources.CreateVideo_FileTypes;
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            saveFileDialog1.FileName = createVideoViewModel.DefaultFileName;
            SaveFileDialog saveFileDialog2 = saveFileDialog1;
            if (saveFileDialog2.ShowDialog() != DialogResult.OK)
                return;
            createVideoViewModel.SelectedFileName = saveFileDialog2.FileName;
            this.VideoSettingPanel.Visibility = Visibility.Collapsed;
            this.ExportProgressPanel.Visibility = Visibility.Visible;
            createVideoViewModel.StartVideoProcessing();
        }

        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            CreateVideoViewModel createVideoViewModel = this.DataContext as CreateVideoViewModel;
            if (createVideoViewModel == null)
                return;
            createVideoViewModel.OpenFile();
        }

        private void OnConfigureSoundtrackClick(object sender, RoutedEventArgs e)
        {
            CreateVideoViewModel createVideoViewModel = this.DataContext as CreateVideoViewModel;
            if (createVideoViewModel == null)
                return;
            this.loop = createVideoViewModel.Loop;
            this.soundtrack = createVideoViewModel.IsSoundtrackOptionSet ? createVideoViewModel.SelectedSoundtrackLocation : (string)null;
            this.fadeIn = createVideoViewModel.FadeIn;
            this.fadeOut = createVideoViewModel.FadeOut;
            this.VideoSettingPanel.Visibility = Visibility.Collapsed;
            this.SoundtrackSelectionPanel.Visibility = Visibility.Visible;
            this.BrowseSoundtrackButton.Focus();
        }

        private void SoundtrackSelectionCancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            CreateVideoViewModel createVideoViewModel = this.DataContext as CreateVideoViewModel;
            if (createVideoViewModel == null)
                return;
            this.SoundtrackSelectionPanel.Visibility = Visibility.Collapsed;
            this.VideoSettingPanel.Visibility = Visibility.Visible;
            createVideoViewModel.FadeIn = this.fadeIn;
            createVideoViewModel.FadeOut = this.fadeOut;
            createVideoViewModel.Loop = this.loop;
            if (!createVideoViewModel.TrySetSoundtrack(this.soundtrack))
            {
                int num = (int)System.Windows.MessageBox.Show(
                    Microsoft.Data.Visualization.VisualizationControls.Resources.AudioSelectionDialog_UnsupportedAudioFormatErrorMessage,
                    Microsoft.Data.Visualization.VisualizationControls.Resources.AudioSelectionDialog_UnsupportedAudioFormatErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Microsoft.Data.Visualization.VisualizationControls.Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                createVideoViewModel.SetDefaultSoundtrackConfiguration();
            }
            this.VideoCreateButton.Focus();
        }

        private void SoundtrackSelectionDoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is CreateVideoViewModel))
                return;
            this.VideoSettingPanel.Visibility = Visibility.Visible;
            this.SoundtrackSelectionPanel.Visibility = Visibility.Collapsed;
            this.VideoCreateButton.Focus();
        }

        private void OnRemoveSoundtrackClick(object sender, RoutedEventArgs e)
        {
            CreateVideoViewModel createVideoViewModel = this.DataContext as CreateVideoViewModel;
            if (createVideoViewModel == null)
                return;
            createVideoViewModel.SetDefaultSoundtrackConfiguration();
        }

        private void VideoSettingPanel_OnLoaded(object sender, RoutedEventArgs e)
        {
            CreateVideoViewModel createVideoViewModel = this.DataContext as CreateVideoViewModel;
            if (createVideoViewModel == null || createVideoViewModel.VideoSessionInitialized)
                return;
            int num = (int)System.Windows.MessageBox.Show(
                Microsoft.Data.Visualization.VisualizationControls.Resources.CreateVideo_MediaFoundationMissingDialogText,
                Microsoft.Data.Visualization.VisualizationControls.Resources.CreateVideo_MediaFoundationMissingDialogCaption, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK,
                Microsoft.Data.Visualization.VisualizationControls.Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
        }
    }
}
