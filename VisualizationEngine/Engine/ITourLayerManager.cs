// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ITourLayerManager
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;

namespace Microsoft.Data.Visualization.Engine
{
  public interface ITourLayerManager
  {
    DateTime? PlayFromTime { get; set; }

    DateTime? PlayToTime { get; set; }

    string GetSceneLayersContent();

    string CreateDefaultSceneLayerContent();

    void SetSceneLayersContent(string layersContent, Guid customMapSpaceId, Action<object, Exception> completedCallback = null, object completionContext = null);

    object PrepareSceneLayers(string layersContent, Guid customMapSpaceId, Action<object, Exception> completedCallback = null, object completionContext = null);

    void SetSceneLayers(object preparedContext, Action<object, Exception> completedCallback = null, object completionContext = null);
  }
}
