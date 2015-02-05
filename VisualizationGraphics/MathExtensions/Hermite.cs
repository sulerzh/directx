namespace Microsoft.Data.Visualization.Engine.MathExtensions
{
    /// <summary>
    /// 构造两点三次Hermite多项式进行插值运算
    /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intervalStart">区间起始值</param>
        /// <param name="intervalEnd">区间结束值</param>
        /// <param name="startValue">起始值</param>
        /// <param name="startDerivative">起始导数值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="endDerivative">结束导数值</param>
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

        /// <summary>
        /// 计算某点的插值结果
        /// 已知x0/x1/y0/y1/y0'/y1',
        /// 构造次数小于等于3的多项式H3(x),满足
        /// y=H3(x),y'=H'(x),x位于（x0，x1）之间**。
        /// 设H3(x)=ax^3+bx^2+cx+d,
        /// 引入四个基函数a0(x)/a1(x)/b0(x)/b1(x)，均为3次多项式，
        /// 满足a0(x0)=1、a1(x1)=1、b0'(x0)=1、b1'(x1)=1,其他已知值结果为0，
        /// 令H3(x)=a0(x)*v0+a1(x)*v1+b0(x)*d0+b1(x)*d1，
        /// 则H3(x)是次数小于等于3的多项式且满足插值条件**。
        /// 定理：满足插值条件**的三次多项式存在且唯一。
        /// 令s=（x-x0）/（x1-x0），
        /// 则a0(x)=2s^3-3s^2+1,
        /// a1(x)=3s^2-2s^3
        /// b0(x)=s^3-2s^2+s
        /// b1(x)=s^3-s^2
        /// </summary>
        /// <param name="point">待插值的点</param>
        /// <returns>插值结果</returns>
        public double Evaluate(double point)
        {
            double scalar = (point - this.start) / this.interval;
            double squareOfScalar = scalar * scalar;
            double cubeOfScalar = squareOfScalar * scalar;
            return (2.0 * cubeOfScalar - 3.0 * squareOfScalar + 1.0) * this.v0 + (cubeOfScalar - 2.0 * squareOfScalar + scalar) * this.d0 + (3.0 * squareOfScalar - 2.0 * cubeOfScalar) * this.v1 + (cubeOfScalar - squareOfScalar) * this.d1;
        }
    }
}
