using System;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xaml;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class MultiDpiPngImageExtension : MarkupExtension
  {
    private static Uri baseUri = new Uri("pack://application:,,,/");
    private readonly string basePath;

    public MultiDpiPngImageExtension(string path)
    {
      this.basePath = path;
    }

    private static string GetDpiSubpath(double dpi)
    {
      return dpi <= 100.0 ? "100" : "200";
    }

    private Uri ResolveImageUri(double dpi)
    {
      return new Uri(MultiDpiPngImageExtension.baseUri, Path.Combine(Path.GetDirectoryName(this.basePath), MultiDpiPngImageExtension.GetDpiSubpath(dpi), Path.GetFileName(this.basePath)));
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      IRootObjectProvider rootObjectProvider = (IRootObjectProvider) serviceProvider.GetService(typeof (IRootObjectProvider));
      double dpi = WindowsDisplayProperties.GetDpi();
      try
      {
        return (object) new PngBitmapDecoder(this.ResolveImageUri(dpi), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
      }
      catch (Exception ex)
      {
      }
      try
      {
        return (object) new PngBitmapDecoder(new Uri(MultiDpiPngImageExtension.baseUri, this.basePath), BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
      }
      catch (Exception ex)
      {
      }
      return (object) null;
    }
  }
}
