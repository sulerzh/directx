// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ColorMap
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  public class ColorMap : ColorOperation
  {
    private Texture texture;
    private Color4F[] colorMap;
    private bool updated;

    [XmlArrayItem("Color", typeof (Color4F))]
    [XmlArray("Map")]
    public Color4F[] Map
    {
      get
      {
        return this.colorMap;
      }
      set
      {
        this.colorMap = value;
        this.updated = true;
      }
    }

    internal Texture GetTexture(Renderer renderer)
    {
      if (this.updated)
      {
        this.updated = false;
        if (this.texture != null)
        {
          this.texture.Dispose();
          this.texture = (Texture) null;
        }
        if (this.colorMap == null || this.colorMap.Length == 0)
          return (Texture) null;
        using (Image textureData = new Image(this.GetTextureData(), this.colorMap.Length, 1, PixelFormat.Rgba32Bpp))
        {
          this.texture = renderer.CreateTexture(textureData, false, false, TextureUsage.Static);
          this.texture.OnReset += new EventHandler(this.texture_OnReset);
        }
      }
      return this.texture;
    }

    private unsafe IntPtr GetTextureData()
    {
      IntPtr num = Marshal.AllocHGlobal(4 * this.colorMap.Length);
      uint* numPtr = (uint*) num.ToPointer();
      for (int index = 0; index < this.colorMap.Length; ++index)
        numPtr[index] = this.colorMap[index].ToUint();
      return num;
    }

    private void texture_OnReset(object sender, EventArgs e)
    {
      this.texture.Update(new Image(this.GetTextureData(), this.colorMap.Length, 1, PixelFormat.Rgba32Bpp), true);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this.texture == null)
        return;
      this.texture.Dispose();
    }
  }
}
