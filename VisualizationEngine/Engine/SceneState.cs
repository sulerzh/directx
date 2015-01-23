// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.SceneState
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Windows;

namespace Microsoft.Data.Visualization.Engine
{
  public class SceneState
  {
    private PlaneD[] viewFrustum;
    private Matrix4x4D view;
    private Matrix4x4D projection;
    private Matrix4x4D[] offsetProjection;
    private Matrix4x4D[] offsetViewProjection;

    public double ScreenWidth { get; internal set; }

    public double ScreenHeight { get; internal set; }

    public GraphicsLevel GraphicsLevel { get; internal set; }

    public bool OfflineRender { get; internal set; }

    public long ElapsedMilliseconds { get; internal set; }

    public double ElapsedSeconds { get; internal set; }

    public long ElapsedTicks { get; internal set; }

    public long ElapsedFrames { get; internal set; }

    public DateTime VisualTime { get; internal set; }

    public double VisualTimeToRealtimeRatio { get; internal set; }

    public DateTime? VisualTimeFreeze { get; internal set; }

    public CameraSnapshot CameraSnapshot { get; internal set; }

    public bool CameraMoved { get; internal set; }

    public double NearPlaneDistance { get; internal set; }

    public double FarPlaneDistance { get; internal set; }

    public double FieldOfViewX { get; internal set; }

    public double FieldOfViewY { get; internal set; }

    public bool InputChanged { get; internal set; }

    public Point? CursorPosition { get; internal set; }

    public Point? LastValidCursorPosition { get; internal set; }

    public Vector3F HoveredObjectPosition { get; internal set; }

    public Texture GlobeDepth { get; internal set; }

    public bool TranslucentGlobe { get; internal set; }

    public Color4F? WaterColor { get; internal set; }

    public bool GlowEnabled { get; internal set; }

    public double FlatteningFactor { get; internal set; }

    public CustomMap SceneCustomMap { get; set; }

    public bool IsSkipLayerRelatedSteps { get; set; }

    public Matrix4x4D World { get; internal set; }

    public Matrix4x4D InverseWorld { get; internal set; }

    public Matrix4x4D InverseView { get; internal set; }

    public Matrix4x4D ViewProjection { get; private set; }

    public Vector3D CameraPosition { get; internal set; }

    public Matrix4x4D View
    {
      get
      {
        return this.view;
      }
      internal set
      {
        this.view = value;
        this.ViewProjection = this.view * this.projection;
      }
    }

    public Matrix4x4D Projection
    {
      get
      {
        return this.projection;
      }
      internal set
      {
        this.projection = value;
        this.ViewProjection = this.view * this.projection;
      }
    }

    public Matrix4x4D[] DepthOffsetProjection
    {
      get
      {
        return this.offsetProjection;
      }
      internal set
      {
        this.offsetProjection = value;
        if (this.offsetProjection != null)
        {
          this.offsetViewProjection = new Matrix4x4D[this.offsetProjection.Length];
          for (int index = 0; index < this.offsetProjection.Length; ++index)
            this.offsetViewProjection[index] = this.view * this.offsetProjection[index];
        }
        else
          this.offsetViewProjection = (Matrix4x4D[]) null;
      }
    }

    public Matrix4x4D[] DepthOffsetViewProjection
    {
      get
      {
        return this.offsetViewProjection;
      }
    }

    internal SceneState()
    {
    }

    public PlaneD[] GetViewFrustum()
    {
      return this.viewFrustum;
    }

    internal void SetViewFrustum(PlaneD[] frustum)
    {
      this.viewFrustum = frustum;
    }

    public SceneState Clone()
    {
      SceneState sceneState = new SceneState();
      sceneState.CameraMoved = this.CameraMoved;
      sceneState.CameraPosition = this.CameraPosition;
      if (this.CameraSnapshot != null)
        sceneState.CameraSnapshot = this.CameraSnapshot.Clone();
      sceneState.InputChanged = this.InputChanged;
      sceneState.InverseWorld = this.InverseWorld;
      sceneState.InverseView = this.InverseView;
      sceneState.Projection = this.Projection;
      sceneState.ScreenHeight = this.ScreenHeight;
      sceneState.ScreenWidth = this.ScreenWidth;
      sceneState.SceneCustomMap = this.SceneCustomMap;
      sceneState.GraphicsLevel = this.GraphicsLevel;
      sceneState.OfflineRender = this.OfflineRender;
      sceneState.ElapsedMilliseconds = this.ElapsedMilliseconds;
      sceneState.ElapsedSeconds = this.ElapsedSeconds;
      sceneState.ElapsedTicks = this.ElapsedTicks;
      sceneState.ElapsedFrames = this.ElapsedFrames;
      sceneState.VisualTime = this.VisualTime;
      sceneState.VisualTimeToRealtimeRatio = this.VisualTimeToRealtimeRatio;
      sceneState.VisualTimeFreeze = this.VisualTimeFreeze;
      sceneState.View = this.View;
      sceneState.viewFrustum = this.viewFrustum;
      sceneState.ViewProjection = this.ViewProjection;
      sceneState.DepthOffsetProjection = this.DepthOffsetProjection;
      sceneState.World = this.World;
      sceneState.NearPlaneDistance = this.NearPlaneDistance;
      sceneState.FarPlaneDistance = this.FarPlaneDistance;
      sceneState.FieldOfViewX = this.FieldOfViewX;
      sceneState.FieldOfViewY = this.FieldOfViewY;
      sceneState.HoveredObjectPosition = this.HoveredObjectPosition;
      sceneState.TranslucentGlobe = this.TranslucentGlobe;
      sceneState.WaterColor = this.WaterColor;
      sceneState.GlowEnabled = this.GlowEnabled;
      sceneState.GlobeDepth = (Texture) null;
      sceneState.FlatteningFactor = this.FlatteningFactor;
      return sceneState;
    }
  }
}
