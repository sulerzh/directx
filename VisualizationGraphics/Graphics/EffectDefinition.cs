using System.IO;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class EffectDefinition
    {
        public Stream VertexShaderData { get; set; }

        public Stream PixelShaderData { get; set; }

        public Stream GeometryShaderData { get; set; }

        public RenderParameters Parameters { get; set; }

        public RenderParameters[] SharedParameters { get; set; }

        public VertexFormat VertexFormat { get; set; }

        public TextureSampler[] Samplers { get; set; }

        public VertexFormat StreamFormat { get; set; }

        public EffectDebugInfo DebugInfo { get; set; }
    }
}
