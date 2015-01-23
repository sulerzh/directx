using Microsoft.Data.Visualization.DirectX.Direct3D11;
using Microsoft.Data.Visualization.DirectX.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11VertexFormat : VertexFormat
    {
        private InputElementDescription[] elements;

        public D3D11VertexFormat(VertexComponent component0)
            : base(component0)
        {
        }

        public D3D11VertexFormat(VertexComponent component0, VertexComponent component1)
            : base(component0, component1)
        {
        }

        public D3D11VertexFormat(VertexComponent component0, VertexComponent component1, VertexComponent component2)
            : base(component0, component1, component2)
        {
        }

        public D3D11VertexFormat(VertexComponent component0, VertexComponent component1, VertexComponent component2, VertexComponent component3)
            : base(component0, component1, component2, component3)
        {
        }

        public D3D11VertexFormat(VertexComponent[] components)
            : base(components)
        {
        }

        internal InputElementDescription[] GetFormat()
        {
            if (this.elements == null)
            {
                this.elements = new InputElementDescription[this.Components.Count];
                VertexSemantic[] vertexSemanticArray = (VertexSemantic[])Enum.GetValues(typeof(VertexSemantic));
                Dictionary<VertexSemantic, uint> dictionary1 = new Dictionary<VertexSemantic, uint>();
                for (int index = 0; index < vertexSemanticArray.Length; ++index)
                    dictionary1.Add(vertexSemanticArray[index], 0U);
                for (int index = 0; index < this.Components.Count; ++index)
                {
                    InputElementDescription elementDescription = new InputElementDescription();
                    switch (this.Components[index].Semantic)
                    {
                        case VertexSemantic.Position:
                            elementDescription.SemanticName = "POSITION";
                            break;
                        case VertexSemantic.PositionT:
                            elementDescription.SemanticName = "POSITIONT";
                            break;
                        case VertexSemantic.Normal:
                            elementDescription.SemanticName = "NORMAL";
                            break;
                        case VertexSemantic.Binormal:
                            elementDescription.SemanticName = "BINORMAL";
                            break;
                        case VertexSemantic.Tangent:
                            elementDescription.SemanticName = "TANGENT";
                            break;
                        case VertexSemantic.Color:
                            elementDescription.SemanticName = "COLOR";
                            break;
                        case VertexSemantic.TexCoord:
                            elementDescription.SemanticName = "TEXCOORD";
                            break;
                        case VertexSemantic.BlendIndices:
                            elementDescription.SemanticName = "BLENDINDICES";
                            break;
                    }
                    elementDescription.SemanticIndex = dictionary1[this.Components[index].Semantic];
                    Dictionary<VertexSemantic, uint> dictionary2;
                    VertexSemantic semantic;
                    (dictionary2 = dictionary1)[semantic = this.Components[index].Semantic] = dictionary2[semantic] + 1U;
                    elementDescription.Format = D3D11VertexFormat.GetD3DFormat(this.Components[index].DataType);
                    elementDescription.AlignedByteOffset = uint.MaxValue;
                    elementDescription.InputSlot = (uint)this.Components[index].Slot;
                    switch (this.Components[index].Classification)
                    {
                        case VertexComponentClassification.PerInstanceData:
                            elementDescription.InputSlotClass = InputClassification.PerInstanceData;
                            elementDescription.InstanceDataStepRate = 1U;
                            break;
                        default:
                            elementDescription.InputSlotClass = InputClassification.PerVertexData;
                            elementDescription.InstanceDataStepRate = 0U;
                            break;
                    }
                    this.elements[index] = elementDescription;
                }
            }
            return this.elements;
        }

        internal static Format GetD3DFormat(VertexComponentDataType dataType)
        {
            switch (dataType)
            {
                case VertexComponentDataType.Float:
                    return Format.R32Float;
                case VertexComponentDataType.Float2:
                    return Format.R32G32Float;
                case VertexComponentDataType.Float3:
                    return Format.R32G32B32Float;
                case VertexComponentDataType.Float4:
                    return Format.R32G32B32A32Float;
                case VertexComponentDataType.UInt:
                    return Format.R32UInt;
                case VertexComponentDataType.UInt2:
                    return Format.R32G32UInt;
                case VertexComponentDataType.UInt3:
                    return Format.R32G32B32UInt;
                case VertexComponentDataType.UInt4:
                    return Format.R32G32B32A32UInt;
                case VertexComponentDataType.Int:
                    return Format.R32SInt;
                case VertexComponentDataType.Short2AsFloats:
                    return Format.R16G16SNorm;
                case VertexComponentDataType.Short4AsFloats:
                    return Format.R16G16B16A16SNorm;
                case VertexComponentDataType.Short2:
                    return Format.R16G16SInt;
                case VertexComponentDataType.Short4:
                    return Format.R16G16B16A16SInt;
                case VertexComponentDataType.UnsignedByte4AsFloats:
                    return Format.R8G8B8A8UNorm;
                default:
                    return Format.Unknown;
            }
        }
    }
}
