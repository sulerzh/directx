// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ImageSet
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Data.Visualization.Engine
{
  internal class ImageSet : DisposableResource
  {
    private TileProjection proj;

    public int BaseLevel { get; private set; }

    public string Extension { get; private set; }

    public int ImageSetID { get; private set; }

    public ImagerySet ImagerySet { get; private set; }

    public string Name { get; private set; }

    public ProjectionType Projection { get; private set; }

    public string QuadTreeTileMap { get; private set; }

    public string Url { get; private set; }

    private ImageSet(string name, string url, ProjectionType projection, int imageSetID, int baseLevel, string extension, string quadTreeMap)
    {
      this.Name = name;
      this.QuadTreeTileMap = quadTreeMap;
      this.Url = url;
      this.ImageSetID = imageSetID;
      this.Extension = extension;
      this.Projection = projection;
      this.BaseLevel = baseLevel;
    }

    private static string GetImageSetURL(ImagerySet set, string imageUrl, bool isHC, out ImagerySet actualSet)
    {
      imageUrl = imageUrl.Replace("{0-3}", "{0}");
      imageUrl = imageUrl.Replace("&shading=hill", "&shading=flat");
      imageUrl = imageUrl.Replace("{quadkey}", "{1}");
      imageUrl = Regex.Replace(imageUrl, "&it=.+?&", (MatchEvaluator) (match =>
      {
        switch (set)
        {
          case ImagerySet.Aerial:
            return "&it=A&";
          case ImagerySet.AerialWithLabels:
            return "&it=A,L&";
          case ImagerySet.Road:
            return "&it=G,L&";
          case ImagerySet.RoadWithoutLabels:
            return "&it=G&";
          default:
            return (string) null;
        }
      }));
      if (isHC)
        imageUrl = imageUrl + "&cstl=hc";
      actualSet = set;
      return imageUrl;
    }

    public static ImageSet CreateBingMapsImageSet(ImagerySet set, string bingUrl, bool isHighContrast = false)
    {
      if (string.IsNullOrEmpty(bingUrl))
        return (ImageSet) null;
      try
      {
        ImagerySet actualSet;
        string imageSetUrl = ImageSet.GetImageSetURL(set, bingUrl, isHighContrast, out actualSet);
        return new ImageSet("BingFlat", imageSetUrl, ProjectionType.Mercator, Math.Abs(imageSetUrl.GetHashCode()), 1, ".jpeg", "0123")
        {
          ImagerySet = actualSet
        };
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.Fail("Exception while loading a new Image Set.", ex);
        return (ImageSet) null;
      }
    }

    public static string GetTileKey(ImageSet imageset, int level, int x, int y)
    {
      return imageset.ImageSetID.ToString((IFormatProvider) CultureInfo.InvariantCulture) + "\\" + level.ToString((IFormatProvider) CultureInfo.InvariantCulture) + "\\" + y.ToString((IFormatProvider) CultureInfo.InvariantCulture) + "_" + x.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public Tile GetNewTile(int level, int x, int y, Tile parent, TileCache cache)
    {
      if (this.Projection != ProjectionType.Mercator)
        return (Tile) null;
      if (this.proj == null)
        this.proj = (TileProjection) new MercatorTileProjection();
      return new Tile(level, x, y, this, parent, this.proj, cache);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing || this.proj == null)
        return;
      this.proj.Dispose();
    }
  }
}
