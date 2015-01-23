using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class RenderParameters : GraphicsResource
    {
        protected IRenderParameter[] allParameters;
        private bool isDirty;

        protected int SizeInBytes { get; private set; }

        internal virtual bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
            set
            {
                this.isDirty = value;
            }
        }

        protected RenderParameters(IRenderParameter[] parameters)
        {
            this.allParameters = parameters;
            foreach (IRenderParameter renderParameter in parameters)
            {
                this.SizeInBytes += renderParameter.SizeInBytes;
                if (renderParameter.Parents == null)
                {
                    renderParameter.Parents = new RenderParameters[] { this };
                }
                else
                {
                    RenderParameters[] renderParametersArray = new RenderParameters[renderParameter.Parents.Length + 1];
                    renderParameter.Parents.CopyTo(renderParametersArray, 0);
                    renderParametersArray[renderParameter.Parents.Length] = this;
                    renderParameter.Parents = renderParametersArray;
                }
            }
            this.isDirty = true;
        }

        public static RenderParameters Create(IRenderParameter[] parameters)
        {
            return new D3D11RenderParameters(parameters);
        }

        internal void SetDirty()
        {
            this.IsDirty = true;
        }
    }
}
