using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface IThemeService
  {
    IEnumerable<Color4F> ThemeColors { get; }

    event Action OnThemeChanged;
  }
}
