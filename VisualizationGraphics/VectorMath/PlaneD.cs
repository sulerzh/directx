using System;
using System.Globalization;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    [Serializable]
    public struct PlaneD : IEquatable<PlaneD>
    {
        private Vector3D normal;
        private double d;

        public Vector3D Normal
        {
            get
            {
                return this.normal;
            }
            set
            {
                this.normal = value;
            }
        }

        public double A
        {
            get
            {
                return this.normal.X;
            }
            set
            {
                this.normal.X = value;
            }
        }

        public double B
        {
            get
            {
                return this.normal.Y;
            }
            set
            {
                this.normal.Y = value;
            }
        }

        public double C
        {
            get
            {
                return this.normal.Z;
            }
            set
            {
                this.normal.Z = value;
            }
        }

        public double D
        {
            get
            {
                return this.d;
            }
            set
            {
                this.d = value;
            }
        }

        public PlaneD(Vector3D N, double constant)
        {
            this.normal = N;
            this.d = constant;
        }

        public static bool operator ==(PlaneD left, PlaneD right)
        {
            return left.normal == right.normal && left.d == right.d;
        }

        public static bool operator !=(PlaneD left, PlaneD right)
        {
            return !(left == right);
        }

        public double DistanceTo(Vector3D vector)
        {
            return this.normal * vector + this.d;
        }

        public bool Contains(Vector3D point)
        {
            this.normal.AssertIsUnitVector();
            return this.normal * point + this.d <= Constants.Fuzz;
        }

        public Vector3D Intersection(Ray3D worldRay)
        {
            double num1 = this.normal * worldRay.Direction;
            if (Math.Abs(num1) < Constants.Fuzz)
                return Vector3D.Empty;
            double num2 = -(this.normal * worldRay.Origin + this.d) / num1;
            if (num2 < 0.0)
                return Vector3D.Empty;
            else
                return worldRay.Origin + num2 * worldRay.Direction;
        }

        public bool Intersection(PlaneD plane1, PlaneD plane2, out Vector3D point)
        {
            Vector3D normal1 = this.Normal;
            Vector3D normal2 = plane1.Normal;
            Vector3D normal3 = plane2.Normal;
            double num = Vector3D.Dot(Vector3D.Cross(ref normal1, ref normal2), normal3);
            Vector3D vector3D = -this.d * Vector3D.Cross(ref normal2, ref normal3) - plane1.d * Vector3D.Cross(ref normal3, ref normal1) - plane2.d * Vector3D.Cross(ref normal1, ref normal2);
            if (num * num < 1E-14 * vector3D * vector3D)
            {
                point = Vector3D.Empty;
                return false;
            }
            else
            {
                point = vector3D / num;
                return true;
            }
        }

        public bool Normalize()
        {
            double num = this.normal.Length();
            if (num == 0.0)
                return false;
            this.normal /= num;
            this.D /= num;
            return true;
        }

        public static PlaneD FromPointNormal(Vector3D point, Vector3D normal)
        {
            normal.Normalize();
            return new PlaneD(normal, -normal * point);
        }

        public static PlaneD FromPoints(Vector3D p1, Vector3D p2, Vector3D p3)
        {
            Vector3D left = Vector3D.Subtract(ref p3, ref p2);
            Vector3D right = Vector3D.Subtract(ref p2, ref p1);
            Vector3D point = Vector3D.Cross(ref left, ref right);
            point.Normalize();
            return PlaneD.FromPointNormal(point, p1);
        }

        public override int GetHashCode()
        {
            return this.normal.GetHashCode() ^ this.D.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PlaneD)
                return (PlaneD)obj == this;
            else
                return false;
        }

        public override string ToString()
        {
            return this.normal.ToString() +
                   string.Format(
                       CultureInfo.CurrentUICulture,
                       ", constant={0}", new object[1]
                       {
                           (object) this.D
                       });
        }

        public bool Equals(PlaneD other)
        {
            return this.normal == other.normal && this.D == other.d;
        }
    }
}
