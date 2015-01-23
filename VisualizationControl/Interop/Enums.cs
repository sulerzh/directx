using System;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
  internal static class Enums
  {
    [Flags]
    internal enum MF_SOURCE_READER_FLAG : uint
    {
      None = 0U,
      ERROR = 1U,
      ENDOFSTREAM = 2U,
      NEWSTREAM = 4U,
      NATIVEMEDIATYPECHANGED = 16U,
      CURRENTMEDIATYPECHANGED = 32U,
      STREAMTICK = 256U,
    }

    [Flags]
    internal enum MFSESSION_SETTOPOLOGY_FLAGS : uint
    {
      None = 0U,
      MFSESSION_SETTOPOLOGY_IMMEDIATE = 1U,
      MFSESSION_SETTOPOLOGY_NORESOLUTION = 2U,
      MFSESSION_SETTOPOLOGY_CLEAR_CURRENT = 4U,
    }

    [Flags]
    internal enum MFT_ENUM_FLAG : uint
    {
      None = 0U,
      MFT_ENUM_FLAG_SYNCMFT = 1U,
      MFT_ENUM_FLAG_ASYNCMFT = 2U,
      MFT_ENUM_FLAG_HARDWARE = 4U,
      MFT_ENUM_FLAG_FIELDOFUSE = 8U,
      MFT_ENUM_FLAG_LOCALMFT = 16U,
      MFT_ENUM_FLAG_TRANSCODE_ONLY = 32U,
      MFT_ENUM_FLAG_SORTANDFILTER = 64U,
      MFT_ENUM_FLAG_ALL = MFT_ENUM_FLAG_TRANSCODE_ONLY | MFT_ENUM_FLAG_LOCALMFT | MFT_ENUM_FLAG_FIELDOFUSE | MFT_ENUM_FLAG_HARDWARE | MFT_ENUM_FLAG_ASYNCMFT | MFT_ENUM_FLAG_SYNCMFT,
    }

    [Flags]
    internal enum MFSESSION_GETFULLTOPOLOGY_FLAGS : uint
    {
      None = 0U,
      MF_SESSION_GETFULLTOPOLOGY_CURRENT = 1U,
    }

    public enum _MFBYTESTREAM_SEEK_ORIGIN
    {
      msoBegin,
      msoCurrent,
    }

    public enum _MFVideoInterlaceMode : uint
    {
      MFVideoInterlace_Unknown = 0U,
      MFVideoInterlace_Progressive = 2U,
      MFVideoInterlace_FieldInterleavedUpperFirst = 3U,
      MFVideoInterlace_FieldInterleavedLowerFirst = 4U,
      MFVideoInterlace_FieldSingleUpper = 5U,
      MFVideoInterlace_FieldSingleLower = 6U,
      MFVideoInterlace_MixedInterlaceOrProgressive = 7U,
      MFVideoInterlace_Last = 8U,
      MFVideoInterlace_ForceDWORD = 2147483647U,
    }

    internal enum eAVEncH264VProfile : uint
    {
      eAVEncH264VProfile_unknown = 0U,
      eAVEncH264VProfile_Base = 66U,
      eAVEncH264VProfile_Simple = 66U,
      eAVEncH264VProfile_Main = 77U,
      eAVEncH264VProfile_ScalableBase = 83U,
      eAVEncH264VProfile_ScalableHigh = 86U,
      eAVEncH264VProfile_Extended = 88U,
      eAVEncH264VProfile_High = 100U,
      eAVEncH264VProfile_High10 = 110U,
      eAVEncH264VProfile_MultiviewHigh = 118U,
      eAVEncH264VProfile_422 = 122U,
      eAVEncH264VProfile_StereoHigh = 128U,
      eAVEncH264VProfile_444 = 144U,
      eAVEncH264VProfile_ConstrainedBase = 256U,
      eAVEncH264VProfile_UCConstrainedHigh = 257U,
      eAVEncH264VProfile_UCScalableConstrainedBase = 258U,
      eAVEncH264VProfile_UCScalableConstrainedHigh = 259U,
    }
  }
}
