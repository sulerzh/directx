using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public abstract class GeoDataSource : DataSource, IInstanceIdRelationshipProvider
    {
        internal List<int> seriesToColorIndex = new List<int>();

        private List<Tuple<TableField, AggregationFunction>> colorAssignedMeasures =
            new List<Tuple<TableField, AggregationFunction>>();

        public const int InvalidInstanceCount = -1;
        protected const int ElementIdBase = 1;
        private TimeChunkPeriod timeChunkPeriod;
        private bool accumulateResultsOverTime;
        private GeoFieldWellDefinition.PlaybackValueDecayType decay;
        private long queryCount;

        public GeoField Geo { get; private set; }

        public List<Tuple<TableField, AggregationFunction>> Measures { get; private set; }

        public TableField Category { get; private set; }

        public Tuple<TableField, AggregationFunction> Color { get; private set; }

        public TableField Time { get; private set; }

        public Filter Filter { get; private set; }

        public TimeChunkPeriod ChunkBy
        {
            get { return this.timeChunkPeriod; }
            protected set
            {
                if (this.timeChunkPeriod == value)
                    return;
                this.timeChunkPeriod = value;
                this.FieldsChangedSinceLastQuery = true;
            }
        }

        public bool AccumulateResultsOverTime
        {
            get { return this.accumulateResultsOverTime; }
            protected set
            {
                if (this.accumulateResultsOverTime == value)
                    return;
                this.accumulateResultsOverTime = value;
                this.FieldsChangedSinceLastQuery = true;
            }
        }

        public GeoFieldWellDefinition.PlaybackValueDecayType Decay
        {
            get { return this.decay; }
            protected set
            {
                if (this.decay == value)
                    return;
                this.decay = value;
                this.FieldsChangedSinceLastQuery = true;
            }
        }

        public bool HasLatLong
        {
            get
            {
                if (this.Geo != null)
                    return this.Geo.HasLatLongOrXY;
                return false;
            }
        }

        public GeoQueryResults QueryResults { get; protected set; }

        public DateTime? PlayFromTime
        {
            get { return this.EarliestTimeInQueryResults; }
        }

        public DateTime? PlayToTime
        {
            get { return this.LatestTimeInQueryResults; }
        }

        public DateTime? EarliestTimeInQueryResults
        {
            get
            {
                GeoQueryResults queryResults = this.QueryResults;
                if (queryResults != null && queryResults.Time != null)
                    return queryResults.Time.Min;
                return new DateTime?();
            }
        }

        public DateTime? LatestTimeInQueryResults
        {
            get
            {
                GeoQueryResults queryResults = this.QueryResults;
                if (queryResults != null && queryResults.Time != null)
                    return queryResults.Time.Max;
                return new DateTime?();
            }
        }

        public string[] AllCategories
        {
            get
            {
                GeoQueryResults queryResults = this.QueryResults;
                ModelQueryIndexedKeyColumn indexedKeyColumn = queryResults == null
                    ? null
                    : queryResults.Category;
                if (indexedKeyColumn == null)
                    return null;
                return (string[]) indexedKeyColumn.AllValues;
            }
        }

        protected bool MainQueryUpdatePending { get; set; }

        protected bool QueryUsesAggregation { get; set; }

        protected override bool NoQueryResults
        {
            get
            {
                if (this.QueryResults != null)
                    return this.QueryResults.ResultsItemCount == 0;
                return true;
            }
        }

        protected int InstanceCount { get; set; }

        public GeoDataSource(string name)
            : base(name)
        {
            this.Measures = new List<Tuple<TableField, AggregationFunction>>();
            this.ChunkBy = TimeChunkPeriod.None;
            this.AccumulateResultsOverTime = false;
            this.Decay = GeoFieldWellDefinition.PlaybackValueDecayType.None;
            this.InstanceCount = -1;
            this.QueryResults = null;
            this.QueryUsesAggregation = false;
            this.Filter = new Filter();
        }

        public override void SetFieldsFromFieldWellDefinition(FieldWellDefinition wellDefinition, bool forceMainQuery)
        {
            GeoFieldWellDefinition geoWellDefinition = wellDefinition as GeoFieldWellDefinition;
            bool flag1 = false;
            if (geoWellDefinition == null)
                return;
            bool noneAFsInQuery = false;
            bool nonNoneAFsInQuery = false;
            geoWellDefinition.Measures.ForEach(m =>
            {
                noneAFsInQuery = noneAFsInQuery || m.Item2 == AggregationFunction.None;
                nonNoneAFsInQuery = nonNoneAFsInQuery || m.Item2 != AggregationFunction.None;
            });
            if (geoWellDefinition.Color != null)
            {
                noneAFsInQuery = noneAFsInQuery || geoWellDefinition.Color.Item2 == AggregationFunction.None;
                nonNoneAFsInQuery = nonNoneAFsInQuery || geoWellDefinition.Color.Item2 != AggregationFunction.None;
            }
            bool flag2 = noneAFsInQuery && nonNoneAFsInQuery;
            if (flag2)
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0,
                    "{0}: The query will be built without using aggregation functions. Aggregation functions have been set with a measure or the color field. They will be ignored.",
                    (object) this.Name);
            if (this.SetGeo(geoWellDefinition.Geo))
            {
                flag1 = true;
                this.MainQueryUpdatePending = true;
            }
            if (this.SetCategory(geoWellDefinition.Category))
            {
                flag1 = true;
                this.MainQueryUpdatePending = true;
            }
            if (this.SetTime(geoWellDefinition.Time))
            {
                flag1 = true;
                this.MainQueryUpdatePending = true;
            }
            if (this.ChunkBy != geoWellDefinition.ChunkBy)
            {
                flag1 = true;
                this.MainQueryUpdatePending = true;
            }
            this.ChunkBy = geoWellDefinition.ChunkBy;
            if (this.AccumulateResultsOverTime != geoWellDefinition.AccumulateResultsOverTime)
                this.MainQueryUpdatePending = true;
            this.AccumulateResultsOverTime = geoWellDefinition.AccumulateResultsOverTime;
            if (this.Decay != geoWellDefinition.Decay)
                this.MainQueryUpdatePending = true;
            this.Decay = geoWellDefinition.Decay;
            if (geoWellDefinition.Color != null)
            {
                if (this.SetColor(geoWellDefinition.Color.Item1,
                    flag2 ? AggregationFunction.None : geoWellDefinition.Color.Item2))
                    this.MainQueryUpdatePending = true;
            }
            else if (this.SetColor(geoWellDefinition.Color))
                this.MainQueryUpdatePending = true;
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null)
                return;
            List<Tuple<TableField, AggregationFunction>> updatedDataSourceMeasures =
                new List<Tuple<TableField, AggregationFunction>>(measures.Count);
            measures.ForEach(measure =>
            {
                Tuple<TableField, AggregationFunction> tuple =
                    geoWellDefinition.Measures.FirstOrDefault(newMeasure =>
                        TableMember.QuerySubstitutable(measure.Item1, newMeasure.Item1));
                TableField tableField = tuple == null ? null : tuple.Item1;
                updatedDataSourceMeasures.Add(tableField == null
                    ? measure
                    : new Tuple<TableField, AggregationFunction>(tableField, measure.Item2));
            });
            List<Tuple<TableField, AggregationFunction>> newMeasures;
            if (flag2)
            {
                newMeasures = new List<Tuple<TableField, AggregationFunction>>(geoWellDefinition.Measures.Count);
                geoWellDefinition.Measures.ForEach(
                    measure =>
                        newMeasures.Add(new Tuple<TableField, AggregationFunction>(measure.Item1,
                            AggregationFunction.None)));
            }
            else
                newMeasures = geoWellDefinition.Measures;
            if (
                updatedDataSourceMeasures.Except(newMeasures).Any() ||
                newMeasures.Except(updatedDataSourceMeasures).Any())
            {
                this.RemoveAllMeasures();
                newMeasures.ForEach(
                    newMeasure => this.AddMeasure(newMeasure));
                flag1 = flag1 | noneAFsInQuery;
                this.MainQueryUpdatePending = true;
            }
            else
                this.Measures = updatedDataSourceMeasures;
            Filter filter = this.Filter;
            if (filter == null)
            {
                GeoDataSource geoDataSource = this;
                int num = (geoDataSource.MainQueryUpdatePending ? 1 : 0) |
                          (geoWellDefinition.Filter == null
                              ? 0
                              : (!geoWellDefinition.Filter.FilteredEffectSame(filter) ? 1 : 0));
                geoDataSource.MainQueryUpdatePending = num != 0;
            }
            else
            {
                GeoDataSource geoDataSource = this;
                int num = geoDataSource.MainQueryUpdatePending | !filter.FilteredEffectSame(geoWellDefinition.Filter)
                    ? 1
                    : 0;
                geoDataSource.MainQueryUpdatePending = num != 0;
                if (filter.SetFilterClausesFrom(geoWellDefinition.Filter))
                    this.FieldsChangedSinceLastQuery = true;
                if (flag1)
                    filter.ForceUpdate();
            }
            GeoDataSource geoDataSource1 = this;
            int num1 = geoDataSource1.MainQueryUpdatePending | forceMainQuery ? 1 : 0;
            geoDataSource1.MainQueryUpdatePending = num1 != 0;
        }

        public GeoField GetGeoFieldUsedInQuery(int sourceDataVersion)
        {
            this.VerifyQueryResultsAreCurrent(sourceDataVersion);
            return this.Geo;
        }

        public override int GetQueryResultsItemCount(int sourceDataVersion)
        {
            this.VerifyQueryResultsAreCurrent(sourceDataVersion);
            if (this.QueryResults != null)
                return this.QueryResults.ResultsItemCount;
            return 0;
        }

        public bool QueryResultsHaveTimeData(int sourceDataVersion)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (queryResults != null)
                return queryResults.Time != null;
            return false;
        }

        public bool QueryResultsHaveMeasures(int sourceDataVersion)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            List<ModelQueryMeasureColumn> list = queryResults == null
                ? null
                : queryResults.Measures;
            if (list != null)
                return list.Count > 0;
            return false;
        }

        public bool QueryResultsHaveCategory(int sourceDataVersion)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (queryResults != null)
                return queryResults.Category != null;
            return false;
        }

        public int GetMaxInstanceCount(int sourceDataVersion)
        {
            this.VerifyQueryResultsAreCurrent(sourceDataVersion);
            return this.InstanceCount;
        }

        public void GetMinMaxInstanceValues(int sourceDataVersion, out double min, out double max)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            List<ModelQueryMeasureColumn> list = queryResults == null
                ? null
                : queryResults.Measures;
            if (list != null && list.Count > 0)
            {
                max = list.Max(measure => measure.Max);
                double maxVal = max;
                min = double.IsNaN(max)
                    ? double.NaN
                    : list.Min(measure =>
                    {
                        if (!double.IsNaN(measure.Min))
                            return measure.Min;
                        return maxVal;
                    });
            }
            else
            {
                max = double.NaN;
                min = double.NaN;
            }
        }

        public void GetMinMaxLatLong(int sourceDataVersion, out RangeOf<double> out_latRange,
            out RangeOf<double> out_longRange)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (queryResults != null)
            {
                double[] numArray1 = queryResults.Latitude != null
                    ? queryResults.Latitude.Values as double[]
                    : null;
                double[] numArray2 = queryResults.Longitude != null
                    ? queryResults.Longitude.Values as double[]
                    : null;
                out_latRange = numArray1.RangeExcludingNaNs();
                out_longRange = numArray2.RangeExcludingNaNs();
            }
            else
            {
                out_latRange = RangeExtensions.RangeOfNaN;
                out_longRange = RangeExtensions.RangeOfNaN;
            }
        }

        public void GetTimeBounds(int sourceDataVersion, out DateTime? beginTime, out DateTime? endtime)
        {
            this.VerifyQueryResultsAreCurrent(sourceDataVersion);
            beginTime = this.EarliestTimeInQueryResults;
            endtime = this.LatestTimeInQueryResults;
        }

        public override string ModelDataIdForId(InstanceId id, bool anyMeasure, bool anyCategoryValue)
        {
            try
            {
                GeoQueryResults queryResults = this.GetQueryResults(this.DataVersion);
                GeoModelDataId geoModelDataId1 = new GeoModelDataId();
                int rowFromInstanceId = this.GetRowFromInstanceId(id);
                if (rowFromInstanceId < 0)
                    return null;
                if (this.Geo != null)
                {
                    if (this.Geo is LatLongField && !this.Geo.IsUsingXY)
                    {
                        geoModelDataId1.LatitudeColumn = this.Geo.Latitude;
                        geoModelDataId1.Latitude = this.LatitudeForId(id);
                        geoModelDataId1.LongitudeColumn = this.Geo.Longitude;
                        geoModelDataId1.Longitude = this.LongitudeForId(id);
                    }
                    else if (this.Geo is LatLongField && this.Geo.IsUsingXY)
                    {
                        geoModelDataId1.XCoordColumn = this.Geo.Longitude;
                        geoModelDataId1.XCoord = this.LongitudeForId(id);
                        geoModelDataId1.YCoordColumn = this.Geo.Latitude;
                        geoModelDataId1.YCoord = this.LatitudeForId(id);
                    }
                    else if (this.Geo is GeoEntityField)
                    {
                        GeoEntityField geoEntityField = this.Geo as GeoEntityField;
                        for (int index = 0; index < geoEntityField.GeoColumns.Count; ++index)
                        {
                            TableColumn column = geoEntityField.GeoColumns[index];
                            GeoEntityField.GeoEntityLevel geoEntityLevel = geoEntityField.GeoLevel(column);
                            string str = (string) queryResults.GeoFields[index].Values[rowFromInstanceId];
                            switch (geoEntityLevel)
                            {
                                case GeoEntityField.GeoEntityLevel.AddressLine:
                                    geoModelDataId1.AddressLineColumn = column;
                                    geoModelDataId1.AddressLine = str;
                                    break;
                                case GeoEntityField.GeoEntityLevel.Locality:
                                    geoModelDataId1.LocalityColumn = column;
                                    geoModelDataId1.Locality = str;
                                    break;
                                case GeoEntityField.GeoEntityLevel.AdminDistrict2:
                                    geoModelDataId1.AdminDistrict2Column = column;
                                    geoModelDataId1.AdminDistrict2 = str;
                                    break;
                                case GeoEntityField.GeoEntityLevel.AdminDistrict:
                                    geoModelDataId1.AdminDistrictColumn = column;
                                    geoModelDataId1.AdminDistrict = str;
                                    break;
                                case GeoEntityField.GeoEntityLevel.PostalCode:
                                    geoModelDataId1.PostalCodeColumn = column;
                                    geoModelDataId1.PostalCode = str;
                                    break;
                                case GeoEntityField.GeoEntityLevel.Country:
                                    geoModelDataId1.CountryColumn = column;
                                    geoModelDataId1.Country = str;
                                    break;
                            }
                        }
                    }
                    else if (this.Geo is GeoFullAddressField)
                    {
                        GeoFullAddressField fullAddressField = this.Geo as GeoFullAddressField;
                        int index = 0;

                        string str = (string) queryResults.GeoFields[index].Values[rowFromInstanceId];
                        if (fullAddressField.FullAddress != null)
                        {
                            geoModelDataId1.FullAddressColumn = fullAddressField.FullAddress;
                            geoModelDataId1.FullAddress = str;
                        }
                        else if (fullAddressField.OtherLocationDescription != null)
                        {
                            geoModelDataId1.OtherLocationDescriptionColumn = fullAddressField.OtherLocationDescription;
                            geoModelDataId1.OtherLocationDescription = str;
                        }
                    }
                }
                geoModelDataId1.AnyMeasure = anyMeasure;
                geoModelDataId1.AnyCategoryValue = anyCategoryValue;
                if (this.Category != null)
                {
                    geoModelDataId1.CategoryColumn = this.Category as TableColumn;
                    if (!anyCategoryValue)
                        geoModelDataId1.Category = this.CategoryForId(id);
                }
                if (!anyMeasure && this.Measures.Count > 0)
                {
                    Tuple<TableField, AggregationFunction> tuple = this.MeasureForId(id);
                    if (tuple == null)
                        return null;
                    geoModelDataId1.Measure = tuple;
                    if (tuple.Item2 == AggregationFunction.None)
                    {
                        geoModelDataId1.MeasureValue =
                            (double?)
                                this.QueryResults.Measures[this.GetMeasureIndexFromInstanceId(id)].Values[
                                    rowFromInstanceId];
                    }
                }
                return geoModelDataId1.ToString();
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return null;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
            catch (IndexOutOfRangeException ex)
            {
                return null;
            }
            catch (NullReferenceException ex)
            {
                return null;
            }
        }

        public override string ModelDataIdForSeriesIndex(int seriesIndex)
        {
            try
            {
                this.VerifyQueryResultsAreCurrent(this.DataVersion);
                GeoModelDataId geoModelDataId = new GeoModelDataId();
                if (seriesIndex < 0)
                    return null;
                if (this.Category != null)
                {
                    string[] allCategories = this.AllCategories;
                    if (allCategories == null ||
                        seriesIndex >= allCategories.Count())
                        return null;
                    geoModelDataId.CategoryColumn = this.Category as TableColumn;
                    geoModelDataId.Category = allCategories[seriesIndex];
                }
                else
                {
                    if (seriesIndex >= this.Measures.Count)
                        return null;
                    geoModelDataId.Measure = this.Measures[seriesIndex];
                    if (geoModelDataId.Measure.Item2 == AggregationFunction.None)
                        geoModelDataId.MeasureValue = 0.0;
                }
                return geoModelDataId.ToString();
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return null;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
            catch (IndexOutOfRangeException ex)
            {
                return null;
            }
            catch (NullReferenceException ex)
            {
                return null;
            }
        }

        public List<Tuple<AggregationFunction?, string, object>> TableColumnsWithValuesForId(InstanceId id,
            bool showRelatedCategories, DateTime? currentDateTime = null)
        {
            try
            {
                List<Tuple<AggregationFunction?, string, dynamic>> retval =
                    new List<Tuple<AggregationFunction?, string, dynamic>>();
                GeoQueryResults queryResults = this.GetQueryResults(this.DataVersion);
                int row = this.GetRowFromInstanceId(id);
                if (row < 0)
                    return null;
                if (this.HasLatLong)
                {
                    if (this.Geo.GeoColumns[0] == this.Geo.Longitude)
                    {
                        retval.Add(new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(),
                            queryResults.Longitude.TableColumn.Name, this.LongitudeForId(id)));
                        retval.Add(new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(),
                            queryResults.Latitude.TableColumn.Name, this.LatitudeForId(id)));
                    }
                    else
                    {
                        retval.Add(new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(),
                            queryResults.Latitude.TableColumn.Name, this.LatitudeForId(id)));
                        retval.Add(new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(),
                            queryResults.Longitude.TableColumn.Name, this.LongitudeForId(id)));
                    }
                }
                else
                    queryResults.GeoFields.ForEach(col =>
                    {
                        Tuple<AggregationFunction?, string, dynamic> tuple = new Tuple
                            <AggregationFunction?, string, dynamic>(
                            null,
                            col.TableColumn.Name,
                            col.Values[row]);
                        retval.Add(tuple);
                    });
                if (showRelatedCategories)
                {
                    if (this.Category == null)
                    {
                        List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
                        for (int index = 0; index < measures.Count; ++index)
                        {

                            dynamic value = queryResults.Measures[index].Values[row];

                            if (double.IsNaN(value))
                                value = string.Empty;


                            retval.Add(new Tuple<AggregationFunction?, string, dynamic>(
                                queryResults.Measures[index].AggregationFunction,
                                queryResults.Measures[index].TableColumn.Name,
                                value));
                        }
                    }
                }
                else
                {
                    int indexFromInstanceId = this.GetMeasureIndexFromInstanceId(id);
                    if (indexFromInstanceId >= 0 && indexFromInstanceId < this.Measures.Count)
                    {

                        dynamic value = queryResults.Measures[indexFromInstanceId].Values[row];

                        if (double.IsNaN(value))
                            value = string.Empty;

                        retval.Add(new Tuple<AggregationFunction?, string, dynamic>(
                            queryResults.Measures[indexFromInstanceId].AggregationFunction,
                            queryResults.Measures[indexFromInstanceId].TableColumn.Name,
                            value));
                    }
                }
                if (this.Category != null)
                {
                    if (!showRelatedCategories)
                    {
                        retval.Add(new Tuple<AggregationFunction?, string, dynamic>(
                            null,
                            queryResults.Category.TableColumn.Name,
                            queryResults.Category.AllValues[queryResults.Category.Values[row]]));
                    }
                    else
                    {
                        DateTime currentTime = currentDateTime.Value;
                        int indexFromInstanceId = this.GetMeasureIndexFromInstanceId(id);
                        int rowInNextGeoBucket = this.GetFirstRowInNextGeoBucket(row);
                        int firstRowInGeoBucket = this.GetFirstRowInGeoBucket(row);
                        int num1 = -1;
                        for (int i = firstRowInGeoBucket; i < rowInNextGeoBucket; ++i)
                        {
                            if (queryResults.Category.Values[i] > -1)
                            {
                                double? time = indexFromInstanceId >= 0
                                    ? this.GetValueForMeasureAtTime(i, indexFromInstanceId, currentTime)
                                    : double.NaN;
                                if (time.HasValue)
                                {
                                    dynamic value;
                                    if (double.IsNaN(time.Value))
                                    {
                                        value = indexFromInstanceId < 0 ? null : string.Empty;
                                    }
                                    else
                                    {
                                        value = time.Value;
                                    }

                                    retval.Add(new Tuple<AggregationFunction?, string, dynamic>(
                                        null,
                                        queryResults.Category.TableColumn.Name,
                                        queryResults.Category.AllValues[queryResults.Category.Values[i]]));
                                    if (indexFromInstanceId >= 0)
                                    {
                                        retval.Add(new Tuple<AggregationFunction?, string, dynamic>(
                                            queryResults.Measures[indexFromInstanceId].AggregationFunction,
                                            queryResults.Measures[indexFromInstanceId].TableColumn.Name,
                                            value));
                                    }

                                    num1 = (int) queryResults.Category.Values[i];
                                }
                            }
                        }
                    }
                }
                if (this.Time != null)
                {
                    retval.Add(new Tuple<AggregationFunction?, string, dynamic>(
                        null,
                        queryResults.Time.TableColumn.Name,
                        queryResults.Time.Values[row]));
                }
                return retval;
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return null;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
            catch (IndexOutOfRangeException ex)
            {
                return null;
            }
            catch (NullReferenceException ex)
            {
                return null;
            }
        }

        public double? GetValueForIdAtTime(InstanceId id, DateTime currentTime)
        {
            return this.GetValueForMeasureAtTime(this.GetRowFromInstanceId(id), this.GetMeasureIndexFromInstanceId(id),
                currentTime);
        }

        protected double? GetValueForMeasureAtTime(int row, int measureIndex, DateTime currentTime)
        {
            try
            {
                if (row < 0)
                    return new double?();
                if (this.QueryResultsHaveTimeData(this.DataVersion))
                {
                    DateTime? forQueryResultsRow1 = this.GetEndTimeForQueryResultsRow(this.DataVersion, row);
                    if (forQueryResultsRow1.HasValue && forQueryResultsRow1.Value < currentTime)
                        return new double?();
                    DateTime? forQueryResultsRow2 = this.GetStartTimeForQueryResultsRow(this.DataVersion, row);
                    if (!forQueryResultsRow2.HasValue || currentTime < forQueryResultsRow2.Value)
                        return new double?();
                    if (forQueryResultsRow1.HasValue && currentTime == forQueryResultsRow1.Value &&
                        currentTime > forQueryResultsRow2.Value)
                        return new double?();
                }
                if (measureIndex < 0)
                    return double.NaN;
                return (double?) this.QueryResults.Measures[measureIndex].Values[row];
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return new double?();
            }
            catch (IndexOutOfRangeException ex)
            {
                return new double?();
            }
            catch (NullReferenceException ex)
            {
                return new double?();
            }
        }

        public double GetLatitudeForQueryResultsRow(int sourceDataVersion, int row)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (!this.HasLatLong)
                return double.NaN;
            try
            {
                double d = (double) queryResults.Latitude.Values[row];
                if ((double.IsNaN(d) || Math.Abs(d) > 90.0) && (this.Geo == null || !this.Geo.IsUsingXY))
                    return double.NaN;
                return d;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
            catch (NullReferenceException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
        }

        public double GetLongitudeForQueryResultsRow(int sourceDataVersion, int row)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (!this.HasLatLong)
                return double.NaN;
            try
            {
                return (double) queryResults.Longitude.Values[row];
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
            catch (NullReferenceException ex)
            {
                throw new DataSource.InvalidQueryResultsException("Query results stale", ex)
                {
                    QueryResultsStale = true
                };
            }
        }

        public void GetEnumerableForQueryResultsRow(int sourceDataVersion, int row, List<int> colorIndices,
            out IEnumerable<IInstanceParameter> enumerable, out int nextRowInResults)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            int num;
            int firstRow;
            if (this.QueryUsesAggregation || measures == null || measures.Count == 0)
            {
                num = this.GetFirstRowInNextGeoBucket(row);
                firstRow = this.GetFirstRowInGeoBucket(row);
                if (this.Time != null || this.Category != null)
                {
                }
            }
            else
            {
                firstRow = row;
                num = row + 1;
            }
            nextRowInResults = num;
            enumerable =
                new GeoDataSource.QueryResultsDataEnumerable(queryResults, colorIndices, this.seriesToColorIndex,
                    firstRow, nextRowInResults);
        }

        public IEnumerable<InstanceId> GetRelatedIdsOverTime(InstanceId id)
        {
            if (this.Time != null)
                return
                    new GeoDataSource.InstanceIdOverTimeEnumerable(this, this.DataVersion, id);
            return new InstanceId[1]
            {
                id
            };
        }

        public int? GetSeriesIndexForModelDataId(string modelId)
        {
            try
            {
                return this.GetSeriesIndexForModelDataId(this.DataVersion, modelId);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return new int?();
            }
        }

        public int? GetSeriesIndexForModelDataId(int sourceDataVersion, string modelId)
        {
            this.VerifyQueryResultsAreCurrent(sourceDataVersion);
            GeoModelDataId modelDataId = GeoModelDataId.TryParse(modelId, this.Geo, this.Category as TableColumn,
                this.Category == null ? this.Measures : new List<Tuple<TableField, AggregationFunction>>(), true);
            if (modelDataId == null)
                return new int?();
            if (modelDataId.AnyMeasure)
                return new int?();
            if (modelDataId.AnyCategoryValue)
                return new int?();
            if (this.Category != null)
            {
                int num = this.IndexForCategory(modelDataId.Category);
                if (num >= 0)
                    return num;
                return new int?();
            }
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null)
                return new int?();
            if (measures.Count <= 0)
                return new int?();
            int index = measures.FindIndex(measure =>
            {
                if (measure.Item2 == modelDataId.Measure.Item2)
                    return
                        (measure.Item1 as TableMember).QuerySubstitutable(modelDataId.Measure.Item1 as TableMember);
                return false;
            });
            if (index != -1)
                return index;
            return new int?();
        }

        public InstanceId? GetCanonicalInstanceIdForModelDataId(int sourceDataVersion, string modelId)
        {
            return this.GetCanonicalInstanceIdForModelDataId(sourceDataVersion, modelId, false, false);
        }

        public InstanceId?[] GetCanonicalInstanceIdsForAllSeriesForModelDataId(int sourceDataVersion, string modelId,
            bool anyMeasure, bool anyCategoryValue)
        {
            InstanceId? idForModelDataId = this.GetCanonicalInstanceIdForModelDataId(sourceDataVersion, modelId,
                anyMeasure, anyCategoryValue);
            return this.GetCanonicalInstanceIdsForAllSeriesWorker(sourceDataVersion, idForModelDataId);
        }

        public InstanceId?[] GetCanonicalInstanceIdsForAllSeries(int sourceDataVersion, InstanceId? instanceId)
        {
            if (!instanceId.HasValue)
                return null;
            InstanceId? canonicalInstanceId = this.GetCanonicalInstanceId(sourceDataVersion, instanceId.Value);
            return this.GetCanonicalInstanceIdsForAllSeriesWorker(sourceDataVersion, canonicalInstanceId);
        }

        protected InstanceId?[] GetCanonicalInstanceIdsForAllSeriesWorker(int sourceDataVersion, InstanceId? instanceId)
        {
            if (!instanceId.HasValue)
                return null;
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null)
                return null;
            if (this.Category == null && measures.Count <= 1)
            {
                return new InstanceId?[1]
                {
                    instanceId.Value
                };
            }
            int rowFromInstanceId = this.GetRowFromInstanceId(instanceId.Value);
            List<InstanceId?> list = new List<InstanceId?>();
            if (measures.Count > 1)
            {
                for (int measureIndex = 0; measureIndex < measures.Count; ++measureIndex)
                    list.Add(this.GetInstanceIdForRow(rowFromInstanceId, measureIndex));
                return list.ToArray();
            }
            try
            {
                GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
                int firstRowInGeoBucket = this.GetFirstRowInGeoBucket(rowFromInstanceId);
                int rowInNextGeoBucket = this.GetFirstRowInNextGeoBucket(rowFromInstanceId);
                int firstRowValue = (int) queryResults.Category.Values[firstRowInGeoBucket];
                list.Add(this.GetInstanceIdForRow(firstRowInGeoBucket, 0));
                for (int row = firstRowInGeoBucket; row < rowInNextGeoBucket; ++row)
                {
                    if (firstRowValue != queryResults.Category.Values[row])
                    {
                        list.Add(this.GetInstanceIdForRow(row, 0));
                        firstRowValue = (int) queryResults.Category.Values[row];
                    }
                }
                return list.ToArray();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
            catch (IndexOutOfRangeException ex)
            {
                return null;
            }
            catch (NullReferenceException ex)
            {
                return null;
            }
        }

        protected InstanceId? GetCanonicalInstanceIdForModelDataId(
            int sourceDataVersion, string modelId, bool anyMeasure, bool anyCategoryValue)
        {
            int resultsItemCount = GetQueryResultsItemCount(sourceDataVersion);
            GeoQueryResults queryResults = GetQueryResults(sourceDataVersion);
            TableColumn cat = Category as TableColumn;
            List<Tuple<TableField, AggregationFunction>> measures = anyMeasure
                ? new List<Tuple<TableField, AggregationFunction>>(0)
                : this.Measures;
            if (measures == null)
                return new InstanceId?();
            GeoModelDataId modelDataId = GeoModelDataId.TryParse(modelId, this.Geo, cat, measures, false);
            if (modelDataId == null)
                return new InstanceId?();
            if (modelDataId.AnyMeasure != anyMeasure)
                return new InstanceId?();
            if (modelDataId.AnyCategoryValue != anyCategoryValue)
                return new InstanceId?();
            if (anyCategoryValue)
                cat = null;
            int measureIndex = 0;
            if (cat != null && this.IndexForCategory(modelDataId.Category) < 0)
                return new InstanceId?();
            if (measures.Count > 0)
            {
                int index = measures.FindIndex(measure =>
                {
                    if (measure.Item2 == modelDataId.Measure.Item2)
                        return
                            (measure.Item1 as TableMember).QuerySubstitutable(modelDataId.Measure.Item1 as TableMember);
                    return false;
                });
                if (index == -1)
                    return new InstanceId?();
                measureIndex = index;
            }
            InstanceId? result = new InstanceId?();
            try
            {
                if (this.Geo is LatLongField)
                {
                    double? y = this.Geo.IsUsingXY ? modelDataId.YCoord : modelDataId.Latitude;
                    double? x = this.Geo.IsUsingXY ? modelDataId.XCoord : modelDataId.Longitude;
                    if (!x.HasValue || !y.HasValue || (double.IsNaN(x.Value) || double.IsNaN(y.Value)))
                        return new InstanceId?();
                    for (int row = 0; row < resultsItemCount; ++row)
                    {
                        if (Math.Abs(x.Value - (double) queryResults.Longitude.Values[row]) < 1E-09 &&
                            Math.Abs(y.Value - (double) queryResults.Latitude.Values[row]) < 1E-09 &&
                            (cat == null ||
                             string.Compare(modelDataId.Category,
                                 queryResults.Category.AllValues[queryResults.Category.Values[row]], false,
                                 this.ModelCulture) == 0) &&
                            ((this.QueryUsesAggregation || measures.Count == 0 ||
                              (double.IsNaN(modelDataId.MeasureValue.Value) &&
                               double.IsNaN(queryResults.Measures[measureIndex].Values[row]))) ||
                             modelDataId.MeasureValue.Value == queryResults.Measures[measureIndex].Values[row]))
                        {
                            result = this.GetInstanceIdForRow(row, measureIndex);
                            break;
                        }
                    }
                }
                else if (this.Geo is GeoFullAddressField)
                {
                    GeoField geo = this.Geo;
                    string str1 = modelDataId.FullAddress ?? modelDataId.OtherLocationDescription;
                    for (int row = 0; row < resultsItemCount; ++row)
                    {
                        if (string.Compare(str1, queryResults.GeoFields[0].Values[row], false, this.ModelCulture) == 0 &&
                            (cat == null ||
                             string.Compare(modelDataId.Category,
                                 queryResults.Category.AllValues[queryResults.Category.Values[row]], false,
                                 this.ModelCulture) == 0) &&
                            (this.QueryUsesAggregation ||
                             measures.Count == 0 ||
                             (double.IsNaN(modelDataId.MeasureValue.Value) &&
                              double.IsNaN(queryResults.Measures[measureIndex].Values[row])) ||
                             modelDataId.MeasureValue.Value == queryResults.Measures[measureIndex].Values[row]))
                        {
                            result = this.GetInstanceIdForRow(row, measureIndex);
                            break;
                        }
                    }
                }
                else
                {
                    GeoEntityField geoEntityField = this.Geo as GeoEntityField;
                    int count = this.Geo.GeoColumns.Count;
                    string[] strArray = new string[count];
                    int num1 = 0;
                    foreach (TableColumn column in geoEntityField.GeoColumns)
                    {
                        switch (geoEntityField.GeoLevel(column))
                        {
                            case GeoEntityField.GeoEntityLevel.AddressLine:
                                strArray[num1++] = modelDataId.AddressLine;
                                continue;
                            case GeoEntityField.GeoEntityLevel.Locality:
                                strArray[num1++] = modelDataId.Locality;
                                continue;
                            case GeoEntityField.GeoEntityLevel.AdminDistrict2:
                                strArray[num1++] = modelDataId.AdminDistrict2;
                                continue;
                            case GeoEntityField.GeoEntityLevel.AdminDistrict:
                                strArray[num1++] = modelDataId.AdminDistrict;
                                continue;
                            case GeoEntityField.GeoEntityLevel.PostalCode:
                                strArray[num1++] = modelDataId.PostalCode;
                                continue;
                            case GeoEntityField.GeoEntityLevel.Country:
                                strArray[num1++] = modelDataId.Country;
                                continue;
                            default:
                                return new InstanceId?();
                        }
                    }
                    for (int row = 0; row < resultsItemCount; ++row)
                    {
                        int index;
                        for (index = 0; index < count; ++index)
                        {
                            if (
                                string.Compare(strArray[index], queryResults.GeoFields[index].Values[row], false,
                                    this.ModelCulture) != 0)
                                break;
                        }

                        if (index == count &&
                            (cat == null ||
                             string.Compare(modelDataId.Category,
                                 queryResults.Category.AllValues[queryResults.Category.Values[row]], false,
                                 this.ModelCulture) == 0) &&
                            (this.QueryUsesAggregation || measures.Count == 0 ||
                             (double.IsNaN(modelDataId.MeasureValue.Value) &&
                              double.IsNaN(queryResults.Measures[measureIndex].Values[row])) ||
                             modelDataId.MeasureValue.Value == queryResults.Measures[measureIndex].Values[row]))
                        {
                            result = this.GetInstanceIdForRow(row, measureIndex);
                            break;
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return new InstanceId?();
            }
            catch (IndexOutOfRangeException ex)
            {
                return new InstanceId?();
            }
            catch (NullReferenceException ex)
            {
                return new InstanceId?();
            }
            if (result.HasValue)
                result = this.GetCanonicalInstanceId(sourceDataVersion, result.Value);
            return result;
        }

        public InstanceId? GetCanonicalInstanceId(int sourceDataVersion, InstanceId id)
        {
            this.VerifyQueryResultsAreCurrent(sourceDataVersion);
            return this.GetRelatedIdsOverTime(id).First();
        }

        internal int ColorIndexForSeriesIndex(int seriesIndex)
        {
            List<int> list = this.seriesToColorIndex;
            if (list == null || seriesIndex < 0 || seriesIndex >= list.Count)
                return -1;
            return list[seriesIndex];
        }

        internal int IndexForCategory(string category)
        {
            GeoQueryResults queryResults = this.QueryResults;
            if (queryResults == null)
                return -1;
            return queryResults.IndexForCategory(category);
        }

        internal bool IndexForCategoryColor(string category, out int seriesIndex, out int colorIndex)
        {
            GeoQueryResults queryResults = this.QueryResults;
            if (queryResults == null)
            {
                seriesIndex = colorIndex = -1;
                return false;
            }
            seriesIndex = queryResults.IndexForCategory(category);
            if (seriesIndex == -1)
            {
                seriesIndex = colorIndex = -1;
                return false;
            }
            ModelQueryIndexedKeyColumn category1 = queryResults.Category;
            if (category1 == null)
            {
                seriesIndex = colorIndex = -1;
                return false;
            }
            colorIndex = category1.PreservedValuesIndex[seriesIndex];
            return true;
        }

        internal Func<int, int, string> GetGeoValuesAccessor(int sourceDataVersion, out int firstGeoCol)
        {
            firstGeoCol = 0;
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            firstGeoCol = 0;
            return (row, col) =>
            {
                try
                {
                    return (string) queryResults.GeoFields[col].Values[row];
                }
                catch (NullReferenceException ex)
                {
                    throw new OperationCanceledException("Datasource has been shut down", ex);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw new OperationCanceledException("Datasource has been shut down", ex);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new OperationCanceledException("Datasource has been shut down", ex);
                }
            };
        }

        internal Func<int, int> GetGeoRowsAccessor(int sourceDataVersion)
        {
            int numRows = this.GetQueryResultsItemCount(sourceDataVersion);
            this.GetQueryResults(sourceDataVersion);
            return row =>
            {
                try
                {
                    return this.QueryResults.FirstModelQueryKey.GetFirstRowInNextBucket(row, numRows);
                }
                catch (NullReferenceException ex)
                {
                    throw new OperationCanceledException("Datasource has been shut down", ex);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw new OperationCanceledException("Datasource has been shut down", ex);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new OperationCanceledException("Datasource has been shut down", ex);
                }
            };
        }

        internal int GetColorCount(int sourceDataVersion)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (queryResults == null)
                return -1;
            ModelQueryIndexedKeyColumn category = queryResults.Category;
            if (category == null)
            {
                if (queryResults.Measures == null)
                    return 0;
                if (this.seriesToColorIndex.Count != 0)
                    return this.seriesToColorIndex.Max() + 1;
                return 1;
            }
            int[] preservedValuesIndex = category.PreservedValuesIndex;
            if (preservedValuesIndex != null)
                return preservedValuesIndex.Max() + 1;
            return 0;
        }

        internal GeoEntityField.GeoEntityLevel? GetGeoEntityLevel(int sourceDataVersion)
        {
            this.VerifyQueryResultsAreCurrent(sourceDataVersion);
            GeoEntityField geoEntityField = this.Geo as GeoEntityField;
            if (geoEntityField != null && geoEntityField.GeoColumns.Count > 0)
                return geoEntityField.GeoLevel(geoEntityField.GeoColumns[0]);
            return new GeoEntityField.GeoEntityLevel?();
        }

        internal RegionLayerShadingMode? GetRegionShadingMode(int sourceDataVersion,
            RegionLayerShadingMode? selectedShadingMode)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (queryResults == null)
                return new RegionLayerShadingMode?();
            ModelQueryIndexedKeyColumn category = queryResults.Category;
            List<ModelQueryMeasureColumn> measures = queryResults.Measures;
            if (measures == null || measures.Count == 0)
                return RegionLayerShadingMode.FullBleed;
            if (measures.Count == 1 && category == null)
                return RegionLayerShadingMode.Global;
            if (selectedShadingMode.HasValue)
                return selectedShadingMode;
            if (category == null)
                return RegionLayerShadingMode.Local;
            int[] numArray1 = category.Values as int[];
            if (numArray1 == null)
                return new RegionLayerShadingMode?();
            ModelQueryKeyColumn firstModelQueryKey = queryResults.FirstModelQueryKey;
            int[] numArray2 = firstModelQueryKey == null ? null : firstModelQueryKey.Buckets;
            int resultsItemCount = this.GetQueryResultsItemCount(sourceDataVersion);
            if (numArray2 == null)
                return new RegionLayerShadingMode?();
            int length = numArray2.Length;
            if (length == 0 || resultsItemCount == 0)
                return RegionLayerShadingMode.Local;
            int index1 = resultsItemCount - 1;
            int index2 = length;
            while (index2-- > 0)
            {
                int num1 = numArray2[index2];
                int num2 = numArray1[index1];
                while (--index1 >= num1)
                {
                    if (numArray1[index1] != num2)
                        return RegionLayerShadingMode.Local;
                }
            }
            return RegionLayerShadingMode.ShiftGlobal;
        }

        internal int GetGeoBucketCount(int sourceDataVersion)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (queryResults != null)
                return queryResults.FirstModelQueryKey.Buckets.Length;
            return -1;
        }

        internal int GetBucketForInstanceId(int sourceDataVersion, InstanceId id)
        {
            return this.GetQueryResults(sourceDataVersion).GetBucketForRow(this.GetRowFromInstanceId(id));
        }

        internal int GetFirstRowInGeoBucket(int row)
        {
            return this.GetQueryResults(this.DataVersion).GetFirstRowInBucket(row);
        }

        internal int GetFirstRowInNextGeoBucket(int row)
        {
            return this.GetQueryResults(this.DataVersion).GetFirstRowInNextBucket(row);
        }

        internal InstanceId GetInstanceIdForRow(int row, int measureIndex)
        {
            uint numMeasures = (uint) Math.Max(this.Measures.Count, 1);
            return GeoDataSource.GetInstanceIdForRow(row, measureIndex, numMeasures);
        }

        internal static InstanceId GetInstanceIdForRow(int row, int measureIndex, uint numMeasures)
        {
            return new InstanceId((uint) ((ulong) row*numMeasures + (ulong) measureIndex + 1UL));
        }

        internal int GetSeriesIndexForInstanceId(InstanceId id, bool considerPointMarkersAsSeries)
        {
            int indexFromInstanceId = this.GetMeasureIndexFromInstanceId(id);
            if (indexFromInstanceId > 1)
                return indexFromInstanceId;
            if (this.Category == null)
            {
                if (indexFromInstanceId != -1)
                    return indexFromInstanceId;
                return considerPointMarkersAsSeries && this.Measures.Count == 0 ? 0 : -1;
            }
            try
            {
                GeoQueryResults queryResults = this.GetQueryResults(this.DataVersion);
                int rowFromInstanceId = this.GetRowFromInstanceId(id);
                return (int) queryResults.Category.Values[rowFromInstanceId];
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return -1;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return -1;
            }
            catch (IndexOutOfRangeException ex)
            {
                return -1;
            }
            catch (NullReferenceException ex)
            {
                return -1;
            }
        }

        protected internal override void Shutdown()
        {
            if (!this.OkayToShutdown)
                return;
            GeoQueryResults queryResults = this.QueryResults;
            this.QueryResults = null;
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            this.Measures = null;
            if (measures != null)
                measures.Clear();
            this.Geo = null;
            this.Category = null;
            this.Time = null;
            this.Color = null;
            this.Filter = null;
            if (queryResults != null)
                queryResults.Shutdown();
            base.Shutdown();
        }

        protected int GetRowFromInstanceId(InstanceId id)
        {
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null)
                return -1;
            int num = Math.Max(measures.Count, 1);
            if (num == 1)
                return (int) id.ElementId - 1;
            return ((int) id.ElementId - 1)/num;
        }

        protected int GetMeasureIndexFromInstanceId(InstanceId id)
        {
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null || measures.Count == 0)
                return -1;
            if (measures.Count == 1)
                return 0;
            return ((int) id.ElementId - 1)%measures.Count;
        }

        protected GeoQueryResults GetQueryResults(int sourceDataVersion)
        {
            this.VerifyQueryResultsAreCurrent(sourceDataVersion);
            return this.QueryResults;
        }

        protected string CategoryForId(InstanceId id)
        {
            try
            {
                GeoQueryResults queryResults = this.GetQueryResults(this.DataVersion);
                int rowFromInstanceId = this.GetRowFromInstanceId(id);
                if (this.Category == null)
                    return null;

                return (string) queryResults.Category.AllValues[queryResults.Category.Values[rowFromInstanceId]];
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return null;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
            catch (IndexOutOfRangeException ex)
            {
                return null;
            }
            catch (NullReferenceException ex)
            {
                return null;
            }
        }

        protected Tuple<TableField, AggregationFunction> MeasureForId(InstanceId id)
        {
            try
            {
                int indexFromInstanceId = this.GetMeasureIndexFromInstanceId(id);
                List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
                if (indexFromInstanceId < 0 || measures == null || indexFromInstanceId >= measures.Count)
                    return null;
                return measures[indexFromInstanceId];
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return null;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
            catch (IndexOutOfRangeException ex)
            {
                return null;
            }
        }

        protected double? LatitudeForId(InstanceId id)
        {
            int rowFromInstanceId = this.GetRowFromInstanceId(id);
            if (rowFromInstanceId < 0 || rowFromInstanceId >= this.GetQueryResultsItemCount(this.DataVersion))
                return new double?();
            double forQueryResultsRow = this.GetLatitudeForQueryResultsRow(this.DataVersion, rowFromInstanceId);
            if (double.IsNaN(forQueryResultsRow))
                return new double?();
            return forQueryResultsRow;
        }

        protected double? LongitudeForId(InstanceId id)
        {
            int rowFromInstanceId = this.GetRowFromInstanceId(id);
            if (rowFromInstanceId < 0 || rowFromInstanceId >= this.GetQueryResultsItemCount(this.DataVersion))
                return new double?();
            double forQueryResultsRow = this.GetLongitudeForQueryResultsRow(this.DataVersion, rowFromInstanceId);
            if (double.IsNaN(forQueryResultsRow))
                return new double?();
            return forQueryResultsRow;
        }

        protected DateTime? GetStartTimeForQueryResultsRow(int sourceDataVersion, int row)
        {
            GeoQueryResults queryResults = this.GetQueryResults(sourceDataVersion);
            if (!this.QueryResultsHaveTimeData(sourceDataVersion))
                return new DateTime?();
            return queryResults.Time.StartTime[row];
        }

        protected DateTime? GetEndTimeForQueryResultsRow(int sourceDataVersion, int row)
        {
            if (!this.QueryResultsHaveTimeData(sourceDataVersion))
                return new DateTime?();
            return this.GetQueryResults(sourceDataVersion).GetEndTimeForRow(row);
        }

        protected override bool QueryData(CancellationToken cancellationToken, bool shouldRunMainQuery)
        {
            GeoQueryResults geoQueryResults = null;
            bool flag1 = false;
            long num1 = -1L;
            GeoQueryResults queryResults = this.QueryResults;
            VisualizationTraceSource visualizationTraceSource = null;
            GeoField geo = this.Geo;
            List<Tuple<TableField, AggregationFunction>> measuresList = this.Measures;
            TableField categoryColumn = this.Category;
            TableField timeColumn = this.Time;
            Filter filterForMainQuery = this.Filter ?? new Filter();
            string name = this.Name;
            long num2 = Math.Max(1, measuresList == null ? 1 : measuresList.Count);
            bool flag2 = this.MainQueryUpdatePending && shouldRunMainQuery;
            cancellationToken.ThrowIfCancellationRequested();
            if (geo != null && geo.GeoColumns.Count > 0)
            {
                List<Tuple<FilterClause, Filter>> forPropertyQueries = filterForMainQuery.GetFiltersForPropertyQueries();
                Tuple<ModelQuery, GeoQueryResults> tuple = null;
                if (forPropertyQueries.Count == 0)
                {
                    if (flag2)
                        tuple = this.RunMainQuery(geo, categoryColumn, timeColumn, measuresList, filterForMainQuery,
                            this.QueryResults, cancellationToken);
                }
                else
                {
                    Task<Tuple<ModelQuery, GeoQueryResults>> task = null;
                    if (flag2)
                        task =
                            Task.Factory.StartNew<Tuple<ModelQuery, GeoQueryResults>>(
                                () =>
                                    this.RunMainQuery(geo, categoryColumn, timeColumn, measuresList,
                                        filterForMainQuery, this.QueryResults, cancellationToken),
                                cancellationToken);
                    forPropertyQueries.ForEach(
                        filterClauseWithFilter =>
                            Task.Factory.StartNew(
                                () =>
                                    this.RunQueryForFilterProperties(filterClauseWithFilter.Item1, geo,
                                        categoryColumn, timeColumn, filterClauseWithFilter.Item2,
                                        cancellationToken)));
                    try
                    {
                        if (flag2)
                            tuple = task.Result;
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException is OperationCanceledException)
                            throw new OperationCanceledException(ex.InnerException.Message, ex.InnerException);
                        if (ex.InnerException is ThreadAbortException)
                            throw ex.InnerException;
                        if (ex.InnerException is DataSource.InvalidQueryResultsException)
                        {
                            DataSource.InvalidQueryResultsException resultsException =
                                ex.InnerException as DataSource.InvalidQueryResultsException;
                            throw new DataSource.InvalidQueryResultsException(ex.InnerException.Message,
                                ex.InnerException)
                            {
                                QueryEvaluationFailed = resultsException.QueryEvaluationFailed,
                                QueryResultsStale = resultsException.QueryResultsStale
                            };
                        }
                        throw;
                    }
                }
                if (flag2)
                {
                    ModelQuery modelQuery = tuple.Item1;
                    geoQueryResults = tuple.Item2;
                    flag1 = modelQuery.QueryUsesAggregation;
                    num1 = geoQueryResults.ResultsItemCount*num2;
                    if (num1 >= int.MaxValue)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0,
                            "{0}: Instance Count (= {1}) >= int.MaxValue, resetting to {2}", (object) name,
                            (object) num1, (object) -1);
                        num1 = -1L;
                    }
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0,
                        "{0}: Instance Count for layer = {1}, rows * measures = {2}", (object) name, (object) num1,
                        (object) ((long) geoQueryResults.ResultsItemCount*num2));
                    this.seriesToColorIndex.Clear();
                    if (measuresList != null)
                    {
                        int count = this.colorAssignedMeasures.Count;
                        for (int index1 = 0; index1 < measuresList.Count; ++index1)
                        {
                            int index2;
                            for (index2 = 0; index2 < count; ++index2)
                            {
                                if (measuresList[index1].Item2 == this.colorAssignedMeasures[index2].Item2 &&
                                    TableMember.QuerySubstitutable(measuresList[index1].Item1,
                                        this.colorAssignedMeasures[index2].Item1))
                                {
                                    this.seriesToColorIndex.Add(index2);
                                    break;
                                }
                            }
                            if (index2 == count)
                            {
                                this.seriesToColorIndex.Add(count);
                                this.colorAssignedMeasures.Add(measuresList[index1]);
                                ++count;
                            }
                        }
                    }
                    visualizationTraceSource = modelQuery.CreateLogQueryResultsTraceSource(name);
                    modelQuery.Shutdown();
                }
            }
            if (!flag2)
                return false;
            cancellationToken.ThrowIfCancellationRequested();
            if (geoQueryResults == null)
                num1 = -1L;
            bool flag3 = !object.ReferenceEquals(queryResults, geoQueryResults);
            if (flag3)
            {
                ++this.DataVersion;
                if (geoQueryResults != null)
                    geoQueryResults.Log(visualizationTraceSource, cancellationToken, this.DataVersion,
                        (uint) num2);
                if (visualizationTraceSource != null)
                    visualizationTraceSource.Close();
                this.QueryResults = geoQueryResults;
                this.QueryUsesAggregation = flag1;
                this.InstanceCount = (int) num1;
                if (queryResults != null)
                    queryResults.Shutdown();
            }
            this.MainQueryUpdatePending = false;
            return flag3;
        }

        private Tuple<ModelQuery, GeoQueryResults> RunMainQuery(GeoField geo, TableField categoryColumn,
            TableField timeColumn, List<Tuple<TableField, AggregationFunction>> measuresList, Filter filter,
            GeoQueryResults currentQueryResults, CancellationToken cancellationToken)
        {
            ModelQueryMeasureColumn.AccumulationType accumulation;
            string name = string.Format("{0} (map query filterId={1}, querycount={2})", this.Name, filter.Id,
                Interlocked.Increment(ref this.queryCount));
            ModelQuery modelQuery = this.InstantiateModelQuery(name, filter, cancellationToken);
            if (modelQuery == null)
                throw new OperationCanceledException("Datasource has been shut down: modelQuery = null",
                    cancellationToken);
            GeoQueryResults queryResults = new GeoQueryResults()
            {
                HoldTillReplaced =
                    timeColumn != null && this.Decay == GeoFieldWellDefinition.PlaybackValueDecayType.HoldTillReplaced,
                ModelCulture = this.ModelCulture
            };
            if (geo.HasLatLongOrXY)
            {

                queryResults.Latitude = new ModelQueryKeyColumn()
                {
                    TableColumn = geo.Latitude,
                    Type = KeyColumnDataType.Double,
                    SortAscending = true,
                    UseForBuckets = true,
                    DiscardNulls = true,
                    FetchValues = true
                };
                ;
                queryResults.Longitude = new ModelQueryKeyColumn()
                {
                    TableColumn = geo.Longitude,
                    Type = KeyColumnDataType.Double,
                    SortAscending = true,
                    UseForBuckets = true,
                    DiscardNulls = true,
                    FetchValues = true,
                };
                ;
                queryResults.FirstModelQueryKey = queryResults.Latitude;
                modelQuery.AddKey(queryResults.Latitude);
                modelQuery.AddKey(queryResults.Longitude);
            }
            else
            {
                queryResults.GeoFields = new List<ModelQueryKeyColumn>(geo.GeoColumns.Count);
                geo.GeoColumns.ForEach(col =>
                {
                    ModelQueryKeyColumn item = new ModelQueryKeyColumn
                    {
                        TableColumn = col,
                        Type = KeyColumnDataType.String,
                        SortAscending = true,
                        UseForBuckets = true,
                        DiscardNulls = false,
                        FetchValues = true
                    };
                    queryResults.GeoFields.Add(item);
                });

                queryResults.GeoFields[0].DiscardNulls = true;

                queryResults.GeoFields.ForEach(
                    (ModelQueryKeyColumn key) =>
                    {
                        modelQuery.AddKey(key);
                    });

                queryResults.FirstModelQueryKey = queryResults.GeoFields[0];
            }
            if (categoryColumn != null)
            {
                ModelQueryIndexedKeyColumn indexedKeyColumn1 = currentQueryResults == null
                    ? null
                    : currentQueryResults.Category;
                bool flag = indexedKeyColumn1 != null &&
                            indexedKeyColumn1.TableColumn.RefersToTheSameMemberAs(
                                categoryColumn as TableColumn);

                queryResults.Category = new ModelQueryIndexedKeyColumn()
                {
                    TableColumn = categoryColumn as TableColumn,
                    Type = KeyColumnDataType.String,
                    SortAscending = true,
                    UseForBuckets = false,
                    DiscardNulls = true,
                    FetchValues = true,
                    PreserveValues = true,
                    AllPreservedValues = flag ? indexedKeyColumn1.AllPreservedValues : null
                };
                ;
                modelQuery.AddKey(queryResults.Category);
            }
            if (timeColumn != null)
            {
                queryResults.Time = new ModelQueryTimeKeyColumn()
                {
                    TableColumn = timeColumn as TableColumn,
                    Type = KeyColumnDataType.DateTime,
                    SortAscending = true,
                    UseForBuckets = false,
                    DiscardNulls = true,
                    TimeChunk = this.ChunkBy,
                    FetchValues = true
                };
                modelQuery.AddKey(queryResults.Time);
            }
            queryResults.Measures = new List<ModelQueryMeasureColumn>(measuresList == null ? 0 : measuresList.Count);

            ModelQueryIndexedKeyColumn categoryKey = null;
            if (timeColumn == null || !this.AccumulateResultsOverTime)
            {
                accumulation = ModelQueryMeasureColumn.AccumulationType.NoAccumulation;
            }
            else if (categoryColumn != null)
            {
                accumulation = ModelQueryMeasureColumn.AccumulationType.AccumulateByBucketsAndIndex;
                categoryKey = queryResults.Category;
            }
            else
            {
                accumulation = ModelQueryMeasureColumn.AccumulationType.AccumulateByBuckets;
            }
            if (measuresList != null)
            {
                measuresList.ForEach(
                    measure =>
                        queryResults.Measures.Add(
                            new ModelQueryMeasureColumn
                            {
                                TableColumn = measure.Item1 as TableMember,
                                AggregationFunction = measure.Item2,
                                Accumulate = accumulation,
                                ModelQueryIndexedKey = categoryKey,
                                FetchValues = true
                            }));
            }
            queryResults.Measures.ForEach(
                measure => modelQuery.AddMeasure(measure));
            cancellationToken.ThrowIfCancellationRequested();
            modelQuery.QueryData(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            queryResults.ResultsItemCount = modelQuery.ResultsItemCount;
            return new Tuple<ModelQuery, GeoQueryResults>(modelQuery, queryResults);
        }

        private void RunQueryForFilterProperties(FilterClause filterClause, GeoField geo, TableField categoryColumn,
            TableField timeColumn, Filter filter, CancellationToken cancellationToken)
        {
            ModelQuery modelQuery = null;
            string name = string.Format(
                "{0} filterclause (tablecol = {1}, Agg Fn = {2}, filterId={3}, querycount={4})", (object) this.Name,
                (object) filterClause.TableMember.Name, (object) filterClause.AggregationFunction, (object) filter.Id,
                (object) Interlocked.Increment(ref this.queryCount));
            try
            {
                TableMember tableMember = null;
                modelQuery = this.InstantiateModelQuery(name, filter, cancellationToken);
                if (modelQuery == null)
                    throw new OperationCanceledException("Datasource has been shut down: modelQuery = null",
                        cancellationToken);
                List<Tuple<TableField, AggregationFunction>> list = null;
                ModelQueryKeyColumn modelQueryKey1 = null;
                ModelQueryMeasureColumn modelQueryMeasure = null;
                if (filterClause is NumericRangeFilterClause)
                {
                    list = new List<Tuple<TableField, AggregationFunction>>();
                    list.Add(new Tuple<TableField, AggregationFunction>(filterClause.TableMember,
                        filterClause.AggregationFunction));
                }
                else if (filterClause is CategoryFilterClause<double>)
                {
                    ModelQueryKeyColumn modelQueryKeyColumn = new ModelQueryKeyColumn();
                    modelQueryKeyColumn.TableColumn = filterClause.TableMember;
                    modelQueryKeyColumn.Type = KeyColumnDataType.Double;
                    modelQueryKeyColumn.SortAscending = true;
                    modelQueryKeyColumn.SortDescending = false;
                    modelQueryKeyColumn.UseForBuckets = false;
                    modelQueryKeyColumn.DiscardNulls = false;
                    modelQueryKeyColumn.FetchValues = true;
                    modelQueryKey1 = modelQueryKeyColumn;
                    modelQuery.AddKey(modelQueryKey1);
                }
                else if (filterClause is CategoryFilterClause<string>)
                {
                    ModelQueryKeyColumn modelQueryKeyColumn = new ModelQueryKeyColumn();
                    modelQueryKeyColumn.TableColumn = filterClause.TableMember;
                    modelQueryKeyColumn.Type = KeyColumnDataType.String;
                    modelQueryKeyColumn.SortAscending = true;
                    modelQueryKeyColumn.SortDescending = false;
                    modelQueryKeyColumn.UseForBuckets = false;
                    modelQueryKeyColumn.DiscardNulls = false;
                    modelQueryKeyColumn.FetchValues = true;
                    modelQueryKey1 = modelQueryKeyColumn;
                    modelQuery.AddKey(modelQueryKey1);
                }
                else if (filterClause is CategoryFilterClause<DateTime>)
                {
                    ModelQueryKeyColumn modelQueryKeyColumn = new ModelQueryKeyColumn();
                    modelQueryKeyColumn.TableColumn = filterClause.TableMember;
                    modelQueryKeyColumn.Type = KeyColumnDataType.DateTime;
                    modelQueryKeyColumn.SortAscending = true;
                    modelQueryKeyColumn.SortDescending = false;
                    modelQueryKeyColumn.UseForBuckets = false;
                    modelQueryKeyColumn.DiscardNulls = false;
                    modelQueryKeyColumn.FetchValues = true;
                    modelQueryKey1 = modelQueryKeyColumn;
                    modelQuery.AddKey(modelQueryKey1);
                    tableMember = filterClause.TableMember;
                }
                else if (filterClause is CategoryFilterClause<bool>)
                {
                    ModelQueryKeyColumn modelQueryKeyColumn = new ModelQueryKeyColumn();
                    modelQueryKeyColumn.TableColumn = filterClause.TableMember;
                    modelQueryKeyColumn.Type = KeyColumnDataType.Bool;
                    modelQueryKeyColumn.SortAscending = false;
                    modelQueryKeyColumn.SortDescending = true;
                    modelQueryKeyColumn.UseForBuckets = false;
                    modelQueryKeyColumn.DiscardNulls = false;
                    modelQueryKeyColumn.FetchValues = true;
                    modelQueryKey1 = modelQueryKeyColumn;
                    modelQuery.AddKey(modelQueryKey1);
                }
                else
                {
                    if (!(filterClause is AndOrFilterClause))
                        throw new NotImplementedException("Unknown filter clause type" + filterClause.GetType().Name);
                    ModelQueryKeyColumn modelQueryKeyColumn = new ModelQueryKeyColumn();
                    modelQueryKeyColumn.TableColumn = filterClause.TableMember;
                    modelQueryKeyColumn.Type = KeyColumnDataType.DateTime;
                    modelQueryKeyColumn.SortAscending = true;
                    modelQueryKeyColumn.SortDescending = false;
                    modelQueryKeyColumn.UseForBuckets = false;
                    modelQueryKeyColumn.DiscardNulls = true;
                    modelQueryKeyColumn.FetchValues = true;
                    modelQueryKey1 = modelQueryKeyColumn;
                    modelQuery.AddKey(modelQueryKey1);
                }
                if (geo.HasLatLongOrXY)
                {
                    ModelQuery modelQuery1 = modelQuery;
                    ModelQueryKeyColumn modelQueryKeyColumn1 = new ModelQueryKeyColumn();
                    modelQueryKeyColumn1.TableColumn = geo.Latitude;
                    modelQueryKeyColumn1.Type = KeyColumnDataType.Double;
                    modelQueryKeyColumn1.SortAscending = false;
                    modelQueryKeyColumn1.SortDescending = false;
                    modelQueryKeyColumn1.UseForBuckets = false;
                    modelQueryKeyColumn1.DiscardNulls = true;
                    modelQueryKeyColumn1.FetchValues = false;
                    ModelQueryKeyColumn modelQueryKey2 = modelQueryKeyColumn1;
                    modelQuery1.AddKey(modelQueryKey2);
                    ModelQuery modelQuery2 = modelQuery;
                    ModelQueryKeyColumn modelQueryKeyColumn2 = new ModelQueryKeyColumn();
                    modelQueryKeyColumn2.TableColumn = geo.Longitude;
                    modelQueryKeyColumn2.Type = KeyColumnDataType.Double;
                    modelQueryKeyColumn2.SortAscending = false;
                    modelQueryKeyColumn2.SortDescending = false;
                    modelQueryKeyColumn2.UseForBuckets = false;
                    modelQueryKeyColumn2.DiscardNulls = true;
                    modelQueryKeyColumn2.FetchValues = false;
                    ModelQueryKeyColumn modelQueryKey3 = modelQueryKeyColumn2;
                    modelQuery2.AddKey(modelQueryKey3);
                }
                else
                    geo.GeoColumns.ForEach(col =>
                    {
                        ModelQuery modelQuery1 = modelQuery;
                        ModelQueryKeyColumn modelQueryKey = new ModelQueryKeyColumn()
                        {
                            TableColumn = col,
                            Type = KeyColumnDataType.String,
                            SortAscending = false,
                            SortDescending = false,
                            UseForBuckets = false,
                            DiscardNulls = false,
                            FetchValues = false
                        };
                        modelQuery1.AddKey(modelQueryKey);
                    });
                if (categoryColumn != null)
                {
                    ModelQuery modelQuery1 = modelQuery;
                    ModelQueryKeyColumn modelQueryKeyColumn = new ModelQueryKeyColumn();
                    modelQueryKeyColumn.TableColumn = categoryColumn as TableColumn;
                    modelQueryKeyColumn.Type = KeyColumnDataType.String;
                    modelQueryKeyColumn.SortAscending = false;
                    modelQueryKeyColumn.SortDescending = false;
                    modelQueryKeyColumn.UseForBuckets = false;
                    modelQueryKeyColumn.DiscardNulls = true;
                    modelQueryKeyColumn.FetchValues = false;
                    ModelQueryKeyColumn modelQueryKey2 = modelQueryKeyColumn;
                    modelQuery1.AddKey(modelQueryKey2);
                }
                if (timeColumn != null)
                {
                    ModelQuery modelQuery1 = modelQuery;
                    ModelQueryTimeKeyColumn queryTimeKeyColumn1 = new ModelQueryTimeKeyColumn();
                    queryTimeKeyColumn1.TableColumn = timeColumn as TableColumn;
                    queryTimeKeyColumn1.Type = KeyColumnDataType.DateTime;
                    queryTimeKeyColumn1.SortAscending = false;
                    queryTimeKeyColumn1.SortDescending = false;
                    queryTimeKeyColumn1.UseForBuckets = false;
                    queryTimeKeyColumn1.DiscardNulls = true;
                    queryTimeKeyColumn1.TimeChunk = tableMember == null ||
                                                    !tableMember.RefersToTheSameMemberAs(timeColumn as TableMember)
                        ? this.ChunkBy
                        : TimeChunkPeriod.None;
                    queryTimeKeyColumn1.FetchValues = false;
                    ModelQueryTimeKeyColumn queryTimeKeyColumn2 = queryTimeKeyColumn1;
                    modelQuery1.AddKey(queryTimeKeyColumn2);
                }
                if (list != null)
                {
                    ModelQueryMeasureColumn queryMeasureColumn = new ModelQueryMeasureColumn();
                    queryMeasureColumn.TableColumn = list[0].Item1 as TableMember;
                    queryMeasureColumn.AggregationFunction = list[0].Item2;
                    queryMeasureColumn.Accumulate = ModelQueryMeasureColumn.AccumulationType.NoAccumulation;
                    queryMeasureColumn.ModelQueryIndexedKey = null;
                    queryMeasureColumn.FetchValues = true;
                    modelQueryMeasure = queryMeasureColumn;
                    modelQuery.AddMeasure(modelQueryMeasure);
                }
                cancellationToken.ThrowIfCancellationRequested();
                modelQuery.QueryData(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0,
                    "{0}: Calling UpdateProperties() for filterclause", (object) name);
                if (modelQueryKey1 != null)
                    filterClause.UpdateProperties(filter.MajorVersion, modelQueryKey1);
                else
                    filterClause.UpdateProperties(filter.MajorVersion, modelQueryMeasure);
            }
            catch (OperationCanceledException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0,
                    "GeoDataSource {0}: User cancelled filter properties query evaluation (caught OperationCanceledException)",
                    (object) name);
            }
            catch (ThreadAbortException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0,
                    "GeoDataSource {0}: Caught ThreadAbortException", (object) name);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                VisualizationTraceSource.Current.Fail(
                    "QueryEngine caught and ignored InvalidQueryResultsException exception in query evaluation for " +
                    name, ex);
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0,
                    "GeoDataSource {0}: InvokeActionOnDispatcher() caught InvalidQueryResultsException, QueryResultsFailed = {1}, QueryResultsFailed = {2}, innerException={3}",
                    (object) name, ex.QueryEvaluationFailed,
                    ex.QueryResultsStale, (object) ex.InnerException);
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(
                    "QueryEngine caught and ignored exception in query evaluation for " + name, ex);
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0,
                    "GeoDataSource {0}: InvokeActionOnDispatcher() caught and ignored exception {1}", (object) name,
                    (object) ex);
            }
            finally
            {
                try
                {
                    if (modelQuery != null)
                        modelQuery.Shutdown();
                }
                catch (Exception ex)
                {
                }
            }
        }

        protected bool SetGeo(GeoField geoField)
        {
            bool flag;
            if (geoField != null)
            {
                this.ValidateGeoField(geoField);
                flag = !geoField.QuerySubstitutable(this.Geo);
            }
            else
                flag = this.Geo != null;
            this.Geo = geoField;
            if (flag)
                this.FieldsChangedSinceLastQuery = true;
            return flag;
        }

        protected bool SetCategory(TableField categoryField)
        {
            if (categoryField != null)
                this.ValidateCategoryField(categoryField);
            bool flag = !TableMember.QuerySubstitutable(categoryField, this.Category);
            this.Category = categoryField;
            if (flag)
                this.FieldsChangedSinceLastQuery = true;
            return flag;
        }

        protected bool SetTime(TableField timeField)
        {
            if (timeField != null)
                this.ValidateTimeField(timeField);
            bool flag = !TableMember.QuerySubstitutable(timeField, this.Time);
            this.Time = timeField;
            if (flag)
                this.FieldsChangedSinceLastQuery = true;
            return flag;
        }

        protected bool SetColor(TableField colorField, AggregationFunction aggregationFunction)
        {
            return
                this.SetColor(colorField == null
                    ? null
                    : new Tuple<TableField, AggregationFunction>(colorField, aggregationFunction));
        }

        protected bool SetColor(Tuple<TableField, AggregationFunction> color)
        {
            bool flag;
            if (color != null)
            {
                this.ValidateColorField(color.Item1);
                flag = this.Color == null || color.Item2 != this.Color.Item2 ||
                       !(color.Item1 as TableColumn).QuerySubstitutable(this.Color.Item1 as TableColumn);
            }
            else
                flag = this.Color != null;
            this.Color = color;
            if (flag)
                this.FieldsChangedSinceLastQuery = true;
            return flag;
        }

        protected bool AddMeasure(TableField measureField, AggregationFunction aggregationFunction)
        {
            return
                this.AddMeasure(measureField == null
                    ? null
                    : new Tuple<TableField, AggregationFunction>(measureField, aggregationFunction));
        }

        protected bool AddMeasure(Tuple<TableField, AggregationFunction> measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");
            this.ValidateMeasureField(measure.Item1);
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null || measures.Contains(measure))
                return false;
            measures.Add(measure);
            this.FieldsChangedSinceLastQuery = true;
            return true;
        }

        protected bool RemoveAllMeasures()
        {
            List<Tuple<TableField, AggregationFunction>> measures = this.Measures;
            if (measures == null)
                return false;
            bool flag =
                measures.Any();
            measures.RemoveAll(measure => true);
            if (flag)
                this.FieldsChangedSinceLastQuery = true;
            return flag;
        }

        protected virtual void ValidateGeoField(GeoField field)
        {
        }

        protected virtual void ValidateMeasureField(TableField field)
        {
            if (!(field is TableMember))
                throw new ArgumentException("field must be a TableMember");
        }

        protected virtual void ValidateCategoryField(TableField field)
        {
            if (!(field is TableColumn))
                throw new ArgumentException("field must be a TableColumn");
        }

        protected virtual void ValidateColorField(TableField field)
        {
            if (!(field is TableColumn))
                throw new ArgumentException("field must be a TableColumn");
        }

        protected virtual void ValidateTimeField(TableField field)
        {
            if (!(field is TableColumn))
                throw new ArgumentException("field must be a TableColumn");
        }

        protected abstract ModelQuery InstantiateModelQuery(string name, Filter filter,
            CancellationToken cancellationToken);

        public class InstanceIdOverTimeEnumerable : IEnumerable<InstanceId>, IEnumerable
        {
            private GeoDataSource dataSource;
            private int sourceVersion;
            private InstanceId id;

            public InstanceIdOverTimeEnumerable(GeoDataSource dataSource, int sourceVersion, InstanceId id)
            {
                this.dataSource = dataSource;
                this.sourceVersion = sourceVersion;
                this.id = id;
            }

            public IEnumerator<InstanceId> GetEnumerator()
            {
                return
                    new GeoDataSource.InstanceIdOverTimeEnumerator(this.dataSource, this.sourceVersion, this.id);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        public class InstanceIdOverTimeEnumerator : IEnumerator<InstanceId>, IDisposable, IEnumerator
        {
            private GeoDataSource dataSource;
            private int sourceVersion;
            private InstanceId id;
            private bool disposed;
            private bool initialized;
            private int current;
            private int last;
            private int measureIndex;
            private int category;
            private bool queryResultsStale;
            private bool dataSourceHasMeasures;

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public InstanceId Current
            {
                get
                {
                    if (this.disposed)
                        throw new ObjectDisposedException("InstanceIdOverTimeEnumerator");
                    try
                    {
                        this.dataSource.VerifyQueryResultsAreCurrent(this.sourceVersion);
                    }
                    catch (DataSource.InvalidQueryResultsException ex)
                    {
                        this.queryResultsStale = true;
                    }
                    if (!this.initialized || this.queryResultsStale)
                        return new InstanceId(134217727U);
                    return this.dataSource.GetInstanceIdForRow(this.current, this.measureIndex);
                }
            }

            public InstanceIdOverTimeEnumerator(GeoDataSource dataSource, int sourceVersion, InstanceId id)
            {
                this.dataSource = dataSource;
                this.sourceVersion = sourceVersion;
                this.id = id;
                this.disposed = false;
                this.initialized = false;
                this.current = -1;
                this.last = -1;
                this.measureIndex = -1;
                this.category = -1;
                this.queryResultsStale = false;
                List<Tuple<TableField, AggregationFunction>> measures = dataSource.Measures;
                this.dataSourceHasMeasures = measures != null && measures.Count > 0;
            }

            public bool MoveNext()
            {
                if (this.disposed)
                    throw new ObjectDisposedException("InstanceIdOverTimeEnumerator");
                if (this.queryResultsStale)
                    return false;
                try
                {
                    if (!this.initialized)
                    {
                        int rowFromInstanceId = this.dataSource.GetRowFromInstanceId(this.id);
                        if (rowFromInstanceId < 0)
                            return false;
                        int firstRowInGeoBucket = this.dataSource.GetFirstRowInGeoBucket(rowFromInstanceId);
                        int firstRowInNextGeoBucket = this.dataSource.GetFirstRowInNextGeoBucket(rowFromInstanceId) - 1;
                        if (this.dataSource.Category != null)
                        {
                            this.category = (int) this.dataSource.QueryResults.Category.Values[rowFromInstanceId];
                            firstRowInGeoBucket = this.FindRowWithCategory(firstRowInGeoBucket, firstRowInNextGeoBucket,
                                this.category);
                            firstRowInNextGeoBucket = this.FindRowWithCategory(firstRowInNextGeoBucket,
                                firstRowInGeoBucket, this.category);
                        }
                        this.current = firstRowInGeoBucket;
                        this.last = firstRowInNextGeoBucket;
                        this.measureIndex = this.dataSourceHasMeasures
                            ? this.dataSource.GetMeasureIndexFromInstanceId(this.id)
                            : 0;
                        this.initialized = true;
                        return this.current <= this.last;
                    }
                    if (this.current == this.last)
                        return false;
                    if (this.dataSource.Category != null)
                        this.current = this.FindRowWithCategory(this.current + 1, this.last, this.category);
                    else
                        ++this.current;
                    return this.current <= this.last;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    this.queryResultsStale = true;
                    return false;
                }
                catch (IndexOutOfRangeException ex)
                {
                    this.queryResultsStale = true;
                    return false;
                }
                catch (ArgumentNullException ex)
                {
                    this.queryResultsStale = true;
                    return false;
                }
                catch (DataSource.InvalidQueryResultsException ex)
                {
                    this.queryResultsStale = true;
                    return false;
                }
            }

            public void Reset()
            {
                if (this.disposed)
                    throw new ObjectDisposedException("InstanceIdOverTimeEnumerator");
                this.initialized = false;
            }

            public void Dispose()
            {
                this.disposed = true;
            }

            private int FindRowWithCategory(int from, int to, int category)
            {
                bool flag = from <= to;

                do
                {
                    if (this.category != this.dataSource.QueryResults.Category.Values[from])
                    {
                        if (flag)
                        {
                            ++from;
                            if (from > to)
                            {
                                return from;
                            }
                        }
                        else
                        {
                            --from;
                            if (from < to)
                            {
                                return from;
                            }
                        }
                    }
                    else
                    {
                        return from;
                    }
                } while (true);
            }
        }

        public class QueryResultsDataEnumerable : IEnumerable<IInstanceParameter>, IEnumerable
        {
            private GeoQueryResults queryResults;
            private List<int> colorIndices;
            private List<int> seriesToColorIndex;
            private int firstRow;
            private int lastRow;

            public QueryResultsDataEnumerable(GeoQueryResults queryResults, List<int> colorIndices,
                List<int> seriesToColorIndex, int firstRow, int lastRow)
            {
                this.queryResults = queryResults;
                this.colorIndices = colorIndices;
                this.seriesToColorIndex = seriesToColorIndex;
                this.firstRow = firstRow;
                this.lastRow = lastRow;
            }

            public IEnumerator<IInstanceParameter> GetEnumerator()
            {
                return
                    new GeoDataSource.QueryResultsDataEnumerator(this.queryResults, this.colorIndices,
                        this.seriesToColorIndex, this.firstRow, this.lastRow);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        public class QueryResultsDataEnumerator : IEnumerator<IInstanceParameter>, IDisposable, IEnumerator
        {
            private GeoQueryResults queryResults;
            private ModelQueryIndexedKeyColumn category;
            private List<int> colorIndices;
            private List<int> seriesToColorIndex;
            private int firstRow;
            private int lastRow;
            private int firstRowCurrentCategory;
            private int lastRowCurrentCategory;
            private int currentRow;
            private int currentMeasure;
            private int numMeasures;
            private bool disposed;

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public IInstanceParameter Current
            {
                get
                {
                    if (this.disposed)
                        throw new ObjectDisposedException("QueryResultsDataEnumerator");
                    return
                        new GeoDataSource.QueryResultsDataForRow(this.queryResults, this.colorIndices,
                            this.queryResults.Measures, this.seriesToColorIndex, this.currentMeasure,
                            this.category, this.currentRow);
                }
            }

            public QueryResultsDataEnumerator(GeoQueryResults queryResults, List<int> colorIndices,
                List<int> seriesToColorIndex, int firstRow, int lastRow)
            {
                this.queryResults = queryResults;
                this.colorIndices = colorIndices;
                this.seriesToColorIndex = seriesToColorIndex;
                this.firstRow = firstRow;
                this.lastRow = lastRow;
                this.category = queryResults.Category;
                List<ModelQueryMeasureColumn> measures = queryResults.Measures;
                this.numMeasures = measures == null || colorIndices == null ? 0 : Math.Max(1, measures.Count);
                this.Reset();
            }

            public bool MoveNext()
            {
                if (this.disposed)
                    throw new ObjectDisposedException("QueryResultsDataEnumerator");
                if (this.currentMeasure < 0 ||
                    this.currentRow == this.lastRowCurrentCategory - 1 && !this.InitializeRowsForNextCategory() &&
                    !this.InitializeRowsForNextMeasure())
                    return false;
                ++this.currentRow;
                return true;
            }

            public void Reset()
            {
                if (this.disposed)
                    throw new ObjectDisposedException("QueryResultsDataEnumerator");
                this.currentRow = -1;
                this.firstRowCurrentCategory = -1;
                this.lastRowCurrentCategory = -1;
                this.currentMeasure = this.numMeasures;
                this.InitializeRowsForNextMeasure();
            }

            public void Dispose()
            {
                this.disposed = true;
            }

            private bool InitializeRowsForNextMeasure()
            {
                --this.currentMeasure;
                if (this.currentMeasure < 0)
                {
                    this.firstRowCurrentCategory = -1;
                    this.lastRowCurrentCategory = -1;
                    return false;
                }
                this.firstRowCurrentCategory = this.lastRow;
                return this.InitializeRowsForNextCategory();
            }

            private bool InitializeRowsForNextCategory()
            {
                if (this.SetRowsForNextCategory(this.firstRowCurrentCategory))
                {
                    this.currentRow = this.firstRowCurrentCategory - 1;
                    return true;
                }
                this.currentRow = -1;
                return false;
            }

            private bool SetRowsForNextCategory(int row)
            {
                if (this.category == null)
                {
                    if (row == this.lastRow)
                    {
                        this.firstRowCurrentCategory = this.firstRow;
                        this.lastRowCurrentCategory = this.lastRow;
                        return true;
                    }
                    this.firstRowCurrentCategory = -1;
                    this.lastRowCurrentCategory = -1;
                    return false;
                }
                if (--row < this.firstRow)
                {
                    this.firstRowCurrentCategory = -1;
                    this.lastRowCurrentCategory = -1;
                    return false;
                }
                this.lastRowCurrentCategory = row + 1;
                if (this.category.Values == null)
                    return false;
                int uniqueValue = (int) this.category.Values[row];
                do
                {
                } while (--row >= this.firstRow && this.category.Values[row] == uniqueValue);

                this.firstRowCurrentCategory = row + 1;
                return true;
            }
        }

        public class QueryResultsDataForRow : IInstanceParameter
        {
            private GeoQueryResults queryResults;
            private List<int> colorIndices;
            private int rowIndex;
            private int measureIndex;
            private List<ModelQueryMeasureColumn> qrMeasures;
            private ModelQueryIndexedKeyColumn category;
            private List<int> seriesToColorIndex;

            public InstanceId Id
            {
                get
                {
                    if (this.qrMeasures != null)
                        return GeoDataSource.GetInstanceIdForRow(this.rowIndex, this.measureIndex,
                            (uint) Math.Max(1, this.qrMeasures.Count));
                    return new InstanceId(134217727U);
                }
            }

            public int IntegerValue
            {
                get { return 0; }
            }

            public double RealNumberValue
            {
                get
                {
                    if (this.qrMeasures == null || this.qrMeasures.Count == 0)
                        return double.NaN;
                    return this.qrMeasures[this.measureIndex].Values != null
                        ? double.NaN
                        : (double) this.qrMeasures[this.measureIndex].Values[this.rowIndex];
                }
            }

            public int ColorValue
            {
                get
                {
                    if (this.category != null)
                        return this.colorIndices[this.category.PreservedValuesIndex[this.ShiftValue]];
                    if (this.qrMeasures == null || this.qrMeasures.Count == 0)
                        return this.colorIndices[0];
                    return this.colorIndices[this.seriesToColorIndex[this.ShiftValue]];
                }
            }

            public int ShiftValue
            {
                get
                {
                    if (this.qrMeasures == null || this.qrMeasures.Count == 0)
                        return 0;
                    if (this.category == null)
                        return this.measureIndex;
                    return this.category.Values == null ? 0 : (int) this.category.Values[this.rowIndex];
                }
            }

            public DateTime? StartTime
            {
                get
                {
                    ModelQueryTimeKeyColumn time = this.queryResults.Time;
                    DateTime[] dateTimeArray = time == null ? null : time.StartTime;
                    if (dateTimeArray != null)
                        return dateTimeArray[this.rowIndex];
                    return new DateTime?();
                }
            }

            public DateTime? EndTime
            {
                get
                {
                    try
                    {
                        return this.queryResults.Time == null
                            ? new DateTime?()
                            : this.queryResults.GetEndTimeForRow(this.rowIndex);
                    }
                    catch (DataSource.InvalidQueryResultsException ex)
                    {
                        return new DateTime?();
                    }
                }
            }

            public QueryResultsDataForRow(GeoQueryResults queryResults, List<int> colorIndices,
                List<ModelQueryMeasureColumn> qrMeasures, List<int> seriesToColorIndex, int measureIndex,
                ModelQueryIndexedKeyColumn category, int rowIndex)
            {
                this.queryResults = queryResults;
                this.colorIndices = colorIndices;
                this.rowIndex = rowIndex;
                this.measureIndex = measureIndex;
                this.qrMeasures = qrMeasures;
                this.seriesToColorIndex = seriesToColorIndex;
                this.category = category;
            }
        }
    }
}