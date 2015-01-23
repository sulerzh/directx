// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Scene
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  [Serializable]
  public class Scene : CompositePropertyChangeNotificationBase
  {
    private Guid _CustomMapId = CustomMap.InvalidMapId;
    private string _Name;
    private Transition _TransitionType;
    private SceneEffect _EffectType;
    private BuiltinTheme _ThemeId;
    private bool _ThemeWithLabel;
    private bool _FlatModeEnabled;
    private TimeSpan _Duration;
    private TimeSpan _TransitionDuration;
    private double _EffectSpeed;
    private Frame _Frame;
    private string _LayersContent;

    public static string PropertyName
    {
      get
      {
        return "Name";
      }
    }

    [XmlAttribute("Name")]
    public string Name
    {
      get
      {
        return this._Name;
      }
      set
      {
        base.SetProperty<string>(Scene.PropertyName, ref this._Name, value);
      }
    }

    public static string PropertyTransition
    {
      get
      {
        return "Transition";
      }
    }

    [XmlElement("Transition")]
    public Transition TransitionType
    {
      get
      {
        return this._TransitionType;
      }
      set
      {
        base.SetProperty<Transition>(Scene.PropertyTransition, ref this._TransitionType, value);
      }
    }

    public static string PropertyEffectType
    {
      get
      {
        return "EffectType";
      }
    }

    [XmlElement("Effect")]
    public SceneEffect EffectType
    {
      get
      {
        return this._EffectType;
      }
      set
      {
        base.SetProperty<SceneEffect>(Scene.PropertyEffectType, ref this._EffectType, value);
      }
    }

    public static string PropertyThemeId
    {
      get
      {
        return "ThemeId";
      }
    }

    [XmlElement("Theme")]
    public BuiltinTheme ThemeId
    {
      get
      {
        return this._ThemeId;
      }
      set
      {
        base.SetProperty<BuiltinTheme>(Scene.PropertyThemeId, ref this._ThemeId, value);
      }
    }

    public static string PropertyThemeWithLabel
    {
      get
      {
        return "ThemeWithLabel";
      }
    }

    [XmlElement("ThemeWithLabel")]
    public bool ThemeWithLabel
    {
      get
      {
        return this._ThemeWithLabel;
      }
      set
      {
        base.SetProperty<bool>(Scene.PropertyThemeWithLabel, ref this._ThemeWithLabel, value);
      }
    }

    public static string PropertyFlatModeEnabled
    {
      get
      {
        return "FlatModeEnabled";
      }
    }

    [XmlElement("FlatModeEnabled")]
    public bool FlatModeEnabled
    {
      get
      {
        return this._FlatModeEnabled;
      }
      set
      {
        base.SetProperty<bool>(Scene.PropertyFlatModeEnabled, ref this._FlatModeEnabled, value);
      }
    }

    public static string PropertyDuration
    {
      get
      {
        return "Duration";
      }
    }

    [XmlIgnore]
    public TimeSpan Duration
    {
      get
      {
        return this._Duration;
      }
      set
      {
        base.SetProperty<TimeSpan>(Scene.PropertyDuration, ref this._Duration, value);
      }
    }

    [XmlElement("Duration")]
    public long DurationTicks
    {
      get
      {
        return this.Duration.Ticks;
      }
      set
      {
        this.Duration = new TimeSpan(value);
      }
    }

    public static string PropertyTransitionDuration
    {
      get
      {
        return "TransitionDuration";
      }
    }

    [XmlIgnore]
    public TimeSpan TransitionDuration
    {
      get
      {
        return this._TransitionDuration;
      }
      set
      {
        base.SetProperty<TimeSpan>(Scene.PropertyTransitionDuration, ref this._TransitionDuration, value);
      }
    }

    [XmlElement("TransitionDuration")]
    public long TransitionDurationTicks
    {
      get
      {
        return this.TransitionDuration.Ticks;
      }
      set
      {
        this.TransitionDuration = new TimeSpan(value);
      }
    }

    public static string PropertyEffectSpeed
    {
      get
      {
        return "EffectSpeed";
      }
    }

    [XmlElement("Speed")]
    public double EffectSpeed
    {
      get
      {
        return this._EffectSpeed;
      }
      set
      {
        base.SetProperty<double>(Scene.PropertyEffectSpeed, ref this._EffectSpeed, value);
      }
    }

    public static string PropertyFrame
    {
      get
      {
        return "Frame";
      }
    }

    [XmlElement("Frame")]
    public Frame Frame
    {
      get
      {
        return this._Frame;
      }
      set
      {
        base.SetProperty<Frame>(Scene.PropertyFrame, ref this._Frame, value);
      }
    }

    public static string PropertyLayersContent
    {
      get
      {
        return "LayersContent";
      }
    }

    [XmlElement("LayersContent")]
    public string LayersContent
    {
      get
      {
        return this._LayersContent;
      }
      set
      {
        base.SetProperty<string>(Scene.PropertyLayersContent, ref this._LayersContent, value);
      }
    }

    public static string PropertyCustomMapId
    {
      get
      {
        return "CustomMapId";
      }
    }

    [XmlAttribute("CustomMapGuid")]
    public Guid CustomMapId
    {
      get
      {
        return this._CustomMapId;
      }
      set
      {
        base.SetProperty<Guid>(Scene.PropertyCustomMapId, ref this._CustomMapId, value);
      }
    }

    [XmlAttribute("CustomMapId")]
    public string StringCustomMapId
    {
      get
      {
        return this.CustomMapId.ToString();
      }
      set
      {
        Guid result;
        if (!Guid.TryParse(value, out result))
          result = CustomMap.InvalidMapId;
        this.CustomMapId = result;
      }
    }

    [XmlIgnore]
    public bool HasCustomMap
    {
      get
      {
        return this._CustomMapId != CustomMap.InvalidMapId;
      }
    }

    public Scene Clone()
    {
      return new Scene()
      {
        _Name = this.Name,
        _EffectSpeed = this.EffectSpeed,
        _Duration = this.Duration,
        _EffectType = this.EffectType,
        _LayersContent = this.LayersContent,
        _ThemeId = this.ThemeId,
        _ThemeWithLabel = this._ThemeWithLabel,
        _FlatModeEnabled = this._FlatModeEnabled,
        _TransitionType = this.TransitionType,
        _TransitionDuration = this.TransitionDuration,
        _Frame = this.Frame.Clone(),
        _CustomMapId = this._CustomMapId
      };
    }
  }
}
