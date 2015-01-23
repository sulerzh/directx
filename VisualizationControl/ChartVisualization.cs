using System;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ChartVisualization : Visualization
  {
    private ChartDecoratorModel model;

    public ChartDecoratorModel Model
    {
      get
      {
        return this.model;
      }
      set
      {
        this.SetModel(value);
        ((ChartDataBinding) this.DataBinding).ChartDecoratorModel = this.model;
      }
    }

    public ChartFieldWellDefinition ChartFieldWellDefinition
    {
      get
      {
        return this.FieldWellDefinition as ChartFieldWellDefinition;
      }
    }

    public ChartVisualization.ChartVisualizationType Type { get; set; }

    public Guid Id { get; set; }

    public ChartVisualization(ChartDecoratorModel model, LayerDefinition layerDefinition, DataSource dataSource)
      : base(layerDefinition, dataSource)
    {
      this.SetModel(model);
      this.Id = model.Id;
      this.Type = model.Type;
      this.FieldWellDefinition = (FieldWellDefinition) new ChartFieldWellDefinition(this);
      this.DataBinding = this.CreateDataBinding();
      this.initialized = true;
    }

    private ChartVisualization(ChartVisualization.SerializableChartVisualization serializableChartVisualization, LayerDefinition layerDefinition, GeoDataSource geoDataSource, CultureInfo modelCulture)
      : base((Visualization.SerializableVisualization) serializableChartVisualization, layerDefinition, (DataSource) geoDataSource)
    {
      this.Unwrap(serializableChartVisualization, modelCulture);
      this.DataBinding = this.CreateDataBinding();
      this.initialized = true;
    }

    public override bool Remove()
    {
      this.Removed();
      return true;
    }

    internal override void Removed()
    {
      this.Hide();
      base.Removed();
    }

    internal override void Shutdown()
    {
      base.Shutdown();
    }

    protected override sealed DataBinding CreateDataBinding()
    {
      ChartDataBinding chartDataBinding = new ChartDataBinding(this, this.Model, this.DataSource.CreateDataView(DataView.DataViewType.Chart));
      this.initialized = true;
      return (DataBinding) chartDataBinding;
    }

    internal override void OnVisibleChanged(bool visible)
    {
      if (visible)
        this.Show(CancellationToken.None);
      else
        this.Hide();
    }

    protected override void Show(CancellationToken cancellationToken)
    {
      if (this.model == null)
        return;
      this.model.IsVisible = true;
    }

    protected override void Hide()
    {
      if (this.model == null)
        return;
      this.model.IsVisible = false;
    }

    internal ChartVisualization.SerializableChartVisualization Wrap()
    {
      ChartVisualization.SerializableChartVisualization chartVisualization1 = new ChartVisualization.SerializableChartVisualization();
      chartVisualization1.ChartFieldWellDefinition = this.ChartFieldWellDefinition.Wrap() as ChartFieldWellDefinition.SerializableChartFieldWellDefinition;
      chartVisualization1.Type = this.Type;
      chartVisualization1.Visible = this.Visible;
      chartVisualization1.Id = this.Id;
      ChartVisualization.SerializableChartVisualization chartVisualization2 = chartVisualization1;
      this.SnapState((Visualization.SerializableVisualization) chartVisualization2);
      return chartVisualization2;
    }

    private void SetModel(ChartDecoratorModel value)
    {
      this.model = value;
      this.model.IsVisible = this.LayerDefinition.Visible && (this.LayerDefinition.GeoVisualization == null || this.LayerDefinition.GeoVisualization.Visible);
      this.model.ChartVisualization = this;
    }

    private void Unwrap(ChartVisualization.SerializableChartVisualization serializableChartVisualization, CultureInfo modelCulture)
    {
      this.Type = serializableChartVisualization.Type;
      this.Id = serializableChartVisualization.Id;
      this.FieldWellDefinition = serializableChartVisualization.ChartFieldWellDefinition.Unwrap((Visualization) this, modelCulture);
    }

    [Serializable]
    public enum ChartVisualizationType
    {
      Top,
      Bottom,
    }

    [Serializable]
    public class SerializableChartVisualization : Visualization.SerializableVisualization
    {
      [XmlElement("Type", typeof (ChartVisualization.ChartVisualizationType))]
      public ChartVisualization.ChartVisualizationType Type { get; set; }

      [XmlElement("ChartFieldWellDefinition", typeof (ChartFieldWellDefinition.SerializableChartFieldWellDefinition))]
      public ChartFieldWellDefinition.SerializableChartFieldWellDefinition ChartFieldWellDefinition { get; set; }

      [XmlElement("Id")]
      public Guid Id { get; set; }

      internal ChartVisualization Unwrap(LayerDefinition layerDefinition, DataSource dataSource, CultureInfo modelCulture)
      {
        if (!(dataSource is GeoDataSource))
          throw new ArgumentException("DataSource is not of type GeoDataSource");
        else
          return new ChartVisualization(this, layerDefinition, dataSource as GeoDataSource, modelCulture);
      }
    }
  }
}
