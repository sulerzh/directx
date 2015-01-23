using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid(IID.IMFSample)]
  [ComImport]
  internal interface IMFSample
  {
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetItem([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [In, Out] ref object pValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetItemType([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, out uint pType);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CompareItem([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, ref object Value, out int pbResult);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Compare([MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs, uint MatchType, out int pbResult);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetUINT32([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, out uint punValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetUINT64([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, out ulong punValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetDouble([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, out double pfValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetGUID([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, out Guid pguidValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetStringLength([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, out uint pcchLength);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetString([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [MarshalAs(UnmanagedType.LPWStr), Out] string pwszValue, uint cchBufSize, out uint pcchLength);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetAllocatedString([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue, out uint pcchLength);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetBlobSize([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, out uint pcbBlobSize);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetBlob([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [MarshalAs(UnmanagedType.LPArray), Out] byte[] pBuf, uint cbBufSize, out uint pcbBlobSize);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetAllocatedBlob([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [Out] IntPtr ppBuf, out uint pcbSize);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetUnknown([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, ref Guid riid, out IntPtr ppv);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetItem([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, ref object Value);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void DeleteItem([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void DeleteAllItems();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetUINT32([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, uint unValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetUINT64([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, ulong unValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetDouble([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, double fValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetGUID([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [MarshalAs(UnmanagedType.LPStruct), In] Guid guidValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetString([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [MarshalAs(UnmanagedType.LPWStr), In] string wszValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetBlob([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), In] byte[] pBuf, uint cbBufSize);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetUnknown([MarshalAs(UnmanagedType.LPStruct), In] Guid guidKey, [MarshalAs(UnmanagedType.IUnknown), In] object pUnknown);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void LockStore();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void UnlockStore();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetCount(out uint pcItems);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetItemByIndex(uint unIndex, out Guid pguidKey, [In, Out] ref object pValue);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CopyAllItems([MarshalAs(UnmanagedType.Interface), In] IMFAttributes pDest);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetSampleFlags(out uint pdwSampleFlags);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetSampleFlags([In] uint dwSampleFlags);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetSampleTime(out ulong phnsSampleTime);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetSampleTime([In] ulong hnsSampleTime);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetSampleDuration(out ulong phnsSampleDuration);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetSampleDuration([In] ulong hnsSampleDuration);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetBufferCount(out uint pdwBufferCount);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetBufferByIndex([In] uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer ppBuffer);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void ConvertToContiguousBuffer([MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer ppBuffer);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void AddBuffer([MarshalAs(UnmanagedType.Interface), In] IMFMediaBuffer pBuffer);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void RemoveBufferByIndex([In] uint dwIndex);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void RemoveAllBuffers();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetTotalLength(out uint pcbTotalLength);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void CopyToBuffer([MarshalAs(UnmanagedType.Interface), In] IMFMediaBuffer pBuffer);
  }
}
