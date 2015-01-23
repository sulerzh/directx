using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public struct Box2D
    {
        public double minX;
        public double maxX;
        public double minY;
        public double maxY;

        public bool IsEmpty
        {
            get
            {
                return this.minX > this.maxX || this.minY > this.maxY;
            }
        }

        public double Extent
        {
            get
            {
                return MathEx.Hypot(this.maxX - this.minX, this.maxY - this.minY);
            }
        }

        public Box2D(double xMin, double xMax, double yMin, double yMax)
        {
            this.minX = xMin;
            this.maxX = xMax;
            this.minY = yMin;
            this.maxY = yMax;
        }

        private void UpdateValue(double newMin, double newMax, ref double minValue, ref double maxValue)
        {
            if (newMax > maxValue)
                maxValue = newMax;
            if (newMin >= minValue)
                return;
            minValue = newMin;
        }

        public void Initialize()
        {
            this.minX = this.minY = double.MaxValue;
            this.maxX = this.maxY = double.MinValue;
        }

        public void UpdateWith(double x, double y)
        {
            this.UpdateValue(x, x, ref this.minX, ref this.maxX);
            this.UpdateValue(y, y, ref this.minY, ref this.maxY);
        }

        public void UpdateWith(Vector2D point)
        {
            this.UpdateWith(point.X, point.Y);
        }

        public void UpdateWith(Vector2F point)
        {
            this.UpdateValue((double)point.X, (double)point.X, ref this.minX, ref this.maxX);
            this.UpdateValue((double)point.Y, (double)point.Y, ref this.minY, ref this.maxY);
        }

        public void UpdateWith(Box2D other)
        {
            this.UpdateValue(other.minX, other.maxX, ref this.minX, ref this.maxX);
            this.UpdateValue(other.minY, other.maxY, ref this.minY, ref this.maxY);
        }

        public bool Intersects(Box2D other)
        {
            if (this.IsEmpty || other.IsEmpty || this.maxX < other.minX || this.minX > other.maxX || this.maxY < other.minY || this.minY > other.maxY)
                return false;
            else
                return true;
        }

        public bool Includes(Box2D other)
        {
            return 
                this.minX <= other.minX && this.maxX >= other.maxX && 
                this.minY <= other.minY && this.maxY >= other.maxY;
        }

        public bool Contains(Vector2D point)
        {
            return 
                this.minX <= point.X && this.maxX >= point.X &&
                this.minY <= point.Y && this.maxY >= point.Y;
        }
    }
}
