using System.Net;
using System.Net.Http;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class WebRequestHelper
  {
    public static WebClient CreateWebClient()
    {
      WebClient webClient = new WebClient();
      if (WebRequest.DefaultWebProxy != null)
      {
        webClient.Proxy = WebRequest.DefaultWebProxy;
        webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
      }
      return webClient;
    }

    public static WebRequest CreateWebRequest(string url)
    {
      WebRequest webRequest = WebRequest.Create(url);
      if (WebRequest.DefaultWebProxy != null)
      {
        webRequest.Proxy = WebRequest.DefaultWebProxy;
        webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
      }
      return webRequest;
    }

    public static HttpClient CreateHttpClient()
    {
      HttpClientHandler httpClientHandler;
      if (WebRequest.DefaultWebProxy != null)
        httpClientHandler = new HttpClientHandler()
        {
          UseCookies = false,
          Proxy = WebRequest.DefaultWebProxy,
          Credentials = CredentialCache.DefaultCredentials
        };
      else
        httpClientHandler = new HttpClientHandler()
        {
          UseCookies = false
        };
      return new HttpClient((HttpMessageHandler) httpClientHandler);
    }
  }
}
