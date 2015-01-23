// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.SelectionEventArgs
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  public class SelectionEventArgs : EventArgs
  {
    public IList<InstanceId> SelectedIds { get; private set; }

    public SelectionMode Mode { get; private set; }

    public SelectionStyle Style { get; private set; }

    public object Tag { get; private set; }

    internal SelectionEventArgs(HitTestableLayer sender, IList<InstanceId> selectedIds, SelectionMode mode, SelectionStyle style, object tag)
    {
      this.SelectedIds = selectedIds;
      this.Mode = mode;
      this.Style = style;
      this.Tag = tag;
    }
  }
}
