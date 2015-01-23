using Microsoft.Data.Visualization.DirectX.Direct3D11;
using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal static class D3DDeviceExtension
    {
        public static void NotifyError(this D3DDevice device, string message, Exception e)
        {
            ((Renderer)device.Tag).NotifyError(message, e);
        }

        public static void NotifyMessage(this D3DDevice device, string message)
        {
            ((Renderer)device.Tag).Notify(message);
        }
    }
}
