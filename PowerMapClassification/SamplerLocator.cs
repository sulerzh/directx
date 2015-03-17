using ADODB;
using Microsoft.Data.Recommendation.Client.PowerMap.Sampler;
using Microsoft.Data.Recommendation.Client.Sampler;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Recommendation.Client.PowerMap
{
    internal class SamplerLocator : ISamplerLocator, IDisposable
    {
        private Dictionary<string, string> SamplerSettings = new Dictionary<string, string>();
        private Dictionary<string, IDataProvider> Providers = new Dictionary<string, IDataProvider>();
        private const string CardinalitySettingKey = "HighCardinalityLimit";
        private const string CardinalitySettingValue = "100000";
        private const string MaxLengthKey = "MaxValueLength";
        private const string MaxLengthValue = "512";
        private bool IsDisposed;
        private Connection dataSource;

        public virtual IDictionary<string, string> Settings
        {
            get
            {
                return this.SamplerSettings;
            }
        }

        public SamplerLocator(Connection dataSource)
        {
            this.dataSource = dataSource;
            this.SamplerSettings.Add(CardinalitySettingKey, CardinalitySettingValue);
            this.SamplerSettings.Add(MaxLengthKey, MaxLengthValue);
            this.Providers.Add(dataSource.ConnectionString, new GeoflowDataProvider(dataSource));
        }

        public virtual IDataProvider GetProvider(DatasetType type, string dataSourceIdentity)
        {
            return new GeoflowDataProvider(this.dataSource);
        }

        public virtual IColumnSampler GetColumnSampler(Type columnType)
        {
            return new DaxDefaultColumnSampler(this);
        }

        public virtual Microsoft.Data.Recommendation.Client.Sampler.Sampler GetSampler(string dataSourceIdentity)
        {
            return new GeoflowSampler(this, dataSourceIdentity, new TraceTemp());
        }

        public void Dispose()
        {
            if (this.IsDisposed)
                return;
            foreach (IDisposable disposable in this.Providers.Values)
            {
                if (disposable != null)
                    disposable.Dispose();
            }
            this.IsDisposed = true;
        }
    }
}
