using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine.MathExtensions
{
    public struct DifferentiableScalar
    {
        public double Value;
        public double Derivative;

        public DifferentiableScalar Sin
        {
            get
            {
                return new DifferentiableScalar(Math.Sin(this.Value), Math.Cos(this.Value) * this.Derivative);
            }
        }

        public DifferentiableScalar Cos
        {
            get
            {
                return new DifferentiableScalar(Math.Cos(this.Value), -Math.Sin(this.Value) * this.Derivative);
            }
        }

        public DifferentiableScalar Asin
        {
            get
            {
                return new DifferentiableScalar(Math.Asin(this.Value), this.Derivative / Math.Sqrt(1.0 - MathEx.Square(this.Value)));
            }
        }

        public DifferentiableScalar Atan
        {
            get
            {
                return new DifferentiableScalar(Math.Atan(this.Value), this.Derivative / (1.0 + MathEx.Square(this.Value)));
            }
        }

        public DifferentiableScalar Exp
        {
            get
            {
                return new DifferentiableScalar(Math.Exp(this.Value), Math.Exp(this.Value) * this.Derivative);
            }
        }

        public DifferentiableScalar(double value, double derivative = 0.0)
        {
            this.Value = value;
            this.Derivative = derivative;
        }

        public static DifferentiableScalar operator +(DifferentiableScalar left, DifferentiableScalar right)
        {
            return new DifferentiableScalar(left.Value + right.Value, left.Derivative + right.Derivative);
        }

        public static DifferentiableScalar operator +(DifferentiableScalar left, double constant)
        {
            return new DifferentiableScalar(left.Value + constant, left.Derivative);
        }

        public static DifferentiableScalar operator -(DifferentiableScalar left, DifferentiableScalar right)
        {
            return new DifferentiableScalar(left.Value - right.Value, left.Derivative - right.Derivative);
        }

        public static DifferentiableScalar operator -(DifferentiableScalar left, double constant)
        {
            return new DifferentiableScalar(left.Value - constant, left.Derivative);
        }

        public static DifferentiableScalar operator -(DifferentiableScalar operand)
        {
            return new DifferentiableScalar(-operand.Value, -operand.Derivative);
        }

        public static DifferentiableScalar operator *(DifferentiableScalar differentiable, double constant)
        {
            return new DifferentiableScalar(differentiable.Value * constant, differentiable.Derivative * constant);
        }

        public static DifferentiableScalar operator *(double constant, DifferentiableScalar differentiable)
        {
            return differentiable * constant;
        }

        public static DifferentiableScalar operator *(DifferentiableScalar left, DifferentiableScalar right)
        {
            return new DifferentiableScalar(left.Value * right.Value, left.Value * right.Derivative + left.Derivative * right.Value);
        }

        public static DifferentiableVector operator *(DifferentiableScalar left, Vector3D right)
        {
            return new DifferentiableVector(left.Value * right, left.Derivative * right);
        }

        public static DifferentiableScalar operator /(DifferentiableScalar numerator, DifferentiableScalar denominator)
        {
            return new DifferentiableScalar(numerator.Value / denominator.Value, (numerator.Derivative * denominator.Value - numerator.Value * denominator.Derivative) / MathEx.Square(denominator.Value));
        }

        public static DifferentiableScalar operator /(DifferentiableScalar numerator, double denominator)
        {
            return new DifferentiableScalar(numerator.Value / denominator, numerator.Derivative / denominator);
        }

        public static DifferentiableScalar Atan2(DifferentiableScalar y, DifferentiableScalar x)
        {
            return new DifferentiableScalar(Math.Atan2(y.Value, x.Value), (y.Derivative * x.Value - x.Derivative * y.Value) / (MathEx.Square(x.Value) + MathEx.Square(y.Value)));
        }

        public static bool Hypot(DifferentiableScalar x, DifferentiableScalar y, out DifferentiableScalar hypot)
        {
            double num1 = MathEx.Hypot(x.Value, y.Value);
            double num2 = x.Value * x.Derivative + y.Value * y.Derivative;
            bool flag = num1 <= Math.Abs(num2) * 1E-06;
            double derivative;
            if (flag)
                num1 = derivative = 0.0;
            else
                derivative = num2 / num1;
            hypot = new DifferentiableScalar(num1, derivative);
            return !flag;
        }

        public static DifferentiableScalar Max(DifferentiableScalar a, double b)
        {
            if (a.Value < b)
            {
                a.Value = b;
                a.Derivative = 0.0;
            }
            return a;
        }

        public static DifferentiableScalar Min(DifferentiableScalar a, double b)
        {
            if (a.Value > b)
            {
                a.Value = b;
                a.Derivative = 0.0;
            }
            return a;
        }
    }
}
