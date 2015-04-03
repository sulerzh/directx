namespace Microsoft.Data.Visualization.Engine.Graphics
{
    // 预定义VetexFormat
    public static class VertexFormats
    {
        public static VertexFormat PositionOnly = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3));
        public static VertexFormat Position2D = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float2));
        public static VertexFormat Position2DColor = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float2), 
            new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
        public static VertexFormat PositionColorSeparateStreams = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerVertexData, 0), 
            new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats, VertexComponentClassification.PerVertexData, 1));
        public static VertexFormat PositionColor = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
        public static VertexFormat PositionColor2 = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats), 
            new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
        public static VertexFormat PositionTextured = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2));
        public static VertexFormat PositionNormalTextured = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.Normal, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2));
        public static VertexFormat PositionNormalTextured2 = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.Normal, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2), 
            new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2));
        public static VertexFormat PositionNormalColor = VertexFormat.Create(
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.Normal, VertexComponentDataType.Float3), 
            new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
    }

    // 预定义Vetex结构体
    public static class Vertex
    {
        public struct PositionOnly : IVertex
        {
            public float X;
            public float Y;
            public float Z;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.PositionOnly;
                }
            }

            public PositionOnly(float x, float y, float z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }
        }

        public struct Position2D : IVertex
        {
            public float X;
            public float Y;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.Position2D;
                }
            }

            public Position2D(float x, float y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        public struct Position2DColor : IVertex
        {
            public float X;
            public float Y;
            public uint Color;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.Position2DColor;
                }
            }

            public Position2DColor(float x, float y, uint color)
            {
                this.X = x;
                this.Y = y;
                this.Color = color;
            }
        }

        public struct PositionColor : IVertex
        {
            public float X;
            public float Y;
            public float Z;
            public uint Color1;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.PositionColor;
                }
            }
        }

        public struct PositionColorSeparateStreams : IVertex
        {
            public float X;
            public float Y;
            public float Z;
            public uint Color1;

            public Vertex.PositionColorSeparateStreams.Position PositionValue
            {
                get
                {
                    return new Vertex.PositionColorSeparateStreams.Position(this.X, this.Y, this.Z);
                }
            }

            public Vertex.PositionColorSeparateStreams.Color ColorValue
            {
                get
                {
                    return new Vertex.PositionColorSeparateStreams.Color(this.Color1);
                }
            }

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.PositionColorSeparateStreams;
                }
            }

            public struct Position : IVertex
            {
                public float X;
                public float Y;
                public float Z;

                public VertexFormat Format
                {
                    get
                    {
                        return VertexFormats.PositionColorSeparateStreams.StreamFormats[0];
                    }
                }

                public Position(float x, float y, float z)
                {
                    this.X = x;
                    this.Y = y;
                    this.Z = z;
                }
            }

            public struct Color : IVertex
            {
                public uint Color1;

                public VertexFormat Format
                {
                    get
                    {
                        return VertexFormats.PositionColorSeparateStreams.StreamFormats[1];
                    }
                }

                public Color(uint color)
                {
                    this.Color1 = color;
                }
            }
        }

        public struct PositionColor2 : IVertex
        {
            public float X;
            public float Y;
            public float Z;
            public uint Color1;
            public uint Color2;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.PositionColor2;
                }
            }
        }

        public struct PositionTextured : IVertex
        {
            public float X;
            public float Y;
            public float Z;
            public float TU;
            public float TV;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.PositionTextured;
                }
            }
        }

        public struct PositionNormalTextured : IVertex
        {
            public float X;
            public float Y;
            public float Z;
            public float NX;
            public float NY;
            public float NZ;
            public float TU;
            public float TV;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.PositionNormalTextured;
                }
            }
        }

        public struct PositionNormalTextured2 : IVertex
        {
            public float X;
            public float Y;
            public float Z;
            public float NX;
            public float NY;
            public float NZ;
            public float TU1;
            public float TV1;
            public float TU2;
            public float TV2;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.PositionNormalTextured2;
                }
            }
        }

        public struct PositionNormalColor : IVertex
        {
            public float X;
            public float Y;
            public float Z;
            public float NX;
            public float NY;
            public float NZ;
            public uint Color;

            public VertexFormat Format
            {
                get
                {
                    return VertexFormats.PositionNormalColor;
                }
            }
        }
    }
}
