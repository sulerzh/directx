using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [Guid("AD4C1B00-4BF7-422F-9175-756693D9130D")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IMFByteStream
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetCapabilities(out uint pdwCapabilities);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetLength(out ulong pqwLength);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetLength([In] ulong qwLength);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetCurrentPosition(out ulong pqwPosition);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetCurrentPosition([In] ulong qwPosition);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void IsEndOfStream(out int pfEndOfStream);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Read(out byte pb, [In] uint cb, out uint pcbRead);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void RemoteBeginRead([In] uint cb, [MarshalAs(UnmanagedType.Interface), In] IMFRemoteAsyncCallback pCallback);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void RemoteEndRead([MarshalAs(UnmanagedType.IUnknown), In] object punkResult, out byte pb, [In] uint cb, out uint pcbRead);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Write([In] ref byte pb, [In] uint cb, out uint pcbWritten);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void RemoteBeginWrite([In] ref byte pb, [In] uint cb, [MarshalAs(UnmanagedType.Interface), In] IMFRemoteAsyncCallback pCallback);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void RemoteEndWrite([MarshalAs(UnmanagedType.IUnknown), In] object punkResult, out uint pcbWritten);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Seek([In] Enums._MFBYTESTREAM_SEEK_ORIGIN SeekOrigin, [In] long llSeekOffset, [In] uint dwSeekFlags, out ulong pqwCurrentPosition);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Flush();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Close();
  }
}
