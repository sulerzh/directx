using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public interface IRenderParameter
    {
        int SizeInBytes { get; }

        string Name { get; }

        RenderParameters[] Parents { get; set; }

        void CopyDataToBlob(IntPtr blob);
    }
}
