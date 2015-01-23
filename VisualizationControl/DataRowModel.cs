using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class DataRowModel : PropertyChangeNotificationBase
    {
        public const string FieldNameForAnyMeasure = "";

        public List<Tuple<AggregationFunction?, string, dynamic>> Fields { get; private set; }

        public bool AnyMeasure { get; set; }

        public DataRowModel(List<Tuple<AggregationFunction?, string, dynamic>> columns = null, bool anyMeasure = false)
        {
            if (columns == null)
            {
                this.Fields = new List<Tuple<AggregationFunction?, string, dynamic>>(0);
                this.AnyMeasure = false;
            }
            else
            {
                this.AnyMeasure = anyMeasure;
                this.Fields = columns;
            }
        }

        public string GetColumnName(string fieldName, AggregationFunction? agFn)
        {
            foreach (Tuple<AggregationFunction?, string, object> tuple in this.Fields)
            {
                if (string.Compare(fieldName, tuple.Item2, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    AggregationFunction? nullable1 = agFn;
                    AggregationFunction? nullable2 = tuple.Item1;
                    if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 0 : (nullable1.HasValue == nullable2.HasValue ? 1 : 0)) != 0)
                        goto label_5;
                }
                if (!this.AnyMeasure || !agFn.HasValue || string.Compare(fieldName, "", StringComparison.OrdinalIgnoreCase) != 0 || !tuple.Item1.HasValue)
                    continue;
            label_5:
                if (!tuple.Item1.HasValue)
                    return tuple.Item2;
                AggregationFunction? nullable = tuple.Item1;
                if ((nullable.GetValueOrDefault() != AggregationFunction.UserDefined ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
                    return tuple.Item2;
                else
                    return string.Format(Resources.Tooltip_FieldFormat, (object)tuple.Item2, (object)AggregationFunctionExtensions.DisplayString(tuple.Item1.Value));
            }
            return (string)null;
        }

        public string GetValueForField(string fieldName, AggregationFunction? agFn)
        {
            foreach (Tuple<AggregationFunction?, string, dynamic> tuple in this.Fields)
            {
                if (string.Compare(fieldName, tuple.Item2, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (agFn.GetValueOrDefault() == tuple.Item1.GetValueOrDefault() &&
                        agFn.HasValue == tuple.Item1.HasValue)
                    {
                        if (tuple.Item3 == null)
                            return (string)null;
                        if (tuple.Item1.HasValue)
                        {
                            if (tuple.Item3 is string)
                            {
                                return (string)tuple.Item3;
                            }
                            else
                            {
                                AggregationFunction? nullable1 = tuple.Item1;
                                int num1;
                                if ((nullable1.GetValueOrDefault() != AggregationFunction.Count ? 1 : (!nullable1.HasValue ? 1 : 0)) != 0)
                                {
                                    AggregationFunction? nullable2 = tuple.Item1;
                                    num1 = nullable2.GetValueOrDefault() != AggregationFunction.DistinctCount ? 1 : (!nullable2.HasValue ? 1 : 0);
                                }
                                else
                                    num1 = 0;
                                bool flag = num1 != 0;

                                string format = (flag && Math.Abs(tuple.Item3) > 0.01 && Math.Abs(tuple.Item3) < 1E+21) ? "N" : null;
                                return (string)tuple.Item3.ToString(format);

                            }
                        }
                        else
                        {
                            return (string)tuple.Item3.ToString();
                        }
                    }
                }
                if (!this.AnyMeasure || !agFn.HasValue || string.Compare(fieldName, "", StringComparison.OrdinalIgnoreCase) != 0 || !tuple.Item1.HasValue)
                    continue;
            }
            return (string)null;
        }

        public bool IsMeasure(Tuple<AggregationFunction?, string, object> column)
        {
            if (column != null)
                return column.Item1.HasValue;
            else
                return false;
        }
    }
}
