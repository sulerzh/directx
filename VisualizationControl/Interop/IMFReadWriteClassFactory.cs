using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [Guid(IID.IMFReadWriteClassFactory)]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IMFReadWriteClassFactory
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CreateInstanceFromURL([MarshalAs(UnmanagedType.LPStruct), In] Guid clsid, [MarshalAs(UnmanagedType.LPWStr), In] string pwszURL, [MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttributes, [MarshalAs(UnmanagedType.LPStruct), In] Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CreateInstanceFromObject([MarshalAs(UnmanagedType.LPStruct), In] Guid clsid, [MarshalAs(UnmanagedType.IUnknown), In] object punkObject, [MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttributes, [MarshalAs(UnmanagedType.LPStruct), In] Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
  }
}
