using System;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class ThemeResourceDictionary : ResourceDictionary
  {
    private ResourceDictionary themeDictionary;
    private ResourceDictionary staticallyThemedTemplateDictionary;
    private bool isHighContrast;
    private Color currentHighlightColor;
    private Uri _HighContrastThemeSource;
    private Uri _StandardThemeSource;

    public Uri HighContrastThemeSource
    {
      get
      {
        return this._HighContrastThemeSource;
      }
      set
      {
        this._HighContrastThemeSource = value;
        if (!SystemParameters.HighContrast)
          return;
        this.ChangeThemes(value);
      }
    }

    public Uri StandardThemeSource
    {
      get
      {
        return this._StandardThemeSource;
      }
      set
      {
        this._StandardThemeSource = value;
        if (SystemParameters.HighContrast)
          return;
        this.ChangeThemes(value);
      }
    }

    private void ChangeThemes(ResourceDictionary newThemeDictionary)
    {
      this.MergedDictionaries.Remove(this.themeDictionary);
      this.themeDictionary = newThemeDictionary;
      this.MergedDictionaries.Add(this.themeDictionary);
    }

    private void ChangeThemes(Uri themeSource)
    {
      this.ChangeThemes(new ResourceDictionary()
      {
        Source = themeSource
      });
    }

    private void ChangeThemes()
    {
      this.isHighContrast = SystemParameters.HighContrast;
      this.currentHighlightColor = SystemColors.HighlightColor;
      this.ChangeThemes(SystemParameters.HighContrast ? this.HighContrastThemeSource : this.StandardThemeSource);
    }
  }
}
