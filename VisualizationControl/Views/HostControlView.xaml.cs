using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class HostControlView : UserControl
    {
        private static ScaleTransform flipTrans = new ScaleTransform(-1.0, 1.0);
        private static KeyGesture SwitchTabsKey = new KeyGesture(Key.F6);
        private const double TooltipCursorOffsetX = 20.0;
        private const double TooltipCursorOffsetY = 20.0;
        private HostControlViewModel viewModel;
        private readonly Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<HostControlView, object, PropertyChangedEventArgs> viewModelPropertyChanged;
        private readonly Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<HostControlView, object, PropertyChangedEventArgs> globePropertyChanged;
        private readonly Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<HostControlView, object, PropertyChangingEventArgs> viewModelPropertyChanging;
        private readonly WeakDelegate<HostControlView, int, int, BitmapSource> captureImage;
        private TabBranchHandler tabBranchHandler;
        public HostControlView()
        {
            this.InitializeComponent();
            this.viewModelPropertyChanged = new Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<HostControlView, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = new Action<HostControlView, object, PropertyChangedEventArgs>(HostControlView.OnViewModel_PropertyChanged)
            };
            this.viewModelPropertyChanging = new Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<HostControlView, object, PropertyChangingEventArgs>(this)
            {
                OnEventAction = new Action<HostControlView, object, PropertyChangingEventArgs>(HostControlView.OnViewModel_PropertyChanging)
            };
            this.globePropertyChanged = new Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<HostControlView, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = new Action<HostControlView, object, PropertyChangedEventArgs>(HostControlView.OnGlobePropertyChanged)
            };
            this.captureImage = new WeakDelegate<HostControlView, int, int, BitmapSource>(this)
            {
                OnInvokeAction = new Func<HostControlView, int, int, BitmapSource>(HostControlView.CaptureSceneImage)
            };
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.HostWindowDataContextChanged);
            this.Loaded += new RoutedEventHandler(this.HostControlView_Loaded);
        }

        private void AttachExplorationTabBranches()
        {
            List<UIElement> list = new List<UIElement>()
      {
        (UIElement) this.LayerPane,
        (UIElement) this.Ribbon,
        (UIElement) this.TourEditorPanel,
        (UIElement) this.DecoratorLayer,
        (UIElement) this.NavigationControls,
        (UIElement) this.Globe,
        (UIElement) this.StatusBar
      };
            Window window = Window.GetWindow((DependencyObject)this);
            if (window != null)
            {
                ChromelessWindowBase chromelessWindowBase = window as ChromelessWindowBase;
                if (chromelessWindowBase != null)
                    list.Add((UIElement)chromelessWindowBase.ChromeControls);
            }
            this.tabBranchHandler = new TabBranchHandler((FrameworkElement)this, (IReadOnlyList<UIElement>)list, HostControlView.SwitchTabsKey);
            this.tabBranchHandler.SwitchToTabBranch((UIElement)this.LayerPane);
        }

        private void AttachPlaybackTabBranches()
        {
            this.tabBranchHandler = new TabBranchHandler((FrameworkElement)this, (IReadOnlyList<UIElement>)new UIElement[2]
      {
        (UIElement) this.Globe,
        (UIElement) this.TourPlayerControl
      }, HostControlView.SwitchTabsKey);
            this.tabBranchHandler.SwitchToTabBranch((UIElement)this.Globe);
        }

        private void HostControlView_Loaded(object sender, RoutedEventArgs e)
        {
            this.AttachExplorationTabBranches();
        }

        private void HostWindowDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.CaptureSceneImage = (Func<int, int, BitmapSource>)null;
                this.viewModel.PropertyChanged -= new PropertyChangedEventHandler(this.viewModelPropertyChanged.OnEvent);
                this.viewModel.Globe.PropertyChanged -= new PropertyChangedEventHandler(this.globePropertyChanged.OnEvent);
                this.viewModel.PropertyChanging -= new PropertyChangingEventHandler(this.viewModelPropertyChanging.OnEvent);
            }
            this.viewModel = e.NewValue as HostControlViewModel;
            if (this.viewModel == null)
                return;
            this.viewModel.CaptureSceneImage = new Func<int, int, BitmapSource>(this.captureImage.OnInvoke);
            this.viewModel.PropertyChanged += new PropertyChangedEventHandler(this.viewModelPropertyChanged.OnEvent);
            this.viewModel.Globe.PropertyChanged += new PropertyChangedEventHandler(this.globePropertyChanged.OnEvent);
            this.viewModel.PropertyChanging += new PropertyChangingEventHandler(this.viewModelPropertyChanging.OnEvent);
        }

        private static void OnGlobePropertyChanged(HostControlView view, object sender, PropertyChangedEventArgs e)
        {
            if (view.viewModel.Mode != HostWindowMode.Exploration || !e.PropertyName.Contains(GlobeViewModel.PropertyShowContextMenu))
                return;
            if (view._contextMenu.Items.Count == 0)
                view._contextMenu.ItemsSource = (IEnumerable)view.viewModel.ContextCommands;
            view._contextMenu.Placement = PlacementMode.MousePoint;
            view._contextMenu.IsOpen = view.viewModel.Globe.ShowContextMenu;
        }

        private static void OnViewModel_PropertyChanged(HostControlView view, object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Contains(HostControlViewModel.PropertyMode) || view.viewModel == null)
                return;
            if (view.viewModel.Mode == HostWindowMode.Playback)
            {
                view.tabBranchHandler.Detach();
                view.AttachPlaybackTabBranches();
            }
            else
            {
                view.tabBranchHandler.Detach();
                view.AttachExplorationTabBranches();
            }
        }

        private static void OnViewModel_PropertyChanging(HostControlView view, object sender, PropertyChangingEventArgs e)
        {
            if (!e.PropertyName.Contains(HostControlViewModel.PropertyToolTipVisible))
                return;
            view.Tooltip.HorizontalOffset = view.Tooltip.HorizontalOffset + view.InnerTooltip.ActualWidth > view.BackGrid.ActualWidth ? view.BackGrid.ActualWidth - view.InnerTooltip.ActualWidth : view.Tooltip.HorizontalOffset;
            view.Tooltip.VerticalOffset = view.Tooltip.VerticalOffset + view.InnerTooltip.ActualHeight > view.BackGrid.ActualHeight ? view.BackGrid.ActualHeight - view.InnerTooltip.ActualHeight : view.Tooltip.VerticalOffset;
        }

        private static BitmapSource CaptureSceneImage(HostControlView view, int width, int height)
        {
            double actualWidth = view.BackGrid.ActualWidth;
            double actualHeight = view.BackGrid.ActualHeight;
            double num1 = (double)width / actualWidth;
            double num2 = (double)height / actualHeight;
            double num3 = num1 > num2 ? num1 : num2;
            double num4 = num1 > num2 ? (double)width : num3 * actualWidth;
            double num5 = num1 > num2 ? num3 * actualHeight : (double)height;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)num4, (int)num5, num3 * 96.0, num3 * 96.0, PixelFormats.Pbgra32);
            renderTargetBitmap.Render((Visual)view.BackGrid);
            BitmapSource bitmapSource = (BitmapSource)new CroppedBitmap((BitmapSource)renderTargetBitmap, num1 > num2 ? new Int32Rect(0, (int)((num5 - (double)height) / 2.0), width, height) : new Int32Rect((int)((num4 - (double)width) / 2.0), 0, width, height));
            if (view.BackGrid.FlowDirection == FlowDirection.RightToLeft)
            {
                TransformedBitmap transformedBitmap = new TransformedBitmap();
                transformedBitmap.BeginInit();
                transformedBitmap.Source = bitmapSource;
                transformedBitmap.Transform = (Transform)HostControlView.flipTrans;
                transformedBitmap.EndInit();
                if (transformedBitmap.CanFreeze)
                    transformedBitmap.Freeze();
                return (BitmapSource)transformedBitmap;
            }
            else
            {
                if (bitmapSource.CanFreeze)
                    bitmapSource.Freeze();
                return bitmapSource;
            }
        }

        private void CaptureScreen_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = HostControlView.CaptureSceneImage(this, (int)this.BackGrid.ActualWidth, (int)this.BackGrid.ActualHeight);
            JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
            jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(source));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                jpegBitmapEncoder.Save((Stream)memoryStream);
                using (System.Drawing.Image image = System.Drawing.Image.FromStream((Stream)memoryStream))
                    Clipboard.SetDataObject((object)image, true);
            }
        }

        private void MouseRightButton(object sender, MouseButtonEventArgs e)
        {
            this.viewModel.ToolTipVisible = false;
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.viewModel.ToolTipVisible && e.StylusDevice == null)
                return;
            System.Windows.Point position = e.GetPosition((IInputElement)this.BackGrid);
            this.Tooltip.HorizontalOffset = position.X + 20.0;
            this.Tooltip.VerticalOffset = position.Y + 20.0;
        }

        private void Tooltip_Opened(object sender, EventArgs e)
        {
            if (this.BackGrid.IsMouseOver)
                return;
            this.viewModel.ToolTipVisible = false;
        }

        private void BackGrid_Mouse(object sender, MouseEventArgs e)
        {
            if (this.viewModel.Mode != HostWindowMode.Playback)
                return;
            this.viewModel.TourPlayer.OptionsVisible = false;
            if (!this.viewModel.TourPlayer.TourPlayer.IsPlaying)
                return;
            this.viewModel.TourPlayer.PauseCommand.Execute((object)null);
        }

        private void BackGrid_Touch(object sender, TouchEventArgs e)
        {
            if (this.viewModel.Mode != HostWindowMode.Playback)
                return;
            this.viewModel.TourPlayer.OptionsVisible = false;
            if (!this.viewModel.TourPlayer.TourPlayer.IsPlaying)
                return;
            this.viewModel.TourPlayer.PauseCommand.Execute((object)null);
        }

        private void BackGrid_Key(object sender, KeyEventArgs e)
        {
            if (this.viewModel.Mode != HostWindowMode.Playback)
                return;
            switch (e.Key)
            {
                case Key.Escape:
                    if (this.viewModel.TourPlayer.OptionsVisible)
                    {
                        this.viewModel.TourPlayer.OptionsVisible = false;
                        e.Handled = true;
                        break;
                    }
                    else
                    {
                        this.viewModel.TourPlayer.ExitTourPlaybackModeCommand.Execute((object)null);
                        this.viewModel.TourPlayer.ControlsVisible = true;
                        break;
                    }
                case Key.Space:
                    if (this.viewModel.TourPlayer.TourPlayer.IsPlaying)
                        this.viewModel.TourPlayer.PauseCommand.Execute((object)null);
                    else
                        this.viewModel.TourPlayer.PlayCommand.Execute((object)null);
                    e.Handled = true;
                    break;
                case Key.Prior:
                case Key.MediaPreviousTrack:
                    if (!this.viewModel.TourPlayer.IsPreviousEnabled)
                        break;
                    this.viewModel.TourPlayer.PreviousSceneCommand.Execute((object)null);
                    break;
                case Key.Next:
                case Key.MediaNextTrack:
                    if (!this.viewModel.TourPlayer.IsNextEnabled)
                        break;
                    this.viewModel.TourPlayer.NextSceneCommand.Execute((object)null);
                    break;
            }
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (this.viewModel.Mode != HostWindowMode.Playback)
                return;
            e.Handled = true;
        }

        private void BackGrid_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.Tooltip.IsMouseOver)
                return;
            this.viewModel.ToolTipVisible = false;
        }

        private void MapTypesRibbonMenuButton_DropDownOpened(object sender, EventArgs e)
        {
            this.viewModel.TourEditor.UpdateSelectedScene();
            this.viewModel.UpdateMapTypesCommand.Execute((object)this);
            this.viewModel.MapTypesGalleryViewModel.GalleryClosing += (Action)(() => this.AddSceneSplitButton.IsDropDownOpen = false);
        }

        private void MapTypesRibbonMenuButton_DropDownClosed(object sender, EventArgs e)
        {
            this.viewModel.MapTypesGalleryViewModel.SetCurrentScene((Scene)null);
        }

        private void OnAddSceneSplitButtonRightMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnDecoratorMouseEnter(object sender, MouseEventArgs e)
        {
            this.viewModel.ToolTipVisible = false;
        }

    }
}
