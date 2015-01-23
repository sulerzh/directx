using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  internal static class MFHelper
  {
    public static readonly uint MediaFoundationVersion = 624U;

    [DllImport("mfplat.dll", PreserveSig = false)]
    public static extern void MFStartup([In] uint IVersion, [In] uint dwFlags);

    [DllImport("mfplat.dll", PreserveSig = false)]
    public static extern void MFShutdown();

    [DllImport("mfplat.dll", PreserveSig = false)]
    public static extern void MFCreateMediaType([MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMFType);

    [DllImport("mfplat.dll", PreserveSig = false)]
    public static extern void MFCreateSourceResolver([MarshalAs(UnmanagedType.Interface)] out IMFSourceResolver ppISourceResolver);

    [DllImport("mfplat.dll", PreserveSig = false)]
    public static extern void MFCreateAttributes([MarshalAs(UnmanagedType.Interface)] out IMFAttributes ppMFAttributes, [In] uint cInitialSize);

    [DllImport("Mf.dll", PreserveSig = false)]
    public static extern void MFCreateMediaSession([MarshalAs(UnmanagedType.Interface), In] IMFAttributes pConfiguration, [MarshalAs(UnmanagedType.Interface)] out IMFMediaSession ppMS);

    [DllImport("Mf.dll", PreserveSig = false)]
    public static extern void MFCreateTranscodeProfile([MarshalAs(UnmanagedType.Interface)] out IMFTranscodeProfile ppTranscodeProfile);

    [DllImport("Mf.dll", PreserveSig = false)]
    public static extern void MFTranscodeGetAudioOutputAvailableTypes([MarshalAs(UnmanagedType.LPStruct), In] Guid guidSubType, [In] uint dwMFTFlags, [In] uint pCodecConfig, [MarshalAs(UnmanagedType.Interface)] out IMFCollection ppAvailableTypes);

    [DllImport("Mf.dll", PreserveSig = false)]
    public static extern void MFCreateTranscodeTopology([MarshalAs(UnmanagedType.Interface), In] IMFMediaSource pSrc, [MarshalAs(UnmanagedType.LPWStr), In] string pwszOutputFilePath, [MarshalAs(UnmanagedType.Interface), In] IMFTranscodeProfile pProfile, [MarshalAs(UnmanagedType.Interface)] out IMFTopology ppTranscodeTopo);

    [DllImport("Mfreadwrite.dll", PreserveSig = false)]
    public static extern void MFCreateSinkWriterFromURL([MarshalAs(UnmanagedType.LPWStr), In] string pwszOutputURL, [MarshalAs(UnmanagedType.Interface), In] IMFByteStream pByteStream, [MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttributes, [MarshalAs(UnmanagedType.Interface)] out IMFSinkWriter ppSinkWriter);

    [DllImport("Mfreadwrite.dll", PreserveSig = false)]
    public static extern void MFCreateSourceReaderFromURL([MarshalAs(UnmanagedType.LPWStr), In] string pwszInputURL, [MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttributes, [MarshalAs(UnmanagedType.Interface)] out IMFSourceReader ppSourceReader);

    [DllImport("Mf.dll", PreserveSig = false)]
    public static extern void MFSetAttributeSize([MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttributes, [In] Guid guidkey, [In] uint unWidth, [In] uint unHeight);

    [DllImport("Mf.dll", EntryPoint = "MFSetAttributeSize", PreserveSig = false)]
    public static extern void MFSetAttributeRatio([MarshalAs(UnmanagedType.Interface), In] IMFAttributes pAttributes, [In] Guid guidkey, [In] uint unNumerator, [In] uint unDenominator);

    [DllImport("Mfplat.dll", PreserveSig = false)]
    public static extern void MFCreateMemoryBuffer([In] uint cbMaxLength, [MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer mediaBuffer);

    [DllImport("Mfplat.dll", PreserveSig = false)]
    public static extern void MFCopyImage([In] IntPtr pDest, [In] uint lDestStride, [In] IntPtr pSrc, [In] uint lSrcStride, [In] uint dwWidthInBytes, [In] uint dwLines);

    [DllImport("Mfplat.dll", PreserveSig = false)]
    public static extern void MFCreateSample([MarshalAs(UnmanagedType.Interface)] out IMFSample sample);

    [DllImport("Mfplat.dll", PreserveSig = false)]
    public static extern void MFFrameRateToAverageTimePerFrame([In] uint unNumerator, [In] uint unDenominator, out ulong punAverageTimePerFrame);
  }
}
