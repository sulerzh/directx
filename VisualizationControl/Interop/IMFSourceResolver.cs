using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid(IID.IMFSourceResolver)]
  [ComImport]
  internal interface IMFSourceResolver
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CreateObjectFromURL([MarshalAs(UnmanagedType.LPWStr), In] string pwszURL, [In] uint dwFlags, [MarshalAs(UnmanagedType.Interface), In] IPropertyStore pProps, out uint pObjectType, [MarshalAs(UnmanagedType.IUnknown)] out object ppObject);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CreateObjectFromByteStream([MarshalAs(UnmanagedType.Interface), In] object pByteStream, [MarshalAs(UnmanagedType.LPWStr), In] string pwszURL, [In] uint dwFlags, [MarshalAs(UnmanagedType.Interface), In] IPropertyStore pProps, out uint pObjectType, [MarshalAs(UnmanagedType.IUnknown)] out object ppObject);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void BeginCreateObjectFromURL([MarshalAs(UnmanagedType.LPWStr), In] string pwszURL, [In] uint dwFlags, [MarshalAs(UnmanagedType.Interface), In] IPropertyStore pProps, [MarshalAs(UnmanagedType.Interface)] out object ppIUnknownCancelCookie, [MarshalAs(UnmanagedType.Interface), In] IMFAsyncCallback pCallback, [MarshalAs(UnmanagedType.IUnknown), In] object punkState);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void EndCreateObjectFromURL([MarshalAs(UnmanagedType.IUnknown), In] object pResult, out uint pObjectType, [MarshalAs(UnmanagedType.IUnknown)] out object ppObject);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void BeginCreateObjectFromByteStream([MarshalAs(UnmanagedType.Interface), In] object pByteStream, [MarshalAs(UnmanagedType.LPWStr), In] string pwszURL, [In] uint dwFlags, [MarshalAs(UnmanagedType.Interface), In] IPropertyStore pProps, [MarshalAs(UnmanagedType.IUnknown)] out IMFAsyncCallback ppIUnknownCancelCookie, [MarshalAs(UnmanagedType.Interface), In] object pCallback, [MarshalAs(UnmanagedType.IUnknown), In] object punkState);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void EndCreateObjectFromByteStream([MarshalAs(UnmanagedType.IUnknown), In] object pResult, out uint pObjectType, [MarshalAs(UnmanagedType.IUnknown)] out object ppObject);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CancelObjectCreation([MarshalAs(UnmanagedType.IUnknown), In] object pIUnknownCancelCookie);
  }
}
