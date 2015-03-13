using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Client.Excel
{
    [XmlRoot("CustomMapList", IsNullable = false, Namespace = "http://microsoft.data.visualization.Client.Excel.CustomMapList/1.0")]
    [Serializable]
    public class WorkbookCustomMapListState
    {
        private CustomMapsList mapList;
        private CustomXMLPart mapListXmlPart;

        [XmlIgnore]
        public CustomMapsList MapList
        {
            get
            {
                return this.mapList;
            }
        }

        [XmlElement(ElementName = "ml", IsNullable = false)]
        public string SerializedCustomMapList
        {
            get
            {
                if (this.mapList != null)
                {
                    try
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionLevel.Fastest))
                            {
                                using (XmlWriter xmlWriter = XmlWriter.Create(gzipStream))
                                    new XmlSerializer(typeof(CustomMapsList.SerializableCustomMapsList)).Serialize(xmlWriter, this.mapList.Wrap());
                            }
                            return Convert.ToBase64String(memoryStream.GetBuffer());
                        }
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while serializing CustomMapList.cg.", ex);
                    }
                }
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "CustomMapList.cg.set: value is empty or white space, ignored.");
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
                                    this.mapList = ((CustomMapsList.SerializableCustomMapsList)new XmlSerializer(typeof(CustomMapsList.SerializableCustomMapsList)).Deserialize(xmlReader)).Unwrap();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while deserializing CustomMapList.cg.", ex);
                        this.mapList = new CustomMapsList();
                    }
                }
            }
        }

        public WorkbookCustomMapListState()
        {
            this.mapList = new CustomMapsList();
        }

        internal void DeleteAllMaps()
        {
            CustomMapsList customMapsList = this.mapList;
            if (customMapsList == null)
                return;
            customMapsList.DeleteAllCustomMaps();
        }

        internal void Delete()
        {
            if (this.mapListXmlPart == null)
                return;
            this.mapListXmlPart.Delete();
            this.mapListXmlPart = null;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "CustomMapList.Delete(): removed old xml");
        }

        internal void Save(Workbook workbook)
        {
            if (workbook == null)
                return;
            if (this.mapList == null)
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "CustomMapList.Save(): cg is null, returning");
            else if (!this.mapList.ShouldSave())
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "CustomMapList.Save(): cg not changed, returning");
            }
            else
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "CustomMapList.Save(): starting");
                StringBuilder output = new StringBuilder();
                XmlWriter xmlWriter = XmlWriter.Create(output);
                CustomXMLPart customXmlPart;
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(WorkbookCustomMapListState));
                    this.mapList.SetShouldSave(false);
                    xmlSerializer.Serialize(xmlWriter, this);
                    customXmlPart = workbook.CustomXMLParts.Add(output.ToString(), Missing.Value);
                }
                catch (NullReferenceException ex)
                {
                    this.mapList.SetShouldSave(true);
                    VisualizationTraceSource.Current.Fail("CustomMapList.Save(): adding new xml, NullRefEx - throwing OOM", ex);
                    throw new OutOfMemoryException("Updating tour list xml failed", ex);
                }
                catch (COMException ex)
                {
                    this.mapList.SetShouldSave(true);
                    if (ex.ErrorCode == -2147467259)
                    {
                        VisualizationTraceSource.Current.Fail("CustomMapList.Save(): adding new xml, COMException with E_FAIL - assuming ex.Message is 'Not enough storage is available to complete this operation'  - throwing OOM", ex);
                        throw new OutOfMemoryException("Updating tour list xml failed", ex);
                    }
                    else
                    {
                        VisualizationTraceSource.Current.Fail("CustomMapList.Save(): adding new xml, caught exception, ignoring; ex=", ex);
                        return;
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    this.mapList.SetShouldSave(true);
                    VisualizationTraceSource.Current.Fail("CustomMapList.Save(): adding new xml, OOM - rethrowing");
                    throw;
                }
                catch (Exception ex)
                {
                    this.mapList.SetShouldSave(true);
                    VisualizationTraceSource.Current.Fail("CustomMapList.Save(): adding new xml, caught exception, ignoring; ex=", ex);
                    return;
                }
                finally
                {
                    xmlWriter.Close();
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "CustomMapList.Save(): added new xml");
                this.Delete();
                this.mapListXmlPart = customXmlPart;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "CustomMapList.Save(): done");
            }
        }

        internal static WorkbookCustomMapListState CreateFromXmlPart(Workbook workbook)
        {
            CustomXMLParts customXmlParts = workbook.CustomXMLParts.SelectByNamespace("http://microsoft.data.visualization.Client.Excel.CustomMapList/1.0");
            if (customXmlParts.Count == 0)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "No existing Excel CustomMapList XMLPart was found");
                return new WorkbookCustomMapListState();
            }
            if (customXmlParts.Count > 1)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0} CustomMapList CustomXMLPart with namespace {1} were found, ignoring all", (object)customXmlParts.Count, (object)"http://microsoft.data.visualization.Client.Excel.CustomMapList/1.0");
                return new WorkbookCustomMapListState();
            }
            CustomXMLPart customXmlPart = customXmlParts[1];
            string xml = customXmlPart.DocumentElement.XML;
            WorkbookCustomMapListState customMapListState;
            if (string.IsNullOrEmpty(xml))
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "CustomMapList CustomXMLPart with namespace {0} xml is empty string", (object)"http://microsoft.data.visualization.Client.Excel.CustomMapList/1.0");
                customMapListState = new WorkbookCustomMapListState();
            }
            else
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(WorkbookCustomMapListState));
                StringReader stringReader = new StringReader(xml);
                try
                {
                    customMapListState = (WorkbookCustomMapListState)xmlSerializer.Deserialize(stringReader);
                }
                catch (Exception ex)
                {
                    VisualizationTraceSource.Current.Fail("Exception while loading the CustomMapList, discarding state", ex);
                    customMapListState = new WorkbookCustomMapListState();
                }
                finally
                {
                    stringReader.Close();
                }
            }
            customMapListState.mapListXmlPart = customXmlPart;
            return customMapListState;
        }
    }
}
