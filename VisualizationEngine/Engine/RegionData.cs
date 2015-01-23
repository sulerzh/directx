// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionData
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  [DataContract(Name = "rdata", Namespace = "")]
  public class RegionData
  {
    private const string SafeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
    private string ring;

    public List<Coordinates> Polygon { get; private set; }

    [DataMember(Name = "id", Order = 1)]
    public long PolygonId { get; set; }

    [DataMember(Name = "ring", Order = 2)]
    public string Ring
    {
      get
      {
        return this.ring;
      }
      set
      {
        this.ring = value;
        List<Coordinates> parsedValue;
        if (!string.IsNullOrWhiteSpace(this.ring) && RegionData.TryParseEncodedValue(this.ring, out parsedValue))
        {
          this.Polygon = parsedValue;
        }
        else
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Failed to parse compression region polygon for Polygon ID {0}", (object) this.PolygonId);
          this.Polygon = new List<Coordinates>();
        }
      }
    }

    private static bool TryParseEncodedValue(string value, out List<Coordinates> parsedValue)
    {
      parsedValue = (List<Coordinates>) null;
      List<Coordinates> list = new List<Coordinates>();
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
label_8:
      if (num1 < value.Length)
      {
        long num4 = 0L;
        int num5 = 0;
        while (num1 < value.Length)
        {
          int num6 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-".IndexOf(value[num1++]);
          if (num6 == -1)
            return false;
          num4 |= ((long) num6 & 31L) << num5;
          num5 += 5;
          if (num6 < 32)
          {
            int num7 = (int) ((Math.Sqrt((double) (8L * num4 + 5L)) - 1.0) / 2.0);
            int num8 = (int) (num4 - (long) num7 * ((long) num7 + 1L) / 2L);
            int num9 = num7 - num8;
            int num10 = num9 >> 1 ^ -(num9 & 1);
            int num11 = num8 >> 1 ^ -(num8 & 1);
            num2 += num10;
            num3 += num11;
            list.Add(Coordinates.FromDegrees((double) num2 * 1E-05, (double) num3 * 1E-05));
            goto label_8;
          }
        }
        return false;
      }
      else
      {
        parsedValue = list;
        return true;
      }
    }
  }
}
