using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Client.Excel
{
    [XmlRoot("VisualizationPState", IsNullable = false, Namespace = "http://microsoft.data.visualization.Client.Excel.PState/1.0")]
    [Serializable]
    public class WorkbookPolygonState
    {
        private int ConcurrencyLimit = Math.Min(Environment.ProcessorCount * 4, Environment.Is64BitProcess ? 32 : 16);
        private IRegionProvider regionProvider;
        private CustomXMLPart regionXmlPart;

        [XmlIgnore]
        public IRegionProvider RegionProvider
        {
            get
            {
                return this.regionProvider;
            }
        }

        [XmlElement(ElementName = "rp", IsNullable = false)]
        public string SerializedRegionCache
        {
            get
            {
                if (this.regionProvider != null)
                {
                    try
                    {
                        return ((BingRegionProvider)this.regionProvider).SerializedRegionProvider;
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while serializing VisualizationPState.rp cache.", ex);
                    }
                }
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "VisualizationPState.rp.set: value is empty or white space, ignored.");
                }
                else
                {
                    try
                    {
                        this.regionProvider = new BingRegionProvider(value, this.ConcurrencyLimit);
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while deserializing VisualizationPState.rp cache.", ex);
                        this.regionProvider = new BingRegionProvider(this.ConcurrencyLimit);
                    }
                }
            }
        }

        public WorkbookPolygonState()
        {
            this.regionProvider = new BingRegionProvider(this.ConcurrencyLimit);
        }

        internal void Dispose()
        {
            this.regionXmlPart = null;
            if (this.regionProvider != null)
            {
                this.regionProvider.Dispose();
                this.regionProvider = null;
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationPState.Dispose(): done");
        }

        internal void Delete()
        {
            if (this.regionXmlPart == null)
                return;
            this.regionXmlPart.Delete();
            this.regionXmlPart = null;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationPState.Delete(): removed old xml");
        }

        internal void Save(Workbook workbook)
        {
            if (workbook == null)
                return;
            if (this.regionProvider == null)
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationPState.Save(): rp is null, returning");
            else if (!this.regionProvider.IsDirty)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationPState.Save(): rp not changed, returning");
            }
            else
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationPState.Save(): starting");
                StringBuilder output = new StringBuilder();
                XmlWriter xmlWriter = XmlWriter.Create(output);
                CustomXMLPart customXmlPart;
                try
                {
                    new XmlSerializer(typeof(WorkbookPolygonState)).Serialize(xmlWriter, this);
                    customXmlPart = workbook.CustomXMLParts.Add(output.ToString(), Missing.Value);
                }
                catch (NullReferenceException ex)
                {
                    this.regionProvider.IsDirty = true;
                    VisualizationTraceSource.Current.Fail("VisualizationPState.Save(): adding new xml, NullRefEx - throwing OOM", ex);
                    throw new OutOfMemoryException("Updating tour list xml failed", ex);
                }
                catch (COMException ex)
                {
                    this.regionProvider.IsDirty = true;
                    if (ex.ErrorCode == -2147467259)
                    {
                        VisualizationTraceSource.Current.Fail("VisualizationPState.Save(): adding new xml, COMException with E_FAIL - assuming ex.Message is 'Not enough storage is available to complete this operation'  - throwing OOM", ex);
                        throw new OutOfMemoryException("Updating tour list xml failed", ex);
                    }
                    else
                    {
                        VisualizationTraceSource.Current.Fail("VisualizationPState.Save(): adding new xml, caught exception, ignoring; ex=", ex);
                        return;
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    this.regionProvider.IsDirty = true;
                    VisualizationTraceSource.Current.Fail("VisualizationPState.Save(): adding new xml, OOM - rethrowing");
                    throw;
                }
                catch (Exception ex)
                {
                    this.regionProvider.IsDirty = true;
                    VisualizationTraceSource.Current.Fail("VisualizationPState.Save(): adding new xml, caught exception, ignoring; ex=", ex);
                    return;
                }
                finally
                {
                    xmlWriter.Close();
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationPState.Save(): added new xml");
                this.Delete();
                this.regionXmlPart = customXmlPart;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationPState.Save(): done");
            }
        }

        internal static WorkbookPolygonState CreateFromXmlPart(Workbook workbook)
        {
            CustomXMLParts customXmlParts = workbook.CustomXMLParts.SelectByNamespace("http://microsoft.data.visualization.Client.Excel.PState/1.0");
            if (customXmlParts.Count == 0)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "No existing Excel VisualizationPState XMLPart was found");
                return new WorkbookPolygonState();
            }
            if (customXmlParts.Count > 1)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0} VisualizationPState CustomXMLPart with namespace {1} were found, ignoring all", (object)customXmlParts.Count, (object)"http://microsoft.data.visualization.Client.Excel.PState/1.0");
                return new WorkbookPolygonState();
            }
            CustomXMLPart customXmlPart = customXmlParts[1];
            string xml = customXmlPart.DocumentElement.XML;
            WorkbookPolygonState workbookPolygonState;
            if (string.IsNullOrEmpty(xml))
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "VisualizationPState CustomXMLPart with namespace {0} xml is empty string", (object)"http://microsoft.data.visualization.Client.Excel.PState/1.0");
                workbookPolygonState = new WorkbookPolygonState();
            }
            else
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(WorkbookPolygonState));
                StringReader stringReader = new StringReader(xml);
                try
                {
                    workbookPolygonState = (WorkbookPolygonState)xmlSerializer.Deserialize(stringReader);
                }
                catch (Exception ex)
                {
                    VisualizationTraceSource.Current.Fail("Exception while loading the VisualizationPState, discarding state", ex);
                    workbookPolygonState = new WorkbookPolygonState();
                }
                finally
                {
                    stringReader.Close();
                }
            }
            workbookPolygonState.regionXmlPart = customXmlPart;
            return workbookPolygonState;
        }
    }
}
