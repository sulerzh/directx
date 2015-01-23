using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class WindowViewModel : ViewModelBase, IDialogServiceProvider
  {
    private ResizeMode _ResizeMode = ResizeMode.CanResize;
    private double _Height = double.NaN;
    private double _MinWidth = double.NaN;
    private bool _ChromeBarVisible = true;
    private Cursor _Cursor = Cursors.Arrow;
    protected List<Window> _childWindows = new List<Window>();
    private Window _Window;
    private string _Title;
    private double _Width;
    private double _MinHeight;
    private object _Dialog;
    private bool _UndoRedoVisible;
    private bool _AppIconVisible;
    private ICommand _FullScreenCommand;
    private ICommand _HelpCommand;
    private ICommand _MaximizeCommand;
    private ICommand _MinimizeCommand;
    private ICommand _CloseCommand;
    private ICommand _EscapeCommand;
    private ICommand _UndoCommand;
    private ICommand _RedoCommand;
    private bool _FullScreenMode;
    private WindowState _oldWindowState;
    private bool isInternalClosing;

    public Window Window
    {
      get
      {
        return this._Window;
      }
      set
      {
        if (this._Window != null)
          this._Window.Closing -= new CancelEventHandler(this.Window_Closing);
        if (value != null)
          value.Closing += new CancelEventHandler(this.Window_Closing);
        this.isInternalClosing = false;
        this._Window = value;
      }
    }

    public string PropertyTitle
    {
      get
      {
        return "Title";
      }
    }

    public string Title
    {
      get
      {
        return this._Title;
      }
      set
      {
        this.SetProperty<string>(this.PropertyTitle, ref this._Title, value, false);
      }
    }

    public string PropertyResizeMode
    {
      get
      {
        return "ResizeMode";
      }
    }

    public ResizeMode ResizeMode
    {
      get
      {
        return this._ResizeMode;
      }
      set
      {
        this.SetProperty<ResizeMode>(this.PropertyResizeMode, ref this._ResizeMode, value, false);
      }
    }

    public string PropertyWidth
    {
      get
      {
        return "Width";
      }
    }

    public double Width
    {
      get
      {
        return this._Width;
      }
      set
      {
        this.SetProperty<double>(this.PropertyWidth, ref this._Width, value, false);
      }
    }

    public string PropertyHeight
    {
      get
      {
        return "Height";
      }
    }

    public double Height
    {
      get
      {
        return this._Height;
      }
      set
      {
        this.SetProperty<double>(this.PropertyHeight, ref this._Height, value, false);
      }
    }

    public string PropertyMinWidth
    {
      get
      {
        return "MinWidth";
      }
    }

    public double MinWidth
    {
      get
      {
        return this._MinWidth;
      }
      set
      {
        this.SetProperty<double>(this.PropertyMinWidth, ref this._MinWidth, value, false);
      }
    }

    public string PropertyMinHeight
    {
      get
      {
        return "MinHeight";
      }
    }

    public double MinHeight
    {
      get
      {
        return this._MinHeight;
      }
      set
      {
        this.SetProperty<double>(this.PropertyMinHeight, ref this._MinHeight, value, false);
      }
    }

    public static string PropertyDialog
    {
      get
      {
        return "Dialog";
      }
    }

    public object Dialog
    {
      get
      {
        return this._Dialog;
      }
      set
      {
        this.SetProperty<object>(WindowViewModel.PropertyDialog, ref this._Dialog, value, false);
      }
    }

    public string PropertyUndoRedoVisible
    {
      get
      {
        return "UndoRedoVisible";
      }
    }

    public bool UndoRedoVisible
    {
      get
      {
        return this._UndoRedoVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyUndoRedoVisible, ref this._UndoRedoVisible, value, false);
      }
    }

    public string PropertyAppIconVisible
    {
      get
      {
        return "AppIconVisible";
      }
    }

    public bool AppIconVisible
    {
      get
      {
        return this._AppIconVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyAppIconVisible, ref this._AppIconVisible, value, false);
      }
    }

    public string PropertyFullScreenCommand
    {
      get
      {
        return "FullScreenCommand";
      }
    }

    public ICommand FullScreenCommand
    {
      get
      {
        return this._FullScreenCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyFullScreenCommand, ref this._FullScreenCommand, value, false);
      }
    }

    public string PropertyHelpCommand
    {
      get
      {
        return "HelpCommand";
      }
    }

    public ICommand HelpCommand
    {
      get
      {
        return this._HelpCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyHelpCommand, ref this._HelpCommand, value, false);
      }
    }

    public string PropertyMaximizeCommand
    {
      get
      {
        return "MaximizeCommand";
      }
    }

    public ICommand MaximizeCommand
    {
      get
      {
        return this._MaximizeCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyMaximizeCommand, ref this._MaximizeCommand, value, false);
      }
    }

    public string PropertyMinimizeCommand
    {
      get
      {
        return "MinimizeCommand";
      }
    }

    public ICommand MinimizeCommand
    {
      get
      {
        return this._MinimizeCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyMinimizeCommand, ref this._MinimizeCommand, value, false);
      }
    }

    public string PropertyCloseCommand
    {
      get
      {
        return "CloseCommand";
      }
    }

    public ICommand CloseCommand
    {
      get
      {
        return this._CloseCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyCloseCommand, ref this._CloseCommand, value, false);
      }
    }

    public string PropertyEscapeCommand
    {
      get
      {
        return "EscapeCommand";
      }
    }

    public ICommand EscapeCommand
    {
      get
      {
        return this._EscapeCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyEscapeCommand, ref this._EscapeCommand, value, false);
      }
    }

    public string PropertyUndoCommand
    {
      get
      {
        return "UndoCommand";
      }
    }

    public ICommand UndoCommand
    {
      get
      {
        return this._UndoCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyUndoCommand, ref this._UndoCommand, value, false);
      }
    }

    public string PropertyRedoCommand
    {
      get
      {
        return "RedoCommand";
      }
    }

    public ICommand RedoCommand
    {
      get
      {
        return this._RedoCommand;
      }
      set
      {
        this.SetProperty<ICommand>(this.PropertyRedoCommand, ref this._RedoCommand, value, false);
      }
    }

    public string PropertyChromeBarVisible
    {
      get
      {
        return "ChromeBarVisible";
      }
    }

    public bool ChromeBarVisible
    {
      get
      {
        return this._ChromeBarVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyChromeBarVisible, ref this._ChromeBarVisible, value, false);
      }
    }

    public string PropertyCursor
    {
      get
      {
        return "Cursor";
      }
    }

    public Cursor Cursor
    {
      get
      {
        return this._Cursor;
      }
      set
      {
        this.SetProperty<Cursor>(this.PropertyCursor, ref this._Cursor, value, false);
      }
    }

    public string PropertyFullScreenMode
    {
      get
      {
        return "FullScreenMode";
      }
    }

    public bool FullScreenMode
    {
      get
      {
        return this._FullScreenMode;
      }
      set
      {
        base.SetProperty<bool>(this.PropertyFullScreenMode, ref this._FullScreenMode, value, new Action(this.OnFullScreenModeChanged));
      }
    }

    protected bool TakeEntireScreenInNextFullScreen { get; set; }

    public WindowViewModel()
    {
      this.CloseCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteClose));
      this.MaximizeCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteMaximize));
      this.MinimizeCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteMinimize));
      this.EscapeCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteEscape));
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      this.CloseDialogPopups();
      if (this.isInternalClosing)
        return;
      this.OnBeforeClose();
    }

    protected virtual void OnBeforeClose()
    {
      foreach (Window window in new List<Window>((IEnumerable<Window>) this._childWindows))
        window.Close();
      this._childWindows.Clear();
    }

    protected virtual void OnExecuteClose()
    {
      this.isInternalClosing = true;
      this.OnBeforeClose();
      if (this.Window == null)
        return;
      this.Window.Close();
    }

    protected virtual void OnExecuteMaximize()
    {
      if (this.Window == null)
        return;
      if (this.Window.WindowState == WindowState.Maximized)
        this.Window.WindowState = WindowState.Normal;
      else
        this.Window.WindowState = WindowState.Maximized;
    }

    private void OnFullScreenModeChanged()
    {
      if (this.Window == null)
        return;
      if (this.FullScreenMode)
      {
        this.ResizeMode = ResizeMode.NoResize;
        this._oldWindowState = this.Window.WindowState;
        if (this.TakeEntireScreenInNextFullScreen)
        {
          this.TakeEntireScreenInNextFullScreen = false;
          this.Window.WindowState = WindowState.Normal;
        }
        this.Window.WindowState = WindowState.Maximized;
      }
      else
      {
        this.ResizeMode = ResizeMode.CanResize;
        this.Window.WindowState = this._oldWindowState;
      }
    }

    protected virtual void OnExecuteMinimize()
    {
      if (this.Window == null)
        return;
      this.Window.WindowState = WindowState.Minimized;
    }

    protected virtual void OnExecuteEscape()
    {
    }

    protected void BusyOperation(Action action)
    {
      if (action == null)
        return;
      this.Cursor = Cursors.Wait;
      action();
      this.Cursor = Cursors.Arrow;
    }

    protected virtual void CloseDialogPopups()
    {
    }

    public bool ShowDialog(IDialog dialog)
    {
      if (this.Dialog != dialog)
        this.CloseDialogPopups();
      if (dialog == null)
        return false;
      if (dialog.CancelCommand == null)
        dialog.CancelCommand = (ICommand) new DelegatedCommand((Action) (() => this.DismissDialog(dialog)))
        {
          Name = Resources.Dialog_CancelText
        };
      if (dialog == null || this.Dialog != null)
        return false;
      this.Dialog = (object) dialog;
      return true;
    }

    public bool DismissDialog(IDialog dialog)
    {
      if (dialog == null || this.Dialog != dialog)
        return false;
      this.Dialog = (object) null;
      return true;
    }
  }
}
