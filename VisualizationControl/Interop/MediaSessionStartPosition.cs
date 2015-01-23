using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [StructLayout(LayoutKind.Explicit)]
  internal class MediaSessionStartPosition
  {
    [FieldOffset(0)]
    public short internalUse;
    [FieldOffset(8)]
    public long startPosition;

    public MediaSessionStartPosition(long startPosition)
    {
      this.internalUse = (short) 20;
      this.startPosition = startPosition;
    }
  }
}
