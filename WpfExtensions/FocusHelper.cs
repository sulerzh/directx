using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public static class FocusHelper
    {
        public static readonly DependencyProperty AdvanceOnEnterKeyProperty = DependencyProperty.RegisterAttached("AdvanceOnEnterKey", typeof(bool), typeof(FocusHelper), (PropertyMetadata)new UIPropertyMetadata(new PropertyChangedCallback(FocusHelper.AdvanceOnEnterKeyPropertyChanged)));
        public static readonly DependencyProperty FocusFirstElementProperty = DependencyProperty.RegisterAttached("FocusFirstElement", typeof(bool), typeof(FocusHelper), (PropertyMetadata)new UIPropertyMetadata(new PropertyChangedCallback(FocusHelper.FocusFirstElementPropertyChanged)));
        public static readonly DependencyProperty ActivateOnEnterOrSpacebarProperty = DependencyProperty.RegisterAttached("ActivateOnEnterOrSpacebar", typeof(bool), typeof(FocusHelper), (PropertyMetadata)new UIPropertyMetadata(new PropertyChangedCallback(FocusHelper.ActivateOnEnterOrSpacebarPropertyChanged)));
        public static readonly DependencyProperty AttachedInputBindingsProperty = DependencyProperty.RegisterAttached("AttachedInputBindings", typeof(InputBindingCollection), typeof(FocusHelper), (PropertyMetadata)new UIPropertyMetadata(new PropertyChangedCallback(FocusHelper.AttachedInputBindingsPropertyChanged)));

        public static bool GetAdvanceOnEnterKey(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusHelper.AdvanceOnEnterKeyProperty);
        }

        public static void SetAdvanceOnEnterKey(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusHelper.AdvanceOnEnterKeyProperty, value);
        }

        private static void AdvanceOnEnterKeyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            UIElement uiElement = obj as UIElement;
            if (uiElement == null)
                return;
            if ((bool)e.NewValue)
                uiElement.KeyDown += new KeyEventHandler(FocusHelper.Element_KeyDown);
            else
                uiElement.KeyDown -= new KeyEventHandler(FocusHelper.Element_KeyDown);
        }

        private static void Element_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.Equals((object)Key.Return))
                return;
            ((UIElement)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public static bool GetFocusFirstElement(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusHelper.FocusFirstElementProperty);
        }

        public static void SetFocusFirstElement(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusHelper.FocusFirstElementProperty, value);
        }

        private static void FocusFirstElementPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement frameworkElement = (FrameworkElement)obj;
            if ((bool)args.NewValue)
                frameworkElement.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(FocusHelper.Element_Focused);
            else
                frameworkElement.GotKeyboardFocus -= new KeyboardFocusChangedEventHandler(FocusHelper.Element_Focused);
        }

        private static void Element_Focused(object sender, RoutedEventArgs e)
        {
            IInputElement focusedElement = Keyboard.FocusedElement;
            ((UIElement)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public static bool GetActivateOnEnterOrSpacebar(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusHelper.ActivateOnEnterOrSpacebarProperty);
        }

        public static void SetActivateOnEnterOrSpacebar(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusHelper.ActivateOnEnterOrSpacebarProperty, value);
        }

        private static void ActivateOnEnterOrSpacebarPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            IInputElement inputElement = (IInputElement)obj;
            if ((bool)args.NewValue)
            {
                inputElement.PreviewKeyDown += new KeyEventHandler(FocusHelper.ActivateOnKey_PreviewKeyDownHandler);
                inputElement.PreviewKeyUp += new KeyEventHandler(FocusHelper.ActivateOnKey_PreviewKeyUpHandler);
            }
            else
            {
                inputElement.PreviewKeyDown -= new KeyEventHandler(FocusHelper.ActivateOnKey_PreviewKeyDownHandler);
                inputElement.PreviewKeyUp -= new KeyEventHandler(FocusHelper.ActivateOnKey_PreviewKeyUpHandler);
            }
        }

        private static void ActivateOnKey_PreviewKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space && e.Key != Key.Return || !(sender is ComboBox) && !(sender is Hyperlink))
                return;
            e.Handled = true;
        }

        private static void ActivateOnKey_PreviewKeyUpHandler(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space && e.Key != Key.Return)
                return;
            if (sender is ComboBox)
            {
                ComboBox comboBox = sender as ComboBox;
                if (comboBox.IsDropDownOpen)
                {
                    ComboBoxItem comboBoxItem = Keyboard.FocusedElement as ComboBoxItem;
                    if (comboBoxItem != null)
                    {
                        comboBox.SelectedItem = comboBoxItem.DataContext;
                        comboBox.IsDropDownOpen = false;
                    }
                }
                else
                    comboBox.IsDropDownOpen = true;
                e.Handled = true;
            }
            else
            {
                if (!(sender is Hyperlink))
                    return;
                (sender as Hyperlink).DoClick();
            }
        }

        public static InputBindingCollection GetAttachedInputBindings(DependencyObject obj)
        {
            return (InputBindingCollection)obj.GetValue(FocusHelper.AttachedInputBindingsProperty);
        }

        public static void SetAttachedInputBindings(DependencyObject obj, InputBindingCollection value)
        {
            obj.SetValue(FocusHelper.AttachedInputBindingsProperty, (object)value);
        }

        private static void AttachedInputBindingsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement frameworkElement = (FrameworkElement)obj;
            if (args.OldValue != null)
            {
                foreach (InputBinding inputBinding in Enumerable.Cast<InputBinding>((IEnumerable)args.OldValue))
                    frameworkElement.InputBindings.Remove(inputBinding);
            }
            InputBindingCollection bindingCollection = (InputBindingCollection)args.NewValue;
            frameworkElement.InputBindings.AddRange((ICollection)bindingCollection);
        }
    }
}
