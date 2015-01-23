namespace ConsoleApplication1
{
    using Microsoft.Data.Visualization.Utilities;
    using Microsoft.Data.Visualization.VisualizationControls;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    [Serializable, XmlRoot("VisualizationLState", Namespace = "http://microsoft.data.visualization.Client.Excel.LState/1.0", IsNullable = false)]
    public class LatLonCacheState
    {
        private BingLatLonCache latLonCache = new BingLatLonCache();

        internal void Initialize(string bingMapsKey, string bingMapsRootUrl, CultureInfo displayLanguageCulture)
        {
            this.LatLonCache.Initialize(20, bingMapsKey, bingMapsRootUrl, 0x2d, displayLanguageCulture);
        }

        [XmlIgnore]
        public BingLatLonCache LatLonCache
        {
            get
            {
                return this.latLonCache;
            }
        }

        [XmlElement(ElementName = "cg", IsNullable = false)]
        public string SerializedBingLatLonCache
        {
            get
            {
                if (this.latLonCache != null)
                {
                    try
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            using (GZipStream stream2 = new GZipStream(stream, System.IO.Compression.CompressionLevel.Fastest))
                            {
                                using (XmlWriter writer = XmlWriter.Create(stream2))
                                {
                                    new XmlSerializer(typeof(BingLatLonCache)).Serialize(writer, this.latLonCache);
                                }
                            }
                            return Convert.ToBase64String(stream.GetBuffer());
                        }
                    }
                    catch (Exception exception)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while serializing VisualizationLState.cg.", exception);
                    }
                }
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "VisualizationLState.cg.set: value is empty or white space, ignored.");
                }
                else
                {
                    try
                    {
                        using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(value)))
                        {
                            using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress))
                            {
                                using (XmlReader reader = XmlReader.Create(stream2))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(BingLatLonCache));
                                    this.latLonCache = (BingLatLonCache)serializer.Deserialize(reader);
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while deserializing VisualizationLState.cg.", exception);
                        this.latLonCache = new BingLatLonCache();
                    }
                }
            }
        }
    }
}

