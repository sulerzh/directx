using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [Guid(IID.IMFMediaSession)]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IMFMediaSession
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
    void SetTopology([In] uint dwSetTopologyFlags, [MarshalAs(UnmanagedType.Interface), In] IMFTopology pTopology);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void ClearTopologies();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Start([MarshalAs(UnmanagedType.LPStruct), In] Guid pguidTimeFormat, [MarshalAs(UnmanagedType.LPStruct), In] MediaSessionStartPosition pvarStartPosition);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Pause();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Stop();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Close();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Shutdown();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetClock([MarshalAs(UnmanagedType.Interface)] out IMFClock ppClock);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetSessionCapabilities(out uint pdwCaps);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetFullTopology([In] uint dwGetFullTopologyFlags, [In] ulong TopoId, [MarshalAs(UnmanagedType.Interface)] out IMFTopology ppFullTopology);
  }
}
