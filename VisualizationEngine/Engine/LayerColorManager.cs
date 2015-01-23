// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LayerColorManager
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class LayerColorManager : DisposableResource
  {
    private Color4F[] customColors = new Color4F[1024];
    private const int MaxVisualColors = 3072;
    private RenderParameterVector4F[] colorParameters;
    private RenderParameters colorBuffer;
    private Color4F[] visualColors;

    public RenderParameters GetColors(out int count)
    {
      count = this.visualColors.Length;
      return this.colorBuffer;
    }

    public Color4F GetColor(int index)
    {
      return this.visualColors[index % this.visualColors.Length];
    }

    public void SetVisualColors(IList<Color4F> colors, IList<double> lightnessSteps)
    {
      Color4F[] color4FArray = new Color4F[Math.Min(colors.Count * lightnessSteps.Count, 3072)];
      int num = 0;
      for (int index1 = 0; index1 < lightnessSteps.Count; ++index1)
      {
        for (int index2 = 0; index2 < colors.Count; ++index2)
        {
          if (num < color4FArray.Length)
            color4FArray[num++] = Color4F.ApplyLightnessFactor(colors[index2], lightnessSteps[index1]) ?? new Color4F(1f, 1f, 1f, 1f);
        }
      }
      this.visualColors = color4FArray;
      this.UpdateColorBuffer();
    }

    public int AddColor(Color4F color)
    {
      for (int index = 0; index < this.customColors.Length; ++index)
      {
        if ((int) this.customColors[index].ToUint() == 0)
        {
          this.customColors[index] = color;
          this.UpdateColorBuffer();
          return -(index + 3072);
        }
      }
      return 0;
    }

    public bool RemoveColor(int colorIndex)
    {
      if (colorIndex > -3072 || colorIndex <= -4096)
        return false;
      this.customColors[-colorIndex - 3072] = new Color4F();
      return true;
    }

    private void UpdateColorBuffer()
    {
      if (this.colorParameters == null)
      {
        this.colorParameters = new RenderParameterVector4F[4096];
        for (int index = 0; index < this.colorParameters.Length; ++index)
          this.colorParameters[index] = new RenderParameterVector4F("InstanceColor");
        if (this.colorBuffer != null)
          this.colorBuffer.Dispose();
        this.colorBuffer = RenderParameters.Create((IRenderParameter[]) this.colorParameters);
      }
      for (int index = 0; index < this.visualColors.Length; ++index)
        this.colorParameters[index].Value = new Vector4F(this.visualColors[index].R, this.visualColors[index].G, this.visualColors[index].B, this.visualColors[index].A);
      for (int index = 0; index < this.customColors.Length; ++index)
        this.colorParameters[index + 3072].Value = new Vector4F(this.customColors[index].R, this.customColors[index].G, this.customColors[index].B, this.customColors[index].A);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing || this.colorBuffer == null)
        return;
      this.colorBuffer.Dispose();
      this.colorBuffer = (RenderParameters) null;
    }
  }
}
