using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.VisualizationControls.Video
{
    internal sealed class TourVideoSession : IDisposable
    {
        private static readonly Dictionary<VideoDisplayResolution, Tuple<uint, uint>> DisplayResolutionMap =
            new Dictionary<VideoDisplayResolution, Tuple<uint, uint>>()
            {
                {
                    VideoDisplayResolution.Vd1080P,
                    Tuple.Create<uint, uint>(1920U, 1080U)
                },
                {
                    VideoDisplayResolution.Vd720P,
                    Tuple.Create<uint, uint>(1280U, 720U)
                },
                {
                    VideoDisplayResolution.Vd360P,
                    Tuple.Create<uint, uint>(640U, 360U)
                }
            };
        public const uint DefaultFps = 30U;
        private const string FileExt = ".mp4";
        private bool isDisposed;
        private readonly IVideoEncoder encoder;
        private bool commit;
        private string fileName;
        private byte[] buffer;
        private Rectangle displayRectangle;
        private ulong perFrameDuration;
        private ulong startOfFrame;
        private BitmapData data;

        public TourVideoSession()
        {
            MFHelper.MFStartup(MFHelper.MediaFoundationVersion, 0U);
            this.encoder = new AVEncoder();
        }

        ~TourVideoSession()
        {
            this.Dispose(false);
        }

        public EncodingStatus ConfigureSoundtrack(string inputAudioFileUrl, double videoDuration, out double audioDuration)
        {
            ulong num;
            EncodingStatus encodingStatus = this.encoder.PresetAudioSource(inputAudioFileUrl, videoDuration, out num);
            audioDuration = encodingStatus != EncodingStatus.Success ? -1.0 : (double)num / 10000000.0;
            return encodingStatus;
        }

        public EncodingStatus ConfigureVideoOptions(string outputFileName, VideoDisplayResolution displayResolution, BitmapData bmpData, uint fps = DefaultFps)
        {
            if (!outputFileName.EndsWith(FileExt, StringComparison.InvariantCultureIgnoreCase))
                outputFileName = outputFileName + FileExt;
            this.data = bmpData;
            this.fileName = outputFileName;
            this.displayRectangle = new Rectangle(0, 0, (int)DisplayResolutionMap[displayResolution].Item1, (int)DisplayResolutionMap[displayResolution].Item2);
            this.buffer = new byte[(int)Math.Ceiling(1.5 * this.displayRectangle.Width * this.displayRectangle.Height)];
            MFHelper.MFFrameRateToAverageTimePerFrame(fps, 1U, out this.perFrameDuration);
            return this.encoder.Initialize(this.fileName, (uint)this.displayRectangle.Width, (uint)this.displayRectangle.Height, fps);
        }

        public Task<EncodingStatus> OverlaySoundtrackAsync(bool loop, bool fadeIn, bool fadeOut, CancellationToken token)
        {
            return this.encoder.WriteAudioAsync(loop, fadeIn, fadeOut, token);
        }

        public EncodingStatus AddFrame()
        {
            EncodingStatus encodingStatus = EncodingStatus.Success;
            try
            {
                if (Environment.ProcessorCount > 1)
                    ImageConverter.ParallelRGB32ToYV12(this.buffer, this.data.Width, this.data.Scan0, this.data.Stride, this.data.Width, this.data.Height);
                else
                    ImageConverter.RGB32ToYV12(this.buffer, this.data.Width, this.data.Scan0, this.data.Stride, this.data.Width, this.data.Height);
                encodingStatus = this.encoder.WriteFrame(this.buffer, (uint)this.displayRectangle.Height, (uint)this.displayRectangle.Width, this.startOfFrame, this.perFrameDuration);
                this.startOfFrame += this.perFrameDuration;
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(string.Format("TourVideoSession.AddFrame: Failed to write frame. Error Code = {0}, Message = {1}.", (object)(ex is COMException ? ((ExternalException)ex).ErrorCode : 0), (object)ex.Message));
            }
            return encodingStatus;
        }

        public static VideoDisplayResolution GetSupportedResolution(int w, int h)
        {
            foreach (VideoDisplayResolution index in DisplayResolutionMap.Keys)
            {
                if (DisplayResolutionMap[index].Item1 == w && DisplayResolutionMap[index].Item2 == h)
                    return index;
            }
            throw new NotSupportedException("Invalid display resolution.");
        }

        public void Commit()
        {
            this.commit = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
                return;
            try
            {
                if (disposing)
                    this.encoder.Dispose();
                MFHelper.MFShutdown();
                if (!this.commit)
                {
                    if (File.Exists(this.fileName))
                        File.Delete(this.fileName);
                }
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(string.Format("TourVideoSession.Dispose: Exception while disposing. Error Code = {0}, Message = {1}.", (object)(ex is COMException ? ((ExternalException)ex).ErrorCode : 0), (object)ex.Message));
            }
            this.isDisposed = true;
        }
    }
}
