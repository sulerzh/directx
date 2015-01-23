// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TileRenderer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.Engine
{
  internal class TileRenderer : DisposableResource
  {
    private readonly Color4F DefaultColor = new Color4F(1f, 0.72549f, 0.811765f, 0.921569f);
    private ColorOperations postProcessingOps = new ColorOperations();
    private MultiplyTransform colorModulation = new MultiplyTransform();
    private int customMapRefTextureVersion = -1;
    private RenderParameterMatrix4x4F viewProjWithOffset = new RenderParameterMatrix4x4F("ViewProjWithOffset");
    private RenderParameterMatrix4x4F viewWithOffset = new RenderParameterMatrix4x4F("ViewWithOffset");
    private RenderParameterMatrix4x4F offset = new RenderParameterMatrix4x4F("Offset");
    private RenderParameterMatrix4x4F colorMatrix = new RenderParameterMatrix4x4F("ColorMatrix");
    private RenderParameterVector4F glowParameters = new RenderParameterVector4F("GlowParameters");
    private RenderParameterColor4F glowColor = new RenderParameterColor4F("GlowColor");
    private RenderParameterColor4F replaceSourceColor = new RenderParameterColor4F("ReplaceSourceColor");
    private RenderParameterColor4F replaceDestColor = new RenderParameterColor4F("ReplaceDestColor");
    private RenderParameterVector4F textureOffset = new RenderParameterVector4F("TextureOffset");
    private RenderParameterBool colorReplaceEnable = new RenderParameterBool("ColorReplaceEnable");
    private RenderParameterBool colorMapEnable = new RenderParameterBool("ColorMapEnable");
    private RenderParameterBool glowEnable = new RenderParameterBool("GlowEnable");
    private List<TileRenderable> renderables = new List<TileRenderable>();
    private const float GlobeAlphaFactor = 0.7f;
    private Effect effect;
    private Effect depthEffect;
    private Effect wireframeEffect;
    private DepthStencilState depthEnabledState;
    private BlendState blendState;
    private BlendState blendStateAlpha;
    private BlendState flatteningBlendState;
    private RasterizerState rasterizerState;
    private RasterizerState flatteningRasterizerState;
    private DepthStencilState wireframeDepthState;
    private RasterizerState wireframeRasterizerState;
    private RenderTarget globeDepthTarget;
    private Texture globeDepthTexture;
    private Texture defaultTexture;
    private float glowFactor;
    private CustomMap customMapRef;
    private CustomSpaceTransform customMapRefVertices;
    private VertexBuffer customMapVertices;
    private Texture customMapTexture;
    private bool useWaterColorForReplace;

    public bool ModulateTileLevels { get; set; }

    public bool WireframeEnabled { get; set; }

    public Texture DepthTexture
    {
      get
      {
        return this.globeDepthTexture;
      }
    }

    public bool GlowEnabled
    {
      set
      {
        this.glowEnable.Value = value;
      }
    }

    public Color4F GlowColor
    {
      set
      {
        this.glowColor.Value = value;
      }
    }

    public float GlowReflectanceIndex
    {
      set
      {
        this.glowParameters.Value = new Vector4F(value, this.glowParameters.Value.Y, this.glowParameters.Value.Z, this.glowParameters.Value.W);
      }
    }

    public float GlowPower
    {
      set
      {
        this.glowParameters.Value = new Vector4F(this.glowParameters.Value.X, value, this.glowParameters.Value.Z, this.glowParameters.Value.W);
      }
    }

    public float GlowFactor
    {
      set
      {
        this.glowFactor = value;
      }
    }

    public ColorOperations PostProcessingOperations
    {
      get
      {
        return this.postProcessingOps;
      }
    }

    public TileRenderer(bool useWaterColor)
    {
      this.Initialize();
      this.useWaterColorForReplace = useWaterColor;
      this.colorReplaceEnable.Value = false;
      this.colorMapEnable.Value = false;
      this.glowEnable.Value = true;
      this.glowFactor = 0.7f;
      this.glowParameters.Value = new Vector4F(0.0002f, 5f, 0.7f, 0.0f);
      this.glowColor.Value = new Color4F(1f, 1f, 1f, 1f);
    }

    private void Initialize()
    {
      TextureSampler textureSampler1 = TextureSampler.Create(TextureFilter.Anisotropic, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(1f, 1f, 0.0f, 0.0f));
      TextureSampler textureSampler2 = TextureSampler.Create(TextureFilter.Linear, TextureAddressMode.Clamp, TextureAddressMode.Clamp, new Color4F(0.0f, 0.0f, 0.0f, 0.0f));
      this.effect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.TileRenderable.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.TileRenderable.ps"),
        VertexFormat = new TileVertex().Format,
        Samplers = new TextureSampler[2]
        {
          textureSampler1,
          textureSampler2
        },
        Parameters = RenderParameters.Create(new IRenderParameter[12]
        {
          (IRenderParameter) this.viewProjWithOffset,
          (IRenderParameter) this.viewWithOffset,
          (IRenderParameter) this.offset,
          (IRenderParameter) this.colorMatrix,
          (IRenderParameter) this.glowParameters,
          (IRenderParameter) this.glowColor,
          (IRenderParameter) this.replaceSourceColor,
          (IRenderParameter) this.replaceDestColor,
          (IRenderParameter) this.textureOffset,
          (IRenderParameter) this.colorReplaceEnable,
          (IRenderParameter) this.colorMapEnable,
          (IRenderParameter) this.glowEnable
        })
      });
      this.depthEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.TileRenderableDepth.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.TileRenderableDepth.ps"),
        VertexFormat = new TileVertex().Format,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[2]
        {
          (IRenderParameter) this.viewProjWithOffset,
          (IRenderParameter) this.viewWithOffset
        })
      });
      this.wireframeEffect = Effect.Create(new EffectDefinition()
      {
        VertexShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.TileRenderable.vs"),
        GeometryShaderData = (Stream) null,
        PixelShaderData = this.GetType().Assembly.GetManifestResourceStream("Microsoft.Data.Visualization.Engine.Shaders.Compiled.TileRenderableWireframe.ps"),
        VertexFormat = new TileVertex().Format,
        Samplers = (TextureSampler[]) null,
        Parameters = RenderParameters.Create(new IRenderParameter[12]
        {
          (IRenderParameter) this.viewProjWithOffset,
          (IRenderParameter) this.viewWithOffset,
          (IRenderParameter) this.offset,
          (IRenderParameter) this.colorMatrix,
          (IRenderParameter) this.glowParameters,
          (IRenderParameter) this.glowColor,
          (IRenderParameter) this.replaceSourceColor,
          (IRenderParameter) this.replaceDestColor,
          (IRenderParameter) this.textureOffset,
          (IRenderParameter) this.colorReplaceEnable,
          (IRenderParameter) this.colorMapEnable,
          (IRenderParameter) this.glowEnable
        })
      });
      this.depthEnabledState = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthFunction = ComparisonFunction.Less,
        DepthEnable = true,
        DepthWriteEnable = true
      });
      this.blendState = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = false,
        SourceBlend = BlendFactor.SourceAlpha,
        DestBlend = BlendFactor.InvSourceAlpha,
        BlendOp = BlendOperation.Add
      });
      this.blendStateAlpha = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = true,
        SourceBlend = BlendFactor.BlendFactor,
        DestBlend = BlendFactor.InvBlendFactor,
        BlendOp = BlendOperation.Add,
        SourceBlendAlpha = BlendFactor.One,
        DestBlendAlpha = BlendFactor.One,
        BlendOpAlpha = BlendOperation.Add,
        BlendFactor = new Color4F(0.7f, 0.7f, 0.7f, 0.7f)
      });
      this.flatteningBlendState = BlendState.Create(new BlendStateDescription()
      {
        BlendEnable = true,
        SourceBlend = BlendFactor.SourceAlpha,
        DestBlend = BlendFactor.InvSourceAlpha,
        SourceBlendAlpha = BlendFactor.One,
        DestBlendAlpha = BlendFactor.One,
        BlendOp = BlendOperation.Add,
        BlendOpAlpha = BlendOperation.Add
      });
      this.rasterizerState = RasterizerState.Create(new RasterizerStateDescription()
      {
        AntialiasedLineEnable = true,
        CullMode = CullMode.Back,
        FillMode = FillMode.Solid,
        MultisampleEnable = true
      });
      this.flatteningRasterizerState = RasterizerState.Create(new RasterizerStateDescription()
      {
        AntialiasedLineEnable = true,
        CullMode = CullMode.None,
        FillMode = FillMode.Solid,
        MultisampleEnable = true
      });
      this.wireframeRasterizerState = RasterizerState.Create(new RasterizerStateDescription()
      {
        AntialiasedLineEnable = true,
        CullMode = CullMode.Back,
        FillMode = FillMode.Wireframe,
        MultisampleEnable = true
      });
      this.wireframeDepthState = DepthStencilState.Create(new DepthStencilStateDescription()
      {
        DepthFunction = ComparisonFunction.LessEqual,
        DepthEnable = true,
        DepthWriteEnable = false
      });
    }

    public void AddRenderable(TileRenderable renderable)
    {
      this.renderables.Add(renderable);
    }

    public void RenderDepth(Microsoft.Data.Visualization.Engine.Graphics.Renderer renderer, SceneState state)
    {
      int width = (int) (state.ScreenWidth * 0.5);
      int height = (int) (state.ScreenHeight * 0.5);
      if (this.globeDepthTexture != null && (this.globeDepthTexture.Width != width || this.globeDepthTexture.Height != height))
      {
        this.globeDepthTexture.Dispose();
        this.globeDepthTexture = (Texture) null;
        this.globeDepthTarget.Dispose();
      }
      if (this.globeDepthTexture == null)
      {
        using (Image textureData = new Image(IntPtr.Zero, width, height, Microsoft.Data.Visualization.Engine.Graphics.PixelFormat.Float32Bpp))
          this.globeDepthTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
        this.globeDepthTarget = RenderTarget.Create(this.globeDepthTexture, RenderTargetDepthStencilMode.Enabled);
      }
      renderer.SetDepthStencilState(this.depthEnabledState);
      renderer.SetRasterizerState(this.rasterizerState);
      renderer.SetBlendState(this.blendState);
      renderer.Profiler.BeginSection("[Globe] Depth pass");
      renderer.SetEffect(this.depthEffect);
      renderer.BeginRenderTargetFrame(this.globeDepthTarget, new Color4F?(new Color4F(10f, 10f, 10f, 10f)));
      try
      {
        this.RenderTiles(renderer, state, true);
      }
      finally
      {
        renderer.EndRenderTargetFrame();
      }
      renderer.Profiler.EndSection();
    }

    public void CustomMapResourcesClear()
    {
      if (this.customMapTexture != null)
      {
        if (this.customMapTexture != this.defaultTexture)
          this.customMapTexture.Dispose();
        this.customMapTexture = (Texture) null;
      }
      if (this.customMapVertices != null)
      {
        this.customMapVertices.Dispose();
        this.customMapVertices = (VertexBuffer) null;
      }
      this.customMapRef = (CustomMap) null;
      this.customMapRefTextureVersion = -1;
      this.customMapRefVertices = (CustomSpaceTransform) null;
    }

    public void CustomMapTextureUpdate(Microsoft.Data.Visualization.Engine.Graphics.Renderer renderer, SceneState state)
    {
      bool flag = this.customMapRef != state.SceneCustomMap;
      if (this.customMapRef != null && state.SceneCustomMap != null && (this.customMapRefTextureVersion != state.SceneCustomMap.ImageVersion || this.customMapRefVertices != state.SceneCustomMap.GetTransformForBackground()))
        flag = true;
      if (flag)
      {
        this.CustomMapResourcesClear();
        this.customMapRef = state.SceneCustomMap;
        if (this.customMapRef != null)
        {
          this.customMapRefTextureVersion = this.customMapRef.ImageVersion;
          this.customMapRefVertices = this.customMapRef.GetTransformForBackground();
        }
      }
      if (this.customMapRef == null)
        return;
      if (this.customMapTexture == null)
      {
        if (this.customMapRef.HasImage)
        {
          try
          {
            FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(this.customMapRef.GenerateImageOnCurrentThread(), PixelFormats.Bgra32, (BitmapPalette) null, 0.0);
            int pixelWidth = formatConvertedBitmap.PixelWidth;
            int pixelHeight = formatConvertedBitmap.PixelHeight;
            int num1 = pixelWidth * pixelHeight * 4;
            IntPtr num2 = Marshal.AllocHGlobal(num1);
            formatConvertedBitmap.CopyPixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), num2, num1, pixelWidth * 4);
            Image textureData = new Image(num2, pixelWidth, pixelHeight, Microsoft.Data.Visualization.Engine.Graphics.PixelFormat.Bgra32Bpp);
            this.customMapTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.Static);
            this.customMapTexture.OnReset += (EventHandler) ((sender, ed) =>
            {
              if (this.customMapTexture == null || this.customMapTexture == this.defaultTexture)
                return;
              this.customMapTexture.Dispose();
              this.customMapTexture = (Texture) null;
            });
            textureData.Dispose();
          }
          catch (OutOfMemoryException ex)
          {
            this.customMapTexture = this.defaultTexture;
          }
        }
        else
          this.customMapTexture = this.defaultTexture;
      }
      if (this.customMapVertices != null)
        return;
      CustomSpaceTransform transformForBackground = this.customMapRef.GetTransformForBackground();
      TileVertex[] data = new TileVertex[12];
      data[0] = TileRenderer.CustomMapCorner(transformForBackground, 0.0, 0.0);
      data[1] = TileRenderer.CustomMapCorner(transformForBackground, 0.0, 1.0);
      data[2] = TileRenderer.CustomMapCorner(transformForBackground, 1.0, 1.0);
      data[3] = TileRenderer.CustomMapCorner(transformForBackground, 1.0, 1.0);
      data[4] = TileRenderer.CustomMapCorner(transformForBackground, 1.0, 0.0);
      data[5] = TileRenderer.CustomMapCorner(transformForBackground, 0.0, 0.0);
      data[6] = data[0];
      data[7] = data[2];
      data[8] = data[1];
      data[9] = data[4];
      data[10] = data[3];
      data[11] = data[5];
      this.customMapVertices = VertexBuffer.Create<TileVertex>(data, data.Length, false);
      this.customMapVertices.OnReset += (EventHandler) ((sender, ed) =>
      {
        if (this.customMapVertices == null)
          return;
        this.customMapVertices.Dispose();
        this.customMapVertices = (VertexBuffer) null;
      });
    }

    public static TileVertex CustomMapCorner(CustomSpaceTransform cs, double ux, double uy)
    {
      TileVertex tileVertex = new TileVertex();
      double to_lat_y;
      double to_long_x;
      cs.TransformSpaceTextureToDegrees(uy, ux, out to_lat_y, out to_long_x);
      Coordinates coordinates = Coordinates.FromDegrees(to_long_x, to_lat_y);
      tileVertex.Position = (Vector3F) Coordinates.GeoTo3DFlattening(coordinates.Longitude, coordinates.Latitude, 1.0);
      tileVertex.Tu = (float) (ux * 1.0 + 0.0);
      tileVertex.Tv = (float) (uy * -1.0 + 1.0);
      return tileVertex;
    }

    public void Render(Microsoft.Data.Visualization.Engine.Graphics.Renderer renderer, SceneState state, bool useAlpha)
    {
      this.InitializeDefaultTexture(renderer);
      this.CustomMapTextureUpdate(renderer, state);
      renderer.SetDepthStencilState(this.depthEnabledState);
      bool flag = !useAlpha && state.FlatteningFactor > 0.0 & state.FlatteningFactor < 1.0;
      renderer.SetBlendState(useAlpha ? this.blendStateAlpha : (flag ? this.flatteningBlendState : this.blendState));
      renderer.SetRasterizerState(flag ? this.flatteningRasterizerState : this.rasterizerState);
      this.colorMatrix.Value = this.postProcessingOps.GetMatrix();
      Texture rampTexture = this.postProcessingOps.GetRampTexture(renderer);
      this.colorMapEnable.Value = rampTexture != null;
      renderer.SetTexture(1, rampTexture);
      Tuple<Color4F, Color4F> colorReplacement = this.postProcessingOps.GetColorReplacement();
      this.colorReplaceEnable.Value = colorReplacement != null;
      if (this.colorReplaceEnable.Value)
      {
        this.replaceSourceColor.Value = !this.useWaterColorForReplace || !state.WaterColor.HasValue ? colorReplacement.Item1 : state.WaterColor.Value;
        this.replaceDestColor.Value = colorReplacement.Item2;
      }
      this.glowParameters.Value = new Vector4F(this.glowParameters.Value.X, this.glowParameters.Value.Y, this.glowFactor * (float) Math.Min(1.0, Math.Sqrt(state.CameraPosition.Length() - 1.0) * 3.165), this.glowParameters.Value.W);
      renderer.Profiler.BeginSection("[Globe] Color pass");
      renderer.SetEffect(this.effect);
      this.RenderTiles(renderer, state, false);
      if (this.WireframeEnabled)
      {
        renderer.SetRasterizerState(this.wireframeRasterizerState);
        renderer.SetDepthStencilState(this.wireframeDepthState);
        renderer.SetEffect(this.wireframeEffect);
        this.RenderTiles(renderer, state, false);
      }
      renderer.Profiler.EndSection();
      if (this.renderables.Count != 0)
        return;
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "No globe tiles were marked for rendering.");
    }

    private unsafe void InitializeDefaultTexture(Microsoft.Data.Visualization.Engine.Graphics.Renderer renderer)
    {
      if (this.defaultTexture != null)
        return;
      IntPtr data = Marshal.AllocHGlobal(16);
      uint* numPtr = (uint*) data.ToPointer();
      for (int index = 0; index < 4; ++index)
        *numPtr++ = this.DefaultColor.ToUint();
      using (Image textureData = new Image(data, 2, 2, Microsoft.Data.Visualization.Engine.Graphics.PixelFormat.Rgba32Bpp))
      {
        this.defaultTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.Static);
        this.defaultTexture.OnReset += new EventHandler(this.defaultTexture_OnReset);
      }
    }

    private void defaultTexture_OnReset(object sender, EventArgs e)
    {
      if (this.defaultTexture == null)
        return;
      this.defaultTexture.Dispose();
      this.defaultTexture = (Texture) null;
    }

    public void ClearRenderables()
    {
      this.renderables.Clear();
    }

    private void RenderTiles(Microsoft.Data.Visualization.Engine.Graphics.Renderer renderer, SceneState state, bool depthPass)
    {
      for (int i = 0; i < this.renderables.Count; ++i)
      {
        this.renderables[i].PlanarCoordinates = state.FlatteningFactor == 1.0;
        if (this.renderables[i].ReferencePoint != Vector3D.Empty)
        {
          Matrix4x4D matrix4x4D1 = Matrix4x4D.Translation(this.renderables[i].ReferencePoint);
          Matrix4x4D matrix4x4D2 = matrix4x4D1 * state.View;
          this.offset.Value = (Matrix4x4F) matrix4x4D1;
          this.viewWithOffset.Value = (Matrix4x4F) matrix4x4D2;
          this.viewProjWithOffset.Value = (Matrix4x4F) (matrix4x4D2 * state.Projection);
        }
        else
        {
          this.offset.Value = Matrix4x4F.Identity;
          this.viewWithOffset.Value = (Matrix4x4F) state.View;
          this.viewProjWithOffset.Value = (Matrix4x4F) state.ViewProjection;
        }
        if (!depthPass)
          this.ModulateTileColor(i);
        if (this.customMapTexture != null)
        {
          renderer.SetTexture(0, this.customMapTexture);
          renderer.SetVertexSource(this.customMapVertices);
          this.textureOffset.Value = new Vector4F(0.0f, 0.0f, 1f, 1f);
          renderer.Draw(0, 12, PrimitiveTopology.TriangleList);
          renderer.SetTexture(0, (Texture) null);
          renderer.SetVertexSource((VertexBuffer) null);
          break;
        }
        else
          this.renderables[i].Render(renderer, this.textureOffset, this.defaultTexture, this.customMapTexture);
      }
    }

    private void ModulateTileColor(int i)
    {
      if (!this.ModulateTileLevels)
        return;
      Color4F color4F = new Color4F(0.0f, 1f, 1f, 1f);
      switch (this.renderables[i].Owner.Level % 5)
      {
        case 0:
          color4F = new Color4F(1f, 1f, 0.0f, 0.0f);
          break;
        case 1:
          color4F = new Color4F(1f, 1f, 1f, 0.0f);
          break;
        case 2:
          color4F = new Color4F(1f, 0.0f, 1f, 0.0f);
          break;
        case 3:
          color4F = new Color4F(1f, 0.0f, 1f, 1f);
          break;
        case 4:
          color4F = new Color4F(1f, 0.0f, 0.0f, 1f);
          break;
      }
      if (this.renderables[i].Owner.X % 2 == 0)
        color4F = new Color4F(1f, color4F.R * 0.9f, color4F.G * 0.9f, color4F.B * 0.9f);
      if (this.renderables[i].Owner.Y % 2 == 0)
        color4F = new Color4F(1f, color4F.R * 0.8f, color4F.G * 0.8f, color4F.B * 0.8f);
      this.colorModulation.Color = color4F;
      this.colorMatrix.Value = (Matrix4x4F) this.colorModulation.GetMatrix();
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      this.CustomMapResourcesClear();
      DisposableResource[] disposableResourceArray = new DisposableResource[16]
      {
        (DisposableResource) this.effect,
        (DisposableResource) this.depthEffect,
        (DisposableResource) this.wireframeEffect,
        (DisposableResource) this.depthEnabledState,
        (DisposableResource) this.blendState,
        (DisposableResource) this.blendStateAlpha,
        (DisposableResource) this.flatteningBlendState,
        (DisposableResource) this.rasterizerState,
        (DisposableResource) this.wireframeRasterizerState,
        (DisposableResource) this.flatteningRasterizerState,
        (DisposableResource) this.wireframeDepthState,
        (DisposableResource) this.globeDepthTexture,
        (DisposableResource) this.globeDepthTarget,
        (DisposableResource) this.postProcessingOps,
        (DisposableResource) this.colorModulation,
        (DisposableResource) this.defaultTexture
      };
      for (int index = 0; index < disposableResourceArray.Length; ++index)
      {
        if (disposableResourceArray[index] != null)
          disposableResourceArray[index].Dispose();
      }
    }
  }
}
