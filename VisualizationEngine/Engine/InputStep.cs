// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InputStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal class InputStep : EngineStep, IUIControllerManager
  {
    private List<UIController> controllers = new List<UIController>();
    private Dictionary<EngineStep, List<UIController>> controllerOwners = new Dictionary<EngineStep, List<UIController>>();
    private object inputHandlerSync = new object();
    private readonly WeakReference<InputHandler> inputHandler = new WeakReference<InputHandler>((InputHandler) null);
    private Point? lastValidCursorPosition;

    public InputHandler InputHandler
    {
      private get
      {
        InputHandler target;
        if (!this.inputHandler.TryGetTarget(out target))
          target = (InputHandler) null;
        return target;
      }
      set
      {
        lock (this.inputHandlerSync)
          this.inputHandler.SetTarget(value);
      }
    }

    public InputStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher, InputHandler handler)
      : base(dispatcher, eventDispatcher)
    {
      this.InputHandler = handler;
    }

    public override void OnInitialized()
    {
      base.OnInitialized();
      VisualizationEngine visualizationEngine = this.EngineDispatcher as VisualizationEngine;
      if (visualizationEngine != null)
        this.AddUIController((UIController) new VisualizationEngineUIController(visualizationEngine));
      this.AddUIController((UIController) new DefaultCameraUIController(this.CameraControllerManager.DefaultController));
    }

    internal override bool PreExecute(SceneState state, int phase)
    {
      lock (this.inputHandlerSync)
      {
        InputHandler local_0 = this.InputHandler;
        if (local_0 == null)
          return false;
        try
        {
          List<InputEvent> local_1 = local_0.GetEvents();
          foreach (InputEvent item_0 in local_1)
          {
            foreach (UIController item_1 in this.controllers)
            {
              if (item_0.ProcessEvent(item_1))
                break;
            }
          }
          state.InputChanged = local_1.Count > 0;
          state.CursorPosition = local_0.CursorPosition;
          if (state.CursorPosition.HasValue)
            this.lastValidCursorPosition = state.CursorPosition;
          state.LastValidCursorPosition = this.lastValidCursorPosition;
          return state.InputChanged;
        }
        catch (Exception exception_0)
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Exception while pre-executing InputStep. Exception: {0}.", (object) ((object) exception_0).ToString());
          return false;
        }
      }
    }

    internal override void Execute(Renderer renderer, SceneState state, int phase)
    {
    }

    public void AddController(EngineStep owner, UIController controller)
    {
      if (!this.controllerOwners.ContainsKey(owner))
        this.controllerOwners.Add(owner, new List<UIController>());
      this.controllers.Insert(0, controller);
      this.controllerOwners[owner].Add(controller);
    }

    public void RemoveController(EngineStep owner, UIController controller)
    {
      if (!this.controllerOwners.ContainsKey(owner))
        return;
      this.controllers.Remove(controller);
      List<UIController> list = this.controllerOwners[owner];
      list.Remove(controller);
      if (list.Count != 0)
        return;
      this.controllerOwners.Remove(owner);
    }

    public void RemoveAllControllers(EngineStep owner)
    {
      if (!this.controllerOwners.ContainsKey(owner))
        return;
      foreach (UIController uiController in this.controllerOwners[owner])
        this.controllers.Remove(uiController);
      this.controllerOwners.Remove(owner);
    }

    public override void Dispose()
    {
    }
  }
}
