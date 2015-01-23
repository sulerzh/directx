// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ICustomSpaceTransform
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

namespace Microsoft.Data.Visualization.Engine
{
  public interface ICustomSpaceTransform
  {
    void TransformSpacesDataToDegrees(double from_lat_y, double from_long_x, out double to_lat_y, out double to_long_x);

    void TransformSpaceTextureToDegrees(double from_lat_y, double from_long_x, out double to_lat_y, out double to_long_x);
  }
}
