using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  internal class CopyrightServiceResponse
  {
    public string authenticationResultCode { get; set; }

    public string brandLogoUri { get; set; }

    public string copyright { get; set; }

    public List<CopyrightServiceResourceSet> resourceSets { get; set; }

    public int statusCode { get; set; }

    public string statusDescription { get; set; }

    public string traceId { get; set; }
  }
}
