// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceId
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;

namespace Microsoft.Data.Visualization.Engine
{
  public struct InstanceId : IEquatable<InstanceId>
  {
    public const uint MaxLayerId = 31U;
    public const uint MaxElementId = 134217727U;
    private const uint LayerMask = 4160749568U;
    private const uint ElementMask = 134217727U;
    private const int LayerShift = 27;
    private const int ElementShift = 0;

    internal uint LayerId
    {
      get
      {
        return (this.Id & 4160749568U) >> 27;
      }
    }

    public uint ElementId
    {
      get
      {
        return this.Id & 134217727U;
      }
    }

    public uint Id { get; set; }

    public InstanceId(uint element)
    {
      this = new InstanceId(0U, element);
    }

    public InstanceId(uint layer, uint element)
    {
      this = new InstanceId();
      this.Id = (uint) ((int) layer << 27 & -134217728 | (int) element & 134217727);
    }

    internal InstanceId(uint layer, InstanceId instance)
    {
      this = new InstanceId();
      this.Id = (uint) ((int) layer << 27 & -134217728 | (int) instance.Id & 134217727);
    }

    public static bool operator ==(InstanceId left, InstanceId right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(InstanceId left, InstanceId right)
    {
      return !left.Equals(right);
    }

    internal static InstanceId FromId(uint id)
    {
      return new InstanceId((id & 4160749568U) >> 27, id & 134217727U);
    }

    public bool Equals(InstanceId other)
    {
      return (int) other.Id == (int) this.Id;
    }

    public override bool Equals(object obj)
    {
      return (int) ((InstanceId) obj).Id == (int) this.Id;
    }

    public override int GetHashCode()
    {
      return this.Id.GetHashCode();
    }
  }
}
