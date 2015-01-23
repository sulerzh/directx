using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DecoratorViewModel : ViewModelBase
  {
    private DecoratorModel _Model = new DecoratorModel();
    private bool _ShowAdornerUI;
    private ResizeSource _ResizeSource;

    public string PropertyShowAdornerUI
    {
      get
      {
        return "ShowAdornerUI";
      }
    }

    public bool ShowAdornerUI
    {
      get
      {
        return this._ShowAdornerUI;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyShowAdornerUI, ref this._ShowAdornerUI, value, false);
      }
    }

    public string PropertyResizeSource
    {
      get
      {
        return "ResizeSource";
      }
    }

    public ResizeSource ResizeSource
    {
      get
      {
        return this._ResizeSource;
      }
      set
      {
        this.SetProperty<ResizeSource>(this.PropertyResizeSource, ref this._ResizeSource, value, false);
      }
    }

    public ValidateNewSizeHandler ValidateNewSizeCallback { get; set; }

    public ICommand EditCommand { get; private set; }

    public Action<DecoratorViewModel> EditCommandCallback { get; set; }

    public Predicate<DecoratorViewModel> CanEditCommandCallback { get; set; }

    public ICommand CloseCommand { get; private set; }

    public Action<DecoratorViewModel> CloseCommandCallback { get; set; }

    public ICommand IncrementZOrderCommand { get; private set; }

    public Action<DecoratorViewModel> IncrementZOrderCommandCallback { get; set; }

    public ICommand DecrementZOrderCommand { get; private set; }

    public Action<DecoratorViewModel> DecrementZOrderCommandCallback { get; set; }

    public ICommand TopZOrderCommand { get; private set; }

    public Action<DecoratorViewModel> TopZOrderCommandCallback { get; set; }

    public ICommand BottomZOrderCommand { get; private set; }

    public Action<DecoratorViewModel> BottomZOrderCommandCallback { get; set; }

    public ObservableCollectionEx<ContextCommand> ContextCommands { get; private set; }

    public string PropertyModel
    {
      get
      {
        return "Model";
      }
    }

    public DecoratorModel Model
    {
      get
      {
        return this._Model;
      }
      set
      {
        this.SetProperty<DecoratorModel>(this.PropertyModel, ref this._Model, value, false);
      }
    }

    public event DragDeltaEventHandler OnDragged;

    public DecoratorViewModel()
    {
      this.IncrementZOrderCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteIncrementZOrderCommand));
      this.DecrementZOrderCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteDecrementZOrderCommand));
      this.TopZOrderCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteTopZOrderCommand));
      this.BottomZOrderCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteBottomZOrderCommand));
      this.EditCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteEditCommand), new Predicate(this.CanExecuteEditCommand));
      this.CloseCommand = (ICommand) new DelegatedCommand(new Action(this.OnExecuteCloseCommand));
      this.ContextCommands = new ObservableCollectionEx<ContextCommand>();
      this.ContextCommands.Add(new ContextCommand(Resources.DecoratorCommand_BringForward, this.IncrementZOrderCommand));
      this.ContextCommands.Add(new ContextCommand(Resources.DecoratorCommand_SendBack, this.DecrementZOrderCommand));
      this.ContextCommands.Add(new ContextCommand(Resources.DecoratorCommand_BringToFront, this.TopZOrderCommand));
      this.ContextCommands.Add(new ContextCommand(Resources.DecoratorCommand_SendToBack, this.BottomZOrderCommand));
      this.ContextCommands.Add((ContextCommand) null);
      this.ContextCommands.Add(new ContextCommand(Resources.DecoratorCommand_Edit, this.EditCommand));
      this.ContextCommands.Add(new ContextCommand(Resources.DecoratorCommand_Remove, this.CloseCommand));
    }

    private void OnExecuteEditCommand()
    {
      if (this.EditCommandCallback == null)
        return;
      this.EditCommandCallback(this);
    }

    private bool CanExecuteEditCommand()
    {
      if (this.CanEditCommandCallback == null)
        return false;
      else
        return this.CanEditCommandCallback(this);
    }

    private void OnExecuteCloseCommand()
    {
      if (this.CloseCommandCallback == null)
        return;
      this.CloseCommandCallback(this);
    }

    private void OnExecuteIncrementZOrderCommand()
    {
      if (this.IncrementZOrderCommandCallback == null)
        return;
      this.IncrementZOrderCommandCallback(this);
    }

    private void OnExecuteDecrementZOrderCommand()
    {
      if (this.DecrementZOrderCommandCallback == null)
        return;
      this.DecrementZOrderCommandCallback(this);
    }

    private void OnExecuteTopZOrderCommand()
    {
      if (this.TopZOrderCommandCallback == null)
        return;
      this.TopZOrderCommandCallback(this);
    }

    private void OnExecuteBottomZOrderCommand()
    {
      if (this.BottomZOrderCommandCallback == null)
        return;
      this.BottomZOrderCommandCallback(this);
    }

    public void UpdateDragDelta(double xDelta, double yDelta)
    {
      this.Model.X += xDelta;
      this.Model.Y += yDelta;
      if (this.OnDragged == null)
        return;
      this.OnDragged(this, xDelta, yDelta);
    }
  }
}
