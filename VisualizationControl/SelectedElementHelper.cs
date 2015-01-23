using Microsoft.Data.Visualization.Engine;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class SelectedElementHelper
  {
    public LayerViewModel Layer { get; set; }

    public InstanceId DataInstance { get; set; }

    public List<Tuple<AggregationFunction?, string, object>> GetColumns()
    {
      if (this.Layer == null)
        return (List<Tuple<AggregationFunction?, string, object>>) null;
      LayerDefinition layerDefinition = this.Layer.LayerDefinition;
      GeoVisualization geoVisualization = layerDefinition == null ? (GeoVisualization) null : layerDefinition.GeoVisualization;
      if (geoVisualization != null)
        return geoVisualization.TableColumnsWithValuesForId(this.DataInstance, false);
      else
        return (List<Tuple<AggregationFunction?, string, object>>) null;
    }

    public bool GetDisplayAnyMeasure()
    {
      if (this.Layer == null)
        return false;
      LayerDefinition layerDefinition = this.Layer.LayerDefinition;
      GeoVisualization geoVisualization = layerDefinition == null ? (GeoVisualization) null : layerDefinition.GeoVisualization;
      if (geoVisualization != null)
        return geoVisualization.GetDisplayAnyMeasure();
      else
        return false;
    }

    public AnnotationTemplateModel GetAnnotationForSelectedElements()
    {
      if (this.Layer == null)
        return (AnnotationTemplateModel) null;
      if (this.Layer == null)
        return (AnnotationTemplateModel) null;
      LayerDefinition layerDefinition = this.Layer.LayerDefinition;
      GeoVisualization geoVisualization = layerDefinition == null ? (GeoVisualization) null : layerDefinition.GeoVisualization;
      if (geoVisualization == null)
        return (AnnotationTemplateModel) null;
      return geoVisualization.GetAnnotation(this.DataInstance);
    }

    public void AddOrUpdateAnnotationForSelectedElements(AnnotationTemplateModel annotation)
    {
      if (this.Layer == null)
        return;
      LayerDefinition layerDefinition = this.Layer.LayerDefinition;
      GeoVisualization geoVisualization = layerDefinition == null ? (GeoVisualization) null : layerDefinition.GeoVisualization;
      IList<InstanceId> selectedElements = this.Layer.SelectedElements;
      if (annotation == null || selectedElements == null || geoVisualization == null)
        return;
      int indexForInstanceId = geoVisualization.GetSeriesIndexForInstanceId(this.DataInstance, true);
      if (indexForInstanceId == -1)
        return;
      bool flag = geoVisualization.VisualType == LayerType.RegionChart;
      foreach (InstanceId id in (IEnumerable<InstanceId>) selectedElements)
      {
        if (flag || indexForInstanceId == geoVisualization.GetSeriesIndexForInstanceId(id, true))
          geoVisualization.AddOrUpdateAnnotation(id, annotation.Clone());
      }
    }

    public void DeleteAnnotationForSelectedElements()
    {
      if (this.Layer == null)
        return;
      LayerDefinition layerDefinition = this.Layer.LayerDefinition;
      GeoVisualization geoVisualization = layerDefinition == null ? (GeoVisualization) null : layerDefinition.GeoVisualization;
      IList<InstanceId> selectedElements = this.Layer.SelectedElements;
      if (selectedElements == null || geoVisualization == null)
        return;
      foreach (InstanceId id in (IEnumerable<InstanceId>) selectedElements)
        geoVisualization.DeleteAnnotation(id);
    }

    public bool IsSelectedElementAnnotated()
    {
      if (this.Layer == null)
        return false;
      LayerDefinition layerDefinition = this.Layer.LayerDefinition;
      GeoVisualization geoVisualization = layerDefinition == null ? (GeoVisualization) null : layerDefinition.GeoVisualization;
      if (geoVisualization == null)
        return false;
      else
        return geoVisualization.IsInstanceIdAnnotated(this.DataInstance);
    }
  }
}
