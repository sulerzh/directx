using System.IO;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class Effect : GraphicsResource
    {
        private FileSystemWatcher vertexShaderWatcher;
        private FileSystemWatcher geometryShaderWatcher;
        private FileSystemWatcher pixelShaderWatcher;
        protected EffectDebugInfo debugInfo;
        protected bool fileSystemHasUpdatedShaders;
        protected Stream vertexShaderData;
        protected Stream geometryShaderData;
        protected Stream pixelShaderData;

        public VertexFormat VertexFormat { get; private set; }

        public RenderParameters EffectParameters { get; private set; }

        public RenderParameters[] SharedEffectParameters { get; set; }

        public TextureSampler[] Samplers { get; private set; }

        public VertexFormat StreamFormat { get; set; }

        public EffectData Data { get; private set; }

        protected Effect(EffectDefinition definition)
        {
            this.EffectParameters = definition.Parameters;
            this.SharedEffectParameters = definition.SharedParameters;
            this.Samplers = definition.Samplers;
            this.VertexFormat = definition.VertexFormat;
            this.vertexShaderData = definition.VertexShaderData;
            this.geometryShaderData = definition.GeometryShaderData;
            this.pixelShaderData = definition.PixelShaderData;
            this.Data = EffectData.Create();
            this.debugInfo = definition.DebugInfo;
        }

        public static Effect Create(EffectDefinition definition)
        {
            if (definition == null)
                return null;
            if (definition.StreamFormat == null)
                return new D3D11Effect(definition);
            return new D3D11StreamEffect(definition);
        }

        private void LoadDebugShaders()
        {
            if (this.debugInfo.VertexShaderPath != null && File.Exists(this.debugInfo.VertexShaderPath))
            {
                if (this.vertexShaderData != null)
                    this.vertexShaderData.Close();
                this.vertexShaderData = File.OpenRead(this.debugInfo.VertexShaderPath);
            }
            if (this.debugInfo.GeometryShaderPath != null && File.Exists(this.debugInfo.GeometryShaderPath))
            {
                if (this.geometryShaderData != null)
                    this.geometryShaderData.Close();
                this.geometryShaderData = File.OpenRead(this.debugInfo.GeometryShaderPath);
            }
            if (this.debugInfo.PixelShaderPath == null || !File.Exists(this.debugInfo.PixelShaderPath))
                return;
            if (this.pixelShaderData != null)
                this.pixelShaderData.Close();
            this.pixelShaderData = File.OpenRead(this.debugInfo.PixelShaderPath);
        }

        private void InitFileWatchers()
        {
            if (this.debugInfo.VertexShaderPath != null)
            {
                this.vertexShaderWatcher = new FileSystemWatcher(Path.GetDirectoryName(this.debugInfo.VertexShaderPath), Path.GetFileName(this.debugInfo.VertexShaderPath));
                this.vertexShaderWatcher.Changed += new FileSystemEventHandler(this.shaderWatcher_Changed);
                this.vertexShaderWatcher.EnableRaisingEvents = true;
            }
            if (this.debugInfo.GeometryShaderPath != null)
            {
                this.geometryShaderWatcher = new FileSystemWatcher(Path.GetDirectoryName(this.debugInfo.GeometryShaderPath), Path.GetFileName(this.debugInfo.GeometryShaderPath));
                this.geometryShaderWatcher.Changed += new FileSystemEventHandler(this.shaderWatcher_Changed);
                this.geometryShaderWatcher.EnableRaisingEvents = true;
            }
            if (this.debugInfo.PixelShaderPath == null)
                return;
            this.pixelShaderWatcher = new FileSystemWatcher(Path.GetDirectoryName(this.debugInfo.PixelShaderPath), Path.GetFileName(this.debugInfo.PixelShaderPath));
            this.pixelShaderWatcher.Changed += new FileSystemEventHandler(this.shaderWatcher_Changed);
            this.pixelShaderWatcher.EnableRaisingEvents = true;
        }

        private void shaderWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.fileSystemHasUpdatedShaders = true;
        }

        protected bool ReloadDebugShadersIfNeeded()
        {
            if (!this.fileSystemHasUpdatedShaders)
                return false;
            this.fileSystemHasUpdatedShaders = false;
            this.LoadDebugShaders();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            if (this.EffectParameters != null)
                this.EffectParameters.Dispose();
            if (this.Samplers != null)
            {
                for (int i = 0; i < this.Samplers.Length; ++i)
                {
                    if (this.Samplers[i] != null)
                        this.Samplers[i].Dispose();
                }
            }
            if (this.Data != null)
                this.Data.Dispose();
            if (this.vertexShaderData != null)
                this.vertexShaderData.Dispose();
            if (this.geometryShaderData != null)
                this.geometryShaderData.Dispose();
            if (this.pixelShaderData != null)
                this.pixelShaderData.Dispose();
            if (this.vertexShaderWatcher != null)
                this.vertexShaderWatcher.Dispose();
            if (this.geometryShaderWatcher != null)
                this.geometryShaderWatcher.Dispose();
            if (this.pixelShaderWatcher == null)
                return;
            this.pixelShaderWatcher.Dispose();
        }
    }
}
