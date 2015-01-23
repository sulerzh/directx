using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public static class CopyrightService
    {
        private const string CopyrightServiceUrl = "https://dev.virtualearth.net/REST/V1/Imagery/Copyright/{0}/{1}/{2}/{3}/{4}/{5}/{6}?output=json&key={7}";

        public static Task FetchCopyrightAsync(int token, string imagerySet, Dictionary<int, TileExtent> tileExtents, Action<int, bool, List<string>, int> callBack)
        {
            int failureCount = 0;
            return Task.Factory.ContinueWhenAll<CopyrightServiceResponse>(
                (from tileExtent in tileExtents
                 select Task.Factory.StartNew<CopyrightServiceResponse>(delegate
                 {
                     try
                     {
                         using (WebClient client = WebRequestHelper.CreateWebClient())
                         {
                             string name = Resources.Culture.Name;
                             string address = string.Format(CultureInfo.InvariantCulture, "https://dev.virtualearth.net/REST/V1/Imagery/Copyright/{0}/{1}/{2}/{3}/{4}/{5}/{6}?output=json&key={7}", new object[] { name, imagerySet, tileExtent.Key, tileExtent.Value.TopLeft.LatitudeInDegrees, tileExtent.Value.TopLeft.LongitudeInDegrees, tileExtent.Value.BottomRight.LatitudeInDegrees, tileExtent.Value.BottomRight.LongitudeInDegrees, "AutmxuJvVVVQyluwfF-Le9A6WQ_ypucXcJbzx5Rwf5u8on47kJRDu19BzV4kZlq9" });
                             string input = Regex.Replace(Encoding.UTF8.GetString(client.DownloadData(address)), "\"__type\".+?,", string.Empty);
                             JavaScriptSerializer serializer = new JavaScriptSerializer();
                             return serializer.Deserialize<CopyrightServiceResponse>(input);
                         }
                     }
                     catch (Exception exception)
                     {
                         if (exception is WebException)
                         {
                             Interlocked.Increment(ref failureCount);
                         }
                         VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Retrieving Bing copyright information failed with error: '{0}'", new object[] { exception.Message });
                     }
                     return null;
                 })).ToArray<Task<CopyrightServiceResponse>>(), delegate(Task<CopyrightServiceResponse>[] alltasks)
            {
                Action<CopyrightServiceResourceSet> action = null;
                bool displayCombined = false;
                HashSet<string> copyrights = new HashSet<string>();
                foreach (Task<CopyrightServiceResponse> task in alltasks)
                {
                    if ((task.Status == TaskStatus.RanToCompletion) && (task.Result != null))
                    {
                        if (action == null)
                        {
                            action = csrs => csrs.resources.ForEach(delegate(CopyrightServiceResource cpy)
                            {
                                displayCombined |= cpy.displayCombinedLogo;
                                cpy.imageryProviders.ForEach(delegate(string str)
                                {
                                    if (!copyrights.Contains(str))
                                    {
                                        copyrights.Add(str);
                                    }
                                });
                            });
                        }
                        task.Result.resourceSets.ForEach(action);
                    }
                }
                callBack(token, displayCombined, copyrights.ToList<string>(), failureCount);
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

        }
    }
}
