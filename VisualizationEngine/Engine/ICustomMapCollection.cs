// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ICustomMapCollection
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  public interface ICustomMapCollection
  {
    IEnumerable<CustomMap> AvailableMaps { get; }

    CustomMap CreateCustomMap();

    CustomMap FindOrCreateMapFromId(Guid uniqueId);

    void PermanentlyDeleteCustomMap(CustomMap cm);

    void MarkMapListAsDirty();
  }
}
