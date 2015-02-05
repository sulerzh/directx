using System;

namespace Microsoft.Data.Visualization.Engine.MathExtensions
{
    public class MathEx
    {
        private const double epsilon = 1E-12;

        public static double Square(double t)
        {
            return t * t;
        }

        /// <summary>
        /// 计算三角形斜边长
        /// </summary>
        public static double Hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static double[] SolveTrigEquation(double a, double b, double c)
        {
            double num1 = MathEx.Hypot(a, b);
            if (num1 == 0.0)
                return new double[0];
            if (Math.Abs(c) > num1 * 1.000001)
                return new double[0];
            double num2 = Math.Atan2(b, a);
            double d = c / num1;
            if (d >= 1.0)
            {
                double[] roots = new double[] { num2 };
                MathEx.ValidateTrigEquationSolution(a, b, c, roots);
                return roots;
            }
            else if (d <= -1.0)
            {
                double[] roots = new double[] { num2 + Math.PI };
                MathEx.ValidateTrigEquationSolution(a, b, c, roots);
                return roots;
            }
            else
            {
                double num3 = Math.Acos(d);
                double[] roots = new double[] { num2 + num3, num2 - num3 };
                MathEx.ValidateTrigEquationSolution(a, b, c, roots);
                return roots;
            }
        }

        private static void ValidateTrigEquationSolution(double a, double b, double c, double[] roots)
        {
        }

        public static double Remainder(double numerator, double denominator)
        {
            return numerator - Math.Floor(numerator / denominator) * denominator;
        }

        public static double GetNormalized(double value, double halfPeriod)
        {
            if (-halfPeriod < value && value <= halfPeriod)
                return value;
            else
                return MathEx.Remainder(value + halfPeriod, halfPeriod * 2.0) - halfPeriod;
        }

        public static double GetClosestRepresentation(double value, double target, double halfPeriod)
        {
            double num = value - target;
            if (Math.Abs(num) <= halfPeriod)
                return value;
            else
                return target + MathEx.GetNormalized(num, halfPeriod);
        }

        public static double InterpolateQuadratic(double startValue, double midValue, double endValue, double t)
        {
            double num1 = midValue * 2.0 - (startValue + endValue) / 2.0;
            double num2 = 1.0 - t;
            return startValue * num2 * num2 + 2.0 * num1 * num2 * t + endValue * t * t;
        }

        public static void AssertEqual(double actual, double expected, double tolerance)
        {
        }

        public static void Clamp(ref double value, double lowerBound, double upperBound)
        {
            if (value < lowerBound)
                value = lowerBound;
            if (value <= upperBound)
                return;
            value = upperBound;
        }

        public static double[] SolveQuadratic(double a, double b, double c)
        {
            double d = b * b - a * c;
            if (d < 0.0)
                return new double[0];
            double num1 = Math.Sqrt(d);
            double num2 = -b - num1 * (double)Math.Sign(b);
            double[] numArray;
            if (Math.Abs(a) <= Math.Abs(num2) * (0.0 / 1.0))
            {
                if (Math.Abs(b) <= Math.Abs(c) * (0.0 / 1.0))
                    return new double[0];
                numArray = new double[]{-c / (2.0 * b)};
            }
            else
                numArray = new double[]{num2 / a,c / num2};
            return numArray;
        }

        public static bool AreOfEqualSigns(double a, double b, double zero)
        {
            if (Math.Abs(a) >= zero || Math.Abs(b) >= zero)
                return Math.Sign(a) == Math.Sign(b);
            return true;
        }
    }
}
