using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal abstract class RenderQuery : IQuery
    {
        protected const double minCurvature = 0.001;
        private Matrix4x4D viewProjection;
        private float viewScale;
        private List<int> result;
        private InstanceLayer instanceLayer;
        protected double sphereRadius;

        internal RenderQuery(Matrix4x4D projection, float scale, InstanceLayer layer, List<int> queryResult)
        {
            this.viewProjection = projection;
            this.viewScale = scale;
            this.instanceLayer = layer;
            this.result = queryResult;
        }

        protected abstract Box3D Get3DBounds(SpatialIndex.Node node);

        public bool IsRelevant(SpatialIndex.Node node)
        {
            bool flag1 = true;
            bool flag2 = true;
            bool flag3 = true;
            bool flag4 = true;
            bool flag5 = true;
            bool flag6 = true;
            Box3D box3D = this.Get3DBounds(node);
            double maxInstanceExtent = this.instanceLayer.GetMaxInstanceExtent(this.viewScale, (int)node.MaxClusterOrStackSize);
            box3D.Inflate(maxInstanceExtent);
            foreach (Vector4D p in new Vector4D[8]
      {
        new Vector4D(box3D.minX, box3D.minY, box3D.minZ, 1.0),
        new Vector4D(box3D.minX, box3D.maxY, box3D.minZ, 1.0),
        new Vector4D(box3D.maxX, box3D.minY, box3D.minZ, 1.0),
        new Vector4D(box3D.maxX, box3D.maxY, box3D.minZ, 1.0),
        new Vector4D(box3D.minX, box3D.minY, box3D.maxZ, 1.0),
        new Vector4D(box3D.minX, box3D.maxY, box3D.maxZ, 1.0),
        new Vector4D(box3D.maxX, box3D.minY, box3D.maxZ, 1.0),
        new Vector4D(box3D.maxX, box3D.maxY, box3D.maxZ, 1.0)
      })
            {
                Vector4D vector4D = this.viewProjection.Transform(p);
                if (vector4D.X >= -vector4D.W)
                    flag1 = false;
                if (vector4D.X <= vector4D.W)
                    flag2 = false;
                if (vector4D.Y >= -vector4D.W)
                    flag3 = false;
                if (vector4D.Y <= vector4D.W)
                    flag4 = false;
                if (vector4D.Z >= 0.0)
                    flag5 = false;
                if (vector4D.Z <= vector4D.W)
                    flag6 = false;
            }
            if (!flag1 && !flag2 && (!flag3 && !flag4) && !flag5)
                return !flag6;
            else
                return false;
        }

        public void ProcessLeaf(SpatialIndex.Node node)
        {
            this.result.Add(node.BlockId);
        }

        public static RenderQuery GetQuery(double curvature, Matrix4x4D projection, float scale, InstanceLayer layer, List<int> queryResult)
        {
            if (curvature >= 1.0)
                return (RenderQuery)new OnUnitSphereRenderQuery(projection, scale, layer, queryResult);
            if (curvature <= 0.001)
                return (RenderQuery)new OnFlatMapRenderQuery(projection, scale, layer, queryResult);
            else
                return (RenderQuery)new OnWarpSphereRenderQuery(curvature, projection, scale, layer, queryResult);
        }
    }
}
