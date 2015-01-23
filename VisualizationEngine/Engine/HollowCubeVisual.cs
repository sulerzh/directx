// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HollowCubeVisual
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal class HollowCubeVisual : HollowColumnVisual
  {
    public HollowCubeVisual()
      : base(4)
    {
      this.CreateMesh();
    }

    internal override void CreateInstanceVertices()
    {
      double num1 = Constants.SqrtOf2 / 2.0;
      this.MeshVertices = new InstanceVertex[40];
      Vector2D[] vector2DArray1 = new Vector2D[4]
      {
        new Vector2D(num1, -num1),
        new Vector2D(num1, num1),
        new Vector2D(-num1, num1),
        new Vector2D(-num1, -num1)
      };
      Vector2D[] vector2DArray2 = new Vector2D[4];
      for (int index = 0; index < 4; ++index)
        vector2DArray2[index] = vector2DArray1[index] * HollowColumnVisual.innerRadius;
      Vector3D[] vector3DArray1 = new Vector3D[4];
      for (int index = 0; index < 4; ++index)
        vector3DArray1[index] = InstancedVisual.To3D(vector2DArray1[index], 1.0);
      Vector3D[] vector3DArray2 = new Vector3D[4];
      for (int index = 0; index < 4; ++index)
        vector3DArray2[index] = InstancedVisual.To3D(vector2DArray2[index], 1.0);
      Vector3D[] vector3DArray3 = new Vector3D[4];
      for (int index = 0; index < 4; ++index)
        vector3DArray3[index] = InstancedVisual.To3D(vector2DArray1[index], 0.0);
      Vector3D[] vector3DArray4 = new Vector3D[4];
      for (int index = 0; index < 4; ++index)
        vector3DArray4[index] = InstancedVisual.To3D(vector2DArray2[index], 0.0);
      Vector3D[] vector3DArray5 = new Vector3D[4]
      {
        Vector3D.XVector,
        Vector3D.YVector,
        -Vector3D.XVector,
        -Vector3D.YVector
      };
      short num2 = (short) 0;
      this.outerCeiling = (int) num2;
      for (int index = 0; index < 4; ++index)
        this.MeshVertices[(int) num2++] = new InstanceVertex(vector3DArray1[index], Vector3D.ZVector);
      this.innerCeiling = (int) num2;
      for (int index = 0; index < 4; ++index)
        this.MeshVertices[(int) num2++] = new InstanceVertex(vector3DArray2[index], Vector3D.ZVector);
      this.outerTopWall = (int) num2;
      for (int index1 = 0; index1 < 4; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = (int) num2;
        int num3 = 1;
        short num4 = (short) (index2 + num3);
        meshVertices1[index2] = new InstanceVertex(vector3DArray1[index1], vector3DArray5[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = (int) num4;
        int num5 = 1;
        num2 = (short) (index3 + num5);
        meshVertices2[index3] = new InstanceVertex(vector3DArray1[(index1 + 1) % 4], vector3DArray5[index1]);
      }
      this.outerBottomWall = (int) num2;
      for (int index1 = 0; index1 < 4; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = (int) num2;
        int num3 = 1;
        short num4 = (short) (index2 + num3);
        meshVertices1[index2] = new InstanceVertex(vector3DArray3[index1], vector3DArray5[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = (int) num4;
        int num5 = 1;
        num2 = (short) (index3 + num5);
        meshVertices2[index3] = new InstanceVertex(vector3DArray3[(index1 + 1) % 4], vector3DArray5[index1]);
      }
      this.innerTopWall = (int) num2;
      for (int index1 = 0; index1 < 4; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = (int) num2;
        int num3 = 1;
        short num4 = (short) (index2 + num3);
        meshVertices1[index2] = new InstanceVertex(vector3DArray2[index1], -vector3DArray5[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = (int) num4;
        int num5 = 1;
        num2 = (short) (index3 + num5);
        meshVertices2[index3] = new InstanceVertex(vector3DArray2[(index1 + 1) % 4], -vector3DArray5[index1]);
      }
      this.innerBottomWall = (int) num2;
      for (int index1 = 0; index1 < 4; ++index1)
      {
        InstanceVertex[] meshVertices1 = this.MeshVertices;
        int index2 = (int) num2;
        int num3 = 1;
        short num4 = (short) (index2 + num3);
        meshVertices1[index2] = new InstanceVertex(vector3DArray4[index1], -vector3DArray5[index1]);
        InstanceVertex[] meshVertices2 = this.MeshVertices;
        int index3 = (int) num4;
        int num5 = 1;
        num2 = (short) (index3 + num5);
        meshVertices2[index3] = new InstanceVertex(vector3DArray4[(index1 + 1) % 4], -vector3DArray5[index1]);
      }
    }
  }
}
