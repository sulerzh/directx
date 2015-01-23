using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ChartFieldWellDefinition : FieldWellDefinition
  {
    public TableField Category { get; set; }

    public TableField Measure { get; set; }

    public AggregationFunction Function { get; set; }

    public string CategoryValue { get; set; }

    public ChartFieldWellDefinition(ChartFieldWellDefinition.SerializableChartFieldWellDefinition wellDefinition, ChartVisualization visualization)
      : base((Visualization) visualization)
    {
      this.UnWrap(wellDefinition);
    }

    public ChartFieldWellDefinition(ChartVisualization visualization)
      : base((Visualization) visualization)
    {
    }

    internal override void Shutdown()
    {
      this.Category = (TableField) null;
      this.Measure = (TableField) null;
      this.CategoryValue = (string) null;
      base.Shutdown();
    }

    internal override void ModelMetadataChanged(ModelMetadata modelMetadata, List<string> tablesWithUpdatedData, ref bool requery, ref bool queryChanged)
    {
    }

    internal override FieldWellDefinition.SerializableFieldWellDefinition Wrap()
    {
      return (FieldWellDefinition.SerializableFieldWellDefinition) new ChartFieldWellDefinition.SerializableChartFieldWellDefinition()
      {
        Category = (this.Category == null ? (TableField.SerializableTableField) null : this.Category.Wrap()),
        Measure = (this.Measure == null ? (TableField.SerializableTableField) null : this.Measure.Wrap()),
        Function = this.Function,
        CategoryValue = this.CategoryValue
      };
    }

    private void UnWrap(ChartFieldWellDefinition.SerializableChartFieldWellDefinition wellDefinition)
    {
      this.Category = wellDefinition.Category == null ? (TableField) null : wellDefinition.Category.Unwrap();
      this.Measure = wellDefinition.Measure == null ? (TableField) null : wellDefinition.Measure.Unwrap();
      this.CategoryValue = wellDefinition.CategoryValue;
      this.Function = wellDefinition.Function;
    }

    [Serializable]
    public class SerializableChartFieldWellDefinition : FieldWellDefinition.SerializableFieldWellDefinition
    {
      [XmlElement(typeof (TableColumn.SerializableTableColumn))]
      public TableField.SerializableTableField Category;
      [XmlElement("CalcFn", typeof (TableMeasure.SerializableTableMeasure))]
      [XmlElement(typeof (TableColumn.SerializableTableColumn))]
      public TableField.SerializableTableField Measure;
      [XmlElement]
      public string CategoryValue;
      [XmlElement]
      public AggregationFunction Function;

      internal override FieldWellDefinition Unwrap(Visualization visualization, CultureInfo modelCulture)
      {
        if (!(visualization is ChartVisualization))
          throw new ArgumentException("Visualization is not a ChartVisualization");
        else
          return (FieldWellDefinition) new ChartFieldWellDefinition(this, visualization as ChartVisualization);
      }
    }
  }
}
