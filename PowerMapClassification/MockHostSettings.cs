using Microsoft.Data.Recommendation.Client;
using System.Globalization;

namespace Microsoft.Data.Recommendation.Client.PowerMap
{
    internal class MockHostSettings : IHostSettings
    {
        public bool AllowRecommendations { get; internal set; }

        public bool AutoClassify { get; internal set; }

        public CultureInfo CurrentUICulture { get; internal set; }

        public bool UpdateClassificationData { get; internal set; }

        public bool UploadSamplesForUserClassification { get; internal set; }

        public bool UseOnlineClassification { get; internal set; }

        public MockHostSettings()
        {
            this.AutoClassify = true;
            this.UpdateClassificationData = false;
            this.UseOnlineClassification = false;
            this.UploadSamplesForUserClassification = false;
            this.AllowRecommendations = false;
            this.CurrentUICulture = CultureInfo.CurrentUICulture;
        }
    }
}
