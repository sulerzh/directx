using Microsoft.Data.Visualization.Engine.VectorMath;
namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public struct Box3F
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        public float minZ;
        public float maxZ;

        public bool IsEmpty
        {
            get
            {
                return this.minX > this.maxX || this.minY > this.maxY || this.minZ > this.maxZ;
            }
        }

        public Box3F(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
        {
            this.minX = xMin;
            this.maxX = xMax;
            this.minY = yMin;
            this.maxY = yMax;
            this.minZ = zMin;
            this.maxZ = zMax;
        }

        private void UpdateValue(float newMin, float newMax, ref float minValue, ref float maxValue)
        {
            if (newMax > maxValue)
                maxValue = newMax;
            if (newMin >= minValue)
                return;
            minValue = newMin;
        }

        public void Initialize()
        {
            this.minX = this.minY = this.minZ = float.MaxValue;
            this.maxX = this.maxY = this.maxZ = float.MinValue;
        }

        public void UpdateWith(Vector3F point)
        {
            this.UpdateValue(point.X, point.X, ref this.minX, ref this.maxX);
            this.UpdateValue(point.Y, point.Y, ref this.minY, ref this.maxY);
            this.UpdateValue(point.Z, point.Z, ref this.minZ, ref this.maxZ);
        }

        public void UpdateWith(Box3F other)
        {
            this.UpdateValue(other.minX, other.maxX, ref this.minX, ref this.maxX);
            this.UpdateValue(other.minY, other.maxY, ref this.minY, ref this.maxY);
            this.UpdateValue(other.minZ, other.maxZ, ref this.minZ, ref this.maxZ);
        }

        public bool Intersects(Box3F other)
        {
            if (this.IsEmpty || other.IsEmpty || 
                this.maxX < other.minX || this.minX > other.maxX || 
                this.maxY < other.minY || this.minY > other.maxY ||
                this.maxZ < other.minZ || this.minZ > other.maxZ)
                return false;
            else
                return true;
        }

        public bool Includes(Box3F other)
        {
            return this.minX <= other.minX && this.maxX >= other.maxX &&
                   this.minY <= other.minY && this.maxY >= other.maxY &&
                   this.minZ <= other.minZ && this.maxZ >= other.maxZ;
        }

        public bool Contains(Vector3F point)
        {
            return this.minX <= point.X && this.maxX >= point.X &&
                   this.minY <= point.Y && this.maxY >= point.Y &&
                   this.minZ <= point.Z && this.maxZ >= point.Z;
        }

        public bool Contains(Vector3D point)
        {
            return this.minX <= point.X && this.maxX >= point.X &&
                   this.minY <= point.Y && this.maxY >= point.Y &&
                   this.minZ <= point.Z && this.maxZ >= point.Z;
        }

        public void Inflate(float margin)
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
