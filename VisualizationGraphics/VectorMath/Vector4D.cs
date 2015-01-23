using System;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
  [Serializable]
  public struct Vector4D : IEquatable<Vector4D>
  {
    private static Random random = new Random();
    private const double eps = 1E-07;
    public double X;
    public double Y;
    public double Z;
    public double W;

    public Vector4D(double x, double y, double z, double w)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
      this.W = w;
    }

    public Vector4D(Vector4D vec)
    {
      this.X = vec.X;
      this.Y = vec.Y;
      this.Z = vec.Z;
      this.W = vec.W;
    }

    public Vector4D(Vector3D vector3, double w)
    {
      this.X = vector3.X;
      this.Y = vector3.Y;
      this.Z = vector3.Z;
      this.W = w;
    }

    public static explicit operator Vector4D(Vector3D p)
    {
      return new Vector4D(p.X, p.Y, p.Z, 1.0);
    }

    public static explicit operator Vector4F(Vector4D vector)
    {
      return new Vector4F((float) vector.X, (float) vector.Y, (float) vector.Z, (float) vector.W);
    }

    public static bool operator ==(Vector4D vec0, Vector4D vec1)
    {
        return
            (vec0 == null && vec1 == null) ||
            (vec0 != null && vec1 != null &&
             Vector4D.EqualsWithinEpsilon(vec0.X, vec1.X) &&
             Vector4D.EqualsWithinEpsilon(vec0.Y, vec1.Y) &&
             Vector4D.EqualsWithinEpsilon(vec0.Z, vec1.Z) &&
             Vector4D.EqualsWithinEpsilon(vec0.W, vec1.W));
    }

    public static bool operator !=(Vector4D vec0, Vector4D vec1)
    {
      return !(vec0 == vec1);
    }

    public static Vector4D operator +(Vector4D vec0, Vector4D vec1)
    {
      return vec0.Add(vec1);
    }

    public static Vector4D operator -(Vector4D vec0, Vector4D vec1)
    {
      return vec0.Subtract(vec1);
    }

    public static Vector4D operator -(Vector4D vec)
    {
      return vec.Negate();
    }

    public static Vector4D operator *(Vector4D vec, double s)
    {
      return vec.Multiply(s);
    }

    public static Vector4D operator *(double s, Vector4D vec)
    {
      return vec.Multiply(s);
    }

    public static double operator *(Vector4D vec0, Vector4D vec1)
    {
      return vec0.Dot(vec1);
    }

    public static Vector4D operator ^(Vector4D vec0, Vector4D vec1)
    {
      return new Vector4D(vec0.Y * vec1.Z - vec0.Z * vec1.Y + vec0.X * vec1.W + vec0.W * vec1.X, vec0.Z * vec1.X - vec0.X * vec1.Z + vec0.Y * vec1.W + vec0.W * vec1.Y, vec0.X * vec1.Y - vec0.Y * vec1.X + vec0.Z * vec1.W + vec0.W * vec1.Z, vec0.W + vec1.W - vec0.X * vec1.X - vec0.Y * vec1.Y - vec0.Z * vec1.Z);
    }

    private static bool EqualsWithinEpsilon(double d0, double d1)
    {
      return Math.Abs(d0 - d1) < eps;
    }

    public override int GetHashCode()
    {
      return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.W.GetHashCode();
    }

    public override string ToString()
    {
      return this.X.ToString() + " " + this.Y.ToString() + " " + this.Z.ToString() + " " + this.W.ToString();
    }

    public void AddTo(Vector4D right)
    {
      this.X += right.X;
      this.Y += right.Y;
      this.Z += right.Z;
      this.W += right.W;
    }

    public Vector4D Add(Vector4D vec)
    {
      return new Vector4D(this.X + vec.X, this.Y + vec.Y, this.Z + vec.Z, this.W + vec.W);
    }

    public Vector4D Subtract(Vector4D vec)
    {
      return new Vector4D(this.X - vec.X, this.Y - vec.Y, this.Z - vec.Z, this.W - vec.W);
    }

    public Vector4D Negate()
    {
      return new Vector4D(-this.X, -this.Y, -this.Z, -this.W);
    }

    public double Dot(Vector4D vec)
    {
      return this.X * vec.X + this.Y * vec.Y + this.Z * vec.Z + this.W * vec.W;
    }

    public Vector4D Multiply(double s)
    {
      return new Vector4D(this.X * s, this.Y * s, this.Z * s, this.W * s);
    }

    public void MultiplyBy(double s)
    {
      this.X *= s;
      this.Y *= s;
      this.Z *= s;
      this.W *= s;
    }

    public void Normalize()
    {
      double num = Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W);
      if (num == 0.0)
        return;
      this.X /= num;
      this.Y /= num;
      this.Z /= num;
      this.W /= num;
    }

    public Vector4D Lerp(Vector4D vec, double w)
    {
      return new Vector4D(this.X + (vec.X - this.X) * w, this.Y + (vec.Y - this.Y) * w, this.Z + (vec.Z - this.Z) * w, this.W + (vec.W - this.W) * w);
    }

    public Vector4D Min(Vector4D vec)
    {
      return new Vector4D(Math.Min(this.X, vec.X), Math.Min(this.Y, vec.Y), Math.Min(this.Z, vec.Z), Math.Min(this.W, vec.W));
    }

    public Vector4D Max(Vector4D vec)
    {
      return new Vector4D(Math.Max(this.X, vec.X), Math.Max(this.Y, vec.Y), Math.Max(this.Z, vec.Z), Math.Max(this.W, vec.W));
    }

    public static Vector4D TransformCoordinate(Vector4D vector, Matrix4x4D matrix)
    {
      return new Vector4D(vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41, vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42, vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43, vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
    }

    public static Vector4D TransformCoordinate(ref Vector4D vector, ref Matrix4x4D matrix)
    {
      return new Vector4D(vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41, vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42, vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43, vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
    }

    public static Vector4D TransformCoordinate(Vector4D vector, ref Matrix4x4D matrix)
    {
      return new Vector4D(vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41, vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42, vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43, vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
    }

    public double Minimal()
    {
      return Math.Min(Math.Min(Math.Min(this.X, this.Y), this.Z), this.W);
    }

    public double Maximal()
    {
      return Math.Max(Math.Max(Math.Max(this.X, this.Y), this.Z), this.W);
    }

    public double AbsMinimal()
    {
      return Math.Min(Math.Min(Math.Min(Math.Abs(this.X), Math.Abs(this.Y)), Math.Abs(this.Z)), Math.Abs(this.W));
    }

    public double AbsMaximal()
    {
      return Math.Max(Math.Max(Math.Max(Math.Abs(this.X), Math.Abs(this.Y)), Math.Abs(this.Z)), Math.Abs(this.W));
    }

    public override bool Equals(object o)
    {
      if (this.GetType() == o.GetType())
        return this == (Vector4D) o;
      else
        return false;
    }

    public bool Equals(Vector4D other)
    {
      return this == other;
    }

    public static Vector4D Lerp(Vector4D v0, Vector4D v1, double w)
    {
      return v0 + w * (v1 - v0);
    }

    public static Vector4D Lerp(ref Vector4D v0, ref Vector4D v1, double w)
    {
      return v0 + w * (v1 - v0);
    }

    public static Vector4D Min(Vector4D v0, Vector4D v1)
    {
      return v0.Min(v1);
    }

    public static Vector4D Max(Vector4D v0, Vector4D v1)
    {
      return v0.Max(v1);
    }

    public double Length()
    {
      return Math.Sqrt(this * this);
    }

    private static double UnitRandom()
    {
      return (double) Vector4D.random.Next(int.MaxValue) / (double) int.MaxValue;
    }

    public static Vector4D Random()
    {
      return new Vector4D(Vector4D.UnitRandom(), Vector4D.UnitRandom(), Vector4D.UnitRandom(), Vector4D.UnitRandom());
    }
  }
}
