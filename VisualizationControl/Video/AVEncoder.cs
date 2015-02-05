using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.VisualizationControls.Video
{
    internal sealed class AVEncoder : IVideoEncoder, IDisposable
    {
        internal static readonly Guid MfLowLatency = new Guid(0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 160, 0x24, 0xee);
        internal static readonly Guid MfSinkWriterDisableThrottling = new Guid(0x8b845d8, 0x2b74, 0x4afe, 0x9d, 0x53, 190, 0x16, 210, 0xd5, 0xae, 0x4f);
        internal static readonly Guid MfReadwriteEnableHardwareTransforms = new Guid(Consts.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS);
        private const string MP4 = ".mp4";
        private const uint HundredNanoSecondsPerSec = 10000000U;
        private const uint BitsPerSample = 16U;
        private const uint DefaultAudioSamplingRate = 44100U;
        private const uint NumOfAudioChannels = 2U;
        private const uint AverageBytesPerSec = 16000U;
        private uint Bitrate;
        private uint VideoWidth;
        private uint VideoHeight;
        private uint VideoFps;
        private IMFSinkWriter sinkWriter;
        private IMFSourceReader sourceReader;
        private bool isDisposed;
        private IMFMediaType audioMediaType;
        private IMFMediaType audioOutMediaType;
        private uint videoStreamIndex;
        private uint audioStreamIndex;
        private ulong numOfAudioSamples;
        private ulong averageSampleDuration;
        private ulong audioLength;
        private uint selectedAudioSamplingRate;
        private double targetDuration;

        ~AVEncoder()
        {
            this.Dispose(false);
        }

        public EncodingStatus Initialize(string outputFileUrl, uint width, uint height, uint fps)
        {
            this.VideoWidth = width;
            this.VideoHeight = height;
            this.VideoFps = fps;
            switch (this.VideoHeight)
            {
                case 720U:
                    this.Bitrate = 10000000U;
                    break;
                case 1080U:
                    this.Bitrate = 16000000U;
                    break;
                default:
                    this.Bitrate = 4000000U;
                    break;
            }
            try
            {
                IMFAttributes ppMFAttributes;
                MFHelper.MFCreateAttributes(out ppMFAttributes, 1U);
                ppMFAttributes.SetUINT32(MfLowLatency, 1U);
                ppMFAttributes.SetUINT32(MfSinkWriterDisableThrottling, 1U);
                ppMFAttributes.SetUINT32(MfReadwriteEnableHardwareTransforms, 1U);
                MFHelper.MFCreateSinkWriterFromURL(outputFileUrl, null, ppMFAttributes, out this.sinkWriter);
                IMFMediaType h264VideoType;
                MFHelper.MFCreateMediaType(out h264VideoType);
                h264VideoType.SetGUID(new Guid(Consts.MF_MT_MAJOR_TYPE), new Guid(Consts.MFMediaType_Video));
                h264VideoType.SetGUID(new Guid(Consts.MF_MT_SUBTYPE), new Guid(Consts.MFVideoFormat_H264));
                h264VideoType.SetUINT32(new Guid(Consts.MF_MT_AVG_BITRATE), this.Bitrate);
                h264VideoType.SetUINT32(new Guid(Consts.MF_MT_MPEG2_PROFILE), 77U);
                this.SetCommonAttributes(h264VideoType);
                this.sinkWriter.AddStream(h264VideoType, out this.videoStreamIndex);
                IMFMediaType yv12VideoType;
                MFHelper.MFCreateMediaType(out yv12VideoType);
                yv12VideoType.SetGUID(new Guid(Consts.MF_MT_MAJOR_TYPE), new Guid(Consts.MFMediaType_Video));
                yv12VideoType.SetGUID(new Guid(Consts.MF_MT_SUBTYPE), new Guid(Consts.MFVideoFormat_YV12));
                this.SetCommonAttributes(yv12VideoType);
                this.sinkWriter.SetInputMediaType(this.videoStreamIndex, yv12VideoType, null);
                if (this.audioOutMediaType != null && this.audioMediaType != null)
                {
                    this.sinkWriter.AddStream(this.audioOutMediaType, out this.audioStreamIndex);
                    this.sinkWriter.SetInputMediaType(this.audioStreamIndex, this.audioMediaType, null);
                }
                this.sinkWriter.BeginWriting();
                return EncodingStatus.Success;
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(string.Format("AVEncoder.WriteSample: Failed to initialize video encoder. Error Code = {0}, Message = {1}.", (object)(ex is COMException ? ((ExternalException)ex).ErrorCode : 0), (object)ex.Message));
                return EncodingStatus.EncoderInitializationError;
            }
        }

        public EncodingStatus PresetAudioSource(string inputAudioFileUrl, double videoDuration, out ulong audioDuration)
        {
            audioDuration = 0UL;
            try
            {
                this.ResetAudioConfiguration();
                if (string.IsNullOrEmpty(inputAudioFileUrl))
                    return EncodingStatus.Success;
                this.targetDuration = videoDuration;

                // Create the source reader to read the input file.
                MFHelper.MFCreateSourceReaderFromURL(inputAudioFileUrl, null, out this.sourceReader);
                this.selectedAudioSamplingRate = DefaultAudioSamplingRate;
                object pvarAttribute;
                // use Presentation Descriptor to get Duration
                this.sourceReader.GetPresentationAttribute(uint.MaxValue, new Guid(Consts.MF_PD_DURATION), out pvarAttribute);
                this.audioLength = audioDuration = (ulong)pvarAttribute;
                this.audioLength = 0;
                this.numOfAudioSamples = audioDuration * NumOfAudioChannels * this.selectedAudioSamplingRate / HundredNanoSecondsPerSec;
                this.averageSampleDuration = this.numOfAudioSamples * HundredNanoSecondsPerSec / (this.selectedAudioSamplingRate * NumOfAudioChannels);

                // Select the first audio stream, and deselect all other streams.
                this.sourceReader.SetStreamSelection(Consts.MF_SOURCE_READER_ALL_STREAMS, false);
                this.sourceReader.SetStreamSelection(Consts.MF_SOURCE_READER_FIRST_AUDIO_STREAM, true);

                // Create a partial media type that specifies uncompressed PCM audio.
                IMFMediaType pPartialType;
                MFHelper.MFCreateMediaType(out pPartialType);
                pPartialType.SetGUID(new Guid(Consts.MF_MT_MAJOR_TYPE), new Guid(Consts.MFMediaType_Audio));
                pPartialType.SetGUID(new Guid(Consts.MF_MT_SUBTYPE), new Guid(Consts.MFAudioFormat_PCM));
                pPartialType.SetUINT32(new Guid(Consts.MF_MT_AUDIO_BITS_PER_SAMPLE), BitsPerSample);
                pPartialType.SetUINT32(new Guid(Consts.MF_MT_AUDIO_SAMPLES_PER_SECOND), this.selectedAudioSamplingRate);
                pPartialType.SetUINT32(new Guid(Consts.MF_MT_AUDIO_NUM_CHANNELS), NumOfAudioChannels);

                // Set this type on the source reader. The source reader will
                // load the necessary decoder.
                this.sourceReader.SetCurrentMediaType(Consts.MF_SOURCE_READER_FIRST_AUDIO_STREAM, IntPtr.Zero, pPartialType);
                // Get the complete uncompressed format.
                this.sourceReader.GetCurrentMediaType(Consts.MF_SOURCE_READER_FIRST_AUDIO_STREAM, out this.audioMediaType);

                // ？
                MFHelper.MFCreateMediaType(out this.audioOutMediaType);
                this.audioOutMediaType.SetGUID(new Guid(Consts.MF_MT_MAJOR_TYPE), new Guid(Consts.MFMediaType_Audio));
                this.audioOutMediaType.SetGUID(new Guid(Consts.MF_MT_SUBTYPE), new Guid(Consts.MFAudioFormat_AAC));
                this.audioOutMediaType.SetUINT32(new Guid(Consts.MF_MT_AUDIO_AVG_BYTES_PER_SECOND), AverageBytesPerSec);
                this.audioOutMediaType.SetUINT32(new Guid(Consts.MF_MT_AUDIO_BITS_PER_SAMPLE), BitsPerSample);
                this.audioOutMediaType.SetUINT32(new Guid(Consts.MF_MT_AUDIO_SAMPLES_PER_SECOND), this.selectedAudioSamplingRate);
                return EncodingStatus.Success;
            }
            catch (Exception ex)
            {
                this.ResetAudioConfiguration();
                VisualizationTraceSource.Current.Fail(string.Format("AVEncoder.WriteSample: Failed to initialize audio encoder. Error Code = {0}, Message = {1}.", (object)(ex is COMException ? ((ExternalException)ex).ErrorCode : 0), (object)ex.Message));
                return EncodingStatus.AudioFileInitializationError;
            }
        }

        /// <summary>
        /// 写入关键帧
        /// </summary>
        /// <param name="buffer">YV12缓冲区</param>
        /// <param name="lines">帧高像素数</param>
        /// <param name="stride">
        /// The stride is the number of bytes 
        /// from one row of pixels in memory 
        /// to the next row of pixels in memory. 
        /// Stride is also called pitch
        /// </param>
        /// <param name="start">帧位置</param>
        /// <param name="duration">帧持续时间</param>
        /// <returns></returns>
        public EncodingStatus WriteFrame(byte[] buffer, uint lines, uint stride, ulong start, ulong duration)
        {
            IMFSample sample = null;
            IMFMediaBuffer mediaBuffer = null;
            GCHandle gcHandle = new GCHandle();
            try
            {
                uint num = (uint)buffer.Length;
                MFHelper.MFCreateMemoryBuffer(num, out mediaBuffer);
                IntPtr ppbBuffer;
                uint pcbMaxLength;
                uint pcbCurrentLength;
                mediaBuffer.Lock(out ppbBuffer, out pcbMaxLength, out pcbCurrentLength);
                gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                MFHelper.MFCopyImage(ppbBuffer, stride, gcHandle.AddrOfPinnedObject(), stride, stride, lines);
                MFHelper.MFCopyImage(IncrementPointer(ppbBuffer, stride * lines), stride / 2U, IncrementPointer(gcHandle.AddrOfPinnedObject(), (long)(stride * lines)), stride / 2U, stride / 2U, lines / 2U);
                MFHelper.MFCopyImage(IncrementPointer(ppbBuffer, 5U * stride * lines / 4U), stride / 2U, IncrementPointer(gcHandle.AddrOfPinnedObject(), (long)(5U * stride * lines / 4U)), stride / 2U, stride / 2U, lines / 2U);
                mediaBuffer.Unlock();
                mediaBuffer.SetCurrentLength(num);
                MFHelper.MFCreateSample(out sample);
                sample.AddBuffer(mediaBuffer);
                sample.SetSampleDuration(duration);
                sample.SetSampleTime(start);
                this.sinkWriter.WriteSample(this.videoStreamIndex, sample);
                ReleaseComObject(sample);
                return EncodingStatus.Success;
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(string.Format("AVEncoder.WriteSample: Failed to write sample at stream index {0}. Error Code = {1}, Message = {2}.", (object)this.videoStreamIndex, (object)(ex is COMException ? ((ExternalException)ex).ErrorCode : 0), (object)ex.Message));
                return EncodingStatus.VideoFrameProcessingError;
            }
            finally
            {
                ReleaseComObject(sample);
                ReleaseComObject(mediaBuffer);
                if (gcHandle.IsAllocated)
                    gcHandle.Free();
            }
        }

        /// <summary>
        /// Decoding Audio
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/dd757929(v=vs.85).aspx
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="fadeIn"></param>
        /// <param name="fadeOut"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<EncodingStatus> WriteAudioAsync(bool loop, bool fadeIn, bool fadeOut, CancellationToken token)
        {
            TaskCompletionSource<EncodingStatus> source = new TaskCompletionSource<EncodingStatus>();
            if (this.audioMediaType == null || this.audioOutMediaType == null)
                source.SetResult(EncodingStatus.Success);
            else
                Task.Factory.StartNew((() =>
                {
                    ulong tarDuration = (ulong)(this.targetDuration * HundredNanoSecondsPerSec);
                    ulong fadeDuration = Math.Min(tarDuration / NumOfAudioChannels, 20000000UL);
                    ulong maxSampleNum = fadeDuration / HundredNanoSecondsPerSec * this.selectedAudioSamplingRate * 2UL;
                    Hermite fadeOutHermite = new Hermite(0.0, maxSampleNum, 1.0, 0.0, 0.0, 0.0);
                    Hermite fadeInHermite = new Hermite(0.0, maxSampleNum, 0.0, 0.0, 1.0, 0.0);
                    IMFSample pSample = null;
                    try
                    {
                        bool flag = false;
                        ulong sampleTime = 0UL;
                        ulong currSampleNum1 = 0UL;
                        ulong currSampleNum2 = 0UL;
                        while (sampleTime < tarDuration)
                        {
                            if (token.IsCancellationRequested)
                            {
                                source.SetCanceled();
                                return;
                            }
                            uint pdwActualStreamIndex;
                            uint dwFlags;
                            ulong pllTimestamp;

                            // Read the next sample.
                            this.sourceReader.ReadSample(
                                Consts.MF_SOURCE_READER_FIRST_AUDIO_STREAM,
                                0U, out pdwActualStreamIndex, out dwFlags,
                                out pllTimestamp, out pSample);
                            // 到达流的结尾
                            if (((int)dwFlags & (int)Enums.MF_SOURCE_READER_FLAG.ENDOFSTREAM) != 0)
                            {
                                // 循环模式下，重新定位音频流到开始位置
                                if (sampleTime < tarDuration && loop)
                                {
                                    IMFSourceReader mfSourceReader = this.sourceReader;
                                    Guid guidTimeFormat = Guid.Empty;//GUID_NULL means 100-nanosecond units.
                                    PropVariant propVariant = new PropVariant()
                                    {
                                        vt = 20, // variant type is VT_I8
                                        hVal = 0L
                                    };
                                    mfSourceReader.SetCurrentPosition(guidTimeFormat, ref propVariant);
                                    flag = true;
                                }
                                else
                                {
                                    ReleaseComObject(pSample);
                                    break;
                                }
                            }
                            // 流读取出错
                            else if (((int)dwFlags & (int)Enums.MF_SOURCE_READER_FLAG.ERROR) != 0)
                            {
                                source.SetResult(EncodingStatus.AudioSampleProcessingError);
                                return;
                            }
                            // 开始采样
                            else if (pSample != null)
                            {
                                ulong duration;
                                pSample.GetSampleDuration(out duration);
                                duration = (long)duration == 0L ? this.averageSampleDuration : duration;
                                if (flag)
                                    pSample.SetSampleTime(sampleTime);
                                if (fadeOut &&
                                    (sampleTime + fadeDuration >= tarDuration && loop ||
                                     sampleTime + fadeDuration >= Math.Min(this.audioLength, tarDuration) && !loop))
                                    this.ScaleSample(pSample, fadeOutHermite, maxSampleNum, ref currSampleNum1);
                                if (fadeIn && sampleTime < fadeDuration)
                                    this.ScaleSample(pSample, fadeInHermite, maxSampleNum, ref currSampleNum2);
                                sampleTime += duration;
                                this.sinkWriter.WriteSample(this.audioStreamIndex, pSample);
                                ReleaseComObject(pSample);
                                pSample = null;
                            }
                            else
                            {
                                this.sinkWriter.SendStreamTick(this.audioStreamIndex, sampleTime);
                                sampleTime += this.averageSampleDuration;
                            }
                        }
                        source.SetResult(EncodingStatus.Success);
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail(
                            string.Format(
                                "AVEncoder.WriteAudioAsync: Failed to write sample at stream index {0}. Error Code = {1}, Message = {2}.",
                                this.audioStreamIndex,
                                (ex is COMException ? ((ExternalException)ex).ErrorCode : 0),
                                ex.Message));
                        ReleaseComObject(pSample);
                        source.SetResult(EncodingStatus.AudioSampleProcessingError);
                    }
                }), token, TaskCreationOptions.LongRunning, TaskScheduler.FromCurrentSynchronizationContext());
            return source.Task;
        }

        /// <summary>
        /// 使用采样器和缩放器对声音进行缩放，实现隐入和隐退效果
        /// </summary>
        /// <param name="sample">采样器</param>
        /// <param name="hermite">缩放器</param>
        /// <param name="maxSampleNum"></param>
        /// <param name="currSampleNum"></param>
        private unsafe void ScaleSample(IMFSample sample, Hermite hermite, ulong maxSampleNum, ref ulong currSampleNum)
        {
            IMFMediaBuffer pBuffer;
            // Get a pointer to the audio data in the sample.
            sample.ConvertToContiguousBuffer(out pBuffer);
            try
            {
                IntPtr pbBuffer;
                uint maxLength;
                uint currentLength;
                pBuffer.Lock(out pbBuffer, out maxLength, out currentLength);
                short* pSampleData = (short*)pbBuffer.ToPointer();
                uint channelLength = currentLength / NumOfAudioChannels;
                for (uint i = 0U; i < channelLength; ++i)
                {
                    currSampleNum = currSampleNum > maxSampleNum ? maxSampleNum : currSampleNum;
                    double fadedValue = pSampleData[0] * hermite.Evaluate(currSampleNum++);
                    if (fadedValue < short.MinValue)
                        fadedValue = short.MinValue;
                    else if (fadedValue > short.MaxValue)
                        fadedValue = short.MaxValue;
                    pSampleData[0] = (short)fadedValue;
                    if (i < channelLength - 1U)
                        ++pSampleData;
                }
            }
            finally
            {
                pBuffer.Unlock();
                ReleaseComObject(pBuffer);
            }
        }

        private void ResetAudioConfiguration()
        {
            ReleaseComObject(this.audioMediaType);
            ReleaseComObject(this.audioOutMediaType);
            ReleaseComObject(this.sourceReader);
            this.audioMediaType = null;
            this.audioOutMediaType = null;
            this.sourceReader = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
                return;
            try
            {
                if (this.sinkWriter != null)
                    this.sinkWriter.DoFinalize();
                ReleaseComObject(this.audioOutMediaType);
                ReleaseComObject(this.audioMediaType);
                ReleaseComObject(this.sinkWriter);
                ReleaseComObject(this.sourceReader);
                this.sinkWriter = null;
                this.sourceReader = null;
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(string.Format("AVEncoder.Dispose: Exception while disposing. Error Code = {0}, Message = {1}.", (object)(ex is COMException ? ((ExternalException)ex).ErrorCode : 0), (object)ex.Message));
            }
            this.isDisposed = true;
        }

        private void SetCommonAttributes(IMFMediaType mediaType)
        {
            ulong unValue1 = (ulong)this.VideoWidth << 32 | (ulong)this.VideoHeight;
            ulong unValue2 = (ulong)this.VideoFps << 32 | 1UL;
            mediaType.SetUINT32(new Guid(Consts.MF_MT_INTERLACE_MODE), 2U);
            mediaType.SetUINT64(new Guid(Consts.MF_MT_FRAME_SIZE), unValue1);
            mediaType.SetUINT64(new Guid(Consts.MF_MT_FRAME_RATE), unValue2);
            mediaType.SetUINT64(new Guid(Consts.MF_MT_PIXEL_ASPECT_RATIO), 1UL);
        }

        private static void ReleaseComObject(object comObj)
        {
            if (comObj == null || !Marshal.IsComObject(comObj))
                return;
            Marshal.ReleaseComObject(comObj);
        }

        private static IntPtr IncrementPointer(IntPtr ptr, long size)
        {
            return new IntPtr(ptr.ToInt64() + size);
        }
    }
}
