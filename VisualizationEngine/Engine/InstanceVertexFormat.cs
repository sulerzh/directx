// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceVertexFormat
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class InstanceVertexFormat
  {
    private static VertexComponent[] components;

    public static VertexComponent[] Components
    {
      get
      {
        if (InstanceVertexFormat.components == null)
          InstanceVertexFormat.InitializeVertexFormat();
        return InstanceVertexFormat.components;
      }
    }

    private InstanceVertexFormat()
    {
    }

    private static void InitializeVertexFormat()
    {
      InstanceVertexFormat.components = new VertexComponent[2]
      {
        new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Short4AsFloats, VertexComponentClassification.PerVertexData, 0),
        new VertexComponent(VertexSemantic.Normal, VertexComponentDataType.Short4AsFloats, VertexComponentClassification.PerVertexData, 0)
      };
    }
  }
}
