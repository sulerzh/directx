using Microsoft.Data.Visualization.VisualizationCommon;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class CompletionStats : PropertyChangedNotificationBase
  {
    private object _writerLock = new object();
    private bool _Pending = true;
    public const int UnknownRequestCount = -1;
    private bool _Cancelled;
    private bool _Failed;
    private int _Requested;
    private int _Completed;
    private int _RegionsRequested;
    private int _RegionsCompleted;
    private int _Resolved;
    private int _NotFound;
    private int _InvalidArgs;
    private int _QueryFailed;
    private int _AmbiguousUnresolved;
    private int _Ambiguous;
    private int _AmbiguousResolved;
    private int _CacheHits;
    private int _BingQueries;
    private int _BingRetries;
    private int _ExceptionsQueryingBing;

    public object WriterLock
    {
      get
      {
        return this._writerLock;
      }
    }

    public string PropertyPending
    {
      get
      {
        return "Pending";
      }
    }

    public bool Pending
    {
      get
      {
        return this._Pending;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyPending, ref this._Pending, value);
      }
    }

    public string PropertyCancelled
    {
      get
      {
        return "Cancelled";
      }
    }

    public bool Cancelled
    {
      get
      {
        return this._Cancelled;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyCancelled, ref this._Cancelled, value);
      }
    }

    public string PropertyFailed
    {
      get
      {
        return "Failed";
      }
    }

    public bool Failed
    {
      get
      {
        return this._Failed;
      }
      set
      {
        lock (this.WriterLock)
        {
          if (!value || value == this._Failed || this.Cancelled)
            return;
          this.SetProperty<bool>(this.PropertyFailed, ref this._Failed, value);
          this.Pending = false;
        }
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
        this.SetProperty<int>(this.PropertyRequested, ref this._Requested, value);
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
        this.SetProperty<int>(this.PropertyCompleted, ref this._Completed, value);
      }
    }

    public string PropertyRegionsRequested
    {
      get
      {
        return "RegionsRequested";
      }
    }

    public int RegionsRequested
    {
      get
      {
        return this._RegionsRequested;
      }
      set
      {
        this.SetProperty<int>(this.PropertyRegionsRequested, ref this._RegionsRequested, value);
      }
    }

    public string PropertyRegionsCompleted
    {
      get
      {
        return "RegionsCompleted";
      }
    }

    public int RegionsCompleted
    {
      get
      {
        return this._RegionsCompleted;
      }
      set
      {
        this.SetProperty<int>(this.PropertyRegionsCompleted, ref this._RegionsCompleted, value);
      }
    }

    public string PropertyResolved
    {
      get
      {
        return "Resolved";
      }
    }

    public int Resolved
    {
      get
      {
        return this._Resolved;
      }
      set
      {
        this.SetProperty<int>(this.PropertyResolved, ref this._Resolved, value);
      }
    }

    public string PropertyNotFound
    {
      get
      {
        return "NotFound";
      }
    }

    public int NotFound
    {
      get
      {
        return this._NotFound;
      }
      set
      {
        this.SetProperty<int>(this.PropertyNotFound, ref this._NotFound, value);
      }
    }

    public string PropertyInvalidArgs
    {
      get
      {
        return "InvalidArgs";
      }
    }

    public int InvalidArgs
    {
      get
      {
        return this._InvalidArgs;
      }
      set
      {
        this.SetProperty<int>(this.PropertyInvalidArgs, ref this._InvalidArgs, value);
      }
    }

    public string PropertyQueryFailed
    {
      get
      {
        return "QueryFailed";
      }
    }

    public int QueryFailed
    {
      get
      {
        return this._QueryFailed;
      }
      set
      {
        this.SetProperty<int>(this.PropertyQueryFailed, ref this._QueryFailed, value);
      }
    }

    public string PropertyAmbiguousUnresolved
    {
      get
      {
        return "AmbiguousUnresolved";
      }
    }

    public int AmbiguousUnresolved
    {
      get
      {
        return this._AmbiguousUnresolved;
      }
      set
      {
        this.SetProperty<int>(this.PropertyAmbiguousUnresolved, ref this._AmbiguousUnresolved, value);
      }
    }

    public string PropertyAmbiguous
    {
      get
      {
        return "Ambiguous";
      }
    }

    public int Ambiguous
    {
      get
      {
        return this._Ambiguous;
      }
      set
      {
        this.SetProperty<int>(this.PropertyAmbiguous, ref this._Ambiguous, value);
      }
    }

    public string PropertyAmbiguousResolved
    {
      get
      {
        return "AmbiguousResolved";
      }
    }

    public int AmbiguousResolved
    {
      get
      {
        return this._AmbiguousResolved;
      }
      set
      {
        this.SetProperty<int>(this.PropertyAmbiguousResolved, ref this._AmbiguousResolved, value);
      }
    }

    public string PropertyCacheHits
    {
      get
      {
        return "CacheHits";
      }
    }

    public int CacheHits
    {
      get
      {
        return this._CacheHits;
      }
      set
      {
        this.SetProperty<int>(this.PropertyCacheHits, ref this._CacheHits, value);
      }
    }

    public string PropertyBingQueries
    {
      get
      {
        return "BingQueries";
      }
    }

    public int BingQueries
    {
      get
      {
        return this._BingQueries;
      }
      set
      {
        this.SetProperty<int>(this.PropertyBingQueries, ref this._BingQueries, value);
      }
    }

    public string PropertyBingRetries
    {
      get
      {
        return "BingRetries";
      }
    }

    public int BingRetries
    {
      get
      {
        return this._BingRetries;
      }
      set
      {
        this.SetProperty<int>(this.PropertyBingRetries, ref this._BingRetries, value);
      }
    }

    public string PropertyExceptionsQueryingBing
    {
      get
      {
        return "ExceptionsQueryingBing";
      }
    }

    public int ExceptionsQueryingBing
    {
      get
      {
        return this._ExceptionsQueryingBing;
      }
      set
      {
        this.SetProperty<int>(this.PropertyExceptionsQueryingBing, ref this._ExceptionsQueryingBing, value);
      }
    }

    internal void Shutdown()
    {
      this.RemoveSubscriptions();
    }
  }
}
