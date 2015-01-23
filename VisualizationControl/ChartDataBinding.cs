using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  internal class ChartDataBinding : DataBinding
  {
    private readonly object syncLock = new object();
    private const int MaxCount = 100;
    private volatile bool isInitialized;
    private ChartVisualization chartVisualization;
    private readonly WeakEventListener<ChartDataBinding> colorSelectionChanged;
    private readonly WeakEventListener<ChartDataBinding, object, PropertyChangedEventArgs> modelPropertyChanged;

    public ChartDecoratorModel ChartDecoratorModel
    {
      get
      {
        return this.VisualElement as ChartDecoratorModel;
      }
      set
      {
        this.VisualElement = (object) value;
        this.InitializeModel();
      }
    }

    public ChartDataBinding(ChartVisualization visualization, ChartDecoratorModel chartDecoratorModel, DataView dataView)
      : base((object) chartDecoratorModel, dataView)
    {
      this.chartVisualization = visualization;
      if (this.chartVisualization.LayerDefinition != null)
      {
        this.colorSelectionChanged = new WeakEventListener<ChartDataBinding>(this, new Action<ChartDataBinding>(ChartDataBinding.ColorSelectorOnColorsChanged));
        this.chartVisualization.VisualizationModel.ColorSelector.ColorsChanged += new Action(this.colorSelectionChanged.OnEvent);
      }
      this.modelPropertyChanged = new WeakEventListener<ChartDataBinding, object, PropertyChangedEventArgs>(this)
      {
        OnEventAction = new Action<ChartDataBinding, object, PropertyChangedEventArgs>(ChartDataBinding.ChartDecoratorModel_PropertyChanged)
      };
      this.InitializeModel();
    }

    internal override void Shutdown()
    {
      this.chartVisualization = (ChartVisualization) null;
      base.Shutdown();
    }

    protected override void RefreshData(CancellationToken cancellationToken, int sourceDataVersion, bool updateDisplay, DataChangedEventArgs dataChangedEventArgs)
    {
      ChartVisualization chartVisualization = this.chartVisualization;
      if (chartVisualization == null || chartVisualization.VisualizationModel == null || chartVisualization.VisualizationModel.UIDispatcher == null)
        return;
      DispatcherExtensions.CheckedInvoke(chartVisualization.VisualizationModel.UIDispatcher, new Action(this.InitializeModel), false);
    }

    private IEnumerable<ChartDataView.ChartResult> GetChartValues(out ModelQueryMeasureColumn measureUsed)
    {
      measureUsed = (ModelQueryMeasureColumn) null;
      ChartDataView chartDataView = this.DataView as ChartDataView;
      if (chartDataView != null)
      {
        if (!chartDataView.IsQueryResultAvailable)
          return Enumerable.Empty<ChartDataView.ChartResult>();
        Tuple<TableMember, string> categoryOrDefault = chartDataView.GetCategoryOrDefault((TableMember) this.chartVisualization.ChartFieldWellDefinition.Category, this.chartVisualization.ChartFieldWellDefinition.CategoryValue);
        Tuple<TableMember, AggregationFunction> measureOrDefault = chartDataView.GetMeasureOrDefault((TableMember) this.chartVisualization.ChartFieldWellDefinition.Measure, this.chartVisualization.ChartFieldWellDefinition.Function);
        this.chartVisualization.ChartFieldWellDefinition.Measure = (TableField) measureOrDefault.Item1;
        this.chartVisualization.ChartFieldWellDefinition.Function = measureOrDefault.Item2;
        this.chartVisualization.ChartFieldWellDefinition.Category = (TableField) categoryOrDefault.Item1;
        this.chartVisualization.ChartFieldWellDefinition.CategoryValue = categoryOrDefault.Item2;
        List<ChartDataView.ChartResult> dataByCategory = chartDataView.GetDataByCategory(categoryOrDefault.Item2, measureOrDefault.Item1, measureOrDefault.Item2, 100, this.chartVisualization.Type == ChartVisualization.ChartVisualizationType.Top, out measureUsed);
        dataByCategory.ForEach((Action<ChartDataView.ChartResult>) (item => item.Values.ForEach((Action<ChartDataView.ChartResult.DataPoint>) (point =>
        {
          if (!string.IsNullOrEmpty(point.CategoryName))
            point.Color = this.chartVisualization.LayerDefinition.GeoVisualization.ColorForCategory(point.CategoryName).ToWindowsColor();
          else
            point.Color = this.chartVisualization.LayerDefinition.GeoVisualization.ColorForMeasure((TableField) point.Measure.Item1, point.Measure.Item2).ToWindowsColor();
        }))));
        return (IEnumerable<ChartDataView.ChartResult>) dataByCategory;
      }
      else
      {
        this.chartVisualization.ChartFieldWellDefinition.Measure = (TableField) null;
        this.chartVisualization.ChartFieldWellDefinition.Category = (TableField) null;
        this.chartVisualization.ChartFieldWellDefinition.CategoryValue = (string) null;
        return Enumerable.Empty<ChartDataView.ChartResult>();
      }
    }

    protected override void ClearDisplay()
    {
      DispatcherExtensions.CheckedInvoke(this.chartVisualization.VisualizationModel.UIDispatcher, (Action) (() =>
      {
        lock (this.syncLock)
        {
          if (this.ChartDecoratorModel == null)
            return;
          this.ChartDecoratorModel.PropertyChanged -= new PropertyChangedEventHandler(this.modelPropertyChanged.OnEvent);
          this.ChartDecoratorModel.SetValues((IEnumerable<ChartDataView.ChartResult>) new List<ChartDataView.ChartResult>(), (string) null, (ModelQueryMeasureColumn) null);
          this.ChartDecoratorModel.SortFields = ((ChartDataView) this.DataView).SortFields;
          this.ChartDecoratorModel.SelectedSortField = this.ChartDecoratorModel.SortFields.Length > 0 ? this.ChartDecoratorModel.SortFields[0] : (SortField) null;
          this.ChartDecoratorModel.PropertyChanged += new PropertyChangedEventHandler(this.modelPropertyChanged.OnEvent);
        }
      }), false);
    }

    internal override void ClearVisualElement()
    {
      if (this.ChartDecoratorModel != null)
        this.ChartDecoratorModel.PropertyChanged -= new PropertyChangedEventHandler(this.modelPropertyChanged.OnEvent);
      if (this.chartVisualization.LayerDefinition != null)
        this.chartVisualization.VisualizationModel.ColorSelector.ColorsChanged -= new Action(this.colorSelectionChanged.OnEvent);
      base.ClearVisualElement();
      this.ChartDecoratorModel = (ChartDecoratorModel) null;
    }

    private static void ChartDecoratorModel_PropertyChanged(ChartDataBinding dataBinding, object model, PropertyChangedEventArgs e)
    {
      if (!e.PropertyName.Equals(dataBinding.ChartDecoratorModel.PropertySelectedValue) && !e.PropertyName.Equals(dataBinding.ChartDecoratorModel.PropertyType))
        return;
      if (dataBinding.ChartDecoratorModel.SelectedSortField != null)
      {
        if (dataBinding.chartVisualization.ChartFieldWellDefinition.Category != null)
        {
          dataBinding.chartVisualization.ChartFieldWellDefinition.CategoryValue = dataBinding.ChartDecoratorModel.SelectedSortField != null ? dataBinding.ChartDecoratorModel.SelectedSortField.CategoryValue : (string) null;
        }
        else
        {
          dataBinding.chartVisualization.ChartFieldWellDefinition.Measure = (TableField) dataBinding.ChartDecoratorModel.SelectedSortField.Column;
          dataBinding.chartVisualization.ChartFieldWellDefinition.Function = dataBinding.ChartDecoratorModel.SelectedSortField.Function;
        }
      }
      dataBinding.chartVisualization.Type = dataBinding.ChartDecoratorModel.Type;
      dataBinding.RefreshData(new CancellationToken(), dataBinding.LastSourceDataVersion, true, (DataChangedEventArgs) null);
    }

    private static void ColorSelectorOnColorsChanged(ChartDataBinding dataBinding)
    {
      if (!dataBinding.isInitialized)
        return;
      dataBinding.RefreshData(new CancellationToken(), dataBinding.LastSourceDataVersion, true, (DataChangedEventArgs) null);
    }

    private void InitializeModel()
    {
      lock (this.syncLock)
      {
        if (this.ChartDecoratorModel == null)
          return;
        this.ChartDecoratorModel.PropertyChanged -= new PropertyChangedEventHandler(this.modelPropertyChanged.OnEvent);
        ModelQueryMeasureColumn local_0;
        IEnumerable<ChartDataView.ChartResult> local_1 = this.GetChartValues(out local_0);
        ChartDataView local_2 = (ChartDataView) this.DataView;
        this.ChartDecoratorModel.SortFields = local_2.SortFields;
        this.ChartDecoratorModel.SelectedSortField = Enumerable.FirstOrDefault<SortField>((IEnumerable<SortField>) this.ChartDecoratorModel.SortFields, (Func<SortField, bool>) (sf =>
        {
          if (sf.CategoryValue != null)
            return sf.CategoryValue.Equals(this.chartVisualization.ChartFieldWellDefinition.CategoryValue);
          if (sf.Column != null && sf.Column.RefersToTheSameMemberAs((TableMember) this.chartVisualization.ChartFieldWellDefinition.Measure))
            return sf.Function == this.chartVisualization.ChartFieldWellDefinition.Function;
          else
            return false;
        }));
        this.ChartDecoratorModel.SetValues(local_1, local_2.GeoFieldName, local_0);
        this.ChartDecoratorModel.PropertyChanged += new PropertyChangedEventHandler(this.modelPropertyChanged.OnEvent);
        this.isInitialized = true;
      }
    }
  }
}
