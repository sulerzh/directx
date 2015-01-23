using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class LayerLegendDecoratorModel : DecoratorContentBase
  {
    private string _LayerName;
    private VisualizationChartType _ChartType;
    private RegionLayerShadingMode _RegionShadingMode;
    private Guid _LayerId;
    private double _Minimum;
    private double _Maximum;

    [XmlIgnore]
    public int MaxLegendItemsCount
    {
      get
      {
        return 50;
      }
    }

    public string PropertyLegendItems
    {
      get
      {
        return "LegendItems";
      }
    }

    [XmlIgnore]
    public ObservableCollectionEx<LayerLegendItemModel> LegendItems { get; private set; }

    public string PropertyLayerName
    {
      get
      {
        return "LayerName";
      }
    }

    [XmlIgnore]
    public string LayerName
    {
      get
      {
        return this._LayerName;
      }
      set
      {
        if (!base.SetProperty<string>(this.PropertyLayerName, ref this._LayerName, value))
          return;
        foreach (LayerLegendItemModel layerLegendItemModel in (Collection<LayerLegendItemModel>) this.LegendItems)
          layerLegendItemModel.LayerNameUpdated(value);
      }
    }

    public string PropertyChartType
    {
      get
      {
        return "ChartType";
      }
    }

    [XmlIgnore]
    public VisualizationChartType ChartType
    {
      get
      {
        return this._ChartType;
      }
      set
      {
        base.SetProperty<VisualizationChartType>(this.PropertyChartType, ref this._ChartType, value);
      }
    }

    public string PropertyRegionShadingMode
    {
      get
      {
        return "RegionShadingMode";
      }
    }

    [XmlIgnore]
    public RegionLayerShadingMode RegionShadingMode
    {
      get
      {
        return this._RegionShadingMode;
      }
      set
      {
        base.SetProperty<RegionLayerShadingMode>(this.PropertyRegionShadingMode, ref this._RegionShadingMode, value);
      }
    }

    public string PropertyLayerId
    {
      get
      {
        return "LayerId";
      }
    }

    public Guid LayerId
    {
      get
      {
        return this._LayerId;
      }
      set
      {
        base.SetProperty<Guid>(this.PropertyLayerId, ref this._LayerId, value);
      }
    }

    public string PropertyMinimum
    {
      get
      {
        return "Minimum";
      }
    }

    public double Minimum
    {
      get
      {
        return this._Minimum;
      }
      set
      {
        base.SetProperty<double>(this.PropertyMinimum, ref this._Minimum, value);
      }
    }

    public string PropertyMaximum
    {
      get
      {
        return "Maximum";
      }
    }

    public double Maximum
    {
      get
      {
        return this._Maximum;
      }
      set
      {
        base.SetProperty<double>(this.PropertyMaximum, ref this._Maximum, value);
      }
    }

    public LayerLegendDecoratorModel()
    {
      this.LegendItems = new ObservableCollectionEx<LayerLegendItemModel>();
      this.LegendItems.CollectionChanged += (NotifyCollectionChangedEventHandler) ((o, s) => this.RaisePropertyChanged(this.PropertyLegendItems));
    }

    public bool TryAddLegendItem(LayerLegendItemModel legendItem)
    {
      if (legendItem == null || this.LegendItems.Count >= this.MaxLegendItemsCount)
        return false;
      this.LegendItems.Add(legendItem);
      return true;
    }
  }
}
