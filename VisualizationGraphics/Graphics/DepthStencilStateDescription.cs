namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class DepthStencilStateDescription
    {
        public bool DepthEnable { get; set; }

        public ComparisonFunction DepthFunction { get; set; }

        public bool DepthWriteEnable { get; set; }

        public bool StencilEnable { get; set; }

        public StencilDescription StencilFrontFace { get; set; }

        public StencilDescription StencilBackFace { get; set; }

        public int StencilReadMask { get; set; }

        public int StencilWriteMask { get; set; }

        public int StencilReferenceValue { get; set; }

        public DepthStencilStateDescription()
        {
            this.DepthEnable = true;
            this.DepthFunction = ComparisonFunction.Less;
            this.DepthWriteEnable = true;
            this.StencilEnable = false;
            this.StencilReadMask = (int)byte.MaxValue;
            this.StencilWriteMask = (int)byte.MaxValue;
            this.StencilReferenceValue = 0;
            this.StencilFrontFace = new StencilDescription()
            {
                Function = ComparisonFunction.Always,
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Keep,
                PassOperation = StencilOperation.Keep
            };
            this.StencilBackFace = new StencilDescription()
            {
                Function = ComparisonFunction.Always,
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Keep,
                PassOperation = StencilOperation.Keep
            };
        }

        public DepthStencilStateDescription Clone()
        {
            return (DepthStencilStateDescription)this.MemberwiseClone();
        }
    }
}
