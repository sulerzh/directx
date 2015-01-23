using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine.MathExtensions
{
    public struct DifferentiableVector
    {
        public Vector3D Value;
        public Vector3D Derivative;

        public DifferentiableScalar X
        {
            get
            {
                return new DifferentiableScalar(this.Value.X, this.Derivative.X);
            }
        }

        public DifferentiableScalar Y
        {
            get
            {
                return new DifferentiableScalar(this.Value.Y, this.Derivative.Y);
            }
        }

        public DifferentiableScalar Z
        {
            get
            {
                return new DifferentiableScalar(this.Value.Z, this.Derivative.Z);
            }
        }

        public DifferentiableVector(Vector3D value, Vector3D derivative)
        {
            this.Value = value;
            this.Derivative = derivative;
        }

        public DifferentiableVector(Vector3D value)
        {
            this.Value = value;
            this.Derivative = Vector3D.Empty;
        }

        public DifferentiableVector(DifferentiableScalar x, DifferentiableScalar y, DifferentiableScalar z)
        {
            this.Value = new Vector3D(x.Value, y.Value, z.Value);
            this.Derivative = new Vector3D(x.Derivative, y.Derivative, z.Derivative);
        }

        public static DifferentiableVector operator +(DifferentiableVector left, DifferentiableVector right)
        {
            return new DifferentiableVector(left.Value + right.Value, left.Derivative + right.Derivative);
        }

        public static DifferentiableVector operator -(DifferentiableVector left, DifferentiableVector right)
        {
            return new DifferentiableVector(left.Value - right.Value, left.Derivative - right.Derivative);
        }

        public static DifferentiableScalar operator *(DifferentiableVector left, DifferentiableVector right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static DifferentiableVector operator *(DifferentiableVector differentiable, double constant)
        {
            return new DifferentiableVector(differentiable.Value * constant, differentiable.Derivative * constant);
        }

        public static DifferentiableVector operator *(double constant, DifferentiableVector differentiable)
        {
            return differentiable * constant;
        }

        public static DifferentiableVector operator *(DifferentiableVector left, DifferentiableScalar right)
        {
            return new DifferentiableVector(left.Value * right.Value, left.Value * right.Derivative + left.Derivative * right.Value);
        }

        public static DifferentiableVector operator /(DifferentiableVector numerator, DifferentiableScalar denominator)
        {
            return new DifferentiableVector(numerator.Value / denominator.Value, (numerator.Derivative * denominator.Value - numerator.Value * denominator.Derivative) / MathEx.Square(denominator.Value));
        }

        public static DifferentiableVector operator ^(DifferentiableVector left, DifferentiableVector right)
        {
            return new DifferentiableVector(left.Y * right.Z - left.Z * right.Y, left.Z * right.X - left.X * right.Z, left.X * right.Y - left.Y * right.X);
        }

        public bool GetNorm(out DifferentiableScalar norm)
        {
            double num1 = this.Value.Length();
            double num2 = this.Value * this.Derivative;
            bool flag = num1 <= Math.Abs(num2) * 1E-06;
            double derivative;
            if (flag)
                num1 = derivative = 0.0;
            else
                derivative = num2 / num1;
            norm = new DifferentiableScalar(num1, derivative);
            return !flag;
        }

        public bool Normalize()
        {
            DifferentiableScalar norm;
            if (!this.GetNorm(out norm))
                return false;
            this = this / norm;
            return true;
        }
    }
}
