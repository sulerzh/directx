using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    internal class EndpointServiceResource
    {
        public bool isDisputedArea { get; set; }

        public bool isSupported { get; set; }

        public string userRegion { get; set; }

        public List<Service> services { get; set; }
    }
}
