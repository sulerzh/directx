using System;
using System.Globalization;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
  [Serializable]
  public struct Vector3F : IEquatable<Vector3F>
  {
    public static readonly Vector3F Empty = new Vector3F();
    public float X;
    public float Y;
    public float Z;

    public float Pitch
    {
      get
      {
        return (float) Math.Atan2(this.X, Math.Sqrt( this.Y * this.Y + this.Z * this.Z));
      }
    }

    public float Heading
    {
      get
      {
        return (float) Math.Atan2(this.Y, this.Z);
      }
    }

    public Vector3F(float x, float y, float z)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }

    public Vector3F(Vector3D other)
    {
      this.X = (float) other.X;
      this.Y = (float) other.Y;
      this.Z = (float) other.Z;
    }

    public static Vector3F operator +(Vector3F left, Vector3F right)
    {
      return new Vector3F(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vector3F operator -(Vector3F left, Vector3F right)
    {
      return new Vector3F(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vector3F operator *(float left, Vector3F right)
    {
      return new Vector3F(left * right.X, left * right.Y, left * right.Z);
    }

    public static Vector3F operator *(Vector3F left, float right)
    {
      return new Vector3F(left.X * right, left.Y * right, left.Z * right);
    }

    public static Vector3F operator /(Vector3F left, float right)
    {
      return new Vector3F(left.X / right, left.Y / right, left.Z / right);
    }

    public static bool operator ==(Vector3F left, Vector3F right)
    {
      return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
    }

    public static bool operator !=(Vector3F left, Vector3F right)
    {
      return !(left == right);
    }

    public void Add(Vector3F right)
    {
      this.X += right.X;
      this.Y += right.Y;
      this.Z += right.Z;
    }

    public void Add(ref Vector3F right)
    {
      this.X += right.X;
      this.Y += right.Y;
      this.Z += right.Z;
    }

    public static Vector3F Add(Vector3F left, Vector3F right)
    {
      return new Vector3F(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vector3F Add(Vector3F left, ref Vector3F right)
    {
      return new Vector3F(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vector3F Add(ref Vector3F left, ref Vector3F right)
    {
      return new Vector3F(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public void Subtract(Vector3F right)
    {
      this.X -= right.X;
      this.Y -= right.Y;
      this.Z -= right.Z;
    }

    public void Subtract(ref Vector3F right)
    {
      this.X -= right.X;
      this.Y -= right.Y;
      this.Z -= right.Z;
    }

    public Vector3D ToVector3D()
    {
      return new Vector3D(this.X, this.Y, this.Z);
    }

    public static Vector3F Subtract(Vector3F left, Vector3F right)
    {
      return new Vector3F(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vector3F Subtract(ref Vector3F left, ref Vector3F right)
    {
      return new Vector3F(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vector3F Cross(Vector3F left, Vector3F right)
    {
      return new Vector3F(
          left.Y * right.Z - left.Z * right.Y, 
          left.Z * right.X - left.X * right.Z, 
          left.X * right.Y - left.Y * right.X);
    }

    public static float Dot(Vector3F left, Vector3F right)
    {
      return (float) (left.X * right.X + left.Y * right.Y + left.Z * right.Z);
    }

    public static float Distance(Vector3F pointA, Vector3F pointB)
    {
      return (float) Math.Sqrt(Vector3F.DistanceSq(pointA, pointB));
    }

    public static float DistanceSq(Vector3F pointA, Vector3F pointB)
    {
      float disX = pointA.X - pointB.X;
      float disY = pointA.Y - pointB.Y;
      float disZ = pointA.Z - pointB.Z;
      return (float) (disX * disX + disY * disY + disZ * disZ);
    }

    public float Length()
    {
      return (float) Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
    }

    public static float Length(Vector3F vector)
    {
      return vector.Length();
    }

    public float LengthSq()
    {
      return (float) (this.X * this.X + this.Y * this.Y + this.Z * this.Z);
    }

    public static float LengthSq(Vector3F vector)
    {
      return vector.LengthSq();
    }

    public void Multiply(float value)
    {
      this.X *= value;
      this.Y *= value;
      this.Z *= value;
    }

    public static Vector3F Multiply(Vector3F source, float value)
    {
      return new Vector3F(source.X * value, source.Y * value, source.Z * value);
    }

    public static Vector3F Multiply(ref Vector3F source, float value)
    {
      return new Vector3F(source.X * value, source.Y * value, source.Z * value);
    }

    public void Normalize()
    {
      float lenth = this.Length();
      if (lenth == 0.0)
        return;
      this.Multiply(1f / lenth);
    }

    public static Vector3F Normalize(Vector3F vector)
    {
      float length = vector.Length();
      if (length == 0.0)
        return vector;
      float num2 = 1f / length;
      return new Vector3F(vector.X * num2, vector.Y * num2, vector.Z * num2);
    }

    public override string ToString()
    {
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{{X={0},Y={1},Z={2}}}", (object) this.X, (object) this.Y, (object) this.Z);
    }

    public bool Equals(Vector3F other)
    {
      return this == other;
    }

    public bool FuzzyEquals(Vector3F other)
    {
        return
            this.X <= other.X + 1E-14 &&
            this.X >= other.X - 1E-14 &&
            this.Y <= other.Y + 1E-14 &&
            this.Y >= other.Y - 1E-14 &&
            this.Z <= other.Z + 1E-14 &&
            this.Z >= other.Z - 1E-14;
    }

    public bool FuzzyEquals(Vector3F other, float epsilon)
    {
      return
          Math.Abs(this.X - other.X) < epsilon && 
          Math.Abs(this.Y - other.Y) < epsilon && 
          Math.Abs(this.Z - other.Z) < epsilon;
    }

    public static float GetAngle(Vector3F a, Vector3F b)
    {
      float aLenth = a.Length();
      if (aLenth == 0.0)
        throw new ArgumentOutOfRangeException("a");
      float bLength = b.Length();
      if (bLength == 0.0)
        throw new ArgumentOutOfRangeException("b");
      float cosAB = Vector3F.Dot(a, b) / (aLenth * bLength);
      if (cosAB >= 1.0)
        return 0.0f;
      if (cosAB <= -1.0)
        return 3.141593f;
      else
        return (float) Math.Acos(cosAB);
    }

    public static float GetAngle(Vector3F a, Vector3F b, Vector3F axis)
    {
      Matrix4x4D matrix1 = Matrix4x4D.RotationY(-Math.Atan2(axis.X, axis.Z));
      Vector3F vector3F = Vector3F.TransformCoordinate(axis, matrix1);
      Matrix4x4D matrix2 = matrix1 * Matrix4x4D.RotationX(Math.Atan2(vector3F.Y, vector3F.Z));
      return Vector2F.GetAngle(Vector2F.TransformCoordinate(a, matrix2), Vector2F.TransformCoordinate(b, matrix2));
    }

    public void TransformCoordinate(Matrix4x4D matrix)
    {
      float x = this.X;
      float y = this.Y;
      float z = this.Z;
      double num = 1.0 / ( x * matrix.M14 + y * matrix.M24 + z * matrix.M34 + matrix.M44);
      this.X = (float) ((x * matrix.M11 + y * matrix.M21 + z * matrix.M31 + matrix.M41) * num);
      this.Y = (float) ((x * matrix.M12 + y * matrix.M22 + z * matrix.M32 + matrix.M42) * num);
      this.Z = (float) ((x * matrix.M13 + y * matrix.M23 + z * matrix.M33 + matrix.M43) * num);
    }

    public static Vector3F TransformCoordinate(Vector3F vector, Matrix4x4D matrix)
    {
      double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
      return new Vector3F(
          (float)((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41) * num),
          (float)((vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42) * num),
          (float)((vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43) * num));
    }

    public void TransformNormal(Matrix4x4D matrix)
    {
      float x = this.X;
      float y = this.Y;
      float z = this.Z;
      double num = 1.0 / (x * matrix.M14 + y * matrix.M24 + z * matrix.M34 + 1.0);
      this.X = (float) ((x * matrix.M11 + y * matrix.M21 + z * matrix.M31) * num);
      this.Y = (float) ((x * matrix.M12 + y * matrix.M22 + z * matrix.M32) * num);
      this.Z = (float) ((x * matrix.M13 + y * matrix.M23 + z * matrix.M33) * num);
    }

    public static Vector3F TransformNormal(Vector3F vector, Matrix4x4D matrix)
    {
      double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + 1.0);
      return new Vector3F(
          (float) ((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31) * num), 
          (float) ((vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32) * num), 
          (float) ((vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33) * num));
    }

    public override bool Equals(object obj)
    {
      if (obj is Vector3F)
        return this == (Vector3F) obj;
      else
        return false;
    }

    public override int GetHashCode()
    {
      return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
    }

    public void AssertIsUnitVector()
    {
    }
  }
}
