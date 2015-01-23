// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CustomMap
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  public class CustomMap : CompositePropertyChangeNotificationBase
  {
    public static readonly Guid InvalidMapId = Guid.Empty;
    private string _ImageDisplayName = "";
    private double _ImageWidthOverHeight = 1.0;
    private CustomSpaceDefinition _CustomSpaceDef = new CustomSpaceDefinition();
    private int mTransformVersion = 1;
    private string _Name;
    private bool _IsTemporary;
    private volatile CustomSpaceDefinition mFirstAutoDef;
    private CustomSpaceTransform _LatestCustomSpaceTransform;
    private BitmapSource _ImageForUI;

    public Guid UniqueCustomMapId { get; private set; }

    public static string PropertyName
    {
      get
      {
        return "Name";
      }
    }

    public string Name
    {
      get
      {
        return this._Name;
      }
      set
      {
        if (!base.SetProperty<string>(CustomMap.PropertyName, ref this._Name, value))
          return;
        this.MarkAsDirty();
      }
    }

    public static string PropertyImageDisplayName
    {
      get
      {
        return "ImageDisplayName";
      }
    }

    public string ImageDisplayName
    {
      get
      {
        return this._ImageDisplayName;
      }
      set
      {
        if (!base.SetProperty<string>(CustomMap.PropertyImageDisplayName, ref this._ImageDisplayName, value))
          return;
        this.MarkAsDirty();
      }
    }

    public static string PropertyImageWidthOverHeight
    {
      get
      {
        return "ImageWidthOverHeight";
      }
    }

    public double ImageWidthOverHeight
    {
      get
      {
        return this._ImageWidthOverHeight;
      }
      set
      {
        if (!base.SetProperty<double>(CustomMap.PropertyImageWidthOverHeight, ref this._ImageWidthOverHeight, value))
          return;
        this.UpdateTransform();
      }
    }

    public static string PropertyIsTemporary
    {
      get
      {
        return "IsTemporary";
      }
    }

    public bool IsTemporary
    {
      get
      {
        return this._IsTemporary;
      }
      set
      {
        base.SetProperty<bool>(CustomMap.PropertyIsTemporary, ref this._IsTemporary, value);
      }
    }

    public static string PropertyCustomSpaceDef
    {
      get
      {
        return "CustomSpaceDef";
      }
    }

    public CustomSpaceDefinition CustomSpaceDef
    {
      get
      {
        return this._CustomSpaceDef;
      }
      set
      {
        if (value == null || !base.SetProperty<CustomSpaceDefinition>(CustomMap.PropertyCustomSpaceDef, ref this._CustomSpaceDef, value))
          return;
        this.MarkAsDirty();
        this.UpdateTransform();
      }
    }

    public string Base64ImageData { get; set; }

    public ICustomMapCollection OwningMapsList { get; set; }

    public bool HasImage
    {
      get
      {
        return !string.IsNullOrWhiteSpace(this.Base64ImageData);
      }
    }

    private CustomSpaceDefinition CustomSpaceDef_OnAnyThread
    {
      get
      {
        CustomSpaceDefinition customSpaceDefinition1 = this.CustomSpaceDef;
        if (customSpaceDefinition1.IsCalibrateOnFirst && !customSpaceDefinition1.IsLocked)
        {
          CustomSpaceDefinition customSpaceDefinition2 = this.mFirstAutoDef;
          if (customSpaceDefinition2 != null)
          {
            customSpaceDefinition1 = customSpaceDefinition1.Clone();
            customSpaceDefinition1.IsCalibrateOnFirst = false;
            customSpaceDefinition1.AxisX.AxisRange = customSpaceDefinition2.AxisX.AxisRange.Clone();
            customSpaceDefinition1.AxisY.AxisRange = customSpaceDefinition2.AxisY.AxisRange.Clone();
          }
        }
        return customSpaceDefinition1;
      }
    }

    private CustomSpaceTransform LatestFixedTransform
    {
      get
      {
        if (this._LatestCustomSpaceTransform != null)
          return this._LatestCustomSpaceTransform;
        else
          return this.UpdateTransform();
      }
      set
      {
        this._LatestCustomSpaceTransform = value;
        ++this.TransformVersion;
      }
    }

    public int TransformVersion
    {
      get
      {
        return this.mTransformVersion;
      }
      set
      {
        Interlocked.Exchange(ref this.mTransformVersion, value);
      }
    }

    public int ImageVersion { get; set; }

    public BitmapSource ImageForUI
    {
      get
      {
        if (this._ImageForUI == null && this.HasImage)
          this._ImageForUI = this.GenerateImageOnCurrentThread();
        return this._ImageForUI;
      }
    }

    public event Action<CustomSpaceDefinition> LatestAutoDefUpdated;

    public CustomMap(Guid uniqueId, bool startAsAutoFit)
    {
      this.UniqueCustomMapId = uniqueId;
      this.CustomSpaceDef.IsCalibrateOnFirst = startAsAutoFit;
    }

    protected CustomMap()
    {
    }

    public CustomMap(CustomMap.SerializableCustomMap scm)
    {
      this.UniqueCustomMapId = scm.UniqueCustomMapId;
      this.Name = scm.Name;
      this.ImageDisplayName = scm.ImageDisplayName;
      this.ImageWidthOverHeight = scm.ImageWidthOverHeight;
      this.IsTemporary = false;
      this.CustomSpaceDef = scm.CustomSpaceDef.Unwrap();
      this.Base64ImageData = scm.Base64ImageData;
    }

    public CustomMap.SerializableCustomMap Wrap()
    {
      return new CustomMap.SerializableCustomMap()
      {
        UniqueCustomMapId = this.UniqueCustomMapId,
        Name = this.Name,
        ImageDisplayName = this.ImageDisplayName,
        ImageWidthOverHeight = this.ImageWidthOverHeight,
        IsTemporary = false,
        CustomSpaceDef = this.CustomSpaceDef_OnAnyThread.Wrap(),
        Base64ImageData = this.Base64ImageData
      };
    }

    public void EnsureUpdatedOnUIThread()
    {
      if (this.CustomSpaceDef.IsLocked || !this.CustomSpaceDef.IsCalibrateOnFirst || this.mFirstAutoDef == null)
        return;
      this.CustomSpaceDef = this.CustomSpaceDef_OnAnyThread;
      this.MarkAsDirty();
    }

    public void ResetToAutoFit()
    {
      CustomSpaceDefinition customSpaceDef = this.CustomSpaceDef;
      customSpaceDef.AxisX.AxisRange = new RangeOf<double>(-180.0, 180.0);
      customSpaceDef.AxisX.ScaleOffsetPct = 0.0;
      customSpaceDef.AxisX.ScalePct = 1.0;
      customSpaceDef.AxisX.IsAxisFlipped = false;
      customSpaceDef.AxisY.AxisRange = new RangeOf<double>(-90.0, 90.0);
      customSpaceDef.AxisY.ScaleOffsetPct = 0.0;
      customSpaceDef.AxisY.ScalePct = 1.0;
      customSpaceDef.AxisY.IsAxisFlipped = false;
      customSpaceDef.IsLocked = false;
      customSpaceDef.IsSwapXandY = false;
      customSpaceDef.IsCalibrateOnFirst = true;
      this.mFirstAutoDef = (CustomSpaceDefinition) null;
    }

    private CustomSpaceTransform UpdateTransform()
    {
      CustomSpaceTransform transformForDefinition = this.GetTransformForDefinition(this.CustomSpaceDef_OnAnyThread);
      this.LatestFixedTransform = transformForDefinition;
      return transformForDefinition;
    }

    public CustomSpaceTransform GetTransformForBackground()
    {
      return this.LatestFixedTransform;
    }

    public CustomSpaceTransform GetTransformOrNullForAuto()
    {
      CustomSpaceDefinition spaceDefOnAnyThread = this.CustomSpaceDef_OnAnyThread;
      if (spaceDefOnAnyThread.IsAnyAutoCalculated)
        return (CustomSpaceTransform) null;
      if (!spaceDefOnAnyThread.IsCalibrateOnFirst)
        return this.LatestFixedTransform;
      if (this.mFirstAutoDef == null)
        return (CustomSpaceTransform) null;
      else
        return this.GetTransformForDefinition(this.mFirstAutoDef);
    }

    public CustomSpaceTransform GetTransformFromAutoRanges(RangeOf<double> rangeLat, RangeOf<double> rangeLong)
    {
      if (rangeLong != null && (double.IsNaN(rangeLong.From) || double.IsNaN(rangeLong.To)))
        rangeLong = (RangeOf<double>) null;
      if (rangeLat != null && (double.IsNaN(rangeLat.From) || double.IsNaN(rangeLat.To)))
        rangeLat = (RangeOf<double>) null;
      if (rangeLat == null && rangeLong == null)
        return this.LatestFixedTransform;
      CustomSpaceDefinition csd = this.CustomSpaceDef_OnAnyThread.Clone();
      if (rangeLong != null)
        csd.AxisX.AxisRange = rangeLong;
      if (rangeLat != null)
        csd.AxisY.AxisRange = rangeLat;
      if (csd.IsCalibrateOnFirst)
      {
        if (this.mFirstAutoDef != null)
        {
          if (!csd.IsAnyAutoCalculated)
            csd = this.mFirstAutoDef;
        }
        else if (!csd.IsLocked)
        {
          this.mFirstAutoDef = csd;
          if (this.LatestAutoDefUpdated != null)
            this.LatestAutoDefUpdated(csd);
        }
      }
      return this.GetTransformForDefinition(csd);
    }

    private CustomSpaceTransform GetTransformForDefinition(CustomSpaceDefinition csd)
    {
      return new CustomSpaceTransform(csd, this.ImageWidthOverHeight);
    }

    public BitmapSource GenerateImageOnCurrentThread()
    {
      if (this.HasImage)
        return CustomMap.Base64ToImage(this.Base64ImageData);
      else
        return (BitmapSource) null;
    }

    public void SetImageFromUI(BitmapImage img, string imgDisplayName)
    {
      this.ImageDisplayName = imgDisplayName;
      this.Base64ImageData = CustomMap.ImageToBase64_Lossy((BitmapSource) img);
      this._ImageForUI = (BitmapSource) img;
      ++this.ImageVersion;
      this.ImageWidthOverHeight = (double) img.PixelWidth / (double) img.PixelHeight;
      this.IsTemporary = false;
      this.MarkAsDirty();
      this.UpdateTransform();
      this.RaisePropertyChanged("ImageForUI");
      this.RaisePropertyChanged("HasImage");
    }

    public void MarkAsDirty()
    {
      if (this.OwningMapsList == null)
        return;
      this.OwningMapsList.MarkMapListAsDirty();
    }

    private static string ImageToBase64_Lossy(BitmapSource bitmap)
    {
      if (bitmap == null)
        return string.Empty;
      BitmapEncoder bitmapEncoder = bitmap.PixelHeight > 700 || bitmap.PixelWidth > 700 ? (BitmapEncoder) new JpegBitmapEncoder() : (BitmapEncoder) new PngBitmapEncoder();
      BitmapFrame bitmapFrame = BitmapFrame.Create(bitmap);
      bitmapEncoder.Frames.Add(bitmapFrame);
      using (MemoryStream memoryStream = new MemoryStream())
      {
        bitmapEncoder.Save((Stream) memoryStream);
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

    [XmlType("CustomMap")]
    [Serializable]
    public class SerializableCustomMap
    {
      [XmlAttribute("UniqueCustomMapId")]
      public Guid UniqueCustomMapId { get; set; }

      [XmlAttribute("Name")]
      public string Name { get; set; }

      [XmlAttribute("ImageDisplayName")]
      public string ImageDisplayName { get; set; }

      [XmlAttribute("ImageWidthOverHeight")]
      public double ImageWidthOverHeight { get; set; }

      [XmlAttribute("IsTemporary")]
      public bool IsTemporary { get; set; }

      [XmlElement("CustomSpaceDef")]
      public CustomSpaceDefinition.SerializableCustomSpaceDefinition CustomSpaceDef { get; set; }

      [XmlElement("ImageData")]
      public string Base64ImageData { get; set; }

      public CustomMap Unwrap()
      {
        return new CustomMap(this);
      }
    }
  }
}
