using System;
using System.Globalization;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    [Serializable]
    public struct Vector2D : IEquatable<Vector2D>
    {
        public static readonly Vector2D Empty = new Vector2D();
        public static readonly Vector2D XVector = new Vector2D(1.0, 0.0);
        public static readonly Vector2D YVector = new Vector2D(0.0, 1.0);
        public double X;
        public double Y;

        public Vector2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static bool operator ==(Vector2D left, Vector2D right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2D left, Vector2D right)
        {
            return left.X != right.X && left.Y != right.Y;
        }

        public static Vector2D operator +(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2D operator -(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2D operator *(Vector2D left, double right)
        {
            return new Vector2D(left.X * right, left.Y * right);
        }

        public static Vector2D operator *(double left, Vector2D right)
        {
            return new Vector2D(left * right.X, left * right.Y);
        }

        public static double operator *(Vector2D vec0, Vector2D vec1)
        {
            return vec0.Dot(vec1);
        }

        public static Vector2D operator /(Vector2D left, double right)
        {
            return new Vector2D(left.X / right, left.Y / right);
        }

        public static Vector2D operator -(Vector2D vec)
        {
            return new Vector2D(-vec.X, -vec.Y);
        }

        public void AddTo(Vector2D right)
        {
            this.X += right.X;
            this.Y += right.Y;
        }

        public static Vector2D Add(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X + right.X, left.Y + right.Y);
        }

        public void SubtractFrom(Vector2D right)
        {
            this.X -= right.X;
            this.Y -= right.Y;
        }

        public static Vector2D Subtract(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        public void MultiplyBy(Vector2D value)
        {
            this.X *= value.X;
            this.Y *= value.Y;
        }

        public void MultiplyBy(double value)
        {
            this.X *= value;
            this.Y *= value;
        }

        public static Vector2D Multiply(Vector2D source, double value)
        {
            return new Vector2D(source.X * value, source.Y * value);
        }

        public void Normalize()
        {
            this.DivideBy(this.Length());
        }

        public void TurnRight()
        {
            double num = -this.X;
            this.X = this.Y;
            this.Y = num;
        }

        public void Minimize(Vector2D toMinimize)
        {
            if (toMinimize.X < this.X)
                this.X = toMinimize.X;
            if (toMinimize.Y >= this.Y)
                return;
            this.Y = toMinimize.Y;
        }

        public void Maximize(Vector2D toMaximize)
        {
            if (toMaximize.X > this.X)
                this.X = toMaximize.X;
            if (toMaximize.Y <= this.Y)
                return;
            this.Y = toMaximize.Y;
        }

        public static Vector2D Normalize(Vector2D vector)
        {
            double length = vector.Length();
            if (length == 0.0)
                return vector;
            double num = 1.0 / length;
            return new Vector2D(vector.X * num, vector.Y * num);
        }

        public static double Distance(Vector2D pointA, Vector2D pointB)
        {
            return Math.Sqrt(Vector2D.DistanceSq(pointA, pointB));
        }

        public static double DistanceSq(Vector2D pointA, Vector2D pointB)
        {
            double disX = pointA.X - pointB.X;
            double disY = pointA.Y - pointB.Y;
            return disX * disX + disY * disY;
        }

        public double Length()
        {
            return Math.Sqrt(this.X * this.X + this.Y * this.Y);
        }

        public static double Length(Vector2D vector)
        {
            return vector.Length();
        }

        public double LengthSq()
        {
            return this.X * this.X + this.Y * this.Y;
        }

        public static double LengthSq(Vector2D vector)
        {
            return vector.LengthSq();
        }

        public static double Cross(Vector2D left, Vector2D right)
        {
            return left.X * right.Y - left.Y * right.X;
        }

        public static double Cross(ref Vector2D left, ref Vector2D right)
        {
            return left.X * right.Y - left.Y * right.X;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{X={0},Y={1}}}",
                new object[2]
                {
                    (object) this.X,
                    (object) this.Y
                });
        }

        public void DivideBy(double denominator)
        {
            if (denominator == 0.0)
                return;
            this.X /= denominator;
            this.Y /= denominator;
        }

        public static Vector2D TransformCoordinate(Vector3D vector, Matrix4x4D matrix)
        {
            double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
            return new Vector2D((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41) * num, (vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42) * num);
        }

        public static Vector2D TransformCoordinate(ref Vector3D vector, ref Matrix4x4D matrix)
        {
            double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
            return new Vector2D((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41) * num, (vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42) * num);
        }

        public static double Dot(Vector2D left, Vector2D right)
        {
            return left.X * right.X + left.Y * right.Y;
        }

        public static double Dot(ref Vector2D left, ref Vector2D right)
        {
            return left.X * right.X + left.Y * right.Y;
        }

        public static double GetAngle(Vector2D a, Vector2D b)
        {
            double aLength = a.Length();
            if (aLength == 0.0)
                throw new ArgumentOutOfRangeException("a");
            double bLength = b.Length();
            if (bLength == 0.0)
                throw new ArgumentOutOfRangeException("b");
            double d = Vector2D.Dot(ref a, ref b) / (aLength * bLength);
            if (d >= 1.0)
                return 0.0;
            if (d <= -1.0)
                return Math.PI;
            double result = Math.Acos(d);
            if (Vector2D.Cross(ref a, ref b) < 0.0)
                result *= -1.0;
            return result;
        }

        public static Vector2D GetNormalVector(Vector2D v)
        {
            return new Vector2D(v.Y, -v.X);
        }

        public bool Equals(Vector2D other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public bool Equals(ref Vector2D other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2D)
                return this.Equals((Vector2D)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }

        public Vector2D Add(Vector2D vec)
        {
            return new Vector2D(this.X + vec.X, this.Y + vec.Y);
        }

        public Vector2D Subtract(Vector2D vec)
        {
            return new Vector2D(this.X - vec.X, this.Y - vec.Y);
        }

        public Vector2D Multiply(double s)
        {
            return new Vector2D(this.X * s, this.Y * s);
        }

        public double Dot(Vector2D vec)
        {
            return this.X * vec.X + this.Y * vec.Y;
        }

        public Vector2D Negate()
        {
            return new Vector2D(-this.X, -this.Y);
        }

        public Vector2D Min(Vector2D vec)
        {
            return new Vector2D(Math.Min(this.X, vec.X), Math.Min(this.Y, vec.Y));
        }

        public Vector2D Max(Vector2D vec)
        {
            return new Vector2D(Math.Max(this.X, vec.X), Math.Max(this.Y, vec.Y));
        }

        public Vector2D Lerp(Vector2D vec, double w)
        {
            return new Vector2D(this.X + (vec.X - this.X) * w, this.Y + (vec.Y - this.Y) * w);
        }

        public double Minimal()
        {
            return Math.Min(this.X, this.Y);
        }

        public double Maximal()
        {
            return Math.Max(this.X, this.Y);
        }

        public double AbsMinimal()
        {
            return Math.Min(Math.Abs(this.X), Math.Abs(this.Y));
        }

        public double AbsMaximal()
        {
            return Math.Max(Math.Abs(this.X), Math.Abs(this.Y));
        }
    }
}
