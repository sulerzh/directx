using System;
using System.Drawing.Imaging;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  internal class VideoImageSource
  {
    private const int Dpi = 96;
    private readonly HostControlViewModel model;
    private readonly RenderTargetBitmap renderTargetBitmap;
    private readonly CreateVideoTaskView previewControl;
    private readonly DrawingVisual dv;
    private readonly Dispatcher dispatcher;
    private readonly BitmapData data;
    private readonly Rect controlSize;

    public VideoImageSource(HostControlViewModel model, BitmapData data, Dispatcher dispatcher, double scale)
    {
      this.data = data;
      this.model = model;
      this.dispatcher = dispatcher;
      CreateVideoTaskView createVideoTaskView = new CreateVideoTaskView();
      createVideoTaskView.DataContext = (object) model;
      createVideoTaskView.FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
      this.previewControl = createVideoTaskView;
      this.renderTargetBitmap = new RenderTargetBitmap(data.Width, data.Height, 96.0, 96.0, PixelFormats.Pbgra32);
      this.dv = new DrawingVisual();
      Size size = new Size((double) data.Width, (double) data.Height);
      if (scale > 1.0)
      {
        this.dv.Transform = (Transform) new ScaleTransform(1.0 / scale, 1.0 / scale);
        size.Width *= scale;
        size.Height *= scale;
      }
      this.controlSize = new Rect(size);
      this.previewControl.Measure(size);
      this.previewControl.Arrange(this.controlSize);
    }

    public bool Fill()
    {
      this.DoEvents();
      return this.dispatcher.Invoke<bool>((Func<bool>) (() =>
      {
        using (DrawingContext drawingContext = this.dv.RenderOpen())
        {
          if (!this.model.Globe.AdvanceRecordingFrame())
            return false;
          VisualBrush visualBrush = new VisualBrush((Visual) this.previewControl);
          drawingContext.DrawRectangle((Brush) visualBrush, (Pen) null, this.controlSize);
        }
        this.renderTargetBitmap.Clear();
        this.renderTargetBitmap.Render((Visual) this.dv);
        this.renderTargetBitmap.CopyPixels(new Int32Rect(0, 0, this.data.Width, this.data.Height), this.data.Scan0, this.data.Height * this.data.Stride, this.data.Stride);
        return true;
      }), DispatcherPriority.Render);
    }

    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public void DoEvents()
    {
      DispatcherFrame frame = new DispatcherFrame();
        Func<object, object> task = delegate(object f)
        {
            ((DispatcherFrame) f).Continue = false;
            return null;
        };
      Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, task, (object) frame);
      Dispatcher.PushFrame(frame);
    }
  }
}
