using Microsoft.Data.Visualization.Engine.Graphics.Internal;
using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine
{
    internal class Tesselator
    {
        private List<Vector3D> regionVertices = new List<Vector3D>();
        private List<int> rings = new List<int>();
        private RegionTriangleList tesselatedRegion = new RegionTriangleList();
        private const double maxLongitude = 3.14158951199714;
        private const double maxLatitude = 1.57079475599857;
        private const double maxEdgeLength = 0.03;
        private const double maxSquaredEdgeLength = 0.0009;
        private Vector3D currentVertex;

        public RegionTriangleList TesselatedRegion
        {
            get
            {
                return this.tesselatedRegion;
            }
        }

        public void BeginPolygon()
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "RegionsLayer.BeginPolygon.");
            this.regionVertices.Clear();
            this.rings.Clear();
        }

        public void BeginRing(double longitude, double latitude)
        {
            MathEx.Clamp(ref longitude, -maxLongitude, maxLongitude);
            MathEx.Clamp(ref latitude, -maxLatitude, maxLatitude);
            this.currentVertex = Coordinates.GeoTo3D(longitude, latitude);
            this.regionVertices.Add(this.currentVertex);
            this.rings.Add(1);
        }

        public void AddVertex(double longitude, double latitude)
        {
            MathEx.Clamp(ref longitude, -maxLongitude, maxLongitude);
            MathEx.Clamp(ref latitude, -maxLatitude, maxLatitude);
            this.AddVertex3D(Coordinates.GeoTo3D(longitude, latitude));
        }

        public unsafe void EndPolygon()
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "RegionsLayer.EndPolygon.");
            if (this.regionVertices.Count > int.MaxValue)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Vertex count {0} exceeds the limit.", (object)this.regionVertices.Count);
            }
            else
            {
                IntPtr pVertexArray = IntPtr.Zero;
                IntPtr pRingArray = IntPtr.Zero;
                IntPtr pTrangleCoordinatesArray = IntPtr.Zero;
                IntPtr pTrangleIndexArray = IntPtr.Zero;
                try
                {
                    this.tesselatedRegion.Clear();
                    // 从List<Vector3D>构建顶点数组
                    int vertexSize = Marshal.SizeOf(typeof(double)) * 3;
                    pVertexArray = Marshal.AllocCoTaskMem(this.regionVertices.Count * vertexSize);
                    int i = 0;
                    IntPtr pVertex = pVertexArray;
                    while (i < this.regionVertices.Count)
                    {
                        double* pVertexElementArray = (double*)(void*)pVertex;
                        *pVertexElementArray = this.regionVertices[i].X;
                        pVertexElementArray[1] = this.regionVertices[i].Z;
                        pVertexElementArray[2] = this.regionVertices[i].Y;
                        ++i;
                        pVertex += vertexSize;
                    }
                    // 从List<int>构建环数组
                    int ringUnit = Marshal.SizeOf(typeof(int));
                    pRingArray = Marshal.AllocCoTaskMem(this.rings.Count * ringUnit);
                    int j = 0;
                    IntPtr pRing = pRingArray;
                    while (j < this.rings.Count)
                    {
                        *(int*)(void*)pRing = this.rings[j];
                        ++j;
                        pRing += ringUnit;
                    }
                    // Tessellate
                    int trangleCoordinatesCount;
                    int trangleIndexCount;
                    GeoLib.Tessellate(
                        pVertexArray, this.regionVertices.Count * 3, 
                        pRingArray, this.rings.Count, maxEdgeLength, 
                        &pTrangleCoordinatesArray, &trangleCoordinatesCount,
                        &pTrangleIndexArray, &trangleIndexCount);
                    if (trangleCoordinatesCount < 9 || trangleIndexCount < 3)
                        return;
                    // 根据三角形顶点坐标构建Region
                    double* pTrangleCoordinates = (double*)pTrangleCoordinatesArray.ToPointer();
                    int* pTrangleIndex = (int*)pTrangleIndexArray.ToPointer();
                    if (trangleIndexCount > int.MaxValue)
                        return;
                    int m = 0;
                    while (m < trangleCoordinatesCount)
                    {
                        Vector3D position = new Vector3D(pTrangleCoordinates[m], pTrangleCoordinates[m + 2], pTrangleCoordinates[m + 1]);
                        position.AssertIsUnitVector();
                        this.tesselatedRegion.AddVertex(position);
                        m += 3;
                    }
                    for (int n = 0; n < trangleIndexCount; ++n)
                        this.tesselatedRegion.AddIndex(pTrangleIndex[n]);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pVertexArray);
                    Marshal.FreeCoTaskMem(pRingArray);
                    GeoLib.DeleteArray(pTrangleCoordinatesArray);
                    GeoLib.DeleteArray(pTrangleIndexArray);
                }
                // 构建环
                List<Vector3D> rings = new List<Vector3D>();
                for (int x = 0; x < this.regionVertices.Count; ++x)
                    rings.Add(this.regionVertices[x]);
                this.tesselatedRegion.SetRingVertices(rings);
                int beginRing = 0;
                for (int y = 0; y < this.rings.Count; ++y)
                {
                    this.tesselatedRegion.AddRing(new Tuple<int, int>(beginRing, this.rings[y]));
                    beginRing += this.rings[y];
                }
            }
        }

        private void AddVertex3D(Vector3D vertex)
        {
            if (this.rings.Count < 1)
                return;
            if (Vector3D.DistanceSq(vertex, this.currentVertex) > maxSquaredEdgeLength)
            {
                Vector3D vertex1 = vertex + this.currentVertex;
                if (!vertex1.Normalize())
                    throw new InvalidPolygonException("A polygon edge with antipodal vertices");
                this.AddVertex3D(vertex1);
                this.AddVertex3D(vertex);
            }
            else
            {
                this.regionVertices.Add(vertex);
                List<int> list;
                int index;
                (list = this.rings)[index = this.rings.Count - 1] = list[index] + 1;
                this.currentVertex = vertex;
            }
        }
    }
}
