using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid(IID.IMFSourceReader)]
  [ComImport]
  internal interface IMFSourceReader
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetStreamSelection([In] uint dwStreamIndex, [MarshalAs(UnmanagedType.Bool)] out bool pfSelected);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetStreamSelection([In] uint dwStreamIndex, [MarshalAs(UnmanagedType.Bool), In] bool fSelected);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetNativeMediaType([In] uint dwStreamIndex, [In] uint dwMediaTypeIndex, [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetCurrentMediaType([In] uint dwStreamIndex, [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetCurrentMediaType([In] uint dwStreamIndex, [In, Out] IntPtr pdwReserved, [MarshalAs(UnmanagedType.Interface), In] IMFMediaType pMediaType);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetCurrentPosition([MarshalAs(UnmanagedType.LPStruct), In] Guid guidTimeFormat, [In] ref PropVariant varPosition);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void ReadSample([In] uint dwStreamIndex, [In] uint dwControlFlags, out uint pdwActualStreamIndex, out uint pdwStreamFlags, out ulong pllTimestamp, [MarshalAs(UnmanagedType.Interface)] out IMFSample ppSample);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Flush([In] uint dwStreamIndex);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetServiceForStream([In] uint dwStreamIndex, [MarshalAs(UnmanagedType.LPStruct), In] Guid guidService, [MarshalAs(UnmanagedType.LPStruct), In] Guid riid, out IntPtr ppvObject);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetPresentationAttribute([In] uint dwStreamIndex, [MarshalAs(UnmanagedType.LPStruct), In] Guid guidAttribute, out object pvarAttribute);
  }
}
