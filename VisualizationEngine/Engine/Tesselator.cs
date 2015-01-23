// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Tesselator
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

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
      MathEx.Clamp(ref longitude, -3.14158951199714, 3.14158951199714);
      MathEx.Clamp(ref latitude, -1.57079475599857, 1.57079475599857);
      this.currentVertex = Coordinates.GeoTo3D(longitude, latitude);
      this.regionVertices.Add(this.currentVertex);
      this.rings.Add(1);
    }

    public void AddVertex(double longitude, double latitude)
    {
      MathEx.Clamp(ref longitude, -3.14158951199714, 3.14158951199714);
      MathEx.Clamp(ref latitude, -1.57079475599857, 1.57079475599857);
      this.AddVertex3D(Coordinates.GeoTo3D(longitude, latitude));
    }

    public unsafe void EndPolygon()
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "RegionsLayer.EndPolygon.");
      if (this.regionVertices.Count > int.MaxValue)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Vertex count {0} exceeds the limit.", (object) this.regionVertices.Count);
      }
      else
      {
        IntPtr num1 = IntPtr.Zero;
        IntPtr num2 = IntPtr.Zero;
        IntPtr ptr1 = IntPtr.Zero;
        IntPtr ptr2 = IntPtr.Zero;
        try
        {
          this.tesselatedRegion.Clear();
          int num3 = Marshal.SizeOf(typeof (double)) * 3;
          num1 = Marshal.AllocCoTaskMem(this.regionVertices.Count * num3);
          int index1 = 0;
          IntPtr num4 = num1;
          while (index1 < this.regionVertices.Count)
          {
            double* numPtr = (double*) (void*) num4;
            *numPtr = this.regionVertices[index1].X;
            numPtr[1] = this.regionVertices[index1].Z;
            numPtr[2] = this.regionVertices[index1].Y;
            ++index1;
            num4 += num3;
          }
          int num5 = Marshal.SizeOf(typeof (int));
          num2 = Marshal.AllocCoTaskMem(this.rings.Count * num5);
          int index2 = 0;
          IntPtr num6 = num2;
          while (index2 < this.rings.Count)
          {
            *(int*) (void*) num6 = this.rings[index2];
            ++index2;
            num6 += num5;
          }
          int num7;
          int num8;
          GeoLib.Tessellate(num1, this.regionVertices.Count * 3, num2, this.rings.Count, 0.03, &ptr1, &num7, &ptr2, &num8);
          if (num7 < 9 || num8 < 3)
            return;
          double* numPtr1 = (double*) ptr1.ToPointer();
          int* numPtr2 = (int*) ptr2.ToPointer();
          if (num8 > int.MaxValue)
            return;
          int index3 = 0;
          while (index3 < num7)
          {
            Vector3D position = new Vector3D(numPtr1[index3], numPtr1[index3 + 2], numPtr1[index3 + 1]);
            position.AssertIsUnitVector();
            this.tesselatedRegion.AddVertex(position);
            index3 += 3;
          }
          for (int index4 = 0; index4 < num8; ++index4)
            this.tesselatedRegion.AddIndex(numPtr2[index4]);
        }
        finally
        {
          Marshal.FreeCoTaskMem(num1);
          Marshal.FreeCoTaskMem(num2);
          GeoLib.DeleteArray(ptr1);
          GeoLib.DeleteArray(ptr2);
        }
        List<Vector3D> rings = new List<Vector3D>();
        for (int index = 0; index < this.regionVertices.Count; ++index)
          rings.Add(this.regionVertices[index]);
        this.tesselatedRegion.SetRingVertices(rings);
        int num9 = 0;
        for (int index = 0; index < this.rings.Count; ++index)
        {
          this.tesselatedRegion.AddRing(new Tuple<int, int>(num9, this.rings[index]));
          num9 += this.rings[index];
        }
      }
    }

    private void AddVertex3D(Vector3D vertex)
    {
      if (this.rings.Count < 1)
        return;
      if (Vector3D.DistanceSq(vertex, this.currentVertex) > 0.0009)
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
