using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class TableFieldToolTipViewModel : ViewModelBase
    {
        public ObservableCollectionEx<Tuple<string, string>> ToolTipProperties { get; private set; }

        public TableFieldToolTipViewModel()
        {
            this.ToolTipProperties = new ObservableCollectionEx<Tuple<string, string>>();
        }

        public TableFieldToolTipViewModel(GeoVisualization visualization, InstanceId id, bool showRelatedCategories)
            : this()
        {
            if (visualization == null)
                return;
            List<Tuple<AggregationFunction?, string, dynamic>> list = visualization.TableColumnsWithValuesForId(id,
                showRelatedCategories);
            if (list == null)
                return;
            foreach (Tuple<AggregationFunction?, string, dynamic> tuple in list)
            {
                if (tuple.Item3 != null)
                {
                    if (tuple.Item1.HasValue)
                    {
                        string name = tuple.Item1.GetValueOrDefault() == AggregationFunction.UserDefined &&
                                      tuple.Item1.HasValue ? tuple.Item2
                            : string.Format(
                                Resources.Tooltip_FieldFormat,
                                tuple.Item2,
                                AggregationFunctionExtensions.DisplayString(tuple.Item1.Value));
                        if (tuple.Item3 is string)
                        {
                            this.AddProperty(name, tuple.Item3);
                        }
                        else
                        {
                            string format =
                                (tuple.Item1.GetValueOrDefault() != AggregationFunction.Count || !tuple.Item1.HasValue) &&
                                (tuple.Item1.GetValueOrDefault() != AggregationFunction.DistinctCount ||
                                 !tuple.Item1.HasValue) &&
                                Math.Abs(tuple.Item3) > 0.01 &&
                                Math.Abs(tuple.Item3) < 1E+21
                                    ? "N"
                                    : null;
                            this.AddProperty(name, tuple.Item3, format);
                        }
                    }
                    else
                    {
                        this.AddProperty(tuple.Item2, tuple.Item3);
                    }
                }
            }
        }

        private void AddProperty(object name, dynamic value, string formatString = null)
        {
            if (name == null || value == null)
                return;
            if (formatString != null)
            {
                this.ToolTipProperties.Add(new Tuple<string, string>(name.ToString(), value.ToString(formatString)));
            }
            else
            {
                this.ToolTipProperties.Add(new Tuple<string, string>(name.ToString(), value.ToString()));
            }
        }
    }
}
