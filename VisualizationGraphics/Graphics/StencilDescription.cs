namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public struct StencilDescription
    {
        public ComparisonFunction Function { get; set; }

        public StencilOperation FailOperation { get; set; }

        public StencilOperation DepthFailOperation { get; set; }

        public StencilOperation PassOperation { get; set; }
    }
}
