using System;
using System.Globalization;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    [Serializable]
    public struct Ray3D : IEquatable<Ray3D>
    {
        public static readonly Ray3D Empty = new Ray3D(true);
        public Vector3D Origin;
        public Vector3D Direction;

        private Ray3D(bool internalConstructor)
        {
            this.Origin = new Vector3D(0.0, 0.0, 0.0);
            this.Direction = new Vector3D(1.0, 0.0, 0.0);
        }

        public Ray3D(Vector3D origin, Vector3D direction)
        {
            this.Origin = origin;
            this.Direction = direction;
            double num = 1.0 / Math.Sqrt(this.Direction.LengthSq());
            this.Direction.X *= num;
            this.Direction.Y *= num;
            this.Direction.Z *= num;
        }

        public Ray3D(Ray3D ray)
        {
            this.Origin = ray.Origin;
            this.Direction = ray.Direction;
        }

        public static bool operator ==(Ray3D left, Ray3D right)
        {
            return left.Origin.Equals(ref right.Origin) && left.Direction.Equals(ref right.Direction);
        }

        public static bool operator !=(Ray3D left, Ray3D right)
        {
            return !left.Origin.Equals(ref right.Origin) || !left.Direction.Equals(ref right.Direction);
        }

        public bool IsValid()
        {
            return !double.IsNaN(this.Direction.X) && !double.IsNaN(this.Direction.Y) && (!double.IsNaN(this.Direction.Z) && !double.IsNaN(this.Origin.X)) && (!double.IsNaN(this.Origin.Y) && !double.IsNaN(this.Origin.Z));
        }

        public Vector3D GetSmallestPoint()
        {
            this.Direction.AssertIsUnitVector();
            double num = -this.Origin * this.Direction;
            if (num < 0.0)
                return this.Origin;
            else
                return this.Origin + this.Direction * num;
        }

        public double GetDistanceFromOrigin()
        {
            Vector3D smallestPoint = this.GetSmallestPoint();
            if (smallestPoint == Vector3D.Empty)
                return this.Origin.Length();
            else
                return smallestPoint.Length();
        }

        public Vector3D GetSphereIntersection(double radius)
        {
            // Origin is vector of sphere center to ray orition
            double projOnRay = this.Origin * this.Direction;

            double l_squared = this.Origin * this.Origin;
            double r_squared = radius * radius;

            bool isOutside = l_squared > r_squared;
            bool isAwayFromCenter = projOnRay > 0;
            // ray goes away from the center and the point is outside the sphere
            if (isOutside && isAwayFromCenter)
            {
                return Vector3D.Empty;
            }

            // distance of the ray from the sphere center(squared)
            double m_squared = l_squared - projOnRay * projOnRay;

            bool isDistanceOfCenterToRayBiggerThanRadius = m_squared > r_squared;
            if (isDistanceOfCenterToRayBiggerThanRadius)
            {
                return Vector3D.Empty;
            }

            //球心到射线距离最近的点到球面的距离（在射线上）
            double q = Math.Sqrt(r_squared - m_squared);
            //球面相交点到射线起点的距离
            double t = -projOnRay - q;
            t = Math.Max(0.0, t);
            return (this.Origin + t * this.Direction);
        }

        public override int GetHashCode()
        {
            return this.Origin.GetHashCode() ^ this.Direction.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Ray3D)
                return (Ray3D)obj == this;
            else
                return false;
        }

        public bool Equals(Ray3D ray)
        {
            if (this.Origin.Equals(ref ray.Origin))
                return this.Direction.Equals(ref ray.Direction);
            else
                return false;
        }

        public bool Equals(ref Ray3D ray)
        {
            if (this.Origin.Equals(ref ray.Origin))
                return this.Direction.Equals(ref ray.Direction);
            else
                return false;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentUICulture,
                "{{Origin={0},Direction={1}}}",
                new object[2]
                {
                    (object) this.Origin,
                    (object) this.Direction
                });
        }
    }
}
