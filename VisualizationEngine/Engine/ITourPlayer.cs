// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ITourPlayer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.ComponentModel;

namespace Microsoft.Data.Visualization.Engine
{
  public interface ITourPlayer : INotifyPropertyChanged
  {
    bool IsPlaying { get; }

    int CurrentSceneIndex { get; }

    int SceneCount { get; }

    bool Loop { get; set; }

    void SetTour(Tour tour, ITourLayerManager layerManager, ITimeController timeController);

    void Play();

    void Pause();

    void Stop();

    void NextScene();

    void PreviousScene();
  }
}
