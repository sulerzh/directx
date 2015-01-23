// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ITimeController
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.Engine
{
  public interface ITimeController : INotifyPropertyChanged
  {
    string PropertyVisualTimeEnabled { get; }

    string PropertyLooping { get; }

    string PropertyCurrentVisualTime { get; }

    string PropertyDuration { get; }

    TimeSpan Duration { get; set; }

    bool VisualTimeEnabled { get; set; }

    bool Looping { get; set; }

    DateTime CurrentVisualTime { get; set; }

    void SetVisualTimeRange(DateTime startTime, DateTime endTime, bool unionWithCurrentRange);
  }
}
