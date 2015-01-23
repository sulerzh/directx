// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.SingleTouchPoint
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Windows;

namespace Microsoft.Data.Visualization.Engine
{
  public class SingleTouchPoint
  {
    public int Id;
    public Point PosCurrent;
    public Point PosStart;
    public Vector3D WorldPosStart;
    public Vector3D WorldPosCur;
    public bool HasFirstWorldSet;
    public bool UsedForGesture;
  }
}
