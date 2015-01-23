// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionTriangleList
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class RegionTriangleList
  {
    private List<Vector3D> vertices = new List<Vector3D>();
    private List<int> indices = new List<int>();
    private List<Vector3D> ringVertices = new List<Vector3D>();
    private List<Tuple<int, int>> ringSubsets = new List<Tuple<int, int>>();

    public IList<int> GetIndices()
    {
      return (IList<int>) this.indices.AsReadOnly();
    }

    public IList<Vector3D> GetVertices()
    {
      return (IList<Vector3D>) this.vertices.AsReadOnly();
    }

    public IList<Tuple<int, int>> GetRingSubsets()
    {
      return (IList<Tuple<int, int>>) this.ringSubsets.AsReadOnly();
    }

    public IList<Vector3D> GetRingVertices()
    {
      return (IList<Vector3D>) this.ringVertices.AsReadOnly();
    }

    public void AddIndex(int index)
    {
      this.indices.Add(index);
    }

    public int AddVertex(Vector3D position)
    {
      int count = this.vertices.Count;
      this.vertices.Add(position);
      return count;
    }

    public void AddTriangle(int i, int j, int k)
    {
      this.indices.Add(i);
      this.indices.Add(j);
      this.indices.Add(k);
    }

    public void SetRingVertices(List<Vector3D> rings)
    {
      this.ringVertices = rings;
    }

    public void AddRing(Tuple<int, int> subset)
    {
      this.ringSubsets.Add(subset);
    }

    public void Clear()
    {
      this.vertices.Clear();
      this.indices.Clear();
      this.ringSubsets.Clear();
      this.ringVertices = (List<Vector3D>) null;
    }
  }
}
