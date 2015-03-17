using Microsoft.Data.Recommendation.Client;
using Microsoft.Data.Recommendation.Client.Sampler;
using Microsoft.Data.Recommendation.Common;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Recommendation.Client.PowerMap.Sampler
{
    internal class GeoflowSampler : Microsoft.Data.Recommendation.Client.Sampler.Sampler
  {
    private ISamplerLocator SamplerLocator;
    private string DataSource;

    public GeoflowSampler(string dataSourceIdentity, IFeatureExtractionTrace trace)
      : base(trace)
    {
      this.DataSource = dataSourceIdentity;
    }

    public GeoflowSampler(ISamplerLocator locator, string dataSourceIdentity, IFeatureExtractionTrace trace)
      : base(trace)
    {
      this.SamplerLocator = locator;
      this.DataSource = dataSourceIdentity;
    }

    public override List<ColumnSampleItem> GetColumnSamples(IHostModelIdentifier columnIdentity, Type columnType, int desiredSamples)
    {
      return this.GetColumnSamples(columnIdentity, desiredSamples);
    }

    public override List<ColumnSampleItem> GetColumnSamples(IHostModelIdentifier columnIdentity, int desiredSamples)
    {
      return new DaxDefaultColumnSampler(this.SamplerLocator).GetColumnSamples(this.DataSource, columnIdentity, desiredSamples);
    }

    public override SampleSet GetTableSamples(List<IHostModelIdentifier> columnIdentities, int desiredSamples)
    {
      throw new NotImplementedException();
    }

    public override List<IHostModelIdentifier> GetColumnIdentities()
    {
      throw new NotImplementedException();
    }

    public override void Dispose()
    {
    }
  }
}
