using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("A27003D0-2354-4F2A-8D6A-AB7CFF15437E")]
  [ComImport]
  public interface IMFRemoteAsyncCallback
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Invoke([MarshalAs(UnmanagedType.Error), In] int hr, [MarshalAs(UnmanagedType.IUnknown), In] object pRemoteResult);
  }
}
