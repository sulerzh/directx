namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public static class VertexFormats
    {
        public static VertexFormat PositionOnly = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3));
        public static VertexFormat Position2D = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float2));
        public static VertexFormat Position2DColor = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float2), new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
        public static VertexFormat PositionColorSeparateStreams = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerVertexData, 0), new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats, VertexComponentClassification.PerVertexData, 1));
        public static VertexFormat PositionColor = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
        public static VertexFormat PositionColor2 = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats), new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
        public static VertexFormat PositionTextured = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2));
        public static VertexFormat PositionNormalTextured = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.Normal, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2));
        public static VertexFormat PositionNormalTextured2 = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.Normal, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2), new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2));
        public static VertexFormat PositionNormalColor = VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.Normal, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
    }
}
