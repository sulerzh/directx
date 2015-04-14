using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
    internal class CrossColumnVisual : ColumnVisual
    {
        internal static double WingWidth = 0.316228;
        internal static double WingLength = 0.948683;

        internal override float HorizontalSpacing
        {
            get
            {
                return (float)(2.0 * CrossColumnVisual.WingLength);
            }
        }

        public CrossColumnVisual()
            : base(12)
        {
            this.StartAngle = Math.Atan2(-3.0, -1.0) - Math.PI / 2.0;
            this.AngleIncrement = 0.0;
            this.outline = (InstancedVisual)new CrossColumnOutline();
            this.CreateMesh();
        }

        protected static Vector2D[] Create2DPositions()
        {
            Vector2D vector2D1 = new Vector2D(CrossColumnVisual.WingLength, 0.0);
            Vector2D vector2D2 = new Vector2D(0.0, CrossColumnVisual.WingLength);
            Vector2D vector2D3 = new Vector2D(CrossColumnVisual.WingWidth, 0.0);
            Vector2D vector2D4 = new Vector2D(0.0, CrossColumnVisual.WingWidth);
            return new Vector2D[12]
      {
        -vector2D2 - vector2D3,
        -vector2D2 + vector2D3,
        vector2D3 - vector2D4,
        vector2D1 - vector2D4,
        vector2D1 + vector2D4,
        vector2D3 + vector2D4,
        vector2D2 + vector2D3,
        vector2D2 - vector2D3,
        -vector2D3 + vector2D4,
        -vector2D1 + vector2D4,
        -vector2D1 - vector2D4,
        -vector2D3 - vector2D4
      };
        }

        private int CreateWallVertices(Vector3D[] positions, Vector3D xVector, Vector3D yVector, int index)
        {
            int num1 = index;
            InstanceVertex[] meshVertices1 = this.MeshVertices;
            int index1 = num1;
            int num2 = 1;
            int num3 = index1 + num2;
            meshVertices1[index1] = new InstanceVertex(positions[0], -yVector);
            InstanceVertex[] meshVertices2 = this.MeshVertices;
            int index2 = num3;
            int num4 = 1;
            int num5 = index2 + num4;
            meshVertices2[index2] = new InstanceVertex(positions[1], -yVector);
            InstanceVertex[] meshVertices3 = this.MeshVertices;
            int index3 = num5;
            int num6 = 1;
            int num7 = index3 + num6;
            meshVertices3[index3] = new InstanceVertex(positions[1], xVector);
            InstanceVertex[] meshVertices4 = this.MeshVertices;
            int index4 = num7;
            int num8 = 1;
            int num9 = index4 + num8;
            meshVertices4[index4] = new InstanceVertex(positions[2], xVector);
            InstanceVertex[] meshVertices5 = this.MeshVertices;
            int index5 = num9;
            int num10 = 1;
            int num11 = index5 + num10;
            meshVertices5[index5] = new InstanceVertex(positions[2], -yVector);
            InstanceVertex[] meshVertices6 = this.MeshVertices;
            int index6 = num11;
            int num12 = 1;
            int num13 = index6 + num12;
            meshVertices6[index6] = new InstanceVertex(positions[3], -yVector);
            InstanceVertex[] meshVertices7 = this.MeshVertices;
            int index7 = num13;
            int num14 = 1;
            int num15 = index7 + num14;
            meshVertices7[index7] = new InstanceVertex(positions[3], xVector);
            InstanceVertex[] meshVertices8 = this.MeshVertices;
            int index8 = num15;
            int num16 = 1;
            int num17 = index8 + num16;
            meshVertices8[index8] = new InstanceVertex(positions[4], xVector);
            InstanceVertex[] meshVertices9 = this.MeshVertices;
            int index9 = num17;
            int num18 = 1;
            int num19 = index9 + num18;
            meshVertices9[index9] = new InstanceVertex(positions[4], yVector);
            InstanceVertex[] meshVertices10 = this.MeshVertices;
            int index10 = num19;
            int num20 = 1;
            int num21 = index10 + num20;
            meshVertices10[index10] = new InstanceVertex(positions[5], yVector);
            InstanceVertex[] meshVertices11 = this.MeshVertices;
            int index11 = num21;
            int num22 = 1;
            int num23 = index11 + num22;
            meshVertices11[index11] = new InstanceVertex(positions[5], xVector);
            InstanceVertex[] meshVertices12 = this.MeshVertices;
            int index12 = num23;
            int num24 = 1;
            int num25 = index12 + num24;
            meshVertices12[index12] = new InstanceVertex(positions[6], xVector);
            InstanceVertex[] meshVertices13 = this.MeshVertices;
            int index13 = num25;
            int num26 = 1;
            int num27 = index13 + num26;
            meshVertices13[index13] = new InstanceVertex(positions[6], yVector);
            InstanceVertex[] meshVertices14 = this.MeshVertices;
            int index14 = num27;
            int num28 = 1;
            int num29 = index14 + num28;
            meshVertices14[index14] = new InstanceVertex(positions[7], yVector);
            InstanceVertex[] meshVertices15 = this.MeshVertices;
            int index15 = num29;
            int num30 = 1;
            int num31 = index15 + num30;
            meshVertices15[index15] = new InstanceVertex(positions[7], -xVector);
            InstanceVertex[] meshVertices16 = this.MeshVertices;
            int index16 = num31;
            int num32 = 1;
            int num33 = index16 + num32;
            meshVertices16[index16] = new InstanceVertex(positions[8], -xVector);
            InstanceVertex[] meshVertices17 = this.MeshVertices;
            int index17 = num33;
            int num34 = 1;
            int num35 = index17 + num34;
            meshVertices17[index17] = new InstanceVertex(positions[8], yVector);
            InstanceVertex[] meshVertices18 = this.MeshVertices;
            int index18 = num35;
            int num36 = 1;
            int num37 = index18 + num36;
            meshVertices18[index18] = new InstanceVertex(positions[9], yVector);
            InstanceVertex[] meshVertices19 = this.MeshVertices;
            int index19 = num37;
            int num38 = 1;
            int num39 = index19 + num38;
            meshVertices19[index19] = new InstanceVertex(positions[9], -xVector);
            InstanceVertex[] meshVertices20 = this.MeshVertices;
            int index20 = num39;
            int num40 = 1;
            int num41 = index20 + num40;
            meshVertices20[index20] = new InstanceVertex(positions[10], -xVector);
            InstanceVertex[] meshVertices21 = this.MeshVertices;
            int index21 = num41;
            int num42 = 1;
            int num43 = index21 + num42;
            meshVertices21[index21] = new InstanceVertex(positions[10], -yVector);
            InstanceVertex[] meshVertices22 = this.MeshVertices;
            int index22 = num43;
            int num44 = 1;
            int num45 = index22 + num44;
            meshVertices22[index22] = new InstanceVertex(positions[11], -yVector);
            InstanceVertex[] meshVertices23 = this.MeshVertices;
            int index23 = num45;
            int num46 = 1;
            int num47 = index23 + num46;
            meshVertices23[index23] = new InstanceVertex(positions[11], -xVector);
            InstanceVertex[] meshVertices24 = this.MeshVertices;
            int index24 = num47;
            int num48 = 1;
            int num49 = index24 + num48;
            meshVertices24[index24] = new InstanceVertex(positions[0], -xVector);
            return num49;
        }

        internal override void CreateInstanceVertices()
        {
            this.MeshVertices = new InstanceVertex[6 * this.WallCount + 1];
            int index1 = 0;
            Vector2D[] vector2DArray = CrossColumnVisual.Create2DPositions();
            Vector3D[] positions1 = new Vector3D[this.WallCount];
            for (int index2 = 0; index2 < this.WallCount; ++index2)
                positions1[index2] = InstancedVisual.To3D(vector2DArray[index2], 1.0);
            Vector3D[] positions2 = new Vector3D[this.WallCount];
            for (int index2 = 0; index2 < this.WallCount; ++index2)
                positions2[index2] = InstancedVisual.To3D(vector2DArray[index2], 0.0);
            Vector3D xVector = new Vector3D(1.0, 0.0, 0.0);
            Vector3D yVector = new Vector3D(0.0, 1.0, 0.0);
            this.CeilingVertices = index1;
            for (int index2 = 0; index2 < this.WallCount; ++index2)
                this.MeshVertices[index1++] = new InstanceVertex(positions1[index2], Vector3D.ZVector);
            this.WallTop = index1;
            int wallVertices1 = this.CreateWallVertices(positions1, xVector, yVector, index1);
            this.WallBottom = wallVertices1;
            int wallVertices2 = this.CreateWallVertices(positions2, xVector, yVector, wallVertices1);
            this.CeilingCenter = (short)wallVertices2;
            this.MeshVertices[wallVertices2] = new InstanceVertex(Vector3D.ZVector, Vector3D.ZVector, 0.0f);
            this.FloorVertices = wallVertices2;
            for (int index2 = 0; index2 < this.WallCount; ++index2)
                this.MeshVertices[wallVertices2++] = new InstanceVertex(positions2[index2], -Vector3D.ZVector);
        }

        protected override void SetTriangles()
        {
            this.MeshIndices = new short[3 * (2 * this.WallCount + 12)];
            int current1 = this.SetRawCeilingIndexs(0);
            this.FirstWallIndex = current1;
            for (int index = 0; index < 4; ++index)
            {
                int whichWall = 3 * index;
                int current2 = this.SetQuadIndexes(this.GetWallVertex(whichWall - 1, this.WallBottom, 0), this.GetWallVertex(whichWall - 1, this.WallBottom, 1), this.GetWallVertex(whichWall - 1, this.WallTop, 1), this.GetWallVertex(whichWall - 1, this.WallTop, 0), current1);
                int current3 = this.SetQuadIndexes(this.GetWallVertex(whichWall, this.WallBottom, 0), this.GetWallVertex(whichWall, this.WallBottom, 1), this.GetWallVertex(whichWall, this.WallTop, 1), this.GetWallVertex(whichWall, this.WallTop, 0), current2);
                current1 = this.SetQuadIndexes(this.GetWallVertex(whichWall + 1, this.WallBottom, 0), this.GetWallVertex(whichWall + 1, this.WallBottom, 1), this.GetWallVertex(whichWall + 1, this.WallTop, 1), this.GetWallVertex(whichWall + 1, this.WallTop, 0), current3);
            }
            this.WallIndexCount = current1 - this.FirstWallIndex;
            int current4 = this.SetQuadIndexes(this.GetFloorVertex(11), this.GetFloorVertex(8), this.GetFloorVertex(5), this.GetFloorVertex(2), current1);
            for (int index = 0; index < 4; ++index)
            {
                int i = 3 * index;
                current4 = this.SetQuadIndexes(this.GetFloorVertex(i + 2), this.GetFloorVertex(i + 1), this.GetFloorVertex(i), this.GetFloorVertex(i - 1), current4);
            }
        }
    }
}
