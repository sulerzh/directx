using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class AnnotationImageSource : IAnnotationImageSource
    {
        private LayerDataBinding layerDataBinding;
        private Thread renderingThread;

        public AnnotationImageSource(LayerDataBinding layerDataBinding, Thread renderingThread)
        {
            this.layerDataBinding = layerDataBinding;
            this.renderingThread = renderingThread;
        }

        public Image GetAnnotationImage(InstanceId id, bool isSummary)
        {
            AnnotationPreviewView previewControl = null;
            ApplyDataModelDelegate dataModelDelegate = this.ApplyDataModel;
            MeasureControlDelegate measureControlDelegate = this.MeasureControl;
            ArrangeControlDelegate arrangeControlDelegate = this.ArrangeControl;
            RenderAnnotationImageDelegate annotationImageDelegate = this.RenderAnnotationImage;
            if (this.renderingThread == null)
                return null;
            Action action = delegate
            {
                previewControl = new AnnotationPreviewView();
                previewControl.FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
            };
            Dispatcher.FromThread(this.renderingThread).Invoke(action, DispatcherPriority.Background, null);
            if (!(bool)previewControl.Dispatcher.Invoke(dataModelDelegate, DispatcherPriority.Normal, new object[]{id,previewControl}))
                return null;
            previewControl.Dispatcher.Invoke(measureControlDelegate, DispatcherPriority.Render, new object[]{previewControl});
            previewControl.Dispatcher.Invoke(arrangeControlDelegate, DispatcherPriority.Loaded, new object[]{previewControl});
            return previewControl.Dispatcher.Invoke(annotationImageDelegate, DispatcherPriority.Input, new object[]{previewControl}) as Image;
        }

        private bool ApplyDataModel(InstanceId id, AnnotationPreviewView previewControl)
        {
            AnnotationTemplateModel annotation = this.layerDataBinding.GetAnnotation(id);
            if (annotation == null)
                return false;
            if (annotation != null)
            {
                DataRowModel model = new DataRowModel(this.layerDataBinding.GeoDataView.TableColumnsWithValuesForId(id, false, new DateTime?()), this.layerDataBinding.GetAnyMeasure());
                annotation.Apply(model);
            }
            previewControl.DataContext = annotation;
            return true;
        }

        private void MeasureControl(AnnotationPreviewView previewControl)
        {
            Size availableSize = new Size(512.0, 512.0);
            previewControl.Measure(availableSize);
        }

        private void ArrangeControl(AnnotationPreviewView previewControl)
        {
            previewControl.Arrange(new System.Windows.Rect(previewControl.DesiredSize));
        }

        private Image RenderAnnotationImage(AnnotationPreviewView previewControl)
        {
            int num1 = (int)previewControl.ActualWidth;
            int num2 = (int)previewControl.ActualHeight;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(num1, num2, 96.0, 96.0, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(previewControl);
            previewControl.DataContext = null;
            int num3 = num1 * num2 * 4;
            IntPtr num4 = Marshal.AllocHGlobal(num3);
            BitmapFrame.Create(renderTargetBitmap).CopyPixels(new Int32Rect(0, 0, num1, num2), num4, num3, num1 * 4);
            return new Image(num4, num1, num2, Microsoft.Data.Visualization.Engine.Graphics.PixelFormat.Bgra32Bpp);
        }

        private delegate bool ApplyDataModelDelegate(InstanceId id, AnnotationPreviewView previewControl);

        private delegate void MeasureControlDelegate(AnnotationPreviewView previewControl);

        private delegate void ArrangeControlDelegate(AnnotationPreviewView previewControl);

        private delegate Image RenderAnnotationImageDelegate(AnnotationPreviewView previewControl);
    }
}
