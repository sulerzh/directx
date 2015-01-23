// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CustomSpaceTransform
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  public class CustomSpaceTransform : ICustomSpaceTransform
  {
    private double mImageWidthOverHeight;
    private RangeOf<double> mAxisX;
    private RangeOf<double> mAxisY;
    private bool mIsSwapXandY;

    private double AxisYMin
    {
      get
      {
        return this.mAxisY.From;
      }
    }

    private double AxisYMax
    {
      get
      {
        return this.mAxisY.To;
      }
    }

    private double AxisXMin
    {
      get
      {
        return this.mAxisX.From;
      }
    }

    private double AxisXMax
    {
      get
      {
        return this.mAxisX.To;
      }
    }

    private double ImageWidthOverHeight
    {
      get
      {
        return this.mImageWidthOverHeight;
      }
    }

    public static double WorldScaleInDegrees
    {
      get
      {
        return 8.0;
      }
    }

    public CustomSpaceTransform(CustomSpaceDefinition _def, double _imageWidthOverHeight)
    {
      this.mImageWidthOverHeight = _imageWidthOverHeight;
      this.mIsSwapXandY = _def.IsSwapXandY;
      this.mAxisX = _def.AxisX.EffectiveRange;
      this.mAxisY = _def.AxisY.EffectiveRange;
    }

    private CustomSpaceTransform()
    {
    }

    private void Swap<T>(ref T a, ref T b)
    {
      T obj = a;
      a = b;
      b = obj;
    }

    public void TransformSpaceTextureToDegrees(double from_lat_y, double from_long_x, out double to_lat_y, out double to_long_x)
    {
      if (this.mIsSwapXandY)
        this.Swap<double>(ref from_lat_y, ref from_long_x);
      this.TransformSpacesDataToDegrees(this.AxisYMax * from_lat_y + this.AxisYMin * (1.0 - from_lat_y), this.AxisXMax * from_long_x + this.AxisXMin * (1.0 - from_long_x), out to_lat_y, out to_long_x);
    }

    private void TransformSpacesDataToUnit(double from_lat_y, double from_long_x, out double to_lat_y, out double to_long_x)
    {
      double num1 = this.AxisXMax - this.AxisXMin;
      double num2 = this.AxisYMax - this.AxisYMin;
      to_lat_y = (from_lat_y - this.AxisYMin) / num2;
      to_long_x = (from_long_x - this.AxisXMin) / num1;
      if (!this.mIsSwapXandY)
        return;
      this.Swap<double>(ref to_lat_y, ref to_long_x);
    }

    private void TransformSpacesUnitToDegrees(double from_lat_y, double from_long_x, out double to_lat_y, out double to_long_x)
    {
      if (this.ImageWidthOverHeight > 1.0)
      {
        double num1 = 1.0 / this.ImageWidthOverHeight;
        double num2 = (1.0 - num1) / 2.0;
        from_lat_y = from_lat_y * num1 + num2;
      }
      else
      {
        double imageWidthOverHeight = this.ImageWidthOverHeight;
        double num = (1.0 - imageWidthOverHeight) / 2.0;
        from_long_x = from_long_x * imageWidthOverHeight + num;
      }
      to_lat_y = from_lat_y * 2.0 * CustomSpaceTransform.WorldScaleInDegrees - CustomSpaceTransform.WorldScaleInDegrees;
      to_long_x = from_long_x * 2.0 * CustomSpaceTransform.WorldScaleInDegrees - CustomSpaceTransform.WorldScaleInDegrees;
      double num3 = Coordinates.InverseMercator(to_lat_y * (Math.PI / 180.0));
      to_lat_y = num3 * 57.2957795130823;
    }

    public void TransformSpacesDataToDegrees(double from_lat_y, double from_long_x, out double to_lat_y, out double to_long_x)
    {
      double to_lat_y1;
      double to_long_x1;
      this.TransformSpacesDataToUnit(from_lat_y, from_long_x, out to_lat_y1, out to_long_x1);
      this.TransformSpacesUnitToDegrees(to_lat_y1, to_long_x1, out to_lat_y, out to_long_x);
      if (double.IsNaN(to_lat_y))
        to_lat_y = 0.0;
      if (!double.IsNaN(to_long_x))
        return;
      to_long_x = 0.0;
    }
  }
}
