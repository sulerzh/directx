using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct InstanceVertex
    {
        public Vector3D Position;
        public Vector3D Normal;
        public float Texture;

        public InstanceVertex(Vector3D offset, Vector3D normal)
        {
            this.Position = offset;
            this.Normal = normal;
            this.Texture = 1f;
        }

        public InstanceVertex(Vector3D offset, Vector3D normal, float texture)
        {
            this.Position = offset;
            this.Normal = normal;
            this.Texture = texture;
        }
    }
}
