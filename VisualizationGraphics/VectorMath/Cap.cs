using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    public abstract class Cap
    {
        protected Vector3D center;
        protected double extent;

        public Vector3D Center
        {
            get
            {
                return this.center;
            }
        }

        public double MaxDistanceFromCenter2
        {
            get
            {
                return this.extent;
            }
        }

        public double MaxDistanceFromCenter
        {
            get
            {
                return Math.Sqrt(this.extent);
            }
        }

        public abstract double AngularExtent { get; }

        public abstract double ProjectionRadius { get; }

        public abstract bool IsWholeWorld { get; set; }

        public bool IsEmpty
        {
            get
            {
                return this.extent < 0.0;
            }
        }

        protected Cap()
        {
            this.center = Vector3D.XVector;
            this.extent = -1.0;
        }

        protected Cap(Vector3D C, double e)
        {
            this.center = C;
            this.extent = e;
        }

        public void SetExtent(List<Vector3D> locations)
        {
            this.extent = -1.0;
            for (int index = 0; index < locations.Count; ++index)
            {
                double num = this.SquaredDistanceFromCenter(locations[index]);
                if (num > this.extent)
                    this.extent = num;
            }
        }

        public bool Contains(Vector3D point)
        {
            this.AssertPointValidity(point);
            return Vector3D.DistanceSq(point, this.center) <= this.extent;
        }

        public static Cap Construct(bool flat)
        {
            if (flat)
                return (Cap)new FlatCap();
            else
                return (Cap)new SphericalCap();
        }

        public static Cap Construct(Vector3D c, double e, bool flat)
        {
            if (flat)
                return (Cap)new FlatCap(c, e);
            else
                return (Cap)new SphericalCap(c, e);
        }

        public abstract bool SetCenter(List<Vector3D> locations);

        public abstract double SquaredDistanceFromCenter(Vector3D point);

        protected abstract void AssertPointValidity(Vector3D point);
    }
}
