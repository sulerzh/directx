using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [Guid(IID.IMFAsyncCallback)]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IMFAsyncCallback
  {
    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    int GetParameters(out uint pdwFlags, out uint pdwQueue);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    int Invoke([MarshalAs(UnmanagedType.Interface), In] IMFAsyncResult pAsyncResult);
  }
}
