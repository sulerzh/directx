using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Visualization.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class ChartDataView : DataView
    {
        public bool IsQueryResultAvailable
        {
            get
            {
                GeoDataSource geoDataSource = this.Source as GeoDataSource;
                if (geoDataSource != null)
                    return geoDataSource.QueryResults != null;
                else
                    return false;
            }
        }

        public string GeoFieldName
        {
            get
            {
                GeoField geoField = (GeoField)null;
                try
                {
                    GeoDataSource geoDataSource = (GeoDataSource)this.Source;
                    if (geoDataSource != null)
                    {
                        if (geoDataSource.QueryResults != null)
                            geoField = geoDataSource.GetGeoFieldUsedInQuery(this.Source.DataVersion);
                    }
                }
                catch (DataSource.InvalidQueryResultsException ex)
                {
                }
                string str = string.Empty;
                if (geoField != null)
                {
                    if (geoField.HasLatLongOrXY)
                    {
                        StringBuilder sb = new StringBuilder();
                        geoField.GeoColumns.ForEach((Action<TableColumn>)(tc => sb.AppendFormat("{0} ", (object)tc.Name.Trim())));
                        str = ((object)sb).ToString().TrimEnd();
                    }
                    else
                        str = Enumerable.First<TableColumn>((IEnumerable<TableColumn>)geoField.GeoColumns).Name.Trim();
                }
                return str;
            }
        }

        public SortField[] SortFields
        {
            get
            {
                GeoDataSource geoDataSource = (GeoDataSource)this.Source;
                GeoQueryResults results = geoDataSource == null ? (GeoQueryResults)null : geoDataSource.QueryResults;
                if (results != null)
                {
                    if (results.Category != null && results.Category.AllValues != null)
                    {
                        return
                            ((string[])results.Category.AllValues).Select<string, SortField>(
                                (Func<string, SortField>)(s => new SortField()
                                {
                                    Column = results.Category.TableColumn,
                                    CategoryValue = s
                                })).ToArray<SortField>();

                    }
                    else if (results.Measures != null)
                    {
                        return
                            results.Measures.Select<ModelQueryMeasureColumn, SortField>(
                                (Func<ModelQueryMeasureColumn, SortField>)(m => new SortField()
                                {
                                    CategoryValue = (string)null,
                                    Function = m.AggregationFunction,
                                    Column = m.TableColumn
                                })).ToArray<SortField>();
                    }
                }
                return new SortField[0];
            }
        }

        public ChartDataView(DataSource source)
            : base(source)
        {
        }

        public Tuple<TableMember, string> GetCategoryOrDefault(TableMember category, string value)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            GeoQueryResults geoQueryResults = geoDataSource == null ? (GeoQueryResults)null : geoDataSource.QueryResults;
            if (geoQueryResults != null)
            {
                ModelQueryIndexedKeyColumn category1 = geoQueryResults.Category;
                if (category1 != null)
                {
                    string[] strArray = (string[])category1.AllValues;
                    if (strArray != null)
                    {
                        string str = geoQueryResults.IndexForCategory(value) >= 0 ? value : (strArray.Length == 0 ? (string)null : strArray[0]);
                        return Tuple.Create<TableMember, string>(category1.TableColumn, str);
                    }
                }
            }
            return Tuple.Create<TableMember, string>((TableMember)null, (string)null);
        }

        public Tuple<TableMember, AggregationFunction> GetMeasureOrDefault(TableMember measure, AggregationFunction function)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            GeoQueryResults geoQueryResults = geoDataSource == null ? (GeoQueryResults)null : geoDataSource.QueryResults;
            if (geoQueryResults != null)
            {
                if (measure != null && geoQueryResults.Measures != null && Enumerable.Any<ModelQueryMeasureColumn>((IEnumerable<ModelQueryMeasureColumn>)geoQueryResults.Measures, (Func<ModelQueryMeasureColumn, bool>)(m =>
                {
                    if (m != null && m.TableColumn != null && m.TableColumn.RefersToTheSameMemberAs(measure))
                        return m.AggregationFunction == function;
                    else
                        return false;
                })))
                    return Tuple.Create<TableMember, AggregationFunction>(measure, function);
                if (geoQueryResults.Measures != null && geoQueryResults.Measures.Count > 0)
                    return Tuple.Create<TableMember, AggregationFunction>(geoQueryResults.Measures[0].TableColumn, geoQueryResults.Measures[0].AggregationFunction);
            }
            return Tuple.Create<TableMember, AggregationFunction>((TableMember)null, AggregationFunction.None);
        }

        public List<ChartDataView.ChartResult> GetDataByCategory(string categoryValue, TableMember measure, AggregationFunction function, int count, bool top, out ModelQueryMeasureColumn measureUsed)
        {
            measureUsed = null;
            measureUsed = (ModelQueryMeasureColumn)null;
            GeoDataSource ds = (GeoDataSource)this.Source;
            GeoQueryResults queryResults = ds == null ? (GeoQueryResults)null : ds.QueryResults;
            if (queryResults == null)
                return new List<ChartDataView.ChartResult>();
            int categoryIndex = -1;

            if (queryResults.Category != null && queryResults.Category.AllValues != null && !string.IsNullOrEmpty(categoryValue))
                categoryIndex = queryResults.IndexForCategory(categoryValue);
            int measureIndex = queryResults.Measures.FindIndex((Predicate<ModelQueryMeasureColumn>)(m =>
            {
                if (m.TableColumn.RefersToTheSameMemberAs(measure))
                    return m.AggregationFunction == function;
                else
                    return false;
            }));
            if (measureIndex < 0)
                return new List<ChartDataView.ChartResult>();

            double[] numArray = (double[])queryResults.Measures[measureIndex].Values;
            if (numArray == null)
                return new List<ChartDataView.ChartResult>();
            try
            {
                measureUsed = queryResults.Measures[measureIndex];
                SortedList<double, int> sortedList = new SortedList<double, int>((IComparer<double>)new ChartDataView.SortComparer()
                {
                    Top = (top ? -1 : 1)
                });
                for (int row = 0; row < numArray.Length; ++row)
                {
                    if (categoryIndex > -1)
                    {
                        if (((int[])queryResults.Category.Values)[row] != categoryIndex)
                            continue;
                    }
                    if (!ChartDataView.IsNonEndTimeRow(queryResults, row) && !double.IsNaN(numArray[row]) && !double.IsInfinity(numArray[row]))
                    {
                        if (sortedList.Count < count)
                            sortedList.Add(numArray[row], row);
                        else if (top && sortedList.Keys[sortedList.Count - 1] < numArray[row] || !top && sortedList.Keys[sortedList.Count - 1] > numArray[row])
                        {
                            sortedList.RemoveAt(sortedList.Count - 1);
                            sortedList.Add(numArray[row], row);
                        }
                    }
                }
                return Enumerable.ToList<ChartDataView.ChartResult>(Enumerable.Select<KeyValuePair<double, int>, ChartDataView.ChartResult>((IEnumerable<KeyValuePair<double, int>>)sortedList, (Func<KeyValuePair<double, int>, ChartDataView.ChartResult>)(returnVal => ChartDataView.GetChartResult(ds, queryResults, returnVal.Value, measureIndex, categoryIndex))));
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return new List<ChartDataView.ChartResult>();
            }
            catch (NullReferenceException ex)
            {
                return new List<ChartDataView.ChartResult>();
            }
            return new List<ChartDataView.ChartResult>();
        }

        private static ChartDataView.ChartResult GetChartResult(GeoDataSource ds, GeoQueryResults queryResults, int rowIndex, int measureIndex, int categoryIndex)
        {

            ChartResult result = new ChartResult
            {
                Geo = ds.HasLatLong ? 
                    string.Format(Resources.ChartGeoDisplayString, queryResults.Latitude.Values[rowIndex], queryResults.Longitude.Values[rowIndex]) : 
                    GetGeoFieldCompositeName(queryResults.GeoFields, rowIndex),
                Values = new List<ChartResult.DataPoint>()
            };
            if (queryResults.Measures[measureIndex].AggregationFunction == AggregationFunction.None)
            {
                ChartResult.DataPoint item = new ChartResult.DataPoint
                {
                    Value = (double)queryResults.Measures[measureIndex].Values[rowIndex]
                };

                item.CategoryName = categoryIndex >= 0 ? (string)queryResults.Category.AllValues[categoryIndex] : string.Empty;
                item.Id = ds.GetInstanceIdForRow(rowIndex, measureIndex);
                item.Measure = Tuple.Create<TableMember, AggregationFunction>(queryResults.Measures[measureIndex].TableColumn, AggregationFunction.None);
                result.Values.Add(item);
                return result;
            }
            if (categoryIndex < 0)
            {
                for (int j = 0; j < queryResults.Measures.Count; j++)
                {
                    ModelQueryMeasureColumn column = queryResults.Measures[j];
                    double num2 = (double)((dynamic)column.Values)[rowIndex];
                    ChartResult.DataPoint point2 = new ChartResult.DataPoint
                    {
                        Value = num2,
                        Measure = Tuple.Create<TableMember, AggregationFunction>(column.TableColumn, column.AggregationFunction),
                        Id = ds.GetInstanceIdForRow(rowIndex, j),
                        ShiftIndex = j
                    };
                    result.Values.Add(point2);
                }
                return result;
            }
            int firstRowInBucket = queryResults.GetFirstRowInBucket(rowIndex);
            int firstRowInNextBucket = queryResults.GetFirstRowInNextBucket(rowIndex);
            for (int i = firstRowInBucket; i < firstRowInNextBucket; i++)
            {
                double d = (double)((dynamic)queryResults.Measures[measureIndex].Values)[i];
                if ((!double.IsNaN(d) && !double.IsInfinity(d)) && !IsNonEndTimeRow(queryResults, i))
                {
                    ChartResult.DataPoint point3 = new ChartResult.DataPoint
                    {
                        Value = d,
                        CategoryName = (string)((dynamic)queryResults.Category.AllValues)[((dynamic)queryResults.Category.Values)[i]],
                        ShiftIndex = (int)((dynamic)queryResults.Category.Values)[i],
                        Id = ds.GetInstanceIdForRow(i, measureIndex),
                        Measure = Tuple.Create<TableMember, AggregationFunction>(queryResults.Measures[measureIndex].TableColumn, queryResults.Measures[measureIndex].AggregationFunction)
                    };
                    result.Values.Add(point3);
                }
            }
            return result;

        }

        private static bool IsNonEndTimeRow(GeoQueryResults results, int row)
        {
            DateTime? endTimeForRow = results.GetEndTimeForRow(row);
            if (results.Time == null)
                return false;
            if (!results.HoldTillReplaced)
            {
                DateTime? nullable = endTimeForRow;
                DateTime max = results.Time.Max;
                if ((!nullable.HasValue ? 1 : (nullable.GetValueOrDefault() != max ? 1 : 0)) != 0)
                    return true;
            }
            if (results.HoldTillReplaced)
                return endTimeForRow.HasValue;
            else
                return false;
        }

        private static string GetGeoFieldCompositeName(List<ModelQueryKeyColumn> columns, int rowIndex)
        {
            string str = (string)columns[0].Values[rowIndex];
            for (int i = 1; i < columns.Count; i++)
            {
                str = (string)string.Format(Resources.ChartGeoDisplayString, str, ((dynamic)columns[i].Values)[rowIndex]);
            }
            return str;

        }

        internal override void Shutdown()
        {
            base.Shutdown();
        }

        public class ChartResult
        {
            public string Geo { get; set; }

            public List<ChartDataView.ChartResult.DataPoint> Values { get; set; }

            public class DataPoint
            {
                public double Value { get; set; }

                public string CategoryName { get; set; }

                public Color Color { get; set; }

                public InstanceId Id { get; set; }

                public int ShiftIndex { get; set; }

                public Tuple<TableMember, AggregationFunction> Measure { get; set; }
            }
        }

        private class SortComparer : IComparer<double>
        {
            public int Top { get; set; }

            public int Compare(double x, double y)
            {
                int num = x.CompareTo(y);
                if (num != 0)
                    return num * this.Top;
                else
                    return 1;
            }
        }
    }
}
