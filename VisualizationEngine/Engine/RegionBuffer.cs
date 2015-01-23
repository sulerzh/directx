// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionBuffer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

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
      for (int index1 = 0; index1 < this.verticesInUse.Count; ++index1)
      {
        if (this.vertices[index1] == token.Vertices)
          return RegionBuffer.GetVertexPosition(this.verticesSource[index1][index], planarCoordinates);
      }
      return Vector3D.Empty;
    }

    public unsafe RegionBufferToken AddRegion(RegionTriangleList triangleList, bool planarCoordinates)
    {
      if (triangleList == null)
        return (RegionBufferToken) null;
      int count1 = triangleList.GetVertices().Count;
      int count2 = triangleList.GetIndices().Count;
      if (count1 > 349525 || count2 > 1048576)
        return (RegionBufferToken) null;
      this.EnsureBuffers(count1, count2);
      int num1 = this.verticesInUse[this.lastVertexBufferIndex];
      int startVertex = this.indicesInUse[this.lastIndexBufferIndex];
      RegionVertex* regionVertexPtr1 = (RegionVertex*) this.vertices[this.lastVertexBufferIndex].GetData().ToPointer();
      for (int index1 = 0; index1 < count1; ++index1)
      {
        Vector3D vertexPosition = RegionBuffer.GetVertexPosition(triangleList.GetVertices()[index1], planarCoordinates);
        this.verticesSource[this.lastVertexBufferIndex].Add(triangleList.GetVertices()[index1]);
        RegionVertex* regionVertexPtr2 = regionVertexPtr1;
        List<int> list;
        int index2;
        int num2;
        (list = this.verticesInUse)[index2 = this.lastVertexBufferIndex] = (num2 = list[index2]) + 1;
        regionVertexPtr2[num2] = new RegionVertex(vertexPosition);
      }
      uint* numPtr1 = (uint*) this.indices[this.lastIndexBufferIndex].GetData().ToPointer();
      for (int index1 = 0; index1 < count2; ++index1)
      {
        uint* numPtr2 = numPtr1;
        List<int> list;
        int index2;
        int num2;
        (list = this.indicesInUse)[index2 = this.lastIndexBufferIndex] = (num2 = list[index2]) + 1;
        numPtr2[num2] = (uint)(triangleList.GetIndices()[index1] + num1);
      }
      this.vertices[this.lastVertexBufferIndex].SetDirty();
      this.indices[this.lastIndexBufferIndex].SetDirty();
      return new RegionBufferToken(this.vertices[this.lastVertexBufferIndex], this.indices[this.lastIndexBufferIndex], startVertex, count2);
    }

    public unsafe List<RegionBufferToken> AddRegionRings(RegionTriangleList triangleList, bool planarCoordinates)
    {
      if (triangleList == null)
        return (List<RegionBufferToken>) null;
      int count = triangleList.GetRingVertices().Count;
      this.EnsureRingBuffers(count);
      int num1 = this.ringVerticesInUse[this.lastRingVertexBufferIndex];
      RegionVertex* regionVertexPtr1 = (RegionVertex*) this.ringVertices[this.lastRingVertexBufferIndex].GetData().ToPointer();
      for (int index1 = 0; index1 < count; ++index1)
      {
        Vector3D vertexPosition = RegionBuffer.GetVertexPosition(triangleList.GetRingVertices()[index1], planarCoordinates);
        this.ringVerticesSource[this.lastRingVertexBufferIndex].Add(triangleList.GetRingVertices()[index1]);
        RegionVertex* regionVertexPtr2 = regionVertexPtr1;
        List<int> list;
        int index2;
        int num2;
        (list = this.ringVerticesInUse)[index2 = this.lastRingVertexBufferIndex] = (num2 = list[index2]) + 1;
        regionVertexPtr2[num2] = new RegionVertex(vertexPosition);
      }
      this.ringVertices[this.lastRingVertexBufferIndex].SetDirty();
      List<RegionBufferToken> list1 = new List<RegionBufferToken>();
      for (int index = 0; index < triangleList.GetRingSubsets().Count; ++index)
        list1.Add(new RegionBufferToken(this.ringVertices[this.lastRingVertexBufferIndex], (IndexBuffer) null, triangleList.GetRingSubsets()[index].Item1 + num1, triangleList.GetRingSubsets()[index].Item2));
      return list1;
    }

    private bool EnsureBuffers(int polygonVertexCount, int polygonIndexCount)
    {
      bool flag = false;
      if (this.lastVertexBufferIndex == -1 || polygonVertexCount + this.verticesInUse[this.lastVertexBufferIndex] > this.vertices[this.lastVertexBufferIndex].VertexCount)
      {
        ++this.lastVertexBufferIndex;
        this.verticesSource.Add(new List<Vector3D>());
        if (this.lastVertexBufferIndex == this.vertices.Count)
        {
          this.vertices.Add(VertexBuffer.Create<RegionVertex>((RegionVertex[]) null, 349525, false));
          this.verticesInUse.Add(0);
        }
        this.verticesInUse[this.lastVertexBufferIndex] = 0;
        flag = true;
      }
      if (this.lastIndexBufferIndex == -1 || polygonIndexCount + this.indicesInUse[this.lastIndexBufferIndex] > this.indices[this.lastIndexBufferIndex].IndexCount)
      {
        ++this.lastIndexBufferIndex;
        if (this.lastIndexBufferIndex == this.indices.Count)
        {
          this.indices.Add(IndexBuffer.Create<uint>((uint[]) null, 1048576, false));
          this.indicesInUse.Add(0);
        }
        this.indicesInUse[this.lastIndexBufferIndex] = 0;
        flag = true;
      }
      return flag;
    }

    private bool EnsureRingBuffers(int polygonRingVertexCount)
    {
      bool flag = false;
      if (this.lastRingVertexBufferIndex == -1 || polygonRingVertexCount + this.ringVerticesInUse[this.lastRingVertexBufferIndex] > this.ringVertices[this.lastRingVertexBufferIndex].VertexCount)
      {
        ++this.lastRingVertexBufferIndex;
        this.ringVerticesSource.Add(new List<Vector3D>());
        if (this.lastRingVertexBufferIndex == this.ringVertices.Count)
        {
          this.ringVertices.Add(VertexBuffer.Create<RegionVertex>((RegionVertex[]) null, 349525, false));
          this.ringVerticesInUse.Add(0);
        }
        this.ringVerticesInUse[this.lastRingVertexBufferIndex] = 0;
        flag = true;
      }
      return flag;
    }

    public unsafe void ReprojectVertices(bool planarCoordinates)
    {
      for (int index1 = 0; index1 < this.verticesInUse.Count; ++index1)
      {
        RegionVertex* regionVertexPtr = (RegionVertex*) this.vertices[index1].GetData().ToPointer();
        for (int index2 = 0; index2 < this.verticesInUse[index1]; ++index2)
        {
          Vector3D vertexPosition = RegionBuffer.GetVertexPosition(this.verticesSource[index1][index2], planarCoordinates);
          regionVertexPtr[index2] = new RegionVertex(vertexPosition);
        }
        this.vertices[index1].SetDirty();
      }
      for (int index1 = 0; index1 < this.ringVerticesInUse.Count; ++index1)
      {
        RegionVertex* regionVertexPtr = (RegionVertex*) this.ringVertices[index1].GetData().ToPointer();
        for (int index2 = 0; index2 < this.ringVerticesInUse[index1]; ++index2)
        {
          Vector3D vertexPosition = RegionBuffer.GetVertexPosition(this.ringVerticesSource[index1][index2], planarCoordinates);
          regionVertexPtr[index2] = new RegionVertex(vertexPosition);
        }
        this.ringVertices[index1].SetDirty();
      }
    }

    public void Clear()
    {
      this.lastVertexBufferIndex = -1;
      this.lastIndexBufferIndex = -1;
      this.lastRingVertexBufferIndex = -1;
      for (int index = 0; index < this.verticesInUse.Count; ++index)
        this.verticesInUse[index] = 0;
      for (int index = 0; index < this.indicesInUse.Count; ++index)
        this.indicesInUse[index] = 0;
      for (int index = 0; index < this.ringVerticesInUse.Count; ++index)
        this.ringVerticesInUse[index] = 0;
      this.verticesSource.Clear();
      this.ringVerticesSource.Clear();
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
      foreach (DisposableResource disposableResource in this.vertices)
        disposableResource.Dispose();
      foreach (DisposableResource disposableResource in this.ringVertices)
        disposableResource.Dispose();
      foreach (DisposableResource disposableResource in this.indices)
        disposableResource.Dispose();
    }
  }
}
