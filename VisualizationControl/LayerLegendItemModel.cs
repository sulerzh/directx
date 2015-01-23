using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Globalization;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class LayerLegendItemModel : CompositePropertyChangeNotificationBase
  {
    private bool nameIsLayerName;
    private Color _Color;
    private Color _MinColor;
    private string _Name;
    private string _MinVal;
    private string _MaxVal;

    public string PropertyColor
    {
      get
      {
        return "Color";
      }
    }

    public Color Color
    {
      get
      {
        return this._Color;
      }
      set
      {
        base.SetProperty<Color>(this.PropertyColor, ref this._Color, value);
      }
    }

    public string PropertyMinColor
    {
      get
      {
        return "MinColor";
      }
    }

    public Color MinColor
    {
      get
      {
        return this._MinColor;
      }
      set
      {
        base.SetProperty<Color>(this.PropertyMinColor, ref this._MinColor, value);
      }
    }

    public string PropertyName
    {
      get
      {
        return "Name";
      }
    }

    public string Name
    {
      get
      {
        return this._Name;
      }
      set
      {
        base.SetProperty<string>(this.PropertyName, ref this._Name, value);
      }
    }

    public string PropertyMinVal
    {
      get
      {
        return "MinVal";
      }
    }

    public string MinVal
    {
      get
      {
        return this._MinVal;
      }
      set
      {
        base.SetProperty<string>(this.PropertyMinVal, ref this._MinVal, value);
      }
    }

    public string PropertyMaxVal
    {
      get
      {
        return "MaxVal";
      }
    }

    public string MaxVal
    {
      get
      {
        return this._MaxVal;
      }
      set
      {
        base.SetProperty<string>(this.PropertyMaxVal, ref this._MaxVal, value);
      }
    }

    public LayerLegendItemModel(string name, Color color, double? minVal, double? maxVal, bool displayValueAsPercentage, bool nameIsLayerName)
    {
      this.Name = name;
      this.nameIsLayerName = nameIsLayerName;
      this.Color = color;
      this.MinColor = new Color()
      {
        A = color.A,
        R = (byte) (216.75 + 0.15 * (double) color.R),
        G = (byte) (216.75 + 0.15 * (double) color.G),
        B = (byte) (216.75 + 0.15 * (double) color.B)
      };
      if (maxVal.HasValue)
      {
        this.MaxVal = !displayValueAsPercentage ? maxVal.Value.ToString(Math.Abs(maxVal.Value) <= 0.01 || Math.Abs(maxVal.Value) >= 1E+21 ? (string) null : "N", (IFormatProvider) CultureInfo.CurrentUICulture) : maxVal.Value.ToString("P", (IFormatProvider) CultureInfo.CurrentUICulture);
        if (!minVal.HasValue)
        {
          minVal = maxVal;
          this.MinColor = this.Color;
        }
      }
      if (minVal.HasValue)
      {
        if (displayValueAsPercentage)
          this.MinVal = minVal.Value.ToString("P", (IFormatProvider) CultureInfo.CurrentUICulture);
        else
          this.MinVal = minVal.Value.ToString(Math.Abs(minVal.Value) <= 0.01 || Math.Abs(minVal.Value) >= 1E+21 ? (string) null : "N", (IFormatProvider) CultureInfo.CurrentUICulture);
      }
      else
        this.MinColor = this.Color;
    }

    internal void LayerNameUpdated(string layerName)
    {
      if (!this.nameIsLayerName)
        return;
      this.Name = layerName;
    }
  }
}
