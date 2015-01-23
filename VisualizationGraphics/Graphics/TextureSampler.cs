namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class TextureSampler : GraphicsResource
    {
        public TextureFilter Filter { get; private set; }

        public TextureAddressMode AddressU { get; private set; }

        public TextureAddressMode AddressV { get; private set; }

        public Color4F BorderColor { get; private set; }

        public ComparisonFunction Comparison { get; private set; }

        public bool ComparisonEnabled { get; private set; }

        protected TextureSampler(TextureFilter filter, TextureAddressMode addressU, TextureAddressMode addressV, Color4F borderColor, bool comparisonEnabled, ComparisonFunction comparison)
        {
            this.Filter = filter;
            this.AddressU = addressU;
            this.AddressV = addressV;
            this.BorderColor = borderColor;
            this.Comparison = comparison;
            this.ComparisonEnabled = comparisonEnabled;
        }

        public static TextureSampler Create(TextureFilter filter, TextureAddressMode addressU, TextureAddressMode addressV, Color4F borderColor)
        {
            return new D3D11TextureSampler(filter, addressU, addressV, borderColor, false, ComparisonFunction.Never);
        }

        public static TextureSampler Create(TextureFilter filter, TextureAddressMode addressU, TextureAddressMode addressV, Color4F borderColor, ComparisonFunction comparison)
        {
            return new D3D11TextureSampler(filter, addressU, addressV, borderColor, true, comparison);
        }
    }
}
