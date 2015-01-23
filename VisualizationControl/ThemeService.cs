using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ThemeService : IThemeService
  {
    private int ThemeIndex;
    private VisualizationEngine engine;
    private WeakEventListener<ThemeService, BuiltinTheme, VisualizationTheme, bool> onThemeChanged;

    public IEnumerable<Color4F> ThemeColors
    {
      get
      {
        return Enumerable.SelectMany<double, Color4F, Color4F>((IEnumerable<double>) this.engine.CurrentVisualizationTheme.ColorStepsForVisuals, (Func<double, IEnumerable<Color4F>>) (s => (IEnumerable<Color4F>) this.engine.CurrentVisualizationTheme.ColorsForVisuals), (Func<double, Color4F, Color4F>) ((s, c) => Color4F.ApplyLightnessFactor(c, s) ?? new Color4F(1f, 1f, 1f, 1f)));
      }
    }

    public event Action OnThemeChanged;

    public ThemeService(VisualizationEngine engine)
    {
      this.engine = engine;
      this.onThemeChanged = new WeakEventListener<ThemeService, BuiltinTheme, VisualizationTheme, bool>(this)
      {
        OnEventAction = new Action<ThemeService, BuiltinTheme, VisualizationTheme, bool>(ThemeService.VisualizationEngineOnThemeChanged)
      };
      this.engine.ThemeChanged += new Action<BuiltinTheme, VisualizationTheme, bool>(this.onThemeChanged.OnEvent);
    }

    private static void VisualizationEngineOnThemeChanged(ThemeService sender, BuiltinTheme theme, VisualizationTheme vtheme, bool unused)
    {
      if (sender.OnThemeChanged == null)
        return;
      sender.OnThemeChanged();
    }
  }
}
