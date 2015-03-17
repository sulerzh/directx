using Microsoft.Data.Recommendation.Client;
using Microsoft.Data.Recommendation.Client.PowerMap;
using Microsoft.Data.Recommendation.Client.Sampler;
using Microsoft.Data.Recommendation.Common;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Data.Recommendation.Client.PowerMap.Sampler
{
    internal class DaxDefaultColumnSampler : IColumnSampler
    {
        private ISamplerLocator samplerLocator;
        private int MaxValueLength;

        public DaxDefaultColumnSampler(ISamplerLocator locator)
        {
            this.samplerLocator = locator;
            this.MaxValueLength = 512;
            if (!Configuration.Instance.IsInitialized)
                return;
            int sampleMaxLength = Configuration.Instance.Store.ClientSettings.SampleMaxLength;
            if (sampleMaxLength <= 0)
                return;
            this.MaxValueLength = sampleMaxLength;
        }

        private string PrepareMdxColumnName(string identifier)
        {
            string str1 = "\"]";
            string str2 = identifier;
            foreach (char c in str1)
            {
                string oldValue = new string(c, 1);
                string newValue = new string(c, 2);
                str2 = str2.Replace(oldValue, newValue);
            }
            return str2;
        }

        public List<ColumnSampleItem> GetColumnSamples(string dataSourceIdentity, IHostModelIdentifier columnHostId, int desiredSamples)
        {
            List<ColumnSampleItem> list1 = new List<ColumnSampleItem>();
            string modelQueryName = ((HostModelIdentifier)columnHostId).Column.ModelQueryName;
            List<object> list2 = null;
            IDataProvider provider = this.samplerLocator.GetProvider(DatasetType.PowerPivot, dataSourceIdentity);
            int num = 0;
            bool flag;
            do
            {
                try
                {
                    list2 = DaxDefaultColumnSampler.SampleColumn(provider, modelQueryName, desiredSamples, this.MaxValueLength);
                    flag = false;
                }
                catch (RetryableException ex)
                {
                    if (num >= 1)
                        throw new SampleFailedException(ex.Message, ex);
                    flag = true;
                    ++num;
                }
            }
            while (flag);
            if (list2 != null)
            {
                foreach (object obj in list2)
                    list1.Add(new ColumnSampleItem(obj));
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Column {0} sampled, {1} rows", (object)modelQueryName, (object)list1.Count);
            return list1;
        }

        private static List<object> SampleColumn(IDataProvider data, string column, int desiredSamples, int maxValueLength)
        {
            string queryStr = string.Format(CultureInfo.InvariantCulture, "EVALUATE(SAMPLE({1}, VALUES({0}), 1))",
                new object[2]
                {
                    column,
                    desiredSamples
                });
            return data.ExecuteQuery(queryStr, maxValueLength);
        }
    }
}
