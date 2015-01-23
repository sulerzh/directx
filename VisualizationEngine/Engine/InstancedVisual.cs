// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstancedVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class InstancedVisual : DisposableResource
  {
    protected InstancedVisual outline;
    protected InstancedVisual shadowVolume;
    protected short[] MeshIndicesWithAdjacency;

    protected InstanceVertex[] MeshVertices { get; set; }

    protected short[] MeshIndices { get; set; }

    public VertexBuffer MeshVertexBuffer { get; private set; }

    public VertexBuffer OutlineMeshVertexBuffer
    {
      get
      {
        return this.outline.MeshVertexBuffer;
      }
    }

    public VertexBuffer ShadowMeshVertexBuffer
    {
      get
      {
        return this.shadowVolume.MeshVertexBuffer;
      }
    }

    public IndexBuffer MeshIndexBuffer { get; private set; }

    public IndexBuffer OutlineMeshIndexBuffer
    {
      get
      {
        return this.outline.MeshIndexBufferWithAdjacency;
      }
    }

    public IndexBuffer ShadowMeshIndexBuffer
    {
      get
      {
        return this.shadowVolume.MeshIndexBuffer;
      }
    }

    private IndexBuffer MeshIndexBufferWithAdjacency { get; set; }

    protected double StartAngle { get; set; }

    protected double AngleIncrement { get; set; }

    protected int WallCount { get; set; }

    internal int FirstWallIndex { get; set; }

    internal int WallIndexCount { get; set; }

    internal int FirstCeilingIndex { get; set; }

    internal int CeilingIndexCount { get; set; }

    internal virtual bool IsPieSegment
    {
      get
      {
        return false;
      }
    }

    internal virtual bool IsNullMarker
    {
      get
      {
        return false;
      }
    }

    internal virtual bool IsZeroMarker
    {
      get
      {
        return false;
      }
    }

    internal virtual bool MayHaveNegativeInstances
    {
      get
      {
        return true;
      }
    }

    internal virtual float HorizontalSpacing
    {
      get
      {
        return (float) (2.0 * Math.Cos(this.StartAngle + Math.Round(-this.StartAngle / this.AngleIncrement) * this.AngleIncrement));
      }
    }

    protected InstancedVisual(int walls)
    {
      this.WallCount = walls;
      this.outline = this;
      this.shadowVolume = this;
    }

    protected void CreateMesh()
    {
      this.CreateInstanceVertices();
      this.SetTriangles();
      this.CreateBuffers();
    }

    private void CreateBuffers()
    {
      OffsetVertex[] data = new OffsetVertex[this.MeshVertices.Length];
      for (int index = 0; index < this.MeshVertices.Length; ++index)
      {
        data[index].X = (short) (this.MeshVertices[index].Position.X * (double) short.MaxValue);
        data[index].Y = (short) (this.MeshVertices[index].Position.Y * (double) short.MaxValue);
        data[index].Z = (short) (this.MeshVertices[index].Position.Z * (double) short.MaxValue);
        data[index].R = (short) ((double) this.MeshVertices[index].Texture * (double) short.MaxValue);
        data[index].NX = (short) (this.MeshVertices[index].Normal.X * (double) short.MaxValue);
        data[index].NY = (short) (this.MeshVertices[index].Normal.Y * (double) short.MaxValue);
        data[index].NZ = (short) (this.MeshVertices[index].Normal.Z * (double) short.MaxValue);
        data[index].Unused = (short) 0;
      }
      this.MeshVertexBuffer = VertexBuffer.Create<OffsetVertex>(data, false);
      this.MeshIndexBuffer = IndexBuffer.Create<short>(this.MeshIndices, false);
      if (this.MeshIndicesWithAdjacency == null)
        return;
      this.MeshIndexBufferWithAdjacency = IndexBuffer.Create<short>(this.MeshIndicesWithAdjacency, true);
    }

    internal static InstancedVisual Create(InstancedShape shape)
    {
      switch (shape)
      {
        case InstancedShape.InvertedPyramid:
          return (InstancedVisual) new PolygonalCone(4, false);
        case InstancedShape.CircularCone:
          return (InstancedVisual) new CircularCone();
        case InstancedShape.Triangle:
          return (InstancedVisual) new PolygonalColumnVisual(3, false);
        case InstancedShape.Square:
          return (InstancedVisual) new PolygonalColumnVisual(4, false);
        case InstancedShape.Pentagon:
          return (InstancedVisual) new PolygonalColumnVisual(5, false);
        case InstancedShape.Hexagon:
          return (InstancedVisual) new PolygonalColumnVisual(6, false);
        case InstancedShape.Heptagon:
          return (InstancedVisual) new PolygonalColumnVisual(7, false);
        case InstancedShape.Octagon:
          return (InstancedVisual) new PolygonalColumnVisual(8, false);
        case InstancedShape.Circle:
          return (InstancedVisual) new CylinderVisual();
        case InstancedShape.Star4:
          return (InstancedVisual) new StarColumnVisual(4);
        case InstancedShape.Star5:
          return (InstancedVisual) new StarColumnVisual(5);
        case InstancedShape.Star8:
          return (InstancedVisual) new StarColumnVisual(8);
        case InstancedShape.Star12:
          return (InstancedVisual) new StarColumnVisual(12);
        case InstancedShape.Star16:
          return (InstancedVisual) new StarColumnVisual(16);
        case InstancedShape.Star24:
          return (InstancedVisual) new StarColumnVisual(24);
        case InstancedShape.Cross:
          return (InstancedVisual) new CrossColumnVisual();
        case InstancedShape.SquareNullMarker:
          return (InstancedVisual) new SquareNullMarkerVisual();
        case InstancedShape.RoundNullMarker:
          return (InstancedVisual) new RoundNullMarkerVisual();
        default:
          return (InstancedVisual) null;
      }
    }

    protected static Vector2D[] Create2DRegularPolygon(double startAngle, int count)
    {
      Vector2D[] vector2DArray = new Vector2D[count];
      double num1 = 2.0 * Math.PI / (double) count;
      double num2 = Math.Cos(num1);
      double num3 = Math.Sin(num1);
      vector2DArray[0].X = Math.Cos(startAngle);
      vector2DArray[0].Y = Math.Sin(startAngle);
      for (int index = 1; index < count; ++index)
      {
        vector2DArray[index].X = vector2DArray[index - 1].X * num2 - vector2DArray[index - 1].Y * num3;
        vector2DArray[index].Y = vector2DArray[index - 1].X * num3 + vector2DArray[index - 1].Y * num2;
      }
      return vector2DArray;
    }

    protected static Vector3D To3D(Vector2D vector, double thirdCoordinate)
    {
      return new Vector3D(vector.X, vector.Y, thirdCoordinate);
    }

    protected int Mod(int index)
    {
      return (index + this.WallCount) % this.WallCount;
    }

    protected virtual void SetTriangleIndexes(short vertex1, short vertex2, short vertex3, short adjacent1, short adjacent2, short adjacent3, ref int raw, ref int withAdjacency)
    {
      this.MeshIndices[raw++] = this.MeshIndicesWithAdjacency[withAdjacency++] = vertex1;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent1;
      this.MeshIndices[raw++] = this.MeshIndicesWithAdjacency[withAdjacency++] = vertex2;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent2;
      this.MeshIndices[raw++] = this.MeshIndicesWithAdjacency[withAdjacency++] = vertex3;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent3;
    }

    protected int SetRawTriangleIndexes(short vertex1, short vertex2, short vertex3, int current)
    {
      int num1 = current;
      short[] meshIndices1 = this.MeshIndices;
      int index1 = num1;
      int num2 = 1;
      int num3 = index1 + num2;
      int num4 = (int) vertex1;
      meshIndices1[index1] = (short) num4;
      short[] meshIndices2 = this.MeshIndices;
      int index2 = num3;
      int num5 = 1;
      int num6 = index2 + num5;
      int num7 = (int) vertex2;
      meshIndices2[index2] = (short) num7;
      short[] meshIndices3 = this.MeshIndices;
      int index3 = num6;
      int num8 = 1;
      int num9 = index3 + num8;
      int num10 = (int) vertex3;
      meshIndices3[index3] = (short) num10;
      return num9;
    }

    protected int SetQuadIndexes(short vertex1, short vertex2, short vertex3, short vertex4, int current)
    {
      current = this.SetRawTriangleIndexes(vertex1, vertex2, vertex3, current);
      return this.SetRawTriangleIndexes(vertex1, vertex3, vertex4, current);
    }

    internal virtual void OverrideDimensionAndScales(ref float fixedDimension, ref Vector2F fixedScale, ref Vector2F variableScale)
    {
    }

    internal abstract void CreateInstanceVertices();

    protected abstract void SetTriangles();

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      if (this.MeshVertexBuffer != null)
        this.MeshVertexBuffer.Dispose();
      if (this.MeshIndexBuffer != null)
        this.MeshIndexBuffer.Dispose();
      if (this.MeshIndexBufferWithAdjacency != null)
        this.MeshIndexBufferWithAdjacency.Dispose();
      if (this.outline != null && this.outline != this && !this.outline.Disposed)
        this.outline.Dispose();
      if (this.shadowVolume == null || this.shadowVolume == this || this.shadowVolume.Disposed)
        return;
      this.shadowVolume.Dispose();
    }

    internal enum Alignment
    {
      None,
      Horizontal,
      Vertical,
      Angular,
    }
  }
}
