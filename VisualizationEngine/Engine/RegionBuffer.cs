using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    internal class RegionBuffer : DisposableResource
    {
        private List<VertexBuffer> vertices = new List<VertexBuffer>();
        private List<VertexBuffer> ringVertices = new List<VertexBuffer>();
        private List<IndexBuffer> indices = new List<IndexBuffer>();
        private List<int> verticesInUse = new List<int>();
        private List<int> ringVerticesInUse = new List<int>();
        private List<int> indicesInUse = new List<int>();
        private int lastVertexBufferIndex = -1;
        private int lastIndexBufferIndex = -1;
        private int lastRingVertexBufferIndex = -1;
        private List<List<Vector3D>> verticesSource = new List<List<Vector3D>>();
        private List<List<Vector3D>> ringVerticesSource = new List<List<Vector3D>>();
        private const int MaxRegionVertexBufferCount = 349525;
        private const int MaxRegionIndexBufferCount = 1048576;

        public Vector3D GetVertex(RegionBufferToken token, int index, bool planarCoordinates)
        {
            for (int index1 = 0; index1 < verticesInUse.Count; ++index1)
            {
                if (vertices[index1] == token.Vertices)
                    return RegionBuffer.GetVertexPosition(verticesSource[index1][index], planarCoordinates);
            }
            return Vector3D.Empty;
        }

        public unsafe RegionBufferToken AddRegion(RegionTriangleList triangleList, bool planarCoordinates)
        {
            if (triangleList == null)
                return null;
            int vertexCount = triangleList.GetVertices().Count;
            int indexCount = triangleList.GetIndices().Count;
            if (vertexCount > MaxRegionVertexBufferCount || indexCount > MaxRegionIndexBufferCount)
                return null;
            EnsureBuffers(vertexCount, indexCount);
            int num1 = verticesInUse[lastVertexBufferIndex];
            int startVertex = indicesInUse[lastIndexBufferIndex];
            RegionVertex* pVertex = (RegionVertex*)vertices[lastVertexBufferIndex].GetData().ToPointer();
            for (int i = 0; i < vertexCount; ++i)
            {
                Vector3D vertexPosition = RegionBuffer.GetVertexPosition(triangleList.GetVertices()[i], planarCoordinates);
                verticesSource[lastVertexBufferIndex].Add(triangleList.GetVertices()[i]);
                //RegionVertex* regionVertexPtr2 = pVertex;
                //List<int> list;
                //int index2;
                //int num2;
                //(list = verticesInUse)[index2 = lastVertexBufferIndex] = (num2 = list[index2]) + 1;
                //regionVertexPtr2[num2] = new RegionVertex(vertexPosition);
                pVertex[verticesInUse[lastVertexBufferIndex]++] = new RegionVertex(vertexPosition);
            }
            uint* pIndex = (uint*)indices[lastIndexBufferIndex].GetData().ToPointer();
            for (int i = 0; i < indexCount; ++i)
            {
                //uint* numPtr2 = pIndex;
                //List<int> list;
                //int index2;
                //int num2;
                //(list = indicesInUse)[index2 = lastIndexBufferIndex] = (num2 = list[index2]) + 1;
                //numPtr2[num2] = (uint)(triangleList.GetIndices()[i] + num1);
                pIndex[indicesInUse[lastIndexBufferIndex]++] = (uint)(triangleList.GetIndices()[i] + num1);
            }
            vertices[lastVertexBufferIndex].SetDirty();
            indices[lastIndexBufferIndex].SetDirty();
            return new RegionBufferToken(vertices[lastVertexBufferIndex], indices[lastIndexBufferIndex], startVertex, indexCount);
        }

        public unsafe List<RegionBufferToken> AddRegionRings(RegionTriangleList triangleList, bool planarCoordinates)
        {
            if (triangleList == null)
                return null;
            int count = triangleList.GetRingVertices().Count;
            EnsureRingBuffers(count);
            int num1 = ringVerticesInUse[lastRingVertexBufferIndex];
            RegionVertex* pRingVertex = (RegionVertex*)ringVertices[lastRingVertexBufferIndex].GetData().ToPointer();
            for (int i = 0; i < count; ++i)
            {
                Vector3D vertexPosition = RegionBuffer.GetVertexPosition(triangleList.GetRingVertices()[i], planarCoordinates);
                ringVerticesSource[lastRingVertexBufferIndex].Add(triangleList.GetRingVertices()[i]);
                //RegionVertex* regionVertexPtr2 = pRingVertex;
                //List<int> list;
                //int index2;
                //int num2;
                //(list = ringVerticesInUse)[index2 = lastRingVertexBufferIndex] = (num2 = list[index2]) + 1;
                //regionVertexPtr2[num2] = new RegionVertex(vertexPosition);

                pRingVertex[ringVerticesInUse[lastRingVertexBufferIndex]++] = new RegionVertex(vertexPosition);
            }
            ringVertices[lastRingVertexBufferIndex].SetDirty();
            List<RegionBufferToken> tokens = new List<RegionBufferToken>();
            for (int i = 0; i < triangleList.GetRingSubsets().Count; ++i)
                tokens.Add(new RegionBufferToken(ringVertices[lastRingVertexBufferIndex], null, triangleList.GetRingSubsets()[i].Item1 + num1, triangleList.GetRingSubsets()[i].Item2));
            return tokens;
        }

        private bool EnsureBuffers(int polygonVertexCount, int polygonIndexCount)
        {
            bool flag = false;
            if (lastVertexBufferIndex == -1 || 
                polygonVertexCount + verticesInUse[lastVertexBufferIndex] > vertices[lastVertexBufferIndex].VertexCount)
            {
                ++lastVertexBufferIndex;
                verticesSource.Add(new List<Vector3D>());
                if (lastVertexBufferIndex == vertices.Count)
                {
                    vertices.Add(VertexBuffer.Create<RegionVertex>(null, MaxRegionVertexBufferCount, false));
                    verticesInUse.Add(0);
                }
                verticesInUse[lastVertexBufferIndex] = 0;
                flag = true;
            }
            if (lastIndexBufferIndex == -1 || polygonIndexCount + indicesInUse[lastIndexBufferIndex] > indices[lastIndexBufferIndex].IndexCount)
            {
                ++lastIndexBufferIndex;
                if (lastIndexBufferIndex == indices.Count)
                {
                    indices.Add(IndexBuffer.Create<uint>(null, MaxRegionIndexBufferCount, false));
                    indicesInUse.Add(0);
                }
                indicesInUse[lastIndexBufferIndex] = 0;
                flag = true;
            }
            return flag;
        }

        private bool EnsureRingBuffers(int polygonRingVertexCount)
        {
            bool flag = false;
            if (lastRingVertexBufferIndex == -1 || 
                polygonRingVertexCount + ringVerticesInUse[lastRingVertexBufferIndex] > ringVertices[lastRingVertexBufferIndex].VertexCount)
            {
                ++lastRingVertexBufferIndex;
                ringVerticesSource.Add(new List<Vector3D>());
                if (lastRingVertexBufferIndex == ringVertices.Count)
                {
                    ringVertices.Add(VertexBuffer.Create<RegionVertex>(null, MaxRegionVertexBufferCount, false));
                    ringVerticesInUse.Add(0);
                }
                ringVerticesInUse[lastRingVertexBufferIndex] = 0;
                flag = true;
            }
            return flag;
        }

        public unsafe void ReprojectVertices(bool planarCoordinates)
        {
            for (int i = 0; i < verticesInUse.Count; ++i)
            {
                RegionVertex* pVertex = (RegionVertex*)vertices[i].GetData().ToPointer();
                for (int j = 0; j < verticesInUse[i]; ++j)
                {
                    Vector3D vertexPosition = RegionBuffer.GetVertexPosition(verticesSource[i][j], planarCoordinates);
                    pVertex[j] = new RegionVertex(vertexPosition);
                }
                vertices[i].SetDirty();
            }
            for (int i = 0; i < ringVerticesInUse.Count; ++i)
            {
                RegionVertex* pRingVertex = (RegionVertex*)ringVertices[i].GetData().ToPointer();
                for (int j = 0; j < ringVerticesInUse[i]; ++j)
                {
                    Vector3D vertexPosition = RegionBuffer.GetVertexPosition(ringVerticesSource[i][j], planarCoordinates);
                    pRingVertex[j] = new RegionVertex(vertexPosition);
                }
                ringVertices[i].SetDirty();
            }
        }

        public void Clear()
        {
            lastVertexBufferIndex = -1;
            lastIndexBufferIndex = -1;
            lastRingVertexBufferIndex = -1;
            for (int i = 0; i < verticesInUse.Count; ++i)
                verticesInUse[i] = 0;
            for (int i = 0; i < indicesInUse.Count; ++i)
                indicesInUse[i] = 0;
            for (int i = 0; i < ringVerticesInUse.Count; ++i)
                ringVerticesInUse[i] = 0;
            verticesSource.Clear();
            ringVerticesSource.Clear();
        }

        private static Vector3D GetVertexPosition(Vector3D position, bool planarCoordinates)
        {
            position.AssertIsUnitVector();
            if (planarCoordinates)
                position = Coordinates.UnitSphereToFlatMap(position);
            return position;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            foreach (DisposableResource vertex in vertices)
                vertex.Dispose();
            foreach (DisposableResource ringVertex in ringVertices)
                ringVertex.Dispose();
            foreach (DisposableResource index in indices)
                index.Dispose();
        }
    }
}
