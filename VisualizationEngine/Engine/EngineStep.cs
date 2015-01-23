// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.EngineStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class EngineStep : PropertyChangedNotificationBase, IDisposable
  {
    private IUIControllerManager uiControllerManager;
    private ICameraControllerManager cameraControllerManager;
    private IHitTestManager hitTestManager;
    private IShadowManager shadowRenderingManager;
    private IVisualizationEngineDispatcher engineDispatcher;
    private Dispatcher eventDispatcher;

    protected ICameraControllerManager CameraControllerManager
    {
      get
      {
        return this.cameraControllerManager;
      }
    }

    protected IHitTestManager HitTestManager
    {
      get
      {
        return this.hitTestManager;
      }
    }

    protected IShadowManager ShadowManager
    {
      get
      {
        return this.shadowRenderingManager;
      }
    }

    protected IVisualizationEngineDispatcher EngineDispatcher
    {
      get
      {
        return this.engineDispatcher;
      }
    }

    protected Dispatcher EventDispatcher
    {
      get
      {
        return this.eventDispatcher;
      }
    }

    internal EngineStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eDispatcher)
    {
      this.engineDispatcher = dispatcher;
      this.eventDispatcher = eDispatcher;
    }

    internal void Initialize(IUIControllerManager uiManager, ICameraControllerManager cameraManager, IHitTestManager hitManager, IShadowManager shadowManager)
    {
      this.uiControllerManager = uiManager;
      this.cameraControllerManager = cameraManager;
      this.hitTestManager = hitManager;
      this.shadowRenderingManager = shadowManager;
    }

    public virtual void OnInitialized()
    {
    }

    internal abstract bool PreExecute(SceneState state, int phase);

    internal abstract void Execute(Renderer renderer, SceneState state, int phase);

    public abstract void Dispose();

    protected void AddUIController(UIController controller)
    {
      this.uiControllerManager.AddController(this, controller);
    }

    protected void RemoveUIController(UIController controller)
    {
      this.uiControllerManager.RemoveController(this, controller);
    }

    protected void RemoveAllUIControllers()
    {
      this.uiControllerManager.RemoveAllControllers(this);
    }
  }
}
