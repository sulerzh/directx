// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceVertex
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal struct InstanceVertex
  {
    public Vector3D Position;
    public Vector3D Normal;
    public float Texture;

    public InstanceVertex(Vector3D offset, Vector3D normal)
    {
      this.Position = offset;
      this.Normal = normal;
      this.Texture = 1f;
    }

    public InstanceVertex(Vector3D offset, Vector3D normal, float texture)
    {
      this.Position = offset;
      this.Normal = normal;
      this.Texture = texture;
    }
  }
}
