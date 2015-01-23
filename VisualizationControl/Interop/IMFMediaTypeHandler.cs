using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [Guid(IID.IMFMediaTypeHandler)]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IMFMediaTypeHandler
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void IsMediaTypeSupported([MarshalAs(UnmanagedType.Interface), In] IMFMediaType pMediaType, [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetMediaTypeCount(out uint pdwTypeCount);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetMediaTypeByIndex([In] uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppType);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetCurrentMediaType([MarshalAs(UnmanagedType.Interface), In] IMFMediaType pMediaType);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetCurrentMediaType(out IMFMediaType pMediaType);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetMajorType(out Guid pguidMajorType);
  }
}
