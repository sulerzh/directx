using ADODB;
using Microsoft.Data.Recommendation.Client;
using Microsoft.Data.Recommendation.Client.Sampler;
using Microsoft.Data.Recommendation.Common.ObjectModel;
using Microsoft.Data.Visualization.Utilities;
using System.Diagnostics;

namespace Microsoft.Data.Recommendation.Client.PowerMap.Sampler
{
    internal class GeoflowSampleManager : SampleManager
    {
        private Connection DataSource { get; set; }

        public GeoflowSampleManager(Connection DataSource)
            : base(DataSource.ConnectionString)
        {
            this.DataSource = DataSource;
        }

        public override void UpdateSamples(Tableset tables, int desiredSamples)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            this.UpdateSamples(new Microsoft.Data.Recommendation.Client.PowerMap.SamplerLocator(this.DataSource), tables, desiredSamples);
            stopwatch.Stop();
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Total column sampling time for Orlando classification: {0} ms", (object)stopwatch.ElapsedMilliseconds);
        }
    }
}
