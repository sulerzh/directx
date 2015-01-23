using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class TourEditorView : UserControl
    {
        public TourEditorView()
        {
            this.InitializeComponent();
        }

        private void MouseClick(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus((IInputElement)this.ScenesListBox);
        }

        private void ListBoxItemKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                case Key.Space:
                    ((ListBoxItem)sender).IsSelected = true;
                    e.Handled = true;
                    break;
                case Key.Up:
                    ((UIElement)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    e.Handled = true;
                    break;
                case Key.Down:
                    ((UIElement)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    e.Handled = true;
                    break;
            }
        }

        private void ListBoxItemKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;
            ListBoxItem listBoxItem = (ListBoxItem)sender;
            SceneViewModel sceneViewModel = (SceneViewModel)listBoxItem.DataContext;
            FocusNavigationDirection focusNavigationDirection = listBoxItem.PredictFocus(FocusNavigationDirection.Down) is ListBoxItem ? FocusNavigationDirection.Down : FocusNavigationDirection.Up;
            listBoxItem.MoveFocus(new TraversalRequest(focusNavigationDirection));
            sceneViewModel.DeleteCommand.Execute((object)sceneViewModel);
            e.Handled = true;
        }

        private void ListBoxItemGotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

    }
}
