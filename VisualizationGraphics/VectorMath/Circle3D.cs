using Microsoft.Data.Visualization.Engine.MathExtensions;
using System;

namespace Microsoft.Data.Visualization.Engine.VectorMath
{
    public struct Circle3D
    {
        private Vector3D center;
        private Vector3D A;
        private Vector3D B;

        public Vector3D Center
        {
            get
            {
                return this.center;
            }
        }

        public Circle3D(Vector3D C, Vector3D V, Vector3D W)
        {
            this.center = C;
            this.A = V;
            this.B = W;
        }

        public Circle3D(Vector3D C, Vector3D normal, double radius)
        {
            normal.AssertIsUnitVector();
            this.center = C;
            Coordinates.GetLocalFrame(normal, out this.B, out this.A);
            this.A *= radius;
            this.B *= radius;
        }

        public Vector3D[] Intersect(PlaneD plane)
        {
            double[] numArray = MathEx.SolveTrigEquation(plane.Normal * this.A, plane.Normal * this.B, plane.Normal * this.center + plane.D);
            Vector3D[] vector3DArray = new Vector3D[numArray.Length];
            for (int index = 0; index < vector3DArray.Length; ++index)
                vector3DArray[index] = this.center + Math.Cos(numArray[index]) * this.A + Math.Sin(numArray[index]) * this.B;
            return vector3DArray;
        }
    }
}
