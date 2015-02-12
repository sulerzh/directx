using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    internal class EndpointServiceResourceSet
    {
        public int estimatedTotal { get; set; }

        public List<EndpointServiceResource> resources { get; set; }
    }
}
