using System;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class ConcurrentValueBase<T>
  {
    private T mCurrent;
    private object mLocker;

    public ConcurrentValueBase(T _start)
    {
      this.mCurrent = _start;
      this.mLocker = new object();
    }

    protected T BeginUse()
    {
      Monitor.Enter(this.mLocker);
      return this.mCurrent;
    }

    protected void EndUse()
    {
      Monitor.Exit(this.mLocker);
    }

    protected void EndUseAndUpdate(T updatedValue)
    {
      this.mCurrent = updatedValue;
      this.EndUse();
    }

    protected bool TryBeginUse(out T curValue)
    {
      if (Monitor.TryEnter(this.mLocker))
      {
        curValue = this.mCurrent;
        return true;
      }
      else
      {
        curValue = default (T);
        return false;
      }
    }

    protected X SelectByReading<X>(Func<T, X> func)
    {
      X x = default (X);
      try
      {
        this.BeginUse();
        return func(this.mCurrent);
      }
      finally
      {
        this.EndUse();
      }
    }

    public override int GetHashCode()
    {
      return this.mLocker.GetHashCode();
    }
  }
}
