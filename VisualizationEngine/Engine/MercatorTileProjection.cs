using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
    /// <summary>
    /// Bing Maps Tile System
    /// https://msdn.microsoft.com/en-us/library/bb259689.aspx
    /// </summary>
    internal class MercatorTileProjection : TileProjection
    {
        private const double BaseMetersPerPixel = 156543.0;
        private const double OffsetMeters = 20037508.0;
        private IndexBuffer tileIndices;

        public MercatorTileProjection()
        {
            this.BuildIndexBuffers();
        }

        /// <summary>
        /// 构建格网索引
        /// </summary>
        private void BuildIndexBuffers()
        {
            short[] data = new short[1536];
            bool flag = false;
            int gridCount = 8;
            int quadIndex = 0;
            // Build common set of indexes for the 4 child meshes.
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    // 正方形格网，每个格子剖分为两个三角形，每个三角形三个定点
                    int bufferSize = gridCount * gridCount * 3 * 2; // 384
                    short[] quadIndexData = new short[bufferSize];
                    int index = 0;
                    for (int m = gridCount * i; m < gridCount * (i + 1); ++m)
                    {
                        for (int n = gridCount * j; n < gridCount * (j + 1); ++n)
                        {
                            // 左手坐标系，17为每行中的顶点数量
                            if (flag)
                            {
                                // 副对角线剖分
                                quadIndexData[index] = (short)(m * 17 + n);
                                quadIndexData[index + 2] = (short)((m + 1) * 17 + n);
                                quadIndexData[index + 1] = (short)(m * 17 + (n + 1));
                                quadIndexData[index + 3] = (short)(m * 17 + (n + 1));
                                quadIndexData[index + 5] = (short)((m + 1) * 17 + n);
                                quadIndexData[index + 4] = (short)((m + 1) * 17 + (n + 1));
                                index += 6;
                            }
                            else
                            {
                                // 主对角线剖分
                                quadIndexData[index] = (short)(m * 17 + n);
                                quadIndexData[index + 2] = (short)((m + 1) * 17 + (n + 1));
                                quadIndexData[index + 1] = (short)(m * 17 + (n + 1));
                                quadIndexData[index + 3] = (short)(m * 17 + n);
                                quadIndexData[index + 5] = (short)((m + 1) * 17 + n);
                                quadIndexData[index + 4] = (short)((m + 1) * 17 + (n + 1));
                                index += 6;
                            }
                            flag = !flag;
                        }
                        flag = !flag;
                    }
                    // 复制数组
                    Array.Copy(quadIndexData, 0, data, quadIndex++ * quadIndexData.Length, quadIndexData.Length);
                }
            }
            this.tileIndices = IndexBuffer.Create(data, false);
        }

        public override IndexBuffer GetTileIndexBuffer()
        {
            return this.tileIndices;
        }

        public override TileExtent GetExtent(Tile tile)
        {
            double span = 2.0 * Math.PI / Math.Pow(2.0, tile.Level);
            double north = tile.IsNorthmost ? Math.PI / 2.0 : AbsoluteMetersToLatAtZoom(tile.Y * 256, tile.Level);
            double sourth = tile.IsSouthmost ? -1.0 * Math.PI / 2.0 : AbsoluteMetersToLatAtZoom((tile.Y + 1) * 256, tile.Level);
            double west = tile.X * span - Math.PI;
            double east = (tile.X + 1) * span - Math.PI;
            return new TileExtent(new Coordinates(west, north), new Coordinates(east, sourth));
        }

        /// <summary>
        /// 计算Tile的包围球，并返回瓦片的四个角点的空间坐标
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="corners"></param>
        /// <returns></returns>
        public override SphereD ComputeBoundingSphere(Tile tile, out Vector3D[] corners)
        {
            TileExtent extent = this.GetExtent(tile);
            double lat = AbsoluteMetersToLatAtZoom((tile.Y * 2 + 1) * 256, tile.Level + 1);
            Vector3D center = Coordinates.GeoTo3DFlattening((extent.TopLeft.Longitude + extent.BottomRight.Longitude) / 2.0, lat, tile.FlatteningFactor);
            Vector3D topLeftCorner = Coordinates.GeoTo3DFlattening(extent.TopLeft.Longitude, extent.TopLeft.Latitude, tile.FlatteningFactor);
            Vector3D bottomRightCorner = Coordinates.GeoTo3DFlattening(extent.BottomRight.Longitude, extent.BottomRight.Latitude, tile.FlatteningFactor);
            Vector3D topRightCorner = Coordinates.GeoTo3DFlattening(extent.BottomRight.Longitude, extent.TopLeft.Latitude, tile.FlatteningFactor);
            Vector3D bottomLeftCorner = Coordinates.GeoTo3DFlattening(extent.TopLeft.Longitude, extent.BottomRight.Latitude, tile.FlatteningFactor);
            Vector3D oaVector = topLeftCorner;
            oaVector = oaVector.Subtract(center);
            Vector3D obVector = topRightCorner;
            obVector = obVector.Subtract(center);
            Vector3D ocVector = bottomLeftCorner;
            ocVector = ocVector.Subtract(center);
            Vector3D odVector = bottomRightCorner;
            odVector = odVector.Subtract(center);
            double radius = Math.Max(Math.Max(oaVector.Length(), obVector.Length()), Math.Max(ocVector.Length(), odVector.Length()));
            corners = new Vector3D[4]
            {
                topLeftCorner,
                topRightCorner,
                bottomLeftCorner,
                bottomRightCorner
            };
            return new SphereD(center, radius);
        }

        public override unsafe void InitializeVertexBuffer(Tile tile, VertexBuffer vertexBuffer, double flatteningFactor)
        {
            double radPerPixcel = 2.0 * Math.PI / Math.Pow(2.0, tile.Level);
            double topLat = AbsoluteMetersToLatAtZoom(tile.Y * 256, tile.Level);
            double bottomLat = AbsoluteMetersToLatAtZoom((tile.Y + 1) * 256, tile.Level);
            double leftLon = tile.X * radPerPixcel - Math.PI;
            int x = tile.X;
            double certerLat = AbsoluteMetersToLatAtZoom((tile.Y * 2 + 1) * 256, tile.Level + 1);
            TileVertex* tileVertexPtr = (TileVertex*)(void*)vertexBuffer.GetData();
            double step = 1.0 / 16.0;
            for (int i = 0; i < 2; ++i)
            {
                // 瓦片顶点坐标原点位于坐下，编号自下往上,自左至右
                // i=0,通过latSpan计算的步长为负值，通过减实现自下往中
                // i=0,通过latSpan计算的步长为正值，通过加实现自中往上
                double latSpan = i == 0 ? bottomLat - certerLat : topLat - certerLat;
                int start = i == 0 ? 0 : 8;
                int end = i == 0 ? 8 : 16;
                for (int row = start; row <= end; ++row)
                {
                    double lat = i == 0 ? (bottomLat - 2.0 * step * latSpan * row) : (certerLat + 2.0 * step * latSpan * (row - 8));
                    for (int col = 0; col <= 16; ++col)
                    {
                        double lon = leftLon + step * radPerPixcel * col;
                        int index = row * 17 + col;
                        Vector3D vertexVector = Coordinates.GeoTo3DFlattening(lon, lat, flatteningFactor) - tile.ReferencePoint;
                        tileVertexPtr[index].Position = (Vector3F)vertexVector;
                        // 纹理坐标原点位于左上，方向为自上往下,自左至右
                        tileVertexPtr[index].Tu = col * (float)step;
                        tileVertexPtr[index].Tv = (float)((AbsoluteLatToMetersAtZoom(lat, tile.Level) - (tile.Y * 256)) / 256.0);
                    }
                }
                // 南北极点时，将瓦片的边缘线折叠为一个点
                if (flatteningFactor < 1.0 && (tile.IsNorthmost || tile.IsSouthmost))
                {
                    for (int col = 0; col <= 16; ++col)
                    {
                        int index = (tile.IsNorthmost ? 16 : 0) * 17 + col;
                        tileVertexPtr[index].Position = (Vector3F)(new Vector3D(0.0, tile.IsNorthmost ? 1.0 : -1.0, 0.0) - tile.ReferencePoint);
                    }
                }
            }
            vertexBuffer.SetDirty();
        }

        /// <summary>
        /// todo 探讨该方法与网络上普遍出现的方法之间的关系
        /// </summary>
        /// <param name="latitude">纬度，弧度单位</param>
        /// <param name="zoom">缩放级别，对应层号</param>
        /// <returns>像素坐标系，pixelY</returns>
        private static double AbsoluteLatToMetersAtZoom(double latitude, int zoom)
        {
            double num = Math.Sin(latitude);
            return (OffsetMeters - 0.5 * Constants.EarthRadius * Math.Log((1.0 + num) / (1.0 - num))) / MetersPerPixel2(zoom);
        }

        /// <summary>
        /// 像素坐标系pixelY转纬度
        /// </summary>
        /// <param name="pixelY">像素坐标系pixelY</param>
        /// <param name="zoom">缩放级别，对应层号</param>
        /// <returns>纬度，弧度单位</returns>
        private static double AbsoluteMetersToLatAtZoom(int pixelY, int zoom)
        {
            double levelMetersPerPixel = MetersPerPixel2(zoom);
            return Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp(-(OffsetMeters - pixelY * levelMetersPerPixel) / Constants.EarthRadius));
        }

        private static double MetersPerPixel2(int zoom)
        {
            return BaseMetersPerPixel / (1 << zoom);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing || this.tileIndices == null)
                return;
            this.tileIndices.Dispose();
        }
    }
}
