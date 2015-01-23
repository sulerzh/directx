using Microsoft.Data.Visualization.Utilities;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class AnnotationDialogContentView : UserControl
    {
        private AnnotationDialogContentViewModel ViewModel
        {
            get
            {
                return this.DataContext as AnnotationDialogContentViewModel;
            }
        }

        public AnnotationDialogContentView()
        {
            this.InitializeComponent();
        }

        private void MouseClick_Title(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Model.Title;
        }

        private void MouseClick_Description(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Model.DescriptionType != AnnotationDescriptionType.Custom ? this.ViewModel.Model.FieldFormat : this.ViewModel.Model.Description;
            if (this.ViewModel.Model.DescriptionType == AnnotationDescriptionType.Image)
                return;
            this.ViewModel.Model.OriginalImage = (BitmapSource)null;
        }

        private void Focus_Title(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Model.Title;
        }

        private void Focus_Description(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Model.Description;
        }

        private void Focus_Fields(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.ViewModel.ActiveTextFormat = this.ViewModel.Model.FieldFormat;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = Microsoft.Data.Visualization.VisualizationControls.Resources.AnnotationSelectImageTitle;
            openFileDialog.Filter = Microsoft.Data.Visualization.VisualizationControls.Resources.AnnotationSelectImageFilterAllSupportedGraphics + "|*.jpg;*.jpeg;*.png|" +
                Microsoft.Data.Visualization.VisualizationControls.Resources.AnnotationSelectImageFilterJPEG + "|*.jpg;*.jpeg|" +
                Microsoft.Data.Visualization.VisualizationControls.Resources.AnnotationSelectImageFilterPortableNetworkGraphic + "|*.png";
            if (this.ViewModel == null)
                return;
            bool? nullable = openFileDialog.ShowDialog();
            if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
                return;
            if (string.IsNullOrEmpty(openFileDialog.FileName))
                return;
            try
            {
                int num1 = 512;
                BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                double width = bitmapImage.Width;
                double height = bitmapImage.Height;
                if (width > (double)num1 || height > (double)num1)
                {
                    double num2 = (double)num1 / width;
                    double num3 = (double)num1 / height;
                    double num4 = num2 < num3 ? num2 : num3;
                    TransformedBitmap transformedBitmap = new TransformedBitmap();
                    transformedBitmap.BeginInit();
                    transformedBitmap.Source = (BitmapSource)bitmapImage;
                    transformedBitmap.Transform = (Transform)new ScaleTransform(num4, num4);
                    transformedBitmap.EndInit();
                    this.ViewModel.Model.OriginalImage = (BitmapSource)transformedBitmap;
                }
                else
                    this.ViewModel.Model.OriginalImage = (BitmapSource)bitmapImage;
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, string.Format("Unable To load annotation image from URl {0}, Excepttion {1}", (object)openFileDialog.FileName, (object)((object)ex).ToString()));
            }
        }
    }
}
