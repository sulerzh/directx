using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ThemeGalleryViewModel : ViewModelBase
  {
    private VisualizationEngine engine;
    private BingMapResourceUri bingMapResourceUri;

    public string PropertyUseLabels
    {
      get
      {
        return "UseLabels";
      }
    }

    public bool UseLabels
    {
      get
      {
        return this.engine.CurrentThemeWithLabels;
      }
      set
      {
        this.engine.SetTheme(this.engine.CurrentTheme, value);
      }
    }

    public string PropertyLabelsEnabled
    {
      get
      {
        return "LabelsEnabled";
      }
    }

    public bool LabelsEnabled
    {
      get
      {
        if (this.CanUseLabels(this.engine.CurrentTheme))
          return this.CanUseNoLabels(this.engine.CurrentTheme);
        else
          return false;
      }
    }

    public string PropertyCanUseAerial
    {
      get
      {
        return "CanUseAerial";
      }
    }

    public bool CanUseAerial
    {
      get
      {
        if (this.bingMapResourceUri.AerialWithLabelsTilesUri == null)
          return this.bingMapResourceUri.AerialWithoutLabelsTilesUri != null;
        else
          return true;
      }
    }

    public string PropertyCanUseRoad
    {
      get
      {
        return "CanUseRoad";
      }
    }

    public bool CanUseRoad
    {
      get
      {
        if (this.bingMapResourceUri.RoadWithLabelsTilesUri == null)
          return this.bingMapResourceUri.RoadWithoutLabelsTilesUri != null;
        else
          return true;
      }
    }

    public string PropertySelectedTheme
    {
      get
      {
        return "SelectedTheme";
      }
    }

    public Theme SelectedTheme
    {
      get
      {
        return Theme.FromThemeEnum(this.engine.CurrentTheme);
      }
      set
      {
        this.engine.SetTheme(value.ThemeEnum, this.engine.CurrentThemeWithLabels ? this.CanUseLabels(value.ThemeEnum) : !this.CanUseNoLabels(value.ThemeEnum));
        this.RaisePropertyChanged(this.PropertyLabelsEnabled);
        this.RaisePropertyChanged(this.PropertyUseLabels);
      }
    }

    public ThemeGalleryViewModel(VisualizationEngine engine, BingMapResourceUri bingMapResourceUri)
    {
      this.engine = engine;
      this.bingMapResourceUri = bingMapResourceUri;
      this.engine.ThemeChanged += (Action<BuiltinTheme, VisualizationTheme, bool>) ((t, vt, l) =>
      {
        this.RaisePropertyChanged(this.PropertyUseLabels);
        this.RaisePropertyChanged(this.PropertySelectedTheme);
        this.RaisePropertyChanged(this.PropertyLabelsEnabled);
      });
      this.engine.SetTheme(BuiltinTheme.BingRoad, false);
    }

    private bool CanUseLabels(BuiltinTheme builtinTheme)
    {
      return Theme.FromThemeEnum(builtinTheme).IsAerial && this.bingMapResourceUri.AerialWithLabelsTilesUri != null || !Theme.FromThemeEnum(builtinTheme).IsAerial && this.bingMapResourceUri.RoadWithLabelsTilesUri != null;
    }

    private bool CanUseNoLabels(BuiltinTheme builtinTheme)
    {
      return Theme.FromThemeEnum(builtinTheme).IsAerial && this.bingMapResourceUri.AerialWithoutLabelsTilesUri != null || !Theme.FromThemeEnum(builtinTheme).IsAerial && this.bingMapResourceUri.RoadWithoutLabelsTilesUri != null;
    }
  }
}
