using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  internal class CustomSpaceSettingsViewModel : DialogViewModelBase
  {
    private string cancelOrClose = Resources.Dialog_CancelText;
    private readonly HostControlViewModel hostControlViewModelOriginal;
    private readonly CustomMap customMap;
    private CustomSpaceDefinition mLocalCustomSpace;
    private bool hasRegistered;
    private bool mAlreadyDismissing;
    public Action CustomFinalClosedCommand;

    public ICommand AutoFitCommand { get; private set; }

    public ICommand PixelSpaceCommand { get; private set; }

    public ICommand ApplyCommand { get; private set; }

    public ICommand ImageSelectCommand { get; private set; }

    public CustomMap CustomMap
    {
      get
      {
        return this.customMap;
      }
    }

    public CustomSpaceDefinition LocalCustomSpace
    {
      get
      {
        return this.mLocalCustomSpace;
      }
      set
      {
        this.SetProperty<CustomSpaceDefinition>("LocalCustomSpace", ref this.mLocalCustomSpace, value, false);
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
        this.SetProperty<string>(CustomSpaceSettingsViewModel.CancelOrCloseProperty, ref this.cancelOrClose, value, false);
      }
    }

    public CustomSpaceSettingsViewModel(HostControlViewModel hostControlViewModel, CustomMap theCustomMap)
    {
      this.customMap = theCustomMap;
      this.customMap.EnsureUpdatedOnUIThread();
      this.mLocalCustomSpace = this.customMap.CustomSpaceDef.Clone();
      this.hostControlViewModelOriginal = hostControlViewModel;
      this.CancelCommand = (ICommand) new DelegatedCommand(new Action(this.ApplyAndDismiss));
      this.ApplyCommand = (ICommand) new DelegatedCommand((Action) (() => this.ApplyAndRefreshLocalChanges(false)));
      this.AutoFitCommand = (ICommand) new DelegatedCommand(new Action(this.OnAutoFitCalibration));
      this.PixelSpaceCommand = (ICommand) new DelegatedCommand(new Action(this.OnPixelSpaceCalibration));
      this.ImageSelectCommand = (ICommand) new DelegatedCommand(new Action(this.OnImageSelectionFlow));
      this.customMap.LatestAutoDefUpdated += new Action<CustomSpaceDefinition>(this.CustomMap_LatestAutoDefUpdated);
      this.hasRegistered = true;
    }

    public void OnAutoFitCalibration()
    {
      this.customMap.ResetToAutoFit();
      this.LocalCustomSpace = this.CustomMap.CustomSpaceDef.Clone();
      this.ApplyAndRefreshLocalChanges(true);
      this.ResetUndoStack();
    }

    public void OnPixelSpaceCalibration()
    {
      BitmapSource imageForUi = this.customMap.ImageForUI;
      if (imageForUi == null)
        return;
      this.LocalCustomSpace.AxisX = new CustomSpaceAxisDefinition(0.0, (double) imageForUi.PixelWidth);
      this.LocalCustomSpace.AxisY = new CustomSpaceAxisDefinition(0.0, (double) imageForUi.PixelHeight);
      this.LocalCustomSpace.AxisY.IsAxisFlipped = true;
      this.LocalCustomSpace.IsAnyAutoCalculated = false;
      this.LocalCustomSpace.IsCalibrateOnFirst = false;
      this.LocalCustomSpace.IsSwapXandY = false;
      this.ApplyAndRefreshLocalChanges(false);
      this.ResetUndoStack();
    }

    private void UpdateFromAutoDefintions(CustomSpaceDefinition csd)
    {
      if (csd != null && this.LocalCustomSpace.IsCalibrateOnFirst)
      {
        this.LocalCustomSpace.AxisX.AxisRange = csd.AxisX.AxisRange.Clone();
        this.LocalCustomSpace.AxisY.AxisRange = csd.AxisY.AxisRange.Clone();
        this.LocalCustomSpace.IsCalibrateOnFirst = false;
      }
      this.CustomMap.EnsureUpdatedOnUIThread();
    }

    private void CustomMap_LatestAutoDefUpdated(CustomSpaceDefinition obj)
    {
      VisualizationModel model = this.hostControlViewModelOriginal.Model;
      if (model == null || model.UIDispatcher == null)
        return;
      model.UIDispatcher.Invoke((Action) (() => this.UpdateFromAutoDefintions(obj)));
    }

    public void UnregisterMapCallbacks()
    {
      if (!this.hasRegistered)
        return;
      this.hasRegistered = false;
      this.customMap.LatestAutoDefUpdated -= new Action<CustomSpaceDefinition>(this.CustomMap_LatestAutoDefUpdated);
    }

    public void ResetUndoStack()
    {
      this.hostControlViewModelOriginal.ResetUndoStack();
    }

    public void ApplyAndDismiss()
    {
      if (this.mAlreadyDismissing)
        return;
      this.mAlreadyDismissing = true;
      this.ApplyAndRefreshLocalChanges(false);
      this.UnregisterMapCallbacks();
      if (this.CustomFinalClosedCommand == null)
        this.hostControlViewModelOriginal.DismissDialog((IDialog) this);
      else
        this.CustomFinalClosedCommand();
    }

    public void ApplyAndRefreshLocalChanges(bool forceUpdate = false)
    {
      this.CustomMap.EnsureUpdatedOnUIThread();
      if (!this.LocalCustomSpace.Equals(this.CustomMap.CustomSpaceDef) || forceUpdate)
      {
        bool flag = true;
        if (!forceUpdate)
        {
          if (!this.LocalCustomSpace.AreRangesEqual(this.CustomMap.CustomSpaceDef))
            this.LocalCustomSpace.IsCalibrateOnFirst = false;
          if (this.LocalCustomSpace.IsLocked)
            this.LocalCustomSpace.IsCalibrateOnFirst = false;
          if (this.LocalCustomSpace.IsLocked != this.CustomMap.CustomSpaceDef.IsLocked)
          {
            CustomSpaceDefinition customSpaceDefinition = this.LocalCustomSpace.Clone();
            customSpaceDefinition.IsLocked = this.CustomMap.CustomSpaceDef.IsLocked;
            if (customSpaceDefinition.Equals(this.CustomMap.CustomSpaceDef))
              flag = false;
          }
        }
        this.CustomMap.CustomSpaceDef = this.LocalCustomSpace.Clone();
        this.CustomMap.IsTemporary = false;
        if (flag)
          this.RefreshDisplayedData();
        this.ResetUndoStack();
      }
      this.EnsureRenderUpdate();
    }

    public void OnImageSelectionFlow()
    {
      string displayFilename;
      BitmapImage img = CustomSpaceSettingsViewModel.UserSelectBackgroundImage(out displayFilename);
      if (img == null)
        return;
      this.ChangeMapImage(img, displayFilename);
    }

    public void ChangeMapImage(BitmapImage img, string displayName)
    {
      this.CustomMap.SetImageFromUI(img, displayName);
      this.RefreshDisplayedData();
      this.EnsureRenderUpdate();
      this.ResetUndoStack();
    }

    public void EnsureRenderUpdate()
    {
      this.hostControlViewModelOriginal.Model.Engine.ForceUpdate();
    }

    public static BitmapImage UserSelectBackgroundImage(out string displayFilename)
    {
      displayFilename = Resources.CustomSpace_DefaultImageTitle;
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      openFileDialog1.Title = Resources.CustomSpaceSettings_SelectImageTitle;
      openFileDialog1.AddExtension = true;
      openFileDialog1.Filter = string.Format(Resources.CustomSpaceSettings_SelectImageFilter, (object) Resources.CustomSpaceSettings_SelectImageTypes);
      OpenFileDialog openFileDialog2 = openFileDialog1;
      if (openFileDialog2.ShowDialog() == DialogResult.OK)
      {
        try
        {
          BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog2.FileName));
          if (bitmapImage.PixelHeight > 4096 || bitmapImage.PixelWidth > 4096)
          {
            int num = (int) System.Windows.MessageBox.Show(Resources.CustomSpaceSettings_SelectImageErrorTooBig, Resources.CustomSpaceSettings_SelectImageErrorTitle);
            bitmapImage = (BitmapImage) null;
          }
          if (bitmapImage != null)
            displayFilename = Path.GetFileName(openFileDialog2.FileName);
          return bitmapImage;
        }
        catch (Exception ex)
        {
          int num = (int) System.Windows.MessageBox.Show(Resources.CustomSpaceSettings_SelectImageErrorCouldntLoad, Resources.CustomSpaceSettings_SelectImageErrorTitle);
        }
      }
      return (BitmapImage) null;
    }

    private void RefreshDisplayedData()
    {
      VisualizationModel model = this.hostControlViewModelOriginal.Model;
      LayerManager layerManager = model == null ? (LayerManager) null : model.LayerManager;
      if (layerManager == null)
        return;
      layerManager.ForciblyRefreshDisplay(false);
    }
  }
}
