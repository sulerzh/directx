using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class VertexComponent : IEquatable<VertexComponent>
    {
        public int Slot { get; private set; }

        public VertexSemantic Semantic { get; private set; }

        public VertexComponentDataType DataType { get; private set; }

        public VertexComponentClassification Classification { get; private set; }

        public int SizeInBytes { get; private set; }

        public VertexComponent(VertexSemantic semantic, VertexComponentDataType type)
            : this(semantic, type, VertexComponentClassification.PerVertexData, 0)
        {
        }

        public VertexComponent(VertexSemantic semantic, VertexComponentDataType type, VertexComponentClassification classification, int slot)
        {
            this.Semantic = semantic;
            this.DataType = type;
            this.Slot = slot;
            this.Classification = classification;
            this.SizeInBytes = VertexComponent.GetComponentSize(type);
            if (this.SizeInBytes == 0)
                throw new ArgumentException("type");
        }

        public bool Equals(VertexComponent other)
        {
            if (this.Semantic == other.Semantic && this.DataType == other.DataType && this.Classification == other.Classification)
                return this.SizeInBytes == other.SizeInBytes;
            else
                return false;
        }

        internal static int GetComponentSize(VertexComponentDataType type)
        {
            switch (type)
            {
                case VertexComponentDataType.Float:
                case VertexComponentDataType.UInt:
                case VertexComponentDataType.Int:
                    return 4;
                case VertexComponentDataType.Float2:
                case VertexComponentDataType.UInt2:
                    return 8;
                case VertexComponentDataType.Float3:
                case VertexComponentDataType.UInt3:
                    return 12;
                case VertexComponentDataType.Float4:
                case VertexComponentDataType.UInt4:
                    return 16;
                case VertexComponentDataType.Short2AsFloats:
                case VertexComponentDataType.Short2:
                    return 4;
                case VertexComponentDataType.Short4AsFloats:
                    return 8;
                case VertexComponentDataType.Short4:
                    return 8;
                case VertexComponentDataType.UnsignedByte4AsFloats:
                    return 4;
                default:
                    return 0;
            }
        }
    }
}
