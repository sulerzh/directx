using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.VisualizationControls.Video;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  internal class CreateVideoViewModel : DialogViewModelBase
  {
    private static readonly Dictionary<VideoDisplayResolution, Tuple<int, int>> resolutions = new Dictionary<VideoDisplayResolution, Tuple<int, int>>()
    {
      {
        VideoDisplayResolution.Vd1080P,
        Tuple.Create<int, int>(1920, 1080)
      },
      {
        VideoDisplayResolution.Vd720P,
        Tuple.Create<int, int>(1280, 720)
      },
      {
        VideoDisplayResolution.Vd360P,
        Tuple.Create<int, int>(640, 360)
      }
    };
    private readonly AutoResetEvent cancellationEvent = new AutoResetEvent(false);
    private string videoProcessingStatus = Resources.CreateVideo_StatusInProgress;
    private string cancelOrClose = Resources.Dialog_CancelText;
    private string soundtrackName = Resources.CreateVideo_NoAudioSelected;
    private const int WidthLowDef = 640;
    private const int DefaultDone = 0;
    private const int CancellationTimeout = 5000;
    private readonly VisualizationModel visualizationModel;
    private readonly Dispatcher dispatcher;
    private readonly HostControlViewModel hostControlViewModelOriginal;
    private readonly BackgroundWorker worker;
    private bool failed;
    private bool fileNotFound;
    private bool cancelled;
    private TourVideoSession session;
    private EncodingStatus encodingStatus;
    private VideoDisplayResolution selectedResolution;
    private bool videoSessionInitialized;
    private bool isComplete;
    private int percentDone;
    private bool isSoundtrackOptionSet;
    private string soundtrack;
    private bool fadeIn;
    private bool fadeOut;
    private bool loop;
    private string soundtrackDuration;

    public static string SelectedResolutionProperty
    {
      get
      {
        return "SelectedResolution";
      }
    }

    public VideoDisplayResolution SelectedResolution
    {
      get
      {
        return this.selectedResolution;
      }
      set
      {
        this.SetProperty<VideoDisplayResolution>(CreateVideoViewModel.SelectedResolutionProperty, ref this.selectedResolution, value, false);
      }
    }

    public string SelectedFileName { get; set; }

    public static string VideoSessionInitializedProperty
    {
      get
      {
        return "VideoSessionInitialized";
      }
    }

    public bool VideoSessionInitialized
    {
      get
      {
        return this.videoSessionInitialized;
      }
      set
      {
        this.SetProperty<bool>(CreateVideoViewModel.VideoSessionInitializedProperty, ref this.videoSessionInitialized, value, false);
      }
    }

    public static string VideoProcessingStatusProperty
    {
      get
      {
        return "VideoProcessingStatus";
      }
    }

    public string VideoProcessingStatus
    {
      get
      {
        return this.videoProcessingStatus;
      }
      set
      {
        this.SetProperty<string>(CreateVideoViewModel.VideoProcessingStatusProperty, ref this.videoProcessingStatus, value, false);
      }
    }

    public static string CancelOrCloseProperty
    {
      get
      {
        return "CancelOrClose";
      }
    }

    public string CancelOrClose
    {
      get
      {
        return this.cancelOrClose;
      }
      set
      {
        this.SetProperty<string>(CreateVideoViewModel.CancelOrCloseProperty, ref this.cancelOrClose, value, false);
      }
    }

    public static string IsProcessingCompletedProperty
    {
      get
      {
        return "IsProcessingCompleted";
      }
    }

    public bool IsProcessingCompleted
    {
      get
      {
        return this.isComplete;
      }
      set
      {
        this.SetProperty<bool>(CreateVideoViewModel.IsProcessingCompletedProperty, ref this.isComplete, value, false);
      }
    }

    public static string PercentDoneProperty
    {
      get
      {
        return "PercentDone";
      }
    }

    public int PercentDone
    {
      get
      {
        return this.percentDone;
      }
      set
      {
        this.SetProperty<int>(CreateVideoViewModel.PercentDoneProperty, ref this.percentDone, value, false);
      }
    }

    public static string IsSoundtrackOptionSetProperty
    {
      get
      {
        return "IsSoundtrackOptionSet";
      }
    }

    public bool IsSoundtrackOptionSet
    {
      get
      {
        return this.isSoundtrackOptionSet;
      }
      set
      {
        this.SetProperty<bool>(CreateVideoViewModel.IsSoundtrackOptionSetProperty, ref this.isSoundtrackOptionSet, value, false);
      }
    }

    public static string SelectedSoundtrackLocationProperty
    {
      get
      {
        return "SelectedSoundtrackLocation";
      }
    }

    public string SelectedSoundtrackLocation
    {
      get
      {
        return this.soundtrack ?? Resources.CreateVideo_NoAudioSelected;
      }
      set
      {
        this.SetProperty<string>(CreateVideoViewModel.SelectedSoundtrackLocationProperty, ref this.soundtrack, value, false);
      }
    }

    public static string SoundtrackNameProperty
    {
      get
      {
        return "SoundtrackName";
      }
    }

    public string SoundtrackName
    {
      get
      {
        return this.soundtrackName ?? Resources.CreateVideo_NoAudioSelected;
      }
      set
      {
        this.SetProperty<string>(CreateVideoViewModel.SoundtrackNameProperty, ref this.soundtrackName, value, false);
      }
    }

    public static string FadeInProperty
    {
      get
      {
        return "FadeIn";
      }
    }

    public bool FadeIn
    {
      get
      {
        return this.fadeIn;
      }
      set
      {
        this.SetProperty<bool>(CreateVideoViewModel.FadeInProperty, ref this.fadeIn, value, false);
      }
    }

    public static string FadeOutProperty
    {
      get
      {
        return "FadeOut";
      }
    }

    public bool FadeOut
    {
      get
      {
        return this.fadeOut;
      }
      set
      {
        this.SetProperty<bool>(CreateVideoViewModel.FadeOutProperty, ref this.fadeOut, value, false);
      }
    }

    public static string LoopProperty
    {
      get
      {
        return "Loop";
      }
    }

    public bool Loop
    {
      get
      {
        return this.loop;
      }
      set
      {
        this.SetProperty<bool>(CreateVideoViewModel.LoopProperty, ref this.loop, value, false);
      }
    }

    public string VideoDuration
    {
      get
      {
        return this.GetFormattedDuration((int) this.visualizationModel.CurrentTour.Duration);
      }
    }

    public static string AudioDurationProperty
    {
      get
      {
        return "SoundtrackDuration";
      }
    }

    public string SoundtrackDuration
    {
      get
      {
        return this.soundtrackDuration ?? Resources.CreateVideo_NoAudioSelected;
      }
      set
      {
        this.SetProperty<string>(CreateVideoViewModel.AudioDurationProperty, ref this.soundtrackDuration, value, false);
      }
    }

    public string DefaultFileName { get; set; }

    public CreateVideoViewModel(VisualizationModel visualizationModel, Dispatcher dispatcher, HostControlViewModel hostControlViewModel)
    {
      try
      {
        this.session = new TourVideoSession();
        this.visualizationModel = visualizationModel;
        this.dispatcher = dispatcher;
        this.hostControlViewModelOriginal = hostControlViewModel;
        this.SetDefaultSoundtrackConfiguration();
        this.CancelCommand = (ICommand) new DelegatedCommand((Action) (() => this.hostControlViewModelOriginal.DismissDialog((IDialog) this)));
        this.worker = new BackgroundWorker();
        this.worker.DoWork += new DoWorkEventHandler(this.ProcessVideo);
        this.worker.RunWorkerCompleted += (RunWorkerCompletedEventHandler) ((sender, args) =>
        {
          if (!this.cancelled)
          {
            if (this.failed)
            {
              if (this.fileNotFound)
                this.VideoProcessingStatus = Resources.CreateVideo_VideoFileMissingError;
              else if (this.encodingStatus == EncodingStatus.AudioSampleProcessingError)
                this.VideoProcessingStatus = Resources.CreateVideo_AudioEncodingErrorMessage;
              else if (this.encodingStatus == EncodingStatus.EncoderInitializationError || this.encodingStatus == EncodingStatus.VideoFrameProcessingError)
                this.VideoProcessingStatus = Resources.CreateVideo_StatusFailed;
            }
            else
              this.VideoProcessingStatus = Resources.CreateVideo_StatusCompleted;
            this.CancelCommand = (ICommand) new DelegatedCommand((Action) (() => this.hostControlViewModelOriginal.DismissDialog((IDialog) this)));
            if (!this.failed)
              this.CancelOrClose = Resources.Dialog_CloseText;
          }
          this.IsProcessingCompleted = !this.failed && !this.cancelled;
        });
        this.worker.WorkerReportsProgress = true;
        this.worker.WorkerSupportsCancellation = true;
        this.worker.ProgressChanged += new ProgressChangedEventHandler(this.WorkerOnProgressChanged);
        this.VideoSessionInitialized = true;
      }
      catch
      {
        VisualizationTraceSource.Current.Fail("Create Video encountered an error while initializing media foundation.");
      }
    }

    public bool TrySetSoundtrack(string selectedSoundtrack)
    {
      EncodingStatus status = EncodingStatus.Success;
      double duration = 0.0;
      this.dispatcher.Invoke((Action) (() => status = this.session.ConfigureSoundtrack(selectedSoundtrack, this.visualizationModel.CurrentTour.Duration, out duration)));
      if (status != EncodingStatus.Success && !string.IsNullOrEmpty(selectedSoundtrack))
        return false;
      this.SelectedSoundtrackLocation = selectedSoundtrack;
      if (!string.IsNullOrEmpty(selectedSoundtrack))
      {
        this.SoundtrackName = Path.GetFileName(this.soundtrack);
        this.SoundtrackDuration = this.GetFormattedDuration((int) duration);
        this.IsSoundtrackOptionSet = true;
      }
      else
      {
        this.SoundtrackDuration = (string) null;
        this.SoundtrackName = (string) null;
        this.IsSoundtrackOptionSet = false;
      }
      return true;
    }

    public void SetDefaultSoundtrackConfiguration()
    {
      this.TrySetSoundtrack((string) null);
      this.FadeIn = true;
      this.FadeOut = true;
      this.Loop = true;
    }

    public bool OpenFile()
    {
      if (this.IsProcessingCompleted)
      {
        try
        {
          if (File.Exists(this.SelectedFileName))
          {
            Process.Start(this.SelectedFileName);
          }
          else
          {
            int num = (int) MessageBox.Show(Resources.CreateVideo_OpenFailedDialogText, Resources.CreateVideo_OpenFailedCaption, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
          }
        }
        catch (Exception ex)
        {
          VisualizationTraceSource.Current.Fail(string.Format("Failed to open video file: {0}", (object) this.SelectedFileName), ex);
          return false;
        }
        this.hostControlViewModelOriginal.DismissDialog((IDialog) this);
      }
      return false;
    }

    public void StartVideoProcessing()
    {
      this.CancelCommand = (ICommand) new DelegatedCommand((Action) (() =>
      {
        bool isClosed = false;
        try
        {
          if (MessageBox.Show(Resources.CreateVideo_CancelDialogText, Resources.CreateVideo_CancelDialogCaption, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None) != MessageBoxResult.Yes || this.worker.CancellationPending || this.cancelled)
            return;
          if (this.worker.IsBusy)
          {
            this.worker.CancelAsync();
            this.VideoProcessingStatus = Resources.CreateVideo_StatusCancelling;
            Task.Factory.StartNew((Action) (() =>
            {
              try
              {
                this.cancellationEvent.WaitOne(5000);
              }
              catch (Exception ex)
              {
                VisualizationTraceSource.Current.Fail("Create Video encountered an error while cancelling: {0}. Ignoring exception.", ex);
              }
              finally
              {
                if (!isClosed && this.hostControlViewModelOriginal != null)
                  isClosed = this.hostControlViewModelOriginal.DismissDialog((IDialog) this);
              }
            }), TaskCreationOptions.LongRunning);
          }
          else
          {
            if (this.hostControlViewModelOriginal == null)
              return;
            this.hostControlViewModelOriginal.DismissDialog((IDialog) this);
          }
        }
        catch (Exception ex)
        {
          VisualizationTraceSource.Current.Fail("Create Video encountered an error while cancelling: {0}. Ignoring exception.", ex);
          if (isClosed || this.hostControlViewModelOriginal == null)
            return;
          isClosed = this.hostControlViewModelOriginal.DismissDialog((IDialog) this);
        }
      }));
      this.worker.RunWorkerAsync();
    }

    private void ProcessVideo(object sender, DoWorkEventArgs e)
    {
      VisualizationTraceSource.Current.TraceInformation("Starting video creation process at {0}.", (object) DateTime.Now);
      HostControlViewModel hostControlViewModel = (HostControlViewModel) null;
      Bitmap bitmap = (Bitmap) null;
      BitmapData data = (BitmapData) null;
      this.dispatcher.Invoke((Action) (() =>
      {
        int num1 = CreateVideoViewModel.resolutions[this.SelectedResolution].Item1;
        int num2 = CreateVideoViewModel.resolutions[this.SelectedResolution].Item2;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        try
        {
          int num3 = num1 == 640 ? 2 : 1;
          this.visualizationModel.Initialize(this.hostControlViewModelOriginal.Model.CustomMapProvider, this.hostControlViewModelOriginal.Model.BingMapResources, num1 * num3, num2 * num3, 30);
          hostControlViewModel = new HostControlViewModel(this.visualizationModel, (IHelpViewer) null, this.hostControlViewModelOriginal.BingMapResourceUri, this.hostControlViewModelOriginal.CustomColors);
          hostControlViewModel.SetTour(this.visualizationModel.CurrentTour);
          hostControlViewModel.OnExecutePlayTour();
          bitmap = new Bitmap(num1, num2);
          data = bitmap.LockBits(new Rectangle(0, 0, num1, num2), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
          VideoImageSource videoImageSource = new VideoImageSource(hostControlViewModel, data, this.dispatcher, (double) num3);
          this.encodingStatus = this.session.ConfigureVideoOptions(this.SelectedFileName, TourVideoSession.GetSupportedResolution(num1, num2), data, 30U);
          if (this.encodingStatus != EncodingStatus.Success)
          {
            this.failed = true;
          }
          else
          {
            Task<EncodingStatus> task = this.session.OverlaySoundtrackAsync(this.Loop, this.FadeIn, this.FadeOut, cancellationTokenSource.Token);
            double num4 = 30.0 * this.visualizationModel.CurrentTour.Duration;
            int num5 = 0;
            Stopwatch stopwatch = new Stopwatch();
            while (hostControlViewModel.TourPlayer.TourPlayer.IsPlaying && !this.worker.CancellationPending)
            {
              stopwatch.Start();
              if (!videoImageSource.Fill())
              {
                this.failed = true;
                return;
              }
              else
              {
                VisualizationTraceSource.Current.TraceInformation("Fetched Frame# {0} of {1} from engine in {2} ms.", (object) (num5 + 1), (object) num4, (object) stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Restart();
                if (num5 % 90 == 0 && (this.fileNotFound = !File.Exists(this.SelectedFileName)) || (this.encodingStatus = this.session.AddFrame()) == EncodingStatus.VideoFrameProcessingError)
                {
                  if (!task.IsCompleted)
                    cancellationTokenSource.Cancel();
                  this.failed = true;
                  return;
                }
                else if (task.IsCompleted && (this.encodingStatus = task.Result) == EncodingStatus.AudioSampleProcessingError)
                {
                  this.failed = true;
                  return;
                }
                else
                {
                  stopwatch.Stop();
                  VisualizationTraceSource.Current.TraceInformation("Wrote Frame# {0} of {1} to video stream in {2} ms.", (object) (num5 + 1), (object) num4, (object) stopwatch.Elapsed.TotalMilliseconds);
                  int num6 = (int) ((double) ++num5 * 100.0 / num4);
                  this.worker.ReportProgress(num6 < 100 ? num6 : 99);
                }
              }
            }
            if (this.worker.CancellationPending)
              return;
            task.Wait();
            this.session.Commit();
            this.worker.ReportProgress(100);
          }
        }
        catch (Exception ex)
        {
          cancellationTokenSource.Cancel();
          VisualizationTraceSource.Current.Fail(string.Format("Encountered an error while producing video at resolution {0}x{1}. Exception Message: {2}.", (object) num1, (object) num2, (object) ex.Message));
          this.failed = true;
        }
        finally
        {
          this.session.Dispose();
          this.session = (TourVideoSession) null;
        }
      }));
      DispatcherExtensions.DoEvents(this.dispatcher);
      this.dispatcher.Invoke((Action) (() =>
      {
        try
        {
          if (data != null && bitmap != null)
            bitmap.UnlockBits(data);
          if (hostControlViewModel != null)
          {
            hostControlViewModel.TourPlayer.ExitTourPlaybackModeCommand.Execute((object) null);
            hostControlViewModel.Dispose();
          }
          if (this.visualizationModel != null)
          {
            this.visualizationModel.Reset();
            this.visualizationModel.Dispose();
          }
          if (!this.worker.CancellationPending)
            return;
          this.cancelled = true;
          this.cancellationEvent.Set();
        }
        catch (Exception ex)
        {
          VisualizationTraceSource.Current.Fail(string.Format("Encountered an error while finalizing video processing. Exception Message: {0}.", (object) ex.Message));
        }
      }));
      VisualizationTraceSource.Current.TraceInformation("Finished video creation process at {0}.", (object) DateTime.Now);
    }

    private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
    {
      if (progressChangedEventArgs.ProgressPercentage <= this.PercentDone)
        return;
      this.PercentDone = progressChangedEventArgs.ProgressPercentage;
    }

    private string GetFormattedDuration(int seconds)
    {
      TimeSpan timeSpan = new TimeSpan(0, 0, 0, seconds);
      return string.Format((IFormatProvider) Resources.Culture, Resources.CreateVideo_MinSecFormat, new object[2]
      {
        (object) timeSpan.Minutes,
        (object) timeSpan.Seconds
      });
    }
  }
}
