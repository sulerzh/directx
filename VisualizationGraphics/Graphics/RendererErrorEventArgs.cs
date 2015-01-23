using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RendererInfoEventArgs : EventArgs
    {
        public string Message { get; private set; }

        internal RendererInfoEventArgs(string msg)
        {
            this.Message = msg;
        }
    }

    public class RendererErrorEventArgs : RendererInfoEventArgs
    {
        public Exception Exception { get; private set; }

        internal RendererErrorEventArgs(string msg, Exception e)
            : base(msg)
        {
            this.Exception = e;
        }
    }

    public class RendererPresentEventArgs : EventArgs
    {
        public RenderTarget Backbuffer { get; private set; }

        internal RendererPresentEventArgs(RenderTarget backbuffer)
        {
            this.Backbuffer = backbuffer;
        }
    }

    public delegate void RendererPresentEventHandler(Renderer sender, RendererPresentEventArgs args);
    public delegate void RendererInfoEventHander(Renderer sender, RendererInfoEventArgs args);
    public delegate void RendererErrorEventHandler(Renderer sender, RendererErrorEventArgs args);
}
