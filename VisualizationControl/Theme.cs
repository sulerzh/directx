using Microsoft.Data.Visualization.Engine;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public sealed class Theme
  {
    public static Theme Bing { get; private set; }

    public static Theme BingAerial { get; private set; }

    public static Theme BingAerialGrey { get; private set; }

    public static Theme Light { get; private set; }

    public static Theme Dark { get; private set; }

    public static Theme Mono { get; private set; }

    public static Theme White { get; private set; }

    public static Theme Earthy { get; private set; }

    public static Theme Modern { get; private set; }

    public static Theme Organic { get; private set; }

    public static Theme Radiate { get; private set; }

    public static Theme BingHighContrast { get; private set; }

    public string IconPath { get; private set; }

    public BuiltinTheme ThemeEnum { get; private set; }

    public string ToolTip { get; private set; }

    public static List<Theme> ThemesList { get; private set; }

    public bool IsAerial
    {
      get
      {
        if (this.ThemeEnum != BuiltinTheme.Aerial)
          return this.ThemeEnum == BuiltinTheme.Grey;
        else
          return true;
      }
    }

    static Theme()
    {
      Theme.ThemesList = new List<Theme>();
      Theme.Bing = new Theme(BuiltinTheme.BingRoad, "/VisualizationControl;component/Images/Bing.PNG", Resources.Theme_Bing);
      Theme.BingAerial = new Theme(BuiltinTheme.Aerial, "/VisualizationControl;component/Images/Aerial.PNG", Resources.Theme_Aerial);
      Theme.BingAerialGrey = new Theme(BuiltinTheme.Grey, "/VisualizationControl;component/Images/AerialGrey.PNG", Resources.Theme_AerialGrey);
      Theme.Light = new Theme(BuiltinTheme.Light, "/VisualizationControl;component/Images/Light.PNG", Resources.Theme_Light);
      Theme.Dark = new Theme(BuiltinTheme.Dark, "/VisualizationControl;component/Images/Dark.PNG", Resources.Theme_Dark);
      Theme.Mono = new Theme(BuiltinTheme.Mono, "/VisualizationControl;component/Images/Mono.PNG", Resources.Theme_Mono);
      Theme.White = new Theme(BuiltinTheme.White, "/VisualizationControl;component/Images/WhiteWash.PNG", Resources.Theme_Whitewash);
      Theme.Earthy = new Theme(BuiltinTheme.Earthy, "/VisualizationControl;component/Images/Earthy.PNG", Resources.Theme_Earthy);
      Theme.Modern = new Theme(BuiltinTheme.Modern, "/VisualizationControl;component/Images/Modern.PNG", Resources.Theme_Modern);
      Theme.Organic = new Theme(BuiltinTheme.Organic, "/VisualizationControl;component/Images/Organic.PNG", Resources.Theme_Organic);
      Theme.Radiate = new Theme(BuiltinTheme.Radiate, "/VisualizationControl;component/Images/Radiate.PNG", Resources.Theme_Radiate);
      Theme.BingHighContrast = new Theme(BuiltinTheme.BingRoadHighContrast, "/VisualizationControl;component/Images/HighContrast.PNG", Resources.Theme_HighContrast);
      Theme.ThemesList.Add(Theme.Bing);
      Theme.ThemesList.Add(Theme.BingAerial);
      Theme.ThemesList.Add(Theme.BingAerialGrey);
      Theme.ThemesList.Add(Theme.Light);
      Theme.ThemesList.Add(Theme.Dark);
      Theme.ThemesList.Add(Theme.Mono);
      Theme.ThemesList.Add(Theme.White);
      Theme.ThemesList.Add(Theme.Radiate);
      Theme.ThemesList.Add(Theme.Earthy);
      Theme.ThemesList.Add(Theme.Modern);
      Theme.ThemesList.Add(Theme.Organic);
      Theme.ThemesList.Add(Theme.BingHighContrast);
    }

    private Theme(BuiltinTheme theme, string iconResourcePath, string toolTip)
    {
      this.ThemeEnum = theme;
      this.IconPath = iconResourcePath;
      this.ToolTip = toolTip;
    }

    public static Theme FromThemeEnum(BuiltinTheme themeEnum)
    {
      foreach (Theme theme in Theme.ThemesList)
      {
        if (theme.ThemeEnum == themeEnum)
          return theme;
      }
      return (Theme) null;
    }
  }
}
