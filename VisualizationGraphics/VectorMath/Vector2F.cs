using System;
using System.Globalization;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
  [Serializable]
  public struct Vector2F : IEquatable<Vector2F>
  {
    public static readonly Vector2F Empty = new Vector2F();
    public static readonly Vector2F XVector = new Vector2F(1f, 0.0f);
    public static readonly Vector2F YVector = new Vector2F(0.0f, 1f);
    public float X;
    public float Y;

    public Vector2F(float x, float y)
    {
      this.X = x;
      this.Y = y;
    }

    public static bool operator ==(Vector2F left, Vector2F right)
    {
      return left.X == right.X && left.Y == right.Y;
    }

    public static bool operator !=(Vector2F left, Vector2F right)
    {
      return !(left == right);
    }

    public static Vector2F operator +(Vector2F left, Vector2F right)
    {
      return new Vector2F(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2F operator -(Vector2F left, Vector2F right)
    {
      return new Vector2F(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2F operator *(Vector2F left, float right)
    {
      return new Vector2F(left.X * right, left.Y * right);
    }

    public static Vector2F operator /(Vector2F left, float right)
    {
      float num = 1f / right;
      return new Vector2F(left.X * num, left.Y * num);
    }

    public void Add(Vector2F right)
    {
      this.X += right.X;
      this.Y += right.Y;
    }

    public static Vector2F Add(Vector2F left, Vector2F right)
    {
      return new Vector2F(left.X + right.X, left.Y + right.Y);
    }

    public void Subtract(Vector2F right)
    {
      this.X -= right.X;
      this.Y -= right.Y;
    }

    public static Vector2F Subtract(Vector2F left, Vector2F right)
    {
      return new Vector2F(left.X - right.X, left.Y - right.Y);
    }

    public void Multiply(float value)
    {
      this.X *= value;
      this.Y *= value;
    }

    public static Vector2F Multiply(Vector2F source, float value)
    {
      return new Vector2F(source.X * value, source.Y * value);
    }

    public void Normalize()
    {
      float num = this.Length();
      if (num == 0.0)
        return;
      this.Multiply(1f / num);
    }

    public void Minimize(Vector2F toMinimize)
    {
      if (toMinimize.X < this.X)
        this.X = toMinimize.X;
      if (toMinimize.Y >= this.Y)
        return;
      this.Y = toMinimize.Y;
    }

    public void Maximize(Vector2F toMaximize)
    {
      if (toMaximize.X > this.X)
        this.X = toMaximize.X;
      if (toMaximize.Y <= this.Y)
        return;
      this.Y = toMaximize.Y;
    }

    public static Vector2F Normalize(Vector2F vector)
    {
      float num1 = vector.Length();
      if (num1 == 0.0)
        return vector;
      float num2 = 1f / num1;
      return new Vector2F(vector.X * num2, vector.Y * num2);
    }

    public static float Distance(Vector2F pointA, Vector2F pointB)
    {
      return (float) Math.Sqrt(Vector2F.DistanceSq(pointA, pointB));
    }

    public static float DistanceSq(Vector2F pointA, Vector2F pointB)
    {
      float disX = pointA.X - pointB.X;
      float disY = pointA.Y - pointB.Y;
      return (float) (disX * disX + disY * disY);
    }

    public float Length()
    {
      return (float) Math.Sqrt(this.X * this.X + this.Y * this.Y);
    }

    public static float Length(Vector2F vector)
    {
      return vector.Length();
    }

    public float LengthSq()
    {
      return (float) (this.X * this.X + this.Y * this.Y);
    }

    public static float LengthSq(Vector2F vector)
    {
      return vector.LengthSq();
    }

    public static float Cross(Vector2F left, Vector2F right)
    {
      return (float) (left.X * right.Y - left.Y * right.X);
    }

    public override string ToString()
    {
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{{X={0},Y={1}}}", new object[2]
      {
        (object) this.X,
        (object) this.Y
      });
    }

    public static Vector2F TransformCoordinate(Vector3F vector, Matrix4x4D matrix)
    {
      double num = 1.0 / (vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44);
      return new Vector2F(
          (float) ((vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41) * num), 
          (float) ((vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42) * num));
    }

    public static float Dot(Vector2F left, Vector2F right)
    {
      return (float) (left.X * right.X + left.Y * right.Y);
    }

    public static float GetAngle(Vector2F a, Vector2F b)
    {
      float aLength = a.Length();
      if (aLength == 0.0)
        throw new ArgumentOutOfRangeException("a");
      float bLength = b.Length();
      if (bLength == 0.0)
        throw new ArgumentOutOfRangeException("b");
      float cosAB = Vector2F.Dot(a, b) / (aLength * bLength);
      if (cosAB >= 1.0)
        return 0.0f;
      if (cosAB <= -1.0)
        return 3.141593f;
      float result = (float) Math.Acos(cosAB);
      if (Vector2F.Cross(a, b) < 0.0)
        result *= -1f;
      return result;
    }

    public bool Equals(Vector2F other)
    {
      return this == other;
    }

    public override bool Equals(object obj)
    {
      if (obj is Vector2F)
        return this.Equals((Vector2F) obj);
      else
        return false;
    }

    public override int GetHashCode()
    {
      return this.X.GetHashCode() ^ this.Y.GetHashCode();
    }
  }
}
