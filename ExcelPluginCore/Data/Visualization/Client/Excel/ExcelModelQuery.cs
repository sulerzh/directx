using ADODB;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Microsoft.Data.Visualization.Client.Excel
{
    public class ExcelModelQuery : ModelQuery
    {
        internal static readonly string[] DAXAggregationFunction =
        {
            string.Empty,
            "SUM",
            "AVERAGEA",
            "COUNTA",
            "MIN",
            "MAX",
            "DISTINCTCOUNT",
            string.Empty
        };
        private const int MaxQueryResults = 2147483647;
        private const int NumKeyColumnDataTypes = 5;
        private List<ModelQueryKeyColumn> keys;
        private List<ModelQueryMeasureColumn> measures;
        private List<ModelQueryKeyColumn> orderedUnaliasedKeys;
        private List<ModelQueryKeyColumn> keyOrderInQueryResults;

        // 数据源链接
        public Connection ModelConnection { get; private set; }

        // 构造函数
        public ExcelModelQuery(string name, Microsoft.Data.Visualization.VisualizationControls.Filter filter, CultureInfo modelCulture, Func<Connection> getConnection, CancellationToken cancellationToken)
            : base(name, filter, modelCulture, cancellationToken)
        {
            if (getConnection == null)
                throw new ArgumentNullException("getConnection");
            this.ModelConnection = getConnection();
            this.keys = new List<ModelQueryKeyColumn>();
            this.measures = new List<ModelQueryMeasureColumn>();
            this.orderedUnaliasedKeys = new List<ModelQueryKeyColumn>();
            this.keyOrderInQueryResults = new List<ModelQueryKeyColumn>();
            this.Clear();
        }

        public override void Clear()
        {
            this.keys.Clear();
            this.measures.Clear();
            this.orderedUnaliasedKeys.Clear();
            this.keyOrderInQueryResults.Clear();
            this.ResultsItemCount = 0;
            this.QueryUsesAggregation = false;
        }

        protected override void Shutdown()
        {
            this.Clear();
            this.keys = null;
            this.measures = null;
            this.orderedUnaliasedKeys = null;
            this.keyOrderInQueryResults = null;
            this.ModelConnection = null;
            base.Shutdown();
        }

        public override void AddKey(ModelQueryKeyColumn modelQueryKey)
        {
            if (modelQueryKey == null)
                throw new ArgumentNullException("modelQueryKey");
            if (modelQueryKey.TableColumn == null)
                throw new ArgumentException("TableColumn must not be null");
            if (!modelQueryKey.Sort && modelQueryKey.UseForBuckets)
                throw new ArgumentException("key.Sort must be set since key.UseForBuckets is set");
            bool flag = this.keys.Count <= 0 || this.keys.Last().UseForBuckets;
            if (modelQueryKey.UseForBuckets && !flag)
                throw new ArgumentException("key.UseForBuckets is true but is false for the previously added key");
            if (modelQueryKey is ModelQueryTimeKeyColumn && this.keys.Any(k => k is ModelQueryTimeKeyColumn))
                throw new ArgumentException("Only one key of type TimeKeyColumn is supported");
            if (modelQueryKey is ModelQueryTimeKeyColumn && modelQueryKey.Type != KeyColumnDataType.DateTime)
                throw new ArgumentException("TimeKeyColumn.Type must be set to KeyColumnDataType.DateTime");
            this.keys.Add(modelQueryKey);
        }

        public override void AddMeasure(ModelQueryMeasureColumn modelQueryMeasure)
        {
            if (modelQueryMeasure == null)
                throw new ArgumentNullException("modelQueryMeasure");
            if (modelQueryMeasure.TableColumn == null)
                throw new ArgumentException("TableColumn must not be null");
            if (modelQueryMeasure.Accumulate != ModelQueryMeasureColumn.AccumulationType.NoAccumulation && modelQueryMeasure.AggregationFunction == AggregationFunction.None)
                throw new ArgumentException("Accumulation cannot be performed on measures with AggregationFunction = None");
            if (modelQueryMeasure.Accumulate != ModelQueryMeasureColumn.AccumulationType.NoAccumulation && modelQueryMeasure.AggregationFunction == AggregationFunction.UserDefined)
                throw new ArgumentException("Accumulation cannot be performed on measures with AggregationFunction = UserDefined");
            if (modelQueryMeasure.Accumulate != ModelQueryMeasureColumn.AccumulationType.NoAccumulation && modelQueryMeasure.AggregationFunction == AggregationFunction.DistinctCount)
                throw new ArgumentException("Accumulation cannot be performed on measures with AggregationFunction = DistinctCount");
            if (modelQueryMeasure.Accumulate != ModelQueryMeasureColumn.AccumulationType.NoAccumulation && modelQueryMeasure.AggregationFunction == AggregationFunction.Average)
                throw new ArgumentException("Accumulation is not currently supported on measures with AggregationFunction = Average");
            if (modelQueryMeasure.Accumulate == ModelQueryMeasureColumn.AccumulationType.AccumulateByBucketsAndIndex != (modelQueryMeasure.ModelQueryIndexedKey != null))
                throw new ArgumentException("Accumulation type " + (modelQueryMeasure.ModelQueryIndexedKey == null ? "requires" : "must not have an IndexedKey"));
            this.measures.Add(modelQueryMeasure);
        }

        public override void QueryData(CancellationToken cancellationToken)
        {
            this.Reset();
            if (this.keys.Count == 0)
            {
                this.ResultsItemCount = 0;
                this.QueryUsesAggregation = false;
            }
            else
            {
                long buildTime = -1L;
                long evalTime = -1L;
                long queryTime = -1L;
                long evalQueryFetchTime = -1L;
                int numFetches = -1;
                long sanitizeTime = -1L;
                long indexBuildTime = -1L;
                long bucketizeTime = -1L;
                long accumulateTime = -1L;
                Stopwatch stopwatch = Stopwatch.StartNew();
                long totalTime = -1L;
                bool flag = false;
                try
                {
                    this.QueryUsesAggregation = this.AllMeasuresAreAggregated();
                    if (!this.QueryUsesAggregation)
                    {
                        bool warn = false;

                        this.measures.ForEach((m =>
                        {
                            warn |= m.AggregationFunction != AggregationFunction.None;
                        }));
                        if (warn)
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: Building the query without using aggregation functions. Aggregation functions have been set with a modelQueryMeasure field. They will be ignored.", this.Name);
                            this.measures.ForEach((m => m.AggregationFunction = AggregationFunction.None));
                        }
                    }
                    totalTime = stopwatch.ElapsedMilliseconds;
                    this.AliasColumns();
                    long elapsedMilliseconds1 = stopwatch.ElapsedMilliseconds;
                    cancellationToken.ThrowIfCancellationRequested();
                    long startBuild = stopwatch.ElapsedMilliseconds;
                    string query = this.BuildQuery();
                    this.QueryString = query;
                    buildTime = stopwatch.ElapsedMilliseconds - startBuild;
                    cancellationToken.ThrowIfCancellationRequested();
                    Recordset resultSet = this.ExecuteQuery(cancellationToken, query, out queryTime);
                    evalTime = queryTime;
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        this.FetchAndSanitizeResults(cancellationToken, resultSet, out numFetches, out evalQueryFetchTime, out sanitizeTime);
                        evalTime += evalQueryFetchTime;
                    }
                    finally
                    {
                        try
                        {
                            resultSet.Close();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    if (this.ResultsItemCount == 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        ModelQueryTimeKeyColumn timeKeyColumn = (ModelQueryTimeKeyColumn)this.keys.FirstOrDefault(key => key is ModelQueryTimeKeyColumn);
                        if (timeKeyColumn != null)
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "{0}: Time interval for query: Min = {1}, Max = {2}", this.Name, timeKeyColumn.Min, timeKeyColumn.Max);
                        long buildIndexStart = stopwatch.ElapsedMilliseconds;
                        this.BuildIndexes(cancellationToken);
                        indexBuildTime = stopwatch.ElapsedMilliseconds - buildIndexStart;
                        cancellationToken.ThrowIfCancellationRequested();
                        long createBucketStart = stopwatch.ElapsedMilliseconds;
                        this.CreateBuckets(cancellationToken);
                        bucketizeTime = stopwatch.ElapsedMilliseconds - createBucketStart;
                        cancellationToken.ThrowIfCancellationRequested();
                        long accumStart = stopwatch.ElapsedMilliseconds;
                        this.AccumulateResults(cancellationToken);
                        accumulateTime = stopwatch.ElapsedMilliseconds - accumStart;
                        cancellationToken.ThrowIfCancellationRequested();
                        totalTime = stopwatch.ElapsedMilliseconds;
                        flag = true;
                    }
                }
                finally
                {
                    if (stopwatch.IsRunning)
                        stopwatch.Stop();
                    VisualizationTraceSource.Current.TraceEvent(
                        TraceEventType.Information, 0,
                        "{0}: Query processing stats (msec): {1}: # items {2}, totalTime={3}, build {4}, eval {5} (exec={6}, fetch={7}), sanitize {8}, indexBuild {9}, bucketize {10}, accumulate {11}",
                        this.Name,
                        flag ? "Completed" : "Failed with an exception",
                        this.ResultsItemCount,
                        stopwatch.ElapsedMilliseconds,
                        buildTime,
                        evalTime,
                        queryTime,
                        evalQueryFetchTime,
                        sanitizeTime,
                        indexBuildTime,
                        bucketizeTime,
                        accumulateTime);
                }
            }
        }

        private void Reset()
        {
            this.keys.ForEach(key => key.Reset());
            this.measures.ForEach(measure => measure.Reset());
            this.orderedUnaliasedKeys.Clear();
            this.keyOrderInQueryResults.Clear();
        }

        private bool AllMeasuresAreAggregated()
        {
            if (this.measures.Count == 0)
                return false;
            bool retval = true;
            this.measures.ForEach(m => retval = retval && m.AggregationFunction != AggregationFunction.None);
            return retval;
        }

        private bool IsAliasable(ModelQueryKeyColumn modelQueryKey)
        {
            return modelQueryKey.AliasOf == null;
        }

        private void AliasColumns()
        {
            List<ModelQueryKeyColumn> list = new List<ModelQueryKeyColumn>(this.keys);
            ModelQueryKeyColumn timeKeyColumn = list.FirstOrDefault(key => key is ModelQueryTimeKeyColumn);
            if (timeKeyColumn != null)
            {
                list.Remove(timeKeyColumn);
                list.Insert(0, timeKeyColumn);
            }
            for (int i = 0; i < list.Count; ++i)
            {
                ModelQueryKeyColumn modelQueryKey1 = list[i];
                if (this.IsAliasable(modelQueryKey1))
                {
                    for (int j = i + 1; j < list.Count; ++j)
                    {
                        ModelQueryKeyColumn modelQueryKey2 = list[j];
                        if (this.IsAliasable(modelQueryKey2) && modelQueryKey1.TableColumn.QuerySubstitutable(modelQueryKey2.TableColumn))
                        {
                            modelQueryKey2.AliasOf = modelQueryKey1;
                            modelQueryKey1.SortAscending |= modelQueryKey2.SortAscending;
                            modelQueryKey1.SortDescending |= modelQueryKey2.SortDescending;
                            modelQueryKey1.DiscardNulls |= modelQueryKey2.DiscardNulls;
                            modelQueryKey1.FetchValues |= modelQueryKey2.FetchValues;
                            bool useForBuckets = modelQueryKey2.UseForBuckets;
                        }
                    }
                }
            }
            for (int i = 0; i < this.measures.Count; ++i)
            {
                ModelQueryMeasureColumn queryMeasureColumn1 = this.measures[i];
                if (queryMeasureColumn1.AliasOf == null)
                {
                    for (int j = i + 1; j < this.measures.Count; ++j)
                    {
                        ModelQueryMeasureColumn queryMeasureColumn2 = this.measures[j];
                        if (queryMeasureColumn1.AggregationFunction == queryMeasureColumn2.AggregationFunction && queryMeasureColumn1.TableColumn.QuerySubstitutable(queryMeasureColumn2.TableColumn))
                        {
                            queryMeasureColumn2.AliasOf = queryMeasureColumn1;
                            queryMeasureColumn1.FetchValues |= queryMeasureColumn2.FetchValues;
                        }
                    }
                }
            }
            if (!this.QueryUsesAggregation)
            {
                for (int i = 0; i < this.measures.Count; ++i)
                {
                    ModelQueryMeasureColumn queryMeasureColumn = this.measures[i];
                    if (queryMeasureColumn.AliasOf == null)
                    {
                        for (int j = 0; j < list.Count; ++j)
                        {
                            ModelQueryKeyColumn modelQueryKey = list[j];
                            if (this.IsAliasable(modelQueryKey) && queryMeasureColumn.TableColumn.QuerySubstitutable(modelQueryKey.TableColumn))
                            {
                                queryMeasureColumn.ModelQueryKeyAlias = modelQueryKey;
                                modelQueryKey.ModelQueryMeasureAlias = queryMeasureColumn;
                                modelQueryKey.FetchValues |= queryMeasureColumn.FetchValues;
                            }
                        }
                    }
                }
            }
            this.orderedUnaliasedKeys.AddRange(this.keys.Where(key => key.AliasOf == null).OrderBy(key => key.TableColumn.Table.Rank));
        }

        private string GetFilterForRelatedTables(bool includeMeasures)
        {
            List<TableMetadata> list =
                (this.orderedUnaliasedKeys.Select(keyColumn => keyColumn.TableColumn.Table).
                Union(this.Filter.FilterClauses.Where((fc =>
                {
                    if (fc.AggregationFunction == AggregationFunction.None)
                        return !fc.Unfiltered;
                    return false;
                })).Select(fc => fc.TableMember.Table)).
                Union(includeMeasures ? this.measures.Select(measure => measure.TableColumn.Table) : Enumerable.Empty<TableMetadata>()).
                Distinct().OrderBy(table => table.Rank)).ToList();
            TableMetadata highestRankedTable = list.Last();
            if (list.Count() <= 1)
                return null;
            if (list.All(highestRankedTable.ContainsLookupTable))
                return string.Format("NOT ( ISBLANK ( CALCULATE (COUNTROWS( {0} ) ) ) ) ", highestRankedTable.DAXQueryName());
            if (highestRankedTable.Island == null)
                return null;
            TableMetadata self = highestRankedTable.Island.Tables.OrderBy(table => table.Rank).Except(list).FirstOrDefault(candidateAncestralTable => list.All(candidateAncestralTable.ContainsLookupTable));
            if (self != null)
                return string.Format("NOT ( ISBLANK ( CALCULATE (COUNTROWS( {0} ) ) ) ) ", self.DAXQueryName());
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: No  ancestral table can reach all tables in query, returning defaultFilter={1}", this.Name, null);
            return null;
        }

        private string BuildQuery()
        {
            bool queryUsesAggregation = this.QueryUsesAggregation;
            ModelQueryTimeKeyColumn queryTimeKeyColumn = (ModelQueryTimeKeyColumn)this.keys.FirstOrDefault(k => k is ModelQueryTimeKeyColumn);
            if (queryUsesAggregation && queryTimeKeyColumn != null && queryTimeKeyColumn.TimeChunk != TimeChunkPeriod.None)
                return this.BuildChunkedQueryWithAggregation();
            if (queryUsesAggregation)
                return this.BuildQueryWithAggregation();
            return this.BuildQueryWithoutAggregation();
        }

        private string BuildQueryWithoutAggregation()
        {
            string forRelatedTables = this.GetFilterForRelatedTables(true);
            IEnumerable<FilterClause> filterClauses = this.Filter.FilterClauses;
            List<ModelQueryKeyColumn> list1 = this.orderedUnaliasedKeys;
            this.keyOrderInQueryResults.AddRange(list1);
            IEnumerable<string> filterFieldDAXQueryNames = list1.Where(key => key.DiscardNulls).Select(key => key.TableColumn.ModelQueryName);
            List<string> list2 = this.keys.Where((key =>
            {
                if (key.AliasOf == null)
                    return key.Sort;
                return false;
            })).Select((key =>
            {
                if (!key.SortAscending)
                    return key.TableColumn.ModelQueryName + " DESC";
                return key.TableColumn.ModelQueryName;
            })).ToList();
            IEnumerable<string> collection = this.measures.Where((measure =>
            {
                if (measure.ModelQueryKeyAlias == null)
                    return measure.AliasOf == null;
                return false;
            })).Select(measure => measure.TableColumn.ModelQueryName);
            ModelQueryTimeKeyColumn queryTimeKeyColumn = (ModelQueryTimeKeyColumn)list1.FirstOrDefault(key => key is ModelQueryTimeKeyColumn);
            List<string> list3 = new List<string>(list1.Select(key => key.TableColumn.ModelQueryName));
            list3.AddRange(collection);
            string str1 = list3.Last();
            list3.Reverse();
            list3.RemoveAt(0);
            string str2 = string.Format("TOPN ( {0}, VALUES ( {1} ), {2}, {3} )", int.MaxValue, str1, str1, 1);
            StringBuilder stringBuilder1 = new StringBuilder();
            if (list3.Count > 0)
            {
                string str3 = string.Format("CALCULATETABLE ( {0} )", str2);
                if (forRelatedTables != null)
                    str3 = string.Format("FILTER (KEEPFILTERS ( {0} ), {1} )", str3, forRelatedTables);
                list3.Aggregate(
                    stringBuilder1.Append(str3),
                    (sb, tableFieldDAXQueryName) => sb.Insert(0, " ) ),").Insert(0, tableFieldDAXQueryName).Insert(0, "GENERATE ( KEEPFILTERS ( VALUES ( ").Append(" )"),
                    sb => sb.Insert(0, "KEEPFILTERS ( ").Append(" )"));
            }
            else
                stringBuilder1.AppendFormat("KEEPFILTERS ( {0} )", str2);
            StringBuilder stringBuilder2 = new StringBuilder();
            if (filterFieldDAXQueryNames.Any())
            {
                StringBuilder stringBuilder3 = new StringBuilder();
                filterFieldDAXQueryNames.Skip(1).Aggregate(
                    stringBuilder3.Append("( NOT ( "),
                    ((sb, fieldDAXQueryName) => sb.AppendFormat("ISBLANK ( {0} ) {1}", fieldDAXQueryName, "|| ")),
                    (sb => sb.AppendFormat("ISBLANK ( {0} ) ) )", filterFieldDAXQueryNames.First())));
                stringBuilder2.AppendFormat("FILTER ( {0}, {1} ) ", stringBuilder1, stringBuilder3);
            }
            else
                stringBuilder2.AppendFormat("FILTER ( {0}, TRUE ) ", stringBuilder1);
            if (filterClauses.Count((fc =>
            {
                if (fc.AggregationFunction != AggregationFunction.None)
                    return !fc.Unfiltered;
                return false;
            })) > 0)
            {
                StringBuilder sb = new StringBuilder();
                string str3 = " ";
                stringBuilder2.Insert(0, "FILTER (  KEEPFILTERS (").Append(" ),");
                foreach (FilterClause self in filterClauses.Where((fc =>
                {
                    if (fc.AggregationFunction != AggregationFunction.None)
                        return !fc.Unfiltered;
                    return false;
                })))
                {
                    sb.Clear();
                    sb.Append("(");
                    sb = self.DAXFilterString(sb, null);
                    sb.Append(")");
                    stringBuilder2.AppendFormat("{0}{1}", str3, sb);
                    str3 = " && ";
                }
                stringBuilder2.Append(")");
            }
            if (queryTimeKeyColumn != null)
                this.ExtendQueryWithoutAggregationForTimeChunks(queryTimeKeyColumn.TableColumn.ModelQueryName, queryTimeKeyColumn.TimeChunk, stringBuilder2);
            if (filterClauses.Count((fc =>
            {
                if (fc.AggregationFunction == AggregationFunction.None)
                    return !fc.Unfiltered;
                return false;
            })) == 0)
            {
                stringBuilder2.Insert(0, "EVALUATE ");
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                stringBuilder2.Insert(0, "EVALUATE CALCULATETABLE (");
                foreach (FilterClause self in filterClauses.Where((fc =>
                {
                    if (fc.AggregationFunction == AggregationFunction.None)
                        return !fc.Unfiltered;
                    return false;
                })))
                {
                    sb.Clear();
                    sb.AppendFormat("KEEPFILTERS ( FILTER (KEEPFILTERS ( VALUES ( {0} ) ), ",
                        self.TableMember.TableDAXQueryName());
                    sb = self.DAXFilterString(sb, null);
                    sb.Append("))");
                    stringBuilder2.AppendFormat(", {0}", sb);
                }
                stringBuilder2.Append(")");
            }
            if (list2.Any())
            {
                stringBuilder2.AppendFormat(" Order BY {0}", list2.First());
                IEnumerable<string> source = list2.Skip(1);
                if (source.Any())
                    source.Aggregate(stringBuilder2, (sb, columnDAXQueryName) => sb.AppendFormat(", {0}", columnDAXQueryName));
            }
            return stringBuilder2.ToString();
        }

        private void ExtendQueryWithoutAggregationForTimeChunks(string timeColumnDAXQueryName, TimeChunkPeriod timeChunk, StringBuilder valuesQuery)
        {
            string timeChunkStart;
            string timeChunkEnd;
            if (!this.GetTimeChunkStartEndExpressions(timeColumnDAXQueryName, timeChunk, out timeChunkStart, out timeChunkEnd))
                return;
            string guid = Guid.NewGuid().ToString();
            string str2 = string.Format("\"S{0}\"", guid);
            string str3 = string.Format("\"E{0}\"", guid);
            valuesQuery.Insert(0, "AddColumns(");
            valuesQuery.AppendFormat(", {0}, {1}, {2}, {3})", str2, timeChunkStart, str3, timeChunkEnd);
        }

        private bool GetTimeChunkStartEndExpressions(string timeColumnDAXQueryName, TimeChunkPeriod timeChunk, out string timeChunkStart, out string timeChunkEnd)
        {
            timeChunkStart = null;
            timeChunkEnd = null;
            timeChunkStart = timeColumnDAXQueryName;
            timeChunkEnd = timeColumnDAXQueryName;
            switch (timeChunk)
            {
                case TimeChunkPeriod.None:
                    return false;
                case TimeChunkPeriod.Day:
                    timeChunkStart = string.Format("Date(Year({0}), Month({0}), Day({0}))", timeColumnDAXQueryName);
                    timeChunkEnd = string.Format("Date(Year({0}), Month({0}), Day({0}) + 1)", timeColumnDAXQueryName);
                    break;
                case TimeChunkPeriod.Month:
                    timeChunkStart = string.Format("Date(Year({0}), Month({0}), 1)", timeColumnDAXQueryName);
                    timeChunkEnd = string.Format("Date(Year({0}), Month({0}) + 1, 1)", timeColumnDAXQueryName);
                    break;
                case TimeChunkPeriod.Quarter:
                    timeChunkStart = string.Format("Date(Year({0}), Month({0}) - Mod(Month({0}) - 1, 3), 1)", timeColumnDAXQueryName);
                    timeChunkEnd = string.Format("Date(Year({0}), Month({0}) - Mod(Month({0}) - 1, 3) + 3, 1)", timeColumnDAXQueryName);
                    break;
                case TimeChunkPeriod.Year:
                    timeChunkStart = string.Format("Date(Year({0}), 1, 1)", timeColumnDAXQueryName);
                    timeChunkEnd = string.Format("Date(Year({0}) + 1, 1, 1)", timeColumnDAXQueryName);
                    break;
                default:
                    return false;
            }
            return true;
        }

        private string BuildQueryWithAggregation()
        {
            string forRelatedTables = this.GetFilterForRelatedTables(false);
            IEnumerable<FilterClause> filterClauses = this.Filter.FilterClauses;
            List<ModelQueryKeyColumn> list1 = this.orderedUnaliasedKeys;
            this.keyOrderInQueryResults.AddRange(list1);
            IEnumerable<string> filterFieldDAXQueryNames = list1.Where(key => key.DiscardNulls).Select(key => key.TableColumn.ModelQueryName);
            List<string> list2 = this.keys.Where(key =>
            {
                if (key.AliasOf == null)
                    return key.Sort;
                return false;
            }).Select(key =>
            {
                if (!key.SortAscending)
                    return key.TableColumn.ModelQueryName + " DESC";
                return key.TableColumn.ModelQueryName;
            }).ToList();
            IEnumerable<ModelQueryMeasureColumn> source1 = this.measures.Where(measure => measure.AliasOf == null);
            ModelQueryTimeKeyColumn queryTimeKeyColumn = (ModelQueryTimeKeyColumn)list1.FirstOrDefault(key => key is ModelQueryTimeKeyColumn);
            List<string> list3 = list1.Select(key => key.TableColumn.ModelQueryName).ToList();
            StringBuilder filtersPredicate = new StringBuilder();
            if (filterFieldDAXQueryNames.Any())
                filterFieldDAXQueryNames.Skip(1).Aggregate(filtersPredicate.Append(" ( NOT ( "), (sb, fieldDAXQueryName) => sb.AppendFormat("ISBLANK ( {0} ) {1}", fieldDAXQueryName, "|| "), sb => sb.AppendFormat("ISBLANK ( {0} ) ) )", filterFieldDAXQueryNames.First()));
            else
                filtersPredicate.Append("TRUE");
            StringBuilder stringBuilder = new StringBuilder();
            list3.Reverse();
            string str1 = list3.First();
            list3.RemoveAt(0);
            string str2 = string.Format("TOPN ( {0}, {1}, {2}, {3} )",
                int.MaxValue, string.Format("VALUES ( {0} )", str1), str1, 1);
            if (list3.Count > 0)
            {
                string str3 = string.Format("CALCULATETABLE ( {0} )", str2);
                if (forRelatedTables != null)
                    str3 = string.Format("FILTER (KEEPFILTERS ( {0} ), {1} )", str3, forRelatedTables);
                list3.Aggregate(stringBuilder.Append(str3), (sb, keyFieldDAXQueryName) => sb.Insert(0, " ) ),").Insert(0, keyFieldDAXQueryName).Insert(0, "GENERATE ( KEEPFILTERS ( VALUES ( ").Append(" )"), sb => sb.Insert(0, "FILTER ( KEEPFILTERS ( ").AppendFormat(" ), {0} )", filtersPredicate));
            }
            else
                stringBuilder.AppendFormat("FILTER ( KEEPFILTERS ( {0} ), {1} )", str2, filtersPredicate);
            if (filterClauses.Count(fc =>
            {
                if (fc.AggregationFunction != AggregationFunction.None)
                    return !fc.Unfiltered;
                return false;
            }) > 0)
            {
                StringBuilder sb = new StringBuilder();
                string str3 = " ";
                stringBuilder.Insert(0, "FILTER ( KEEPFILTERS (").Append(" ),");
                foreach (FilterClause self in filterClauses.Where(fc =>
                {
                    if (fc.AggregationFunction != AggregationFunction.None)
                        return !fc.Unfiltered;
                    return false;
                }))
                {
                    sb.Clear();
                    sb.Append("(");
                    sb = self.DAXFilterString(sb, null);
                    sb.Append(")");
                    stringBuilder.AppendFormat("{0}{1}", str3, sb);
                    str3 = " && ";
                }
                stringBuilder.Append(")");
            }
            StringBuilder seed = new StringBuilder();
            int measuresCounter = 1;
            source1.Aggregate(seed.AppendFormat("ADDCOLUMNS ( {0}, ", stringBuilder), (sb, measure) => sb.AppendFormat("\"Measure{0}\", CALCULATE ( {1} ( {2} ) ){3}", (object)measuresCounter++, (object)ExcelModelQuery.DAXAggregationFunction[(int)measure.AggregationFunction], (object)measure.TableColumn.ModelQueryName, (object)", "));
            seed.Remove(seed.Length - ", ".Length, ", ".Length).Append(") ");
            if (filterClauses.Count(fc =>
            {
                if (fc.AggregationFunction == AggregationFunction.None)
                    return !fc.Unfiltered;
                return false;
            }) == 0)
            {
                seed.Insert(0, "EVALUATE ");
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                seed.Insert(0, "EVALUATE CALCULATETABLE (");
                foreach (FilterClause self in filterClauses.Where(fc =>
                {
                    if (fc.AggregationFunction == AggregationFunction.None)
                        return !fc.Unfiltered;
                    return false;
                }))
                {
                    sb.Clear();
                    sb.AppendFormat("KEEPFILTERS ( FILTER (KEEPFILTERS ( VALUES ( {0} ) ), ", self.TableMember.TableDAXQueryName());
                    sb = self.DAXFilterString(sb, null);
                    sb.Append("))");
                    seed.AppendFormat(", {0}", sb);
                }
                seed.Append(")");
            }
            if (list2.Any())
            {
                seed.AppendFormat(" Order BY {0}", list2.First());
                IEnumerable<string> source2 = list2.Skip(1);
                if (source2.Any())
                    source2.Aggregate(seed, (sb, columnDAXQueryName) => sb.AppendFormat(", {0}", columnDAXQueryName));
            }
            return seed.ToString();
        }

        private string BuildChunkedQueryWithAggregation()
        {
            string forRelatedTables = this.GetFilterForRelatedTables(false);
            IEnumerable<FilterClause> filterClauses = this.Filter.FilterClauses;
            List<ModelQueryKeyColumn> list1 = this.orderedUnaliasedKeys;
            string str1 = Guid.NewGuid().ToString();
            string str2 = string.Format("\"S{0}\"", str1);
            string str3 = string.Format("\"E{0}\"", str1);
            string startChunkPeriodColumnDAXQueryName = string.Format("[S{0}]", str1);
            string str4 = string.Format("[E{0}]", str1);
            IEnumerable<ModelQueryMeasureColumn> source1 = this.measures.Where(measure => measure.AliasOf == null);
            ModelQueryTimeKeyColumn modelQueryTimeKey = (ModelQueryTimeKeyColumn)list1.FirstOrDefault(key => key is ModelQueryTimeKeyColumn);
            List<string> list2 = this.keys.Where(key =>
            {
                if (key.AliasOf == null)
                    return key.Sort;
                return false;
            }).Select(key =>
            {
                string str5 = key != modelQueryTimeKey ? key.TableColumn.ModelQueryName : startChunkPeriodColumnDAXQueryName;
                if (!key.SortAscending)
                    return str5 + " DESC";
                return str5;
            }).ToList();
            TableMetadata timeKeyTable = modelQueryTimeKey.TableColumn.Table;
            string modelQueryName = modelQueryTimeKey.TableColumn.ModelQueryName;
            string timeKeyTableDAXQueryName = modelQueryTimeKey.TableColumn.TableDAXQueryName();
            List<ModelQueryKeyColumn> list3 = list1.Where(key => timeKeyTable.ContainsLookupTable(key.TableColumn.Table)).ToList();
            List<ModelQueryKeyColumn> list4 = list1.Except(list3).ToList();
            bool flag = list4.Any();
            list3.Remove(modelQueryTimeKey);
            List<string> list5 = list3.Select(key => key.TableColumn.ModelQueryName).ToList();
            list5.Insert(0, str4);
            list5.Insert(0, startChunkPeriodColumnDAXQueryName);
            list5.AddRange(list4.Select(key => key.TableColumn.ModelQueryName));
            list3.Insert(0, modelQueryTimeKey);
            this.keyOrderInQueryResults.AddRange(list3);
            this.keyOrderInQueryResults.AddRange(list4);
            IEnumerable<string> generateTableFilterFieldDAXQueryNames = list4.Where(key => key.DiscardNulls).Select(key => key.TableColumn.ModelQueryName);
            IEnumerable<string> addColumnsTableFilterFieldDAXQueryNames = list3.Where(key =>
            {
                if (key.DiscardNulls)
                    return timeKeyTable.RefersToTheSameTableAs(key.TableColumn.Table);
                return false;
            }).Select(key => key.TableColumn.ModelQueryName);
            IEnumerable<string> outermostTableFilterFieldDAXQueryNames = list3.Where(key =>
            {
                if (key.DiscardNulls)
                    return !timeKeyTable.RefersToTheSameTableAs(key.TableColumn.Table);
                return false;
            }).Select(key => key.TableColumn.ModelQueryName);
            StringBuilder stringBuilder1 = new StringBuilder();
            if (addColumnsTableFilterFieldDAXQueryNames.Any())
                addColumnsTableFilterFieldDAXQueryNames.Skip(1).Aggregate(stringBuilder1.Append(" ( NOT ( "), (sb, fieldDAXQueryName) => sb.AppendFormat("ISBLANK ( {0} ) {1}", fieldDAXQueryName, "|| "), sb => sb.AppendFormat("ISBLANK ( {0} ) ) )", addColumnsTableFilterFieldDAXQueryNames.First()));
            else
                stringBuilder1.Append("TRUE");
            StringBuilder stringBuilder2 = new StringBuilder();
            if (outermostTableFilterFieldDAXQueryNames.Any())
                outermostTableFilterFieldDAXQueryNames.Skip(1).Aggregate(stringBuilder2.Append(" ( NOT ( "), (sb, fieldDAXQueryName) => sb.AppendFormat("ISBLANK ( {0} ) {1}", fieldDAXQueryName, "|| "), sb => sb.AppendFormat("ISBLANK ( {0} ) ) )", outermostTableFilterFieldDAXQueryNames.First()));
            else
                stringBuilder2.Append("TRUE");
            StringBuilder generateTableFiltersPredicate = new StringBuilder();
            if (generateTableFilterFieldDAXQueryNames.Any())
                generateTableFilterFieldDAXQueryNames.Skip(1).Aggregate(generateTableFiltersPredicate.Append(" ( NOT ( "), (sb, fieldDAXQueryName) => sb.AppendFormat("ISBLANK ( {0} ) {1}", fieldDAXQueryName, "|| "), sb => sb.AppendFormat("ISBLANK ( {0} ) ) )", generateTableFilterFieldDAXQueryNames.First()));
            else
                generateTableFiltersPredicate.Append("TRUE");
            string timeChunkStart = null;
            string timeChunkEnd = null;
            this.GetTimeChunkStartEndExpressions(modelQueryName, modelQueryTimeKey.TimeChunk, out timeChunkStart, out timeChunkEnd);
            string str6 = string.Format("ADDCOLUMNS(FILTER(KEEPFILTERS({0}), {1}), {2}, {3}, {4}, {5})",
                timeKeyTableDAXQueryName, stringBuilder1.ToString(), str2, timeChunkStart, str3, timeChunkEnd);
            string str7;
            if (flag)
            {
                List<string> list6 = list4.Select(key => key.TableColumn.ModelQueryName).ToList();
                StringBuilder stringBuilder3 = new StringBuilder();
                list6.Reverse();
                string str5 = list6.First();
                list6.RemoveAt(0);
                string str8 = string.Format("TOPN ( {0}, {1}, {2}, {3} )",
                    int.MaxValue, string.Format("VALUES ( {0} )", str5), str5, 1);
                string generateAddColumnTablePrefix = string.Format("FILTER ( KEEPFILTERS ( GENERATE ( KEEPFILTERS ( {0} ), ", str6);
                string str9 = string.Format("CALCULATETABLE ( {0} )", str8);
                if (forRelatedTables != null)
                    str9 = string.Format("FILTER (KEEPFILTERS ( {0} ), {1} )", str9, forRelatedTables);
                list6.Aggregate(stringBuilder3.Append(str9), (sb, keyFieldDAXQueryName) => sb.Insert(0, " ) ),").Insert(0, keyFieldDAXQueryName).Insert(0, "GENERATE ( KEEPFILTERS ( VALUES ( ").Append(" )"), sb => sb.Insert(0, generateAddColumnTablePrefix).AppendFormat(" ) ), {0} )", generateTableFiltersPredicate));
                str7 = stringBuilder3.ToString();
            }
            else
                str7 = str6;
            string timeChunkingFilter = string.Format("FILTER({0}, {1} >= {2} && {1} < {3})",
                timeKeyTableDAXQueryName, modelQueryName, startChunkPeriodColumnDAXQueryName, str4);
            if (filterClauses.Count(fc =>
            {
                if (fc.AggregationFunction != AggregationFunction.None)
                    return !fc.Unfiltered;
                return false;
            }) > 0)
            {
                StringBuilder stringBuilder3 = new StringBuilder(str7);
                StringBuilder sb = new StringBuilder();
                string str5 = " ";
                string timeChunkingFilter1 = ", " + timeChunkingFilter;
                stringBuilder3.Insert(0, "FILTER ( KEEPFILTERS (").Append(" ),");
                foreach (FilterClause self in filterClauses.Where(fc =>
                {
                    if (fc.AggregationFunction != AggregationFunction.None)
                        return !fc.Unfiltered;
                    return false;
                }))
                {
                    sb.Clear();
                    sb.Append("(");
                    sb = self.DAXFilterString(sb, timeChunkingFilter1);
                    sb.Append(")");
                    stringBuilder3.AppendFormat("{0}{1}", str5, sb.ToString());
                    str5 = " && ";
                }
                stringBuilder3.Append(")");
                str7 = stringBuilder3.ToString();
            }
            if (filterClauses.Count(fc =>
            {
                if (fc.AggregationFunction == AggregationFunction.None)
                    return !fc.Unfiltered;
                return false;
            }) > 0)
            {
                StringBuilder stringBuilder3 = new StringBuilder(str7);
                StringBuilder sb = new StringBuilder();
                stringBuilder3.Insert(0, "CALCULATETABLE (");
                foreach (FilterClause self in filterClauses.Where(fc =>
                {
                    if (fc.AggregationFunction == AggregationFunction.None)
                        return !fc.Unfiltered;
                    return false;
                }))
                {
                    sb.Clear();
                    sb.AppendFormat("KEEPFILTERS ( FILTER (KEEPFILTERS ( VALUES ( {0} ) ), ", self.TableMember.TableDAXQueryName());
                    sb = self.DAXFilterString(sb, null);
                    sb.Append("))");
                    stringBuilder3.AppendFormat(", {0}", sb.ToString());
                }
                stringBuilder3.Append(")");
                str7 = stringBuilder3.ToString();
            }
            StringBuilder tableKeyColumnsInSummarizeTableQuery = new StringBuilder();
            list5.ForEach(fld => tableKeyColumnsInSummarizeTableQuery.AppendFormat("{0}, ", fld));
            string nonTimeTableChunkFilterFormatString = string.Format("FILTER( GENERATE ( KEEPFILTERS ({{0}}), VALUES( {0} )), {0} >= {1} && {0} < {2})", modelQueryName, startChunkPeriodColumnDAXQueryName, str4);
            StringBuilder seed = new StringBuilder();
            int measuresCounter = 1;
            source1.Aggregate(seed.AppendFormat("FILTER ( KEEPFILTERS ( SUMMARIZE ( {0}, {1} ", str7, tableKeyColumnsInSummarizeTableQuery), (sb, measure) => sb.AppendFormat("\"Measure{0}\", CALCULATE ( {1}( {2} ), {3} ){4}", (object)measuresCounter++, (object)ExcelModelQuery.DAXAggregationFunction[(int)measure.AggregationFunction], (object)measure.TableColumn.ModelQueryName, measure.TableColumn.TableDAXQueryName() == timeKeyTableDAXQueryName ? (object)timeChunkingFilter : (object)string.Format(nonTimeTableChunkFilterFormatString, measure.TableColumn.TableDAXQueryName()), (object)", "));
            seed.Remove(seed.Length - ", ".Length, ", ".Length).AppendFormat(")), {0} ) ", stringBuilder2);
            seed.Insert(0, "EVALUATE ");
            if (list2.Any())
            {
                seed.AppendFormat(" Order BY {0}", list2.First());
                IEnumerable<string> source2 = list2.Skip(1);
                if (source2.Any())
                    source2.Aggregate(seed, (sb, columnDAXQueryName) => sb.AppendFormat(", {0}", columnDAXQueryName));
            }
            return seed.ToString();
        }

        private Recordset ExecuteQuery(CancellationToken cancellationToken, string query, out long queryTime)
        {
            ADODB.Command command = new ADODB.CommandClass();
            command.ActiveConnection = this.ModelConnection;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "{0}: Running DAX query: {1}", (object)this.Name, (object)query);
            ADODB.Recordset recordset = null;
            try
            {
                queryTime = -1L;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                command.CommandText = query;
                command.CommandTimeout = 0;
                try
                {
                    int num = 0;
                    object result;
                    recordset = command.Execute(out result, Type.Missing, 16);
                    int state = recordset.State;
                    while ((state & 14) != 0)
                    {
                        cancellationToken.WaitHandle.WaitOne(100);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "{0}: Cancelling DAX query: {1}", (object)this.Name, (object)query);
                            command.Cancel();
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        try
                        {
                            state = recordset.State;
                            if (num > 0)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "{0}:Running DAX query: obtained RecordSet.State after a previous failure, new state: {1}", (object)this.Name, (object)state);
                                num = 0;
                            }
                        }
                        catch (COMException ex)
                        {
                            if (ex.ErrorCode == -2147467259)
                            {
                                if (++num >= 5)
                                {
                                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "{0}: Errors running DAX query: E_FAIL returned by RecordSet.State: rethrowing; query: {1}", (object)this.Name, (object)query);
                                    throw;
                                }
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "{0}: Errors running DAX query: E_FAIL returned by RecordSet.State: retryCount = {1}, will retry", (object)this.Name, (object)num);
                            }
                            else
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "{0}: Errors running DAX query: Exception (not E_FAIL) calling RecordSet.State: rethrowing; exception: {1}, query: {2}", (object)this.Name, (object)ex, (object)query);
                                throw;
                            }
                        }
                    }
                }
                catch (COMException ex)
                {
                    stopwatch.Stop();
                    queryTime = stopwatch.ElapsedMilliseconds;
                    StringBuilder stringBuilder = new StringBuilder();
                    int num = 1;
                    foreach (ADODB.Error error in this.ModelConnection.Errors)
                        stringBuilder.AppendFormat("{0}: Error source={1}, Number={2:x}, Native error={3:x}, Description={4}; ",
                            num++, error.Source, error.Number, error.NativeError, error.Description);
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "{0}: Errors running DAX query: {1}, exception: {2}, query: {3}", (object)this.Name, (object)stringBuilder.ToString(), (object)ex, (object)query);
                    throw new DataSource.InvalidQueryResultsException("Query evaluation failed", ex)
                    {
                        QueryEvaluationFailed = true
                    };
                }
                queryTime = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                if (recordset.State == 0)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "{0}: Errors running DAX query: RecordSet was closed, bailing; query: {1}", (object)this.Name, (object)query);
                    throw new DataSource.InvalidQueryResultsException("Query evaluation failed: Recordset was closed")
                    {
                        QueryEvaluationFailed = true
                    };
                }
            }
            catch (Exception ex1)
            {
                if (recordset != null)
                {
                    try
                    {
                        recordset.Close();
                    }
                    catch (Exception ex2)
                    {
                    }
                }
                throw;
            }
            return recordset;
        }

        private void FetchAndSanitizeResults(CancellationToken cancellationToken, Recordset resultSet, out int numFetches, out long evalQueryFetchTime, out long sanitizeTime)
        {
            numFetches = 0;
            evalQueryFetchTime = sanitizeTime = 0L;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool flag1 = false;
            bool flag2 = false;
            if (resultSet.EOF)
            {
                evalQueryFetchTime = 0L;
                stopwatch.Reset();
                flag1 = true;
                this.ResultsItemCount = 0;
                flag2 = true;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "{0}: Query returned 0 items (EOF=true)", (object)this.Name);
            }
            List<ModelQueryKeyColumn> list1 = this.keyOrderInQueryResults;
            List<ModelQueryMeasureColumn> list2 = this.measures.Where(measure =>
            {
                if (measure.ModelQueryKeyAlias == null)
                    return measure.AliasOf == null;
                return false;
            }).ToList();
            ModelQueryTimeKeyColumn queryTimeKeyColumn1 = (ModelQueryTimeKeyColumn)list1.FirstOrDefault(key => key is ModelQueryTimeKeyColumn);
            int num1 = 0;
            object[,] objArray = null;
            if (queryTimeKeyColumn1 != null)
            {
                TimeChunkPeriod timeChunk = queryTimeKeyColumn1.TimeChunk;
                int num2;
                bool processAliases;
                if (timeChunk != TimeChunkPeriod.None && !this.QueryUsesAggregation)
                {
                    num2 = resultSet.Fields.Count - 1;
                    num1 = 1;
                    processAliases = false;
                }
                else if (timeChunk == TimeChunkPeriod.None)
                {
                    num2 = list1.IndexOf(queryTimeKeyColumn1);
                    num1 = 1;
                    processAliases = true;
                }
                else
                {
                    num2 = list1.IndexOf(queryTimeKeyColumn1) + 1;
                    num1 = 2;
                    processAliases = false;
                }
                if (queryTimeKeyColumn1.FetchValues)
                {
                    stopwatch.Restart();
                    object[,] values1 = flag1 ? new object[1, 0] : resultSet.GetRows(-1, BookmarkEnum.adBookmarkFirst, num2) as object[,];
                    evalQueryFetchTime += stopwatch.ElapsedMilliseconds;
                    ++numFetches;
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!flag2)
                    {
                        this.ResultsItemCount = values1.Length;
                        flag2 = true;
                    }
                    stopwatch.Restart();
                    this.SanitizeQueryResults(queryTimeKeyColumn1, values1, cancellationToken, processAliases);
                    objArray = null;
                    sanitizeTime += stopwatch.ElapsedMilliseconds;
                    cancellationToken.ThrowIfCancellationRequested();
                    ModelQueryTimeKeyColumn queryTimeKeyColumn2 = queryTimeKeyColumn1;
                    DateTime[] dateTimeArray = (DateTime[])queryTimeKeyColumn1.Values;
                    queryTimeKeyColumn2.EndTime = dateTimeArray;
                    if (timeChunk != TimeChunkPeriod.None)
                    {
                        int num3 = num2 - 1;
                        stopwatch.Restart();
                        object[,] values2 = flag1 ? new object[1, 0] : resultSet.GetRows(-1, BookmarkEnum.adBookmarkFirst, num3) as object[,];
                        evalQueryFetchTime += stopwatch.ElapsedMilliseconds;
                        ++numFetches;
                        cancellationToken.ThrowIfCancellationRequested();
                        stopwatch.Restart();
                        DateTime max = queryTimeKeyColumn1.Max;
                        this.SanitizeQueryResults(queryTimeKeyColumn1, values2, cancellationToken, true);
                        objArray = null;
                        queryTimeKeyColumn1.Max = max;
                        sanitizeTime += stopwatch.ElapsedMilliseconds;
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            int num4 = 0;
            for (int index = 0; index < list1.Count; ++index)
            {
                if (object.ReferenceEquals(list1[index], queryTimeKeyColumn1))
                {
                    num4 += num1;
                }
                else
                {
                    if (list1[index].FetchValues)
                    {
                        stopwatch.Restart();
                        object[,] values = flag1 ? new object[1, 0] : resultSet.GetRows(-1, BookmarkEnum.adBookmarkFirst, num4) as object[,];
                        evalQueryFetchTime += stopwatch.ElapsedMilliseconds;
                        ++numFetches;
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!flag2)
                        {
                            this.ResultsItemCount = values.Length;
                            flag2 = true;
                        }
                        stopwatch.Restart();
                        this.SanitizeQueryResults(list1[index], values, cancellationToken, true);
                        objArray = null;
                        sanitizeTime += stopwatch.ElapsedMilliseconds;
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    ++num4;
                }
            }
            for (int index = 0; index < list2.Count; ++index)
            {
                if (list2[index].FetchValues)
                {
                    stopwatch.Restart();
                    object[,] values = flag1 ? new object[1, 0] : resultSet.GetRows(-1, BookmarkEnum.adBookmarkFirst, num4) as object[,];
                    evalQueryFetchTime += stopwatch.ElapsedMilliseconds;
                    ++numFetches;
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!flag2)
                    {
                        this.ResultsItemCount = values.Length;
                        flag2 = true;
                    }
                    stopwatch.Restart();
                    this.SanitizeQueryResults(list2[index], values, cancellationToken, true);
                    objArray = null;
                    sanitizeTime += stopwatch.ElapsedMilliseconds;
                    cancellationToken.ThrowIfCancellationRequested();
                }
                ++num4;
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "{0}: Query returned {1} items", (object)this.Name, (object)this.ResultsItemCount);
        }

        private void SanitizeQueryResults(ModelQueryColumn modelQueryColumn, object[,] values, CancellationToken cancellationToken, bool processAliases = true)
        {
            values.GetUpperBound(1);
            values.GetLowerBound(1);
            ModelQueryKeyColumn modelQueryKey = modelQueryColumn as ModelQueryKeyColumn;
            ModelQueryMeasureColumn modelQueryMeasure = modelQueryColumn as ModelQueryMeasureColumn;
            object[] objArray = new object[5];
            bool[] convertToType = new bool[5];
            List<ModelQueryColumn> list = new List<ModelQueryColumn>();
            Array.Clear(convertToType, 0, convertToType.Length);
            if (modelQueryMeasure != null)
            {
                if (processAliases)
                    list = this.measures.Where(msr => object.ReferenceEquals(msr.AliasOf, modelQueryMeasure)).Select(msr => (ModelQueryColumn)msr).ToList();
                list.Insert(0, modelQueryColumn);
                convertToType[2] = true;
            }
            else if (modelQueryKey != null)
            {
                if (processAliases)
                {
                    list = this.keys.Where(k => object.ReferenceEquals(k.AliasOf, modelQueryKey)).Select(k => (ModelQueryColumn)k).ToList();
                    list.ForEach(alias => convertToType[(int)((ModelQueryKeyColumn)alias).Type] = true);
                }
                list.Insert(0, modelQueryColumn);
                if (modelQueryColumn is ModelQueryIndexedKeyColumn)
                    convertToType[0] = true;
                else
                    convertToType[(int)((ModelQueryKeyColumn)list[0]).Type] = true;
                ModelQueryMeasureColumn aliasedModelQueryMeasure = modelQueryKey.ModelQueryMeasureAlias;
                if (aliasedModelQueryMeasure != null && processAliases)
                {
                    list.Add(aliasedModelQueryMeasure);
                    list.AddRange(this.measures.Where(msr => object.ReferenceEquals(msr.AliasOf, aliasedModelQueryMeasure)).Select(msr => (ModelQueryColumn)msr));
                    convertToType[2] = true;
                }
            }
            int num = processAliases ? 1 : 0;
            DateTime min1 = DateTime.MaxValue;
            DateTime max1 = DateTime.MinValue;
            double min2 = double.MaxValue;
            double max2 = double.MinValue;
            string str = null;
            for (int index = 0; index < 5; ++index)
            {
                if (convertToType[index])
                {
                    switch (index)
                    {
                        case 0:
                            objArray[index] = values;
                            break;
                        case 1:
                            objArray[index] = this.Sanitize<string>(values, new Func<object, string>(Convert.ToString), null, null, ref str, ref str, false, false);
                            break;
                        case 2:
                            objArray[index] = this.Sanitize<double>(values, new Func<object, double>(Convert.ToDouble), value => Convert.ToDouble(value, this.ModelCulture), double.NaN, ref min2, ref max2, false, false);
                            IEnumerable<double> source = ((double[])objArray[index]).Where(val => !double.IsNaN(val));
                            bool flag = source.Any();
                            max2 = flag ? source.Max() : double.NaN;
                            min2 = flag ? source.Min() : double.NaN;
                            break;
                        case 3:
                            objArray[index] = this.Sanitize<DateTime>(values, new Func<object, DateTime>(Convert.ToDateTime), value => Convert.ToDateTime(value, this.ModelCulture), ModelQuery.UnknownDateTime, ref min1, ref max1, true, true);
                            break;
                        case 4:
                            objArray[index] = this.Sanitize<bool>(values, new Func<object, bool>(Convert.ToBoolean), new Func<object, bool>(Convert.ToBoolean), new bool?());
                            break;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            if (min1 == DateTime.MaxValue && max1 == DateTime.MinValue)
                min1 = max1 = ModelQuery.UnknownDateTime;
            if (min2 == double.MaxValue && max2 == double.MinValue)
                min2 = max2 = double.NaN;
            foreach (ModelQueryColumn modelQueryColumn1 in list)
            {
                ModelQueryKeyColumn modelQueryKeyColumn = modelQueryColumn1 as ModelQueryKeyColumn;
                ModelQueryMeasureColumn queryMeasureColumn = modelQueryColumn1 as ModelQueryMeasureColumn;
                if (modelQueryKeyColumn != null)
                {
                    KeyColumnDataType keyColumnDataType = modelQueryKeyColumn is ModelQueryIndexedKeyColumn ? KeyColumnDataType.Object : modelQueryKeyColumn.Type;
                    switch (keyColumnDataType)
                    {
                        case KeyColumnDataType.Object:
                            modelQueryKeyColumn.Values = values;
                            continue;
                        case KeyColumnDataType.String:
                            modelQueryKeyColumn.Values = objArray[(int)keyColumnDataType];
                            continue;
                        case KeyColumnDataType.Double:
                            modelQueryKeyColumn.Values = objArray[(int)keyColumnDataType];
                            continue;
                        case KeyColumnDataType.DateTime:
                            modelQueryKeyColumn.Values = objArray[(int)keyColumnDataType];
                            if (modelQueryKeyColumn is ModelQueryTimeKeyColumn)
                            {
                                ((ModelQueryTimeKeyColumn)modelQueryKeyColumn).Min = min1;
                                ((ModelQueryTimeKeyColumn)modelQueryKeyColumn).Max = max1;
                                continue;
                            }
                            continue;
                        case KeyColumnDataType.Bool:
                            modelQueryKeyColumn.Values = objArray[(int)keyColumnDataType];
                            continue;
                        default:
                            continue;
                    }
                }
                if (queryMeasureColumn != null)
                {
                    queryMeasureColumn.Values = objArray[2];
                    queryMeasureColumn.Min = min2;
                    queryMeasureColumn.Max = max2;
                }
            }
        }

        private T[] Sanitize<T>(object[,] values, Func<object, T> converter, Func<object, T> converterFromString, T defaultValue, ref T min, ref T max, bool getMin, bool getMax) where T : IComparable<T>
        {
            int length = values.GetUpperBound(1) - values.GetLowerBound(1) + 1;
            T[] objArray = new T[length];
            for (int index = 0; index < length; ++index)
            {
                if (values[0, index] is T)
                {
                    objArray[index] = (T)values[0, index];
                    if (getMin || getMax)
                    {
                        T obj = (T)values[0, index];
                        if (getMin && obj.CompareTo(min) < 0)
                            min = obj;
                        if (getMax && obj.CompareTo(max) > 0)
                            max = obj;
                    }
                }
                else
                {
                    if (values[0, index] != null)
                    {
                        if (!Convert.IsDBNull(values[0, index]))
                        {
                            try
                            {
                                T obj = !(values[0, index] is string) ? converter(values[0, index]) : converterFromString(values[0, index]);
                                objArray[index] = obj;
                                if (getMin && obj.CompareTo(min) < 0)
                                    min = obj;
                                if (getMax)
                                {
                                    if (obj.CompareTo(max) > 0)
                                    {
                                        max = obj;
                                        continue;
                                    }
                                    continue;
                                }
                                continue;
                            }
                            catch (FormatException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: FormatException converting query_result[row={1}, col={2}]='{3}' to {4}, setting value to null", (object)this.Name, (object)index, (object)0, values[0, index], (object)typeof(T));
                                objArray[index] = defaultValue;
                                continue;
                            }
                            catch (InvalidCastException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: InvalidCastException converting query_result[row={1}, col={2}]='{3}' to {4}, setting value to null", (object)this.Name, (object)index, (object)0, values[0, index], (object)typeof(T));
                                objArray[index] = defaultValue;
                                continue;
                            }
                            catch (OverflowException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: OverflowException converting query_result[row={1}, col={2}]='{3}' to {4}, setting value to null", (object)this.Name, (object)index, (object)0, values[0, index], (object)typeof(T));
                                objArray[index] = defaultValue;
                                continue;
                            }
                        }
                    }
                    objArray[index] = defaultValue;
                }
            }
            return objArray;
        }

        private T?[] Sanitize<T>(object[,] values, Func<object, T> converter, Func<object, T> converterFromString, T? defaultValue) where T : struct
        {
            int length = values.GetUpperBound(1) - values.GetLowerBound(1) + 1;
            T?[] nullableArray = new T?[length];
            for (int index = 0; index < length; ++index)
            {
                if (values[0, index] is T)
                {
                    nullableArray[index] = new T?((T)values[0, index]);
                }
                else
                {
                    if (values[0, index] != null)
                    {
                        if (!Convert.IsDBNull(values[0, index]))
                        {
                            try
                            {
                                T obj = !(values[0, index] is string) ? converter(values[0, index]) : converterFromString(values[0, index]);
                                nullableArray[index] = new T?(obj);
                                continue;
                            }
                            catch (FormatException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: FormatException converting query_result[row={1}, col={2}]='{3}' to {4}, setting value to null", (object)this.Name, (object)index, (object)0, values[0, index], (object)typeof(T));
                                nullableArray[index] = defaultValue;
                                continue;
                            }
                            catch (InvalidCastException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: InvalidCastException converting query_result[row={1}, col={2}]='{3}' to {4}, setting value to null", (object)this.Name, (object)index, (object)0, values[0, index], (object)typeof(T));
                                nullableArray[index] = defaultValue;
                                continue;
                            }
                            catch (OverflowException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: OverflowException converting query_result[row={1}, col={2}]='{3}' to {4}, setting value to null", (object)this.Name, (object)index, (object)0, values[0, index], (object)typeof(T));
                                nullableArray[index] = defaultValue;
                                continue;
                            }
                        }
                    }
                    nullableArray[index] = defaultValue;
                }
            }
            return nullableArray;
        }

        private T[] Sanitize<T>(object[] values, Func<object, T> converter, Func<object, T> converterFromString, T defaultValue, ref T min, ref T max, bool getMin, bool getMax) where T : IComparable<T>
        {
            int length = values.GetUpperBound(0) - values.GetLowerBound(0) + 1;
            T[] objArray = new T[length];
            for (int index = 0; index < length; ++index)
            {
                if (values[index] is T)
                {
                    objArray[index] = (T)values[index];
                    if (getMin || getMax)
                    {
                        T obj = (T)values[index];
                        if (getMin && obj.CompareTo(min) < 0)
                            min = obj;
                        if (getMax && obj.CompareTo(max) > 0)
                            max = obj;
                    }
                }
                else
                {
                    if (values[index] != null)
                    {
                        if (!Convert.IsDBNull(values[index]))
                        {
                            try
                            {
                                T obj = !(values[index] is string) ? converter(values[index]) : converterFromString(values[index]);
                                objArray[index] = obj;
                                if (getMin && obj.CompareTo(min) < 0)
                                    min = obj;
                                if (getMax)
                                {
                                    if (obj.CompareTo(max) > 0)
                                    {
                                        max = obj;
                                        continue;
                                    }
                                    continue;
                                }
                                continue;
                            }
                            catch (FormatException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: FormatException converting query_result[row={1}]='{2}' to {3}, setting value to null", (object)this.Name, (object)index, values[index], (object)typeof(T));
                                objArray[index] = defaultValue;
                                continue;
                            }
                            catch (InvalidCastException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: InvalidCastException converting query_result[row={1}]='{2}' to {3}, setting value to null", (object)this.Name, (object)index, values[index], (object)typeof(T));
                                objArray[index] = defaultValue;
                                continue;
                            }
                            catch (OverflowException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0}: OverflowException converting query_result[row={1}]='{2}' to {3}, setting value to null", (object)this.Name, (object)index, values[index], (object)typeof(T));
                                objArray[index] = defaultValue;
                                continue;
                            }
                        }
                    }
                    objArray[index] = defaultValue;
                }
            }
            return objArray;
        }

        private void BuildIndexes(CancellationToken cancellationToken)
        {
            foreach (ModelQueryIndexedKeyColumn key in this.keys.Where(key => key is ModelQueryIndexedKeyColumn).Select(key => key as ModelQueryIndexedKeyColumn))
            {
                switch (key.Type)
                {
                    case KeyColumnDataType.Object:
                        this.BuildIndex<object>(cancellationToken, key);
                        break;
                    case KeyColumnDataType.String:
                        this.BuildIndex<string>(cancellationToken, key);
                        break;
                    case KeyColumnDataType.Double:
                        this.BuildIndex<double>(cancellationToken, key);
                        break;
                    case KeyColumnDataType.DateTime:
                        this.BuildIndex<DateTime>(cancellationToken, key);
                        break;
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private void BuildIndex<T>(CancellationToken cancellationToken, ModelQueryIndexedKeyColumn key)
        {
            IEqualityComparer equalityComparer = null;
            IComparer comparer = null;

            int num1 = 1;

            int length = (int)key.Values.GetUpperBound(1) - key.Values.GetLowerBound(1) + num1;
            for (int index = 0; index < length; ++index)
            {

                object obj5 = key.Values[0, index];
                if (obj5 != null)
                {
                    Type type = obj5.GetType();
                    if (type == typeof(string))
                    {
                        StringComparer stringComparer = StringComparer.Create(this.ModelCulture, true);
                        equalityComparer = stringComparer;
                        comparer = stringComparer;
                        break;
                    }
                    if (type == typeof(long) || type == typeof(int) || type == typeof(short))
                    {
                        equalityComparer = EqualityComparer<long>.Default;
                        comparer = Comparer<long>.Default;
                        break;
                    }
                    if (type == typeof(DateTime))
                    {
                        equalityComparer = EqualityComparer<DateTime>.Default;
                        comparer = Comparer<DateTime>.Default;
                        break;
                    }
                    if (type == typeof(ulong) || type == typeof(uint) || (type == typeof(ushort) || type == typeof(byte)))
                    {
                        equalityComparer = EqualityComparer<ulong>.Default;
                        comparer = Comparer<ulong>.Default;
                        break;
                    }
                    if (type == typeof(double))
                    {
                        equalityComparer = EqualityComparer<double>.Default;
                        comparer = Comparer<double>.Default;
                        break;
                    }
                    if (type == typeof(float))
                    {
                        equalityComparer = EqualityComparer<float>.Default;
                        comparer = Comparer<float>.Default;
                        break;
                    }
                    if (type == typeof(bool))
                    {
                        equalityComparer = EqualityComparer<bool>.Default;
                        comparer = Comparer<bool>.Default;
                        break;
                    }
                    if (type == typeof(Decimal))
                    {
                        equalityComparer = EqualityComparer<Decimal>.Default;
                        comparer = Comparer<Decimal>.Default;
                        break;
                    }
                    break;
                }
            }
            Hashtable hashtable1 = new Hashtable(equalityComparer);
            for (int index1 = 0; index1 < length; ++index1)
            {
                object index2 = key.Values[0, index1];
                hashtable1[index2] = 0;
            }
            cancellationToken.ThrowIfCancellationRequested();
            int count1 = hashtable1.Count;
            object[] values = new object[count1];
            hashtable1.Keys.CopyTo(values, 0);
            Array.Sort(values, comparer);
            cancellationToken.ThrowIfCancellationRequested();
            for (int index = 0; index < count1; ++index)
                hashtable1[values[index]] = index;
            cancellationToken.ThrowIfCancellationRequested();
            int[] numArray1 = new int[length];
            for (int index1 = 0; index1 < length; ++index1)
            {
                int[] numArray2 = numArray1;
                int index2 = index1;

                Hashtable hashtable2 = hashtable1;

                numArray2[index2] = (int)hashtable2[key.Values[0, index1]];
            }
            key.Values = numArray1;
            if (key.PreserveValues)
            {
                if (key.AllPreservedValues == null)
                {
                    key.AllPreservedValues = new ArrayList(values);
                    key.PreservedValuesIndex = new int[count1];
                    for (int index = 0; index < count1; ++index)
                        key.PreservedValuesIndex[index] = index;
                }
                else
                {
                    int count2 = key.AllPreservedValues.Count;
                    key.PreservedValuesIndex = new int[count1];
                    for (int index1 = 0; index1 < count1; ++index1)
                    {
                        object x = values[index1];
                        int index2;
                        for (index2 = 0; index2 < count2; ++index2)
                        {
                            if (equalityComparer.Equals(x, key.AllPreservedValues[index2]))
                            {
                                key.PreservedValuesIndex[index1] = index2;
                                break;
                            }
                        }
                        if (index2 == count2)
                        {
                            key.PreservedValuesIndex[index1] = count2++;
                            key.AllPreservedValues.Add(x);
                        }
                    }
                }
            }
            else
            {
                key.AllPreservedValues = null;
                key.PreservedValuesIndex = null;
            }
            if (key.Type == KeyColumnDataType.Object)
            {
                key.AllValues = values;
            }
            else
            {
                DateTime min1 = DateTime.MaxValue;
                DateTime max1 = DateTime.MinValue;
                double min2 = double.MaxValue;
                double max2 = double.MinValue;
                string str = null;
                cancellationToken.ThrowIfCancellationRequested();
                switch (key.Type)
                {
                    case KeyColumnDataType.String:
                        key.AllValues = this.Sanitize(values, Convert.ToString, null, null, ref str, ref str, false, false);
                        break;
                    case KeyColumnDataType.Double:
                        key.AllValues = this.Sanitize(values, Convert.ToDouble, value => Convert.ToDouble(value, this.ModelCulture), double.NaN, ref min2, ref max2, true, true);
                        break;
                    case KeyColumnDataType.DateTime:
                        key.AllValues = this.Sanitize(values, Convert.ToDateTime, value => Convert.ToDateTime(value, this.ModelCulture), UnknownDateTime, ref min1, ref max1, true, true);
                        break;
                }
            }
        }

        private void CreateBuckets(CancellationToken cancellationToken)
        {
            if (!this.keys[0].UseForBuckets)
                return;
            IEqualityComparer<string> equalityComparer1 = StringComparer.Create(this.ModelCulture, false);
            IEnumerable<ModelQueryKeyColumn> enumerable = this.keys.Where(key => key.UseForBuckets);

            int num1 = (int)this.keys[0].Values.Length;
            foreach (ModelQueryKeyColumn modelQueryKeyColumn in enumerable)
            {

                int num2 = (int)modelQueryKeyColumn.Values.Length;

                int num3 = (int)this.keys[0].Values.Length;
            }
            int length = num1 == 0 ? 0 : 1;
            int num4 = 0;
            for (int index = 0; index < num1; ++index)
            {
                if (index % 1000 == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                bool flag = false;
                foreach (ModelQueryKeyColumn modelQueryKeyColumn in enumerable)
                {
                    if (modelQueryKeyColumn.Type == KeyColumnDataType.Double && !(modelQueryKeyColumn is ModelQueryIndexedKeyColumn))
                    {
                        if (double.IsNaN(modelQueryKeyColumn.Values[num4]))
                        {
                            flag = (bool)!double.IsNaN(modelQueryKeyColumn.Values[index]);
                        }
                        else
                        {
                            if (double.IsNaN(modelQueryKeyColumn.Values[index]))
                            {
                                flag = true;
                            }
                            else
                            {
                                flag = (bool)modelQueryKeyColumn.Values[num4] != modelQueryKeyColumn.Values[index];
                            }
                        }
                    }
                    else if (modelQueryKeyColumn.Type == KeyColumnDataType.String)
                    {
                        IEqualityComparer<string> equalityComparer2 = equalityComparer1;
                        flag = !equalityComparer2.Equals(modelQueryKeyColumn.Values[num4], modelQueryKeyColumn.Values[index]);
                    }
                    else
                    {
                        if (!object.Equals(modelQueryKeyColumn.Values[num4], modelQueryKeyColumn.Values[index]))
                            flag = true;
                    }
                    if (flag)
                    {
                        num4 = index;
                        ++length;
                        break;
                    }
                }
            }
            int[] numArray = new int[length];
            int num5 = 0;
            if (num1 > 0)
                numArray[num5++] = 0;
            int num6 = 0;
            for (int index = 0; index < num1; ++index)
            {
                if (index % 1000 == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                bool flag = false;
                foreach (ModelQueryKeyColumn modelQueryKeyColumn in enumerable)
                {
                    if (modelQueryKeyColumn.Type == KeyColumnDataType.Double && !(modelQueryKeyColumn is ModelQueryIndexedKeyColumn))
                    {
                        if (double.IsNaN(modelQueryKeyColumn.Values[num6]))
                        {
                            flag = (bool)!double.IsNaN(modelQueryKeyColumn.Values[index]);
                        }
                        else
                        {
                            if (double.IsNaN(modelQueryKeyColumn.Values[index]))
                            {
                                flag = true;
                            }
                            else
                            {

                                flag = (bool)modelQueryKeyColumn.Values[num6] != modelQueryKeyColumn.Values[index];
                            }
                        }
                    }
                    else if (modelQueryKeyColumn.Type == KeyColumnDataType.String)
                    {

                        IEqualityComparer<string> equalityComparer2 = equalityComparer1;

                        flag = !(bool)equalityComparer2.Equals(modelQueryKeyColumn.Values[num6], modelQueryKeyColumn.Values[index]);
                    }
                    else
                    {
                        if (!(bool)object.Equals(modelQueryKeyColumn.Values[num6], modelQueryKeyColumn.Values[index]))
                            flag = true;
                    }
                    if (flag)
                    {
                        num6 = index;
                        numArray[num5++] = index;
                        break;
                    }
                }
            }
            foreach (ModelQueryKeyColumn modelQueryKeyColumn in enumerable)
                modelQueryKeyColumn.Buckets = numArray;
        }

        private void AccumulateResults(CancellationToken cancellationToken)
        {
            if (!this.QueryUsesAggregation)
                return;
            foreach (ModelQueryMeasureColumn modelQueryMeasure in this.measures.Where(measure =>
            {
                if (measure.Accumulate != ModelQueryMeasureColumn.AccumulationType.NoAccumulation)
                    return measure.AliasOf == null;
                return false;
            }))
            {
                bool flag1 = false;
                switch (modelQueryMeasure.Accumulate)
                {
                    case ModelQueryMeasureColumn.AccumulationType.AccumulateByBuckets:
                        if (this.keys[0].UseForBuckets)
                        {
                            this.AccumulateResultsByBuckets(cancellationToken, modelQueryMeasure);
                            flag1 = true;
                            break;
                        }
                        break;
                    case ModelQueryMeasureColumn.AccumulationType.AccumulateByBucketsAndIndex:
                        if (this.keys[0].UseForBuckets)
                        {
                            this.AccumulateResultsByBucketsAndIndex(cancellationToken, modelQueryMeasure);
                            flag1 = true;
                            break;
                        }
                        break;
                }
                if (flag1)
                {
                    IEnumerable<double> source = ((double[])modelQueryMeasure.Values).Where(val => !double.IsNaN(val));
                    bool flag2 = source.Any();
                    modelQueryMeasure.Max = flag2 ? source.Max() : double.NaN;
                    modelQueryMeasure.Min = flag2 ? source.Min() : double.NaN;
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private double ResetMeasuresAccumulator(ModelQueryMeasureColumn modelQueryMeasure)
        {
            switch (modelQueryMeasure.AggregationFunction)
            {
                case AggregationFunction.Sum:
                case AggregationFunction.Count:
                    return 0.0;
                case AggregationFunction.Min:
                    return double.MaxValue;
                case AggregationFunction.Max:
                    return double.MinValue;
                default:
                    return double.NaN;
            }
        }

        private void AccumulateResultsByBuckets(CancellationToken cancellationToken, ModelQueryMeasureColumn modelQueryMeasure)
        {
            int num1 = (int)modelQueryMeasure.Values.Length;
            int[] buckets = this.keys[0].Buckets;
            int index1 = 0;
            int num2 = index1 >= buckets.Length ? num1 : buckets[index1];
            double num3 = 0.0;
            bool flag1 = false;
            AggregationFunction aggregationFunction = modelQueryMeasure.AggregationFunction;
            for (int index2 = 0; index2 < num1; ++index2)
            {
                if (index2 % 1000 == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                if (index2 == num2)
                {
                    num3 = this.ResetMeasuresAccumulator(modelQueryMeasure);
                    flag1 = false;
                    ++index1;
                    num2 = index1 >= buckets.Length ? num1 : buckets[index1];
                }

                bool flag2 = (bool)double.IsNaN(modelQueryMeasure.Values[index2]);
                if (!flag2 || flag1)
                {
                    flag1 = true;
                    switch (aggregationFunction)
                    {
                        case AggregationFunction.Sum:
                        case AggregationFunction.Count:


                            modelQueryMeasure.Values[index2] = num3 + (flag2 ? 0.0 : modelQueryMeasure.Values[index2]);
                            continue;
                        case AggregationFunction.Min:


                            if (flag2 || num3 < modelQueryMeasure.Values[index2])
                            {

                                modelQueryMeasure.Values[index2] = num3;
                                continue;
                            }
                            num3 = (double)modelQueryMeasure.Values[index2];
                            continue;
                        case AggregationFunction.Max:

                            if (flag2 || num3 > modelQueryMeasure.Values[index2])
                            {

                                modelQueryMeasure.Values[index2] = num3;
                                continue;
                            }
                            num3 = (double)modelQueryMeasure.Values[index2];
                            continue;
                        default:
                            continue;
                    }
                }
            }
        }

        private void ResetIndexAccumulator(double[] accumulators, AggregationFunction aggFn)
        {
            int length = accumulators.Length;
            switch (aggFn)
            {
                case AggregationFunction.Sum:
                case AggregationFunction.Count:
                    for (int index = 0; index < length; ++index)
                        accumulators[index] = 0.0;
                    break;
                case AggregationFunction.Min:
                    for (int index = 0; index < length; ++index)
                        accumulators[index] = double.MaxValue;
                    break;
                case AggregationFunction.Max:
                    for (int index = 0; index < length; ++index)
                        accumulators[index] = double.MinValue;
                    break;
            }
        }

        private void AccumulateResultsByBucketsAndIndex(CancellationToken cancellationToken, ModelQueryMeasureColumn modelQueryMeasure)
        {
            ModelQueryIndexedKeyColumn modelQueryIndexedKey = modelQueryMeasure.ModelQueryIndexedKey;

            int length = modelQueryIndexedKey.AllValues.Length;
            double[] accumulators = new double[length];
            BitArray bitArray = new BitArray(length);

            int num1 = modelQueryMeasure.Values.Length;
            AggregationFunction aggregationFunction = modelQueryMeasure.AggregationFunction;
            int index1 = 0;
            int[] buckets = this.keys[0].Buckets;
            int num2 = index1 >= buckets.Length ? num1 : buckets[index1];
            for (int index2 = 0; index2 < num1; ++index2)
            {
                if (index2 % 1000 == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                if (index2 == num2)
                {
                    this.ResetIndexAccumulator(accumulators, aggregationFunction);
                    ++index1;
                    num2 = index1 >= buckets.Length ? num1 : buckets[index1];
                    bitArray.SetAll(false);
                }


                bool flag1 = double.IsNaN(modelQueryMeasure.Values[index2]);

                int index3 = modelQueryIndexedKey.Values[index2];
                switch (aggregationFunction)
                {
                    case AggregationFunction.Sum:
                    case AggregationFunction.Count:
                        double[] numArray1 = accumulators;
                        int index4 = index3;

                        double num4 = (double)accumulators[index3] + (flag1 ? 0.0 : modelQueryMeasure.Values[index2]);
                        numArray1[index4] = num4;
                        if (bitArray[index3] || !flag1)
                        {
                            modelQueryMeasure.Values[index2] = accumulators[index3];
                            bitArray[index3] = true;
                            break;
                        }
                        break;
                    case AggregationFunction.Min:


                        if (flag1 || accumulators[index3] < modelQueryMeasure.Values[index2])
                        {
                            if (bitArray[index3] || !flag1)
                            {

                                modelQueryMeasure.Values[index2] = accumulators[index3];
                                bitArray[index3] = true;
                                break;
                            }
                            break;
                        }
                        double[] numArray2 = accumulators;
                        numArray2[index3] = (double)modelQueryMeasure.Values[index2];
                        bitArray[index3] = true;
                        break;
                    case AggregationFunction.Max:

                        if (flag1 || accumulators[index3] > modelQueryMeasure.Values[index2])
                        {
                            if (bitArray[index3] || !flag1)
                            {
                                modelQueryMeasure.Values[index2] = accumulators[index3];
                                bitArray[index3] = true;
                                break;
                            }
                            break;
                        }
                        double[] numArray3 = accumulators;
                        numArray3[index3] = (double)modelQueryMeasure.Values[index2];
                        bitArray[index3] = true;
                        break;
                }
            }
        }
    }
}
