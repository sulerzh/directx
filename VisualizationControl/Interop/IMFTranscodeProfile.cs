using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid(IID.IMFTranscodeProfile)]
  [ComImport]
  internal interface IMFTranscodeProfile
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetAudioAttributes([MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttrs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetAudioAttributes([MarshalAs(UnmanagedType.Interface)] out IMFAttributes ppAttrs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetVideoAttributes([MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttrs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetVideoAttributes([MarshalAs(UnmanagedType.Interface)] out IMFAttributes ppAttrs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetContainerAttributes([MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttrs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetContainerAttributes([MarshalAs(UnmanagedType.Interface)] out IMFAttributes ppAttrs);
  }
}
