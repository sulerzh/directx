using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [Guid(IID.IMFCollection)]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IMFCollection
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetElementCount(out uint pcElements);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetElement([In] uint dwElementIndex, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnkElement);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void AddElement([MarshalAs(UnmanagedType.IUnknown), In] object pUnkElement);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void RemoveElement([In] uint dwElementIndex, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnkElement);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void InsertElementAt([In] uint dwIndex, [MarshalAs(UnmanagedType.IUnknown), In] object pUnknown);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void RemoveAllElements();
  }
}
