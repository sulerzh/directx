// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.NullMarkerVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class NullMarkerVisual : ColumnVisual
  {
    protected static double thickness = 0.4;
    protected static double halfGap = 0.18;

    internal override bool IsNullMarker
    {
      get
      {
        return true;
      }
    }

    internal override bool MayHaveNegativeInstances
    {
      get
      {
        return false;
      }
    }

    internal override float HorizontalSpacing
    {
      get
      {
        return 0.0f;
      }
    }

    public NullMarkerVisual(int walls)
      : base(walls)
    {
    }

    protected static short GetVertex(int whichComponent, int whichVertex)
    {
      return (short) (whichComponent + whichVertex);
    }

    internal override void OverrideDimensionAndScales(ref float fixedDimension, ref Vector2F fixedScale, ref Vector2F variableScale)
    {
      fixedDimension = 1f;
      fixedScale.X = FlatMarker.Width;
      fixedScale.Y = FlatMarker.Height;
      variableScale.X = variableScale.Y = 0.0f;
    }

    private static Vector3D RotateVector(Vector3D vector)
    {
      return new Vector3D(-vector.Y, vector.X, vector.Z);
    }

    private InstanceVertex RotateVertex(int i)
    {
      return new InstanceVertex(NullMarkerVisual.RotateVector(this.MeshVertices[i].Position), NullMarkerVisual.RotateVector(this.MeshVertices[i].Normal));
    }

    protected void ReplicateVertices()
    {
      int num = this.MeshVertices.Length / 4;
      for (int index1 = 1; index1 < 4; ++index1)
      {
        for (int index2 = index1 * num; index2 < (index1 + 1) * num; ++index2)
          this.MeshVertices[index2] = this.RotateVertex(index2 - num);
      }
    }

    protected void ReplicateIndices(int count, ref int currentIndex)
    {
      int num = this.MeshVertices.Length / 4;
      for (int index1 = 1; index1 < 4; ++index1)
      {
        for (int index2 = 1; index2 <= count; ++index2)
        {
          this.MeshIndices[currentIndex] = (short) ((int) this.MeshIndices[currentIndex - count] + num);
          ++currentIndex;
        }
      }
    }
  }
}
