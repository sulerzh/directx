// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CustomSpaceDefinition
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  public class CustomSpaceDefinition : CompositePropertyChangeNotificationBase, IEquatable<CustomSpaceDefinition>
  {
    private bool mIsSwapXandY;
    private bool mIsCalibrateOnFirst;
    private bool mIsLocked;
    private CustomSpaceAxisDefinition mAxisX;
    private CustomSpaceAxisDefinition mAxisY;

    public static string PropertyIsSwapXandY
    {
      get
      {
        return "IsSwapXandY";
      }
    }

    public bool IsSwapXandY
    {
      get
      {
        return this.mIsSwapXandY;
      }
      set
      {
        base.SetProperty<bool>(CustomSpaceDefinition.PropertyIsSwapXandY, ref this.mIsSwapXandY, value);
      }
    }

    public static string PropertyIsCalibrateOnFirst
    {
      get
      {
        return "IsCalibrateOnFirst";
      }
    }

    public bool IsCalibrateOnFirst
    {
      get
      {
        return this.mIsCalibrateOnFirst;
      }
      set
      {
        base.SetProperty<bool>(CustomSpaceDefinition.PropertyIsCalibrateOnFirst, ref this.mIsCalibrateOnFirst, value);
      }
    }

    public static string PropertyIsLocked
    {
      get
      {
        return "IsLocked";
      }
    }

    public bool IsLocked
    {
      get
      {
        return this.mIsLocked;
      }
      set
      {
        base.SetProperty<bool>(CustomSpaceDefinition.PropertyIsLocked, ref this.mIsLocked, value);
      }
    }

    public static string PropertyAxisX
    {
      get
      {
        return "AxisX";
      }
    }

    public CustomSpaceAxisDefinition AxisX
    {
      get
      {
        return this.mAxisX;
      }
      set
      {
        if (value == null)
          return;
        base.SetProperty<CustomSpaceAxisDefinition>(CustomSpaceDefinition.PropertyAxisX, ref this.mAxisX, value);
      }
    }

    public static string PropertyAxisY
    {
      get
      {
        return "AxisY";
      }
    }

    public CustomSpaceAxisDefinition AxisY
    {
      get
      {
        return this.mAxisY;
      }
      set
      {
        if (value == null)
          return;
        base.SetProperty<CustomSpaceAxisDefinition>(CustomSpaceDefinition.PropertyAxisY, ref this.mAxisY, value);
      }
    }

    public bool IsAnyAutoCalculated
    {
      get
      {
        if (!this.AxisX.IsAutoCalculated)
          return this.AxisY.IsAutoCalculated;
        else
          return true;
      }
      set
      {
        this.AxisX.IsAutoCalculated = value;
        this.AxisY.IsAutoCalculated = value;
        this.RaisePropertyChanged("IsAnyAutoCalculated");
      }
    }

    public CustomSpaceDefinition()
    {
      this.IsSwapXandY = false;
      this.AxisX = new CustomSpaceAxisDefinition(-180.0, 180.0);
      this.AxisY = new CustomSpaceAxisDefinition(-90.0, 90.0);
    }

    public CustomSpaceDefinition(CustomSpaceDefinition.SerializableCustomSpaceDefinition ssd)
    {
      this.IsSwapXandY = ssd.IsSwapXandY;
      this.IsCalibrateOnFirst = ssd.IsCalibrateOnFirst;
      this.IsLocked = ssd.IsLocked;
      this.AxisX = ssd.AxisX.Unwrap();
      this.AxisY = ssd.AxisY.Unwrap();
    }

    public CustomSpaceDefinition.SerializableCustomSpaceDefinition Wrap()
    {
      return new CustomSpaceDefinition.SerializableCustomSpaceDefinition()
      {
        IsSwapXandY = this.IsSwapXandY,
        IsCalibrateOnFirst = this.IsCalibrateOnFirst,
        IsLocked = this.IsLocked,
        AxisX = this.AxisX.Wrap(),
        AxisY = this.AxisY.Wrap()
      };
    }

    public bool Equals(CustomSpaceDefinition other)
    {
      if (this.IsSwapXandY == other.IsSwapXandY && this.IsCalibrateOnFirst == other.IsCalibrateOnFirst && (this.IsLocked == other.IsLocked && this.AxisX.Equals(other.AxisX)))
        return this.AxisY.Equals(other.AxisY);
      else
        return false;
    }

    public bool AreRangesEqual(CustomSpaceDefinition other)
    {
      if (this.AxisX.AxisRange.Equals(other.AxisX.AxisRange))
        return this.AxisY.AxisRange.Equals(other.AxisY.AxisRange);
      else
        return false;
    }

    public CustomSpaceDefinition Clone()
    {
      return this.Wrap().Unwrap();
    }

    [XmlType("CustomSpaceDefinition")]
    [Serializable]
    public class SerializableCustomSpaceDefinition
    {
      [XmlElement]
      public bool IsSwapXandY { get; set; }

      [XmlElement]
      public bool IsCalibrateOnFirst { get; set; }

      [XmlElement]
      public bool IsLocked { get; set; }

      [XmlElement]
      public CustomSpaceAxisDefinition.SerializableCustomSpaceAxisDefinition AxisX { get; set; }

      [XmlElement]
      public CustomSpaceAxisDefinition.SerializableCustomSpaceAxisDefinition AxisY { get; set; }

      public CustomSpaceDefinition Unwrap()
      {
        return new CustomSpaceDefinition(this);
      }
    }
  }
}
