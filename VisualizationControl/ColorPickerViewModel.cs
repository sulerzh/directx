using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ColorPickerViewModel : ViewModelBase
  {
    public string PropertySelectedColor = "SelectedColor";
    private Color4F _SelectedColor;

    private static Tuple<Color4F, string>[] _BaseColors
    {
      get
      {
        return new Tuple<Color4F, string>[8]
        {
          Tuple.Create<Color4F, string>(new Color4F(1f, 0.7529412f, 0.0f, 0.0f), Resources.Color_DeepRed),
          Tuple.Create<Color4F, string>(new Color4F(1f, 1f, 0.0f, 0.0f), Resources.Color_Red),
          Tuple.Create<Color4F, string>(new Color4F(1f, 1f, 0.7529412f, 0.0f), Resources.Color_Orange),
          Tuple.Create<Color4F, string>(new Color4F(1f, 1f, 1f, 0.0f), Resources.Color_Yellow),
          Tuple.Create<Color4F, string>(new Color4F(1f, 0.0f, 0.6901961f, 0.3137255f), Resources.Color_Green),
          Tuple.Create<Color4F, string>(new Color4F(1f, 0.0f, 0.6901961f, 0.9411765f), Resources.Color_LightBlue),
          Tuple.Create<Color4F, string>(new Color4F(1f, 0.0f, 0.4392157f, 0.7529412f), Resources.Color_Blue),
          Tuple.Create<Color4F, string>(new Color4F(1f, 0.4392157f, 0.1882353f, 0.627451f), Resources.Color_Purple)
        };
      }
    }

    public static List<Tuple<Color4F, string>> Colors
    {
      get
      {
        List<Tuple<Color4F, string>> list = new List<Tuple<Color4F, string>>();
        int[] numArray1 = new int[6]
        {
          0,
          -5,
          -15,
          -25,
          -35,
          -50
        };
        int[] numArray2 = new int[6]
        {
          0,
          50,
          35,
          25,
          15,
          5
        };
        int[] numArray3 = new int[6]
        {
          0,
          80,
          60,
          40,
          -25,
          -50
        };
        Color4F color = new Color4F(1f, 1f, 1f, 1f);
        for (int index = 0; index < Enumerable.Count<int>((IEnumerable<int>) numArray2); ++index)
        {
          list.Add(Tuple.Create<Color4F, string>(Color4F.ApplyLightnessFactor(color, (double) numArray2[index] / 100.0).Value, ColorPickerViewModel.GetColorName(Resources.Color_Black, numArray2[index])));
          list.Add(ColorPickerViewModel.GetColorWithName(new Color4F(1f, 1f, 1f, 1f), numArray1[index], Resources.Color_White));
          foreach (Tuple<Color4F, string> tuple in ColorPickerViewModel._BaseColors)
            list.Add(ColorPickerViewModel.GetColorWithName(tuple.Item1, numArray3[index], tuple.Item2));
        }
        return list;
      }
    }

    public Color4F SelectedColor
    {
      get
      {
        return this._SelectedColor;
      }
      set
      {
        this.SetSelectedColor(value);
        if (this.SelectedColorChangedByUser == null)
          return;
        this.SelectedColorChangedByUser(value);
      }
    }

    public ICommand ResetColorCommand { get; set; }

    public List<Color4F> CustomColors { get; set; }

    public event Action<Color4F> SelectedColorChangedByUser;

    public ColorPickerViewModel(List<Color4F> customColors)
    {
      this.CustomColors = customColors ?? new List<Color4F>();
      this.SelectedColor = GeoVisualization.DefaultColor;
    }

    private static string GetColorName(string baseName, int percentage)
    {
      if (percentage == 0)
        return baseName;
      if (percentage > 0)
        return string.Format(Resources.Color_LighterFormat, (object) baseName, (object) percentage, (object) CultureInfo.CurrentUICulture);
      else
        return string.Format(Resources.Color_DarkerFormat, (object) baseName, (object) -percentage);
    }

    private static Tuple<Color4F, string> GetColorWithName(Color4F baseColor, int percentage, string baseName)
    {
      return Tuple.Create<Color4F, string>(Color4F.ApplyLightnessFactor(baseColor, 1.0 + (double) percentage / 100.0) ?? new Color4F(1f, 1f, 1f, 1f), ColorPickerViewModel.GetColorName(baseName, percentage));
    }

    public void SetSelectedColor(Color4F newColor)
    {
      this.SetProperty<Color4F>(this.PropertySelectedColor, ref this._SelectedColor, newColor, false);
    }
  }
}
