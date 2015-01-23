// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.SelectionStyleChangedEventArgs
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class SelectionStyleChangedEventArgs : EventArgs
  {
    public SelectionStyle Style { get; private set; }

    public SelectionStyleChangedEventArgs(SelectionStyle style)
    {
      this.Style = style;
    }
  }
}
