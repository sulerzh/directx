// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Chart
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class Chart
  {
    protected double instanceWidth;
    protected double instanceHeight;

    internal float Altitude { get; set; }

    internal float ShadowScale { get; set; }

    internal float InstanceWidth
    {
      get
      {
        return (float) this.instanceWidth;
      }
    }

    internal float InstanceHeight
    {
      get
      {
        return (float) this.instanceHeight;
      }
    }

    internal virtual double HorizontalSpacing
    {
      get
      {
        return 0.0;
      }
    }

    internal abstract float FixedDimension { get; }

    internal abstract Vector2F FixedScale { get; }

    internal abstract LayerType ChartType { get; }

    internal virtual int PrivateVisualsCount
    {
      get
      {
        return 2;
      }
    }

    internal virtual float StackGap
    {
      get
      {
        return 0.0f;
      }
    }

    internal virtual bool IsBubble
    {
      get
      {
        return false;
      }
    }

    internal virtual float AnnotationAnchorHeight
    {
      get
      {
        return 1f;
      }
    }

    internal virtual bool UsesAbsDimension
    {
      get
      {
        return false;
      }
    }

    internal Chart()
    {
    }

    internal abstract Vector2F GetVariableScale(double scale);

    internal virtual void AddPrivateVisuals(ChartLayer layer)
    {
    }

    internal virtual float GetMaxExtent(float scale, int count)
    {
      return scale * (float) MathEx.Hypot((double) this.InstanceWidth, (double) this.InstanceHeight + (double) this.Altitude);
    }

    protected virtual double ComputeMaxAbsValue(List<InstanceData> instanceList, bool timeBased, int first)
    {
      double num1 = 0.0;
      for (int index = first; index < instanceList.Count; ++index)
      {
        double d = (double) instanceList[index].Value;
        if (!double.IsNaN(d))
        {
          double num2 = Math.Abs(d);
          if (num2 > num1)
            num1 = num2;
        }
      }
      return num1;
    }

    internal double ComputeScale(List<InstanceData> instanceList, bool timeBased, int first)
    {
      double maxAbsValue = this.ComputeMaxAbsValue(instanceList, timeBased, first);
      if (maxAbsValue > 0.0)
        return 1.0 / maxAbsValue;
      else
        return 1.0;
    }
  }
}
