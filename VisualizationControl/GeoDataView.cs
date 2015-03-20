using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class GeoDataView : DataView
    {
        public GeoDataView(DataSource source)
            : base(source)
        {
        }

        public GeoField GetGeo(int sourceDataVersion)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return null;
            try
            {
                return geoDataSource.GetGeoFieldUsedInQuery(sourceDataVersion);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return null;
            }
        }

        public Func<int, int, string> GetGeoValuesAccessor(int sourceDataVersion, out int firstGeoCol)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            firstGeoCol = 0;
            if (geoDataSource == null)
                return null;
            try
            {
                return geoDataSource.GetGeoValuesAccessor(sourceDataVersion, out firstGeoCol);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return null;
            }
        }

        public Func<int, int> GetGeoRowsAccessor(int sourceDataVersion)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return null;
            try
            {
                return geoDataSource.GetGeoRowsAccessor(sourceDataVersion);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return null;
            }
        }

        public int GetColorCount(int sourceDataVersion)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return -1;
            try
            {
                return geoDataSource.GetColorCount(sourceDataVersion);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return -1;
            }
        }

        public int GetGeoBucketCount(int sourceDataVersion)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return -1;
            try
            {
                return geoDataSource.GetGeoBucketCount(sourceDataVersion);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return -1;
            }
        }

        public int GetBucketForInstanceId(int sourceDataVersion, InstanceId id)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return -1;
            try
            {
                return geoDataSource.GetBucketForInstanceId(sourceDataVersion, id);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return -1;
            }
        }

        public GeoEntityField.GeoEntityLevel? GetGeoEntityLevel(int sourceDataVersion)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return new GeoEntityField.GeoEntityLevel?();
            try
            {
                return geoDataSource.GetGeoEntityLevel(sourceDataVersion);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return new GeoEntityField.GeoEntityLevel?();
            }
        }

        public RegionLayerShadingMode? GetRegionShadingMode(int sourceDataVersion, RegionLayerShadingMode? selectedShadingMode)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return new RegionLayerShadingMode?();
            try
            {
                return geoDataSource.GetRegionShadingMode(sourceDataVersion, selectedShadingMode);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return new RegionLayerShadingMode?();
            }
        }

        public List<Tuple<AggregationFunction?, string, object>> TableColumnsWithValuesForId(InstanceId id, bool showRelatedCategories, DateTime? currentDateTime = null)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return null;
            return geoDataSource.TableColumnsWithValuesForId(id, showRelatedCategories, new DateTime?());
        }

        public int GetMaxInstanceCount(int sourceDataVersion)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return -1;
            try
            {
                return geoDataSource.GetMaxInstanceCount(sourceDataVersion);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return -1;
            }
        }

        public void GetMinMaxInstanceValues(int sourceDataVersion, out double min, out double max)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
            {
                min = double.NaN;
                max = double.NaN;
            }
            try
            {
                geoDataSource.GetMinMaxInstanceValues(sourceDataVersion, out min, out max);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                min = double.NaN;
                max = double.NaN;
            }
        }

        public void GetMinMaxLatLong(int sourceDataVersion, out RangeOf<double> latRange, out RangeOf<double> longRange)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
            {
                latRange = new RangeOf<double>(double.NaN, double.NaN);
                longRange = new RangeOf<double>(double.NaN, double.NaN);
            }
            try
            {
                geoDataSource.GetMinMaxLatLong(sourceDataVersion, out latRange, out longRange);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                latRange = new RangeOf<double>(double.NaN, double.NaN);
                longRange = new RangeOf<double>(double.NaN, double.NaN);
            }
        }

        public bool QueryResultsHaveMeasures(int sourceDataVersion)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return false;
            try
            {
                return geoDataSource.QueryResultsHaveMeasures(sourceDataVersion);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return false;
            }
        }

        public bool QueryResultsHaveCategory(int sourceDataVersion)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return false;
            try
            {
                return geoDataSource.QueryResultsHaveCategory(sourceDataVersion);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                return false;
            }
        }

        public void GetTimeBounds(int sourceDataVersion, out DateTime? beginTime, out DateTime? endtime)
        {
            beginTime = null;
            endtime = null;
            GeoDataSource source = this.Source as GeoDataSource;
            if (source != null)
            {
                try
                {
                    source.GetTimeBounds(sourceDataVersion, out beginTime, out endtime);
                }
                catch (DataSource.InvalidQueryResultsException)
                {
                }
            }

        }

        public bool GetRowData(int sourceDataVersion, int row, List<int> colorIndices, out double latitude, out double longitude, out IEnumerable<IInstanceParameter> result, out int nextRow, out bool abort)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            abort = false;
            if (geoDataSource != null && row >= 0)
            {
                if (row < this.GetRowCount(sourceDataVersion))
                {
                    try
                    {
                        if (geoDataSource.HasLatLong)
                        {
                            double lat = geoDataSource.GetLatitudeForQueryResultsRow(sourceDataVersion, row);
                            double lon = geoDataSource.GetLongitudeForQueryResultsRow(sourceDataVersion, row);
                            if (double.IsNaN(lat) || double.IsNaN(lon))
                            {
                                nextRow = row + 1;
                                latitude = longitude = double.NaN;
                                result = Enumerable.Empty<IInstanceParameter>();
                                return false;
                            }
                            latitude = lat;
                            longitude = lon;
                        }
                        else
                            latitude = longitude = double.NaN;
                        geoDataSource.GetEnumerableForQueryResultsRow(sourceDataVersion, row, colorIndices, out result, out nextRow);
                    }
                    catch (DataSource.InvalidQueryResultsException ex)
                    {
                        abort = true;
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception when fetching values for index {0}, returning abort={1}: {2}", (object)row, abort, (object)ex);
                        nextRow = row + 1;
                        latitude = longitude = double.NaN;
                        result = Enumerable.Empty<IInstanceParameter>();
                        return false;
                    }
                    return true;
                }
            }
            abort = true;
            nextRow = row + 1;
            latitude = longitude = double.NaN;
            result = Enumerable.Empty<IInstanceParameter>();
            return false;
        }

        public int GetNextGeoBucketRow(int sourceDataVersion, int row)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return -1;
            try
            {
                return geoDataSource.GetFirstRowInNextGeoBucket(row);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception calling GetNextGeoBucketRow: {0}", (object)ex);
                return -1;
            }
        }

        public InstanceId? GetInstanceIdForModelDataId(int sourceDataVersion, string modelId)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return new InstanceId?();
            try
            {
                return geoDataSource.GetCanonicalInstanceIdForModelDataId(sourceDataVersion, modelId);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception when fetching instance id for model id {0}: {1}", (object)modelId, (object)ex);
                return new InstanceId?();
            }
        }

        public InstanceId?[] GetCanonicalInstanceIdsForAllSeriesForModelDataId(int sourceDataVersion, string modelId, bool anyMeasure, bool anyCategoryValue)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return null;
            try
            {
                return geoDataSource.GetCanonicalInstanceIdsForAllSeriesForModelDataId(sourceDataVersion, modelId, anyMeasure, anyCategoryValue);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception when fetching instance id of all series for model id {0}: {1}", (object)modelId, (object)ex);
                return null;
            }
        }

        public int? GetSeriesIndexForModelDataId(int sourceDataVersion, string modelId)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return new int?();
            try
            {
                return geoDataSource.GetSeriesIndexForModelDataId(sourceDataVersion, modelId);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception when fetching series index for model id {0}: {1}", (object)modelId, (object)ex);
                return new int?();
            }
        }

        public InstanceId? GetCanonicalInstanceId(int sourceDataVersion, InstanceId id)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return new InstanceId?();
            try
            {
                return geoDataSource.GetCanonicalInstanceId(sourceDataVersion, id);
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception when fetching canonical id for instanceId {0}: {1}", (object)id, (object)ex);
                return new InstanceId?();
            }
        }

        public InstanceId?[] GetCanonicalInstanceIdsForAllSeries(int sourceDataVersion, InstanceId id)
        {
            GeoDataSource geoDataSource = this.Source as GeoDataSource;
            if (geoDataSource == null)
                return null;
            try
            {
                return geoDataSource.GetCanonicalInstanceIdsForAllSeries(sourceDataVersion, new InstanceId?(id));
            }
            catch (DataSource.InvalidQueryResultsException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception when fetching canonical id for all series for instanceId {0}: {1}", (object)id, (object)ex);
                return null;
            }
        }
    }
}
