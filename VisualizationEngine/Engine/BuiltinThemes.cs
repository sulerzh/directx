// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.BuiltinThemes
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.VisualizationCommon;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class BuiltinThemes : DisposableResource
  {
    private Dictionary<BuiltinTheme, BuiltinThemeDescription> themes = new Dictionary<BuiltinTheme, BuiltinThemeDescription>();
    private Dictionary<BuiltinTheme, BuiltinThemeDescription> themesWithLabels = new Dictionary<BuiltinTheme, BuiltinThemeDescription>();

    public BuiltinThemes(BingMapResourceUri url)
    {
      this.BuildThemes(url);
      this.BuildThemesWithLabels(url);
    }

    internal void BuildThemes(BingMapResourceUri url)
    {
      this.themes.Clear();
      if (url.RoadWithoutLabelsTilesUri != null)
      {
        this.themes.Add(BuiltinTheme.BingRoad, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.BING.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.Dark, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Dark.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = true,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.Light, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Light.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.White, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.White.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.Mono, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Mono.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.Earthy, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Earthy.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.Modern, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Modern.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.Organic, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Organic.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.Radiate, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Radiate.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, false),
          IsDark = true,
          IsWithLabels = false
        });
        this.themes.Add(BuiltinTheme.BingRoadHighContrast, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.BINGROADHIGHCONTRAST.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.RoadWithoutLabels, url.RoadWithoutLabelsTilesUri, true),
          IsDark = true,
          IsWithLabels = false
        });
      }
      if (url.AerialWithoutLabelsTilesUri == null)
        return;
      this.themes.Add(BuiltinTheme.Aerial, new BuiltinThemeDescription()
      {
        ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.AERIAL.xml",
        ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Aerial, url.AerialWithoutLabelsTilesUri, false),
        IsDark = true,
        IsWithLabels = false
      });
      this.themes.Add(BuiltinTheme.Grey, new BuiltinThemeDescription()
      {
        ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.AERIALDESAT.xml",
        ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Aerial, url.AerialWithoutLabelsTilesUri, false),
        IsDark = true,
        IsWithLabels = false
      });
    }

    internal void BuildThemesWithLabels(BingMapResourceUri url)
    {
      this.themesWithLabels.Clear();
      if (url.RoadWithLabelsTilesUri != null)
      {
        this.themesWithLabels.Add(BuiltinTheme.BingRoad, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.BING.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.Dark, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Dark.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = true,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.Light, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Light.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.White, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.White.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.Mono, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Mono.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.Earthy, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Earthy.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.Modern, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Modern.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.Organic, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Organic.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = false,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.Radiate, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.Radiate.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, false),
          IsDark = true,
          IsWithLabels = true
        });
        this.themesWithLabels.Add(BuiltinTheme.BingRoadHighContrast, new BuiltinThemeDescription()
        {
          ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.BINGROADHIGHCONTRAST.xml",
          ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.Road, url.RoadWithLabelsTilesUri, true),
          IsDark = true,
          IsWithLabels = true
        });
      }
      if (url.AerialWithLabelsTilesUri == null)
        return;
      this.themesWithLabels.Add(BuiltinTheme.Aerial, new BuiltinThemeDescription()
      {
        ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.AERIAL.xml",
        ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.AerialWithLabels, url.AerialWithLabelsTilesUri, false),
        IsDark = true,
        IsWithLabels = true
      });
      this.themesWithLabels.Add(BuiltinTheme.Grey, new BuiltinThemeDescription()
      {
        ResourcePath = "Microsoft.Data.Visualization.Engine.Themes.Resources.AERIALDESAT.xml",
        ImageSet = ImageSet.CreateBingMapsImageSet(ImagerySet.AerialWithLabels, url.AerialWithLabelsTilesUri, false),
        IsDark = true,
        IsWithLabels = true
      });
    }

    public string GetResourcePath(BuiltinTheme requestedTheme)
    {
      BuiltinThemeDescription themeDescription = this.GetAvailableThemeDescription(requestedTheme, false);
      if (themeDescription == null)
        return (string) null;
      else
        return themeDescription.ResourcePath;
    }

    public ImageSet GetImageSet(BuiltinTheme requestedTheme, bool isWithLabelsRequested)
    {
      BuiltinThemeDescription themeDescription = this.GetAvailableThemeDescription(requestedTheme, isWithLabelsRequested);
      if (themeDescription == null)
        return (ImageSet) null;
      else
        return themeDescription.ImageSet;
    }

    public bool IsWithLabels(BuiltinTheme requestedTheme, bool isWithLabelsRequested)
    {
      BuiltinThemeDescription themeDescription = this.GetAvailableThemeDescription(requestedTheme, isWithLabelsRequested);
      if (themeDescription == null)
        return false;
      else
        return themeDescription.IsWithLabels;
    }

    private BuiltinThemeDescription GetAvailableThemeDescription(BuiltinTheme theme, bool isWithLabels)
    {
      BuiltinThemeDescription themeDescription = (BuiltinThemeDescription) null;
      if (isWithLabels)
        this.themesWithLabels.TryGetValue(theme, out themeDescription);
      else
        this.themes.TryGetValue(theme, out themeDescription);
      if (themeDescription == null)
      {
        if (!isWithLabels)
          this.themesWithLabels.TryGetValue(theme, out themeDescription);
        else
          this.themes.TryGetValue(theme, out themeDescription);
      }
      if (themeDescription == null)
      {
        BuiltinTheme key;
        switch (theme)
        {
          case BuiltinTheme.None:
          case BuiltinTheme.BingRoad:
          case BuiltinTheme.BingRoadHighContrast:
          case BuiltinTheme.Dark:
          case BuiltinTheme.Light:
          case BuiltinTheme.Mono:
          case BuiltinTheme.White:
          case BuiltinTheme.Earthy:
          case BuiltinTheme.Modern:
          case BuiltinTheme.Organic:
          case BuiltinTheme.Radiate:
            key = BuiltinTheme.Aerial;
            break;
          default:
            key = BuiltinTheme.BingRoad;
            break;
        }
        if (isWithLabels)
          this.themesWithLabels.TryGetValue(key, out themeDescription);
        else
          this.themes.TryGetValue(key, out themeDescription);
        if (themeDescription == null)
        {
          if (!isWithLabels)
            this.themesWithLabels.TryGetValue(key, out themeDescription);
          else
            this.themes.TryGetValue(key, out themeDescription);
        }
      }
      return themeDescription;
    }

    public bool IsDark(BuiltinTheme theme)
    {
      BuiltinThemeDescription themeDescription = (BuiltinThemeDescription) null;
      if (this.themes.TryGetValue(theme, out themeDescription))
        return themeDescription.IsDark;
      else
        return false;
    }

    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      if (this.themes != null)
      {
        foreach (BuiltinThemeDescription themeDescription in this.themes.Values)
        {
          if (themeDescription != null && themeDescription.ImageSet != null)
            themeDescription.ImageSet.Dispose();
        }
        this.themes.Clear();
      }
      if (this.themesWithLabels == null)
        return;
      foreach (BuiltinThemeDescription themeDescription in this.themesWithLabels.Values)
      {
        if (themeDescription != null && themeDescription.ImageSet != null)
          themeDescription.ImageSet.Dispose();
      }
      this.themesWithLabels.Clear();
    }
  }
}
