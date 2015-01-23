using Microsoft.Data.Visualization.DirectX.Direct3D11;
using Microsoft.Data.Visualization.DirectX.Graphics;
namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11TextureSampler : TextureSampler
    {
        private SamplerState samplerState;

        public D3D11TextureSampler(TextureFilter filter, TextureAddressMode addressU, TextureAddressMode addressV, Color4F borderColor, bool comparisonEnabled, ComparisonFunction comparison)
            : base(filter, addressU, addressV, borderColor, comparisonEnabled, comparison)
        {
        }

        internal void SetSampler(D3DDevice device, DeviceContext context, int slot)
        {
            if (this.samplerState == null)
            {
                this.Register((Renderer)device.Tag);
                this.samplerState = device.CreateSamplerState(
                    new SamplerDescription()
                    {
                        AddressU = D3D11TextureSampler.GetAddress(this.AddressU),
                        AddressV = D3D11TextureSampler.GetAddress(this.AddressV),
                        AddressW = Microsoft.Data.Visualization.DirectX.Direct3D11.TextureAddressMode.Border,
                        BorderColor = new ColorRgba(this.BorderColor.Rgba()),
                        Filter = D3D11TextureSampler.GetFilter(this.Filter, this.ComparisonEnabled),
                        ComparisonFunction = D3D11DepthStencilState.GetCompare(this.Comparison),
                        MaxAnisotropy = 16U,
                        MipLevelOfDetailBias = 0.0f,
                        MinimumLevelOfDetail = 0.0f,
                        MaximumLevelOfDetail = float.MaxValue
                    });
            }
            context.VS.SetSamplers((uint)slot, new SamplerState[] { this.samplerState });
            context.PS.SetSamplers((uint)slot, new SamplerState[] { this.samplerState });
        }

        private static Filter GetFilter(TextureFilter filter, bool comparison)
        {
            switch (filter)
            {
                case TextureFilter.MinMagPointMipLinear:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.MinMagPointMipLinear :
                        DirectX.Direct3D11.Filter.ComparisonMinMagPointMipLinear;
                case TextureFilter.MinPointMagLinearMipPoint:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.MinPointMagLinearMipPoint :
                        DirectX.Direct3D11.Filter.ComparisonMinPointMagLinearMipPoint;
                case TextureFilter.MinPointMagMipLinear:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.MinPointMagMipLinear :
                        DirectX.Direct3D11.Filter.ComparisonMinPointMagMipLinear;
                case TextureFilter.MinLinearMagMipPoint:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.MinLinearMagMipPoint :
                        DirectX.Direct3D11.Filter.ComparisonMinLinearMagMipPoint;
                case TextureFilter.MinLinearMagPointMipLinear:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.MinLinearMagPointMipLinear :
                        DirectX.Direct3D11.Filter.ComparisonMinLinearMagPointMipLinear;
                case TextureFilter.MinMagLinearMipPoint:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.MinMagLinearMipPoint :
                        DirectX.Direct3D11.Filter.ComparisonMinMagLinearMipPoint;
                case TextureFilter.Linear:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.MinMagMipLinear :
                        DirectX.Direct3D11.Filter.ComparisonMinMagMipLinear;
                case TextureFilter.Anisotropic:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.Anisotropic :
                        DirectX.Direct3D11.Filter.ComparisonAnisotropic;
                default:
                    return !comparison ?
                        DirectX.Direct3D11.Filter.MinMagMipPoint :
                        DirectX.Direct3D11.Filter.ComparisonMinMagMipPoint;
            }
        }

        private static DirectX.Direct3D11.TextureAddressMode GetAddress(TextureAddressMode mode)
        {
            switch (mode)
            {
                case TextureAddressMode.Wrap:
                    return DirectX.Direct3D11.TextureAddressMode.Wrap;
                case TextureAddressMode.Mirror:
                    return DirectX.Direct3D11.TextureAddressMode.Mirror;
                case TextureAddressMode.Border:
                    return DirectX.Direct3D11.TextureAddressMode.Border;
                default:
                    return DirectX.Direct3D11.TextureAddressMode.Clamp;
            }
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            return 0;
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            return 0;
        }

        protected override bool Reset()
        {
            this.ReleaseGraphicsResources();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            this.ReleaseGraphicsResources();
        }

        private void ReleaseGraphicsResources()
        {
            if (this.samplerState == null)
                return;
            this.samplerState.Dispose();
            this.samplerState = null;
        }
    }
}
