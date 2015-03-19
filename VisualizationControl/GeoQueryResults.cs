using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class GeoQueryResults
    {
        public ModelQueryKeyColumn FirstModelQueryKey { get; set; }

        public ModelQueryKeyColumn Latitude { get; set; }

        public ModelQueryKeyColumn Longitude { get; set; }

        public List<ModelQueryKeyColumn> GeoFields { get; set; }

        public ModelQueryTimeKeyColumn Time { get; set; }

        public ModelQueryIndexedKeyColumn Category { get; set; }

        public List<ModelQueryMeasureColumn> Measures { get; set; }

        public int ResultsItemCount { get; set; }

        public bool HoldTillReplaced { get; set; }

        public CultureInfo ModelCulture { get; set; }

        public int IndexForCategory(string category)
        {
            if (this.Category == null) return -1;
            string[] array = (string[])this.Category.AllValues;
            TableColumn tableColumn = this.Category.TableColumn as TableColumn;
            if (tableColumn != null && tableColumn.DataType == TableMemberDataType.String)
                return Array.BinarySearch<string>(array, category, StringComparer.Create(this.ModelCulture, false));
            int num = 0;
            foreach (string item in array)
            {
                if (string.Compare(category, item, false, this.ModelCulture) == 0)
                    return num;
                ++num;
            }
            return -1;
        }

        public int GetBucketForRow(int row)
        {
            try
            {
                return this.FirstModelQueryKey.GetBucketForRow(row);
            }
            catch (NullReferenceException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
        }

        public int GetFirstRowInBucket(int row)
        {
            try
            {
                return this.FirstModelQueryKey.GetFirstRowInBucket(row);
            }
            catch (NullReferenceException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
        }

        public int GetFirstRowInNextBucket(int row)
        {
            try
            {
                return this.FirstModelQueryKey.GetFirstRowInNextBucket(row, this.ResultsItemCount);
            }
            catch (NullReferenceException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
        }

        public DateTime? GetEndTimeForRow(int row)
        {
            if (this.Time == null)
                return new DateTime?();
            try
            {
                DateTime? endTime = this.Time.EndTime[row];
                if (this.HoldTillReplaced)
                {
                    int index = row + 1;

                    if (index > this.GetFirstRowInNextBucket(row) - 1 ||
                        (this.Category != null && this.Category.Values[row] != this.Category.Values[index]))
                        return new DateTime?();
                    return this.Time.StartTime[index];
                }
                return endTime;
            }
            catch (NullReferenceException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
        }

        public void Log(TraceSource traceSource, CancellationToken cancellationToken, int dataVersion, uint numMeasures)
        {
            if (traceSource != null)
            {
                int resultsItemCount = this.ResultsItemCount;
                StringBuilder builder = new StringBuilder();
                traceSource.TraceInformation("{0}Data Version = {1}{2}Row Count: {3}", new object[] { "\t", dataVersion, ",\t", resultsItemCount });
                try
                {
                    for (int i = 0; i < resultsItemCount; i++)
                    {
                        builder.AppendFormat("{0}Row {1}{2}GeoCluster {3}:{4}", new object[] { "\t", i, ",\t", this.GetBucketForRow(i), "\t" });
                        if (this.Latitude != null)
                        {
                            builder.AppendFormat("{0}: {1}{2}", this.Latitude.TableColumn.ModelQueryName, this.Latitude.Values[i], ",\t");
                        }
                        if (this.Longitude != null)
                        {
                            builder.AppendFormat("{0}: {1}{2}", this.Longitude.TableColumn.ModelQueryName, this.Longitude.Values[i], ",\t");
                        }
                        if (this.GeoFields != null)
                        {
                            foreach (ModelQueryKeyColumn column in this.GeoFields)
                            {
                                builder.AppendFormat("{0}: {1}{2}", column.TableColumn.ModelQueryName, column.Values[i], ",\t");
                            }
                        }
                        if (this.Category != null)
                        {
                            builder.AppendFormat("{0}: CategoryIndex={1}{2}", this.Category.TableColumn.ModelQueryName, this.Category.Values[i], ",\t");
                            builder.AppendFormat("{0}: Category={1}{2}", this.Category.TableColumn.ModelQueryName, this.Category.AllValues[this.Category.Values[i]], ",\t");
                        }
                        if (this.Time != null)
                        {
                            DateTime? endTimeForRow = (DateTime?)this.Time.Values[i];
                            builder.AppendFormat("{0}: Start={1}{2}", this.Time.TableColumn.ModelQueryName, endTimeForRow.HasValue ? endTimeForRow.Value.ToString() : "null", ",\t");
                            endTimeForRow = this.GetEndTimeForRow(i);
                            builder.AppendFormat("{0}: End={1}{2}", this.Time.TableColumn.ModelQueryName, endTimeForRow.HasValue ? endTimeForRow.Value.ToString() : "null", ",\t");
                            builder.AppendFormat("{0}: Chunk={1}{2}", this.Time.TableColumn.ModelQueryName, this.Time.TimeChunk, ",\t");
                        }
                        if (this.Measures != null)
                        {
                            int measureIndex = 0;
                            if (this.Measures.Count > 0)
                            {
                                foreach (ModelQueryMeasureColumn column2 in this.Measures)
                                {
                                    builder.AppendFormat("{0}: {1}{2}", column2.TableColumn.ModelQueryName, column2.Values[i], ",\t");
                                    builder.AppendFormat("{0}: Id={1}{2}", column2.TableColumn.ModelQueryName, GeoDataSource.GetInstanceIdForRow(i, measureIndex, numMeasures).ElementId, ",\t");
                                    builder.AppendFormat("{0}: Aggregation={1}{2}", column2.TableColumn.ModelQueryName, column2.AggregationFunction, ",\t");
                                    measureIndex++;
                                }
                            }
                            else
                            {
                                builder.AppendFormat("Id={0}{1}", GeoDataSource.GetInstanceIdForRow(i, measureIndex, numMeasures).ElementId, ",\t");
                            }
                        }
                        if (builder.Length >= ",\t".Length)
                        {
                            traceSource.TraceInformation(builder.ToString(0, builder.Length - ",\t".Length));
                        }
                        else
                        {
                            traceSource.TraceInformation(builder.ToString());
                        }
                        builder.Clear();
                        if ((i % 50) == 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    traceSource.TraceEvent(TraceEventType.Error, 0, "Logging terminated, query results may be stale: caught exception {0}", new object[] { exception });
                }
                catch (IndexOutOfRangeException exception2)
                {
                    traceSource.TraceEvent(TraceEventType.Error, 0, "Logging terminated, query results may be stale: caught exception {0}", new object[] { exception2 });
                }
                catch (NullReferenceException exception3)
                {
                    traceSource.TraceEvent(TraceEventType.Error, 0, "Logging terminated, query results may be stale: caught exception {0}", new object[] { exception3 });
                }
                catch (DataSource.InvalidQueryResultsException exception4)
                {
                    traceSource.TraceEvent(TraceEventType.Error, 0, "Logging terminated, query results may be stale: caught exception {0}", new object[] { exception4 });
                }
                catch (Exception exception5)
                {
                    traceSource.TraceEvent(TraceEventType.Critical, 0, "Logging terminated: caught exception {0}", new object[] { exception5 });
                }
            }

        }

        internal void Shutdown()
        {
            this.FirstModelQueryKey = null;
            if (this.Latitude != null)
            {
                this.Latitude.Shutdown();
                this.Latitude = null;
            }
            if (this.Longitude != null)
            {
                this.Longitude.Shutdown();
                this.Longitude = null;
            }
            if (this.Time != null)
            {
                this.Time.Shutdown();
                this.Time = null;
            }
            if (this.Category != null)
            {
                this.Category.Shutdown();
                this.Category = null;
            }
            if (this.Measures != null)
            {
                List<ModelQueryMeasureColumn> measures = this.Measures;
                this.Measures = null;
                measures.ForEach(measure => measure.Shutdown());
            }
            if (this.GeoFields == null)
                return;
            List<ModelQueryKeyColumn> geoFields = this.GeoFields;
            this.GeoFields = null;
            geoFields.ForEach(geo => geo.Shutdown());
            geoFields.Clear();
        }
    }
}
