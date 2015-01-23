// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.DollyCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class DollyCameraMover : LineCameraMover
  {
    internal DollyCameraMover(CameraSnapshot midSnapshot, double extentFactor, TimeSpan moveDuration, Vector3D startAttractor, Vector3D endAttractor)
      : base(midSnapshot, moveDuration, Math.PI / 2.0)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "DollyCameraMover constructor.");
      this.ConstructPath(midSnapshot, this.GetArcAngle(extentFactor));
      Vector3D startPoint = this.path.StartPoint;
      Vector3D endPoint = this.path.EndPoint;
      if (Vector3D.DistanceSq(endPoint, startAttractor) + Vector3D.DistanceSq(startPoint, endAttractor) > Vector3D.DistanceSq(startPoint, startAttractor) + Vector3D.DistanceSq(endPoint, endAttractor))
        return;
      this.path.Reverse();
      this.relativeAngle = -this.relativeAngle;
    }
  }
}
