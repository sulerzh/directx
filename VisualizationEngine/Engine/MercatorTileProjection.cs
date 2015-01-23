// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.MercatorTileProjection
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class MercatorTileProjection : TileProjection
  {
    private const double BaseMetersPerPixel = 156543.0;
    private const double OffsetMeters = 20037508.0;
    private IndexBuffer tileIndices;

    public MercatorTileProjection()
    {
      this.BuildIndexBuffers();
    }

    private void BuildIndexBuffers()
    {
      short[] data = new short[1536];
      bool flag = false;
      int num1 = 8;
      int num2 = 0;
      for (int index1 = 0; index1 < 2; ++index1)
      {
        for (int index2 = 0; index2 < 2; ++index2)
        {
          short[] numArray = new short[384];
          int index3 = 0;
          for (int index4 = num1 * index1; index4 < num1 * (index1 + 1); ++index4)
          {
            for (int index5 = num1 * index2; index5 < num1 * (index2 + 1); ++index5)
            {
              if (flag)
              {
                numArray[index3] = (short) (index4 * 17 + index5);
                numArray[index3 + 2] = (short) ((index4 + 1) * 17 + index5);
                numArray[index3 + 1] = (short) (index4 * 17 + (index5 + 1));
                numArray[index3 + 3] = (short) (index4 * 17 + (index5 + 1));
                numArray[index3 + 5] = (short) ((index4 + 1) * 17 + index5);
                numArray[index3 + 4] = (short) ((index4 + 1) * 17 + (index5 + 1));
                index3 += 6;
              }
              else
              {
                numArray[index3] = (short) (index4 * 17 + index5);
                numArray[index3 + 2] = (short) ((index4 + 1) * 17 + (index5 + 1));
                numArray[index3 + 1] = (short) (index4 * 17 + (index5 + 1));
                numArray[index3 + 3] = (short) (index4 * 17 + index5);
                numArray[index3 + 5] = (short) ((index4 + 1) * 17 + index5);
                numArray[index3 + 4] = (short) ((index4 + 1) * 17 + (index5 + 1));
                index3 += 6;
              }
              flag = !flag;
            }
            flag = !flag;
          }
          Array.Copy((Array) numArray, 0, (Array) data, num2++ * numArray.Length, numArray.Length);
        }
      }
      this.tileIndices = IndexBuffer.Create<short>(data, false);
    }

    public override IndexBuffer GetTileIndexBuffer()
    {
      return this.tileIndices;
    }

    public override TileExtent GetExtent(Tile tile)
    {
      double num = 2.0 * Math.PI / Math.Pow(2.0, (double) tile.Level);
      double lat1 = tile.IsNorthmost ? Math.PI / 2.0 : MercatorTileProjection.AbsoluteMetersToLatAtZoom(tile.Y * 256, tile.Level);
      double lat2 = tile.IsSouthmost ? -1.0 * Math.PI / 2.0 : MercatorTileProjection.AbsoluteMetersToLatAtZoom((tile.Y + 1) * 256, tile.Level);
      double lon1 = (double) tile.X * num - Math.PI;
      double lon2 = (double) (tile.X + 1) * num - Math.PI;
      return new TileExtent(new Coordinates(lon1, lat1), new Coordinates(lon2, lat2));
    }

    public override SphereD ComputeBoundingSphere(Tile tile, out Vector3D[] corners)
    {
      TileExtent extent = this.GetExtent(tile);
      double lat = MercatorTileProjection.AbsoluteMetersToLatAtZoom((tile.Y * 2 + 1) * 256, tile.Level + 1);
      Vector3D vector3D1 = Coordinates.GeoTo3DFlattening((extent.TopLeft.Longitude + extent.BottomRight.Longitude) / 2.0, lat, tile.FlatteningFactor);
      Vector3D vector3D2 = Coordinates.GeoTo3DFlattening(extent.TopLeft.Longitude, extent.TopLeft.Latitude, tile.FlatteningFactor);
      Vector3D vector3D3 = Coordinates.GeoTo3DFlattening(extent.BottomRight.Longitude, extent.BottomRight.Latitude, tile.FlatteningFactor);
      Vector3D vector3D4 = Coordinates.GeoTo3DFlattening(extent.BottomRight.Longitude, extent.TopLeft.Latitude, tile.FlatteningFactor);
      Vector3D vector3D5 = Coordinates.GeoTo3DFlattening(extent.TopLeft.Longitude, extent.BottomRight.Latitude, tile.FlatteningFactor);
      Vector3D vector3D6 = vector3D2;
      vector3D6 = vector3D6.Subtract(vector3D1);
      Vector3D vector3D7 = vector3D4;
      vector3D7 = vector3D7.Subtract(vector3D1);
      Vector3D vector3D8 = vector3D5;
      vector3D8 = vector3D8.Subtract(vector3D1);
      Vector3D vector3D9 = vector3D3;
      vector3D9 = vector3D9.Subtract(vector3D1);
      double radius = Math.Max(Math.Max(vector3D6.Length(), vector3D7.Length()), Math.Max(vector3D8.Length(), vector3D9.Length()));
      corners = new Vector3D[4]
      {
        vector3D2,
        vector3D4,
        vector3D5,
        vector3D3
      };
      return new SphereD(vector3D1, radius);
    }

    public override unsafe void InitializeVertexBuffer(Tile tile, VertexBuffer vertexBuffer, double flatteningFactor)
    {
      double num1 = 2.0 * Math.PI / Math.Pow(2.0, (double) tile.Level);
      double num2 = MercatorTileProjection.AbsoluteMetersToLatAtZoom(tile.Y * 256, tile.Level);
      double num3 = MercatorTileProjection.AbsoluteMetersToLatAtZoom((tile.Y + 1) * 256, tile.Level);
      double num4 = (double) tile.X * num1 - Math.PI;
      int x = tile.X;
      double num5 = MercatorTileProjection.AbsoluteMetersToLatAtZoom((tile.Y * 2 + 1) * 256, tile.Level + 1);
      TileVertex* tileVertexPtr = (TileVertex*) (void*) vertexBuffer.GetData();
      double num6 = 1.0 / 16.0;
      for (int index1 = 0; index1 < 2; ++index1)
      {
        double num7 = index1 == 0 ? num3 - num5 : num2 - num5;
        int num8 = index1 == 0 ? 0 : 8;
        int num9 = index1 == 0 ? 8 : 16;
        for (int index2 = num8; index2 <= num9; ++index2)
        {
          double num10 = index1 == 0 ? num3 - 2.0 * num6 * num7 * (double) index2 : num5 + 2.0 * num6 * num7 * (double) (index2 - 8);
          for (int index3 = 0; index3 <= 16; ++index3)
          {
            double lon = num4 + num6 * num1 * (double) index3;
            int index4 = index2 * 17 + index3;
            Vector3D vector3D = Coordinates.GeoTo3DFlattening(lon, num10, flatteningFactor) - tile.ReferencePoint;
            tileVertexPtr[index4].Position = (Vector3F) vector3D;
            tileVertexPtr[index4].Tu = (float) index3 * (float) num6;
            tileVertexPtr[index4].Tv = (float) ((MercatorTileProjection.AbsoluteLatToMetersAtZoom(num10, tile.Level) - (double) (tile.Y * 256)) / 256.0);
          }
        }
        if (flatteningFactor < 1.0 && (tile.IsNorthmost || tile.IsSouthmost))
        {
          for (int index2 = 0; index2 <= 16; ++index2)
          {
            int index3 = (tile.IsNorthmost ? 16 : 0) * 17 + index2;
            tileVertexPtr[index3].Position = (Vector3F) (new Vector3D(0.0, tile.IsNorthmost ? 1.0 : -1.0, 0.0) - tile.ReferencePoint);
          }
        }
      }
      vertexBuffer.SetDirty();
    }

    private static double AbsoluteLatToMetersAtZoom(double latitude, int zoom)
    {
      double num = Math.Sin(latitude);
      return (20037508.0 - 3189068.5 * Math.Log((1.0 + num) / (1.0 - num))) / MercatorTileProjection.MetersPerPixel2(zoom);
    }

    private static double AbsoluteMetersToLatAtZoom(int y, int zoom)
    {
      double num = MercatorTileProjection.MetersPerPixel2(zoom);
      return Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp(0.0 - (20037508.0 - (double) y * num) / 6378137.0));
    }

    private static double MetersPerPixel2(int zoom)
    {
      return 156543.0 / (double) (1 << zoom);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing || this.tileIndices == null)
        return;
      this.tileIndices.Dispose();
    }
  }
}
