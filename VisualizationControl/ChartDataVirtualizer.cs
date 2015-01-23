using Microsoft.Reporting.Windows.Chart.Internal;
using Microsoft.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ChartDataVirtualizer : IXYChartAreaDataVirtualizer
  {
    private Func<int, int, ChartDataPoint> pointGenerator;
    private Func<ChartDataPoint, Tuple<int, int>> pointRetriever;
    private int size;
    private ChartScaleVirtualizationHelper scaleVirtualizer;
    private double lastMinimum;
    private double lastMaximum;

    public ChartDataVirtualizer(Func<int, int, ChartDataPoint> generator, Func<ChartDataPoint, Tuple<int, int>> retriever, int size)
    {
      this.pointGenerator = generator;
      this.pointRetriever = retriever;
      this.size = size;
      this.scaleVirtualizer = new ChartScaleVirtualizationHelper(this.pointGenerator, this.pointRetriever, this.size);
    }

    public void InitializeSeries(XYSeries series)
    {
    }

    public void UninitializeSeries(XYSeries series)
    {
    }

    public void InitializeAxisScale(Axis axis, Scale scale)
    {
      CategoryScale categoryScale = scale as CategoryScale;
      if (categoryScale == null)
        return;
      categoryScale.Binder = (ICategoryBinder) new ChartCategoryBinder();
      categoryScale.VirtualizationHelper = (IHierarchyVirtualizationHelper) this.scaleVirtualizer;
    }

    public void UninitializeAxisScale(Axis axis, Scale scale)
    {
    }

    public void UpdateSeriesForCurrentView(IEnumerable<XYSeries> series)
    {
      int i = 0;
      foreach (XYSeries xySeries in series)
      {
        CategoryScale categoryScale = xySeries.XAxis.Scale as CategoryScale;
        if (Enumerable.Count<Microsoft.Reporting.Windows.Chart.Internal.DataPoint>((IEnumerable<Microsoft.Reporting.Windows.Chart.Internal.DataPoint>) xySeries.DataPoints) == 0 || this.lastMinimum != categoryScale.ActualViewMinimum || this.lastMaximum != categoryScale.ActualViewMaximum)
        {
          this.lastMinimum = categoryScale.ActualViewMinimum;
          this.lastMaximum = categoryScale.ActualViewMaximum;
          xySeries.ItemsSource = (IEnumerable) Enumerable.Select<int, ChartDataPoint>(Enumerable.Range((int) Math.Floor(this.lastMinimum), (int) Math.Ceiling(this.lastMaximum)), (Func<int, ChartDataPoint>) (x => this.pointGenerator(i, x)));
        }
        this.scaleVirtualizer.RaiseLeavesChanged();
        ++i;
      }
      CategoryScale categoryScale1 = Enumerable.First<XYSeries>(series).XAxis.Scale as CategoryScale;
      categoryScale1.ItemSource = (IEnumerable) Enumerable.SelectMany<int, int, ChartDataPoint>(Enumerable.Range(0, i), (Func<int, IEnumerable<int>>) (ix => Enumerable.Range((int) Math.Floor(this.lastMinimum), (int) Math.Ceiling(this.lastMaximum))), (Func<int, int, ChartDataPoint>) ((ix, x) => this.pointGenerator(ix, x)));
      categoryScale1.Recalculate();
    }
  }
}
