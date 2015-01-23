using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public static class WebBrowserEx
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(string), typeof(WebBrowserEx), (PropertyMetadata)new UIPropertyMetadata((object)null, new PropertyChangedCallback(WebBrowserEx.SourcePropertyChanged)));

        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(WebBrowserEx.SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(WebBrowserEx.SourceProperty, (object)value);
        }

        public static void SourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser webBrowser = o as WebBrowser;
            if (webBrowser == null)
                return;
            string uriString = e.NewValue as string;
            webBrowser.Source = uriString != null ? new Uri(uriString) : (Uri)null;
        }
    }
}
