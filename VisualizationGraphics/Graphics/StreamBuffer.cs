namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class StreamBuffer : GraphicsResource
    {
        public int VertexCount { get; private set; }

        public VertexFormat VertexFormat { get; private set; }

        public bool GetDataEnabled { get; private set; }

        public StreamBuffer(int vertexCount)
        {
            this.VertexCount = vertexCount;
        }

        public static StreamBuffer Create(VertexFormat format, int vertexCount, bool getDataEnabled)
        {
            D3D11StreamBuffer d3D11StreamBuffer = new D3D11StreamBuffer(format, vertexCount);
            d3D11StreamBuffer.GetDataEnabled = getDataEnabled;
            d3D11StreamBuffer.VertexFormat = format;
            return (StreamBuffer)d3D11StreamBuffer;
        }

        public abstract VertexType[] GetData<VertexType>(out int vertexCount) where VertexType : struct, IVertex;

        public abstract VertexType[] GetDataImmediate<VertexType>(out int vertexCount) where VertexType : struct, IVertex;

        public abstract VertexBuffer GetVertexBuffer(out int vertexCount);

        public abstract VertexBuffer PeekVertexBuffer();

        public abstract void ResetBuffer();
    }
}
