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
                if ((string.Compare(fieldName, tuple.Item2, StringComparison.OrdinalIgnoreCase) == 0 &&
                     agFn == tuple.Item1) ||
                    (this.AnyMeasure && !agFn.HasValue &&
                     string.Compare(fieldName, "", StringComparison.OrdinalIgnoreCase) != 0 && !tuple.Item1.HasValue))
                {
                    if (tuple.Item1.HasValue)
                    {
                        if ((AggregationFunction)tuple.Item1 == AggregationFunction.UserDefined)
                            return tuple.Item2;
                        return string.Format(Resources.Tooltip_FieldFormat, tuple.Item2, tuple.Item1.Value.DisplayString());
                    }
                    return tuple.Item2;
                }
            }
            return null;
        }

        public string GetValueForField(string fieldName, AggregationFunction? agFn)
        {
            foreach (Tuple<AggregationFunction?, string, dynamic> tuple in this.Fields)
            {
                if ((string.Compare(fieldName, tuple.Item2, StringComparison.OrdinalIgnoreCase) == 0 && agFn == tuple.Item1) ||
                    ((this.AnyMeasure && agFn.HasValue && string.Compare(fieldName, "", StringComparison.OrdinalIgnoreCase) == 0 && tuple.Item1.HasValue)))
                {
                    if (tuple.Item3 == null)
                        return null;
                    if (tuple.Item1.HasValue)
                    {
                        if (tuple.Item3 is string)
                        {
                            return tuple.Item3;
                        }
                        string format = (tuple.Item1 != AggregationFunction.Count &&
                            tuple.Item1 != AggregationFunction.DistinctCount &&
                            Math.Abs(tuple.Item3) > 0.01 &&
                            Math.Abs(tuple.Item3) < 1E+21) ?
                            "N" : null;
                        return tuple.Item3.ToString(format);
                    }
                    return tuple.Item3.ToString();
                }
            }
            return null;
        }

        public bool IsMeasure(Tuple<AggregationFunction?, string, object> column)
        {
            if (column != null)
                return column.Item1.HasValue;
            return false;
        }
    }
}
