// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LightingRig
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public class LightingRig
  {
    private Vector3D directionalLightPosition;
    private Vector3D pointLightPosition;

    public Vector3D DirectionalLightPosition
    {
      get
      {
        return this.directionalLightPosition;
      }
      set
      {
        this.directionalLightPosition = Vector3D.Normalize(value);
      }
    }

    public Vector3D PointLightPosition
    {
      get
      {
        return this.pointLightPosition;
      }
      set
      {
        this.pointLightPosition = value;
      }
    }

    public float PointLightAttenuationFactor { get; set; }

    public float AmbientLightFactor { get; set; }

    public float SecondaryAmbientLightFactor { get; set; }

    public float DirectionalLightFactor { get; set; }

    public float PointLightFactor { get; set; }

    public LightingRig()
    {
      this.DirectionalLightPosition = new Vector3D(-0.1, 0.2, -0.25);
      this.PointLightPosition = new Vector3D(0.1, -0.5, -2.0);
      this.PointLightAttenuationFactor = 1.0f / 1000.0f;
      this.AmbientLightFactor = 0.25f;
      this.SecondaryAmbientLightFactor = 0.6f;
      this.DirectionalLightFactor = 0.4f;
      this.PointLightFactor = 0.43f;
    }
  }
}
