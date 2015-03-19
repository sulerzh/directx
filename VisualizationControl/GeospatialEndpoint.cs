using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public static class GeospatialEndpoint
    {
        private const string Geocode = "Geocode";
        private const string CombinedLogo = "CombinedLogo";
        private const string BingLogo = "BingLogo";
        private const string CombinedLogoAerial = "CombinedLogoAerial";
        private const string BingLogoAerial = "BingLogoAerial";
        private const string RoadWithLabels = "RoadWithLabels";
        private const string AerialWithLabels = "AerialWithLabels";
        private const string RoadWithoutLabels = "RoadWithoutLabels";
        private const string AerialWithoutLabels = "AerialWithoutLabels";
        private const string HttpsUrlFormat = "https://{0}";
        private const string HttpUrlFormat = "http://{0}";
        private const string EndpointServiceUrl = "https://dev.virtualearth.net/REST/V1/GeospatialEndpoint/{0}/{1}?key={2}";

        // 2015-02-12 zsl, set default url to support part function on ui
        private const string DefaultRoadWithLablesUrl = "http://ak.dynamic.t{0-3}.tiles.virtualearth.net/comp/ch/{quadkey}?mkt=zh-Hans&it=G,L&shading=hill&og=74&n=z";
        private const string DefaultRoadWithoutLabelsUrl = "http://ak.dynamic.t{0-3}.tiles.virtualearth.net/comp/ch/{quadkey}?mkt=zh-Hans&it=G&shading=hill&og=74&n=z";
        private const string DefaultAerialWithLablesUrl = "http://ak.dynamic.t{0-3}.tiles.virtualearth.net/comp/ch/{quadkey}?mkt=zh-Hans&it=A,G,L&og=74&n=z";
        private const string DefaultAerialWithoutLablesUrl = "http://ecn.t{0-3}.tiles.virtualearth.net/tiles/a{quadkey}.jpeg?g=3262"; 

        public static async Task<EndPoint> FetchServiceEndpointsAsync(string language, string region, CancellationToken cancellationToken)
        {
            string geoCodingServiceEndpoint = null;
            string aerialWithoutLabels = null;
            string aerialWithLabels = null;
            string roadWithoutLabels = null;
            string roadWithLabels = null;
            string combinedLogoUrl = null;
            string bingLogoUrl = null;
            string combinedLogoAerialUrl = null;
            string bingLogoAerialUrl = null;
            EndPoint endPoint;
            try
            {
                EndpointServiceResponse response =
                    await GetBingServiceInfoAsync(language, region, cancellationToken).ConfigureAwait(false);
                response.resourceSets.ForEach((rs => rs.resources.ForEach((r =>
                {
                    if (!r.isSupported)
                        return;
                    r.services.ForEach((s =>
                    {
                        if (s.serviceName.Equals(Geocode, StringComparison.InvariantCultureIgnoreCase))
                        {
                            geoCodingServiceEndpoint = string.Format(HttpsUrlFormat, s.endpoint);
                            Properties.Settings.Default.GeocodingEndPoint = geoCodingServiceEndpoint;
                        }
                        else if (s.serviceName.Equals(CombinedLogo, StringComparison.InvariantCultureIgnoreCase))
                        {
                            combinedLogoUrl = string.Format(HttpsUrlFormat, s.endpoint);
                            Properties.Settings.Default.CombinedLogoEndPoint = combinedLogoUrl;
                        }
                        else if (s.serviceName.Equals(BingLogo, StringComparison.InvariantCultureIgnoreCase))
                        {
                            bingLogoUrl = string.Format(HttpsUrlFormat, s.endpoint);
                            Properties.Settings.Default.BingLogoEndPoint = bingLogoUrl;
                        }
                        else if (s.serviceName.Equals(CombinedLogoAerial, StringComparison.InvariantCultureIgnoreCase))
                        {
                            combinedLogoAerialUrl = string.Format(HttpsUrlFormat, s.endpoint);
                            Properties.Settings.Default.CombinedLogoAerialEndPoint = combinedLogoAerialUrl;
                        }
                        else if (s.serviceName.Equals(BingLogoAerial, StringComparison.InvariantCultureIgnoreCase))
                        {
                            bingLogoAerialUrl = string.Format(HttpsUrlFormat, s.endpoint);
                            Properties.Settings.Default.BingLogoAerialEndPoint = bingLogoAerialUrl;
                        }
                        else if (s.serviceName.Equals(AerialWithoutLabels, StringComparison.InvariantCultureIgnoreCase))
                        {
                            aerialWithoutLabels = string.Format(HttpUrlFormat, s.endpoint);
                            Properties.Settings.Default.AerialWithoutLabelsEndPoint = aerialWithoutLabels;
                        }
                        else if (s.serviceName.Equals(AerialWithLabels, StringComparison.InvariantCultureIgnoreCase))
                        {
                            aerialWithLabels = string.Format(HttpUrlFormat, s.endpoint);
                            Properties.Settings.Default.AerialWithLabelsEndPoint = aerialWithLabels;
                        }
                        else if (s.serviceName.Equals(RoadWithLabels, StringComparison.InvariantCultureIgnoreCase))
                        {
                            roadWithLabels = string.Format(HttpUrlFormat, s.endpoint);
                            Properties.Settings.Default.RoadWithLabelsEndPoint = roadWithLabels;
                        }
                        else
                        {
                            if (!s.serviceName.Equals(RoadWithoutLabels, StringComparison.InvariantCultureIgnoreCase))
                                return;
                            roadWithoutLabels = string.Format(HttpUrlFormat, s.endpoint);
                            Properties.Settings.Default.RoadWithoutLabelsEndPoint = roadWithoutLabels;
                        }
                    }));
                }))));

                // 2015-02-12, set default urls if not obtain from server
                if (roadWithLabels == null)
                {
                    roadWithLabels = DefaultRoadWithLablesUrl;
                }
                if (roadWithoutLabels == null)
                {
                    roadWithoutLabels = DefaultRoadWithoutLabelsUrl;
                }
                if (aerialWithLabels == null)
                {
                    aerialWithLabels = DefaultAerialWithLablesUrl;
                }
                if (aerialWithoutLabels == null)
                {
                    aerialWithoutLabels = DefaultAerialWithoutLablesUrl;
                }

                Properties.Settings.Default.Save();
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Fetched the following endpoints from Geospatial Endpoint Service; Geocoding: '{0}', CombinedLogo: '{1}', MapTiles: '{2}', BingLogo: '{3}', BingLogoAerial: '{4}', CombinedLogoAerial: '{5}'", (geoCodingServiceEndpoint ?? "null"), (combinedLogoUrl ?? "null"), (aerialWithLabels ?? "null"), (aerialWithoutLabels ?? "null"), (roadWithLabels ?? "null"), (roadWithoutLabels ?? "null"), (bingLogoUrl ?? "null"), (bingLogoAerialUrl ?? "null"), (combinedLogoAerialUrl ?? "null"));
            }
            catch (WebException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Fetching Bing Geocoding and MapTiles URL's failed with exception: {0}", ex.Message);
                bool flag = false;
                int num;
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response is HttpWebResponse)
                {
                    num = (int)((HttpWebResponse)ex.Response).StatusCode;
                    if (num >= 500)
                        flag = true;
                }
                else
                {
                    num = (int)ex.Status;
                    switch (ex.Status)
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
                            flag = true;
                            break;
                    }
                }
                return new GeospatialEndpoint.EndPoint()
                {
                    ServerNotReachable = flag,
                    OtherError = !flag,
                    Geocoding = Properties.Settings.Default.GeocodingEndPoint,
                    RoadWithLabels = Properties.Settings.Default.RoadWithLabelsEndPoint,
                    RoadWithoutLabels = Properties.Settings.Default.RoadWithoutLabelsEndPoint,
                    AerialWithLabels = Properties.Settings.Default.AerialWithLabelsEndPoint,
                    AerialWithoutLabels = Properties.Settings.Default.AerialWithoutLabelsEndPoint,
                    BingLogo = Properties.Settings.Default.BingLogoEndPoint,
                    CombinedLogo = Properties.Settings.Default.CombinedLogoEndPoint,
                    BingLogoAerial = Properties.Settings.Default.BingLogoAerialEndPoint,
                    CombinedLogoAerial = Properties.Settings.Default.CombinedLogoAerialEndPoint,
                    StatusCode = num
                };
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Fetching Bing Geocoding and MapTiles URL's failed with exception: {0}", ex.Message);
                return new EndPoint()
                {
                    ServerNotReachable = false,
                    OtherError = true,
                    Geocoding = geoCodingServiceEndpoint ?? Properties.Settings.Default.GeocodingEndPoint,
                    RoadWithLabels = roadWithLabels ?? Properties.Settings.Default.RoadWithLabelsEndPoint,
                    RoadWithoutLabels = roadWithoutLabels ?? Properties.Settings.Default.RoadWithoutLabelsEndPoint,
                    AerialWithLabels = aerialWithLabels ?? Properties.Settings.Default.AerialWithLabelsEndPoint,
                    AerialWithoutLabels = aerialWithoutLabels ?? Properties.Settings.Default.AerialWithoutLabelsEndPoint,
                    BingLogo = bingLogoUrl ?? Properties.Settings.Default.BingLogoEndPoint,
                    CombinedLogo = combinedLogoUrl ?? Properties.Settings.Default.CombinedLogoEndPoint,
                    BingLogoAerial = bingLogoAerialUrl ?? Properties.Settings.Default.BingLogoAerialEndPoint,
                    CombinedLogoAerial = combinedLogoAerialUrl ?? Properties.Settings.Default.CombinedLogoAerialEndPoint,
                    StatusCode = 900
                };
            }
            return new EndPoint()
            {
                ServerNotReachable = false,
                OtherError = false,
                Geocoding = geoCodingServiceEndpoint,
                RoadWithLabels = roadWithLabels,
                RoadWithoutLabels = roadWithoutLabels,
                AerialWithLabels = aerialWithLabels,
                AerialWithoutLabels = aerialWithoutLabels,
                BingLogo = bingLogoUrl,
                CombinedLogo = combinedLogoUrl,
                BingLogoAerial = bingLogoAerialUrl,
                CombinedLogoAerial = combinedLogoAerialUrl
            };;
        }

        private static async Task<EndpointServiceResponse> GetBingServiceInfoAsync(string language, string region, CancellationToken cancellationToken)
        {
            WebClient client = WebRequestHelper.CreateWebClient();
            EndpointServiceResponse endpointServiceResponse;
            try
            {
                byte[] bytes = await Task.Factory.StartNew((() => client.DownloadData(string.Format(EndpointServiceUrl, language, region, "AutmxuJvVVVQyluwfF-Le9A6WQ_ypucXcJbzx5Rwf5u8on47kJRDu19BzV4kZlq9"))), cancellationToken).ConfigureAwait(false);
                string jsonString = client.Encoding.GetString(bytes);
                int s = jsonString.IndexOf("\"__type\"", StringComparison.InvariantCulture);
                if (s >= 0)
                {
                    int num = jsonString.IndexOf(",", s, StringComparison.InvariantCulture);
                    if (num > s)
                        jsonString = jsonString.Remove(s, num - s + 1);
                }
                JavaScriptSerializer js = new JavaScriptSerializer();
                EndpointServiceResponse response = js.Deserialize<EndpointServiceResponse>(jsonString);
                endpointServiceResponse = response;
            }
            finally
            {
                if (client != null)
                    client.Dispose();
            }
            return endpointServiceResponse;
        }

        public struct EndPoint
        {
            public int StatusCode { get; set; }

            public bool ServerNotReachable { get; set; }

            public bool OtherError { get; set; }

            public string AerialWithLabels { get; set; }

            public string AerialWithoutLabels { get; set; }

            public string RoadWithLabels { get; set; }

            public string RoadWithoutLabels { get; set; }

            public string Geocoding { get; set; }

            public string BingLogo { get; set; }

            public string CombinedLogo { get; set; }

            public string BingLogoAerial { get; set; }

            public string CombinedLogoAerial { get; set; }

            public string BingMapKey
            {
                get
                {
                    return "AutmxuJvVVVQyluwfF-Le9A6WQ_ypucXcJbzx5Rwf5u8on47kJRDu19BzV4kZlq9";
                }
            }

            public bool HasNullEndpoints
            {
                get
                {
                    if (this.Geocoding == null)
                        return true;
                    return
                        this.AerialWithoutLabels == null &&
                        this.AerialWithLabels == null &&
                        this.RoadWithLabels == null &&
                        this.RoadWithoutLabels == null;
                }
            }
        }
    }
}
