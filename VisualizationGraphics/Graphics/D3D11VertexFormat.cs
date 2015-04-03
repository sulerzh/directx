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

        /// <summary>
        /// 根据VetexComponent列表初始化InputElementDescription数组
        /// </summary>
        /// <returns></returns>
        internal InputElementDescription[] GetFormat()
        {
            if (this.elements == null)
            {
                this.elements = new InputElementDescription[this.Components.Count];
                VertexSemantic[] vertexSemanticArray = (VertexSemantic[])Enum.GetValues(typeof(VertexSemantic));
                Dictionary<VertexSemantic, uint> semanticType2IndexMap = new Dictionary<VertexSemantic, uint>();
                // 初始化VertexSemantic枚举类型与索引的映射关系
                for (int i = 0; i < vertexSemanticArray.Length; ++i)
                    semanticType2IndexMap.Add(vertexSemanticArray[i], 0U);
                // 由VetexComponent构建IA描述结构
                for (int i = 0; i < this.Components.Count; ++i)
                {
                    InputElementDescription desc = new InputElementDescription();
                    switch (this.Components[i].Semantic)
                    {
                        case VertexSemantic.Position:
                            desc.SemanticName = "POSITION";
                            break;
                        case VertexSemantic.PositionT:
                            desc.SemanticName = "POSITIONT";
                            break;
                        case VertexSemantic.Normal:
                            desc.SemanticName = "NORMAL";
                            break;
                        case VertexSemantic.Binormal:
                            desc.SemanticName = "BINORMAL";
                            break;
                        case VertexSemantic.Tangent:
                            desc.SemanticName = "TANGENT";
                            break;
                        case VertexSemantic.Color:
                            desc.SemanticName = "COLOR";
                            break;
                        case VertexSemantic.TexCoord:
                            desc.SemanticName = "TEXCOORD";
                            break;
                        case VertexSemantic.BlendIndices:
                            desc.SemanticName = "BLENDINDICES";
                            break;
                    }
                    desc.SemanticIndex = semanticType2IndexMap[this.Components[i].Semantic];
                    semanticType2IndexMap[this.Components[i].Semantic] ++;
                    desc.Format = D3D11VertexFormat.GetD3DFormat(this.Components[i].DataType);
                    desc.AlignedByteOffset = uint.MaxValue;
                    desc.InputSlot = (uint)this.Components[i].Slot;
                    switch (this.Components[i].Classification)
                    {
                        case VertexComponentClassification.PerInstanceData:
                            desc.InputSlotClass = InputClassification.PerInstanceData;
                            desc.InstanceDataStepRate = 1U;
                            break;
                        default:
                            desc.InputSlotClass = InputClassification.PerVertexData;
                            desc.InstanceDataStepRate = 0U;
                            break;
                    }
                    this.elements[i] = desc;
                }
            }
            return this.elements;
        }

        /// <summary>
        /// Graphics VertexComponentDataType 转 DirectX11 Format
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
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
