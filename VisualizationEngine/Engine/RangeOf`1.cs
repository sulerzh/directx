// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RangeOf`1
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  [Serializable]
  public class RangeOf<T> : IEquatable<RangeOf<T>> where T : IComparable<T>
  {
    private T mFrom;
    private T mTo;

    [XmlAttribute("From")]
    public T From
    {
      get
      {
        return this.mFrom;
      }
      set
      {
        this.mFrom = value;
      }
    }

    [XmlAttribute("To")]
    public T To
    {
      get
      {
        return this.mTo;
      }
      set
      {
        this.mTo = value;
      }
    }

    [XmlIgnore]
    public T Min
    {
      get
      {
        if (this.From.CompareTo(this.To) > 0)
          return this.To;
        else
          return this.From;
      }
    }

    [XmlIgnore]
    public T Max
    {
      get
      {
        if (this.From.CompareTo(this.To) <= 0)
          return this.To;
        else
          return this.From;
      }
    }

    public RangeOf(T _from, T _to)
    {
      this.mFrom = _from;
      this.mTo = _to;
    }

    public RangeOf()
      : this(default (T), default (T))
    {
    }

    public bool Equals(RangeOf<T> other)
    {
      if (this.From.CompareTo(other.From) == 0)
        return this.To.CompareTo(other.To) == 0;
      else
        return false;
    }

    public RangeOf<T> Clone()
    {
      return new RangeOf<T>(this.From, this.To);
    }

    public override string ToString()
    {
      return this.From.ToString() + "~" + this.To.ToString();
    }
  }
}
