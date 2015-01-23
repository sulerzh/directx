// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CustomSpaceAxisDefinition
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  public class CustomSpaceAxisDefinition : CompositePropertyChangeNotificationBase, IEquatable<CustomSpaceAxisDefinition>
  {
    private RangeOf<double> mAxisRange = new RangeOf<double>();
    private double mScalePct = 1.0;
    private double mScaleOffsetPct;
    private bool _IsAutoCalculated;
    private bool _IsAxisFlipped;

    public static string PropertyAxisRange
    {
      get
      {
        return "AxisRange";
      }
    }

    public RangeOf<double> AxisRange
    {
      get
      {
        return this.mAxisRange;
      }
      set
      {
        if (value == null)
          return;
        base.SetProperty<RangeOf<double>>(CustomSpaceAxisDefinition.PropertyAxisRange, ref this.mAxisRange, value);
      }
    }

    public static string PropertyScalePct
    {
      get
      {
        return "ScalePct";
      }
    }

    public double ScalePct
    {
      get
      {
        return this.mScalePct;
      }
      set
      {
        base.SetProperty<double>(CustomSpaceAxisDefinition.PropertyScalePct, ref this.mScalePct, value);
      }
    }

    public static string PropertyScaleOffsetPct
    {
      get
      {
        return "ScaleOffsetPct";
      }
    }

    public double ScaleOffsetPct
    {
      get
      {
        return this.mScaleOffsetPct;
      }
      set
      {
        base.SetProperty<double>(CustomSpaceAxisDefinition.PropertyScaleOffsetPct, ref this.mScaleOffsetPct, value);
      }
    }

    public static string PropertyIsAutoCalculated
    {
      get
      {
        return "IsAutoCalculated";
      }
    }

    public bool IsAutoCalculated
    {
      get
      {
        return this._IsAutoCalculated;
      }
      set
      {
        base.SetProperty<bool>(CustomSpaceAxisDefinition.PropertyIsAutoCalculated, ref this._IsAutoCalculated, value);
      }
    }

    public static string PropertyIsAxisFlipped
    {
      get
      {
        return "IsAxisFlipped";
      }
    }

    public bool IsAxisFlipped
    {
      get
      {
        return this._IsAxisFlipped;
      }
      set
      {
        base.SetProperty<bool>(CustomSpaceAxisDefinition.PropertyIsAxisFlipped, ref this._IsAxisFlipped, value);
      }
    }

    public RangeOf<double> EffectiveRange
    {
      get
      {
        double num1 = (this.AxisRange.From + this.AxisRange.To) * 0.5;
        double num2 = this.AxisRange.To - this.AxisRange.From;
        double num3 = num2 * 0.5;
        double num4 = Math.Abs(this.ScalePct) > 0.0001 ? this.ScalePct : 0.0001;
        double num5 = num2 / num4;
        RangeOf<double> rangeOf = new RangeOf<double>(num1 - num3 / num4 - num5 * this.ScaleOffsetPct, num1 + num3 / num4 - num5 * this.ScaleOffsetPct);
        if (this.IsAxisFlipped)
          rangeOf = new RangeOf<double>(rangeOf.To, rangeOf.From);
        if (rangeOf.From == rangeOf.To || Math.Abs(rangeOf.From - rangeOf.To) < 0.0001)
          rangeOf = new RangeOf<double>(rangeOf.Min - 0.0001, rangeOf.Max + 0.0001);
        if (double.IsNaN(rangeOf.From) || double.IsNaN(rangeOf.To) || (double.IsInfinity(rangeOf.From) || double.IsInfinity(rangeOf.To)))
          rangeOf = new RangeOf<double>(-0.0001, 0.0001);
        return rangeOf;
      }
    }

    public CustomSpaceAxisDefinition(double from, double to)
    {
      this.AxisRange = new RangeOf<double>(from, to);
      this.IsAutoCalculated = false;
      this.ScalePct = 1.0;
      this.ScaleOffsetPct = 0.0;
    }

    public CustomSpaceAxisDefinition()
      : this(-180.0, 180.0)
    {
    }

    public CustomSpaceAxisDefinition(CustomSpaceAxisDefinition.SerializableCustomSpaceAxisDefinition ad)
    {
      this.AxisRange = ad.AxisRange;
      this.ScalePct = ad.ScalePct;
      this.ScaleOffsetPct = ad.ScaleOffsetPct;
      this.IsAutoCalculated = ad.IsAutoCalculated;
      this.IsAxisFlipped = ad.IsAxisFlipped;
    }

    public CustomSpaceAxisDefinition.SerializableCustomSpaceAxisDefinition Wrap()
    {
      return new CustomSpaceAxisDefinition.SerializableCustomSpaceAxisDefinition()
      {
        AxisRange = this.AxisRange.Clone(),
        ScalePct = this.ScalePct,
        ScaleOffsetPct = this.ScaleOffsetPct,
        IsAutoCalculated = this.IsAutoCalculated,
        IsAxisFlipped = this.IsAxisFlipped
      };
    }

    public bool Equals(CustomSpaceAxisDefinition other)
    {
      if ((this.AxisRange.Equals(other.AxisRange) || this.IsAutoCalculated && other.IsAutoCalculated) && (this.ScalePct == other.ScalePct && this.ScaleOffsetPct == other.ScaleOffsetPct && this.IsAutoCalculated == other.IsAutoCalculated))
        return this.IsAxisFlipped == other.IsAxisFlipped;
      else
        return false;
    }

    public CustomSpaceAxisDefinition Clone()
    {
      return this.Wrap().Unwrap();
    }

    public override string ToString()
    {
      return "AxisDef:" + this.EffectiveRange.ToString();
    }

    [XmlType("CustomSpaceAxisDefinition")]
    [Serializable]
    public class SerializableCustomSpaceAxisDefinition
    {
      [XmlElement]
      public RangeOf<double> AxisRange { get; set; }

      [XmlAttribute]
      public double ScalePct { get; set; }

      [XmlAttribute]
      public double ScaleOffsetPct { get; set; }

      [XmlAttribute]
      public bool IsAutoCalculated { get; set; }

      [XmlAttribute]
      public bool IsAxisFlipped { get; set; }

      public CustomSpaceAxisDefinition Unwrap()
      {
        return new CustomSpaceAxisDefinition(this);
      }
    }
  }
}
