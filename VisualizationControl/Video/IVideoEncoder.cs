using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.VisualizationControls.Video
{
  internal interface IVideoEncoder : IDisposable
  {
    EncodingStatus Initialize(string outputFileUrl, uint width, uint height, uint fps);

    EncodingStatus PresetAudioSource(string inputAudioFileUrl, double videoDuration, out ulong audioDuration);

    EncodingStatus WriteFrame(byte[] buffer, uint lines, uint stride, ulong start, ulong duration);

    Task<EncodingStatus> WriteAudioAsync(bool loop, bool fadeIn, bool fadeOut, CancellationToken token);
  }
}
