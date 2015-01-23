using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class CustomMapsList : ICustomMapCollection
  {
    private ConcurrentValue<List<CustomMap>> mCustomMaps = new ConcurrentValue<List<CustomMap>>(new List<CustomMap>());
    private volatile bool isDirty;

    protected ConcurrentValue<List<CustomMap>> CustomMapsSecured
    {
      get
      {
        return this.mCustomMaps;
      }
    }

    public IEnumerable<CustomMap> AvailableMaps
    {
      get
      {
        return (IEnumerable<CustomMap>) this.CustomMapsSecured.SelectSafely<List<CustomMap>>((Func<List<CustomMap>, List<CustomMap>>) (list => Enumerable.ToList<CustomMap>((IEnumerable<CustomMap>) list)));
      }
    }

    public CustomMapsList()
    {
    }

    public CustomMapsList(CustomMapsList.SerializableCustomMapsList sl)
      : this()
    {
      this.CustomMapsSecured.UseSafely((Action<List<CustomMap>>) (list => sl.CustomMaps.ForEach((Action<CustomMap.SerializableCustomMap>) (item => list.Add(item.Unwrap())))));
    }

    public CustomMapsList.SerializableCustomMapsList Wrap()
    {
      CustomMapsList.SerializableCustomMapsList ans = new CustomMapsList.SerializableCustomMapsList();
      this.CustomMapsSecured.UseSafely((Action<List<CustomMap>>) (list =>
      {
        List<CustomMap> list1 = Enumerable.ToList<CustomMap>(Enumerable.Where<CustomMap>((IEnumerable<CustomMap>) list, (Func<CustomMap, bool>) (k => !k.IsTemporary)));
        ans.CustomMaps = new List<CustomMap.SerializableCustomMap>(list1.Count);
        list1.ForEach((Action<CustomMap>) (item => ans.CustomMaps.Add(item.Wrap())));
      }));
      return ans;
    }

    private CustomMap CreateCustomMapInList(List<CustomMap> mapList, Guid uniqueId)
    {
      CustomMap customMap = new CustomMap(uniqueId, true);
      customMap.OwningMapsList = (ICustomMapCollection) this;
      customMap.ImageDisplayName = Resources.CustomSpace_DefaultImageTitle;
      customMap.Name = Resources.CustomSpace_DefaultMapTitle_Generic;
      for (int index = mapList.Count + 1; index < 1000; ++index)
      {
        string name = string.Format(Resources.CustomSpace_DefaultMapTitle, (object) index);
        if (!Enumerable.Any<CustomMap>((IEnumerable<CustomMap>) mapList, (Func<CustomMap, bool>) (k => k.Name.Equals(name))))
        {
          customMap.Name = name;
          break;
        }
      }
      mapList.Add(customMap);
      this.MarkMapListAsDirty();
      return customMap;
    }

    public CustomMap CreateCustomMap()
    {
      return this.CustomMapsSecured.SelectSafely<CustomMap>((Func<List<CustomMap>, CustomMap>) (mapList => this.CreateCustomMapInList(mapList, Guid.NewGuid())));
    }

    public CustomMap FindOrCreateMapFromId(Guid uniqueId)
    {
      if (uniqueId == CustomMap.InvalidMapId)
        return (CustomMap) null;
      else
        return this.CustomMapsSecured.SelectSafely<CustomMap>((Func<List<CustomMap>, CustomMap>) (mapList =>
        {
          CustomMap customMapInList = mapList.Find((Predicate<CustomMap>) (k => k.UniqueCustomMapId.Equals(uniqueId)));
          if (customMapInList != null)
          {
            customMapInList.OwningMapsList = (ICustomMapCollection) this;
          }
          else
          {
            customMapInList = this.CreateCustomMapInList(mapList, uniqueId);
            customMapInList.IsTemporary = true;
          }
          return customMapInList;
        }));
    }

    public void PermanentlyDeleteCustomMap(CustomMap cm)
    {
      this.CustomMapsSecured.UseSafely((Action<List<CustomMap>>) (mapList =>
      {
        if (!mapList.Contains(cm))
          return;
        mapList.Remove(cm);
        this.MarkMapListAsDirty();
      }));
    }

    public void DeleteAllCustomMaps()
    {
      this.CustomMapsSecured.UseSafely((Action<List<CustomMap>>) (mapList =>
      {
        mapList.Clear();
        this.MarkMapListAsDirty();
      }));
    }

    public void MarkMapListAsDirty()
    {
      this.SetShouldSave(true);
    }

    public bool ShouldSave()
    {
      return this.isDirty;
    }

    public void SetShouldSave(bool isDirty)
    {
      this.isDirty = isDirty;
    }

    [XmlType("CustomMapsList")]
    [Serializable]
    public class SerializableCustomMapsList
    {
      [XmlArrayItem("CustomMap", typeof (CustomMap.SerializableCustomMap))]
      [XmlArray("CustomMaps")]
      public List<CustomMap.SerializableCustomMap> CustomMaps { get; set; }

      public CustomMapsList Unwrap()
      {
        return new CustomMapsList(this);
      }
    }
  }
}
