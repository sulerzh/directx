using System;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.IO;

namespace Microsoft.Data.Visualization.Utilities
{
    public class VisualizationTraceSource : TraceSource
    {
        private static VisualizationTraceSource tracesource;
        private bool assertUiEnabled;

        public static VisualizationTraceSource Current
        {
            get
            {
                if (VisualizationTraceSource.tracesource == null)
                {
                    VisualizationTraceSource.tracesource = new VisualizationTraceSource("Microsoft.Data.Visualization.DataProvider", SourceLevels.All);
                    VisualizationTraceSource.tracesource.Listeners.Add(new EventProviderTraceListener(VisualizationTraceSource.ETWProviderGuid, "Microsoft.Data.Visualization.DataProvider ETW Listener"));
                    VisualizationTraceSource.tracesource.RemoveDefaultTraceListeners();
                    VisualizationTraceSource.tracesource.AssertUIEnabled = false;
                    foreach (TraceListener traceListener in VisualizationTraceSource.tracesource.Listeners)
                        traceListener.TraceOutputOptions |= TraceOptions.DateTime | TraceOptions.ThreadId;
                }
                return VisualizationTraceSource.tracesource;
            }
        }

        public static string ETWProviderGuid
        {
            get
            {
                return "26033F5A-9643-4E95-BEB8-6A98ADE24B4B";
            }
        }

        public bool AssertUIEnabled
        {
            get
            {
                return this.assertUiEnabled;
            }
            set
            {
                this.assertUiEnabled = value;
                foreach (TraceListener traceListener in this.Listeners)
                {
                    DefaultTraceListener defaultTraceListener = traceListener as DefaultTraceListener;
                    if (defaultTraceListener != null)
                        defaultTraceListener.AssertUiEnabled = this.assertUiEnabled;
                }
            }
        }

        public VisualizationTraceSource(string name, SourceLevels defaultLevel)
            : base(name, defaultLevel)
        {
        }

        public void Fail(string details)
        {
            foreach (TraceListener traceListener in this.Listeners)
                traceListener.Fail("An internal error was encountered:", details);
            this.Flush();
        }

        public void Fail(Exception exception)
        {
            this.Fail("An internal error was encountered:", exception);
        }

        public void Fail(string message, Exception exception)
        {
            if (exception == null)
                return;
            try
            {
                foreach (TraceListener traceListener in this.Listeners)
                    traceListener.Fail(message, exception.ToString());
                this.Flush();
            }
            catch (Exception ex)
            {
            }
        }

        public void RemoveDefaultTraceListeners()
        {
            DefaultTraceListener defaultTraceListener;
            do
            {
                defaultTraceListener = null;
                foreach (TraceListener traceListener in this.Listeners)
                {
                    defaultTraceListener = traceListener as DefaultTraceListener;
                    if (defaultTraceListener != null)
                        break;
                }
                if (defaultTraceListener != null)
                    this.Listeners.Remove(defaultTraceListener);
            }
            while (defaultTraceListener != null);
        }

        public void AddFileTraceListener(string traceFile)
        {
            try
            {
                this.Listeners.Add(new TextWriterTraceListener(new FileStream(traceFile, FileMode.Create, FileAccess.Write, FileShare.Read)));
                Trace.AutoFlush = true;
                this.TraceInformation("Tracing to log file \"" + traceFile + "\"");
            }
            catch (Exception ex)
            {
                this.TraceEvent(TraceEventType.Warning, 1, "Error creating TextWriterTraceListener for trace file \"{0}\"; exception details: {1}", traceFile, ex.ToString());
            }
        }

        [Conditional("DEBUG")]
        private void SetAssertUiEnabled()
        {
            this.AssertUIEnabled = true;
        }
    }
}
