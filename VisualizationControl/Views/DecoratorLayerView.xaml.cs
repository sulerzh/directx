using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class DecoratorLayerView : UserControl
    {
        private DecoratorLayerViewModel viewModel;
        private readonly Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<DecoratorLayerView, object, NotifyCollectionChangedEventArgs> decoratorsOnCollectionChanged;
        private readonly Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<DecoratorLayerView, object, PropertyChangedEventArgs> decoratorsOnPropertyChanged;
        private int readyToUpdateImage;

        public DecoratorLayerView()
        {
            this.InitializeComponent();
            this.decoratorsOnPropertyChanged = new Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<DecoratorLayerView, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = new Action<DecoratorLayerView, object, PropertyChangedEventArgs>(DecoratorLayerView.DecoratorsPropertyChanged)
            };
            this.decoratorsOnCollectionChanged = new Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<DecoratorLayerView, object, NotifyCollectionChangedEventArgs>(this)
            {
                OnEventAction = new Action<DecoratorLayerView, object, NotifyCollectionChangedEventArgs>(DecoratorLayerView.DecoratorsCollectionChanged)
            };
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.DecoratorLayerDataContextChanged);
        }

        private void DecoratorLayerDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            this.viewModel = eventArgs.NewValue as DecoratorLayerViewModel;
            if (this.viewModel == null)
                return;
            this.viewModel.Decorators.Decorators.CollectionChanged += new NotifyCollectionChangedEventHandler(this.decoratorsOnCollectionChanged.OnEvent);
            this.viewModel.Decorators.Decorators.ItemPropertyChanged += new ObservableCollectionExItemChangedHandler<DecoratorViewModel>(this.decoratorsOnPropertyChanged.OnEvent);
            this.viewModel.Decorators.Decorators.ItemDescendentPropertyChanged += new ObservableCollectionExItemChangedHandler<DecoratorViewModel>(this.decoratorsOnPropertyChanged.OnEvent);
        }

        private static void DecoratorsPropertyChanged(DecoratorLayerView view, object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (Interlocked.CompareExchange(ref view.readyToUpdateImage, 1, 0) != 0)
                return;
            view.Dispatcher.BeginInvoke((Delegate)new Action(view.UpdateDecoratorLayerImage), DispatcherPriority.Render, (object[])null);
        }

        private static void DecoratorsCollectionChanged(DecoratorLayerView view, object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (Interlocked.CompareExchange(ref view.readyToUpdateImage, 1, 0) != 0)
                return;
            view.Dispatcher.BeginInvoke((Delegate)new Action(view.UpdateDecoratorLayerImage), DispatcherPriority.Render, (object[])null);
        }

        private void UpdateDecoratorLayerImage()
        {
            if (this.viewModel.DecoratorImageWidth == 0 || this.viewModel.DecoratorImageHeight == 0)
                return;
            Action action = delegate
            {
                this.viewModel.DecoratorImage = this.CaptureDecoratorLayerImage(this.viewModel.DecoratorImageWidth,
                    this.viewModel.DecoratorImageHeight);
                Interlocked.Exchange(ref this.readyToUpdateImage, 0);
            };
            this.Dispatcher.BeginInvoke(action, DispatcherPriority.Background, (object[])null);
        }

        private BitmapSource CaptureDecoratorLayerImage(int width, int height)
        {
            double actualWidth = this.DecoratorsContent.ActualWidth;
            double actualHeight = this.DecoratorsContent.ActualHeight;
            double num1 = (double)width / actualWidth;
            double num2 = (double)height / actualHeight;
            double num3 = num1 > num2 ? num1 : num2;
            double num4 = num1 > num2 ? (double)width : num3 * actualWidth;
            double num5 = num1 > num2 ? num3 * actualHeight : (double)height;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)num4, (int)num5, num3 * 96.0, num3 * 96.0, PixelFormats.Pbgra32);
            renderTargetBitmap.Render((Visual)this.DecoratorsContent);
            BitmapSource bitmapSource = (BitmapSource)new CroppedBitmap((BitmapSource)renderTargetBitmap, num1 > num2 ? new Int32Rect(0, (int)((num5 - (double)height) / 2.0), width, height) : new Int32Rect((int)((num4 - (double)width) / 2.0), 0, width, height));
            if (bitmapSource.CanFreeze)
                bitmapSource.Freeze();
            return bitmapSource;
        }
    }
}
