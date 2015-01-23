using Microsoft.Data.Visualization.WpfExtensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class DecoratorView : UserControl
    {
        private ControlState _startState;
        private bool _resizing;
        private DecoratorViewModel _viewModel;

        public DecoratorViewModel ViewModel
        {
            get
            {
                if (this._viewModel == null)
                    this._viewModel = this.DataContext as DecoratorViewModel;
                return this._viewModel;
            }
        }

        public DecoratorView()
        {
            this.InitializeComponent();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            this.ViewModel.UpdateDragDelta(e.HorizontalChange, e.VerticalChange);
        }

        private void Grip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == this.TopLeftGrip)
                this.ViewModel.ResizeSource = ResizeSource.TopLeft;
            if (sender == this.BottomRightGrip)
                this.ViewModel.ResizeSource = ResizeSource.BottomRight;
            if (e.ClickCount != 1)
                return;
            (sender as FrameworkElement).CaptureMouse();
            this.StartResize();
            e.Handled = true;
        }

        private void Grip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement.IsMouseCaptured)
            {
                frameworkElement.ReleaseMouseCapture();
                this._resizing = false;
                e.Handled = true;
            }
            this.ViewModel.ResizeSource = ResizeSource.None;
        }

        private void Grip_MouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null || !frameworkElement.IsMouseCaptured)
                return;
            this.UpdateSize();
            e.Handled = true;
        }

        private void Grip_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            this.AutoSize();
            e.Handled = true;
        }

        private void StartResize()
        {
            if (this.ViewModel == null)
                return;
            this._startState = new ControlState()
            {
                Origin = FrameworkElementExtensions.GetMouseScreenPosition((FrameworkElement)this),
                Height = this.ActualHeight,
                Width = this.ActualWidth,
                X = this.ViewModel.Model.X,
                Y = this.ViewModel.Model.Y
            };
            this._resizing = true;
        }

        private void UpdateSize()
        {
            if (!this._resizing || this.ViewModel == null)
                return;
            DecoratorModel model = this.ViewModel.Model;
            Point mouseScreenPosition = FrameworkElementExtensions.GetMouseScreenPosition((FrameworkElement)this);
            double num1 = mouseScreenPosition.X - this._startState.Origin.X;
            if (this.FlowDirection == FlowDirection.RightToLeft)
                num1 *= -1.0;
            double num2 = mouseScreenPosition.Y - this._startState.Origin.Y;
            if (this.ViewModel.ResizeSource == ResizeSource.BottomRight)
            {
                double x = model.X;
                double y = model.Y;
                double width = this._startState.Width + num1;
                double height = this._startState.Height + num2;
                this.ViewModel.ValidateNewSizeCallback(ref x, ref y, ref height, ref width, this.ViewModel.ResizeSource);
                model.Width = width;
                model.Height = height;
            }
            if (this.ViewModel.ResizeSource != ResizeSource.TopLeft)
                return;
            double x1 = this._startState.X + num1;
            double y1 = this._startState.Y + num2;
            double width1 = this._startState.Width - num1;
            double height1 = this._startState.Height - num2;
            this.ViewModel.ValidateNewSizeCallback(ref x1, ref y1, ref height1, ref width1, this.ViewModel.ResizeSource);
            model.X = x1;
            model.Y = y1;
            model.Width = width1;
            model.Height = height1;
        }

        private void AutoSize()
        {
            if (this.ViewModel == null)
                return;
            this.ViewModel.Model.AutoSize();
        }

        private void ContentMouseEnter(object sender, MouseEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            this.ViewModel.ShowAdornerUI = true;
        }

        private void ContentMouseLeave(object sender, MouseEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            this.ViewModel.ShowAdornerUI = false;
        }

        private void DecoratorIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            this.ViewModel.ShowAdornerUI = (bool)e.NewValue || this.IsMouseOver;
        }

        private void DecoratorLoaded(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null || !this.ViewModel.Model.SetFocusOnLoadView)
                return;
            this.Body.Focus();
            this.ViewModel.Model.SetFocusOnLoadView = false;
        }
    }
}
