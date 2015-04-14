using Microsoft.Data.Visualization.DirectX.Direct3D11;
using System.IO;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11StreamEffect : D3D11Effect
    {
        private D3D11VertexFormat streamFormat;

        public D3D11StreamEffect(EffectDefinition definition)
            : base(definition)
        {
            this.streamFormat = (D3D11VertexFormat)definition.StreamFormat;
        }

        private StreamOutputDeclarationEntry[] GetStreamFormat()
        {
            InputElementDescription[] format = this.streamFormat.GetFormat();
            StreamOutputDeclarationEntry[] declarationEntryArray = new StreamOutputDeclarationEntry[format.Length];
            for (int i = 0; i < format.Length; ++i)
            {
                declarationEntryArray[i].StreamIndex = 0U;
                declarationEntryArray[i].SemanticName = format[i].SemanticName;
                declarationEntryArray[i].SemanticIndex = format[i].SemanticIndex;
                declarationEntryArray[i].OutputSlot = (byte)format[i].InputSlot;
                declarationEntryArray[i].StartComponent = 0;
                switch (this.streamFormat.Components[i].DataType)
                {
                    case VertexComponentDataType.Float:
                        declarationEntryArray[i].ComponentCount = 1;
                        break;
                    case VertexComponentDataType.Float2:
                    case VertexComponentDataType.Short2AsFloats:
                        declarationEntryArray[i].ComponentCount = 2;
                        break;
                    case VertexComponentDataType.Float3:
                        declarationEntryArray[i].ComponentCount = 3;
                        break;
                    case VertexComponentDataType.Float4:
                        declarationEntryArray[i].ComponentCount = 4;
                        break;
                    default:
                        declarationEntryArray[i].ComponentCount = 0;
                        break;
                }
            }
            return declarationEntryArray;
        }

        protected override GeometryShader CreateGeometryShader(D3DDevice device, Stream data, Stream vsData)
        {
            Stream shaderStream = null;
            if (data != null)
                shaderStream = data;
            else if (vsData != null)
            {
                vsData.Seek(0L, SeekOrigin.Begin);
                shaderStream = vsData;
            }
            return device.CreateGeometryShaderWithStreamOutput(
                shaderStream, this.GetStreamFormat(),
                new uint[] { (uint)this.streamFormat.GetVertexSizeInBytes() },
                0U);
        }
    }
}
