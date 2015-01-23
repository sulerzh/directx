namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    public struct Vector4F
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4F(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public Vector4F(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = 1f;
        }
    }
}
