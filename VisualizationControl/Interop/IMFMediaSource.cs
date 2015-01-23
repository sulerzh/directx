using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [Guid(IID.IMFMediaSource)]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IMFMediaSource
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetEvent([In] uint dwFlags, [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void BeginGetEvent([MarshalAs(UnmanagedType.Interface), In] IMFAsyncCallback pCallback, [MarshalAs(UnmanagedType.IUnknown), In] object punkState);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void EndGetEvent([MarshalAs(UnmanagedType.Interface), In] IMFAsyncResult pResult, [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void QueueEvent([In] uint met, [MarshalAs(UnmanagedType.LPStruct), In] Guid guidExtendedType, [MarshalAs(UnmanagedType.Error), In] int hrStatus, [In] ref object pvValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetCharacteristics(out uint pdwCharacteristics);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CreatePresentationDescriptor([MarshalAs(UnmanagedType.Interface)] out IMFPresentationDescriptor ppPresentationDescriptor);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Start([MarshalAs(UnmanagedType.Interface), In] IMFPresentationDescriptor pPresentationDescriptor, [MarshalAs(UnmanagedType.LPStruct), In] Guid pguidTimeFormat, [In] object pvarStartPosition);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Stop();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Pause();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Shutdown();
  }
}
