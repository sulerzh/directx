using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class BingLatLonCache : ILatLonProvider
  {
    private static readonly string[] BingFindLocationByAddressQueryParameters = new string[6]
    {
      "addressLine",
      "locality",
      "adminDistrict",
      "adminDistrict",
      "postalCode",
      "countryRegion"
    };
    private static readonly string[] BingFindLocationByAddressCacheKeyParameters = new string[6]
    {
      "addressLine",
      "locality",
      "adminDistrict2",
      "adminDistrict",
      "postalCode",
      "countryRegion"
    };
    private static readonly string[] AddressNodeChildNames = new string[6]
    {
      "AddressLine",
      "Locality",
      "AdminDistrict2",
      "AdminDistrict",
      "PostalCode",
      "CountryRegion"
    };
    private static readonly string[] LocalityEntityTypes = new string[3]
    {
      "PopulatedPlace",
      "Neighborhood",
      "UrbanRegion"
    };
    public const int MaxConcurrentRequests = 20;
    public const int MaxTries = 5;
    public const int MinRetryDelayInterval = 1000;
    public const int MaxRetryDelayInterval = 5000;
    private const int BingMapsRequestTimeoutinSeconds = 45;
    private const string BingMapsRootUrl = "https://dev.virtualearth.net/REST/v1/Locations";
    private const string BingFindLocationByQueryQueryParameter = "query";
    private Action<Exception> onInternalError;
    private CultureInfo modelCulture;
    private CultureInfo cultureForRequests;
    private int maxConcurrentRequests;
    private System.Threading.Semaphore requestSemaphore;
    private string bingMapsKey;
    private string bingMapsRootUrl;
    private int bingMapsRequestTimeoutinSeconds;
    private Random random;
    private bool initialized;
    private volatile bool isDirty;
    private ConcurrentDictionary<string, BingLatLonCache.LatLon> cache;

    [XmlIgnore]
    public Action<Exception> OnInternalError
    {
      get
      {
        return this.onInternalError;
      }
      set
      {
        this.onInternalError = value;
      }
    }

    [XmlIgnore]
    public CultureInfo ModelCulture
    {
      get
      {
        return this.modelCulture;
      }
      set
      {
        this.modelCulture = value == null ? CultureInfo.InvariantCulture : value;
      }
    }

    [XmlIgnore]
    public CultureInfo CultureForRequests
    {
      get
      {
        return this.cultureForRequests;
      }
      set
      {
        this.cultureForRequests = value ?? Thread.CurrentThread.CurrentUICulture;
      }
    }

    [XmlArray("Is", Namespace = "http://microsoft.data.visualization.geo3d/1.0")]
    [XmlArrayItem("I", typeof (BingLatLonCache.SerializableLatLonCacheItem))]
    public BingLatLonCache.SerializableLatLonCacheItem[] CachedItems
    {
      get
      {
        return Enumerable.ToArray<BingLatLonCache.SerializableLatLonCacheItem>(Enumerable.Select<KeyValuePair<string, BingLatLonCache.LatLon>, BingLatLonCache.SerializableLatLonCacheItem>((IEnumerable<KeyValuePair<string, BingLatLonCache.LatLon>>) this.cache, (Func<KeyValuePair<string, BingLatLonCache.LatLon>, BingLatLonCache.SerializableLatLonCacheItem>) (kv => new BingLatLonCache.SerializableLatLonCacheItem()
        {
          Key = kv.Key,
          Value = kv.Value
        })));
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");
        int count = this.cache.Count;
        int numElementsSet = 0;
        int numOverwritten = 0;
        Array.ForEach<BingLatLonCache.SerializableLatLonCacheItem>(value, (Action<BingLatLonCache.SerializableLatLonCacheItem>) (kv =>
        {
          ++numElementsSet;
          if (this.cache.ContainsKey(kv.Key))
            ++numOverwritten;
          this.cache[kv.Key] = kv.Value;
        }));
        VisualizationTraceSource.Current.TraceEvent((TraceEventType) (numOverwritten > 0 ? 4 : 8), 0, "LatLonCache.CachedItems_set: initial count = {0}, final count = {1}, numItemsRead = {2}, numOverwritten={3}", (object) count, (object) this.cache.Count, (object) numElementsSet, (object) numOverwritten);
      }
    }

    public BingLatLonCache()
    {
      this.cache = new ConcurrentDictionary<string, BingLatLonCache.LatLon>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this.ModelCulture = (CultureInfo) null;
      this.initialized = false;
      this.isDirty = false;
    }

    public bool Initialize(int maxConcurrentRequests = 20, string bingMapsKey = null, string bingMapsRootUrl = null, int bingMapsRequestTimeoutInSeconds = 45, CultureInfo cultureForRequests = null)
    {
      if (this.initialized)
        return false;
      this.maxConcurrentRequests = maxConcurrentRequests <= 0 ? 20 : maxConcurrentRequests;
      this.random = new Random();
      this.requestSemaphore = new System.Threading.Semaphore(this.maxConcurrentRequests, this.maxConcurrentRequests);
      this.bingMapsKey = bingMapsKey;
      this.bingMapsRootUrl = bingMapsRootUrl ?? "https://dev.virtualearth.net/REST/v1/Locations";
      this.bingMapsRequestTimeoutinSeconds = bingMapsRequestTimeoutInSeconds;
      this.CultureForRequests = cultureForRequests;
      this.initialized = true;
      this.isDirty = false;
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LatLonCache.Initialize(): Using '{0}' as Bing Maps Geocoding URL", (object) this.bingMapsRootUrl);
      return true;
    }

    public bool ShouldSave()
    {
      return this.isDirty;
    }

    public void SetShouldSave(bool isDirty)
    {
      this.isDirty = isDirty;
    }

    public void GetLatLonAsync(GeoField geoField, int firstGeoCol, int count, Func<int, int, string> geoValuesAccessor, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, GeoAmbiguity[] ambiguities, CompletionStats completionStats, Action<object, int, int, int> latLonResolvedCallback = null, Action<object> completionCallback = null, object context = null)
    {
      if (!this.initialized)
        throw new InvalidOperationException("BingLatLonCache has not been initialized.");
      if (cancellationToken.IsCancellationRequested)
      {
        if (completionStats == null)
          return;
        completionStats.Cancelled = true;
      }
      else
      {
        this.ValidateGetLatLonParameters(geoField, count, geoValuesAccessor, geoRowsAccessor, cancellationToken, lat, lon, ambiguities);
        CompletionStats completionStats1 = completionStats;
        if (completionStats1 == null)
          completionStats1 = new CompletionStats()
          {
            Pending = false,
            Requested = -1
          };
        CompletionStats completionStats2 = completionStats1;
        if (!(geoField is GeoEntityField) && !(geoField is GeoFullAddressField))
          return;
        BingLatLonCache.GetLatLonByAddressAsyncRequestState asyncRequestState = new BingLatLonCache.GetLatLonByAddressAsyncRequestState()
        {
          Geo = geoField,
          FirstGeoCol = firstGeoCol,
          QueryResultCount = count,
          GeoValuesAccessor = geoValuesAccessor,
          GeoRowsAccessor = geoRowsAccessor,
          Lat = lat,
          Lon = lon,
          BoundingBox = boundingBox,
          Ambiguities = ambiguities,
          CancellationToken = cancellationToken,
          CompletionStats = completionStats2,
          LatLonResolvedCallback = latLonResolvedCallback,
          CompletionCallback = completionCallback,
          Context = context
        };
        if (!cancellationToken.IsCancellationRequested)
          ThreadPool.QueueUserWorkItem(new WaitCallback(this.GetLatLonByAddressAsyncCallback), (object) asyncRequestState);
        else
          completionStats2.Cancelled = true;
      }
    }

    public void GetLatLon(GeoField geoField, int firstGeoCol, int count, Func<int, int, string> geoValuesAccessor, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, GeoAmbiguity[] ambiguities)
    {
      if (!this.initialized)
        throw new InvalidOperationException("BingLatLonCache has not been initialized.");
      this.ValidateGetLatLonParameters(geoField, count, geoValuesAccessor, geoRowsAccessor, cancellationToken, lat, lon, ambiguities);
      GeoEntityField geoEntityField = geoField as GeoEntityField;
      if (geoEntityField == null)
        return;
      CompletionStats stats = new CompletionStats()
      {
        Pending = false,
        Requested = -1
      };
      this.GetLatLon(count, (BingLatLonCache.QueryParameterBuilder) ((int i, out string cacheKey, out string[] geoValues, out string queryByAddressValue) => BingLatLonCache.FormatBingFindLocationByAddressParameters(geoEntityField, firstGeoCol, i, geoValuesAccessor, out cacheKey, out geoValues, out queryByAddressValue)), geoRowsAccessor, cancellationToken, stats, geoField, lat, lon, boundingBox, ambiguities, (Action<object, int, int, int>) null, (object) null);
    }

    public bool GetLatLon(string geoQuery, CancellationToken cancellationToken, out double lat, out double lon, out GeoResolutionBorder boundingBox, out GeoAmbiguity ambiguity)
    {
      if (!this.initialized)
        throw new InvalidOperationException("BingLatLonCache has not been initialized.");
      if (geoQuery == null)
        throw new ArgumentNullException("geoQuery");
      CompletionStats stats = new CompletionStats()
      {
        Pending = false,
        Requested = 1
      };
      double[] lat1 = new double[1];
      double[] lon1 = new double[1];
      GeoResolutionBorder[] boundingBox1 = new GeoResolutionBorder[1];
      GeoAmbiguity[] ambiguities = new GeoAmbiguity[1];
      string urlParameters = string.Format("?{0}={1}", (object) "query", (object) Uri.EscapeDataString(geoQuery));
      string cacheKey = urlParameters;
      string str = geoQuery;
      this.GetLatLon(1, (BingLatLonCache.QueryParameterBuilder) ((int i, out string cacheKeyParam, out string[] geoValues, out string queryByAddressValue) =>
      {
        geoValues = (string[]) null;
        cacheKeyParam = cacheKey;
        queryByAddressValue = geoQuery;
        return urlParameters;
      }), (Func<int, int>) (row => 1), cancellationToken, stats, (GeoField) null, lat1, lon1, boundingBox1, ambiguities, (Action<object, int, int, int>) null, (object) null);
      lat = lat1[0];
      lon = lon1[0];
      boundingBox = boundingBox1[0];
      ambiguity = ambiguities[0];
      if (!double.IsNaN(lat))
        return !double.IsNaN(lon);
      else
        return false;
    }

    public void GetLatLonAsync(string geoQuery, CancellationToken cancellationToken, Action<object, bool, double, double, GeoResolutionBorder, GeoAmbiguity> completionCallback = null, object context = null)
    {
      if (!this.initialized)
        throw new InvalidOperationException("BingLatLonCache has not been initialized.");
      if (geoQuery == null)
        throw new ArgumentNullException("geoQuery");
      BingLatLonCache.GetLatLonByQueryAsyncRequestState asyncRequestState = new BingLatLonCache.GetLatLonByQueryAsyncRequestState()
      {
        GeoQuery = geoQuery,
        CancellationToken = cancellationToken,
        CompletionCallback = completionCallback,
        Context = context
      };
      if (cancellationToken.IsCancellationRequested)
        return;
      ThreadPool.QueueUserWorkItem(new WaitCallback(this.GetLatLonByQueryAsyncCallback), (object) asyncRequestState);
    }

    private void ValidateGetLatLonParameters(GeoField geoField, int count, Func<int, int, string> geoValuesAccessor, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoAmbiguity[] ambiguities)
    {
      if (geoField == null)
        throw new ArgumentNullException("geoField");
      if (geoValuesAccessor == null)
        throw new ArgumentNullException("geoValuesAccessor");
      if (geoRowsAccessor == null)
        throw new ArgumentNullException("geoRowsAccessor");
      if (lat == null)
        throw new ArgumentNullException("lat");
      if (lon == null)
        throw new ArgumentNullException("lon");
      if (ambiguities == null)
        throw new ArgumentNullException("ambiguities");
      if (count < 0)
        throw new ArgumentException("Count must not be negative");
      if (Enumerable.Count<double>((IEnumerable<double>) lat) != count)
        throw new ArgumentException("lat.Count != count");
      if (Enumerable.Count<double>((IEnumerable<double>) lon) != count)
        throw new ArgumentException("lon.Count != count");
      if (Enumerable.Count<GeoAmbiguity>((IEnumerable<GeoAmbiguity>) ambiguities) != count)
        throw new ArgumentException("ambiguities.Count != count");
    }

    private BingLatLonCache.QueryParameterBuilder GetQueryParameterBuilderForByAddressAsyncCallback(BingLatLonCache.GetLatLonByAddressAsyncRequestState state)
    {
      GeoEntityField geoEntityField = state.Geo as GeoEntityField;
      if (geoEntityField == null)
        return (BingLatLonCache.QueryParameterBuilder) ((int i, out string cacheKeyParam, out string[] geoValues, out string queryByAddressValue) => BingLatLonCache.FormatBingFindLocationByQueryParameters((GeoFullAddressField) state.Geo, state.FirstGeoCol, i, state.GeoValuesAccessor, out cacheKeyParam, out geoValues, out queryByAddressValue));
      if (geoEntityField.HasOnlyLocality)
        return (BingLatLonCache.QueryParameterBuilder) ((int i, out string cacheKeyParam, out string[] geoValues, out string queryByAddressValue) => BingLatLonCache.FormatBingFindLocalityByQueryParameters((GeoEntityField) state.Geo, state.FirstGeoCol, i, state.GeoValuesAccessor, out cacheKeyParam, out geoValues, out queryByAddressValue));
      else
        return (BingLatLonCache.QueryParameterBuilder) ((int i, out string cacheKeyParam, out string[] geoValues, out string queryByAddressValue) => BingLatLonCache.FormatBingFindLocationByAddressParameters((GeoEntityField) state.Geo, state.FirstGeoCol, i, state.GeoValuesAccessor, out cacheKeyParam, out geoValues, out queryByAddressValue));
    }

    private void GetLatLonByAddressAsyncCallback(object context)
    {
      BingLatLonCache.GetLatLonByAddressAsyncRequestState state = context as BingLatLonCache.GetLatLonByAddressAsyncRequestState;
      try
      {
        this.GetLatLon(state.QueryResultCount, this.GetQueryParameterBuilderForByAddressAsyncCallback(state), state.GeoRowsAccessor, state.CancellationToken, state.CompletionStats, state.Geo, state.Lat, state.Lon, state.BoundingBox, state.Ambiguities, state.LatLonResolvedCallback, state.Context);
      }
      catch (OperationCanceledException ex1)
      {
        try
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "GetLatLonAsyncCallback() Caught OperationCanceledException");
        }
        catch (Exception ex2)
        {
        }
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.Fail(ex);
        if (this.OnInternalError == null)
          return;
        this.OnInternalError(ex);
      }
      finally
      {
        if (state.CompletionCallback != null)
        {
          try
          {
            state.CompletionCallback(state.Context);
          }
          catch (Exception ex)
          {
            VisualizationTraceSource.Current.Fail(ex);
            if (this.OnInternalError != null)
              this.OnInternalError(ex);
          }
        }
      }
    }

    private void GetLatLonByQueryAsyncCallback(object context)
    {
      BingLatLonCache.GetLatLonByQueryAsyncRequestState asyncRequestState = context as BingLatLonCache.GetLatLonByQueryAsyncRequestState;
      bool flag = false;
      try
      {
        flag = this.GetLatLon(asyncRequestState.GeoQuery, asyncRequestState.CancellationToken, out asyncRequestState.Lat, out asyncRequestState.Lon, out asyncRequestState.BoundingBox, out asyncRequestState.Ambiguity);
      }
      catch (OperationCanceledException ex1)
      {
        try
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "GetLatLonAsyncCallback() Caught OperationCanceledException");
        }
        catch (Exception ex2)
        {
        }
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.Fail(ex);
        if (this.OnInternalError == null)
          return;
        this.OnInternalError(ex);
      }
      finally
      {
        if (asyncRequestState.CompletionCallback != null)
        {
          try
          {
            asyncRequestState.CompletionCallback(asyncRequestState.Context, flag, asyncRequestState.Lat, asyncRequestState.Lon, asyncRequestState.BoundingBox, asyncRequestState.Ambiguity);
          }
          catch (Exception ex)
          {
            VisualizationTraceSource.Current.Fail(ex);
            if (this.OnInternalError != null)
              this.OnInternalError(ex);
          }
        }
      }
    }

    private void GetLatLon(int count, BingLatLonCache.QueryParameterBuilder queryParameterBuilder, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, CompletionStats stats, GeoField geoField, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, GeoAmbiguity[] ambiguities, Action<object, int, int, int> latLonResolvedCallback = null, object context = null)
    {
      WaitHandle[] waitArray = new WaitHandle[2]
      {
        cancellationToken.WaitHandle,
        (WaitHandle) this.requestSemaphore
      };
      Stopwatch stopwatch = Stopwatch.StartNew();
      bool needBoundingBox = false;
      List<GeoResolutionBorder>[] listArray = (List<GeoResolutionBorder>[]) null;
      for (int index = 0; index < count; ++index)
      {
        lat[index] = double.NaN;
        lon[index] = double.NaN;
        ambiguities[index] = (GeoAmbiguity) null;
      }
      if (boundingBox != null)
      {
        needBoundingBox = true;
        Array.Clear((Array) boundingBox, 0, boundingBox.Length);
        listArray = new List<GeoResolutionBorder>[count];
      }
      CountdownEvent countdownEvent = new CountdownEvent(count);
      try
      {
        cancellationToken.ThrowIfCancellationRequested();
        WaitHandle[] waitHandles = new WaitHandle[2]
        {
          countdownEvent.WaitHandle,
          cancellationToken.WaitHandle
        };
        Dictionary<string, int> countryCount = new Dictionary<string, int>(count, (IEqualityComparer<string>) StringComparer.Ordinal);
        int num = 0;
        for (int geoAccessorIndex = 0; geoAccessorIndex < count; ++geoAccessorIndex)
        {
          string cacheKey;
          string[] geoValues;
          string queryByAddressValue;
          string urlParameters = queryParameterBuilder(num, out cacheKey, out geoValues, out queryByAddressValue);
          int nextRowToGeocode = geoRowsAccessor(num);
          this.ResolveLatLon(cacheKey, urlParameters, needBoundingBox, geoAccessorIndex, num, nextRowToGeocode, geoValues, queryByAddressValue, geoField, countdownEvent, waitArray, 1, cancellationToken, lat, lon, listArray, ambiguities, countryCount, stats, nextRowToGeocode - num, latLonResolvedCallback, context);
          cancellationToken.ThrowIfCancellationRequested();
          num = nextRowToGeocode;
        }
        if (WaitHandle.WaitAny(waitHandles) == 1)
          cancellationToken.ThrowIfCancellationRequested();
        GeoEntityField geoEntityField = geoField as GeoEntityField;
        if (geoEntityField != null && geoEntityField.HasOnlyLocality)
          this.ResolveDeferredAmbiguities(count, geoRowsAccessor, cancellationToken, lat, lon, boundingBox, listArray, ambiguities, BingLatLonCache.LocalityEntityTypes, latLonResolvedCallback, context);
        else
          this.ResolveDeferredAmbiguitiesUsingCountryCounts(count, geoRowsAccessor, cancellationToken, lat, lon, boundingBox, listArray, ambiguities, countryCount, latLonResolvedCallback, context);
      }
      catch (OperationCanceledException ex)
      {
        stats.Cancelled = true;
        throw new OperationCanceledException(ex.Message + " (rethrown with cancellation token)", (Exception) ex, cancellationToken);
      }
      finally
      {
        stopwatch.Stop();
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "GetLatLon() completed in {0} msec; requested: {1}, completed: {2}, from cache: {3}, Bing Queries: {4}, Bing Retries {5}, Resolved: {6}, NotFound {7}, Failed: {8}, Invalid args: {9}, AmbiguousUnresolved: {10}, Ambiguous Resolved: {11}, Ambiguous Total: {12}, Exceptions calling Bing: {13}, caller cancelled: {14}", (object) stopwatch.ElapsedMilliseconds, stats.Requested == -1 ? (object) "unknown" : (object) stats.Requested.ToString(), (object) stats.Completed, (object) stats.CacheHits, (object) stats.BingQueries, (object) stats.BingRetries, (object) stats.Resolved, (object) stats.NotFound, (object) stats.QueryFailed, (object) stats.InvalidArgs, (object) stats.AmbiguousUnresolved, (object) stats.AmbiguousResolved, (object) stats.Ambiguous, (object) stats.ExceptionsQueryingBing, cancellationToken.IsCancellationRequested);
        if (countdownEvent != null)
          countdownEvent.Dispose();
      }
    }

    private static string[] GetGeoValues(GeoEntityField geoField, int firstGeoCol, int rowIndex, Func<int, int, string> geoValuesAccessor)
    {
      int index = 0;
      string[] strArray = new string[6];
      Array.Clear((Array) strArray, 0, strArray.Length);
      int num = firstGeoCol;
      while (index < Enumerable.Count<TableColumn>((IEnumerable<TableColumn>) geoField.GeoColumns))
      {
        TableColumn column = geoField.GeoColumns[index];
        GeoEntityField.GeoEntityLevel geoEntityLevel = geoField.GeoLevel(column);
        strArray[(int) geoEntityLevel] = geoValuesAccessor(rowIndex, num);
        ++index;
        ++num;
      }
      return strArray;
    }

    private static string FormatBingFindLocationByAddressParameters(GeoEntityField geoField, int firstGeoCol, int rowIndex, Func<int, int, string> geoValuesAccessor, out string cacheKey, out string[] geoValues, out string queryByAddressValue)
    {
      StringBuilder stringBuilder1 = new StringBuilder("?");
      StringBuilder stringBuilder2 = new StringBuilder("?");
      string str = "&";
      queryByAddressValue = (string) null;
      geoValues = BingLatLonCache.GetGeoValues(geoField, firstGeoCol, rowIndex, geoValuesAccessor);
      for (GeoEntityField.GeoEntityLevel geoEntityLevel = GeoEntityField.GeoEntityLevel.AddressLine; geoEntityLevel <= GeoEntityField.GeoEntityLevel.Country; ++geoEntityLevel)
      {
        if (geoValues[(int) geoEntityLevel] != null)
        {
          if (geoEntityLevel == GeoEntityField.GeoEntityLevel.AdminDistrict2 && geoValues[3] != null)
          {
            stringBuilder1.AppendFormat("{0}={1},%20{2}{3}", (object) BingLatLonCache.BingFindLocationByAddressQueryParameters[(int) geoEntityLevel], (object) Uri.EscapeDataString(geoValues[(int) geoEntityLevel]), (object) Uri.EscapeDataString(geoValues[3]), (object) str);
            stringBuilder2.AppendFormat("{0}={1}{2}", (object) BingLatLonCache.BingFindLocationByAddressCacheKeyParameters[(int) geoEntityLevel], (object) Uri.EscapeDataString(geoValues[(int) geoEntityLevel]), (object) str);
            ++geoEntityLevel;
            stringBuilder2.AppendFormat("{0}={1}{2}", (object) BingLatLonCache.BingFindLocationByAddressCacheKeyParameters[(int) geoEntityLevel], (object) Uri.EscapeDataString(geoValues[(int) geoEntityLevel]), (object) str);
          }
          else
          {
            stringBuilder1.AppendFormat("{0}={1}{2}", (object) BingLatLonCache.BingFindLocationByAddressQueryParameters[(int) geoEntityLevel], (object) Uri.EscapeDataString(geoValues[(int) geoEntityLevel]), (object) str);
            stringBuilder2.AppendFormat("{0}={1}{2}", (object) BingLatLonCache.BingFindLocationByAddressCacheKeyParameters[(int) geoEntityLevel], (object) Uri.EscapeDataString(geoValues[(int) geoEntityLevel]), (object) str);
          }
        }
      }
      if (Enumerable.Count<TableColumn>((IEnumerable<TableColumn>) geoField.GeoColumns) > 0)
      {
        stringBuilder2.Remove(stringBuilder1.Length - str.Length, str.Length);
        cacheKey = ((object) stringBuilder2).ToString();
        stringBuilder1.Remove(stringBuilder1.Length - str.Length, str.Length);
        return ((object) stringBuilder1).ToString();
      }
      else
      {
        cacheKey = (string) null;
        return (string) null;
      }
    }

    private static string FormatBingFindLocalityByQueryParameters(GeoEntityField geoField, int firstGeoCol, int rowIndex, Func<int, int, string> geoValuesAccessor, out string cacheKey, out string[] geoValues, out string queryByAddressValue)
    {
      geoValues = BingLatLonCache.GetGeoValues(geoField, firstGeoCol, rowIndex, geoValuesAccessor);
      queryByAddressValue = (string) null;
      string stringToEscape = geoValues[1];
      cacheKey = string.Format("?{0}={1}", (object) "query", (object) Uri.EscapeDataString(stringToEscape));
      return string.Format("?{0}={1}", (object) "query", (object) Uri.EscapeDataString(stringToEscape));
    }

    private static string FormatBingFindLocationByQueryParameters(GeoFullAddressField geoField, int firstGeoCol, int rowIndex, Func<int, int, string> geoValuesAccessor, out string cacheKey, out string[] geoValues, out string queryByAddressValue)
    {
      geoValues = (string[]) null;
      queryByAddressValue = geoValuesAccessor(rowIndex, firstGeoCol);
      cacheKey = string.Format("?{0}={1}", (object) "query", (object) Uri.EscapeDataString(queryByAddressValue));
      return string.Format("?{0}={1}", (object) "query", (object) Uri.EscapeDataString(queryByAddressValue));
    }

    private void ResolveDeferredAmbiguities(int count, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, List<GeoResolutionBorder>[] boundingBoxForAmbiguities, GeoAmbiguity[] geoLocations, string[] preferredEntityTypes, Action<object, int, int, int> latLonResolvedCallback, object context)
    {
      bool flag1 = boundingBox != null;
      int num1 = 0;
      for (int index1 = 0; index1 < count; ++index1)
      {
        int num2 = geoRowsAccessor(num1);
        if (geoLocations[index1] != null && geoLocations[index1].ResolutionIndex == -1)
        {
          GeoAmbiguity geoAmbiguity = geoLocations[index1];
          GeoAmbiguity.Resolution resolution = GeoAmbiguity.Resolution.Deferred;
          int index2 = -1;
          int index3 = (int) geoAmbiguity.SmallestGeoValueIndex;
          string smallestGeoValue = geoAmbiguity.SmallestGeoValue;
          BingLatLonCache.LatLon latLon = (BingLatLonCache.LatLon) null;
          this.cache.TryGetValue(geoAmbiguity.AmbiguityKey, out latLon);
          bool flag2 = latLon != null && latLon.HighConfidence;
          CultureInfo modelCulture = this.ModelCulture;
          for (int index4 = 0; index4 < Enumerable.Count<GeoResolution>((IEnumerable<GeoResolution>) geoAmbiguity.GeoResolutions); ++index4)
          {
            GeoResolution geoResolution = geoAmbiguity.GeoResolutions[index4];
            bool flag3 = false;
            foreach (string strA in preferredEntityTypes)
            {
              if (string.CompareOrdinal(strA, geoResolution.EntityType) == 0)
              {
                if (smallestGeoValue != null && geoResolution.AddressFields[index3] != null && string.Compare(smallestGeoValue, geoResolution.AddressFields[index3], true, modelCulture) == 0)
                {
                  index2 = index4;
                  resolution = flag2 ? GeoAmbiguity.Resolution.EntityTypeAndValueMatch : GeoAmbiguity.Resolution.EntityTypeAndValueMatchMediumConf;
                }
                else
                {
                  index2 = index4;
                  resolution = flag2 ? GeoAmbiguity.Resolution.EntityTypeMatch : GeoAmbiguity.Resolution.EntityTypeMatchMediumConf;
                }
                flag3 = true;
                break;
              }
            }
            if (flag3)
              break;
          }
          if (index2 == -1 && geoLocations[index1] != null && geoLocations[index1].GeoResolutions.Count > 0)
          {
            index2 = 0;
            resolution = GeoAmbiguity.Resolution.FirstListed;
          }
          if (index2 != -1)
          {
            geoAmbiguity.ResolutionIndex = index2;
            geoAmbiguity.ResolutionType = resolution;
            lat[index1] = geoAmbiguity.GeoResolutions[index2].Lat;
            lon[index1] = geoAmbiguity.GeoResolutions[index2].Lon;
            if (latLonResolvedCallback != null)
            {
              if (!cancellationToken.IsCancellationRequested)
              {
                try
                {
                  latLonResolvedCallback(context, num1, num2, index1);
                }
                catch (Exception ex)
                {
                  VisualizationTraceSource.Current.Fail("ResolveDeferredAmbiguities(): Exception calling LatLonResolveCallback (after cache lookup) ignored, key=" + geoAmbiguity.AmbiguityKey, ex);
                }
              }
            }
          }
        }
        else if (geoLocations[index1] != null && geoLocations[index1].ResolutionType == GeoAmbiguity.Resolution.SingleMatchHighConf)
        {
          GeoAmbiguity geoAmbiguity = geoLocations[index1];
          GeoResolution geoResolution = geoAmbiguity.GeoResolutions[0];
          bool flag2 = false;
          foreach (string strA in preferredEntityTypes)
          {
            if (string.CompareOrdinal(strA, geoResolution.EntityType) == 0)
            {
              flag2 = true;
              break;
            }
          }
          if (!flag2)
            geoAmbiguity.ResolutionType = GeoAmbiguity.Resolution.FirstListed;
        }
        if (flag1)
        {
          if (boundingBoxForAmbiguities[index1] != null && boundingBoxForAmbiguities[index1].Count > 0 && geoLocations[index1].ResolutionIndex != -1)
            boundingBox[index1] = boundingBoxForAmbiguities[index1][geoLocations[index1].ResolutionIndex];
          else
            boundingBox[index1] = new GeoResolutionBorder()
            {
              East = double.NaN,
              West = double.NaN,
              North = double.NaN,
              South = double.NaN
            };
        }
        cancellationToken.ThrowIfCancellationRequested();
        num1 = num2;
      }
    }

    private void ResolveDeferredAmbiguitiesUsingCountryCounts(int count, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, List<GeoResolutionBorder>[] boundingBoxForAmbiguities, GeoAmbiguity[] geoLocations, Dictionary<string, int> countryCount, Action<object, int, int, int> latLonResolvedCallback, object context)
    {
      bool flag1 = boundingBox != null;
      int num1 = 0;
      for (int index1 = 0; index1 < count; ++index1)
      {
        int num2 = geoRowsAccessor(num1);
        if (geoLocations[index1] != null && geoLocations[index1].ResolutionIndex == -1)
        {
          GeoAmbiguity geoAmbiguity = geoLocations[index1];
          int num3 = -1;
          GeoAmbiguity.Resolution resolution = GeoAmbiguity.Resolution.Deferred;
          int index2 = -1;
          int index3 = (int) geoAmbiguity.SmallestGeoValueIndex;
          string smallestGeoValue = geoAmbiguity.SmallestGeoValue;
          bool flag2 = false;
          bool flag3 = geoAmbiguity.SmallestGeoValueIndex == GeoEntityField.GeoEntityLevel.AdminDistrict;
          bool flag4 = geoAmbiguity.SmallestGeoValueIndex == GeoEntityField.GeoEntityLevel.AdminDistrict2;
          bool flag5 = false;
          CultureInfo modelCulture = this.ModelCulture;
          for (int index4 = 0; index4 < Enumerable.Count<GeoResolution>((IEnumerable<GeoResolution>) geoAmbiguity.GeoResolutions); ++index4)
          {
            GeoResolution geoResolution = geoAmbiguity.GeoResolutions[index4];
            string index5 = geoResolution.AddressFields[5];
            if (!string.IsNullOrWhiteSpace(index5))
            {
              int num4 = countryCount[index5];
              if (num4 > num3)
              {
                num3 = num4;
                index2 = index4;
                resolution = GeoAmbiguity.Resolution.TopCountCountrySingleMatch;
                flag2 = false;
                flag5 = false;
                if (flag3)
                  flag5 = geoResolution.AddressFields[2] == null;
                else if (flag4)
                  flag5 = geoResolution.AddressFields[2] != null;
                if (smallestGeoValue != null && geoResolution.AddressFields[index3] != null && string.Compare(smallestGeoValue, geoResolution.AddressFields[index3], true, modelCulture) == 0)
                  flag2 = true;
              }
              else if (num4 == num3)
              {
                bool flag6 = false;
                if (flag5)
                {
                  if (flag3)
                    flag6 = geoResolution.AddressFields[2] != null;
                  else if (flag4)
                    flag6 = geoResolution.AddressFields[2] == null;
                }
                else
                {
                  if (flag3)
                    flag5 = geoResolution.AddressFields[2] == null;
                  else if (flag4)
                    flag5 = geoResolution.AddressFields[2] != null;
                  if (flag5)
                  {
                    flag6 = true;
                    index2 = index4;
                    resolution = GeoAmbiguity.Resolution.TopCountCountrySingleMatch;
                    flag2 = false;
                    if (smallestGeoValue != null && geoResolution.AddressFields[index3] != null && string.Compare(smallestGeoValue, geoResolution.AddressFields[index3], true, modelCulture) == 0)
                      flag2 = true;
                  }
                }
                if (!flag6)
                {
                  if (flag2)
                  {
                    if (smallestGeoValue != null && geoResolution.AddressFields[index3] != null && string.Compare(smallestGeoValue, geoResolution.AddressFields[index3], true, modelCulture) == 0)
                      resolution = GeoAmbiguity.Resolution.TopCountCountryClosestFirstMatch;
                  }
                  else if (smallestGeoValue != null && geoResolution.AddressFields[index3] != null && string.Compare(smallestGeoValue, geoResolution.AddressFields[index3], true, modelCulture) == 0)
                  {
                    index2 = index4;
                    flag2 = true;
                    resolution = GeoAmbiguity.Resolution.TopCountCountryClosestMatch;
                  }
                  else
                    resolution = GeoAmbiguity.Resolution.TopCountCountryFirstValue;
                }
              }
            }
          }
          if (index2 == -1 && geoLocations[index1] != null && geoLocations[index1].GeoResolutions.Count > 0)
          {
            index2 = 0;
            resolution = GeoAmbiguity.Resolution.TopCountCountryFirstValue;
          }
          if (index2 != -1)
          {
            geoAmbiguity.ResolutionIndex = index2;
            geoAmbiguity.ResolutionType = resolution;
            lat[index1] = geoAmbiguity.GeoResolutions[index2].Lat;
            lon[index1] = geoAmbiguity.GeoResolutions[index2].Lon;
            if (latLonResolvedCallback != null)
            {
              if (!cancellationToken.IsCancellationRequested)
              {
                try
                {
                  latLonResolvedCallback(context, num1, num2, index1);
                }
                catch (Exception ex)
                {
                  VisualizationTraceSource.Current.Fail("ResolveDeferredAmbiguitiesUsingCountryCounts(): Exception calling LatLonResolveCallback (after cache lookup) ignored, key=" + geoAmbiguity.AmbiguityKey, ex);
                }
              }
            }
          }
        }
        if (flag1)
        {
          if (boundingBoxForAmbiguities[index1] != null && boundingBoxForAmbiguities[index1].Count > 0 && geoLocations[index1].ResolutionIndex != -1)
            boundingBox[index1] = boundingBoxForAmbiguities[index1][geoLocations[index1].ResolutionIndex];
          else
            boundingBox[index1] = new GeoResolutionBorder()
            {
              East = double.NaN,
              West = double.NaN,
              North = double.NaN,
              South = double.NaN
            };
        }
        cancellationToken.ThrowIfCancellationRequested();
        num1 = num2;
      }
    }

    private void ResolveLatLon(string cacheKey, string urlParameters, bool needBoundingBox, int geoAccessorIndex, int rowIndex, int nextRowToGeocode, string[] geoValues, string queryByAddressValue, GeoField geoField, CountdownEvent countdownEvent, WaitHandle[] waitArray, int semaphoreIndexInWaitArray, CancellationToken cancellationToken, double[] lat, double[] lon, List<GeoResolutionBorder>[] boundingBoxes, GeoAmbiguity[] ambiguities, Dictionary<string, int> countryCount, CompletionStats stats, int incrementForCompletionStats, Action<object, int, int, int> latLonResolvedCallback, object context)
    {
      BingLatLonCache.LatLon latLon;
      if (!needBoundingBox && this.cache.TryGetValue(cacheKey, out latLon))
      {
        GeoResolution[] geoResolutionArray = latLon.GeoResolutions;
        if (geoResolutionArray == null)
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unexpected: geoResolutions = null in cache, will issue query again, key = {0}", (object) cacheKey);
        }
        else
        {
          int smartPickIndex;
          GeoAmbiguity.Resolution resolutionType;
          bool flag;
          if (Enumerable.Count<GeoResolution>((IEnumerable<GeoResolution>) geoResolutionArray) == 0)
          {
            smartPickIndex = -1;
            resolutionType = GeoAmbiguity.Resolution.NoMatch;
            flag = false;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Lookup from cache: No candidate locations found for key={0}", (object) cacheKey);
            lock (stats.WriterLock)
            {
              stats.NotFound += incrementForCompletionStats;
              stats.RegionsCompleted += incrementForCompletionStats;
            }
          }
          else if (Enumerable.Count<GeoResolution>((IEnumerable<GeoResolution>) geoResolutionArray) == 1)
          {
            flag = true;
            smartPickIndex = 0;
            resolutionType = latLon.HighConfidence ? GeoAmbiguity.Resolution.SingleMatchHighConf : GeoAmbiguity.Resolution.SingleMatchMediumConf;
            lock (stats.WriterLock)
              stats.Resolved += incrementForCompletionStats;
          }
          else
          {
            smartPickIndex = -1;
            resolutionType = GeoAmbiguity.Resolution.Deferred;
            flag = false;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Lookup from cache: Defer resolution of ambiguous key '{0}': {1} candidate matches", (object) cacheKey, (object) Enumerable.Count<GeoResolution>((IEnumerable<GeoResolution>) geoResolutionArray));
            lock (stats.WriterLock)
              stats.AmbiguousResolved += incrementForCompletionStats;
          }
          if (flag)
          {
            lat[geoAccessorIndex] = geoResolutionArray[smartPickIndex].Lat;
            lon[geoAccessorIndex] = geoResolutionArray[smartPickIndex].Lon;
          }
          if (Enumerable.Count<GeoResolution>((IEnumerable<GeoResolution>) geoResolutionArray) >= 1)
          {
            List<string> list = new List<string>();
            for (int index = 0; index < Enumerable.Count<GeoResolution>((IEnumerable<GeoResolution>) geoResolutionArray); ++index)
            {
              string key = geoResolutionArray[index].AddressFields[5];
              if (!string.IsNullOrWhiteSpace(key) && !Enumerable.Contains<string>((IEnumerable<string>) list, key, (IEqualityComparer<string>) StringComparer.Ordinal))
              {
                lock (countryCount)
                {
                  if (countryCount.ContainsKey(key))
                  {
                    Dictionary<string, int> local_23;
                    string local_24;
                    (local_23 = countryCount)[local_24 = key] = local_23[local_24] + 1;
                  }
                  else
                    countryCount[key] = 1;
                }
                list.Add(key);
              }
            }
          }
          ambiguities[geoAccessorIndex] = new GeoAmbiguity(cacheKey, geoField, geoValues == null ? (string) null : geoValues[0], geoValues == null ? (string) null : geoValues[1], geoValues == null ? (string) null : geoValues[2], geoValues == null ? (string) null : geoValues[3], geoValues == null ? (string) null : geoValues[4], geoValues == null ? (string) null : geoValues[5], queryByAddressValue, resolutionType, smartPickIndex, Enumerable.ToList<GeoResolution>((IEnumerable<GeoResolution>) geoResolutionArray));
          lock (stats.WriterLock)
          {
            stats.CacheHits += incrementForCompletionStats;
            stats.Completed += incrementForCompletionStats;
          }
          if (flag)
          {
            if (latLonResolvedCallback != null)
            {
              if (!cancellationToken.IsCancellationRequested)
              {
                try
                {
                  latLonResolvedCallback(context, rowIndex, nextRowToGeocode, geoAccessorIndex);
                }
                catch (Exception ex)
                {
                  VisualizationTraceSource.Current.Fail("ResolveLatLon(): Exception calling LatLonResolveCallback (after cache lookup) ignored, query param=" + cacheKey, ex);
                }
              }
            }
          }
          try
          {
            countdownEvent.Signal();
            return;
          }
          catch (ObjectDisposedException ex)
          {
            return;
          }
        }
      }
      this.SubmitLatLonRequest((object) new BingLatLonCache.WebRequestState()
      {
        WebRequest = (HttpWebRequest) null,
        UrlParameters = urlParameters,
        GeoAccessorIndex = geoAccessorIndex,
        RowIndex = rowIndex,
        NextRowToGeocode = nextRowToGeocode,
        GeoValues = geoValues,
        QueryByAddressValue = queryByAddressValue,
        GeoField = geoField,
        IncrementForCompletionStats = incrementForCompletionStats,
        CacheKey = cacheKey,
        Lat = lat,
        Lon = lon,
        BoundingBoxes = boundingBoxes,
        Ambiguities = ambiguities,
        CountryCount = countryCount,
        Stats = stats,
        Try = 0,
        CancellationToken = cancellationToken,
        WaitArray = waitArray,
        SemaphoreIndexInWaitArray = semaphoreIndexInWaitArray,
        CountdownEvent = countdownEvent,
        Timer = (System.Threading.Timer) null,
        InResolveCallback = false,
        LatLonResolvedCallback = latLonResolvedCallback,
        Context = context
      });
    }

    private void SubmitLatLonRequest(object context)
    {
      BingLatLonCache.WebRequestState webRequestState = context as BingLatLonCache.WebRequestState;
      bool flag = false;
      if (webRequestState.Timer != null)
      {
        webRequestState.Timer.Dispose();
        webRequestState.Timer = (System.Threading.Timer) null;
        flag = true;
      }
      int num1 = -1;
      int num2;
      try
      {
        num2 = WaitHandle.WaitAny(webRequestState.WaitArray);
      }
      catch (ObjectDisposedException ex1)
      {
        if (num1 == webRequestState.SemaphoreIndexInWaitArray)
        {
          try
          {
            this.requestSemaphore.Release();
          }
          catch (SemaphoreFullException ex2)
          {
          }
        }
        VisualizationTraceSource.Current.TraceEvent((TraceEventType) (flag ? 8 : 2), 0, "A handle in the wait array was closed (user presumably cancelled earlier), timer callback={0}, query param={1}", flag, (object) webRequestState.UrlParameters);
        return;
      }
      if (webRequestState.CancellationToken.IsCancellationRequested)
      {
        if (num2 == webRequestState.SemaphoreIndexInWaitArray)
        {
          try
          {
            this.requestSemaphore.Release();
          }
          catch (SemaphoreFullException ex)
          {
          }
        }
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Caller cancelled Bing Maps request processing, query param={0}", (object) webRequestState.UrlParameters);
      }
      else
      {
        HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequestHelper.CreateWebRequest(string.Format("{0}{1}&o=xml&key={2}&c={3}", (object) this.bingMapsRootUrl, (object) webRequestState.UrlParameters, (object) this.bingMapsKey, (object) this.CultureForRequests));
        httpWebRequest.Method = "GET";
        httpWebRequest.Timeout = this.bingMapsRequestTimeoutinSeconds;
        httpWebRequest.UseDefaultCredentials = true;
        try
        {
          lock (webRequestState.Stats.WriterLock)
            ++webRequestState.Stats.BingQueries;
          webRequestState.WebRequest = httpWebRequest;
          if (flag)
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Retry calling Bing Maps, query param={0}, tryCount={1} of {2}", (object) webRequestState.UrlParameters, (object) webRequestState.Try, (object) 5);
          httpWebRequest.BeginGetResponse(new AsyncCallback(this.ProcessLatLonResponse), (object) webRequestState);
        }
        catch (Exception ex1)
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception calling Bing Maps, query param={0}, proxy={1}: {2}", (object) webRequestState.UrlParameters, (object) httpWebRequest.Proxy.GetProxy(httpWebRequest.RequestUri).AbsoluteUri.Split(new string[1]
          {
            "key="
          }, StringSplitOptions.None)[0], (object) ex1);
          webRequestState.Lat[webRequestState.GeoAccessorIndex] = double.NaN;
          webRequestState.Lon[webRequestState.GeoAccessorIndex] = double.NaN;
          webRequestState.Ambiguities[webRequestState.GeoAccessorIndex] = (GeoAmbiguity) null;
          lock (webRequestState.Stats.WriterLock)
          {
            ++webRequestState.Stats.ExceptionsQueryingBing;
            ++webRequestState.Stats.QueryFailed;
            webRequestState.Stats.Completed += webRequestState.IncrementForCompletionStats;
            webRequestState.Stats.RegionsCompleted += webRequestState.IncrementForCompletionStats;
          }
          try
          {
            webRequestState.CountdownEvent.Signal();
          }
          catch (ObjectDisposedException ex2)
          {
          }
          try
          {
            this.requestSemaphore.Release();
          }
          catch (SemaphoreFullException ex2)
          {
          }
        }
      }
    }

    private void ProcessLatLonResponse(IAsyncResult asynchronousResult)
    {
      BingLatLonCache.WebRequestState webRequestState = (BingLatLonCache.WebRequestState) asynchronousResult.AsyncState;
      HttpStatusCode httpStatusCode = (HttpStatusCode) 0;
      StringBuilder stringBuilder = new StringBuilder();
      List<GeoResolution> ambiguities = new List<GeoResolution>();
      bool flag1 = webRequestState.BoundingBoxes != null;
      List<GeoResolutionBorder> list1 = flag1 ? new List<GeoResolutionBorder>() : (List<GeoResolutionBorder>) null;
      bool flag2 = true;
      List<string> list2 = new List<string>();
      try
      {
        HttpWebResponse httpWebResponse = webRequestState.WebRequest.EndGetResponse(asynchronousResult) as HttpWebResponse;
        httpStatusCode = httpWebResponse.StatusCode;
        int num1 = 0;
        int num2 = 0;
        using (Stream responseStream = httpWebResponse.GetResponseStream())
        {
          XmlDocument xmlDocument = new XmlDocument();
          xmlDocument.Load(responseStream);
          XmlElement documentElement = xmlDocument.DocumentElement;
          XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
          nsmgr.AddNamespace("bm", "http://schemas.microsoft.com/search/local/ws/rest/v1");
          num2 = xmlDocument.SelectNodes("/bm:Response/bm:ResourceSets/bm:ResourceSet/bm:Resources/bm:Location", nsmgr).Count;
          XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/bm:Response/bm:ResourceSets/bm:ResourceSet/bm:Resources/bm:Location[bm:Confidence = \"High\" and (bm:Entity = \"Roadblock\" or not(bm:MatchCode = \"UpHierarchy\"))]", nsmgr);
          flag2 = xmlNodeList.Count != 0;
          if (!flag2)
            xmlNodeList = xmlDocument.SelectNodes("/bm:Response/bm:ResourceSets/bm:ResourceSet/bm:Resources/bm:Location[bm:Confidence = \"Medium\" and (bm:Entity = \"Roadblock\" or not(bm:MatchCode = \"UpHierarchy\"))]", nsmgr);
          num1 = xmlNodeList.Count;
          foreach (XmlNode xmlNode1 in xmlNodeList)
          {
            XmlNode xmlNode2 = xmlNode1.SelectSingleNode("bm:Point/bm:Latitude", nsmgr);
            XmlNode xmlNode3 = xmlNode1.SelectSingleNode("bm:Point/bm:Longitude", nsmgr);
            if (xmlNode2 != null && xmlNode3 != null)
            {
              string str1 = string.Empty;
              XmlNode xmlNode4 = xmlNode1.SelectSingleNode("bm:Address/bm:FormattedAddress", nsmgr);
              XmlNode xmlNode5 = flag1 ? xmlNode1.SelectSingleNode("bm:BoundingBox", nsmgr) : (XmlNode) null;
              XmlNode xmlNode6 = xmlNode1.SelectSingleNode("bm:EntityType", nsmgr);
              string str2 = string.Empty;
              List<XmlNode> list3 = new List<XmlNode>();
              string[] strArray = new string[6];
              foreach (string str3 in BingLatLonCache.AddressNodeChildNames)
                list3.Add(xmlNode1.SelectSingleNode("bm:Address/bm:" + str3, nsmgr));
              try
              {
                double d1 = Convert.ToDouble(xmlNode2.InnerText, (IFormatProvider) CultureInfo.InvariantCulture);
                double d2 = Convert.ToDouble(xmlNode3.InnerText, (IFormatProvider) CultureInfo.InvariantCulture);
                if (!double.IsNaN(d1))
                {
                  if (!double.IsNaN(d2))
                  {
                    if (xmlNode6 != null)
                      str2 = xmlNode6.InnerText.Trim();
                    if (xmlNode4 != null)
                      str1 = xmlNode4.InnerText;
                    for (int index = 0; index < 6; ++index)
                    {
                      if (list3[index] != null)
                        strArray[index] = list3[index].InnerText;
                    }
                    string key = strArray[5];
                    if (!string.IsNullOrWhiteSpace(key) && !Enumerable.Contains<string>((IEnumerable<string>) list2, key, (IEqualityComparer<string>) StringComparer.Ordinal))
                    {
                      lock (webRequestState.CountryCount)
                      {
                        if (webRequestState.CountryCount.ContainsKey(key))
                        {
                          Dictionary<string, int> local_67;
                          string local_68;
                          (local_67 = webRequestState.CountryCount)[local_68 = key] = local_67[local_68] + 1;
                        }
                        else
                          webRequestState.CountryCount[key] = 1;
                        list2.Add(key);
                      }
                    }
                    double num3 = double.NaN;
                    double num4 = double.NaN;
                    double num5 = double.NaN;
                    double num6 = double.NaN;
                    if (xmlNode5 != null)
                    {
                      XmlNode xmlNode7 = xmlNode5.SelectSingleNode("bm:WestLongitude", nsmgr);
                      if (xmlNode7 != null)
                        num3 = Convert.ToDouble(xmlNode7.InnerText, (IFormatProvider) CultureInfo.InvariantCulture);
                      XmlNode xmlNode8 = xmlNode5.SelectSingleNode("bm:NorthLatitude", nsmgr);
                      if (xmlNode8 != null)
                        num4 = Convert.ToDouble(xmlNode8.InnerText, (IFormatProvider) CultureInfo.InvariantCulture);
                      XmlNode xmlNode9 = xmlNode5.SelectSingleNode("bm:EastLongitude", nsmgr);
                      if (xmlNode9 != null)
                        num5 = Convert.ToDouble(xmlNode9.InnerText, (IFormatProvider) CultureInfo.InvariantCulture);
                      XmlNode xmlNode10 = xmlNode5.SelectSingleNode("bm:SouthLatitude", nsmgr);
                      if (xmlNode10 != null)
                        num6 = Convert.ToDouble(xmlNode10.InnerText, (IFormatProvider) CultureInfo.InvariantCulture);
                    }
                    if (flag1)
                      list1.Add(new GeoResolutionBorder()
                      {
                        East = num5,
                        West = num3,
                        North = num4,
                        South = num6
                      });
                    ambiguities.Add(new GeoResolution()
                    {
                      Lat = d1,
                      Lon = d2,
                      EntityType = str2,
                      FormattedAddress = str1,
                      AddressFields = strArray
                    });
                    stringBuilder.AppendFormat("{0}: addr='{1}', entityType={2}", (object) ambiguities.Count, (object) str1, (object) str2);
                    for (int index = 0; index < 6; ++index)
                    {
                      if (strArray[index] != null)
                        stringBuilder.AppendFormat("{0}={1}, ", (object) BingLatLonCache.AddressNodeChildNames[index], (object) strArray[index]);
                    }
                    stringBuilder.Append("; ");
                  }
                }
              }
              catch (Exception ex)
              {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Ignoring location node: Exception processing lat/lon/boundingBoxes in location node, query param={0}, exception={1}", (object) webRequestState.UrlParameters, (object) ((object) ex).ToString());
              }
            }
          }
        }
        bool flag3;
        int smartPickIndex;
        GeoAmbiguity.Resolution resolutionType;
        if (ambiguities.Count == 1)
        {
          flag3 = true;
          smartPickIndex = 0;
          resolutionType = flag2 ? GeoAmbiguity.Resolution.SingleMatchHighConf : GeoAmbiguity.Resolution.SingleMatchMediumConf;
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Single {5} key '{0}':  numCandidateNodes={1}, num location nodes returned in response={2}; num pruned choices={3}: {4}", (object) webRequestState.CacheKey, (object) num1, (object) num2, (object) ambiguities.Count, (object) ((object) stringBuilder).ToString(), flag2 ? (object) "hiConf" : (object) "medConf");
          lock (webRequestState.Stats.WriterLock)
            webRequestState.Stats.Resolved += webRequestState.IncrementForCompletionStats;
        }
        else if (ambiguities.Count > 1)
        {
          flag3 = false;
          smartPickIndex = -1;
          resolutionType = GeoAmbiguity.Resolution.Deferred;
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Ambiguous {5} key '{0}':  numCandidateNodes={1}, num location nodes returned in response={2}; num pruned choices={3}: {4}", (object) webRequestState.CacheKey, (object) num1, (object) num2, (object) ambiguities.Count, (object) ((object) stringBuilder).ToString(), flag2 ? (object) "hiConf" : (object) "medConf");
          lock (webRequestState.Stats.WriterLock)
          {
            webRequestState.Stats.Ambiguous += webRequestState.IncrementForCompletionStats;
            webRequestState.Stats.AmbiguousResolved += webRequestState.IncrementForCompletionStats;
          }
        }
        else
        {
          flag3 = false;
          smartPickIndex = -1;
          resolutionType = GeoAmbiguity.Resolution.NoMatch;
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "No candidate locations found for query param={0}, numCandidateNodes={1}, num location nodes returned in response={2}", (object) webRequestState.UrlParameters, (object) num1, (object) num2);
          lock (webRequestState.Stats.WriterLock)
          {
            webRequestState.Stats.NotFound += webRequestState.IncrementForCompletionStats;
            webRequestState.Stats.RegionsCompleted += webRequestState.IncrementForCompletionStats;
          }
        }
        if (flag3)
        {
          webRequestState.Lat[webRequestState.GeoAccessorIndex] = ambiguities[smartPickIndex].Lat;
          webRequestState.Lon[webRequestState.GeoAccessorIndex] = ambiguities[smartPickIndex].Lon;
        }
        this.isDirty = true;
        this.cache[webRequestState.CacheKey] = new BingLatLonCache.LatLon()
        {
          GeoResolutions = ambiguities.ToArray(),
          HighConfidence = flag2
        };
        webRequestState.Ambiguities[webRequestState.GeoAccessorIndex] = new GeoAmbiguity(webRequestState.CacheKey, webRequestState.GeoField, webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[0], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[1], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[2], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[3], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[4], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[5], webRequestState.QueryByAddressValue, resolutionType, smartPickIndex, ambiguities);
        if (flag1)
          webRequestState.BoundingBoxes[webRequestState.GeoAccessorIndex] = list1;
        lock (webRequestState.Stats.WriterLock)
          webRequestState.Stats.Completed += webRequestState.IncrementForCompletionStats;
        if (flag3)
        {
          if (webRequestState.LatLonResolvedCallback != null)
          {
            if (!webRequestState.CancellationToken.IsCancellationRequested)
            {
              webRequestState.InResolveCallback = true;
              webRequestState.LatLonResolvedCallback(webRequestState.Context, webRequestState.RowIndex, webRequestState.NextRowToGeocode, webRequestState.GeoAccessorIndex);
              webRequestState.InResolveCallback = false;
            }
          }
        }
        try
        {
          webRequestState.CountdownEvent.Signal();
        }
        catch (ObjectDisposedException ex)
        {
        }
      }
      catch (WebException ex1)
      {
        try
        {
          bool flag3 = false;
          bool flag4 = true;
          bool flag5 = false;
          ++webRequestState.Try;
          int dueTime;
          lock (this.random)
            dueTime = this.random.Next(1000, 5000 << webRequestState.Try - 1);
          if (ex1.InnerException is IOException && webRequestState.Try < 5)
            flag3 = true;
          else if (ex1.Status == WebExceptionStatus.ProtocolError && ex1.Response is HttpWebResponse)
          {
            httpStatusCode = ((HttpWebResponse) ex1.Response).StatusCode;
            switch (httpStatusCode)
            {
              case HttpStatusCode.InternalServerError:
                flag3 = webRequestState.Try < 5;
                break;
              case HttpStatusCode.NotFound:
                flag3 = false;
                flag4 = false;
                flag5 = true;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WebException calling Bing Maps or processing the response, query param={0}, statusCode=NotFound", (object) webRequestState.UrlParameters);
                lock (webRequestState.Stats.WriterLock)
                {
                  webRequestState.Stats.NotFound += webRequestState.IncrementForCompletionStats;
                  break;
                }
            }
          }
          else
          {
            switch (ex1.Status)
            {
              case WebExceptionStatus.NameResolutionFailure:
              case WebExceptionStatus.ConnectFailure:
              case WebExceptionStatus.ReceiveFailure:
              case WebExceptionStatus.SendFailure:
              case WebExceptionStatus.PipelineFailure:
              case WebExceptionStatus.RequestCanceled:
              case WebExceptionStatus.ConnectionClosed:
              case WebExceptionStatus.KeepAliveFailure:
              case WebExceptionStatus.ProxyNameResolutionFailure:
                flag3 = webRequestState.Try < 5;
                break;
            }
          }
          if (flag4)
          {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "WebException calling Bing Maps or processing the response, query param={0}, statusCode={1}, retry={2}, tryCount={3} of {4}, retry_delay={5} msec, proxy={6}: {7}", (object) webRequestState.UrlParameters, (object) httpStatusCode, flag3, (object) webRequestState.Try, (object) 5, (object) dueTime, (object) webRequestState.WebRequest.Proxy.GetProxy(webRequestState.WebRequest.RequestUri).AbsoluteUri.Split(new string[1]
            {
              "key="
            }, StringSplitOptions.None)[0], (object) ex1);
            lock (webRequestState.Stats.WriterLock)
              ++webRequestState.Stats.ExceptionsQueryingBing;
          }
          webRequestState.Lat[webRequestState.GeoAccessorIndex] = double.NaN;
          webRequestState.Lon[webRequestState.GeoAccessorIndex] = double.NaN;
          webRequestState.Ambiguities[webRequestState.GeoAccessorIndex] = (GeoAmbiguity) null;
          if (flag3)
          {
            webRequestState.WebRequest = (HttpWebRequest) null;
            lock (webRequestState.Stats.WriterLock)
              ++webRequestState.Stats.BingRetries;
            webRequestState.Timer = new System.Threading.Timer(new TimerCallback(this.SubmitLatLonRequest), (object) webRequestState, dueTime, -1);
          }
          else
          {
            if (!flag5)
            {
              lock (webRequestState.Stats.WriterLock)
                ++webRequestState.Stats.QueryFailed;
            }
            int smartPickIndex = -1;
            GeoAmbiguity.Resolution resolutionType = GeoAmbiguity.Resolution.NoMatch;
            webRequestState.Ambiguities[webRequestState.GeoAccessorIndex] = new GeoAmbiguity(webRequestState.CacheKey, webRequestState.GeoField, webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[0], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[1], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[2], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[3], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[4], webRequestState.GeoValues == null ? (string) null : webRequestState.GeoValues[5], webRequestState.QueryByAddressValue, resolutionType, smartPickIndex, ambiguities);
            lock (webRequestState.Stats.WriterLock)
            {
              webRequestState.Stats.Completed += webRequestState.IncrementForCompletionStats;
              webRequestState.Stats.RegionsCompleted += webRequestState.IncrementForCompletionStats;
            }
            try
            {
              webRequestState.CountdownEvent.Signal();
            }
            catch (ObjectDisposedException ex2)
            {
            }
          }
        }
        catch (Exception ex2)
        {
        }
      }
      catch (Exception ex1)
      {
        try
        {
          if (webRequestState.InResolveCallback)
          {
            VisualizationTraceSource.Current.Fail("ProcessLatLonResponse(): Exception calling LatLonResolveCallback ignored, query param=" + webRequestState.UrlParameters, ex1);
          }
          else
          {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Exception calling Bing Maps or processing the response, query param={0}, statusCode={1}: {2}", (object) webRequestState.UrlParameters, (object) httpStatusCode, (object) ex1);
            webRequestState.Lat[webRequestState.GeoAccessorIndex] = double.NaN;
            webRequestState.Lon[webRequestState.GeoAccessorIndex] = double.NaN;
            webRequestState.Ambiguities[webRequestState.GeoAccessorIndex] = (GeoAmbiguity) null;
            lock (webRequestState.Stats.WriterLock)
            {
              ++webRequestState.Stats.QueryFailed;
              ++webRequestState.Stats.ExceptionsQueryingBing;
              webRequestState.Stats.Completed += webRequestState.IncrementForCompletionStats;
              webRequestState.Stats.RegionsCompleted += webRequestState.IncrementForCompletionStats;
            }
            try
            {
              webRequestState.CountdownEvent.Signal();
            }
            catch (ObjectDisposedException ex2)
            {
            }
          }
        }
        catch (Exception ex2)
        {
        }
      }
      finally
      {
        try
        {
          this.requestSemaphore.Release();
        }
        catch (SemaphoreFullException ex)
        {
        }
      }
    }

    public delegate string QueryParameterBuilder(int i, out string cacheKey, out string[] geoValues, out string queryByAddressValue);

    private class GetLatLonByAddressAsyncRequestState
    {
      public int QueryResultCount;

      public GeoField Geo { get; set; }

      public int FirstGeoCol { get; set; }

      public Func<int, int, string> GeoValuesAccessor { get; set; }

      public Func<int, int> GeoRowsAccessor { get; set; }

      public double[] Lat { get; set; }

      public double[] Lon { get; set; }

      public GeoResolutionBorder[] BoundingBox { get; set; }

      public GeoAmbiguity[] Ambiguities { get; set; }

      public CancellationToken CancellationToken { get; set; }

      public CompletionStats CompletionStats { get; set; }

      public Action<object, int, int, int> LatLonResolvedCallback { get; set; }

      public Action<object> CompletionCallback { get; set; }

      public object Context { get; set; }
    }

    private class GetLatLonByQueryAsyncRequestState
    {
      public double Lat;
      public double Lon;
      public GeoResolutionBorder BoundingBox;
      public GeoAmbiguity Ambiguity;

      public string GeoQuery { get; set; }

      public CancellationToken CancellationToken { get; set; }

      public Action<object, bool, double, double, GeoResolutionBorder, GeoAmbiguity> CompletionCallback { get; set; }

      public object Context { get; set; }
    }

    private class WebRequestState
    {
      public HttpWebRequest WebRequest;
      public string UrlParameters;
      public int GeoAccessorIndex;
      public int RowIndex;
      public int NextRowToGeocode;
      public string[] GeoValues;
      public string QueryByAddressValue;
      public GeoField GeoField;
      public int IncrementForCompletionStats;
      public string CacheKey;
      public double[] Lat;
      public double[] Lon;
      public List<GeoResolutionBorder>[] BoundingBoxes;
      public GeoAmbiguity[] Ambiguities;
      public Dictionary<string, int> CountryCount;
      public CompletionStats Stats;
      public int Try;
      public CancellationToken CancellationToken;
      public WaitHandle[] WaitArray;
      public int SemaphoreIndexInWaitArray;
      public CountdownEvent CountdownEvent;
      public System.Threading.Timer Timer;

      public bool InResolveCallback { get; set; }

      public Action<object, int, int, int> LatLonResolvedCallback { get; set; }

      public object Context { get; set; }
    }

    [Serializable]
    public class LatLon
    {
      public const int UnresolvedAmbiguity = -1;
      public GeoResolution[] GeoResolutions;
      [XmlAttribute("hc")]
      public bool HighConfidence;
    }

    [Serializable]
    public struct SerializableLatLonCacheItem
    {
      [XmlAttribute("k")]
      public string Key;
      public BingLatLonCache.LatLon Value;
    }
  }
}
