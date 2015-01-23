// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InputHandler
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Data.Visualization.Engine
{
  public abstract class InputHandler
  {
    private List<InputEvent> eventsFront = new List<InputEvent>();
    private List<InputEvent> eventsBack = new List<InputEvent>();
    private object eventsLock = new object();

    public abstract Point? CursorPosition { get; }

    protected void AddEvent(InputEvent inputEvent)
    {
      lock (this.eventsLock)
        this.eventsFront.Add(inputEvent);
    }

    internal List<InputEvent> GetEvents()
    {
      lock (this.eventsLock)
      {
        List<InputEvent> local_0 = this.eventsBack;
        this.eventsBack = this.eventsFront;
        this.eventsFront = local_0;
        this.eventsFront.Clear();
      }
      return this.eventsBack;
    }
  }
}
