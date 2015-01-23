// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionWallFacet
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal class RegionWallFacet
  {
    private static double SquaredZeroAngleCosine = 0.9999;
    private Vector3D bottomStartPoint;
    private Vector3D bottomEndPoint;
    private Vector3D topStartPoint;
    private Vector3D topEndPoint;
    private Vector3D normal;
    private bool isDegenerate;
    private int bottomStartIndex;
    private int bottomEndIndex;
    private int topStartIndex;
    private int topEndIndex;

    public RegionWallFacet(Vector3D start, Vector3D end, double radius)
    {
      this.bottomStartPoint = start;
      this.bottomEndPoint = end;
      this.normal = Vector3D.Cross(end - start, start);
      this.isDegenerate = !this.normal.Normalize();
      this.topStartPoint = start * radius;
      this.topEndPoint = end * radius;
      this.bottomStartIndex = this.bottomEndIndex = this.topStartIndex = this.topEndIndex = -1;
    }

    public RegionWallFacet(RegionWallFacet previous, Vector3D end, double radius)
    {
      this.bottomStartPoint = previous.bottomEndPoint;
      this.topStartPoint = previous.topEndPoint;
      this.bottomEndPoint = end;
      this.topEndPoint = end * radius;
      this.normal = Vector3D.Cross(this.bottomEndPoint - this.bottomStartPoint, this.bottomStartPoint);
      this.isDegenerate = !this.normal.Normalize();
      this.bottomStartIndex = this.bottomEndIndex = this.topStartIndex = this.topEndIndex = -1;
    }

    public void AddTo(RegionTriangleList triangles)
    {
      if (this.isDegenerate)
        return;
      triangles.AddTriangle(this.bottomStartIndex, this.topEndIndex, this.bottomEndIndex);
      triangles.AddTriangle(this.bottomStartIndex, this.topStartIndex, this.topEndIndex);
    }

    public void ProcessCommonEdge(RegionWallFacet other, RegionTriangleList triangles)
    {
      if (this.normal * other.normal >= RegionWallFacet.SquaredZeroAngleCosine)
      {
        this.normal.AssertIsUnitVector();
        other.normal.AssertIsUnitVector();
        this.bottomEndIndex = other.bottomStartIndex = triangles.AddVertex(this.bottomEndPoint);
        this.topEndIndex = other.topStartIndex = triangles.AddVertex(this.topEndPoint);
      }
      else
      {
        if (!this.isDegenerate)
        {
          this.bottomEndIndex = triangles.AddVertex(this.bottomEndPoint);
          this.topEndIndex = triangles.AddVertex(this.topEndPoint);
        }
        if (other.isDegenerate)
          return;
        other.bottomStartIndex = triangles.AddVertex(other.bottomStartPoint);
        other.topStartIndex = triangles.AddVertex(other.topStartPoint);
      }
    }
  }
}
