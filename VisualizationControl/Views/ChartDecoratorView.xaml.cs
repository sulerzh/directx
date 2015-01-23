using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Reporting.Windows.Chart.Internal;
using Microsoft.Reporting.Windows.Common.Internal;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class ChartDecoratorView : UserControl
    {
        private static readonly ChartDecoratorView.InstanceEqComparer comparer = new ChartDecoratorView.InstanceEqComparer();
        private const double Size = 20.0;
        private const int MaxNumberOfSeries = 50;
        private readonly Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<ChartDecoratorView, object, PropertyChangedEventArgs> modelOnPropChange;
        private readonly Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<ChartDecoratorView, object, SelectionEventArgs> onSelectionChanged;
        private readonly WeakEventListener<ChartDecoratorView, Layer> onLayerAdded;
        private readonly WeakEventListener<ChartDecoratorView, Layer> onLayerRemoved;

        public ChartDecoratorView()
        {
            this.InitializeComponent();
            this.modelOnPropChange = new Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<ChartDecoratorView, object, PropertyChangedEventArgs>(this)
            {
                OnEventAction = new Action<ChartDecoratorView, object, PropertyChangedEventArgs>(ChartDecoratorView.ModelOnPropertyChanged)
            };
            this.onSelectionChanged = new Microsoft.Data.Visualization.VisualizationCommon.WeakEventListener<ChartDecoratorView, object, SelectionEventArgs>(this)
            {
                OnEventAction = new Action<ChartDecoratorView, object, SelectionEventArgs>(ChartDecoratorView.OnSelectionChanged)
            };
            this.onLayerAdded = new WeakEventListener<ChartDecoratorView, Layer>(this)
            {
                OnEventAction = new Action<ChartDecoratorView, Layer>(ChartDecoratorView.OnLayerAdded)
            };
            this.onLayerRemoved = new WeakEventListener<ChartDecoratorView, Layer>(this)
            {
                OnEventAction = new Action<ChartDecoratorView, Layer>(ChartDecoratorView.OnLayerRemoved)
            };
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.ChartDecoratorView_DataContextChanged);
        }

        private void ChartDecoratorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChartDecoratorModel chartDecoratorModel = e.OldValue as ChartDecoratorModel;
            if (chartDecoratorModel != null)
            {
                chartDecoratorModel.PropertyChanged -= new PropertyChangedEventHandler(this.modelOnPropChange.OnEvent);
                chartDecoratorModel.ChartVisualization.LayerDefinition.GeoVisualization.LayerAdded -= new Action<Layer>(this.onLayerAdded.OnEvent);
                chartDecoratorModel.ChartVisualization.LayerDefinition.GeoVisualization.LayerRemoved -= new Action<Layer>(this.onLayerRemoved.OnEvent);
            }
            ChartDecoratorModel model = e.NewValue as ChartDecoratorModel;
            if (model == null)
                return;
            model.PropertyChanged += new PropertyChangedEventHandler(this.modelOnPropChange.OnEvent);
            model.ChartVisualization.LayerDefinition.GeoVisualization.LayerAdded += new Action<Layer>(this.onLayerAdded.OnEvent);
            model.ChartVisualization.LayerDefinition.GeoVisualization.LayerRemoved += new Action<Layer>(this.onLayerRemoved.OnEvent);
            this.onLayerAdded.OnEvent(model.ChartVisualization.LayerDefinition.GeoVisualization.Layer);
            this.InitChart(model);
        }

        private void InitChart(ChartDecoratorModel model)
        {
            this.XYChartArea1.BeginInit();
            this.XYChartArea1.Series.Clear();
            Axis axis = Enumerable.First<Axis>((IEnumerable<Axis>)this.XYChartArea1.Axes, (Func<Axis, bool>)(ax => ax.Orientation == AxisOrientation.X));
            axis.Scale = (Microsoft.Reporting.Windows.Chart.Internal.Scale)new CategoryScale();
            axis.IsScrollZoomBarAllowsZooming = false;
            axis.IsScrollZoomBarAllwaysMaximized = true;
            axis.ShowLabels = false;
            HashSet<string> hashSet = new HashSet<string>();
            foreach (IGrouping<int, ChartDataPoint> grouping in Enumerable.Take<IGrouping<int, ChartDataPoint>>((IEnumerable<IGrouping<int, ChartDataPoint>>)Enumerable.OrderByDescending<IGrouping<int, ChartDataPoint>, int>(Enumerable.GroupBy<ChartDataPoint, int>((IEnumerable<ChartDataPoint>)model.DataPoints, (Func<ChartDataPoint, int>)(dp => dp.ShiftIndex)), (Func<IGrouping<int, ChartDataPoint>, int>)(group => Enumerable.Count<ChartDataPoint>((IEnumerable<ChartDataPoint>)group))), 50))
            {
                if (Enumerable.Any<ChartDataPoint>((IEnumerable<ChartDataPoint>)grouping))
                {
                    StackedColumnSeries stackedColumnSeries1 = new StackedColumnSeries();
                    stackedColumnSeries1.Name = string.Format("Series{0}", (object)grouping.Key);
                    stackedColumnSeries1.ClusterGroupKey = (object)grouping.Key;
                    stackedColumnSeries1.XAxisName = this.XAxis.Name;
                    stackedColumnSeries1.TransitionDuration = new TimeSpan?(TimeSpan.Zero);
                    stackedColumnSeries1.XValueType = DataValueType.Category;
                    stackedColumnSeries1.ItemsBinder = (IItemsBinder<Microsoft.Reporting.Windows.Chart.Internal.DataPoint>)new ChartDataPointBinder();
                    stackedColumnSeries1.DataPointStyle = new Style(typeof(StackedColumnDataPoint));
                    stackedColumnSeries1.ItemsSource = (IEnumerable)grouping;
                    StackedColumnSeries stackedColumnSeries2 = stackedColumnSeries1;
                    Brush brush = (Brush)new SolidColorBrush(Enumerable.First<ChartDataPoint>((IEnumerable<ChartDataPoint>)grouping).Color);
                    stackedColumnSeries2.DataPointStyle.Setters.Add((SetterBase)new Setter(Microsoft.Reporting.Windows.Chart.Internal.DataPoint.FillProperty, (object)brush));
                    CategoryScale categoryScale = this.XYChartArea1.GetScale((XYSeries)stackedColumnSeries2, AxisOrientation.X) as CategoryScale;
                    foreach (ChartDataPoint chartDataPoint in (IEnumerable<ChartDataPoint>)grouping)
                    {
                        if (!hashSet.Contains(chartDataPoint.XValue))
                        {
                            hashSet.Add(chartDataPoint.XValue);
                            ((Collection<Category>)categoryScale.Categories).Add(new Category((object)chartDataPoint.XValue, chartDataPoint.XValue, new Category[0]));
                        }
                    }
                    this.XYChartArea1.Series.Add((XYSeries)stackedColumnSeries2);
                }
                else
                    break;
            }
            this.SetClusteringBehavior(model.IsClustered);
            this.OrientationChanged(model.IsBar);
            if (Enumerable.Any<ChartDataPoint>((IEnumerable<ChartDataPoint>)model.DataPoints))
            {
                axis.ShowLabels = true;
                axis.CanRotateLabels = true;
                axis.IsScrollZoomBarAllowsZooming = true;
                axis.IsScrollZoomBarAllwaysMaximized = false;
                axis.Scale.ZoomToPercent(0.0, Math.Min(1.0, 20.0 / (double)Enumerable.Count<ChartDataPoint>((IEnumerable<ChartDataPoint>)model.DataPoints)));
            }
            this.XYChartArea1.EndInit();
        }

        private static void ModelOnPropertyChanged(ChartDecoratorView view, object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            ChartDecoratorModel model = sender as ChartDecoratorModel;
            if (model == null)
                return;
            if (propertyChangedEventArgs.PropertyName.Equals(model.PropertyDataPoints))
                DispatcherExtensions.CheckedInvoke(view.Dispatcher, (Action)(() => view.InitChart(model)), false);
            else if (propertyChangedEventArgs.PropertyName.Equals(model.PropertyIsClustered))
            {
                view.ClusteringChanged(model.IsClustered);
            }
            else
            {
                if (!propertyChangedEventArgs.PropertyName.Equals(model.PropertyIsBar))
                    return;
                view.OrientationChanged(model.IsBar);
            }
        }

        private void ClusteringChanged(bool isClustered)
        {
            this.XYChartArea1.BeginInit();
            this.SetClusteringBehavior(isClustered);
            this.XYChartArea1.EndInit();
        }

        private void OrientationChanged(bool isBar)
        {
            this.XYChartArea1.BeginInit();
            VisualStateManager.GoToState((FrameworkElement)this, isBar ? "BarChart" : "ColumnChart", false);
            this.XYChartArea1.EndInit();
        }

        private static void OnLayerRemoved(ChartDecoratorView view, Layer obj)
        {
            HitTestableLayer hitTestableLayer = obj as HitTestableLayer;
            if (hitTestableLayer == null)
                return;
            hitTestableLayer.OnSelectionChanged -= new EventHandler<SelectionEventArgs>(view.onSelectionChanged.OnEvent);
        }

        private static void OnLayerAdded(ChartDecoratorView view, Layer obj)
        {
            HitTestableLayer hitTestableLayer = obj as HitTestableLayer;
            if (hitTestableLayer == null)
                return;
            hitTestableLayer.OnSelectionChanged += new EventHandler<SelectionEventArgs>(view.onSelectionChanged.OnEvent);
        }

        private static void OnSelectionChanged(ChartDecoratorView view, object sender, SelectionEventArgs e)
        {
            DispatcherExtensions.CheckedInvoke(view.Dispatcher, (Action)(() =>
            {
                Guid? nullable1 = e.Tag as Guid?;
                if (nullable1.HasValue)
                {
                    ChartDecoratorModel chartDecoratorModel = view.DataContext as ChartDecoratorModel;
                    if (chartDecoratorModel != null && chartDecoratorModel.ChartVisualization != null && (chartDecoratorModel.ChartVisualization.LayerDefinition != null && chartDecoratorModel.ChartVisualization.LayerDefinition.GeoVisualization != null))
                    {
                        Guid? nullable2 = nullable1;
                        Guid id = chartDecoratorModel.Id;
                        if ((!nullable2.HasValue ? 0 : (nullable2.GetValueOrDefault() == id ? 1 : 0)) != 0)
                        {
                            chartDecoratorModel.ChartVisualization.LayerDefinition.GeoVisualization.FocusOnSelection();
                            return;
                        }
                    }
                }
                IEnumerable<InstanceId> second = Enumerable.Select<Microsoft.Reporting.Windows.Chart.Internal.DataPoint, InstanceId>(view.XYChartArea1.GetSelectedDataPoints(), (Func<Microsoft.Reporting.Windows.Chart.Internal.DataPoint, InstanceId>)(datapoint => (InstanceId)datapoint.Tag));
                if (Enumerable.Any<InstanceId>((IEnumerable<InstanceId>)e.SelectedIds) && !nullable1.HasValue && Enumerable.Count<InstanceId>(Enumerable.Intersect<InstanceId>(Enumerable.Distinct<InstanceId>((IEnumerable<InstanceId>)e.SelectedIds, (IEqualityComparer<InstanceId>)ChartDecoratorView.comparer), second, (IEqualityComparer<InstanceId>)ChartDecoratorView.comparer)) == Enumerable.Count<InstanceId>(Enumerable.Distinct<InstanceId>((IEnumerable<InstanceId>)e.SelectedIds, (IEqualityComparer<InstanceId>)ChartDecoratorView.comparer)))
                    return;
                view.XYChartArea1.DataPointSelectionChanged -= new DataPointSelectionChangedEventHandler(view.XYChartArea1_DataPointSelectionChanged);
                view.XYChartArea1.ClearSelectedDataPoints();
                view.XYChartArea1.DataPointSelectionChanged += new DataPointSelectionChangedEventHandler(view.XYChartArea1_DataPointSelectionChanged);
            }), false);
        }

        private void XYChartArea1_DataPointSelectionChanged(object sender, DataPointSelectionChangedEventArgs e)
        {
            InstanceId[] ids = Enumerable.ToArray<InstanceId>(Enumerable.Select<Microsoft.Reporting.Windows.Chart.Internal.DataPoint, InstanceId>(this.XYChartArea1.GetSelectedDataPoints(), (Func<Microsoft.Reporting.Windows.Chart.Internal.DataPoint, InstanceId>)(datapoint => (InstanceId)datapoint.Tag)));
            ChartDecoratorModel chartDecoratorModel = this.DataContext as ChartDecoratorModel;
            if (chartDecoratorModel == null || chartDecoratorModel.ChartVisualization.LayerDefinition == null)
                return;
            SelectionStyle style = Enumerable.Count<InstanceId>((IEnumerable<InstanceId>)ids) > 0 ? SelectionStyle.Saturation : SelectionStyle.None;
            chartDecoratorModel.ChartVisualization.LayerDefinition.GeoVisualization.SetSelected(ids, true, style, (object)chartDecoratorModel.Id);
        }

        private void SetClusteringBehavior(bool isClustered)
        {
            int num = 0;
            foreach (StackedColumnSeries stackedColumnSeries in this.XYChartArea1.Series)
            {
                stackedColumnSeries.GroupingKey = (object)num;
                if (isClustered)
                    ++num;
            }
        }

        private void dropButton_Click(object sender, RoutedEventArgs e)
        {
            this.SortValues.IsDropDownOpen = !this.SortValues.IsDropDownOpen;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState((FrameworkElement)this, "MouseOver", true);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState((FrameworkElement)this, "Normal", true);
        }


        private class InstanceEqComparer : IEqualityComparer<InstanceId>
        {
            public bool Equals(InstanceId x, InstanceId y)
            {
                return x.ElementId.Equals(y.ElementId);
            }

            public int GetHashCode(InstanceId obj)
            {
                return obj.ElementId.GetHashCode();
            }
        }
    }
}
