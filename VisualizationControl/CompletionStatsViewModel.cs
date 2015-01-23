using Microsoft.Data.Visualization.WpfExtensions;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class CompletionStatsViewModel : ViewModelBase
  {
    private string _OperationText;
    private bool _ProgressBarVisible;
    private int _Completed;
    private int _Requested;

    public string PropertyOperationText
    {
      get
      {
        return "OperationText";
      }
    }

    public string OperationText
    {
      get
      {
        return this._OperationText;
      }
      set
      {
        this.SetProperty<string>(this.PropertyOperationText, ref this._OperationText, value, false);
      }
    }

    public string PropertyProgressBarVisible
    {
      get
      {
        return "ProgressBarVisible";
      }
    }

    public bool ProgressBarVisible
    {
      get
      {
        return this._ProgressBarVisible;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyProgressBarVisible, ref this._ProgressBarVisible, value, false);
      }
    }

    public string PropertyCompleted
    {
      get
      {
        return "Completed";
      }
    }

    public int Completed
    {
      get
      {
        return this._Completed;
      }
      set
      {
        this.SetProperty<int>(this.PropertyCompleted, ref this._Completed, value, false);
      }
    }

    public string PropertyRequested
    {
      get
      {
        return "Requested";
      }
    }

    public int Requested
    {
      get
      {
        return this._Requested;
      }
      set
      {
        this.SetProperty<int>(this.PropertyRequested, ref this._Requested, value, false);
      }
    }

    public CompletionStats Model { get; private set; }

    public CompletionStatsViewModel(CompletionStats model)
    {
      this.Model = model;
      if (this.Model == null)
        return;
      this.Model.PropertyChanged += new PropertyChangedEventHandler(this.Model_PropertyChanged);
      this.RefreshState();
    }

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == this.Model.PropertyCompleted) && !(e.PropertyName == this.Model.PropertyPending) && (!(e.PropertyName == this.Model.PropertyRequested) && !(e.PropertyName == this.Model.PropertyRegionsRequested)) && !(e.PropertyName == this.Model.PropertyRegionsCompleted))
        return;
      this.RefreshState();
    }

    private void RefreshState()
    {
      if (this.Model == null)
        return;
      if (this.Model.Pending)
      {
        this.ProgressBarVisible = false;
        this.OperationText = Resources.StatusBar_ResolvePending;
      }
      else if (this.Model.Failed)
      {
        this.OperationText = Resources.StatusBar_ResolveFailed;
        this.ProgressBarVisible = false;
      }
      else if (this.Model.Cancelled)
      {
        this.OperationText = Resources.StatusBar_ResolveCancelled;
        this.ProgressBarVisible = false;
      }
      else
      {
        int num1;
        int num2;
        if (this.Model.RegionsRequested > 0)
        {
          num1 = this.Model.RegionsCompleted;
          num2 = this.Model.RegionsRequested;
        }
        else
        {
          num1 = this.Model.Completed;
          num2 = this.Model.Requested;
        }
        this.Completed = num1;
        this.Requested = num2;
        this.ProgressBarVisible = num1 < num2;
        if (this.ProgressBarVisible)
          this.OperationText = string.Format(Resources.StatusBar_Resolving, (object) num1, (object) num2);
        else
          this.OperationText = Resources.StatusBar_DoneResolving;
      }
    }
  }
}
