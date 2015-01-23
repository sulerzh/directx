using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class FieldListPickerView : UserControl
    {

        public FieldListPickerView()
        {
            this.InitializeComponent();
        }

        private void ChooseGeoFieldsView_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                return;
            this.MoveFocusOnViewChange((UIElement)this.ChooseGeoFieldsView, (UIElement)this.ChooseVisFieldView);
        }

        private void ChooseVisFieldView_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                return;
            this.MoveFocusOnViewChange((UIElement)this.ChooseVisFieldView, (UIElement)this.ChooseGeoFieldsView);
        }

        private void MoveFocusOnViewChange(UIElement oldView, UIElement newView)
        {
            if (oldView.IsVisible || !newView.IsVisible)
                return;
            Keyboard.Focus((IInputElement)newView.PredictFocus(FocusNavigationDirection.Down));
        }
    }
}
