using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class PopupAggregationButtonView : UserControl
    {
        private ToggleButton callingControl;
        private bool mousePressed;
        public PopupAggregationButtonView()
        {
            this.InitializeComponent();
        }

        private void Selector_OnSelected(object sender, RoutedEventArgs e)
        {
            this.FunctionPopup.IsOpen = false;
        }

        private void FilterFnButton_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (this.mousePressed)
                return;
            this.callingControl = sender as ToggleButton;
            this.FunctionPopupListbox.Focus();
        }

        private void FunctionPopupListbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.FunctionPopup.IsOpen = false;
                if (this.callingControl != null)
                    this.callingControl.Focus();
            }
            else if (e.Key == Key.Return)
            {
                foreach (object obj in (IEnumerable)this.FunctionPopupListbox.Items)
                {
                    ListBoxItem listBoxItem = (ListBoxItem)this.FunctionPopupListbox.ItemContainerGenerator.ContainerFromItem(obj);
                    if (listBoxItem != null && listBoxItem.IsKeyboardFocusWithin)
                    {
                        this.FunctionPopupListbox.SelectedItem = obj;
                        break;
                    }
                }
            }
            e.Handled = true;
        }

        private void FunctionPopupListbox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.mousePressed = true;
        }

        private void FilterFnButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mousePressed = false;
        }

    }
}
