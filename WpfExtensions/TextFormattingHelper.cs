using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public static class TextFormattingHelper
    {
        public static readonly DependencyProperty AllCapsProperty = DependencyProperty.RegisterAttached("AllCaps", typeof(bool), typeof(TextFormattingHelper), (PropertyMetadata)new UIPropertyMetadata(new PropertyChangedCallback(TextFormattingHelper.AllCapsPropertyChanged)));

        public static bool GetAllCaps(DependencyObject obj)
        {
            return (bool)obj.GetValue(TextFormattingHelper.AllCapsProperty);
        }

        public static void SetAllCaps(DependencyObject obj, bool value)
        {
            obj.SetValue(TextFormattingHelper.AllCapsProperty, value);
        }

        private static void AllCapsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = obj as TextBlock;
            if (textBlock == null)
                return;
            if ((bool)e.NewValue)
                textBlock.Loaded += new RoutedEventHandler(TextFormattingHelper.TextBlock_Loaded);
            else
                textBlock.Loaded -= new RoutedEventHandler(TextFormattingHelper.TextBlock_Loaded);
        }

        private static void TextBlock_Loaded(object sender, EventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            textBlock.Text = textBlock.Text.ToUpper(CultureInfo.CurrentUICulture);
        }
    }
}
