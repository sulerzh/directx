// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Frame
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  [Serializable]
  public class Frame : CompositePropertyChangeNotificationBase
  {
    private BitmapSource _Image;

    [XmlElement("Camera")]
    public CameraSnapshot Camera { get; set; }

    public static string PropertyImage
    {
      get
      {
        return "Image";
      }
    }

    [XmlIgnore]
    public BitmapSource Image
    {
      get
      {
        if (this._Image == null)
          this._Image = Frame.Base64ToImage(this.Base64Image);
        return this._Image;
      }
      set
      {
        if (!base.SetProperty<BitmapSource>(Frame.PropertyImage, ref this._Image, value))
          return;
        this.Base64Image = Frame.ImageToBase64(this._Image);
      }
    }

    [XmlElement("Image")]
    public string Base64Image { get; set; }

    private static string ImageToBase64(BitmapSource bitmap)
    {
      if (bitmap == null)
        return string.Empty;
      PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
      BitmapFrame bitmapFrame = BitmapFrame.Create(bitmap);
      pngBitmapEncoder.Frames.Add(bitmapFrame);
      using (MemoryStream memoryStream = new MemoryStream())
      {
        pngBitmapEncoder.Save((Stream) memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
      }
    }

    private static BitmapSource Base64ToImage(string base64)
    {
      if (string.IsNullOrEmpty(base64))
        return (BitmapSource) null;
      using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(base64)))
        return (BitmapSource) BitmapFrame.Create((Stream) memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
    }

    public Frame Clone()
    {
      return new Frame()
      {
        Base64Image = this.Base64Image,
        Camera = this.Camera
      };
    }
  }
}
