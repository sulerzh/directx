using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class ChartDecoratorModel : DecoratorContentBase, ICommand
  {
    private static readonly ChartDecoratorModel.ChartTypeInfo[] chartInfos = new ChartDecoratorModel.ChartTypeInfo[4]
    {
      new ChartDecoratorModel.ChartTypeInfo(ChartDecoratorModel.XYChartTypeFlags.ColumnsClustered, "/VisualizationControl;component/Images/Columns.png"),
      new ChartDecoratorModel.ChartTypeInfo(ChartDecoratorModel.XYChartTypeFlags.ColumnsNotClustered, "/VisualizationControl;component/Images/StackedColumns.png"),
      new ChartDecoratorModel.ChartTypeInfo(ChartDecoratorModel.XYChartTypeFlags.BarsClustered, "/VisualizationControl;component/Images/Bars.png"),
      new ChartDecoratorModel.ChartTypeInfo(ChartDecoratorModel.XYChartTypeFlags.BarsNotClustered, "/VisualizationControl;component/Images/StackedBars.png")
    };
    private ChartDecoratorModel.XYChartTypeFlags xyChartType = ChartDecoratorModel.XYChartTypeFlags.ColumnsClustered;
    private ChartDecoratorModel.ChartTypeInfo[] chartTypeInfoList = ChartDecoratorModel.chartInfos;
    private bool isClustered = true;
    private string title;
    private ObservableCollectionEx<ChartDataPoint> dataPoints;
    private string yAxisName;
    private string xAxisName;
    private SortField selectedSortField;
    private SortField[] sortFields;
    private ChartVisualization.ChartVisualizationType type;
    private bool isVisible;
    private bool isBar;

    public string PropertyTitle
    {
      get
      {
        return "Title";
      }
    }

    [XmlIgnore]
    public string Title
    {
      get
      {
        return this.title;
      }
      set
      {
        base.SetProperty<string>(this.PropertyTitle, ref this.title, value);
      }
    }

    public string PropertyDataPoints
    {
      get
      {
        return "DataPoints";
      }
    }

    [XmlIgnore]
    public ObservableCollectionEx<ChartDataPoint> DataPoints
    {
      get
      {
        return this.dataPoints;
      }
      set
      {
        base.SetProperty<ObservableCollectionEx<ChartDataPoint>>(this.PropertyDataPoints, ref this.dataPoints, value);
      }
    }

    public string PropertyYAxisName
    {
      get
      {
        return "YAxisName";
      }
    }

    [XmlIgnore]
    public string YAxisName
    {
      get
      {
        return this.yAxisName;
      }
      set
      {
        base.SetProperty<string>(this.PropertyYAxisName, ref this.yAxisName, value);
      }
    }

    public string PropertyXAxisName
    {
      get
      {
        return "XAxisName";
      }
    }

    [XmlIgnore]
    public string XAxisName
    {
      get
      {
        return this.xAxisName;
      }
      set
      {
        base.SetProperty<string>(this.PropertyXAxisName, ref this.xAxisName, value);
      }
    }

    public string PropertySelectedValue
    {
      get
      {
        return "SelectedSortField";
      }
    }

    [XmlIgnore]
    public SortField SelectedSortField
    {
      get
      {
        return this.selectedSortField;
      }
      set
      {
        base.SetProperty<SortField>(this.PropertySelectedValue, ref this.selectedSortField, value);
      }
    }

    public string PropertyValues
    {
      get
      {
        return "SortFields";
      }
    }

    [XmlIgnore]
    public SortField[] SortFields
    {
      get
      {
        return this.sortFields;
      }
      set
      {
        base.SetProperty<SortField[]>(this.PropertyValues, ref this.sortFields, value);
      }
    }

    [XmlIgnore]
    public List<ChartVisualization.ChartVisualizationType> Types { get; private set; }

    public string PropertyType
    {
      get
      {
        return "Type";
      }
    }

    public ChartVisualization.ChartVisualizationType Type
    {
      get
      {
        return this.type;
      }
      set
      {
        base.SetProperty<ChartVisualization.ChartVisualizationType>(this.PropertyType, ref this.type, value);
      }
    }

    public string PropertyIsVisible
    {
      get
      {
        return "IsVisible";
      }
    }

    public bool IsVisible
    {
      get
      {
        return this.isVisible;
      }
      set
      {
        base.SetProperty<bool>(this.PropertyIsVisible, ref this.isVisible, value);
      }
    }

    public string PropertyXYChartType
    {
      get
      {
        return "XYChartType";
      }
    }

    public ChartDecoratorModel.XYChartTypeFlags XYChartType
    {
      get
      {
        return this.xyChartType;
      }
      set
      {
        if (!base.SetProperty<ChartDecoratorModel.XYChartTypeFlags>(this.PropertyXYChartType, ref this.xyChartType, value))
          return;
        this.IsClustered = (value & ChartDecoratorModel.XYChartTypeFlags.Clustered) == ChartDecoratorModel.XYChartTypeFlags.Clustered;
        this.IsBar = (value & ChartDecoratorModel.XYChartTypeFlags.Bars) == ChartDecoratorModel.XYChartTypeFlags.Bars;
      }
    }

    public string PropertyChartTypeInfoList
    {
      get
      {
        return "ChartTypeInfoList";
      }
    }

    public ChartDecoratorModel.ChartTypeInfo[] ChartTypeInfoList
    {
      get
      {
        return ChartDecoratorModel.chartInfos;
      }
    }

    public string PropertyIsClustered
    {
      get
      {
        return "IsClustered";
      }
    }

    public bool IsClustered
    {
      get
      {
        return this.isClustered;
      }
      set
      {
        if (!base.SetProperty<bool>(this.PropertyIsClustered, ref this.isClustered, value))
          return;
        this.XYChartType = this.XYChartType & ~ChartDecoratorModel.XYChartTypeFlags.ClusteringMode | (value ? ChartDecoratorModel.XYChartTypeFlags.Clustered : ChartDecoratorModel.XYChartTypeFlags.NotClustered);
      }
    }

    public string PropertyIsBar
    {
      get
      {
        return "IsBar";
      }
    }

    public bool IsBar
    {
      get
      {
        return this.isBar;
      }
      set
      {
        if (!base.SetProperty<bool>(this.PropertyIsBar, ref this.isBar, value))
          return;
        this.XYChartType = this.XYChartType & ~ChartDecoratorModel.XYChartTypeFlags.Orientation | (value ? ChartDecoratorModel.XYChartTypeFlags.Bars : ChartDecoratorModel.XYChartTypeFlags.Columns);
      }
    }

    public Guid LayerId { get; set; }

    public Guid Id { get; set; }

    [XmlIgnore]
    public ChartVisualization ChartVisualization { get; set; }

    public event EventHandler CanExecuteChanged;

    public ChartDecoratorModel()
    {
      this.isVisible = true;
      this.Id = Guid.NewGuid();
      this.type = ChartVisualization.ChartVisualizationType.Top;
      this.dataPoints = new ObservableCollectionEx<ChartDataPoint>();
      this.Types = new List<ChartVisualization.ChartVisualizationType>()
      {
        ChartVisualization.ChartVisualizationType.Top,
        ChartVisualization.ChartVisualizationType.Bottom
      };
    }

    public void SetValues(IEnumerable<ChartDataView.ChartResult> values, string xAxis, ModelQueryMeasureColumn measureUsed)
    {
      ObservableCollectionEx<ChartDataPoint> observableCollectionEx = new ObservableCollectionEx<ChartDataPoint>();
      foreach (ChartDataView.ChartResult chartResult in values)
      {
        foreach (ChartDataView.ChartResult.DataPoint dataPoint in Enumerable.Reverse<ChartDataView.ChartResult.DataPoint>((IEnumerable<ChartDataView.ChartResult.DataPoint>) chartResult.Values))
          observableCollectionEx.Add(new ChartDataPoint(chartResult.Geo, dataPoint.Value, dataPoint.Id, dataPoint.Color, dataPoint.ShiftIndex, new DataPointToolTip(this.ChartVisualization.LayerDefinition.GeoVisualization, dataPoint.Id)));
      }
      this.DataPoints = observableCollectionEx;
      this.XAxisName = xAxis;
      this.YAxisName = measureUsed != null ? (measureUsed.AggregationFunction != AggregationFunction.UserDefined ? string.Format(Resources.FieldWellHeightAggregatedNameFormat, (object) measureUsed.TableColumn.Name, (object) AggregationFunctionExtensions.DisplayString(measureUsed.AggregationFunction)) : measureUsed.TableColumn.Name) : (string) null;
      this.Title = this.GetTitle();
    }

    private string GetTitle()
    {
      bool flag = this.ChartVisualization != null && this.ChartVisualization.LayerDefinition != null && this.ChartVisualization.LayerDefinition.GeoVisualization != null && !this.ChartVisualization.LayerDefinition.GeoVisualization.HiddenMeasure;
      if (this.sortFields == null || this.sortFields.Length <= 0)
        return (string) null;
      if (this.sortFields[0].CategoryValue != null)
      {
        if (flag)
          return string.Format(Resources.ChartTitle_WithExplicitHeight, (object) this.YAxisName, (object) this.sortFields[0].Column.Name, (object) this.XAxisName);
        else
          return string.Format(Resources.ChartTitle_WithImplicitHeight, (object) this.sortFields[0].Column.Name, (object) this.XAxisName);
      }
      else
      {
        if (this.sortFields.Length == 1)
          return string.Format(Resources.ChartTitle_FormatValueCountOne, (object) this.sortFields[0].Name, (object) this.XAxisName);
        if (this.sortFields.Length == 2)
          return string.Format(Resources.ChartTitle_FormatValueCountTwo, (object) this.sortFields[0].Name, (object) this.sortFields[1].Name, (object) this.XAxisName);
        if (this.sortFields.Length == 3)
          return string.Format(Resources.ChartTitle_FormatValueCountThree, (object) this.sortFields[0].Name, (object) this.sortFields[1].Name, (object) this.sortFields[2].Name, (object) this.XAxisName);
        else if (this.sortFields.Length == 4)
          return string.Format(Resources.ChartTitle_FormatValueCountFour, (object) this.sortFields[0].Name, (object) this.sortFields[1].Name, (object) this.sortFields[2].Name, (object) this.sortFields[3].Name, (object) this.XAxisName);
        else if (this.sortFields.Length == 5)
          return string.Format(Resources.ChartTitle_FormatValueCountFive, (object) this.sortFields[0].Name, (object) this.sortFields[1].Name, (object) this.sortFields[2].Name, (object) this.sortFields[3].Name, (object) this.sortFields[4].Name, (object) this.XAxisName);
        else
          return string.Format(Resources.ChartTitle_FormatValueCountMoreThanFive, (object) this.sortFields[0].Name, (object) this.sortFields[1].Name, (object) this.sortFields[2].Name, (object) this.sortFields[3].Name, (object) this.sortFields[4].Name, (object) this.XAxisName);
      }
    }

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      if (!((string) (parameter ?? (object) parameter.ToString()) == "ToggleTopBottom"))
        return;
      this.Type = this.Type == ChartVisualization.ChartVisualizationType.Bottom ? ChartVisualization.ChartVisualizationType.Top : ChartVisualization.ChartVisualizationType.Bottom;
    }

    public class ChartTypeInfo
    {
      public Uri ImageUri { get; set; }

      public ChartDecoratorModel.XYChartTypeFlags XYChartType { get; set; }

      public ChartTypeInfo()
      {
      }

      public ChartTypeInfo(ChartDecoratorModel.XYChartTypeFlags type, string imageUri)
      {
        this.ImageUri = new Uri(imageUri, UriKind.Relative);
        this.XYChartType = type;
      }

      public override bool Equals(object obj)
      {
        return obj is ChartDecoratorModel.XYChartTypeFlags && (ChartDecoratorModel.XYChartTypeFlags) obj == this.XYChartType;
      }

      public override int GetHashCode()
      {
        return this.XYChartType.GetHashCode();
      }
    }

    [Flags]
    public enum XYChartTypeFlags
    {
      Bars = 1,
      Columns = 2,
      Orientation = Columns | Bars,
      NotClustered = 16,
      Clustered = 32,
      ClusteringMode = Clustered | NotClustered,
      BarsNotClustered = NotClustered | Bars,
      ColumnsNotClustered = NotClustered | Columns,
      BarsClustered = Clustered | Bars,
      ColumnsClustered = Clustered | Columns,
    }
  }
}
