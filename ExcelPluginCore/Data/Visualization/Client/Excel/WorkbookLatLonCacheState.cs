using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
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

namespace Microsoft.Data.Visualization.Client.Excel
{
    [XmlRoot("VisualizationLState", IsNullable = false, Namespace = "http://microsoft.data.visualization.Client.Excel.LState/1.0")]
    [Serializable]
    public class WorkbookLatLonCacheState
    {
        private BingLatLonCache latLonCache;
        private CustomXMLPart latlonXmlPart;

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
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionLevel.Fastest))
                            {
                                using (XmlWriter xmlWriter = XmlWriter.Create(gzipStream))
                                    new XmlSerializer(typeof(BingLatLonCache)).Serialize(xmlWriter, this.latLonCache);
                            }
                            return Convert.ToBase64String(memoryStream.GetBuffer());
                        }
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while serializing VisualizationLState.cg.", ex);
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
                        using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(value)))
                        {
                            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                            {
                                using (XmlReader xmlReader = XmlReader.Create(gzipStream))
                                    this.latLonCache = (BingLatLonCache)new XmlSerializer(typeof(BingLatLonCache)).Deserialize(xmlReader);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while deserializing VisualizationLState.cg.", ex);
                        this.latLonCache = new BingLatLonCache();
                    }
                }
            }
        }

        public WorkbookLatLonCacheState()
        {
            this.latLonCache = new BingLatLonCache();
        }

        internal void Initialize(string bingMapsKey, string bingMapsRootUrl, CultureInfo displayLanguageCulture)
        {
            this.LatLonCache.Initialize(20, bingMapsKey, bingMapsRootUrl, 45, displayLanguageCulture);
        }

        internal void Delete()
        {
            if (this.latlonXmlPart == null)
                return;
            this.latlonXmlPart.Delete();
            this.latlonXmlPart = null;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationLState.Delete(): removed old xml");
        }

        internal void Save(Workbook workbook)
        {
            if (workbook == null)
                return;
            if (this.latLonCache == null)
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationLState.Save(): cg is null, returning");
            else if (!this.latLonCache.ShouldSave())
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationLState.Save(): cg not changed, returning");
            }
            else
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationLState.Save(): starting");
                StringBuilder output = new StringBuilder();
                XmlWriter xmlWriter = XmlWriter.Create(output);
                CustomXMLPart customXmlPart;
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(WorkbookLatLonCacheState));
                    this.latLonCache.SetShouldSave(false);
                    xmlSerializer.Serialize(xmlWriter, this);
                    customXmlPart = workbook.CustomXMLParts.Add(output.ToString(), Missing.Value);
                }
                catch (NullReferenceException ex)
                {
                    this.latLonCache.SetShouldSave(true);
                    VisualizationTraceSource.Current.Fail("VisualizationLState.Save(): adding new xml, NullRefEx - throwing OOM", ex);
                    throw new OutOfMemoryException("Updating tour list xml failed", ex);
                }
                catch (COMException ex)
                {
                    this.latLonCache.SetShouldSave(true);
                    if (ex.ErrorCode == -2147467259)
                    {
                        VisualizationTraceSource.Current.Fail("VisualizationLState.Save(): adding new xml, COMException with E_FAIL - assuming ex.Message is 'Not enough storage is available to complete this operation'  - throwing OOM", ex);
                        throw new OutOfMemoryException("Updating tour list xml failed", ex);
                    }
                    else
                    {
                        VisualizationTraceSource.Current.Fail("VisualizationLState.Save(): adding new xml, caught exception, ignoring; ex=", ex);
                        return;
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    this.latLonCache.SetShouldSave(true);
                    VisualizationTraceSource.Current.Fail("VisualizationLState.Save(): adding new xml, OOM - rethrowing");
                    throw;
                }
                catch (Exception ex)
                {
                    this.latLonCache.SetShouldSave(true);
                    VisualizationTraceSource.Current.Fail("VisualizationLState.Save(): adding new xml, caught exception, ignoring; ex=", ex);
                    return;
                }
                finally
                {
                    xmlWriter.Close();
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationLState.Save(): added new xml");
                this.Delete();
                this.latlonXmlPart = customXmlPart;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationLState.Save(): done");
            }
        }

        internal static WorkbookLatLonCacheState CreateFromXmlPart(Workbook workbook)
        {
            CustomXMLParts customXmlParts = workbook.CustomXMLParts.SelectByNamespace("http://microsoft.data.visualization.Client.Excel.LState/1.0");
            if (customXmlParts.Count == 0)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "No existing Excel VisualizationLState XMLPart was found");
                return new WorkbookLatLonCacheState();
            }
            else if (customXmlParts.Count > 1)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0} VisualizationLState CustomXMLPart with namespace {1} were found, ignoring all", (object)customXmlParts.Count, (object)"http://microsoft.data.visualization.Client.Excel.LState/1.0");
                return new WorkbookLatLonCacheState();
            }
            else
            {
                CustomXMLPart customXmlPart = customXmlParts[1];
                string xml = customXmlPart.DocumentElement.XML;
                WorkbookLatLonCacheState latLonCacheState;
                if (string.IsNullOrEmpty(xml))
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "VisualizationLState CustomXMLPart with namespace {0} xml is empty string", (object)"http://microsoft.data.visualization.Client.Excel.LState/1.0");
                    latLonCacheState = new WorkbookLatLonCacheState();
                }
                else
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(WorkbookLatLonCacheState));
                    StringReader stringReader = new StringReader(xml);
                    try
                    {
                        latLonCacheState = (WorkbookLatLonCacheState)xmlSerializer.Deserialize(stringReader);
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while loading the VisualizationLState, discarding state", ex);
                        latLonCacheState = new WorkbookLatLonCacheState();
                    }
                    finally
                    {
                        stringReader.Close();
                    }
                }
                latLonCacheState.latlonXmlPart = customXmlPart;
                return latLonCacheState;
            }
        }
    }
}
