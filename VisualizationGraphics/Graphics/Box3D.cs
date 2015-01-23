using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public struct Box3D
    {
        public double minX;
        public double maxX;
        public double minY;
        public double maxY;
        public double minZ;
        public double maxZ;

        public bool IsEmpty
        {
            get
            {
                return this.minX > this.maxX || this.minY > this.maxY || this.minZ > this.maxZ;
            }
        }

        public Box3D(double xMin, double xMax, double yMin, double yMax, double zMin, double zMax)
        {
            this.minX = xMin;
            this.maxX = xMax;
            this.minY = yMin;
            this.maxY = yMax;
            this.minZ = zMin;
            this.maxZ = zMax;
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
            this.minX = this.minY = this.minZ = double.MaxValue;
            this.maxX = this.maxY = this.maxZ = double.MinValue;
        }

        public void UpdateWith(Vector3D point)
        {
            this.UpdateValue(point.X, point.X, ref this.minX, ref this.maxX);
            this.UpdateValue(point.Y, point.Y, ref this.minY, ref this.maxY);
            this.UpdateValue(point.Z, point.Z, ref this.minZ, ref this.maxZ);
        }

        public void UpdateWith(Box3D other)
        {
            this.UpdateValue(other.minX, other.maxX, ref this.minX, ref this.maxX);
            this.UpdateValue(other.minY, other.maxY, ref this.minY, ref this.maxY);
            this.UpdateValue(other.minZ, other.maxZ, ref this.minZ, ref this.maxZ);
        }

        public bool Intersects(Box3D other)
        {
            if (this.IsEmpty || other.IsEmpty || 
                this.maxX < other.minX || this.minX > other.maxX || 
                this.maxY < other.minY || this.minY > other.maxY || 
                this.maxZ < other.minZ || this.minZ > other.maxZ)
                return false;
            else
                return true;
        }

        public bool Includes(Box3D other)
        {
            return this.minX <= other.minX && this.maxX >= other.maxX &&
                   this.minY <= other.minY && this.maxY >= other.maxY &&
                   this.minZ <= other.minZ && this.maxZ >= other.maxZ;
        }

        public bool Contains(Vector3D point)
        {
            return
                this.minX <= point.X && this.maxX >= point.X &&
                this.minY <= point.Y && this.maxY >= point.Y &&
                this.minZ <= point.Z && this.maxZ >= point.Z;
        }

        public void Inflate(double margin)
        {
            this.minX -= margin;
            this.minY -= margin;
            this.minZ -= margin;
            this.maxX += margin;
            this.maxY += margin;
            this.maxZ += margin;
        }
    }
}
