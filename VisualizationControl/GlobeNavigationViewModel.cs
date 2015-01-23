using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class GlobeNavigationViewModel : ViewModelBase
  {
    private bool _OnScreenNavigationVisible = true;
    private ICommand _RotateLeftCommand;
    private ICommand _RotateRightCommand;
    private ICommand _RotateUpCommand;
    private ICommand _RotateDownCommand;
    private ICommand _ZoomInCommand;
    private ICommand _ZoomOutCommand;
    private ICommand _ResetViewCommand;
    private ICommand _ZoomToSelectionCommand;
    private bool _CanExecuteZoomToSelectionCommand;

    public string PropertyOnScreenNavigationVisible
    {
      get
      {
        return "OnScreenNavigationVisible";
      }
    }

    public bool OnScreenNavigationVisible
    {
      get
      {
        return this._OnScreenNavigationVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyOnScreenNavigationVisible, ref this._OnScreenNavigationVisible, value, false);
      }
    }

    public string PropertyRotateLeftCommand
    {
      get
      {
        return "RotateLeftCommand";
      }
    }

    public ICommand RotateLeftCommand
    {
      get
      {
        return this._RotateLeftCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyRotateLeftCommand, ref this._RotateLeftCommand, value, false);
      }
    }

    public string PropertyRotateRightCommand
    {
      get
      {
        return "RotateRightCommand";
      }
    }

    public ICommand RotateRightCommand
    {
      get
      {
        return this._RotateRightCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyRotateRightCommand, ref this._RotateRightCommand, value, false);
      }
    }

    public string PropertyRotateUpCommand
    {
      get
      {
        return "RotateUpCommand";
      }
    }

    public ICommand RotateUpCommand
    {
      get
      {
        return this._RotateUpCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyRotateUpCommand, ref this._RotateUpCommand, value, false);
      }
    }

    public string PropertyRotateDownCommand
    {
      get
      {
        return "RotateDownCommand";
      }
    }

    public ICommand RotateDownCommand
    {
      get
      {
        return this._RotateDownCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyRotateDownCommand, ref this._RotateDownCommand, value, false);
      }
    }

    public string PropertyZoomInCommand
    {
      get
      {
        return "ZoomInCommand";
      }
    }

    public ICommand ZoomInCommand
    {
      get
      {
        return this._ZoomInCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyZoomInCommand, ref this._ZoomInCommand, value, false);
      }
    }

    public string PropertyZoomOutCommand
    {
      get
      {
        return "ZoomOutCommand";
      }
    }

    public ICommand ZoomOutCommand
    {
      get
      {
        return this._ZoomOutCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyZoomOutCommand, ref this._ZoomOutCommand, value, false);
      }
    }

    public string PropertyResetViewCommand
    {
      get
      {
        return "ResetViewCommand";
      }
    }

    public ICommand ResetViewCommand
    {
      get
      {
        return this._ResetViewCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyResetViewCommand, ref this._ResetViewCommand, value, false);
      }
    }

    public string PropertyZoomToSelectionCommand
    {
      get
      {
        return "ZoomToSelectionCommand";
      }
    }

    public ICommand ZoomToSelectionCommand
    {
      get
      {
        return this._ZoomToSelectionCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyZoomToSelectionCommand, ref this._ZoomToSelectionCommand, value, false);
      }
    }

    public string PropertyCanExecuteZoomToSelectionCommand
    {
      get
      {
        return "CanExecuteZoomToSelectionCommand";
      }
    }

    public bool CanExecuteZoomToSelectionCommand
    {
      get
      {
        return this._CanExecuteZoomToSelectionCommand;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyCanExecuteZoomToSelectionCommand, ref this._CanExecuteZoomToSelectionCommand, value, false);
      }
    }

    public GlobeNavigationViewModel(VisualizationEngine visualizationEngine)
    {
      if (visualizationEngine == null)
        throw new ArgumentNullException("visualizationEngine");
      this.RotateLeftCommand = (ICommand) new DelegatedCommand((Action) (() => visualizationEngine.Rotate(CameraRotation.Left)));
      this.RotateRightCommand = (ICommand) new DelegatedCommand((Action) (() => visualizationEngine.Rotate(CameraRotation.Right)));
      this.RotateUpCommand = (ICommand) new DelegatedCommand((Action) (() => visualizationEngine.Rotate(CameraRotation.Up)));
      this.RotateDownCommand = (ICommand) new DelegatedCommand((Action) (() => visualizationEngine.Rotate(CameraRotation.Down)));
      this.ZoomInCommand = (ICommand) new DelegatedCommand((Action) (() => visualizationEngine.ZoomIn()));
      this.ZoomOutCommand = (ICommand) new DelegatedCommand((Action) (() => visualizationEngine.ZoomOut()));
      this.ResetViewCommand = (ICommand) new DelegatedCommand((Action) (() => visualizationEngine.ResetView()));
      this.ZoomToSelectionCommand = (ICommand) new DelegatedCommand((Action) (() => visualizationEngine.ZoomToSelection()));
    }
  }
}
