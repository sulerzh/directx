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

    public static async Task<GeospatialEndpoint.EndPoint> FetchServiceEndpointsAsync(string language, string region, CancellationToken cancellationToken)
    {
      string geoCodingServiceEndpoint = (string) null;
      string aerialWithoutLabels = (string) null;
      string aerialWithLabels = (string) null;
      string roadWithoutLabels = (string) null;
      string roadWithLabels = (string) null;
      string combinedLogoUrl = (string) null;
      string bingLogoUrl = (string) null;
      string combinedLogoAerialUrl = (string) null;
      string bingLogoAerialUrl = (string) null;
      GeospatialEndpoint.EndPoint endPoint;
      try
      {
        EndpointServiceResponse response = await GeospatialEndpoint.GetBingServiceInfoAsync(language, region, cancellationToken).ConfigureAwait(false);
        response.resourceSets.ForEach((Action<EndpointServiceResourceSet>) (rs => rs.resources.ForEach((Action<EndpointServiceResource>) (r =>
        {
          if (!r.isSupported)
            return;
          r.services.ForEach((Action<Service>) (s =>
          {
            if (s.serviceName.Equals("Geocode", StringComparison.InvariantCultureIgnoreCase))
            {
              geoCodingServiceEndpoint = string.Format("https://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.GeocodingEndPoint = geoCodingServiceEndpoint;
            }
            else if (s.serviceName.Equals("CombinedLogo", StringComparison.InvariantCultureIgnoreCase))
            {
              combinedLogoUrl = string.Format("https://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.CombinedLogoEndPoint = combinedLogoUrl;
            }
            else if (s.serviceName.Equals("BingLogo", StringComparison.InvariantCultureIgnoreCase))
            {
              bingLogoUrl = string.Format("https://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.BingLogoEndPoint = bingLogoUrl;
            }
            else if (s.serviceName.Equals("CombinedLogoAerial", StringComparison.InvariantCultureIgnoreCase))
            {
              combinedLogoAerialUrl = string.Format("https://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.CombinedLogoAerialEndPoint = combinedLogoAerialUrl;
            }
            else if (s.serviceName.Equals("BingLogoAerial", StringComparison.InvariantCultureIgnoreCase))
            {
              bingLogoAerialUrl = string.Format("https://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.BingLogoAerialEndPoint = bingLogoAerialUrl;
            }
            else if (s.serviceName.Equals("AerialWithoutLabels", StringComparison.InvariantCultureIgnoreCase))
            {
              aerialWithoutLabels = string.Format("http://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.AerialWithoutLabelsEndPoint = aerialWithoutLabels;
            }
            else if (s.serviceName.Equals("AerialWithLabels", StringComparison.InvariantCultureIgnoreCase))
            {
              aerialWithLabels = string.Format("http://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.AerialWithLabelsEndPoint = aerialWithLabels;
            }
            else if (s.serviceName.Equals("RoadWithLabels", StringComparison.InvariantCultureIgnoreCase))
            {
              roadWithLabels = string.Format("http://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.RoadWithLabelsEndPoint = roadWithLabels;
            }
            else
            {
              if (!s.serviceName.Equals("RoadWithoutLabels", StringComparison.InvariantCultureIgnoreCase))
                return;
              roadWithoutLabels = string.Format("http://{0}", (object) s.endpoint);
              Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.RoadWithoutLabelsEndPoint = roadWithoutLabels;
            }
          }));
        }))));
        Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.Save();
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Fetched the following endpoints from Geospatial Endpoint Service; Geocoding: '{0}', CombinedLogo: '{1}', MapTiles: '{2}', BingLogo: '{3}', BingLogoAerial: '{4}', CombinedLogoAerial: '{5}'", (object) (geoCodingServiceEndpoint ?? "null"), (object) (combinedLogoUrl ?? "null"), (object) (aerialWithLabels ?? "null"), (object) (aerialWithoutLabels ?? "null"), (object) (roadWithLabels ?? "null"), (object) (roadWithoutLabels ?? "null"), (object) (bingLogoUrl ?? "null"), (object) (bingLogoAerialUrl ?? "null"), (object) (combinedLogoAerialUrl ?? "null"));
      }
      catch (WebException ex)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Fetching Bing Geocoding and MapTiles URL's failed with exception: {0}", (object) ex.Message);
        bool flag = false;
        int num;
        if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response is HttpWebResponse)
        {
          num = (int) ((HttpWebResponse) ex.Response).StatusCode;
          if (num >= 500)
            flag = true;
        }
        else
        {
          num = (int) ex.Status;
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
        endPoint = new GeospatialEndpoint.EndPoint()
        {
          ServerNotReachable = flag,
          OtherError = !flag,
          Geocoding = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.GeocodingEndPoint,
          RoadWithLabels = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.RoadWithLabelsEndPoint,
          RoadWithoutLabels = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.RoadWithoutLabelsEndPoint,
          AerialWithLabels = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.AerialWithLabelsEndPoint,
          AerialWithoutLabels = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.AerialWithoutLabelsEndPoint,
          BingLogo = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.BingLogoEndPoint,
          CombinedLogo = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.CombinedLogoEndPoint,
          BingLogoAerial = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.BingLogoAerialEndPoint,
          CombinedLogoAerial = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.CombinedLogoAerialEndPoint,
          StatusCode = num
        };
        goto label_11;
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Fetching Bing Geocoding and MapTiles URL's failed with exception: {0}", (object) ex.Message);
        endPoint = new GeospatialEndpoint.EndPoint()
        {
          ServerNotReachable = false,
          OtherError = true,
          Geocoding = geoCodingServiceEndpoint ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.GeocodingEndPoint,
          RoadWithLabels = roadWithLabels ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.RoadWithLabelsEndPoint,
          RoadWithoutLabels = roadWithoutLabels ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.RoadWithoutLabelsEndPoint,
          AerialWithLabels = aerialWithLabels ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.AerialWithLabelsEndPoint,
          AerialWithoutLabels = aerialWithoutLabels ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.AerialWithoutLabelsEndPoint,
          BingLogo = bingLogoUrl ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.BingLogoEndPoint,
          CombinedLogo = combinedLogoUrl ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.CombinedLogoEndPoint,
          BingLogoAerial = bingLogoAerialUrl ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.BingLogoAerialEndPoint,
          CombinedLogoAerial = combinedLogoAerialUrl ?? Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.CombinedLogoAerialEndPoint,
          StatusCode = 900
        };
        goto label_11;
      }
      endPoint = new GeospatialEndpoint.EndPoint()
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
      };
label_11:
      return endPoint;
    }

    private static async Task<EndpointServiceResponse> GetBingServiceInfoAsync(string language, string region, CancellationToken cancellationToken)
    {
      WebClient client = WebRequestHelper.CreateWebClient();
      EndpointServiceResponse endpointServiceResponse;
      try
      {
        byte[] bytes = await Task.Factory.StartNew<byte[]>((Func<byte[]>) (() => client.DownloadData(string.Format("https://dev.virtualearth.net/REST/V1/GeospatialEndpoint/{0}/{1}?key={2}", (object) language, (object) region, (object) "AutmxuJvVVVQyluwfF-Le9A6WQ_ypucXcJbzx5Rwf5u8on47kJRDu19BzV4kZlq9"))), cancellationToken).ConfigureAwait(false);
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
          if (this.AerialWithoutLabels == null && this.AerialWithLabels == null && this.RoadWithLabels == null)
            return this.RoadWithoutLabels == null;
          else
            return false;
        }
      }
    }
  }
}
