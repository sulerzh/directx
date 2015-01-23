namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    public struct Matrix4x4F
    {
        private static Matrix4x4F identity =
            new Matrix4x4F(
                1f, 0.0f, 0.0f, 0.0f,
                0.0f, 1f, 0.0f, 0.0f,
                0.0f, 0.0f, 1f, 0.0f,
                0.0f, 0.0f, 0.0f, 1f);
        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M41;
        public float M42;
        public float M43;
        public float M44;

        public static Matrix4x4F Identity
        {
            get
            {
                return Matrix4x4F.identity;
            }
        }

        public Matrix4x4F(
            float m11, float m12, float m13, float m14, 
            float m21, float m22, float m23, float m24, 
            float m31, float m32, float m33, float m34, 
            float m41, float m42, float m43, float m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        public Matrix4x4F(Matrix4x4D matrix)
        {
            this.M11 = (float)matrix.M11;
            this.M12 = (float)matrix.M12;
            this.M13 = (float)matrix.M13;
            this.M14 = (float)matrix.M14;
            this.M21 = (float)matrix.M21;
            this.M22 = (float)matrix.M22;
            this.M23 = (float)matrix.M23;
            this.M24 = (float)matrix.M24;
            this.M31 = (float)matrix.M31;
            this.M32 = (float)matrix.M32;
            this.M33 = (float)matrix.M33;
            this.M34 = (float)matrix.M34;
            this.M41 = (float)matrix.M41;
            this.M42 = (float)matrix.M42;
            this.M43 = (float)matrix.M43;
            this.M44 = (float)matrix.M44;
        }
    }
}
