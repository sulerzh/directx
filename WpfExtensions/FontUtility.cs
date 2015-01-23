using System.Collections.Generic;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public static class FontUtility
  {
    private static ICollection<string> _systemFontStrings;

    public static ICollection<string> SystemFontStrings
    {
      get
      {
        if (FontUtility._systemFontStrings == null)
        {
          List<string> list = new List<string>();
          foreach (FontFamily fontFamily in (IEnumerable<FontFamily>) Fonts.SystemFontFamilies)
            list.Add(fontFamily.Source);
          list.Sort();
          FontUtility._systemFontStrings = (ICollection<string>) list;
        }
        return FontUtility._systemFontStrings;
      }
    }
  }
}
