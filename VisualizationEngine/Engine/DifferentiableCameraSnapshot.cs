// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.DifferentiableCameraSnapshot
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;

namespace Microsoft.Data.Visualization.Engine
{
  internal struct DifferentiableCameraSnapshot
  {
    public DifferentiableScalar Latitude;
    public DifferentiableScalar Longitude;
    public DifferentiableScalar Rotation;
    public DifferentiableScalar PivotAngle;
    public DifferentiableScalar Distance;

    public DifferentiableCameraSnapshot(DifferentiableScalar latitude, DifferentiableScalar longitude, DifferentiableScalar rotation, DifferentiableScalar pivotAngle, DifferentiableScalar distance)
    {
      this.Latitude = latitude;
      this.Longitude = longitude;
      this.Rotation = rotation;
      this.PivotAngle = pivotAngle;
      this.Distance = distance;
    }

    public DifferentiableCameraSnapshot(CameraSnapshot snapshot)
    {
      this.Latitude = new DifferentiableScalar(snapshot.Latitude, 0.0);
      this.Longitude = new DifferentiableScalar(snapshot.Longitude, 0.0);
      this.Rotation = new DifferentiableScalar(snapshot.Rotation, 0.0);
      this.PivotAngle = new DifferentiableScalar(snapshot.PivotAngle, 0.0);
      this.Distance = new DifferentiableScalar(snapshot.Distance, 0.0);
    }
  }
}
