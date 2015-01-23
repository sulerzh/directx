namespace ConsoleApplication1
{
    using Microsoft.Data.Visualization.Engine;
    using Microsoft.Data.Visualization.Utilities;
    using Microsoft.Data.Visualization.VisualizationControls;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    [Serializable, XmlRoot("VisualizationPState", Namespace = "http://microsoft.data.visualization.Client.Excel.PState/1.0", IsNullable = false)]
    public class PolygonState
    {
        private int ConcurrencyLimit = Math.Min(Environment.ProcessorCount * 4, Environment.Is64BitProcess ? 0x20 : 0x10);
        private IRegionProvider regionProvider;

        public PolygonState()
        {
            this.regionProvider = new BingRegionProvider(this.ConcurrencyLimit);
        }

        internal void Dispose()
        {
            if (this.regionProvider != null)
            {
                this.regionProvider.Dispose();
                this.regionProvider = null;
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "VisualizationPState.Dispose(): done");
        }

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
                    catch (Exception exception)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while serializing VisualizationPState.rp cache.", exception);
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
                    catch (Exception exception)
                    {
                        VisualizationTraceSource.Current.Fail("Exception while deserializing VisualizationPState.rp cache.", exception);
                        this.regionProvider = new BingRegionProvider(this.ConcurrencyLimit);
                    }
                }
            }
        }
    }
}

