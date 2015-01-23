using System;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    [Serializable]
    public struct SphereD : IEquatable<SphereD>
    {
        public static readonly SphereD UnitSphere = new SphereD(Vector3D.Empty, 1.0);

        public Vector3D Origin { get; set; }

        public double Radius { get; set; }

        public SphereD(Vector3D origin, double radius)
        {
            this = new SphereD();
            if (radius <= 0.0)
                throw new ArgumentOutOfRangeException("radius");
            this.Origin = origin;
            this.Radius = radius;
        }

        public SphereD(SphereD sphere)
        {
            this = new SphereD();
            this.Origin = sphere.Origin;
            this.Radius = sphere.Radius;
        }

        public static bool operator ==(SphereD left, SphereD right)
        {
            return left.Origin == right.Origin && left.Radius == right.Radius;
        }

        public static bool operator !=(SphereD left, SphereD right)
        {
            return left.Origin != right.Origin && left.Radius != right.Radius;
        }

        public static SphereD Transform(SphereD sphere, Matrix4x4D matrix)
        {
            return new SphereD(Vector3D.TransformCoordinate(sphere.Origin, ref matrix), sphere.Radius);
        }

        public bool Equals(SphereD other)
        {
            return this == other;
        }

        public override bool Equals(object other)
        {
            return this == (SphereD)other;
        }

        public override int GetHashCode()
        {
            return this.Origin.GetHashCode() ^ this.Radius.GetHashCode();
        }
    }
}
