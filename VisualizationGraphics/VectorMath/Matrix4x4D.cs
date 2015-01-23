using System;
using System.Globalization;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    [Serializable]
    public struct Matrix4x4D : IEquatable<Matrix4x4D>
    {
        public static readonly Matrix4x4D Identity = Matrix4x4D.MakeIdentity();
        public static readonly Matrix4x4D Zero = new Matrix4x4D();
        public const int SizeInMemory = 128;
        public double M11;
        public double M12;
        public double M13;
        public double M14;
        public double M21;
        public double M22;
        public double M23;
        public double M24;
        public double M31;
        public double M32;
        public double M33;
        public double M34;
        public double M41;
        public double M42;
        public double M43;
        public double M44;

        public double TranslationX
        {
            get
            {
                return this.M41;
            }
            set
            {
                this.M41 = value;
            }
        }

        public double TranslationY
        {
            get
            {
                return this.M42;
            }
            set
            {
                this.M42 = value;
            }
        }

        public double TranslationZ
        {
            get
            {
                return this.M43;
            }
            set
            {
                this.M43 = value;
            }
        }

        public bool IsZeroMatrix
        {
            get
            {
                return
                    this.M41 == 0.0 && this.M42 == 0.0 && this.M43 == 0.0 && this.M44 == 0.0 &&
                    this.M11 == 0.0 && this.M12 == 0.0 && this.M13 == 0.0 && this.M14 == 0.0 &&
                    this.M21 == 0.0 && this.M22 == 0.0 && this.M23 == 0.0 && this.M24 == 0.0 &&
                    this.M31 == 0.0 && this.M32 == 0.0 && this.M33 == 0.0 && this.M34 == 0.0;
            }
        }

        public bool IsIdentityMatrix
        {
            get
            {
                return
                    this.M41 == 0.0 && this.M42 == 0.0 && this.M43 == 0.0 && this.M44 == 1.0 &&
                    this.M11 == 1.0 && this.M12 == 0.0 && this.M13 == 0.0 && this.M14 == 0.0 &&
                    this.M21 == 0.0 && this.M22 == 1.0 && this.M23 == 0.0 && this.M24 == 0.0 &&
                    this.M31 == 0.0 && this.M32 == 0.0 && this.M33 == 1.0 && this.M34 == 0.0;
            }
        }

        public Matrix4x4D(
            double m11, double m12, double m13, double m14,
            double m21, double m22, double m23, double m24,
            double m31, double m32, double m33, double m34,
            double m41, double m42, double m43, double m44)
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

        public Matrix4x4D(Matrix4x4D matrix)
        {
            this.M11 = matrix.M11;
            this.M12 = matrix.M12;
            this.M13 = matrix.M13;
            this.M14 = matrix.M14;
            this.M21 = matrix.M21;
            this.M22 = matrix.M22;
            this.M23 = matrix.M23;
            this.M24 = matrix.M24;
            this.M31 = matrix.M31;
            this.M32 = matrix.M32;
            this.M33 = matrix.M33;
            this.M34 = matrix.M34;
            this.M41 = matrix.M41;
            this.M42 = matrix.M42;
            this.M43 = matrix.M43;
            this.M44 = matrix.M44;
        }

        private Matrix4x4D(ref Matrix4x4D matrix)
        {
            this.M11 = matrix.M11;
            this.M12 = matrix.M12;
            this.M13 = matrix.M13;
            this.M14 = matrix.M14;
            this.M21 = matrix.M21;
            this.M22 = matrix.M22;
            this.M23 = matrix.M23;
            this.M24 = matrix.M24;
            this.M31 = matrix.M31;
            this.M32 = matrix.M32;
            this.M33 = matrix.M33;
            this.M34 = matrix.M34;
            this.M41 = matrix.M41;
            this.M42 = matrix.M42;
            this.M43 = matrix.M43;
            this.M44 = matrix.M44;
        }

        public static explicit operator Matrix4x4F(Matrix4x4D matrix)
        {
            return new Matrix4x4F(matrix);
        }

        public static Matrix4x4D operator *(Matrix4x4D a, Matrix4x4D b)
        {
            return new Matrix4x4D(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
                a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
                a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
                a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
                a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
                a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
                a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);
        }

        public static Vector3D operator *(Vector3D vec, Matrix4x4D mat)
        {
            return new Vector3D(
                vec.X * mat.M11 + vec.Y * mat.M21 + vec.Z * mat.M31 + mat.M41,
                vec.X * mat.M12 + vec.Y * mat.M22 + vec.Z * mat.M32 + mat.M42,
                vec.X * mat.M13 + vec.Y * mat.M23 + vec.Z * mat.M33 + mat.M43);
        }

        public static bool operator ==(Matrix4x4D left, Matrix4x4D right)
        {
            return left.Equals(ref right);
        }

        public static bool operator !=(Matrix4x4D left, Matrix4x4D right)
        {
            return !left.Equals(ref right);
        }

        private static Matrix4x4D MakeIdentity()
        {
            return new Matrix4x4D(
                1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        private static void SinCos(double angle, out double sin, out double cos)
        {
            sin = Math.Sin(angle);
            cos = Math.Cos(angle);
        }

        public static Matrix4x4D Add(Matrix4x4D left, Matrix4x4D right)
        {
            return new Matrix4x4D(
                left.M11 + right.M11, left.M12 + right.M12, left.M13 + right.M13, left.M14 + right.M14,
                left.M21 + right.M21, left.M22 + right.M22, left.M23 + right.M23, left.M24 + right.M24,
                left.M31 + right.M31, left.M32 + right.M32, left.M33 + right.M33, left.M34 + right.M34,
                left.M41 + right.M41, left.M42 + right.M42, left.M43 + right.M43, left.M44 + right.M44);
        }

        public static Matrix4x4D Add(ref Matrix4x4D left, ref Matrix4x4D right)
        {
            return new Matrix4x4D(
                left.M11 + right.M11, left.M12 + right.M12, left.M13 + right.M13, left.M14 + right.M14,
                left.M21 + right.M21, left.M22 + right.M22, left.M23 + right.M23, left.M24 + right.M24,
                left.M31 + right.M31, left.M32 + right.M32, left.M33 + right.M33, left.M34 + right.M34,
                left.M41 + right.M41, left.M42 + right.M42, left.M43 + right.M43, left.M44 + right.M44);
        }

        public static Matrix4x4D Subtract(Matrix4x4D left, Matrix4x4D right)
        {
            return new Matrix4x4D(
                left.M11 - right.M11, left.M12 - right.M12, left.M13 - right.M13, left.M14 - right.M14,
                left.M21 - right.M21, left.M22 - right.M22, left.M23 - right.M23, left.M24 - right.M24,
                left.M31 - right.M31, left.M32 - right.M32, left.M33 - right.M33, left.M34 - right.M34,
                left.M41 - right.M41, left.M42 - right.M42, left.M43 - right.M43, left.M44 - right.M44);
        }

        public static Matrix4x4D Subtract(ref Matrix4x4D left, ref Matrix4x4D right)
        {
            return new Matrix4x4D(
                left.M11 - right.M11, left.M12 - right.M12, left.M13 - right.M13, left.M14 - right.M14,
                left.M21 - right.M21, left.M22 - right.M22, left.M23 - right.M23, left.M24 - right.M24,
                left.M31 - right.M31, left.M32 - right.M32, left.M33 - right.M33, left.M34 - right.M34,
                left.M41 - right.M41, left.M42 - right.M42, left.M43 - right.M43, left.M44 - right.M44);
        }

        public Vector3D Transform(Vector3D point)
        {
            double num = 1.0 / (point.X * this.M14 + point.Y * this.M24 + point.Z * this.M34 + this.M44);
            return new Vector3D(
                (point.X * this.M11 + point.Y * this.M21 + point.Z * this.M31 + this.M41) * num,
                (point.X * this.M12 + point.Y * this.M22 + point.Z * this.M32 + this.M42) * num,
                (point.X * this.M13 + point.Y * this.M23 + point.Z * this.M33 + this.M43) * num);
        }

        public Vector4D Transform(Vector4D p)
        {
            return new Vector4D(
                p.X * this.M11 + p.Y * this.M21 + p.Z * this.M31 + p.W * this.M41,
                p.X * this.M12 + p.Y * this.M22 + p.Z * this.M32 + p.W * this.M42,
                p.X * this.M13 + p.Y * this.M23 + p.Z * this.M33 + p.W * this.M43,
                p.X * this.M14 + p.Y * this.M24 + p.Z * this.M34 + p.W * this.M44);
        }

        public static Matrix4x4D Invert(Matrix4x4D source)
        {
            return Matrix4x4D.Invert(ref source);
        }

        public static Matrix4x4D Invert(ref Matrix4x4D source)
        {
            double num1 = source.M33 * source.M44 - source.M43 * source.M34;
            double num2 = source.M32 * source.M44 - source.M42 * source.M34;
            double num3 = source.M32 * source.M43 - source.M42 * source.M33;
            double num4 = source.M31 * source.M44 - source.M41 * source.M34;
            double num5 = source.M31 * source.M43 - source.M41 * source.M33;
            double num6 = source.M31 * source.M42 - source.M41 * source.M32;
            double num7 = source.M23 * source.M44 - source.M43 * source.M24;
            double num8 = source.M22 * source.M44 - source.M42 * source.M24;
            double num9 = source.M22 * source.M43 - source.M42 * source.M23;
            double num10 = source.M21 * source.M44 - source.M41 * source.M24;
            double num11 = source.M21 * source.M43 - source.M41 * source.M23;
            double num12 = source.M21 * source.M42 - source.M41 * source.M22;
            double num13 = source.M23 * source.M34 - source.M33 * source.M24;
            double num14 = source.M22 * source.M34 - source.M32 * source.M24;
            double num15 = source.M22 * source.M33 - source.M32 * source.M23;
            double num16 = source.M21 * source.M34 - source.M31 * source.M24;
            double num17 = source.M21 * source.M33 - source.M31 * source.M23;
            double num18 = source.M21 * source.M32 - source.M31 * source.M22;
            Matrix4x4D matrix4x4D = new Matrix4x4D(
                source.M22 * num1 - source.M23 * num2 + source.M24 * num3,
                source.M21 * num1 - source.M23 * num4 + source.M24 * num5,
                source.M21 * num2 - source.M22 * num4 + source.M24 * num6,
                source.M21 * num3 - source.M22 * num5 + source.M23 * num6,
                source.M12 * num1 - source.M13 * num2 + source.M14 * num3,
                source.M11 * num1 - source.M13 * num4 + source.M14 * num5,
                source.M11 * num2 - source.M12 * num4 + source.M14 * num6,
                source.M11 * num3 - source.M12 * num5 + source.M13 * num6,
                source.M12 * num7 - source.M13 * num8 + source.M14 * num9,
                source.M11 * num7 - source.M13 * num10 + source.M14 * num11,
                source.M11 * num8 - source.M12 * num10 + source.M14 * num12,
                source.M11 * num9 - source.M12 * num11 + source.M13 * num12,
                source.M12 * num13 - source.M13 * num14 + source.M14 * num15,
                source.M11 * num13 - source.M13 * num16 + source.M14 * num17,
                source.M11 * num14 - source.M12 * num16 + source.M14 * num18,
                source.M11 * num15 - source.M12 * num17 + source.M13 * num18);
            double num19 = source.M11 * matrix4x4D.M11 - source.M12 * matrix4x4D.M12 + (source.M13 * matrix4x4D.M13 - source.M14 * matrix4x4D.M14);
            if (num19 == 0.0)
                return Matrix4x4D.Zero;
            double num20 = 1.0 / num19;
            double num21 = -1.0 / num19;
            return new Matrix4x4D(
                matrix4x4D.M11 * num20, matrix4x4D.M21 * num21, matrix4x4D.M31 * num20, matrix4x4D.M41 * num21,
                matrix4x4D.M12 * num21, matrix4x4D.M22 * num20, matrix4x4D.M32 * num21, matrix4x4D.M42 * num20,
                matrix4x4D.M13 * num20, matrix4x4D.M23 * num21, matrix4x4D.M33 * num20, matrix4x4D.M43 * num21,
                matrix4x4D.M14 * num21, matrix4x4D.M24 * num20, matrix4x4D.M34 * num21, matrix4x4D.M44 * num20);
        }

        public static Matrix4x4D LookAtLH(Vector3D cameraPosition, Vector3D cameraTarget, Vector3D cameraUpVector)
        {
            Vector3D vector3D1 = Vector3D.Subtract(ref cameraTarget, ref cameraPosition);
            vector3D1.Normalize();
            Vector3D vector3D2 = Vector3D.Cross(ref cameraUpVector, ref vector3D1);
            vector3D2.Normalize();
            Vector3D left = Vector3D.Cross(ref vector3D1, ref vector3D2);
            return new Matrix4x4D(vector3D2.X, left.X, vector3D1.X, 0.0, vector3D2.Y, left.Y, vector3D1.Y, 0.0, vector3D2.Z, left.Z, vector3D1.Z, 0.0, -Vector3D.Dot(ref vector3D2, ref cameraPosition), -Vector3D.Dot(ref left, ref cameraPosition), -Vector3D.Dot(ref vector3D1, ref cameraPosition), 1.0);
        }

        public static Matrix4x4D LookAtRH(Vector3D cameraPosition, Vector3D cameraTarget, Vector3D cameraUpVector)
        {
            Vector3D vector3D1 = Vector3D.Subtract(ref cameraPosition, ref cameraTarget);
            vector3D1.Normalize();
            Vector3D vector3D2 = Vector3D.Cross(ref cameraUpVector, ref vector3D1);
            vector3D2.Normalize();
            Vector3D left = Vector3D.Cross(ref vector3D1, ref vector3D2);
            return new Matrix4x4D(vector3D2.X, left.X, vector3D1.X, 0.0, vector3D2.Y, left.Y, vector3D1.Y, 0.0, vector3D2.Z, left.Z, vector3D1.Z, 0.0, -Vector3D.Dot(ref vector3D2, ref cameraPosition), -Vector3D.Dot(ref left, ref cameraPosition), -Vector3D.Dot(ref vector3D1, ref cameraPosition), 1.0);
        }

        public static Matrix4x4D RotationYawPitchRoll(double yaw, double pitch, double roll)
        {
            double num1 = Math.Sin(pitch);
            double num2 = Math.Cos(pitch);
            double num3 = Math.Sin(yaw);
            double num4 = Math.Cos(yaw);
            double num5 = Math.Sin(roll);
            double num6 = Math.Cos(roll);
            return new Matrix4x4D(num6 * num4 + num5 * num1 * num3, num5 * num2, num6 * -num3 + num5 * num1 * num4, 0.0, -num5 * num4 + num6 * num1 * num3, num6 * num2, num5 * num3 + num6 * num1 * num4, 0.0, num2 * num3, -num1, num2 * num4, 0.0, 0.0, 0.0, 0.0, 1.0);
        }

        public static Matrix4x4D RotationAxis(Vector3D axisRotation, double sin, double cos)
        {
            axisRotation.Normalize();
            double num1 = 1.0 - cos;
            double num2 = axisRotation.X * axisRotation.X;
            double num3 = axisRotation.Y * axisRotation.Y;
            double num4 = axisRotation.Z * axisRotation.Z;
            double num5 = axisRotation.X * axisRotation.Y * num1;
            double num6 = axisRotation.X * axisRotation.Z * num1;
            double num7 = axisRotation.Y * axisRotation.Z * num1;
            double num8 = axisRotation.X * sin;
            double num9 = axisRotation.Y * sin;
            double num10 = axisRotation.Z * sin;
            return new Matrix4x4D(num2 * num1 + cos, num5 + num10, num6 - num9, 0.0, num5 - num10, num3 * num1 + cos, num7 + num8, 0.0, num6 + num9, num7 - num8, num4 * num1 + cos, 0.0, 0.0, 0.0, 0.0, 1.0);
        }

        public static Matrix4x4D RotationAxis(Vector3D axisRotation, double angle)
        {
            return Matrix4x4D.RotationAxis(axisRotation, Math.Sin(angle), Math.Cos(angle));
        }

        public void Multiply(ref Matrix4x4D right)
        {
            Matrix4x4D matrix4x4D = new Matrix4x4D(ref this);
            this.M11 = matrix4x4D.M11 * right.M11 + matrix4x4D.M12 * right.M21 + matrix4x4D.M13 * right.M31 + matrix4x4D.M14 * right.M41;
            this.M12 = matrix4x4D.M11 * right.M12 + matrix4x4D.M12 * right.M22 + matrix4x4D.M13 * right.M32 + matrix4x4D.M14 * right.M42;
            this.M13 = matrix4x4D.M11 * right.M13 + matrix4x4D.M12 * right.M23 + matrix4x4D.M13 * right.M33 + matrix4x4D.M14 * right.M43;
            this.M14 = matrix4x4D.M11 * right.M14 + matrix4x4D.M12 * right.M24 + matrix4x4D.M13 * right.M34 + matrix4x4D.M14 * right.M44;
            this.M21 = matrix4x4D.M21 * right.M11 + matrix4x4D.M22 * right.M21 + matrix4x4D.M23 * right.M31 + matrix4x4D.M24 * right.M41;
            this.M22 = matrix4x4D.M21 * right.M12 + matrix4x4D.M22 * right.M22 + matrix4x4D.M23 * right.M32 + matrix4x4D.M24 * right.M42;
            this.M23 = matrix4x4D.M21 * right.M13 + matrix4x4D.M22 * right.M23 + matrix4x4D.M23 * right.M33 + matrix4x4D.M24 * right.M43;
            this.M24 = matrix4x4D.M21 * right.M14 + matrix4x4D.M22 * right.M24 + matrix4x4D.M23 * right.M34 + matrix4x4D.M24 * right.M44;
            this.M31 = matrix4x4D.M31 * right.M11 + matrix4x4D.M32 * right.M21 + matrix4x4D.M33 * right.M31 + matrix4x4D.M34 * right.M41;
            this.M32 = matrix4x4D.M31 * right.M12 + matrix4x4D.M32 * right.M22 + matrix4x4D.M33 * right.M32 + matrix4x4D.M34 * right.M42;
            this.M33 = matrix4x4D.M31 * right.M13 + matrix4x4D.M32 * right.M23 + matrix4x4D.M33 * right.M33 + matrix4x4D.M34 * right.M43;
            this.M34 = matrix4x4D.M31 * right.M14 + matrix4x4D.M32 * right.M24 + matrix4x4D.M33 * right.M34 + matrix4x4D.M34 * right.M44;
            this.M41 = matrix4x4D.M41 * right.M11 + matrix4x4D.M42 * right.M21 + matrix4x4D.M43 * right.M31 + matrix4x4D.M44 * right.M41;
            this.M42 = matrix4x4D.M41 * right.M12 + matrix4x4D.M42 * right.M22 + matrix4x4D.M43 * right.M32 + matrix4x4D.M44 * right.M42;
            this.M43 = matrix4x4D.M41 * right.M13 + matrix4x4D.M42 * right.M23 + matrix4x4D.M43 * right.M33 + matrix4x4D.M44 * right.M43;
            this.M44 = matrix4x4D.M41 * right.M14 + matrix4x4D.M42 * right.M24 + matrix4x4D.M43 * right.M34 + matrix4x4D.M44 * right.M44;
        }

        public void Multiply(Matrix4x4D right)
        {
            this.Multiply(ref right);
        }

        public static Matrix4x4D Multiply(Matrix4x4D left, Matrix4x4D right)
        {
            return Matrix4x4D.Multiply(ref left, ref right);
        }

        public static Matrix4x4D Multiply(ref Matrix4x4D left, ref Matrix4x4D right)
        {
            return new Matrix4x4D(left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41, left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42, left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43, left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44, left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41, left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42, left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43, left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44, left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41, left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42, left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43, left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44, left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41, left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42, left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43, left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
        }

        public static Matrix4x4D PerspectiveFovLH(double fieldOfViewY, double aspectRatio, double zNearPlane, double zFarPlane, double offset = 0.0)
        {
            double m22 = 1.0 / Math.Tan(fieldOfViewY / 2.0);
            double m11 = m22 / aspectRatio;
            double num = zFarPlane - zNearPlane;
            return new Matrix4x4D(
                m11, 0.0, 0.0, 0.0,
                0.0, m22, 0.0, 0.0,
                0.0, 0.0, zFarPlane / num, 1.0,
                0.0, 0.0, -zNearPlane * zFarPlane / num + offset, 0.0);
        }

        public static Matrix4x4D PerspectiveFovRH(double fieldOfViewY, double aspectRatio, double zNearPlane, double zFarPlane)
        {
            double m22 = 1.0 / Math.Tan(fieldOfViewY / 2.0);
            double m11 = m22 / aspectRatio;
            double num = zNearPlane - zFarPlane;
            return new Matrix4x4D(
                m11, 0.0, 0.0, 0.0,
                0.0, m22, 0.0, 0.0,
                0.0, 0.0, zFarPlane / num, -1.0,
                0.0, 0.0, zNearPlane * zFarPlane / num, 0.0);
        }

        public static Matrix4x4D OrthoOffCenterRH(double left, double right, double bottom, double top, double znear, double zfar)
        {
            return new Matrix4x4D(
                2.0 / (right - left), 0.0, 0.0, 0.0,
                0.0, 2.0 / (top - bottom), 0.0, 0.0,
                0.0, 0.0, 1.0 / (znear - zfar), 0.0,
                (left + right) / (left - right), (top + bottom) / (bottom - top), znear / (znear - zfar), 1.0);
        }

        public static Matrix4x4D RotationX(double angle)
        {
            double sin;
            double cos;
            Matrix4x4D.SinCos(angle, out sin, out cos);
            return new Matrix4x4D(
                1.0, 0.0, 0.0, 0.0,
                0.0, cos, sin, 0.0,
                0.0, -sin, cos, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        public static Matrix4x4D RotationY(double angle)
        {
            double sin;
            double cos;
            Matrix4x4D.SinCos(angle, out sin, out cos);
            return new Matrix4x4D(
                cos, 0.0, -sin, 0.0,
                0.0, 1.0, 0.0, 0.0,
                sin, 0.0, cos, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        public static Matrix4x4D RotationZ(double angle)
        {
            double sin;
            double cos;
            Matrix4x4D.SinCos(angle, out sin, out cos);
            return new Matrix4x4D(
                cos, sin, 0.0, 0.0,
                -sin, cos, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        public static Matrix4x4D Scaling(double valueX, double valueY, double valueZ)
        {
            return new Matrix4x4D(
                valueX, 0.0, 0.0, 0.0,
                0.0, valueY, 0.0, 0.0,
                0.0, 0.0, valueZ, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        public static Matrix4x4D TransposeMatrix(Matrix4x4D source)
        {
            return new Matrix4x4D(
                source.M11, source.M21, source.M31, source.M41,
                source.M12, source.M22, source.M32, source.M42,
                source.M13, source.M23, source.M33, source.M43,
                source.M14, source.M24, source.M34, source.M44);
        }

        public static Matrix4x4D Translation(double offsetX, double offsetY, double offsetZ)
        {
            return new Matrix4x4D(
                1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                offsetX, offsetY, offsetZ, 1.0);
        }

        public static Matrix4x4D Translation(Vector3D offset)
        {
            return new Matrix4x4D(
                1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                offset.X, offset.Y, offset.Z, 1.0);
        }

        public bool Equals(Matrix4x4D other)
        {
            return
                this.M11 == other.M11 && this.M12 == other.M12 && this.M13 == other.M13 && this.M14 == other.M14 &&
                this.M21 == other.M21 && this.M22 == other.M22 && this.M23 == other.M23 && this.M24 == other.M24 &&
                this.M31 == other.M31 && this.M32 == other.M32 && this.M33 == other.M33 && this.M34 == other.M34 &&
                this.M41 == other.M41 && this.M42 == other.M42 && this.M43 == other.M43 && this.M44 == other.M44;
        }

        public bool Equals(ref Matrix4x4D other)
        {
            return
                this.M11 == other.M11 && this.M12 == other.M12 && this.M13 == other.M13 && this.M14 == other.M14 &&
                this.M21 == other.M21 && this.M22 == other.M22 && this.M23 == other.M23 && this.M24 == other.M24 &&
                this.M31 == other.M31 && this.M32 == other.M32 && this.M33 == other.M33 && this.M34 == other.M34 &&
                this.M41 == other.M41 && this.M42 == other.M42 && this.M43 == other.M43 && this.M44 == other.M44;
        }

        public bool EqualsWithinEpsilon(ref Matrix4x4D other, double eps)
        {
            if (Math.Abs(this.M11 - other.M11) < eps && Math.Abs(this.M12 - other.M12) < eps && (Math.Abs(this.M13 - other.M13) < eps && Math.Abs(this.M14 - other.M14) < eps) && (Math.Abs(this.M21 - other.M21) < eps && Math.Abs(this.M22 - other.M22) < eps && (Math.Abs(this.M23 - other.M23) < eps && Math.Abs(this.M24 - other.M24) < eps)) && (Math.Abs(this.M31 - other.M31) < eps && Math.Abs(this.M32 - other.M32) < eps && (Math.Abs(this.M33 - other.M33) < eps && Math.Abs(this.M34 - other.M34) < eps) && (Math.Abs(this.M41 - other.M41) < eps && Math.Abs(this.M42 - other.M42) < eps && Math.Abs(this.M43 - other.M43) < eps)))
                return Math.Abs(this.M44 - other.M44) < eps;
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix4x4D)
                return this.Equals((Matrix4x4D)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return
                (int)this.M11 ^ (int)this.M12 ^ (int)this.M13 ^ (int)this.M14 ^ 
                (int)this.M21 ^ (int)this.M22 ^ (int)this.M23 ^ (int)this.M24 ^ 
                (int)this.M31 ^ (int)this.M32 ^ (int)this.M33 ^ (int)this.M34 ^
                (int)this.M41 ^ (int)this.M42 ^ (int)this.M43 ^ (int)this.M44;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "[{0},{1},{2},{3}][{4},{5},{6},{7}][{8},{9},{10},{11}][{12},{13},{14},{15}]", 
                this.M11, this.M12, this.M13, this.M14, 
                this.M21, this.M22, this.M23, this.M24, 
                this.M31, this.M32, this.M33, this.M34, 
                this.M41, this.M42, this.M43, this.M44);
        }
    }
}
