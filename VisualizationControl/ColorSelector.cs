using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ColorSelector
  {
    private const int InitialIndex = -1;
    private int colorIndex;
    private VisualizationEngine Engine;

    public int NextColorIndex
    {
      get
      {
        return Interlocked.Increment(ref this.colorIndex);
      }
    }

    public event Action ColorsChanged;

    internal ColorSelector(VisualizationEngine engine)
    {
      if (engine == null)
        throw new ArgumentNullException("engine");
      this.Engine = engine;
      this.Engine.ThemeChanged += (Action<BuiltinTheme, VisualizationTheme, bool>) ((themeId, theme, labels) => this.NotifyColorsChanged());
      this.Reset();
    }

    public Color4F GetColor(int index)
    {
      return this.Engine.GetVisualColor(index);
    }

    internal void NotifyColorsChanged()
    {
      if (this.ColorsChanged == null)
        return;
      this.ColorsChanged();
    }

    internal void SetColorIndex(IEnumerable<int> indices)
    {
      if (indices != null && Enumerable.Any<int>(indices))
        Interlocked.Exchange(ref this.colorIndex, Math.Max(Enumerable.Max(indices), -1));
      else
        this.Reset();
    }

    internal void Reset()
    {
      Interlocked.Exchange(ref this.colorIndex, -1);
    }
  }
}
