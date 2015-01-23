// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Tile
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    internal class Tile : DisposableResource
    {
        private Tile[] children = new Tile[4];
        private object renderTileLock = new object();
        public const int Subdivisions = 16;
        public const int TextureWidth = 256;
        private const double LinearLengthMaxThreshold = 1024.0;
        private const double LinearLengthMinThreshold = 64.0;
        private const double AreaThreshold = 130000.0;
        private TileRenderable renderTile;
        private Vector3D[] corners;
        private string tileId;
        private ImageSet dataset;
        private TileCache tileCache;
        private double tileFlatteningFactor;
        private volatile bool textureReady;
        private volatile bool readyToRender;
        private long lastUsed;
        private volatile bool requestPending;

        public bool InViewFrustum { get; private set; }

        public SphereD BoundingSphere { get; private set; }

        public double FlatteningFactor
        {
            get
            {
                return this.tileFlatteningFactor;
            }
        }

        public int Level { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public TileProjection TileProjection { get; private set; }

        public Vector3D ReferencePoint { get; private set; }

        public Tile Parent { get; private set; }

        public TileRenderable Renderable
        {
            get
            {
                return this.renderTile;
            }
        }

        public bool TextureReady
        {
            get
            {
                return this.textureReady;
            }
        }

        public bool ReadyToRender
        {
            get
            {
                return this.readyToRender;
            }
        }

        public long LastUsed
        {
            get
            {
                return this.lastUsed;
            }
        }

        public bool RequestPending
        {
            get
            {
                return this.requestPending;
            }
        }

        public bool TextureFileExists
        {
            get
            {
                if (!this.TextureReady)
                    return File.Exists(this.TextureFilename);
                else
                    return true;
            }
        }

        public ImageSet ImageSet
        {
            get
            {
                return this.dataset;
            }
        }

        public TileExtent Extent
        {
            get
            {
                TileExtent extent = this.TileProjection.GetExtent(this);
                double lat1 = Math.Min(17.0 * Math.PI / 36.0, extent.TopLeft.Latitude);
                double lat2 = Math.Max(-17.0 * Math.PI / 36.0, extent.BottomRight.Latitude);
                return new TileExtent(new Coordinates(extent.TopLeft.Longitude, lat1), new Coordinates(extent.BottomRight.Longitude, lat2));
            }
        }

        public bool IsNorthmost
        {
            get
            {
                return this.Y == 0;
            }
        }

        public bool IsSouthmost
        {
            get
            {
                return this.Y == (2 << this.Level - 1) - 1;
            }
        }

        public string Key
        {
            get
            {
                return ImageSet.GetTileKey(this.dataset, this.Level, this.X, this.Y);
            }
        }

        public int ServerID
        {
            get
            {
                return (this.X & 1) + ((this.Y & 1) << 1);
            }
        }

        public static string ImagingBaseDirectory
        {
            get
            {
                return string.Format("{0}Imagery", (object)GlobeStep.DefaultCacheDir);
            }
        }

        public string ImageSetDirectory
        {
            get
            {
                return string.Format("{0}\\{1}", (object)Tile.ImagingBaseDirectory, (object)this.dataset.ImageSetID.ToString((IFormatProvider)CultureInfo.InvariantCulture));
            }
        }

        public string TextureDirectory
        {
            get
            {
                return this.ImageSetDirectory + "\\" + this.Level.ToString((IFormatProvider)CultureInfo.InvariantCulture) + "\\" + this.Y.ToString((IFormatProvider)CultureInfo.InvariantCulture);
            }
        }

        public string TextureFilename
        {
            get
            {
                return this.TextureDirectory + "\\" + this.Y.ToString((IFormatProvider)CultureInfo.InvariantCulture) + "_" + this.X.ToString((IFormatProvider)CultureInfo.InvariantCulture) + (this.dataset.Extension.StartsWith(".", StringComparison.OrdinalIgnoreCase) ? this.dataset.Extension : "." + this.dataset.Extension);
            }
        }

        public string TextureURL
        {
            get
            {
                return string.Format(this.dataset.Url, (object)this.ServerID, (object)this.GetTileID());
            }
        }

        public Tile(int level, int x, int y, ImageSet dataset, Tile parent, TileProjection projection, TileCache cache)
        {
            this.TileProjection = projection;
            this.Parent = parent;
            this.Level = level;
            this.X = x;
            this.Y = y;
            this.dataset = dataset;
            this.tileCache = cache;
            this.BoundingSphere = this.TileProjection.ComputeBoundingSphere(this, out this.corners);
            this.ComputeReferencePoint();
        }

        public void SetParent(Tile parent)
        {
            this.Parent = parent;
        }

        public void SetTextureReady()
        {
            this.textureReady = true;
        }

        public void SetRequestPending(bool pending)
        {
            this.requestPending = pending;
        }

        public bool Resolve(Dictionary<string, Tile> tileSet, SceneState state, out bool hasIncompleteData)
    {
      bool flag = false;
      Interlocked.Exchange(ref this.lastUsed, state.ElapsedFrames);
      hasIncompleteData = false;
      if (this.NeedsSubdivision(state))
      {
        for (int index = 0; index < 4; ++index)
        {
          if (this.children[index] == null)
          {
            int num1 = (index & 2) >> 1;
            int num2 = index & 1;
            this.children[index] = this.tileCache.GetTile(this.Level + 1, this.X * 2 + num2, this.Y * 2 + (num1 + 1) % 2, this.dataset, this);
          }
          Interlocked.Exchange(ref this.children[index].lastUsed, this.lastUsed);
          if (!this.children[index].ReadyToRender)
          {
            this.children[index].IsTileInFrustum(state.GetViewFrustum(), state.FlatteningFactor);
            this.tileCache.AddTileToQueue(this.children[index]);
            flag = true;
            hasIncompleteData = true;
          }
        }
        if (!flag)
        {
          bool hasIncompleteData1 = false;
          for (int index = 0; index < 4; ++index)
          {
            if (this.children[index].IsTileInFrustum(state.GetViewFrustum(), state.FlatteningFactor))
              this.children[index].Resolve(tileSet, state, out hasIncompleteData1);

            hasIncompleteData |= hasIncompleteData1;

          }
        }
      }
      else
        flag = true;
      if (flag)
      {
        if (!this.ReadyToRender)
        {
          this.tileCache.AddTileToQueue(this);
          hasIncompleteData = true;
          return false;
        }
        else
        {
          if (!this.TextureReady)
          {
            this.tileCache.AddTileToQueue(this);
            hasIncompleteData = true;
          }
          tileSet.Add(this.Key, this);
        }
      }
      return true;
    }

        internal void ComputeReferencePoint()
        {
            this.ReferencePoint = this.Level < 12 ? Vector3D.Empty : this.BoundingSphere.Origin;
        }

        public bool NeedsSubdivision(SceneState state)
        {
            if (this.Level <= 2)
                return true;
            Matrix4x4D viewProjection = state.ViewProjection;
            this.UpdateBoundingSphereIfNeeded(state.FlatteningFactor);
            Vector2D vector2D1 = viewProjection.Transform(this.corners[0]).XY();
            Vector2D vector2D2 = viewProjection.Transform(this.corners[1]).XY();
            Vector2D vector2D3 = viewProjection.Transform(this.corners[2]).XY();
            Vector2D vector2D4 = viewProjection.Transform(this.corners[3]).XY();
            Vector2D vector2D5 = viewProjection.Transform((this.corners[1] + this.corners[0]) / 2.0).XY();
            Vector2D vector2D6 = viewProjection.Transform((this.corners[3] + this.corners[2]) / 2.0).XY();
            Vector2D vector2D7 = viewProjection.Transform((this.corners[2] + this.corners[0]) / 2.0).XY();
            Vector2D vector2D8 = viewProjection.Transform((this.corners[3] + this.corners[1]) / 2.0).XY();
            Vector2D vector2D9 = new Vector2D(state.ScreenWidth / 2.0, state.ScreenHeight / 2.0);
            vector2D1.MultiplyBy(vector2D9);
            vector2D2.MultiplyBy(vector2D9);
            vector2D3.MultiplyBy(vector2D9);
            vector2D4.MultiplyBy(vector2D9);
            vector2D5.MultiplyBy(vector2D9);
            vector2D6.MultiplyBy(vector2D9);
            vector2D7.MultiplyBy(vector2D9);
            vector2D8.MultiplyBy(vector2D9);
            double val2_1 = (vector2D1 - vector2D5).Length() + (vector2D2 - vector2D5).Length();
            double val2_2 = (vector2D3 - vector2D6).Length() + (vector2D4 - vector2D6).Length();
            double val1 = (vector2D3 - vector2D7).Length() + (vector2D1 - vector2D7).Length();
            double val2_3 = (vector2D4 - vector2D8).Length() + (vector2D2 - vector2D8).Length();
            double num1 = Math.Max(Math.Max(Math.Max(val1, val2_3), val2_2), val2_1);
            double num2 = Math.Min(Math.Min(Math.Min(val1, val2_3), val2_2), val2_1);
            return num1 > 1024.0 && num2 > 64.0 || num2 >= 64.0 && Math.Abs(Vector2D.Cross(vector2D1 - vector2D7, vector2D5 - vector2D7)) + Math.Abs(Vector2D.Cross(vector2D2 - vector2D8, vector2D5 - vector2D8)) + Math.Abs(Vector2D.Cross(vector2D3 - vector2D7, vector2D6 - vector2D7)) + Math.Abs(Vector2D.Cross(vector2D4 - vector2D8, vector2D6 - vector2D8)) >= 130000.0;
        }

        public bool IsTileInFrustum(PlaneD[] frustum, double flatteningFactor)
        {
            this.InViewFrustum = false;
            this.UpdateBoundingSphereIfNeeded(flatteningFactor);
            for (int index = 0; index < 6; ++index)
            {
                if (frustum[index].DistanceTo(this.BoundingSphere.Origin) < -this.BoundingSphere.Radius)
                    return false;
            }
            this.InViewFrustum = true;
            return true;
        }

        public void PrepareForRender()
        {
            lock (this.renderTileLock)
            {
                if (this.renderTile != null)
                    return;
                this.renderTile = new TileRenderable(this, this.TileProjection);
                this.renderTile.InitializeVertexBuffer(this.tileCache.GetVertexBufferFromPool(), this.FlatteningFactor < 1.0 ? 0.0 : 1.0);
                this.readyToRender = true;
            }
        }

        public bool InitializeResources(Renderer renderer)
        {
            this.PrepareForRender();
            Texture textureFromPool = this.tileCache.GetTextureFromPool(renderer);
            bool flag = false;
            try
            {
                flag = this.renderTile.InitializeResources(textureFromPool);
                return flag;
            }
            finally
            {
                if (!flag)
                    this.tileCache.ReturnTextureToPool(textureFromPool);
            }
        }

        internal void UpdateBoundingSphereIfNeeded(double flatteningFactor)
        {
            if (flatteningFactor == this.tileFlatteningFactor)
                return;
            this.tileFlatteningFactor = flatteningFactor;
            this.BoundingSphere = this.TileProjection.ComputeBoundingSphere(this, out this.corners);
        }

        private string GetTileID()
        {
            if (this.tileId != null)
                return this.tileId;
            int level = this.Level;
            int x = this.X;
            int y = this.Y;
            string quadTreeTileMap = this.dataset.QuadTreeTileMap;
            if (!string.IsNullOrEmpty(quadTreeTileMap))
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int index1 = level; index1 > 0; --index1)
                {
                    int num = 1 << index1 - 1;
                    int index2 = 0;
                    if ((x & num) != 0)
                        index2 = 1;
                    if ((y & num) != 0)
                        index2 += 2;
                    stringBuilder.Append(quadTreeTileMap[index2]);
                }
                this.tileId = ((object)stringBuilder).ToString();
                return this.tileId;
            }
            else
            {
                this.tileId = "0";
                return this.tileId;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            this.ReleaseResources();
        }

        private void ReleaseResources()
        {
            this.readyToRender = false;
            this.textureReady = false;
            this.children = (Tile[])null;
            if (this.Parent != null && !this.Parent.Disposed)
                this.Parent.children[(this.X & 1) + ((~this.Y & 1) << 1)] = (Tile)null;
            lock (this.renderTileLock)
            {
                if (this.renderTile == null)
                    return;
                if (this.renderTile.VertexBuffer != null)
                    this.tileCache.ReturnVertexBufferToPool(this.renderTile.VertexBuffer);
                if (this.renderTile.Texture != null)
                    this.tileCache.ReturnTextureToPool(this.renderTile.Texture);
                this.renderTile.Dispose();
                this.renderTile = (TileRenderable)null;
            }
        }
    }
}
