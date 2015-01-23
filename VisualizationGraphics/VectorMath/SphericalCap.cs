using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    public class SphericalCap : Cap
    {
        public static SphericalCap Empty = new SphericalCap(Vector3D.XVector, -1.0);

        public override double AngularExtent
        {
            get
            {
                return 2.0 * Math.Asin(this.MaxDistanceFromCenter / 2.0);
            }
        }

        public override double ProjectionRadius
        {
            get
            {
                if (this.extent >= 2.0)
                    return 1.0;
                else
                    return Math.Sqrt(this.extent * (4.0 - this.extent)) / 2.0;
            }
        }

        public override bool IsWholeWorld
        {
            get
            {
                return this.extent == 4.0;
            }
            set
            {
                this.extent = 4.0;
            }
        }

        public SphericalCap()
        {
        }

        public SphericalCap(Vector3D C, double e)
            : base(C, e)
        {
            C.AssertIsUnitVector();
        }

        public SphericalCap(List<Vector3D> locations)
            : base(Vector3D.XVector, -1.0)
        {
            if (locations.Count < 1)
                return;
            if (!this.SetCenterImplementation(locations))
            {
                this.center = Vector3D.XVector;
                this.extent = 4.0;
            }
            else
                this.SetExtent(locations);
        }

        private bool SetCenterImplementation(List<Vector3D> locations)
        {
            this.center = Vector3D.Empty;
            for (int index = 0; index < locations.Count; ++index)
            {
                SphericalCap sphericalCap = this;
                Vector3D vector3D = sphericalCap.center + locations[index];
                sphericalCap.center = vector3D;
            }
            return this.center.Normalize();
        }

        public override bool SetCenter(List<Vector3D> locations)
        {
            return this.SetCenterImplementation(locations);
        }

        public override double SquaredDistanceFromCenter(Vector3D point)
        {
            return Vector3D.DistanceSq(point, this.center);
        }

        protected override void AssertPointValidity(Vector3D point)
        {
            point.AssertIsUnitVector();
        }
    }
}
