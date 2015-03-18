using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public sealed class BingRegionProvider : IRegionProvider, IDisposable
    {
        private readonly BingRegionProvider.ConcurrentDictionary RegionMap = new BingRegionProvider.ConcurrentDictionary();
        private readonly BingRegionProvider.SourcesDictionary sourcesMap = new BingRegionProvider.SourcesDictionary();
        private const string RegionCacheRoot = "RegionCache";
        private const string RegionSourceRoot = "RegionSources";
        private const int MaxRps = 40;
        private const int MaxTries = 5;
        private const int DelayInterval = 3000;
        private const int TimeOut = 30;
        private const string RegionUrl = "https://platform.bing.com/geo/spatial/v1/public/geodata?SpatialFilter=GetBoundary({1},{2},{3},'{4}',{5},{6},'{7}','{8}')&key={0}&fb={9}&$format=json";
        private readonly SemaphoreSlim requestSemaphore;
        private readonly SemaphoreSlim requestPerSecSemaphore;
        private readonly HttpClient webClient;
        private bool isDisposed;
        private volatile bool isDirty;

        public string SerializedRegionProvider
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                StringBuilder output = stringBuilder;
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                using (XmlWriter writer = XmlWriter.Create(output, settings))
                {
                    new DataContractSerializer(typeof(BingRegionProvider.ConcurrentDictionary), RegionCacheRoot, string.Empty).WriteObject(writer, this.RegionMap);
                    new DataContractSerializer(typeof(BingRegionProvider.SourcesDictionary), RegionSourceRoot, string.Empty).WriteObject(writer, this.sourcesMap);
                }
                this.isDirty = false;
                return stringBuilder.ToString();
            }
        }

        public IEnumerable<string> Sources
        {
            get
            {
                if (this.sourcesMap != null)
                    return this.sourcesMap.Select(copyright => copyright.Value);
                return Enumerable.Empty<string>();
            }
        }

        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
            set
            {
                this.isDirty = value;
            }
        }

        public BingRegionProvider(int concurrencyLimit)
        {
            this.requestSemaphore = new SemaphoreSlim(concurrencyLimit, concurrencyLimit);
            this.requestPerSecSemaphore = new SemaphoreSlim(MaxRps, MaxRps);
            this.webClient = WebRequestHelper.CreateHttpClient();
            this.webClient.Timeout = TimeSpan.FromSeconds(TimeOut);
            BingRegionProvider.RequestDispatcher(this);
        }

        public BingRegionProvider(string xml, int concurrencyLimit)
            : this(concurrencyLimit)
        {
            StringReader stringReader = new StringReader(xml);
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };
            using (XmlReader reader = XmlReader.Create(stringReader, settings))
            {
                this.RegionMap = (BingRegionProvider.ConcurrentDictionary)new DataContractSerializer(typeof(BingRegionProvider.ConcurrentDictionary), RegionCacheRoot, string.Empty).ReadObject(reader);
                if (!reader.IsStartElement(RegionSourceRoot))
                    return;
                this.sourcesMap = (BingRegionProvider.SourcesDictionary)new DataContractSerializer(typeof(BingRegionProvider.SourcesDictionary), RegionSourceRoot, string.Empty).ReadObject(reader);
            }
        }

        public Task<List<RegionData>> GetRegionAsync(double lat, double lon, int lod, Microsoft.Data.Visualization.Engine.EntityType entityType, CancellationToken token, bool getAllPolygons = true, bool getMetadata = false, bool upLevel = true)
        {
            bool flag = ((!double.IsNaN(lat) && !double.IsNaN(lon)) && (lod >= 0)) && (lod <= 3);
            TaskCompletionSource<List<RegionData>> source = new TaskCompletionSource<List<RegionData>>();
            RegionKey key = new RegionKey
            {
                Latitude = lat,
                Longitude = lon,
                LevelOfDepth = lod,
                EntityType = entityType,
                UserRegion = new RegionInfo(Thread.CurrentThread.CurrentCulture.LCID).TwoLetterISORegionName,
                Language = Resources.Culture.Name
            };
            RegionDataList list = null;
            if ((!token.IsCancellationRequested && flag) && !this.RegionMap.TryGetValue(key, out list))
            {
                Func<Task<List<RegionData>>> taskCreator =
                    () => this.FetchRegionData(key, lat, lon, lod, entityType, getAllPolygons, getMetadata, upLevel, token); ;

                Action<TaskCompletionSource<List<RegionData>>, Exception> exceptionHandler = delegate(TaskCompletionSource<List<RegionData>> tcs, Exception ex)
                {
                    tcs.TrySetResult(null);
                    VisualizationTraceSource.Current.TraceInformation(string.Format("Giving up on fetching region for key {0} due to error {1}.", key, ex));
                };

                Action<TaskCompletionSource<List<RegionData>>> cancellationHandler = delegate(TaskCompletionSource<List<RegionData>> tcs)
                {
                    tcs.TrySetResult(null);
                    VisualizationTraceSource.Current.TraceInformation(string.Format("Cancelling process to fetch region for key {0}.", key));
                };
                Action cleanupHandler = delegate
                {
                    try
                    {
                        this.requestSemaphore.Release();
                    }
                    catch (SemaphoreFullException)
                    {
                    }
                };
                taskCreator().WithRetry<List<RegionData>>(
                    source, taskCreator, token, MaxTries, DelayInterval, 
                    exceptionHandler, cancellationHandler, cleanupHandler, this.DoRetry);
            }
            else
            {
                source.TrySetResult(list);
            }
            return source.Task;

        }

        public void Reset()
        {
            this.webClient.CancelPendingRequests();
            this.RegionMap.Clear();
            this.sourcesMap.Clear();
            this.isDirty = true;
        }

        private bool DoRetry(Exception ex)
        {
            if (ex == null)
                return true;
            if (ex is AggregateException)
                ex = ex.GetBaseException();
            if (!(ex is HttpRequestException))
                return ex is WebException;
            return true;
        }

        private async Task<List<RegionData>> FetchRegionData(BingRegionProvider.RegionKey key, double lat, double lon, int lod, Microsoft.Data.Visualization.Engine.EntityType entityType, bool getAllPolygons, bool getMetadata, bool upLevel, CancellationToken token)
        {
            await this.requestSemaphore.WaitAsync(token);
            await this.requestPerSecSemaphore.WaitAsync(token);
            HttpResponseMessage response;
            try
            {
                response = await this.webClient.GetAsync(string.Format(CultureInfo.InvariantCulture, RegionUrl, "AutmxuJvVVVQyluwfF-Le9A6WQ_ypucXcJbzx5Rwf5u8on47kJRDu19BzV4kZlq9", lat, lon, lod, entityType, Convert.ToInt32(getAllPolygons), Convert.ToInt32(getMetadata), key.Language, key.UserRegion, Convert.ToInt32(upLevel)), token);
            }
            catch (TaskCanceledException ex)
            {
                if (!ex.CancellationToken.IsCancellationRequested)
                    throw new HttpRequestException("Http request cancelled due to timeout.", ex);
                throw;
            }
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();
            response.Dispose();
            return this.ProcessResponse(jsonString, key);
        }

        private List<RegionData> ProcessResponse(string jsonString, BingRegionProvider.RegionKey key)
        {
            try
            {
                RegionResponse regionResponse = new JavaScriptSerializer().Deserialize<RegionResponse>(jsonString);
                BingRegionProvider.RegionDataList regionDataList = new BingRegionProvider.RegionDataList();
                if (regionResponse != null && regionResponse.D != null)
                {
                    foreach (Result result in regionResponse.D.Results)
                    {
                        foreach (Primitive primitive in result.Primitives)
                        {
                            string[] strArray = primitive.Shape.Split(',');
                            for (int index = 1; index < strArray.Length; ++index)
                                regionDataList.Add(new RegionData()
                                {
                                    PolygonId = primitive.PrimitiveID,
                                    Ring = strArray[index]
                                });
                        }
                        if (result.Copyright != null && result.Copyright.Sources != null)
                        {
                            foreach (Source source in result.Copyright.Sources)
                                this.sourcesMap.TryAdd(source.SourceID, source.Copyright);
                        }
                    }
                }
                this.RegionMap.TryAdd(key, regionDataList);
                this.isDirty = true;
                return regionDataList;
            }
            catch (ArgumentException ex)
            {
                throw new HttpRequestException(string.Format("Failed to parse Json response for key {0}.", key));
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(string.Format("Failed to process region response for key {0}.", key), ex);
                throw;
            }
        }

        private static async void RequestDispatcher(BingRegionProvider provider)
        {
            try
            {
                await Task.Delay(1000).ConfigureAwait(false);
                if (!provider.isDisposed)
                {
                    int releaseCount = MaxRps - provider.requestPerSecSemaphore.CurrentCount;
                    if (releaseCount > 0)
                        provider.requestPerSecSemaphore.Release(releaseCount);
                    BingRegionProvider.RequestDispatcher(provider);
                }
            }
            catch (ObjectDisposedException ex)
            {
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail("Failed to reset RPS semaphore.", ex);
            }
        }

        public void Dispose()
        {
            if (this.isDisposed)
                return;
            try
            {
                this.Reset();
                this.isDirty = false;
                this.requestSemaphore.Dispose();
                this.requestPerSecSemaphore.Dispose();
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail("Errors occurred while disposing Bing region provider.", ex);
            }
            this.isDisposed = true;
        }

        [CollectionDataContract(ItemName = "rpolygons", Namespace = "")]
        internal class RegionDataList : List<RegionData>
        {
        }

        [CollectionDataContract(ItemName = "rsource", KeyName = "rsourceid", Namespace = "", ValueName = "rsourcename")]
        internal class SourcesDictionary : ConcurrentDictionary<int, string>
        {
        }

        [CollectionDataContract(ItemName = "rentry", KeyName = "rentrykey", Namespace = "", ValueName = "rentryvalue")]
        internal class ConcurrentDictionary : ConcurrentDictionary<BingRegionProvider.RegionKey, BingRegionProvider.RegionDataList>
        {
        }

        [DataContract(Name = "rkey", Namespace = "")]
        internal class RegionKey
        {
            private const double tolerance = 1E-06;

            [DataMember(Name = "lat", Order = 1)]
            public double Latitude { get; set; }

            [DataMember(Name = "lon", Order = 2)]
            public double Longitude { get; set; }

            [DataMember(Name = "lod", Order = 3)]
            public int LevelOfDepth { get; set; }

            [DataMember(Name = "type", Order = 4)]
            public Microsoft.Data.Visualization.Engine.EntityType EntityType { get; set; }

            [DataMember(Name = "lang", Order = 5)]
            public string Language { get; set; }

            [DataMember(Name = "ur", Order = 6)]
            public string UserRegion { get; set; }

            public override bool Equals(object obj)
            {
                BingRegionProvider.RegionKey regionKey = obj as BingRegionProvider.RegionKey;
                if (regionKey != null && 
                    regionKey.EntityType == this.EntityType && 
                    (regionKey.LevelOfDepth == this.LevelOfDepth && BingRegionProvider.RegionKey.AreEqual(regionKey.Latitude, this.Latitude)) && 
                    (BingRegionProvider.RegionKey.AreEqual(regionKey.Longitude, this.Longitude) && string.Compare(this.Language, regionKey.Language, StringComparison.InvariantCultureIgnoreCase) == 0))
                    return string.Compare(this.UserRegion, regionKey.UserRegion, StringComparison.InvariantCultureIgnoreCase) == 0;
                return false;
            }

            public override int GetHashCode()
            {
                return (int)((this.Latitude.GetHashCode() ^ this.Longitude.GetHashCode() * 31 ^ this.LevelOfDepth.GetHashCode() * 31 * 31 ^ this.EntityType.GetHashCode() * 31 * 31 * 31) & (long)int.MaxValue);
            }

            public override string ToString()
            {
                return string.Format("{0},{1},{2},{3}", this.Latitude, this.Longitude, this.LevelOfDepth, this.EntityType);
            }

            private static bool AreEqual(double actual, double expected)
            {
                return Math.Abs(actual - expected) <= tolerance * Math.Max(Math.Max(Math.Abs(expected), Math.Abs(actual)), 1.0);
            }
        }
    }
}
