using Microsoft.Data.Visualization.Engine;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class SettingsDialogViewModel : DialogViewModelBase
  {
    private CancellationTokenSource cts;
    private GraphicsLevel selectedGraphicsLevel;
    private long sizeOfCache;
    private bool isShrinkEnabled;

    public static string SelectedGraphicsLevelProperty
    {
      get
      {
        return "SelectedGraphicsLevel";
      }
    }

    public GraphicsLevel SelectedGraphicsLevel
    {
      get
      {
        return this.selectedGraphicsLevel;
      }
      set
      {
        this.SetProperty<GraphicsLevel>(SettingsDialogViewModel.SelectedGraphicsLevelProperty, ref this.selectedGraphicsLevel, value, false);
      }
    }

    public string LegalInfo
    {
      get
      {
        return string.Format(Resources.SettingsDialog_LegalInfoValue, (object) Resources.Culture);
      }
    }

    public static string SizeOfCacheProperty
    {
      get
      {
        return "SizeOfCache";
      }
    }

    public long SizeOfCache
    {
      get
      {
        this.sizeOfCache = TileCache.Size;
        int num = (int) this.sizeOfCache / 1024;
        this.IsShrinkEnabled = !this.IsShrinking && num > 0;
        return (long) num;
      }
      set
      {
        this.SetProperty<long>(SettingsDialogViewModel.SizeOfCacheProperty, ref this.sizeOfCache, value, false);
      }
    }

    public static string IsShrinkEnabledProperty
    {
      get
      {
        return "IsShrinkEnabled";
      }
    }

    public bool IsShrinkEnabled
    {
      get
      {
        return this.isShrinkEnabled;
      }
      set
      {
        this.SetProperty<bool>(SettingsDialogViewModel.IsShrinkEnabledProperty, ref this.isShrinkEnabled, value, false);
      }
    }

    public bool IsShrinking
    {
      get
      {
        return TileCache.IsShrinkTaskRunning;
      }
    }

    public bool StartedShrinkOp { get; set; }

    public SettingsDialogViewModel()
    {
      this.selectedGraphicsLevel = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.GraphicsLevel;
      this.Title = Resources.SettingsDialog_Title;
    }

    public void CancelShrink()
    {
      if (!this.StartedShrinkOp)
        return;
      this.cts.Cancel();
    }

    public async void ShrinkCache()
    {
      if (!this.IsShrinking)
      {
        this.IsShrinkEnabled = false;
        this.StartedShrinkOp = true;
        this.cts = new CancellationTokenSource();
        await TileCache.Shrink(this.sizeOfCache, 0L, this.cts.Token);
        this.StartedShrinkOp = false;
      }
      this.SizeOfCache = 0L;
    }

    public void SetGraphicsLevel(bool saveNew)
    {
      if (saveNew)
      {
        Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.GraphicsLevel = this.SelectedGraphicsLevel;
        Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.Save();
      }
      else
        this.SelectedGraphicsLevel = Microsoft.Data.Visualization.VisualizationControls.Properties.Settings.Default.GraphicsLevel;
    }
  }
}
