using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsTerm_Universal
{
    public static class WebViewHelper
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
        "Html", typeof(string), typeof(WebViewHelper), new PropertyMetadata(0, new PropertyChangedCallback(OnHtmlChanged)));

        public static string GetHtml(DependencyObject dependencyObject)
        {
            return (string)dependencyObject.GetValue(HtmlProperty);
        }

        public static void SetHtml(DependencyObject dependencyObject, string value)
        {
            dependencyObject.SetValue(HtmlProperty, value);
        }

        private static async void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webview = d as WebView;

            if (webview == null)
                return;

           

            var hammer_lib_file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\hammer.min.js");
            var hammer_shim_file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\hammer_shim.js");

            var hammer_lib_text = await Windows.Storage.FileIO.ReadTextAsync(hammer_lib_file);
            var hammer_shim_text = await Windows.Storage.FileIO.ReadTextAsync(hammer_shim_file);

            var html = e.NewValue as String;
            html = String.Format("<script type=\"text/javascript\">{0}</script><script type=\"text/javascript\">{1}</script><div id=nextcloud_container>{2}</div>", hammer_lib_text, hammer_shim_text, html);

            webview.NavigateToString(html);
        }
    }
}
