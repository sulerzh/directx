using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class ChromelessWindowBase : Window
    {
        private WindowViewModel _viewModel;
        private readonly Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<ChromelessWindowBase, object, PropertyChangedEventArgs> viewModelPropertyChanged;
        private IInputElement _previousFocusedElement;
        [ThreadStatic]
        private static ThemeResourceDictionary _mainThemeDictionary;
        [ThreadStatic]
        private static ThemeResourceDictionary _staticallyThemedTemplatesDictionary;

        public static ThemeResourceDictionary StaticallyThemedTemplatesDictionary
        {
            get
            {
                if (ChromelessWindowBase._staticallyThemedTemplatesDictionary == null)
                    ChromelessWindowBase.InitThemedDictionaries();
                return ChromelessWindowBase._staticallyThemedTemplatesDictionary;
            }
        }

        public static ThemeResourceDictionary MainThemeDictionary
        {
            get
            {
                if (ChromelessWindowBase._mainThemeDictionary == null)
                    ChromelessWindowBase.InitThemedDictionaries();
                return ChromelessWindowBase._mainThemeDictionary;
            }
        }

        public FrameworkElement ChromeControls
        {
            get
            {
                return (FrameworkElement)this.ChromeBar;
            }
        }

        public ChromelessWindowBase()
        {
            this.InitializeComponent();
            this.viewModelPropertyChanged = new Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<ChromelessWindowBase, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = new Action<ChromelessWindowBase, object, PropertyChangedEventArgs>(ChromelessWindowBase.OnViewModelPropertyChanged)
            };
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this._viewModel != null)
            {
                this._viewModel.PropertyChanged -= new PropertyChangedEventHandler(this.viewModelPropertyChanged.OnEvent);
                if (this._viewModel.Window == this)
                    this._viewModel.Window = (Window)null;
            }
            this._viewModel = e.NewValue as WindowViewModel;
            if (this._viewModel == null)
                return;
            this._viewModel.PropertyChanged += new PropertyChangedEventHandler(this.viewModelPropertyChanged.OnEvent);
            this._viewModel.Window = (Window)this;
            this.Width = this._viewModel.Width;
            this.Height = this._viewModel.Height;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                if (this.ResizeMode != ResizeMode.CanResize)
                    return;
                this.MaximizeBorder.Margin = new Thickness(6.0);
            }
            else
                this.MaximizeBorder.Margin = new Thickness(0.0);
        }

        private static void OnViewModelPropertyChanged(ChromelessWindowBase windowBase, object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != WindowViewModel.PropertyDialog || windowBase._viewModel.Dialog == null)
                return;
            DispatcherExtensions.CheckedInvoke(windowBase.Dispatcher,
                (Action)(() => FocusManager.SetFocusedElement((DependencyObject)windowBase.dialogContentControl, (IInputElement)windowBase.dialogContentControl)), false);
        }

        private static void InitThemedDictionaries()
        {
            ChromelessWindowBase._mainThemeDictionary = new ThemeResourceDictionary()
            {
                StandardThemeSource = new Uri("/VisualizationControl;component/Themes/Standard.xaml", UriKind.Relative),
                HighContrastThemeSource = new Uri("/VisualizationControl;component/Themes/HighContrast.xaml", UriKind.Relative)
            };
            ChromelessWindowBase._staticallyThemedTemplatesDictionary = new ThemeResourceDictionary()
            {
                StandardThemeSource = new Uri("/VisualizationControl;component/Themes/StaticallyThemedTemplates.xaml", UriKind.Relative),
                HighContrastThemeSource = new Uri("/VisualizationControl;component/Themes/StaticallyThemedTemplates.xaml", UriKind.Relative)
            };
        }

        private void dialogContentControl_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (this._viewModel.Dialog != null)
            {
                if (this.dialogContentControl == null)
                    return;
                this.dialogContentControl.ApplyTemplate();
                if (VisualTreeHelper.GetChildrenCount((DependencyObject)this.dialogContentControl) < 1)
                    return;
                var contentPresenter = VisualTreeHelper.GetChild((DependencyObject)this.dialogContentControl, 0) as ContentPresenter;
                if (contentPresenter == null)
                    return;
                contentPresenter.ApplyTemplate();
                if (VisualTreeHelper.GetChildrenCount((DependencyObject)contentPresenter) < 1)
                    return;
                var frameworkElement = VisualTreeHelper.GetChild((DependencyObject)contentPresenter, 0) as FrameworkElement;
                if (frameworkElement == null)
                    return;
                frameworkElement.Loaded += new RoutedEventHandler(this.dialog_Loaded);
            }
            else
            {
                if (this._previousFocusedElement == null)
                    return;
                try
                {
                    FocusManager.SetFocusedElement((DependencyObject)this, this._previousFocusedElement);
                }
                catch
                {
                }
                this._previousFocusedElement = (IInputElement)null;
            }
        }

        private void dialog_Loaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            try
            {
                var element = FocusManager.GetFocusedElement((DependencyObject)frameworkElement);
                if (element == null)
                {
                    frameworkElement.Focusable = true;
                    element = (IInputElement)frameworkElement;
                }
                this._previousFocusedElement = FocusManager.GetFocusedElement((DependencyObject)this);
                Keyboard.Focus(element);
                FocusManager.SetFocusedElement((DependencyObject)this, element);
                frameworkElement.Loaded -= new RoutedEventHandler(this.dialog_Loaded);
            }
            catch
            {
                this._previousFocusedElement = (IInputElement)null;
            }
        }

    }
}