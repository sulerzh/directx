using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Data.Visualization.Engine
{
    internal class TileRenderable : DisposableResource
    {
        private TileProjection tileProjection;
        private Tile tile;
        private bool planarCoordinates;

        public Tile Owner
        {
            get
            {
                return this.tile;
            }
        }

        public Texture Texture { get; private set; }

        public VertexBuffer VertexBuffer { get; private set; }

        public Vector3D ReferencePoint
        {
            get
            {
                return this.tile.ReferencePoint;
            }
        }

        public bool PlanarCoordinates
        {
            get
            {
                return this.planarCoordinates;
            }
            set
            {
                if (value == this.planarCoordinates)
                    return;
                this.InitializeVertexBuffer(this.VertexBuffer, value ? 1.0 : 0.0);
            }
        }

        public TileRenderable(Tile owner, TileProjection projection)
        {
            this.tile = owner;
            this.tileProjection = projection;
        }

        public void InitializeVertexBuffer(VertexBuffer vb, double flatteningFactor)
        {
            this.planarCoordinates = flatteningFactor == 1.0;
            this.tile.UpdateBoundingSphereIfNeeded(flatteningFactor == 1.0 ? 1.0 : 0.0);
            this.tile.ComputeReferencePoint();
            this.tileProjection.InitializeVertexBuffer(this.tile, vb, flatteningFactor);
            this.VertexBuffer = vb;
        }

        public bool InitializeResources(Texture texture)
        {
            if (this.Texture != null || !this.tile.TextureFileExists)
                return false;
            string textureFilename = this.tile.TextureFilename;
            bool flag = true;
            using (Stream stream = File.OpenRead(textureFilename))
            {
                Image textureData = null;
                try
                {
                    textureData = new Image(stream);
                }
                catch (Exception ex)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Error loading a tile image. " + ex.ToString());
                }
                if (textureData == null || !texture.Update(textureData, true))
                    flag = false;
            }
            if (!flag)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Deleting invalid tile image: " + this.tile.TextureFilename);
                File.Delete(textureFilename);
                return false;
            }
            this.Texture = texture;
            return true;
        }

        public void Render(Renderer renderer, RenderParameterVector4F textureOffset, Texture defaultTexture, Texture overrideTexture)
        {
            if (!this.tile.ReadyToRender)
                return;
            this.SetTileTexture(renderer, textureOffset, defaultTexture, overrideTexture);
            renderer.SetVertexSource(this.VertexBuffer);
            IndexBuffer tileIndexBuffer = this.tileProjection.GetTileIndexBuffer();
            int indexCount = tileIndexBuffer.IndexCount;
            renderer.SetIndexSource(tileIndexBuffer);
            renderer.DrawIndexed(0, indexCount, PrimitiveTopology.TriangleList);
        }

        private void SetTileTexture(Renderer renderer, RenderParameterVector4F textureOffset, Texture defaultTexture, Texture overrideTexture)
        {
            float z = 1f;
            Vector2F vector2F = new Vector2F(0.0f, 0.0f);
            Texture texture = this.Texture;
            if (texture == null)
            {
                Tile parent = this.tile.Parent;
                Tile tile = this.tile;
                for (; parent != null; parent = parent.Parent)
                {
                    z *= 0.5f;
                    vector2F.X *= 0.5f;
                    vector2F.Y *= 0.5f;
                    vector2F.X += (tile.X & 1) != 0 ? 0.5f : 0.0f;
                    vector2F.Y += (tile.Y & 1) != 0 ? 0.5f : 0.0f;
                    if (parent.Renderable != null && parent.Renderable.Texture != null)
                    {
                        texture = parent.Renderable.Texture;
                        break;
                    }
                    tile = tile.Parent;
                }
            }
            if (texture == null)
            {
                z = 1f;
                vector2F = Vector2F.Empty;
                texture = defaultTexture;
            }
            if (overrideTexture != null)
            {
                z = 1f;
                vector2F = Vector2F.Empty;
                texture = overrideTexture;
            }
            textureOffset.Value = new Vector4F(vector2F.X, vector2F.Y, z, 1f);
            renderer.SetTexture(0, texture);
        }
    }
}
