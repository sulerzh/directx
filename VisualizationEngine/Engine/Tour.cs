// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Tour
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Engine
{
  [XmlRoot("Tour", IsNullable = false, Namespace = "http://microsoft.data.visualization.engine.tours/1.0")]
  [Serializable]
  public class Tour : CompositePropertyChangeNotificationBase
  {
    public const int InvalidTourHandle = 0;
    private static int lastTourHandle;
    private string _Name;
    private string _Description;
    private List<Scene> scenes;

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
        base.SetProperty<string>(Tour.PropertyName, ref this._Name, value);
      }
    }

    public static string PropertyDescription
    {
      get
      {
        return "Description";
      }
    }

    [XmlAttribute("Description")]
    public string Description
    {
      get
      {
        return this._Description;
      }
      set
      {
        base.SetProperty<string>(Tour.PropertyDescription, ref this._Description, value);
      }
    }

    [XmlArrayItem("Scene", typeof (Scene))]
    [XmlArray("Scenes")]
    public List<Scene> Scenes
    {
      get
      {
        return this.scenes;
      }
    }

    [XmlIgnore]
    public int Handle { get; set; }

    [XmlIgnore]
    public double Duration
    {
      get
      {
        return Enumerable.Sum(Enumerable.Select<Scene, double>((IEnumerable<Scene>) this.scenes, (Func<Scene, int, double>) ((scene, sceneNum) => scene.Duration.TotalSeconds + (sceneNum <= 0 || !(scene.CustomMapId == this.scenes[sceneNum - 1].CustomMapId) ? 0.0 : scene.TransitionDuration.TotalSeconds))));
      }
    }

    public Tour()
    {
      this.scenes = new List<Scene>();
      this.Handle = Interlocked.Increment(ref Tour.lastTourHandle);
    }

    public static Tour ReadXml(XmlReader reader)
    {
      Tour tour = (Tour) new XmlSerializer(typeof (Tour)).Deserialize(reader);
      tour.Handle = Interlocked.Increment(ref Tour.lastTourHandle);
      return tour;
    }

    public void WriteXml(XmlWriter writer)
    {
      new XmlSerializer(typeof (Tour)).Serialize(writer, (object) this);
    }
  }
}
