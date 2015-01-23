// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LightingStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal class LightingStep : EngineStep
  {
    private LightingRig rig;

    public LightingRig Rig
    {
      get
      {
        return this.rig;
      }
      set
      {
        if (value == null)
          return;
        this.rig = value;
      }
    }

    public LightingStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher)
      : base(dispatcher, eventDispatcher)
    {
      this.rig = new LightingRig();
    }

    internal override bool PreExecute(SceneState state, int phase)
    {
      return false;
    }

    internal override void Execute(Renderer renderer, SceneState state, int phase)
    {
      Vector3D directionalLightPosition = this.Rig.DirectionalLightPosition;
      directionalLightPosition.TransformNormal(state.InverseWorld);
      renderer.FrameParameters.DirectionalLightPosition.Value = (Vector4F) directionalLightPosition;
      Vector3D pointLightPosition = this.Rig.PointLightPosition;
      pointLightPosition.TransformCoordinate(state.InverseWorld);
      renderer.FrameParameters.PointLightPositionAndFactor.Value = (Vector4F) new Vector4D(pointLightPosition, (double) this.Rig.PointLightAttenuationFactor);
      renderer.FrameParameters.LightFactors.Value = new Vector4F(this.Rig.AmbientLightFactor, this.Rig.DirectionalLightFactor, this.Rig.PointLightFactor, this.Rig.SecondaryAmbientLightFactor);
    }

    public override void Dispose()
    {
    }
  }
}
