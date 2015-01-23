using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    [Serializable]
    public struct Vector3D : IEquatable<Vector3D>
    {
        public static readonly Vector3D Empty = new Vector3D();
        public static readonly Vector3D XVector = new Vector3D(1.0, 0.0, 0.0);
        public static readonly Vector3D YVector = new Vector3D(0.0, 1.0, 0.0);
        public static readonly Vector3D ZVector = new Vector3D(0.0, 0.0, 1.0);
        internal const double SmallNumber = 1E-14;
        public double X;
        public double Y;
        public double Z;

        public double this[int componentIdx]
        {
            get
            {
                switch (componentIdx)
                {
                    case 0:
                        return this.X;
                    case 1:
                        return this.Y;
                    case 2:
                        return this.Z;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (componentIdx)
                {
                    case 0:
                        this.X = value;
                        break;
                    case 1:
                        this.Y = value;
                        break;
                    case 2:
                        this.Z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Vector3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3D(Vector3D other)
        {
            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;
        }

        public static explicit operator Vector3F(Vector3D V)
        {
            return new Vector3F(V);
        }

        public static explicit operator Vector4F(Vector3D vector)
        {
            return new Vector4F((float)vector.X, (float)vector.Y, (float)vector.Z, 1f);
        }

        public static Vector3D operator +(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3D operator *(double left, Vector3D right)
        {
            return new Vector3D(left * right.X, left * right.Y, left * right.Z);
        }

        public static Vector3D operator *(Vector3D left, double right)
        {
            return new Vector3D(left.X * right, left.Y * right, left.Z * right);
        }

        public static Vector3D operator /(Vector3D left, double right)
        {
            return new Vector3D(left.X / right, left.Y / right, left.Z / right);
        }

        public static bool operator ==(Vector3D left, Vector3D right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }

        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
        }

        public static Vector3D operator -(Vector3D vec)
        {
            return vec.Negate();
        }

        public static double operator *(Vector3D vec0, Vector3D vec1)
        {
            return vec0.Dot(vec1);
        }

        public static Vector3D operator ^(Vector3D vec0, Vector3D vec1)
        {
            return new Vector3D(vec0.Y * vec1.Z - vec0.Z * vec1.Y, vec0.Z * vec1.X - vec0.X * vec1.Z, vec0.X * vec1.Y - vec0.Y * vec1.X);
        }

        public void AddTo(Vector3D right)
        {
            this.X += right.X;
            this.Y += right.Y;
            this.Z += right.Z;
        }

        public void AddTo(ref Vector3D right)
        {
            this.X += right.X;
            this.Y += right.Y;
            this.Z += right.Z;
        }

        public Vector2D XY()
        {
            return new Vector2D(this.X, this.Y);
        }

        public static Vector3D Add(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Vector3D Add(ref Vector3D left, ref Vector3D right)
        {
            return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public void SubtractFrom(Vector3D right)
        {
            this.X -= right.X;
            this.Y -= right.Y;
            this.Z -= right.Z;
        }

        public void SubtractFrom(ref Vector3D right)
        {
            this.X -= right.X;
            this.Y -= right.Y;
            this.Z -= right.Z;
        }

        public static Vector3D Subtract(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3D Subtract(ref Vector3D left, ref Vector3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3D Cross(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.Y * right.Z - left.Z * right.Y, left.Z * right.X - left.X * right.Z, left.X * right.Y - left.Y * right.X);
        }

        public static Vector3D Cross(ref Vector3D left, ref Vector3D right)
        {
            return new Vector3D(left.Y * right.Z - left.Z * right.Y, left.Z * right.X - left.X * right.Z, left.X * right.Y - left.Y * right.X);
        }

        public static double Dot(Vector3D left, Vector3D right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static double Dot(ref Vector3D left, ref Vector3D right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static double Distance(Vector3D pointA, Vector3D pointB)
        {
            return Math.Sqrt(Vector3D.DistanceSq(ref pointA, ref pointB));
        }

        public static double Distance(ref Vector3D pointA, ref Vector3D pointB)
        {
            return Math.Sqrt(Vector3D.DistanceSq(ref pointA, ref pointB));
        }

        public static double DistanceSq(Vector3D pointA, Vector3D pointB)
        {
            double disX = pointA.X - pointB.X;
            double disY = pointA.Y - pointB.Y;
            double disZ = pointA.Z - pointB.Z;
            return disX * disX + disY * disY + disZ * disZ;
        }

        public static double DistanceSq(ref Vector3D pointA, ref Vector3D pointB)
        {
            double disX = pointA.X - pointB.X;
            double disY = pointA.Y - pointB.Y;
            double disZ = pointA.Z - pointB.Z;
            return disX * disX + disY * disY + disZ * disZ;
        }

        public double Length()
        {
            return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        }

        public static double Length(Vector3D vector)
        {
            return vector.Length();
        }

        public static double Length(ref Vector3D vector)
        {
            return vector.Length();
        }

        /// <summary>
        /// 海伦(Heron)公式求三角形面积
        /// </summary>
        /// <param name="a">向量边A</param>
        /// <param name="b">向量边B</param>
        /// <param name="c">向量边C</param>
        /// <returns></returns>
        public static double Area(ref Vector3D a, ref Vector3D b, ref Vector3D c)
        {
            double ab = Vector3D.Distance(ref a, ref b);
            double ac = Vector3D.Distance(ref a, ref c);
            double bc = Vector3D.Distance(ref b, ref c);
            double p = (ab + ac + bc) * 0.5;
            return Math.Sqrt(p * (p - ab) * (p - ac) * (p - bc));
        }

        public double LengthSq()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }

        public static double LengthSq(Vector3D vector)
        {
            return vector.LengthSq();
        }

        public static double LengthSq(ref Vector3D vector)
        {
            return vector.LengthSq();
        }

        public void MultiplyBy(double value)
        {
            this.X *= value;
            this.Y *= value;
            this.Z *= value;
        }

        public void MultiplyBy(Vector3D value)
        {
            this.X *= value.X;
            this.Y *= value.Y;
            this.Z *= value.Z;
        }

        public static Vector3D Multiply(Vector3D source, double value)
        {
            return new Vector3D(source.X * value, source.Y * value, source.Z * value);
        }

        public static Vector3D Multiply(ref Vector3D source, double value)
        {
            return new Vector3D(source.X * value, source.Y * value, source.Z * value);
        }

        public bool Normalize()
        {
            double length = this.Length();
            if (length == 0.0)
                return false;
            this.MultiplyBy(1.0 / length);
            return true;
        }

        public static Vector3D Normalize(Vector3D vector)
        {
            double length = vector.Length();
            if (length == 0.0)
                return vector;
            double num2 = 1.0 / length;
            return new Vector3D(vector.X * num2, vector.Y * num2, vector.Z * num2);
        }

        public override string ToString()
        {
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{{X={0},Y={1},Z={2}}}", (object)this.X, (object)this.Y, (object)this.Z);
        }

        public bool Equals(Vector3D other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public bool Equals(ref Vector3D other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public bool FuzzyEquals(Vector3D other, double epsilon)
        {
            return Vector3D.DistanceSq(this, other) < epsilon * epsilon;
        }

        public static double GetAngle(Vector3D a, Vector3D b)
        {
            double aLength = a.Length();
            if (aLength == 0.0)
                throw new ArgumentOutOfRangeException("a");
            double bLength = b.Length();
            if (bLength == 0.0)
                throw new ArgumentOutOfRangeException("b");
            double d = Vector3D.Dot(ref a, ref b) / (aLength * bLength);
            if (d >= 1.0)
                return 0.0;
            if (d <= -1.0)
                return Math.PI;
            else
                return Math.Acos(d);
        }

        public static double GetAngle(Vector3D a, Vector3D b, Vector3D axis)
        {
            Matrix4x4D matrix1 = Matrix4x4D.RotationY(-Math.Atan2(axis.X, axis.Z));
            Vector3D vector3D = Vector3D.TransformCoordinate(ref axis, ref matrix1);
            Matrix4x4D matrix2 = matrix1 * Matrix4x4D.RotationX(Math.Atan2(vector3D.Y, vector3D.Z));
            return Vector2D.GetAngle(Vector2D.TransformCoordinate(ref a, ref matrix2), Vector2D.TransformCoordinate(ref b, ref matrix2));
        }

        public void TransformCoordinate(Matrix4x4D matrix)
        {
            double x = this.X;
            double y = this.Y;
            double z = this.Z;
            double num = 1.0 / (x * matrix.M14 + y * matrix.M24 + z * matrix.M34 + matrix.M44);
            this.X = (x * matrix.M11 + y * matrix.M21 + z * matrix.M31 + matrix.M41) * num;
            this.Y = (x * matrix.M12 + y * matrix.M22 + z * matrix.M32 + matrix.M42) * num;
            this.Z = (x * matrix.M13 + y * matrix.M23 + z * matrix.M33 + matrix.M43) * num;
        }

        public void TransformCoordinate(ref Matrix4x4D matrix)
        {
            double x = this.X;
            double y = this.Y;
            double z = this.Z;
            double num = 1.0 / (x * matrix.M14 + y * matrix.M24 + z * matrix.M34 + matrix.M44);
            this.X = (x * matrix.M11 + y * matrix.M21 + z * matrix.M31 + matrix.M41) * num;
            this.Y = (x * matrix.M12 + y * matrix.M22 + z * matrix.M32 + matrix.M42) * num;
            this.Z = (x * matrix.M13 + y * matrix.M23 + z * matrix.M33 + matrix.M43) * num;
        }

        public static Vector3D TransformCoordinate(Vector3D vector, Matrix4x4D matrix)
        {
            double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
            return new Vector3D((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41) * num, (vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42) * num, (vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43) * num);
        }

        public static Vector3D TransformCoordinate(ref Vector3D vector, ref Matrix4x4D matrix)
        {
            double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
            return new Vector3D((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41) * num, (vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42) * num, (vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43) * num);
        }

        public static Vector3D TransformCoordinate(Vector3D vector, ref Matrix4x4D matrix)
        {
            double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
            return new Vector3D((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41) * num, (vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42) * num, (vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43) * num);
        }

        public void TransformNormal(Matrix4x4D matrix)
        {
            double x = this.X;
            double y = this.Y;
            double z = this.Z;
            double num = 1.0 / (x * matrix.M14 + y * matrix.M24 + z * matrix.M34 + 1.0);
            this.X = (x * matrix.M11 + y * matrix.M21 + z * matrix.M31) * num;
            this.Y = (x * matrix.M12 + y * matrix.M22 + z * matrix.M32) * num;
            this.Z = (x * matrix.M13 + y * matrix.M23 + z * matrix.M33) * num;
        }

        public void TransformNormal(ref Matrix4x4D matrix)
        {
            double x = this.X;
            double y = this.Y;
            double z = this.Z;
            double num = 1.0 / (x * matrix.M14 + y * matrix.M24 + z * matrix.M34 + 1.0);
            this.X = (x * matrix.M11 + y * matrix.M21 + z * matrix.M31) * num;
            this.Y = (x * matrix.M12 + y * matrix.M22 + z * matrix.M32) * num;
            this.Z = (x * matrix.M13 + y * matrix.M23 + z * matrix.M33) * num;
        }

        public static Vector3D TransformNormal(Vector3D vector, Matrix4x4D matrix)
        {
            double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + 1.0);
            return new Vector3D((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31) * num, (vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32) * num, (vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33) * num);
        }

        public static Vector3D TransformNormal(ref Vector3D vector, ref Matrix4x4D matrix)
        {
            double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + 1.0);
            return new Vector3D((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31) * num, (vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32) * num, (vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33) * num);
        }

        public static Vector3D TransformNormal(Vector3D vector, ref Matrix4x4D matrix)
        {
            double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + 1.0);
            return new Vector3D((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31) * num, (vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32) * num, (vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33) * num);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3D)
                return this == (Vector3D)obj;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
        }

        public Vector3D Add(Vector3D vec)
        {
            return new Vector3D(this.X + vec.X, this.Y + vec.Y, this.Z + vec.Z);
        }

        public Vector3D Subtract(Vector3D vec)
        {
            return new Vector3D(this.X - vec.X, this.Y - vec.Y, this.Z - vec.Z);
        }

        public Vector3D Subtract(ref Vector3D vec)
        {
            return new Vector3D(this.X - vec.X, this.Y - vec.Y, this.Z - vec.Z);
        }

        public Vector3D Multiply(double s)
        {
            return new Vector3D(this.X * s, this.Y * s, this.Z * s);
        }

        public double Dot(Vector3D vec)
        {
            return this.X * vec.X + this.Y * vec.Y + this.Z * vec.Z;
        }

        public Vector3D Negate()
        {
            return new Vector3D(-this.X, -this.Y, -this.Z);
        }

        public Vector3D Min(Vector3D vec)
        {
            return new Vector3D(Math.Min(this.X, vec.X), Math.Min(this.Y, vec.Y), Math.Min(this.Z, vec.Z));
        }

        public Vector3D Max(Vector3D vec)
        {
            return new Vector3D(Math.Max(this.X, vec.X), Math.Max(this.Y, vec.Y), Math.Max(this.Z, vec.Z));
        }

        public static Vector3D Min(Vector3D v0, Vector3D v1)
        {
            return v0.Min(v1);
        }

        public static Vector3D Max(Vector3D v0, Vector3D v1)
        {
            return v0.Max(v1);
        }

        public Vector3D Lerp(Vector3D vec, double w)
        {
            return this * (1.0 - w) + vec * w;
        }

        public static Vector3D Lerp(Vector3D v0, Vector3D v1, double w)
        {
            return v0 * (1.0 - w) + v1 * w;
        }

        public double Minimal()
        {
            return Math.Min(Math.Min(this.X, this.Y), this.Z);
        }

        public double Maximal()
        {
            return Math.Max(Math.Max(this.X, this.Y), this.Z);
        }

        public double AbsMinimal()
        {
            return Math.Min(Math.Min(Math.Abs(this.X), Math.Abs(this.Y)), Math.Abs(this.Z));
        }

        public double AbsMaximal()
        {
            return Math.Max(Math.Max(Math.Abs(this.X), Math.Abs(this.Y)), Math.Abs(this.Z));
        }

        [Conditional("DEBUG")]
        public static void ValidatePosition(Vector3D position)
        {
        }

        public static Vector3D MidPointByLength(Vector3D left, Vector3D right)
        {
            Vector3D vector3D = left + right;
            vector3D.Normalize();
            vector3D.Multiply(left.Length());
            return vector3D;
        }

        public void AssertIsUnitVector()
        {
        }

        public void AssertIsOrthogonalTo(Vector3D other)
        {
        }

        public Vector3D GetOrthoNormal()
        {
            this.AssertIsUnitVector();
            Vector3D other = Vector3D.Empty;
            if (Math.Abs(this.X) <= Math.Abs(this.Y) && Math.Abs(this.X) <= Math.Abs(this.Z))
            {
                other.Y = -this.Z;
                other.Z = this.Y;
            }
            else if (Math.Abs(this.Y) <= Math.Abs(this.Z))
            {
                other.X = -this.Z;
                other.Z = this.X;
            }
            else
            {
                other.X = -this.Y;
                other.Y = this.X;
            }
            this.AssertIsOrthogonalTo(other);
            other.Normalize();
            return other;
        }

        public bool Orthonormalize(Vector3D other)
        {
            other.AssertIsUnitVector();
            this -= this * other * other;
            return this.Normalize();
        }
    }
}
