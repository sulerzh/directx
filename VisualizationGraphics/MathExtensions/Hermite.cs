namespace Microsoft.Data.Visualization.Engine.MathExtensions
{
    public class Hermite
    {
        private double start;
        private double end;
        private double interval;
        private double v0;
        private double d0;
        private double v1;
        private double d1;

        public Hermite(double intervalStart, double intervalEnd, DifferentiableScalar startCoefficient, DifferentiableScalar endCoefficient)
        {
            this.SetInterval(intervalStart, intervalEnd);
            this.v0 = startCoefficient.Value;
            this.d0 = startCoefficient.Derivative * this.interval;
            this.v1 = endCoefficient.Value;
            this.d1 = endCoefficient.Derivative * this.interval;
            MathEx.AssertEqual(this.Evaluate(this.start), startCoefficient.Value, 1E-06);
            MathEx.AssertEqual(this.Evaluate(this.end), endCoefficient.Value, 1E-06);
        }

        public Hermite(double intervalStart, double intervalEnd, double startValue, double startDerivative, double endValue, double endDerivative)
        {
            this.SetInterval(intervalStart, intervalEnd);
            this.v0 = startValue;
            this.d0 = startDerivative * this.interval;
            this.v1 = endValue;
            this.d1 = endDerivative * this.interval;
            MathEx.AssertEqual(this.Evaluate(this.start), startValue, 1E-06);
            MathEx.AssertEqual(this.Evaluate(this.end), endValue, 1E-06);
        }

        private void SetInterval(double intervalStart, double intervalEnd)
        {
            this.start = intervalStart;
            this.end = intervalEnd;
            this.interval = this.start == this.end ? 1.0 : this.end - this.start;
        }

        public double Evaluate(double point)
        {
            double num1 = (point - this.start) / this.interval;
            double num2 = num1 * num1;
            double num3 = num2 * num1;
            return (2.0 * num3 - 3.0 * num2 + 1.0) * this.v0 + (num3 - 2.0 * num2 + num1) * this.d0 + (3.0 * num2 - 2.0 * num3) * this.v1 + (num3 - num2) * this.d1;
        }
    }
}
