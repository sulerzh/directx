// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ValueRangeCalculator
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class ValueRangeCalculator : TimeSlicer
  {
    private Dictionary<int, double> minShiftMaxValue = new Dictionary<int, double>();
    private Dictionary<int, double> maxShiftMaxValue = new Dictionary<int, double>();
    private const double k = 0.0482549424336947;
    private double currentMaxValue;
    private int currentMaxValueShift;
    private double currentAbsMaxValue;
    private double currentAbsSum;
    private bool zeros;
    private bool negatives;
    private bool logScale;

    public double MinOverallMaxValue { get; private set; }

    public double MaxOverallValue { get; private set; }

    public double MinLocalMaxValue { get; private set; }

    public double MaxLocalMaxValue { get; private set; }

    public ValueRangeCalculator(List<InstanceData> instanceList, bool hasTimeData, bool computeZeros, bool computeNegatives, bool logScale)
      : base(instanceList, hasTimeData)
    {
      this.MinOverallMaxValue = double.MaxValue;
      this.MaxOverallValue = double.MinValue;
      this.MinLocalMaxValue = double.MaxValue;
      this.MaxLocalMaxValue = double.MinValue;
      this.zeros = computeZeros;
      this.negatives = computeNegatives;
      this.logScale = logScale;
    }

    public void GetValuesPerShift(double[] minValues, double[] maxValues)
    {
      for (int key = 0; key < minValues.Length; ++key)
      {
        double num1 = double.NaN;
        double num2 = double.NaN;
        if (!this.minShiftMaxValue.TryGetValue(key, out num1))
          num1 = double.NaN;
        if (!this.maxShiftMaxValue.TryGetValue(key, out num2))
          num2 = double.NaN;
        minValues[key] = num1;
        maxValues[key] = num2;
      }
    }

    protected override void Initialize()
    {
    }

    protected override void BeginSlice()
    {
      this.currentMaxValue = double.MinValue;
      this.currentMaxValueShift = 0;
      this.currentAbsMaxValue = double.MinValue;
      this.currentAbsSum = 0.0;
    }

    protected override void ProcessInstance(InstanceData instance)
    {
      if (!this.zeros && (double) instance.Value == 0.0 || !this.negatives && (double) instance.Value < 0.0 || float.IsNaN(instance.Value))
        return;
      float num1 = instance.Value;
      if (this.logScale)
        num1 = (float) Math.Exp(((double) Math.Abs(num1) - 1.0) / 0.0482549424336947) * (float) Math.Sign(num1);
      if ((double) num1 > this.currentMaxValue)
      {
        this.currentMaxValue = (double) num1;
        this.currentMaxValueShift = (int) instance.Color;
      }
      float num2 = Math.Abs(num1);
      if ((double) num2 > this.currentAbsMaxValue)
        this.currentAbsMaxValue = (double) num2;
      this.currentAbsSum += (double) num2;
    }

    protected override void EndSlice()
    {
      if (this.currentMaxValue == double.MinValue)
        return;
      if (this.currentMaxValue > this.MaxOverallValue)
        this.MaxOverallValue = this.currentMaxValue;
      if (this.currentMaxValue < this.MinOverallMaxValue)
        this.MinOverallMaxValue = this.currentMaxValue;
      double num = this.currentAbsSum == 0.0 ? 1.0 : Math.Abs(this.currentMaxValue / this.currentAbsSum);
      if (num > this.MaxLocalMaxValue)
        this.MaxLocalMaxValue = num;
      if (num < this.MinLocalMaxValue)
        this.MinLocalMaxValue = num;
      if (this.maxShiftMaxValue.ContainsKey(this.currentMaxValueShift))
      {
        if (this.currentMaxValue > this.maxShiftMaxValue[this.currentMaxValueShift])
          this.maxShiftMaxValue[this.currentMaxValueShift] = this.currentMaxValue;
      }
      else
        this.maxShiftMaxValue.Add(this.currentMaxValueShift, this.currentMaxValue);
      if (this.minShiftMaxValue.ContainsKey(this.currentMaxValueShift))
      {
        if (this.currentMaxValue >= this.minShiftMaxValue[this.currentMaxValueShift])
          return;
        this.minShiftMaxValue[this.currentMaxValueShift] = this.currentMaxValue;
      }
      else
        this.minShiftMaxValue.Add(this.currentMaxValueShift, this.currentMaxValue);
    }
  }
}
